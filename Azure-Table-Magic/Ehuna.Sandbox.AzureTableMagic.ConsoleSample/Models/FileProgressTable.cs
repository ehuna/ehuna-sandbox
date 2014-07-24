using System;

namespace Ehuna.Sandbox.AzureTableMagic.ConsoleSample.Models
{
    public class FileProgressTable
    {
        public Guid MerchantId          { get; set; }
        public string FileId            { get; set; }
        public int FileSizeInBytes      { get; set; }
        public int FileBytesProcessed   { get; set; }
    }
}
