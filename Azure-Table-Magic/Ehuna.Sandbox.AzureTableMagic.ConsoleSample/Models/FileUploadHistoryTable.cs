using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ehuna.Sandbox.AzureTableMagic.ConsoleSample.Models
{
    public class FileUploadHistoryTable
    {
        public DateTime LastTimeUpdated { get; set; }

        public Guid MerchantId { get; set; }
        public string FileId { get; set; }
    }
}
