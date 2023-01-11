using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Bocchi.Data;

/* This is used if database provider does't define
 * IBocchiDbSchemaMigrator implementation.
 */
public class NullBocchiDbSchemaMigrator : IBocchiDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
