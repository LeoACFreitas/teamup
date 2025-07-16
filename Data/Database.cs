namespace Teamup.Data;

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Teamup.Helpers;

public class MyDbContext(DbContextOptions<MyDbContext> c) : DbContext(c)
{
    public DbSet<User> User { get; set; }
    public DbSet<Request> Request { get; set; }
    public DbSet<Game> Game { get; set; }
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
        get
        {
            return _country;
        }
        set
        {
            if (!HttpHelper.Countries.Any(c => c[1].Equals(value)))
            {
                throw new InvalidOperationException();
            }
            _country = value;
        }
    }
}

public class Request
{
    [Key]
    public int Request_id { get; set; }
    public User? User { get; set; }
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
