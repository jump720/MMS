using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AISTray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //  Task<string> pqrsNoti = ExecPQRSNotification();


            DateTime oldDate = DateTime.Now;
            DateTime newDate = new DateTime(oldDate.Year, oldDate.Month, oldDate.Day, 06, 00, 00);
            newDate = newDate.AddDays(1);


            var d = newDate - oldDate;
            //var m = d.TotalMilliseconds;
            ////string s = await ExecPQRSNotification();
            //MessageBox.Show(m.ToString());

            //timerPQRS.Interval = (int)d.TotalMilliseconds;
            timerPQRS.Enabled = true;
            timerPQRS.Start();
            //duration = 2; //(int)d.TotalSeconds;
            duration = (int)d.TotalMinutes;
            lbNext.Text = "Next Exec " + newDate.ToString("dd-MM-yyyy hh:mm:ss");
            tbTime.Text = duration.ToString();
            tbStatus.Text = "Started";
        }


        private async Task<string> ExecPQRSNotification()
        {
            HttpClient client = new HttpClient();

            // New code:
            #if DEBUG
                        client.BaseAddress = new Uri("http://dev.apextoolgroup.com.co:90/");
            #else
                                        client.BaseAddress = new Uri("http://ais.apextoolgroup.com.co/");
            #endif
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string path = "api/pqrs/PQRSNotification";
            HttpResponseMessage response = await client.GetAsync(path);
            string result = "";
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        int duration = 0;
        private async void timerPQRS_Tick(object sender, EventArgs e)
        {

            duration--;
            if (duration == 0)
            {
                string s = await ExecPQRSNotification();
               // MessageBox.Show(s);

                DateTime oldDate = DateTime.Now;
                DateTime newDate = new DateTime(oldDate.Year, oldDate.Month, oldDate.Day, 06, 00, 00);
                newDate = newDate.AddDays(1);

                var d = newDate - oldDate;

                // timerPQRS.Interval = (int)d.TotalMilliseconds;
                //duration = 2;//(int)d.TotalSeconds;

                duration = (int)d.TotalMinutes;
                lbNext.Text = "Next Exec " + newDate.ToString("dd-MM-yyyy hh:mm:ss");
                lbLast.Text = "Ultima Ejecución" + oldDate.ToString("dd-MM-yyyy hh:mm:ss");
            }

            tbTime.Text = duration.ToString();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            timerPQRS.Stop();
            tbStatus.Text = "Stopped";
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                niAIS.ShowBalloonTip(1000, "AIS Tray Notification", "AIS will continue running ", ToolTipIcon.Info);
            }
        }

        private void niAIS_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
