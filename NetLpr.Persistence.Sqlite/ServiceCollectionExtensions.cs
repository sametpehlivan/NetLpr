using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Persistence.Sqlite.Repository;
using NetLpr.Core.Services;
using NetLpr.Core.Services.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NetLpr.Persistence.Sqlite
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqliteAdapter(this IServiceCollection service)
        {
            service.AddDbContext<SqliteDbContext>(options => options.UseSqlite("Data Source=net_lpr.db"));
            service.AddSingleton<IRtspSourcePersistenceService, RtspSourceRepository>();
            service.AddSingleton<ITrackedInfoRepository, TrackedInfoRepository>();

            return service;
        }
        public static IServiceProvider MigrateDatabase(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
            context.Database.Migrate();
            return serviceProvider;
        }
    }
}
