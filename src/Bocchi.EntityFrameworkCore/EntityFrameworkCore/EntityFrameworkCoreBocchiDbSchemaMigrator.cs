using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Bocchi.Data;
using Volo.Abp.DependencyInjection;

namespace Bocchi.EntityFrameworkCore;

public class EntityFrameworkCoreBocchiDbSchemaMigrator
    : IBocchiDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreBocchiDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the BocchiDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<BocchiDbContext>()
            .Database
            .MigrateAsync();
    }
}
