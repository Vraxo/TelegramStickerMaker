using FFMpegCore;
using FFMpegCore.Arguments;

namespace TelegramStickerMaker;

internal static class VideoStickerProcessor
{
    private const long TelegramMaxSizeBytes = 256 * 1024;
    private const int TargetSize = 512;
    private const int MaxFramerate = 30;
    private const double MaxDurationSeconds = 3.0;

    public static void Process(string inputPath, string outputPath)
    {
        double duration = FFProbe.Analyse(inputPath).Duration.TotalSeconds;
        double speedFactor = duration > MaxDurationSeconds ? duration / MaxDurationSeconds : 1.0;
        string videoFilter = GetVideoFilter(speedFactor);

        RunFFmpeg(inputPath, outputPath, videoFilter, compress: false);

        if (new FileInfo(outputPath).Length <= TelegramMaxSizeBytes)
        {
            return;
        }

        File.Delete(outputPath);
        RunFFmpeg(inputPath, outputPath, videoFilter, compress: true);
    }

    private static string GetVideoFilter(double speedFactor)
    {
        string scale = $"scale='if(gt(iw,ih),{TargetSize},-2)':'if(gt(iw,ih),-2,{TargetSize})'";
        string speed = speedFactor > 1.0 ? $",setpts=PTS/{speedFactor:0.0000}" : "";
        return $"{scale}{speed},fps={MaxFramerate}";
    }

    private static void RunFFmpeg(string inputPath, string outputFile, string videoFilter, bool compress)
    {
        bool isGif = inputPath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase);

        FFMpegArguments
            .FromFileInput(inputPath, true, options =>
            {
                if (isGif)
                {
                    options.WithArgument(new CustomArgument("-ignore_loop 0"));
                }
            })
            .OutputToFile(outputFile, true, options =>
            {
                options.WithCustomArgument("-c:v libvpx-vp9")
                       .ForcePixelFormat("yuva420p")
                       .WithCustomArgument("-an")
                       .WithCustomArgument($"-t {MaxDurationSeconds}")
                       .WithCustomArgument($"-vf {videoFilter}")
                       .WithCustomArgument("-loglevel error")
                       .WithCustomArgument("-deadline realtime");

                if (compress)
                {
                    options.WithCustomArgument("-b:v 200k")
                           .WithCustomArgument("-maxrate 200k")
                           .WithCustomArgument("-bufsize 400k")
                           .WithCustomArgument("-crf 38")
                           .WithCustomArgument("-cpu-used 8")
                           .WithCustomArgument("-row-mt 1");
                }
                else
                {
                    options.WithCustomArgument("-b:v 0")
                           .WithCustomArgument("-crf 33");
                }
            })
            .ProcessSynchronously();
    }
}