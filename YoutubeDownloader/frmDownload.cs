using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeDownloader
{
    public partial class frmDownload : Form
    {
        private Process _downloadProcess;
        private bool _isDownloading = false;

        public frmDownload()
        {
            InitializeComponent();
            txtFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        }

        private void frmDownload_Load(object sender, EventArgs e)
        {
            PasteClipboardIfUrl();
            CheckYtDlp();
        }

        private void frmDownload_Activated(object sender, EventArgs e)
        {
            if (txtURL.Text.Length == 0)
                PasteClipboardIfUrl();
        }

        private void PasteClipboardIfUrl()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText().Trim();
                if (text.StartsWith("http://") || text.StartsWith("https://"))
                    txtURL.Text = text;
            }
        }

        private void CheckYtDlp()
        {
            if (!File.Exists(YtDlpPath()))
            {
                AppendLog("yt-dlp not found. Attempting to download...");
                DownloadYtDlp();
            }
            else
            {
                AppendLog("yt-dlp found: " + YtDlpPath());
            }
        }

        private string YtDlpPath()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDir, "yt-dlp.exe");
        }

        private async void DownloadYtDlp()
        {
            string url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe";
            string dest = YtDlpPath();
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.DownloadProgressChanged += (s, e) =>
                        SetStatus(string.Format("Downloading yt-dlp... {0}%", e.ProgressPercentage));
                    await client.DownloadFileTaskAsync(new Uri(url), dest);
                }
                AppendLog("yt-dlp downloaded successfully.");
                SetStatus("Ready");
            }
            catch (Exception ex)
            {
                AppendLog("Failed to download yt-dlp: " + ex.Message);
                AppendLog("Please download yt-dlp.exe manually from https://github.com/yt-dlp/yt-dlp/releases and place it next to this application.");
                SetStatus("yt-dlp missing");
            }
        }

        private async void btnDownload_Click(object sender, EventArgs e)
        {
            if (_isDownloading)
            {
                CancelDownload();
                return;
            }

            string url = txtURL.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a YouTube URL.", "Missing URL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(YtDlpPath()))
            {
                MessageBox.Show("yt-dlp.exe not found. Please wait for it to download or place it manually.", "Missing yt-dlp", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string folder = txtFolder.Text.Trim();
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                MessageBox.Show("Please choose a valid save folder.", "Invalid Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetDownloading(true);
            txtLog.Clear();
            progressBar.Value = 0;
            SetStatus("Starting download...");

            bool mp3 = rdoMP3.Checked;
            await Task.Run(() => RunDownload(url, folder, mp3));

            SetDownloading(false);
        }

        private void RunDownload(string url, string folder, bool mp3)
        {
            string outputTemplate = Path.Combine(folder, "%(title)s.%(ext)s");
            string args;

            if (mp3)
            {
                args = string.Format(
                    "--no-playlist -x --audio-format mp3 --audio-quality 0 -o \"{0}\" -- \"{1}\"",
                    outputTemplate, url);
            }
            else
            {
                args = string.Format(
                    "--no-playlist -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\" --merge-output-format mp4 -o \"{0}\" -- \"{1}\"",
                    outputTemplate, url);
            }

            var psi = new ProcessStartInfo
            {
                FileName = YtDlpPath(),
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _downloadProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };

            _downloadProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null) HandleOutput(e.Data);
            };
            _downloadProcess.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null) HandleOutput(e.Data);
            };

            _downloadProcess.Start();
            _downloadProcess.BeginOutputReadLine();
            _downloadProcess.BeginErrorReadLine();
            _downloadProcess.WaitForExit();

            int code = _downloadProcess.ExitCode;
            _downloadProcess.Dispose();
            _downloadProcess = null;

            if (code == 0)
            {
                SetStatus("Download complete!");
                AppendLog("Done.");
                this.BeginInvoke((Action)(() => progressBar.Value = 100));
            }
            else
            {
                SetStatus("Download failed (exit code " + code + ").");
            }
        }

        private static readonly Regex _progressRe =
            new Regex(@"\[download\]\s+([\d\.]+)%", RegexOptions.Compiled);

        private void HandleOutput(string line)
        {
            AppendLog(line);
            var m = _progressRe.Match(line);
            if (m.Success)
            {
                double pct;
                if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out pct))
                {
                    int val = Math.Min(99, (int)pct);
                    this.BeginInvoke((Action)(() =>
                    {
                        progressBar.Value = val;
                        SetStatus(string.Format("Downloading... {0:0.0}%", pct));
                    }));
                }
            }
        }

        private void CancelDownload()
        {
            try { _downloadProcess?.Kill(); } catch { }
            SetStatus("Cancelled.");
            SetDownloading(false);
        }

        private void SetDownloading(bool downloading)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => SetDownloading(downloading)));
                return;
            }
            _isDownloading = downloading;
            btnDownload.Text = downloading ? "&Cancel" : "&Download";
            txtURL.Enabled = !downloading;
            rdoMP3.Enabled = !downloading;
            rdoVideo.Enabled = !downloading;
            txtFolder.Enabled = !downloading;
            btnBrowse.Enabled = !downloading;
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => SetStatus(text))); return; }
            lblStatus.Text = text;
        }

        private void AppendLog(string line)
        {
            if (InvokeRequired) { BeginInvoke((Action)(() => AppendLog(line))); return; }
            txtLog.AppendText(line + Environment.NewLine);
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select download folder";
                dlg.SelectedPath = txtFolder.Text;
                if (dlg.ShowDialog() == DialogResult.OK)
                    txtFolder.Text = dlg.SelectedPath;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_isDownloading) CancelDownload();
            Application.Exit();
        }
    }
}
