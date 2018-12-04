using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageSynchronizer.Models;

namespace ImageSynchronizer.Processor
{
    public class ImageProcessor
    {           
        public int Process(List<ResourceItem> resourceItems, List<ResourceItem> savedResourceItems, DirectoryInfo basePhotoDir, DirectoryInfo baseMovieDir, bool isMove)
        {
            var total = 0;
            foreach (var resourceItem in resourceItems)
            {                                
                if (resourceItem.FileType == FileType.Image || resourceItem.FileType == FileType.Movie)
                {
                    var baseOutputDir = resourceItem.FileType == FileType.Image ? basePhotoDir : baseMovieDir;
                    var fileInfo = new FileInfo(resourceItem.FileFullPath);
                    var year = fileInfo.LastWriteTime.Year.ToString();
                    var month = string.Format("{0}. {1}", fileInfo.LastWriteTime.Month, fileInfo.LastWriteTime.ToString("MMMM"));
                    var day = fileInfo.LastWriteTime.Day.ToString();

                    var outputDir = Directory.CreateDirectory(Path.Combine(baseOutputDir.FullName, year, month));
                    if (outputDir.Exists)
                    {
                        var oldFiles = outputDir.GetFiles();
                        var id = 0;
                        var newFileName = string.Format("{0}-{1}-{2}({3}){4}", year, fileInfo.LastWriteTime.Month, day, id, fileInfo.Extension); 
                        while (oldFiles.Any(o => string.Equals(o.Name.ToLower(), newFileName.ToLower())))
                        {
                            id++;
                            newFileName = string.Format("{0}-{1}-{2}({3}){4}", year, fileInfo.LastWriteTime.Month, day, id, fileInfo.Extension);
                        }
                        try
                        {
                            var destFileName = Path.Combine(outputDir.FullName, newFileName);
                            resourceItem.FileFullPath = destFileName;
                            if (isMove)
                            {
                                File.Move(fileInfo.FullName, destFileName);
                            }
                            else
                            {
                                File.Copy(fileInfo.FullName, destFileName);                                
                            }
                            total++;
                            Console.WriteLine(@"Count= {0}. File Moved from {1} to {2}", total, fileInfo.FullName, destFileName);
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException != null) throw new Exception(e.InnerException.Message);
                        }
                    }
                }
            }
            return total;
        }
        
    }
}
