using BlazorWasm.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BlazorWasm.Server.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use SQL Server for design-time migrations
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TasksDb;Trusted_Connection=true;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
