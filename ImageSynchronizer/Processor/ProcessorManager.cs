using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using ImageSynchronizer.Extension;
using ImageSynchronizer.Models;

namespace ImageSynchronizer.Processor
{
    public class ProcessorManager
    {
        public List<ResourceItem> SavedResourceItems { get; set; }
        public List<ResourceItem> ResourceItems { get; set; }

        public ProcessorManager()
        {            
            ResourceItems = new List<ResourceItem>();
            SavedResourceItems = FileHelper.ReadJSON();
        }


        public void ProcessResources()
        {
            var basePhotoDir = ConfigurationManager.AppSettings["basePhotoDir"];
            var baseMovieDir = ConfigurationManager.AppSettings["baseMovieDir"];
            var inputDir = ConfigurationManager.AppSettings["inputdir"];
            var shouldProcess = ConfigurationManager.AppSettings["shouldprocess"];
            var updateMetaDataOnly = ConfigurationManager.AppSettings["updateMetaDataOnly"];
            var operationType = ConfigurationManager.AppSettings["operationType"];
                        
            if (updateMetaDataOnly.ToLower() == "true")
            {
                UpdateResourceMetaDataOnly(basePhotoDir, baseMovieDir);
                return;
            }

            if (string.IsNullOrWhiteSpace(inputDir) || !Directory.Exists(inputDir))
            {
                MessageBox.Show("Input directory not found", "Error", MessageBoxButton.OK);
                return;
            }

            ReadAllResources(Directory.CreateDirectory(inputDir));
            var isMove = operationType.ToLower().Trim() == "move";

            if (shouldProcess.ToLower() == "true")
            {
                if (string.IsNullOrWhiteSpace(basePhotoDir) || !Directory.Exists(basePhotoDir) || string.IsNullOrWhiteSpace(baseMovieDir) || !Directory.Exists(baseMovieDir))
                {
                    MessageBox.Show("Base directory not found", "Error", MessageBoxButton.OK);
                    return;
                }
                var imageProcessor = new ImageProcessor();
                imageProcessor.Process(ResourceItems, SavedResourceItems, Directory.CreateDirectory(basePhotoDir), Directory.CreateDirectory(baseMovieDir), isMove);
                
                SavedResourceItems.AddRange(ResourceItems);
                FileHelper.WriteJSON(SavedResourceItems.OrderBy(o => o.FileName).ToList());
            }            
        }

        private void UpdateResourceMetaDataOnly(string basePhotoDir, string baseMovieDir)
        {
            if (!string.IsNullOrWhiteSpace(basePhotoDir) && Directory.Exists(basePhotoDir))
            {
                ReadAllResources(Directory.CreateDirectory(basePhotoDir));
            }

            if (!string.IsNullOrWhiteSpace(baseMovieDir) && Directory.Exists(baseMovieDir))
            {
                ReadAllResources(Directory.CreateDirectory(baseMovieDir));
            }
            FileHelper.WriteJSON(ResourceItems.OrderBy(o => o.FileName).ToList());
        }

        private void ReadAllResources(DirectoryInfo directory)
        {
            Console.WriteLine();
            Console.WriteLine(@"Reading resources for directory: "+directory.FullName);
            var resources = directory.GetFiles();

            foreach (var resource in resources)
            {
                if (FileHelper.IsImageFile(resource))
                {
                    AddFileIfNewResource(resource, FileType.Image);
                }
                else if (FileHelper.IsMovieFile(resource))
                {
                    AddFileIfNewResource(resource, FileType.Movie);
                }
            }

            foreach (var directoryInfo in directory.GetDirectories())
            {
                ReadAllResources(directoryInfo);
            }
        }

        private void AddFileIfNewResource(FileInfo file, FileType fileType)
        {
            try
            {
                var checksum = file.Checksum();
                Console.Write(@"#");
                if (SavedResourceItems.Any(r => r.Checksum == checksum) || ResourceItems.Any(r => r.Checksum == checksum))
                {
                    //checksum = Guid.NewGuid().ToString("N");
                    return;
                }

                var resourceItem = new ResourceItem
                {
                    Checksum = checksum,
                    FileFullPath = file.FullName,
                    FileType = fileType
                };

                ResourceItems.Add(resourceItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Exception @ Checksum generation: "+ex.StackTrace);
            }
        }

        public void RenameToUniqueNames()
        {
            var inputDirPath = ConfigurationManager.AppSettings["inputdir"];
            if (Directory.Exists(inputDirPath))
            {
                var inputDir = Directory.CreateDirectory(inputDirPath);
                foreach (var yearDir in inputDir.GetDirectories())
                {
                    if (yearDir.Name.Equals("Womb")) continue;

                    foreach (var monthDir in yearDir.GetDirectories())
                    {
                        //if(monthDir.Name.Equals("Birthday Party"))  continue;
                        
                        foreach (var file in monthDir.GetFiles())
                        {
                            var oldName = file.Name;
                            if (oldName.Contains("-"))
                            {
                                var splits = oldName.Split('-');
                                if (splits.Length == 2)
                                {
                                    var newName = string.Format("{0}-{1}-{2}({3}){4}", yearDir.Name, GetTextBeforeDot(monthDir.Name), splits[0], GetTextBeforeDot(splits[1]), file.Extension);
                                    var newFullName = Path.Combine(monthDir.FullName, newName);
                                    File.Move(file.FullName, newFullName);
                                }
                            }
                            else
                            {
                                var newName = string.Format("{0}-{1}-{2}", yearDir.Name, GetTextBeforeDot(monthDir.Name), oldName);
                                var newFullName = Path.Combine(monthDir.FullName, newName);
                                File.Move(file.FullName, newFullName);
                            }

                        }
                    }
                }
            }
        }

        private string GetTextBeforeDot(string textWithDot)
        {
            var splits = textWithDot.Split('.');
            int digit;
            if(int.TryParse(splits[0], out digit) && digit > -1)
            {
                return digit.ToString();
            }

            return textWithDot;
        }
    }
}
