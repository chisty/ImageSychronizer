﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
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
            var updateMetaData = ConfigurationManager.AppSettings["updateMetaData"];
                        
            if (updateMetaData.ToLower() == "true")
            {
                UpdateResourceMetaDataOnly(basePhotoDir, baseMovieDir);
                return;
            }

            if(string.IsNullOrWhiteSpace(inputDir) || !Directory.Exists(inputDir)) return;

            ReadAllResources(Directory.CreateDirectory(inputDir));

            if (shouldProcess.ToLower() == "true")
            {
                if (string.IsNullOrWhiteSpace(basePhotoDir) || !Directory.Exists(basePhotoDir) || string.IsNullOrWhiteSpace(baseMovieDir) || !Directory.Exists(baseMovieDir))
                {
                    return;
                }
                var imageProcessor = new ImageProcessor();
                imageProcessor.Process(ResourceItems, SavedResourceItems, Directory.CreateDirectory(basePhotoDir), Directory.CreateDirectory(baseMovieDir));
                
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
                if (SavedResourceItems.Any(r => r.Checksum == checksum) || ResourceItems.Any(r => r.Checksum == checksum)) return;

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
    }
}