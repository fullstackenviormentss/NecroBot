﻿using PoGo.NecroBot.Logic.Common;
using PoGo.NecroBot.Logic.Event;
using PoGo.NecroBot.Logic.Event.UI;
using PoGo.NecroBot.Logic.Logging;
using PoGo.NecroBot.Logic.State;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
namespace PoGo.NecroBot.Logic.Forms
{

    public partial class AutoUpdateForm : Form
    {
        private string CHANGE_LOGS = "https://cdn.rawgit.com/Necrobot-Private/NecroBot/master/changelogs/v{0}.rft";

        public string LatestVersion { get; set; }

        public string CurrentVersion { get; set; }

        public bool AutoUpdate { get; set; }
        public string DownloadLink { get; set; }
        public string Destination { get; set; }

        public ISession Session { get; set; }
        public AutoUpdateForm()
        {
            InitializeComponent();
        }

        private void AutoUpdateForm_Load(object sender, EventArgs e)
        {
            richTextBox1.SetInnerMargins(25, 25, 25, 25);
            lblCurrent.Text = CurrentVersion;
            lblLatest.Text = LatestVersion;
            var Data;
            using (WebClient webClient = new System.Net.WebClient())
            {
                WebClient n = new WebClient();
                Data = n.DownloadString("https://api.github.com/repos/Necrobot-Private/Necrobot/releases/tags" + LatestVersion);
            }
            var changelog = Data.body;
            LoadChangeLogs(changelog);
            if (AutoUpdate)
            {
                btnUpdate.Enabled = false;
                btnUpdate.Text = "Downloading...";
                StartDownload();
            }
        }

        private void LoadChangeLogs(string changelog)
        {
            Task.Run(async () =>
            {

                await Task.Delay(2000);

            }).ContinueWith((t) =>
            {
                //load content
                Invoke(new Action(() =>
                {
                    richTextBox1.Text = changelog;
                }));

            });
        }

        public bool DownloadFile(string url, string dest)
        {
            Session.EventDispatcher.Send(new UpdateEvent
            {
                Message = Session.Translation.GetTranslation(TranslationString.DownloadingUpdate)
            });

            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;

                    client.DownloadFileAsync(new Uri(url), dest);
                    Logger.Write(dest, LogLevel.Info);
                }
                catch
                {
                    Close();
                }
                return true;
            }
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Session.EventDispatcher.Send(new UpdateEvent
            {
                Message = Session.Translation.GetTranslation(TranslationString.FinishedDownloadingRelease)
            });

            Invoke(new Action(() =>
            {
                DialogResult = DialogResult.OK;
                Close();
            }));
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                progressBar1.Value = e.ProgressPercentage;
            }));
        }


        public void StartDownload()
        {
            Session.EventDispatcher.Send(new StatusBarEvent($"Auto Update v{LatestVersion}, Downloading from {DownloadLink}"));
            Logger.Write(DownloadLink, LogLevel.Info);
            DownloadFile(DownloadLink, Destination);

        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Text = "Downloading...";
            btnUpdate.Enabled = false;
            StartDownload();
        }

        private void Btncancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        public void GetJson()
        {
            using (WebClient webClient = new System.Net.WebClient())
            {
                WebClient n = new WebClient();
                var json = n.DownloadString("https://api.github.com/repos/Necrobot-Private/Necrobot/releases/latest");
                string valueOriginal = Convert.ToString(json);
                Console.WriteLine(json);
            }
        }
    }
}
