using System.Threading.Tasks;
using Ehuna.Sandbox.AzureTableMagic.Storage.Common.Data;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Interfaces
{
    public interface IPartitionedRepository<TEntity>
    {
        void Insert(TEntity entity); // does not throw an exception if partitionKey + rowKey already exist; but also does not overwrite entity values
        void InsertOrReplace(TEntity entity);
        void Delete(TEntity entity);
        Task<PagedResult<TEntity>> GetPageAsync(object partitionKeyFrom, object partitionKeyTo, int maxPerPage, string nextPageToken = null);
        Task<PagedResult<TEntity>> GetPageAsync(string partitionKey, string rowGreaterThanOrEqual, string rowLessThan, int maxPerPage, string nextPageToken = null);
        TEntity Get(string partitionKey, string rowKey);
        TEntity Get(TEntity entity);
        Task<TEntity> GetAsync(string partitionKey, string rowKey);
        Task<TEntity> GetAsync(TEntity entity);
    }
}
