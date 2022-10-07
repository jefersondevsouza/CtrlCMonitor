using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsCtrlCMonitor
{
    public partial class Form_Alert : Form
    {
        private int TEMPO = 60000; //1 minuto configurados em milisegundos
        private Form_Alert.enmAction action;
        private int x, y;
        private string detalhe = string.Empty;

        public Form_Alert()
        {
            InitializeComponent();
        }

        public string Mensagem
        {
            get
            {
                return "CTRL + C -> " + detalhe.Trim();
            }
        }

        public void showAlert(string detalhe, enmType type, bool automaticClose, int secondsAutoClose)
        {
            this.detalhe = detalhe;

            this.Opacity = 0.0;
            this.StartPosition = FormStartPosition.Manual;
            string fname;

            for (int i = 1; i < 30; i++)
            {
                fname = "alert" + i.ToString();
                Form_Alert frm = (Form_Alert)Application.OpenForms[fname];

                if (frm == null)
                {
                    this.Name = fname;
                    this.x = Screen.PrimaryScreen.WorkingArea.Width - this.Width + 15;
                    this.y = Screen.PrimaryScreen.WorkingArea.Height - this.Height * i - 5 * i;
                    this.Location = new Point(this.x, this.y);
                    break;
                }
            }

            this.x = Screen.PrimaryScreen.WorkingArea.Width - base.Width - 5;

            switch (type)
            {
                case enmType.Success:
                    //this.pictureBox1.Image = Resources.success;
                    this.BackColor = Color.SeaGreen;
                    break;
                case enmType.Error:
                    //this.pictureBox1.Image = Resources.error;
                    this.BackColor = Color.DarkRed;
                    break;
                case enmType.Info:
                    //this.pictureBox1.Image = Resources.info;
                    this.BackColor = Color.RoyalBlue;
                    break;
                case enmType.Warning:
                    //this.pictureBox1.Image = Resources.warning;
                    this.BackColor = Color.DarkOrange;
                    break;
                case enmType.Notification:
                    this.BackColor = SystemColors.Highlight;
                    break;
            }

            this.lblMsg.Text = this.Mensagem;
            this.Show();
            this.action = enmAction.start;
            this.timer1.Interval = 1;

            if (automaticClose)
            {
                this.TEMPO = secondsAutoClose * 1000;
            }


            this.timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            switch (this.action)
            {
                case enmAction.wait:
                    timer1.Interval = 1 * TEMPO;
                    action = enmAction.close;
                    break;
                case Form_Alert.enmAction.start:
                    this.timer1.Interval = 1;
                    this.Opacity += 0.1;
                    if (this.x < this.Location.X)
                    {
                        this.Left--;
                    }
                    else
                    {
                        if (this.Opacity == 1.0)
                        {
                            action = Form_Alert.enmAction.wait;
                        }
                    }
                    break;
                case enmAction.close:
                    timer1.Interval = 1;
                    this.Opacity -= 0.1;

                    this.Left -= 3;
                    if (base.Opacity == 0.0)
                    {
                        base.Close();
                    }
                    break;

            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            timer1.Interval = 1;
            action = enmAction.close;
        }

        public enum enmAction
        {
            wait,
            start,
            close
        }

        public enum enmType
        {
            Success,
            Warning,
            Error,
            Info,
            Notification
        }
    }
}
