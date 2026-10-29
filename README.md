# 🧩 TelegramStickerMaker

Console app that turns a folder of images and videos into Telegram-ready stickers.

## 📦 What it does

| Input | Output | Processing |
| --- | --- | --- |
| `.png` `.jpg` `.jpeg` `.bmp` `.webp` | `.png` | Scaled to fit within 512×512, aspect ratio kept |
| `.mp4` `.gif` | `.webm` | VP9 with alpha, 512 px on the long side, 30 fps max, 3 s max, muted, 256 KB target |

Videos longer than 3 seconds are **sped up** to fit the limit, not trimmed. Anything else in the input folder is skipped and reported as skipped, not failed.

## ✅ Requirements

- [.NET 11 SDK](https://dotnet.microsoft.com/download) — the project targets `net11.0`
- `ffmpeg` and `ffprobe` on your `PATH`, with `libvpx-vp9` support (needed for video input only)

## 🚀 Usage

```sh
git clone https://github.com/Vraxo/TelegramStickerMaker.git
cd TelegramStickerMaker
dotnet run --project TelegramStickerMaker
```

If the input folder is missing, the app creates it, asks you to add files, and exits. Put your files in `Input/`, run again, and collect the results from `Output/`.

Progress, per-file results, and a summary are printed to the console. The app waits for a keypress before closing.

Folders default to `Input/` and `Output/`, so there is nothing to configure. To use other locations, edit `paths.txt`, created next to the executable on first run:

```text
Input
Output
```

- Line 1 is the input folder, line 2 is the output folder.
- Relative paths resolve against the executable's directory; absolute paths work too.
- With `dotnet run`, `paths.txt` and both folders live in `artifacts/bin/TelegramStickerMaker/`.

## 🎬 Video conversion

- Long videos are sped up by the factor needed to reach 3 s, and audio is dropped entirely.
- The first encode targets CRF 33. If the result exceeds 256 KB, the file is re-encoded at a 200 kbit/s target with CRF 38.
- That second pass costs quality. If the output looks soft, cut the source clip down first.

## 🔁 Re-running

Files whose output already exists are skipped. Delete the matching file in `Output/` to reprocess it.

## 🧱 Project layout

| File | Role |
| --- | --- |
| `Program.cs` | Entry point |
| `Orchestrator.cs` | Reads `paths.txt`, walks the input folder, dispatches work |
| `StaticStickerProcessor.cs` | Image resize and PNG export (ImageSharp) |
| `VideoStickerProcessor.cs` | FFmpeg encode to WebM (FFMpegCore) |
| `Log.cs` | Console output and summary (Spectre.Console) |

Built with [ImageSharp](https://github.com/SixLabors/ImageSharp), [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore), and [Spectre.Console](https://github.com/spectreconsole/spectre.console).

## 📄 License

[MIT](LICENSE) © Vraxo
