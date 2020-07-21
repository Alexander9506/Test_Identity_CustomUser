﻿using Microsoft.AspNetCore.Http;
using System.IO;

namespace Test_CustomUserManagement.Models
{
    public static class FileContainerFactory
    {
        public static FileContainer CreateFileContainer(IFormFile formFile, string path, string fileName)
        {
            FileContainer container = new FileContainer
            {
                FileDisplayName = formFile.FileName,
                FilePathFull = Path.Combine(path, fileName),
                FileType = formFile.ContentType
            };

            return container;
        }
    }
}
