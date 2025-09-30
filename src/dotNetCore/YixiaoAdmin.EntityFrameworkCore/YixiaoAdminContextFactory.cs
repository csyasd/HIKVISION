using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace YixiaoAdmin.EntityFrameworkCore
{
    public class YixiaoAdminContextFactory : IDesignTimeDbContextFactory<YixiaoAdminContext>
    {
        YixiaoAdminContext IDesignTimeDbContextFactory<YixiaoAdminContext>.CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<YixiaoAdminContext>();
            optionsBuilder.UseSqlServer(DbConfig.ConnectString);
            return new YixiaoAdminContext(optionsBuilder.Options);
        }
    }

    public class DbConfig
    {
       public  static readonly string ConnectString = "Server=DESKTOP-37R19QA;Database=HIKVISION;User=sa;Password=123456;TrustServerCertificate=true;";
    }
}