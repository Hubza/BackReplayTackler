using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Net;
using System.Text.RegularExpressions;
using OsuParsers.Replays;

namespace BackReplayTackler
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string ArchiveURL;
        public string OutputPath;

        private void button1_Click(object sender, EventArgs e)
        {
            ArchiveURL = textBox1.Text;
            OutputPath = textBox2.Text;

            label3.Text = "Downloading from " + ArchiveURL + "!";
            WebClient wc = new WebClient();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadCompleted;
                wc.DownloadFileAsync(
                    new System.Uri(ArchiveURL),
                    OutputPath + "/temp.html"
                );
            
        }

        string xStrip(string text, string tag)
        {
            return text.Replace("<" + tag + ">", "").Replace("</" + tag + ">", "");
        }

        void wc_DownloadCompleted(object sender, AsyncCompletedEventArgs a)
        {
            label3.Text = "Downloaded: " + a.Error + ". Will now parse!";

            WebClient wc = new WebClient();
            wc.DownloadFile(
                new System.Uri("https://dev.hubza.co.uk/getbeatmaphash.php?b=" + textBox3.Text),
                OutputPath + "/temp.txt"
            );

            if (System.IO.File.Exists(OutputPath + "/temp.html") && System.IO.File.Exists(OutputPath + "/temp.txt"))
            {
                string[] fileContents = System.IO.File.ReadAllLines(OutputPath + "/temp.html");
                string hash = System.IO.File.ReadAllLines(OutputPath + "/temp.txt")[0];
                bool inTable = false;
                List<string> table = new List<string>();
                foreach(string line in fileContents)
                {
                    if(line.Contains("<div class=\"beatmapListing\">"))
                    {
                        inTable = true;
                    }
                    else if(line.Contains("</div>"))
                    {
                        inTable = false;
                    }
                    else if(inTable == true)
                    {
                        table.Add(line);
                    }
                }


                Play[] plays = new Play[59];
                int playIndex = 0;
                bool inPlayer = false;
                int wayThrough = 0;
                foreach(string line in table)
                {
                    if(line.Contains("<tr class=\"row1p\">") || line.Contains("<tr class=\"row2p\">"))
                    {
                        inPlayer = true;
                        plays[playIndex] = new Play();
                    }
                    else if(line.Contains("</tr>"))
                    {
                        inPlayer = false;
                        playIndex++;
                        wayThrough = 0;
                    } else if(line.Contains("<td>") && line.Contains("</td>"))
                    {
                        /* <td>#1</td>
						<td><img src="/web/20121226130435im_/http://osu.ppy.sh/images/A_small.png"/></td>
						<td><b>40,336,802</b></td>
						<td>93.83%</td>
						<td><img class="flag" src="https://web.archive.org/web/20121226130435im_/https://s.ppy.sh/images/flags/kr.gif" title="Korea"/> <a href="/web/20121226130435/http://osu.ppy.sh/u/124493">Cookiezi</a></td>
						<td>1315</td>
						<td>1815&nbsp;&nbsp;/&nbsp;&nbsp;119&nbsp;&nbsp;/&nbsp;&nbsp;36</td>
						<td>225</td>
						<td>16</td>
						<td>13</td>
						<td>None</td>
						<td><a onclick="report(920976587);">Report</a></td> */

                        if(wayThrough == 0)
                        {
                            // don't need to do this one
                        }

                        if(wayThrough == 1)
                        {
                            // todo: parse rank
                            // update: rank shows up fine in-game, not needed!
                        }

                        if(wayThrough == 2)
                        {
                            plays[playIndex].score = float.Parse(xStrip(xStrip(line, "td"), "b"));
                        }

                        if(wayThrough == 3)
                        {
                            plays[playIndex].accuracy = float.Parse(xStrip(line, "td").Replace("%", ""));
                        }

                        if(wayThrough == 4)
                        {
                            plays[playIndex].username = Regex.Matches(line, "(?<=\">)(.*)(?=</a>)")[0].Value;
                        }

                        if(wayThrough == 5)
                        {
                            plays[playIndex].maxCombo = int.Parse(xStrip(line, "td"));
                        }

                        if(wayThrough == 6)
                        {
                            string temp = xStrip(line, "td");
                            temp = temp.Replace(@"&nbsp;&nbsp;/&nbsp;&nbsp;", "$");
                            string[] temp2 = temp.Split('$');
                            plays[playIndex].s300 = int.Parse(temp2[0]);
                            plays[playIndex].s100 = int.Parse(temp2[1]);
                            plays[playIndex].s50 = int.Parse(temp2[2]);
                        }
                        if (wayThrough == 7)
                        {
                            plays[playIndex].sgeki = int.Parse(xStrip(line, "td"));
                        }
                        if (wayThrough == 8)
                        {
                            plays[playIndex].skatu = int.Parse(xStrip(line, "td"));
                        }
                        if (wayThrough == 9)
                        {
                            plays[playIndex].misses = int.Parse(xStrip(line, "td"));
                        }

                        if (wayThrough == 9)
                        {
                            string[] mods = xStrip(line, "td").Split(',');
                            int modNum = 0;
                            foreach(string mod in mods)
                            {
                                // if anyone knows how to do this help, i don't.
                            }
                            plays[playIndex].mods = modNum;
                        }

                        wayThrough += 1;
                    }
                }
                label3.Text = plays[10].username;

                foreach(Play play in plays)
                {
                    try
                    {
                        if (play != null)
                        {
                            label3.Text = "Writing replay for play by " + play.username;
                            Replay replay = new Replay();
                            replay.Combo = (ushort)play.maxCombo;
                            replay.Count100 = (ushort)play.s100;
                            replay.Count50 = (ushort)play.s50;
                            replay.Count300 = (ushort)play.s300;
                            replay.PlayerName = play.username;
                            replay.Ruleset = OsuParsers.Enums.Ruleset.Standard;
                            replay.ReplayTimestamp = new DateTime(0);
                            replay.ReplayScore = (int)play.score;
                            replay.ReplayLength = 90000000;
                            replay.Mods = 0;
                            replay.CountGeki = (ushort)play.sgeki;
                            replay.CountKatu = (ushort)play.skatu;
                            replay.CountMiss = (ushort)play.misses;
                            replay.OnlineId = long.Parse(textBox3.Text);
                            replay.OsuVersion = 20200101;
                            replay.BeatmapMD5Hash = hash;
                            replay.Save(OutputPath + "/" + replay.OnlineId + "_" + replay.PlayerName + ".osr");
                        }
                    }catch
                    {

                    }
                }
            }
            else
            {
                label3.Text = "Uh oh";
            }
            label3.Text = "Done!";
        }

        void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            label3.Text = e.ProgressPercentage + "% downloaded.";
            progressBar1.Value = e.ProgressPercentage;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }

    public class Play
    {
        public string username;
        public float accuracy;
        public float score;
        public int s300;
        public int s100;
        public int s50;
        public int sgeki;
        public int skatu;
        public int misses;
        public int mods;
        public int maxCombo;
        public string rank;
    }

    public enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        TouchDevice = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        /// <summary>
        /// Only set along with DoubleTime. i.e: NC only gives 576
        /// </summary>
        Nightcore = 512,
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        /// <summary>
        /// Autopilot
        /// </summary>
        Relax2 = 8192,
        /// <summary>
        /// Only set along with SuddenDeath. i.e: PF only gives 16416
        /// </summary>
        Perfect = 16384,
        Key4 = 32768,
        Key5 = 65536,
        Key6 = 131072,
        Key7 = 262144,
        Key8 = 524288,
        FadeIn = 1048576,
        Random = 2097152,
        Cinema = 4194304,
        Target = 8388608,
        Key9 = 16777216,
        KeyCoop = 33554432,
        Key1 = 67108864,
        Key3 = 134217728,
        Key2 = 268435456,
        ScoreV2 = 536870912,
        LastMod = 1073741824,
        KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
        FreeModAllowed = NoFail | Easy | Hidden | HardRock | SuddenDeath | Flashlight | FadeIn | Relax | Relax2 | SpunOut | KeyMod,
        ScoreIncreaseMods = Hidden | HardRock | DoubleTime | Flashlight | FadeIn
    }
}
