using Spectre.Console;

namespace TelegramStickerMaker;

internal static class Log
{
    public static void Info(string message)
    {
        AnsiConsole.WriteLine(message);
    }

    public static void Muted(string message)
    {
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(message)}[/]");
    }

    public static void Success(string status, string detail, string? suffix = null)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(status)}[/] {Markup.Escape(detail)}{FormatSuffix(suffix)}");
    }

    public static void Warning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public static void Error(string status, string detail, string? suffix = null)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(status)}[/] {Markup.Escape(detail)}{FormatSuffix(suffix)}");
    }

    public static void Summary(int total, int processed, int skipped, int failed)
    {
        AnsiConsole.MarkupLine($"""

            Total:      [cyan]{total}[/]
            Processed:  [green]{processed}[/]
            Skipped:    [grey]{skipped}[/]
            Failed:     [red]{failed}[/]
            """);
    }

    public static void PromptExit()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to exit...[/]");
        Console.ReadKey(intercept: true);
    }

    private static string FormatSuffix(string? suffix)
    {
        return suffix != null ? $" [grey]{Markup.Escape(suffix)}[/]" : string.Empty;
    }
}