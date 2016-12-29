using System;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;

namespace PocketBookSync.Data
{
    public static class DataPath
    {
        static DataPath()
        {
            var location = Environment.GetEnvironmentVariable("APPDATA");
            if (string.IsNullOrEmpty(location))
            {
                location = PlatformServices.Default.Application.ApplicationBasePath;
            }
            location = System.IO.Path.Combine(location, "pocketbooksync");
            Directory.CreateDirectory(location);
            Path = location;
        }

        public static string Path { get; private set; }
    }
}