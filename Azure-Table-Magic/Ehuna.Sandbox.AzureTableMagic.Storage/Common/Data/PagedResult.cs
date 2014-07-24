using System.Collections.Generic;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Common.Data
{
    public class PagedResult<TEntity>
    {
        public PagedResult()
        {
            Items = new List<TEntity>();
        }

        public IList<TEntity> Items { get; set; }
        public string NextPageUrl { get; set; }
    }
}
