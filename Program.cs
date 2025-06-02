using Teamup;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Diagnostics.Metrics;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

internal class Program
{
    const string CORS_NAME = "frontend";

    private static void Main(string[] args)
    {
        var k = Util.HttpRequest("https://www.googleapis.com/oauth2/v3/certs");
        var ks = JsonWebKeySet.Create(k).GetSigningKeys();
        var connectionString = Environment.GetEnvironmentVariable("csteamup");

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("/var/www/teamup/logs/log.txt")
            .CreateLogger();

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddCors(options =>
            {
                options.AddPolicy(CORS_NAME, builder => builder.AllowAnyMethod().AllowAnyHeader()
                    .WithOrigins("https://teamupgaming.net", "http://localhost:3000"));
            })
            .AddDbContext<MyDbContext>(o =>
                o.UseMySQL(connectionString)
            );

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "https://accounts.google.com",
                    ValidAudience = "274132790884-ct8a1a5ip9a9b0krvm9rajuefg1e7bho.apps.googleusercontent.com",
                    IssuerSigningKeys = ks
                };
            });
        builder.Services.AddAuthorization();
        builder.Host.UseSerilog();

        Log.Logger.Information("It is running...");

        var app = builder.Build();
        app.UseCors(CORS_NAME);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.UseSerilogRequestLogging();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";

                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (contextFeature != null)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError($"Something went wrong: {contextFeature.Error}");
                }
            });
        });

        app.MapGet("/request", async (HttpContext h, MyDbContext db, string? gameName, bool? showMyRequests, string? country, int page = 1) =>
        {
            var userId = h.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var query = db.Request.AsQueryable();
            var pageSize = 20;

            if (!string.IsNullOrEmpty(gameName))
                query = query.Where(r => r.Game != null && r.Game.Name.Equals(gameName));

            if (showMyRequests.HasValue && showMyRequests.Value && userId != null)
                query = query.Where(r => r.User != null && r.User.Sub == Convert.ToDecimal(userId));

            if (!string.IsNullOrEmpty(country))
                query = query.Where(r => r.User != null && r.User.Country.Equals(country, StringComparison.OrdinalIgnoreCase));


            var requests = await query.OrderByDescending(r => r.Date)
                                      .Include(r => r.User)
                                      .Include(r => r.Game)
                                      .Skip((page - 1) * pageSize)
                                      .Take(pageSize)
                                      .ToListAsync();

            return Results.Ok(requests);
        });
        app.MapPost("/request", [Authorize] (HttpContext h, MyDbContext db, [FromBody] Request request) =>
        {
            Game? game = null;
            if (request.Game != null)
                game = db.Game.Where(g => g.Name.Equals(request.Game.Name)).FirstOrDefault();
            if (game == null)
            {
                h.Response.StatusCode = 400;
                return;
            }

            //Validate number of current user requests
            if (db.Request.Where(r => r.User.Sub == Convert.ToDecimal(h.GetSubFromJwt())).Count() >= 2)
            {
                h.Response.StatusCode = 400;
                h.Response.WriteAsync("You have reached the maximum number of requests, delete any to continue");
                return;
            }

            request.Date = DateTime.Now;
            request.Game = game;
            request.User = GetUser(h, db);
            db.Request.Add(request);

            h.SendEmail("New request added: " + request.User.Nickname);

            db.SaveChanges();
        });
        app.MapDelete("/request/{id}", [Authorize] (HttpContext h, MyDbContext db, int id) =>
        {
            var r = db.Request.Include(r => r.User).FirstOrDefault(r => r.Request_id == id);
            if (r == null || r.User.Sub != Convert.ToDecimal(h.GetSubFromJwt()))
            {
                h.Response.StatusCode = 400;
                return;
            }

            db.Request.Remove(r);
            db.SaveChanges();
        }); 
        app.MapGet("/user", GetUser);
        app.MapPost("/user", [Authorize] (HttpContext h, MyDbContext db, [FromBody] User u) =>
        {
            u.Sub = Convert.ToDecimal(h.GetSubFromJwt());

            if (GetUser(h, db) != null)
            {
                h.Response.StatusCode = 400;
                return false;
            }

            db.User.Add(u);
            db.SaveChanges();

            h.SendEmail("New user: " + JsonSerializer.Serialize(u));

            return true;
        });
        app.MapGet("/game", (MyDbContext db, string name) =>
        {
            name = name.ToUpper();

            if (string.IsNullOrEmpty(name))
            {
                return db.Game.OrderByDescending(g => g.Value).Take(10).ToList();
            }

            var r = db.Game.OrderByDescending(g => g.Value).Where(g => g.Name.ToUpper().StartsWith(name)).Take(5).Union(
                db.Game.OrderByDescending(g => g.Value).Where(g => g.Name.ToUpper().Contains(name)).Take(5)).ToList();
            return r;
        });
        app.Run();
    }

    [Authorize]
    private static User GetUser(HttpContext h, MyDbContext c)
    {
        var sub = Convert.ToDecimal(h.GetSubFromJwt());
        return c.User.FirstOrDefault(u => u.Sub.Equals(sub));
    }
}