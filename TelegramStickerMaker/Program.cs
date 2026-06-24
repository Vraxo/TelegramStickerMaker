using Spectre.Console;
using System.Text;
using TelegramStickerMaker;

Console.OutputEncoding = Encoding.UTF8;

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

int processedCount = 0;
int skippedCount = 0;
int failedCount = 0;

AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .Start("Preparing...", ctx =>
    {
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
    });

Log.Summary(files.Length, processedCount, skippedCount, failedCount);
Log.PromptExit();

static ProcessResult ProcessFile(string filePath, string outputFolder)
{
    string extension = Path.GetExtension(filePath).ToLowerInvariant();
    string fileName = Path.GetFileName(filePath);

    bool isStatic = extension is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp";
    bool isVideo = extension is ".mp4" or ".gif";

    if (!isStatic && !isVideo)
    {
        Log.Muted($"Skipped (Unsupported extension): {fileName}");
        return ProcessResult.Skipped;
    }

    string targetExt = isStatic ? ".png" : ".webm";
    string outputPath = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(filePath) + targetExt);

    if (File.Exists(outputPath))
    {
        Log.Muted($"Skipped:   {fileName}");
        return ProcessResult.Skipped;
    }

    try
    {
        if (isStatic)
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