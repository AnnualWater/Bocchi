using System.Threading.Tasks;

namespace Bocchi.Data;

public interface IBocchiDbSchemaMigrator
{
    Task MigrateAsync();
}
