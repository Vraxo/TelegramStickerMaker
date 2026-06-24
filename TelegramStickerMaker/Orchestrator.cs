using Spectre.Console;

namespace TelegramStickerMaker;

internal static class Orchestrator
{
    public static void Run()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string inputFolder = Path.Combine(baseDirectory, "Input");
        string outputFolder = Path.Combine(baseDirectory, "Output");

        if (!Directory.Exists(inputFolder))
        {
            Directory.CreateDirectory(inputFolder);
            Log.Info($"Created input folder: '{inputFolder}'");
            Log.Info("Add your images/videos and run again.");
            Log.PromptExit();
            return;
        }

        Directory.CreateDirectory(outputFolder);

        string[] files = Directory.GetFiles(inputFolder);
        if (files.Length == 0)
        {
            Log.Warning($"No files found in the Input folder: {inputFolder}");
            Log.PromptExit();
            return;
        }

        Log.Info($"Found {files.Length} file(s) in the input folder.\n");

        (int processed, int skipped, int failed) = ProcessFiles(files, outputFolder);

        Log.Summary(files.Length, processed, skipped, failed);
        Log.PromptExit();
    }

    private static (int Processed, int Skipped, int Failed) ProcessFiles(string[] files, string outputFolder)
    {
        return AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Preparing...", ctx => RunProcessingLoop(ctx, files, outputFolder));
    }

    private static (int Processed, int Skipped, int Failed) RunProcessingLoop(StatusContext ctx, string[] files, string outputFolder)
    {
        int processedCount = 0;
        int skippedCount = 0;
        int failedCount = 0;

        for (int i = 0; i < files.Length; i++)
        {
            string filePath = files[i];
            string fileName = Path.GetFileName(filePath);
            ctx.Status($"[purple][[{i + 1}/{files.Length}]][/] Processing {Markup.Escape(fileName)}...");

            ProcessResult result = ProcessFile(filePath, outputFolder);
            switch (result)
            {
                case ProcessResult.Processed:
                    processedCount++;
                    break;
                case ProcessResult.Skipped:
                    skippedCount++;
                    break;
                case ProcessResult.Failed:
                    failedCount++;
                    break;
            }
        }

        return (processedCount, skippedCount, failedCount);
    }

    private static ProcessResult ProcessFile(string filePath, string outputFolder)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        string fileName = Path.GetFileName(filePath);

        string? targetExtension = extension switch
        {
            ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp" => ".png",
            ".mp4" or ".gif" => ".webm",
            _ => null
        };

        if (targetExtension is null)
        {
            Log.Muted($"Skipped (Unsupported extension): {fileName}");
            return ProcessResult.Skipped;
        }

        string outputPath = Path.Combine(
            outputFolder,
            Path.GetFileNameWithoutExtension(filePath) + targetExtension);

        if (File.Exists(outputPath))
        {
            Log.Muted($"Skipped:   {fileName}");
            return ProcessResult.Skipped;
        }

        try
        {
            if (targetExtension == ".png")
            {
                StaticStickerProcessor.Process(filePath, outputPath);
                Log.Success("Processed:", fileName);
            }
            else
            {
                VideoStickerProcessor.Process(filePath, outputPath);
                long sizeBytes = new FileInfo(outputPath).Length;
                Log.Success("Processed:", fileName, $"({sizeBytes / 1024} KB)");
            }
            return ProcessResult.Processed;
        }
        catch (Exception ex)
        {
            Log.Error("Failed:   ", fileName, $"— {ex.Message}");
            return ProcessResult.Failed;
        }
    }
}