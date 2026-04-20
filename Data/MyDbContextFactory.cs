namespace Teamup.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("csteamup")
            ?? "Server=localhost;Database=teamup;Uid=teamupuser;Pwd=teamuppassword;";

        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseMySQL(cs)
            .Options;

        return new MyDbContext(options);
    }
}
