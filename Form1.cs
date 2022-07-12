using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace LoopTimer
{
    public partial class Form1 : Form
    {
        private readonly Thread _mouseTrackerThread;

        public Form1()
        {
            InitializeComponent();

            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 120, Screen.PrimaryScreen.Bounds.Height - 150);

            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
            this.circularProgressBar1.Maximum = config?.MaxSeconds ?? 90;

            // debug code
            //this.circularProgressBar1.Maximum = 10;
            
            this.circularProgressBar1.Value = 0;

            this._mouseTrackerThread = new Thread(this.MouseTracker);
            _mouseTrackerThread.Start();
        }

        private static string GetDeployPath()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var codeBaseUri = new UriBuilder(codeBase).Path;
            return Path.GetDirectoryName(Uri.UnescapeDataString(codeBaseUri));
        }

        private int _currentValue;

        private void Reset()
        {
            this.timer1.Stop();
            this.circularProgressBar1.Value = 0;
            this.SetProgressText(string.Empty);
            this.circularProgressBar1.Refresh();
            this.pictureBox1.Visible = true;
            this._currentValue = 0;
        }

        private void SetProgressText(string text)
        {
            this.circularProgressBar1.Text = text;

            if (string.IsNullOrEmpty(text)) return;

            // adjust font size according to the length of the text
            int fontSize;
            if (text.Length <= 2)
            {
                fontSize = 48;
            }
            else if (text.Length == 3)
            {
                fontSize = 36;
            }
            else if (text.Length == 4)
            {
                fontSize = 24;
            }
            else
            {
                fontSize = 12;
            }

            if (Math.Abs(this.circularProgressBar1.Font.Size - fontSize) > 0.000000001)
            {
                this.circularProgressBar1.Font = new Font("Arial", fontSize, FontStyle.Bold);
                this.circularProgressBar1.Refresh();
            }
        }

        private void Pause()
        {
            this.timer1.Stop();
            this.circularProgressBar1.ForeColor = Color.Gray;
        }

        private void MouseTracker()
        {
            while (true)
            {
                try
                {
                    var position = new Point(0, 0);
                    this.Invoke(new MethodInvoker(delegate { position = this.PointToClient(MousePosition); }));

                    var x = position.X;
                    var y = position.Y;

                    var show = x > 25 && x < 99 && y > 25 && y < 99;

                    if (this.panel1.Visible != show)
                    {
                        this.Invoke(new MethodInvoker(delegate { this.panel1.Visible = show; }));
                    }

                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private void Start()
        {
            if (this.timer1.Enabled)
            {
                this._currentValue = 0;
            }
            else
            {
                this.RefreshProgress();
                this.timer1.Start();
                this.pictureBox1.Visible = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.RefreshProgress();
        }

        private void RefreshProgress()
        {
            this.circularProgressBar1.Value = this._currentValue < this.circularProgressBar1.Maximum
                ? this._currentValue
                : this.circularProgressBar1.Maximum;

            var num = this._currentValue - this.circularProgressBar1.Maximum;

            if (num <= 0)
            {
                this.SetProgressText(Math.Abs(num).ToString());
                if (this.circularProgressBar1.ForeColor != Color.Green)
                {
                    this.circularProgressBar1.ForeColor = Color.Green;
                    this.circularProgressBar1.Refresh();
                }
            }
            else
            {
                this.SetProgressText($@"+{num}");
                if (this.circularProgressBar1.ForeColor != Color.Red)
                {
                    this.circularProgressBar1.ForeColor = Color.Red;
                }

                this.circularProgressBar1.Refresh();
            }

            this._currentValue++;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.Start();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            this.Pause();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this._mouseTrackerThread.Abort();
            this.timer1.Stop();
            this.Close();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            this.Reset();
        }
    }
}