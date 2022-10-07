using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsCtrlCMonitor
{
    //https://www.devmedia.com.br/manipulando-a-area-de-transferencia/3948
    public partial class Form1 : Form
    {
        [DllImport("User32.dll")]
        public static extern IntPtr SetClipboardViewer(IntPtr hwnd);//para definir a tela de vizualização do clipboard

        [DllImport("user32.dll")]
        public static extern bool ChangeClipboardChain(IntPtr hwndRemove, IntPtr hwndNext);//para alterar a tela de vizualição do clipboard

        private IntPtr proxjanela;

        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x030D;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        bool fechar = false;
        bool momentoIteracao = false;

        public Form1()
        {
            InitializeComponent();
            string cont = "Ctrl+C Monitor started";
            clipDate c = new clipDate(true, false, false, cont, null);
            CapturarCtrlC(c);
            this.notifyIcon1.Icon = this.Icon;
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipTitle = "Ctrl+C Monitor";
            this.notifyIcon1.BalloonTipText = "This app is running here";
            this.notifyIcon1.DoubleClick += NotifyIcon1_DoubleClick;

            listBox1.Click += ListBox1_Click;
        }

        private void ListBox1_Click(object sender, EventArgs e)
        {
            if (this.listBox1.SelectedItem != null)
            {
                clipDate item = (clipDate)this.listBox1.SelectedItem;

                momentoIteracao = true;
                item.Transfer();
                momentoIteracao = false;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DefinirEsteFormComoClipboardView();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fechar)
            {
                ChangeClipboardChain(this.Handle, proxjanela);
            }
            else
            {
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                e.Cancel = true;
                this.notifyIcon1.ShowBalloonTip(2000);

                this.DefinirEsteFormComoClipboardView();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.fechar = true;
            this.Close();
        }

        private void NotifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.Visible == false)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;

                this.DefinirEsteFormComoClipboardView();
            }
        }

        private void DefinirEsteFormComoClipboardView()
        {
            this.momentoIteracao = true;
            proxjanela = SetClipboardViewer(this.Handle);
            this.momentoIteracao = false;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (Clipboard.ContainsText())
                    {
                        string cont = Clipboard.GetText();
                        clipDate c = new clipDate(true, false, false, cont, null);
                        CapturarCtrlC(c);
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {

                        var o = Clipboard.GetFileDropList();
                        foreach (var item in o)
                        {
                            clipDate c = new clipDate(false, true, false, item, null);
                            CapturarCtrlC(c);
                        }
                    }
                    else
                    {
                        if (Clipboard.ContainsImage())
                        {

                        }
                        if (Clipboard.ContainsAudio())
                        {

                        }

                        var o = Clipboard.GetDataObject();
                        string texto = "Objeto capturado em " + DateTime.Now.ToShortDateString();
                        clipDate c = new clipDate(false, false, true, texto, o);
                        CapturarCtrlC(c);
                    }
                    SendMessage(proxjanela, WM_DRAWCLIPBOARD, m.WParam, m.LParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (m.WParam == proxjanela)
                    {
                        proxjanela = m.LParam;
                    }
                    else
                    {
                        SendMessage(proxjanela, WM_DRAWCLIPBOARD,
                                m.WParam, m.LParam);
                    }
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void CapturarCtrlC( clipDate c)
        {
            if (!momentoIteracao)
            {
                string msn = c.text;
                if (!string.IsNullOrWhiteSpace(msn))
                {
                    listBox1.Items.Add(c);

                    this.Alert(msn, Form_Alert.enmType.Notification);
                }
            }
        }



        public void Alert(string detalhe, Form_Alert.enmType type)
        {
            Form_Alert frm = new Form_Alert();
            frm.showAlert(detalhe, type, true, 1);
        }


    }

    class clipDate
    {
        bool isText = false;
        bool isCopyTo = false;
        bool isCopyDate = false;

        public string text = string.Empty;
        object objData = null;

        public clipDate(bool isText, bool isCopyTo, bool isCopyDate, string text, object obj)
        {
            this.isText = isText;
            this.isCopyTo = isCopyTo;
            this.isCopyDate = isCopyDate;
            this.text = text;
            this.objData = obj;
        }

        public void Transfer()
        {
            if (isText)
            {
                Clipboard.SetText(text);
            }
            else if (isCopyTo)
            {
                Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection() { this.text });
            }
            else if (isCopyDate)
            {
                Clipboard.SetDataObject(objData);
            }
        }

        public override string ToString()
        {
            return text;
        }
    }
}
