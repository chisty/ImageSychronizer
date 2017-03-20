using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSynchronizer.Models
{
    [Serializable]
    public class ResourceItem
    {
        public string FileFullPath { get; set; }
        public string FileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileFullPath) || File.Exists(FileFullPath) == false) return string.Empty;
                return Path.GetFileName(FileFullPath);
            }
        }
        public string Checksum { get; set; }
        public FileType FileType { get; set; } 
    }

    [Serializable]
    public enum FileType
    {
        Image,
        Movie,
        Zip,
        Other
    }
}
