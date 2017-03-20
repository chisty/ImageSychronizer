using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSynchronizer.Models;
using Newtonsoft.Json;

namespace ImageSynchronizer.Extension
{
    public class FileHelper
    {
        public static List<ResourceItem> ReadJSON()
        {
            var resourceItems = new List<ResourceItem>();
            var blobFilePath = ConfigurationManager.AppSettings["resourceMeta"];
            if (string.IsNullOrWhiteSpace(blobFilePath) || File.Exists(blobFilePath) == false) return resourceItems;
            try
            {
                var json = File.ReadAllText(blobFilePath);
                resourceItems = JsonConvert.DeserializeObject<List<ResourceItem>>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error occurred @ ReadJSON: " + ex.StackTrace);
            }
            return resourceItems;
        }

        public static bool WriteJSON(List<ResourceItem> resourceItems)
        {
            var blobFilePath = ConfigurationManager.AppSettings["resourceMeta"];
            if (string.IsNullOrWhiteSpace(blobFilePath)) return false;
            if (resourceItems == null || resourceItems.Any() == false) return false;

            try
            {
                var json = JsonConvert.SerializeObject(resourceItems);
                if (File.Exists(blobFilePath)) File.Delete(blobFilePath);

                File.WriteAllText(blobFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error occurred @ WriteJSON: " + ex.StackTrace);
            }            

            return true;
        }

        public static bool IsImageFile(FileInfo fileInfo)
        {
            var extension = fileInfo.Extension;
            if (string.IsNullOrWhiteSpace(extension)) return false;

            switch (extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return true;
            }
            return false;
        }

        public static bool IsMovieFile(FileInfo fileInfo)
        {
            var extension = fileInfo.Extension;
            if (string.IsNullOrWhiteSpace(extension)) return false;

            switch (extension.ToLower())
            {
                case ".mp4":
                case ".avi":
                case ".mov":
                case ".mts":
                    return true;
            }
            return false;
        }
    }
}
