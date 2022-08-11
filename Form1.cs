using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WebServer
{
    public partial class WebServer : Form
    {
        Server mainServer;
        private string pathToServe;
        private int port = 8080;
        private bool running = false;
        private string localHostURL = "http://127.0.0.1/";
        public WebServer()
        {
            InitializeComponent();
            loadSettings();
        }
        private void toggleServer()
        {
            if (this.running)
            {
                startServer();
            }
            else if (mainServer != null)
            {
                mainServer.Terminate();
                mainServer = null;
                this.URL.Visible = false;
                this.MSG.Text = "Not Running";
                this.MSG.Visible = true;
            }
        }
        private void startServer()
        {
            this.MSG.Visible = false;
            this.URL.Visible = true;
            mainServer = new Server(pathToServe, port, PUT.Checked, DELETE.Checked, CORS.Checked, AutoIndex.Checked);
            this.localHostURL = "http://127.0.0.1:" + port + "/";
            this.URL.Text = "Open " + this.localHostURL + " in your browser";
        }
        private void saveSettings()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "C-server", "config.1");
                string folder = Path.GetDirectoryName(path);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                FileStream fs = File.Create(path);
                byte[] data = Encoding.UTF8.GetBytes(port + "\n" + pathToServe + "\n" + (PUT.Checked ? "1" : "0") + "\n" + (DELETE.Checked ? "1" : "0") + "\n" + (CORS.Checked ? "1" : "0") + "\n" + (AutoIndex.Checked ? "1" : "0") + "\n");
                fs.Write(data, 0, data.Length);
                fs.Close();
            }
            catch (Exception e) { }
        }
        private void loadSettings()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "C-server", "config.1");
                if (!File.Exists(path))
                {
                    return;
                }
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader reader = new BinaryReader(fs);
                reader.BaseStream.Position = 0;
                byte[] bytes = reader.ReadBytes((int)fs.Length);
                reader.Close();
                fs.Close();
                string[] data = Encoding.UTF8.GetString(bytes, 0, bytes.Length).Split('\n');
                int i = 0;
                foreach (string cl in data)
                {
                    if (i == 0)
                    {
                        port = Int32.Parse(cl);
                    }
                    else if (i == 1)
                    {
                        pathToServe = cl;
                    }
                    else if (i == 2)
                    {
                        PUT.Checked = (cl.Equals("1"));
                    }
                    else if (i == 3)
                    {
                        DELETE.Checked = (cl.Equals("1"));
                    }
                    else if (i == 4)
                    {
                        CORS.Checked = (cl.Equals("1"));
                    }
                    else if (i == 5)
                    {
                        AutoIndex.Checked = (cl.Equals("1"));
                    }
                    i++;
                }
                this.ServingPath.Text = "Currently Serving: " + pathToServe;
                this.Port.Value = new decimal(new int[] { port, 0, 0, 0 });
                this.localHostURL = "http://127.0.0.1:" + port + "/";
                this.URL.Text = "Open " + this.localHostURL + " in your browser";
            }
            catch (Exception e) { }
        }
        bool actionInProgress = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (actionInProgress) return;
            actionInProgress = true;
            running = !running;
            toggleServer();
            this.Status.Text = running ? "Running" : "Not Running";
            actionInProgress = false;
        }

        private void WebServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.Terminate();
                mainServer = null;
            }
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
            Environment.Exit(1);
        }

        private void ChooseDirectory_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                pathToServe = dialog.SelectedPath.Replace('\\', '/');
                if (mainServer != null)
                {
                    mainServer.setMainPath(pathToServe);
                }
                this.ServingPath.Text = "Currently Serving: " + pathToServe;
                saveSettings();
            }
        }

        private void Port_ValueChanged(object sender, EventArgs e)
        {
            if (this.running)
            {
                this.URL.Visible = false;
                this.MSG.Text = "Restart server to init changes";
                this.MSG.Visible = true;
            }
            this.port = Convert.ToInt32(Math.Round(this.Port.Value, 0));
            saveSettings();
        }

        private void PUT_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.setPut(PUT.Checked);
            }
            saveSettings();
        }

        private void DELETE_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.setDelete(DELETE.Checked);
            }
            saveSettings();
        }
        private void CORS_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.setCors(CORS.Checked);
            }
            saveSettings();
        }
        private void AutoIndex_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.setIndex(AutoIndex.Checked);
            }
            saveSettings();
        }

        private void viewOnGithub_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ethanaobrien/C-server");
        }

        private void URL_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(this.localHostURL);
        }
    }
}


