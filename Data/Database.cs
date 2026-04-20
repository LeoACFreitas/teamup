namespace Teamup.Data;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Teamup.Helpers;

public class MyDbContext(DbContextOptions<MyDbContext> c) : DbContext(c)
{
    public DbSet<User> User { get; set; }
    public DbSet<Request> Request { get; set; }
    public DbSet<Game> Game { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(u => u.User_id);
            // Read via field to bypass Country setter validation during materialization
            entity.Property(u => u.Country)
                  .HasField("_country")
                  .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.ToTable("Game");
            entity.HasKey(g => g.Game_id);
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("Request");
            entity.HasKey(r => r.Request_id);
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.User_id)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(r => r.Game)
                  .WithMany()
                  .HasForeignKey(r => r.Game_id)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

public class User
{
    [Key]
    public int User_id { get; set; }
    public string Nickname { get; set; }
    public decimal? Sub { get; set; }
    private string _country;
    public string Country
    {
        get => _country;
        set
        {
            if (!HttpHelper.Countries.Any(c => c[1].Equals(value)))
                throw new InvalidOperationException();
            _country = value;
        }
    }
}

public class Request
{
    [Key]
    public int Request_id { get; set; }
    public int? User_id { get; set; }
    public User? User { get; set; }
    public int? Game_id { get; set; }
    public Game? Game { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
}

public class Game
{
    [Key]
    public int Game_id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }
}
