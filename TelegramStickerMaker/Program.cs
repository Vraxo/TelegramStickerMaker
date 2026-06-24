using System.Text;

namespace TelegramStickerMaker;

internal sealed class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Orchestrator.Run();
    }
}