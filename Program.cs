using Teamup;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Diagnostics;

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
        builder.Services.AddControllers();
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
        app.MapControllers();

        app.Run();
    }
}