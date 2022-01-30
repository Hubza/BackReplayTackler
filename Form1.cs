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

namespace BackReplayTackler
{
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
            if (System.IO.File.Exists(OutputPath + "/temp.html"))
            {
                string[] fileContents = System.IO.File.ReadAllLines(OutputPath + "/temp.html");
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
                            // TODO: parse mods
                        }

                        wayThrough += 1;
                    }
                }
                label3.Text = plays[10].username;
            }
            else
            {
                label3.Text = "Uh oh";
            }
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
    }
}
