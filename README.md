# YoutubeDownloader

A Windows desktop application for downloading YouTube videos and audio using [yt-dlp](https://github.com/yt-dlp/yt-dlp).

## Features

- Download YouTube videos as **MP4** (best quality) or extract audio as **MP3**
- Automatic clipboard detection — pastes YouTube URLs on launch or focus
- Real-time download progress bar and live log output
- Cancel downloads in progress
- Auto-downloads `yt-dlp.exe` on first run — no manual setup needed
- Choose your own output folder (defaults to My Videos)

## Requirements

- Windows
- .NET Framework 4.5.2 or later

> `yt-dlp.exe` is downloaded automatically from the official GitHub releases on first use.

## Getting Started

1. Clone or download this repository.
2. Open `YoutubeDownloader.sln` in Visual Studio.
3. Build and run the project.
4. Paste a YouTube URL (or copy one to your clipboard before launching) and click **Download**.

## Usage

1. Enter or paste a YouTube URL into the URL field.
2. Select output format: **MP4** (video) or **MP3** (audio).
3. Choose a destination folder.
4. Click **Download** and monitor progress in the log window.
5. Click **Cancel** to abort an in-progress download.

## Project Structure

```
YoutubeDownloader/
├── frmDownload.cs        # Main form logic
├── frmDownload.Designer.cs
├── AboutBox.cs           # About dialog
├── Program.cs            # Entry point
└── YoutubeDownloader.csproj
```

## License

This project is for personal use. [yt-dlp](https://github.com/yt-dlp/yt-dlp) is a separate open-source tool governed by its own license.
