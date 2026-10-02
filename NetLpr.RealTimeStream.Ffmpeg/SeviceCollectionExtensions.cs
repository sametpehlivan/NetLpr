using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using Microsoft.Extensions.DependencyInjection;

namespace NetLpr.RealTimeStream.Ffmpeg
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFfmpegAdapter(this IServiceCollection service, FfmpegConfig configs)
        {
            Configure(configs);
            return service;
        }
        private static void Configure(FfmpegConfig configs)
        {
            InitializePath(configs.BasePath);

            InitializeNetwork();
        }
        private static void InitializeNetwork()
        {
            ffmpeg.avformat_network_init();
        }
        private static void InitializePath(string baseDirectory)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetFolder = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    targetFolder = "x64";
                else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    targetFolder = "x64_arm";
                else
                    throw new Exception("Unsupported Windows architecture!");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    targetFolder = "linux64";
                else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                    targetFolder = "linux64_arm";
                else
                    throw new Exception("Unsupported Linux architecture!");
            }
            else
            {
                throw new Exception("This operating system is not supported!");
            }
            string ffmpegLibsPath = Path.Combine(baseDir, baseDirectory, targetFolder);

            if (!Directory.Exists(ffmpegLibsPath))
            {
                throw new Exception($"FFmpeg library folder not found: {ffmpegLibsPath}");
            }
            ffmpeg.RootPath = ffmpegLibsPath;

            Console.WriteLine($"FFmpeg library path has been set: {ffmpeg.RootPath}");
        }
    }

}
