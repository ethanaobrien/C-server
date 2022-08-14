using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WebServer
{
    public partial class WebServer : Form
    {
        private double version = 2.7;
        Server mainServer;
        private string pathToServe;
        private int port = 8080;
        private bool running = false;
        private bool actionInProgress = false;
        public WebServer()
        {
            InitializeComponent();
            loadSettings();
            this.BottomLeftLabel.Text = "C Server Version " + this.version;
            Updater update = new Updater(this.version);
            update.check4Updates((double version) =>
            {
                this.UpdateLink.Visible = true;
                this.UpdateTitle.Visible = true;
                this.UpdateVersion.Visible = true;
                this.UpdateVersion.Text = "Version " + version + " is out!";
            });
            actionInProgress = true;
            toggleServer();
            actionInProgress = false;
        }
        private void toggleServer()
        {
            if (!this.running)
            {
                this.MSG.Visible = false;
                this.Status.Text = "Running";
                this.running = true;
                startServer();
                SetURLText();
            }
            else if (mainServer != null)
            {
                mainServer.Terminate();
                mainServer = null;
                SetURLText();
                this.running = false;
                this.Status.Text = "Not Running";
                this.MSG.Text = "Not Running";
                this.MSG.Visible = true;
            }
        }
        private void startServer()
        {
            mainServer = new Server(pathToServe, port, PUT.Checked, DELETE.Checked, CORS.Checked, AutoIndex.Checked, ListDirectory.Checked, localNetwork.Checked);
        }
        private Label[] URLS = new Label[5] { null, null, null, null, null};
        private void SetURLText()
        {
            for (int i = 0; i < URLS.Length; i++)
            {
                if (URLS[i] == null) break;
                this.Controls.Remove(URLS[i]);
                URLS[i] = null;
                this.URL.Visible = false;
            }
            if (this.mainServer != null)
            {
                string[] urls = this.mainServer.GetURLs();
                this.URL.Visible = true;
                int location = 368;
                for (int i = 0; i < urls.Length; i++)
                {
                    if (urls[i] == null || urls[i].Length == 0) break;
                    if (5 < i) break;
                    URLS[i] = new System.Windows.Forms.Label();
                    URLS[i].AutoSize = true;
                    URLS[i].Cursor = System.Windows.Forms.Cursors.Hand;
                    URLS[i].Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
                    URLS[i].ForeColor = System.Drawing.SystemColors.Highlight;
                    URLS[i].Location = new System.Drawing.Point(80, location);
                    URLS[i].Name = "URL"+i;
                    URLS[i].Size = new System.Drawing.Size(276, 17);
                    URLS[i].TabIndex = 14;
                    URLS[i].Text = "http://"+urls[i]+":"+this.port+"/";
                    URLS[i].Click += new System.EventHandler(this.URL_Click);
                    this.Controls.Add(URLS[i]);
                    location += 30;
                }
            }
        }
        private void URL_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(((Label)sender).Text);
        }
        private void saveSettings()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "C-server", "config.1");
                string folder = Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                FileStream fs = File.Create(path);
                byte[] data = Encoding.UTF8.GetBytes(port + "\n" + pathToServe + "\n" + (PUT.Checked ? "1" : "0") + "\n" + (DELETE.Checked ? "1" : "0") + "\n" + (CORS.Checked ? "1" : "0") + "\n" + (AutoIndex.Checked ? "1" : "0") + "\n" + (ListDirectory.Checked ? "1" : "0") + "\n" + (localNetwork.Checked ? "1" : "0") + "\n");
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
                    else if (i == 6)
                    {
                        ListDirectory.Checked = (cl.Equals("1"));
                    }
                    else if (i == 7)
                    {
                        localNetwork.Checked = (cl.Equals("1"));
                    }
                    i++;
                }
                this.ServingPath.Text = "Currently Serving: " + pathToServe;
                this.Port.Value = new decimal(new int[] { port, 0, 0, 0 });
                this.SetURLText();
            }
            catch (Exception e) { }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (actionInProgress) return;
            actionInProgress = true;
            toggleServer();
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
            if (mainServer != null)
            {
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

        private void ListDirectory_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                mainServer.setDirectory(ListDirectory.Checked);
            }
            saveSettings();
        }

        private void viewOnGithub_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ethanaobrien/C-server");
        }

        private void UpdateLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/ethanaobrien/C-server/releases/latest");
        }

        private void localNetwork_CheckStateChanged(object sender, EventArgs e)
        {
            if (mainServer != null)
            {
                this.MSG.Text = "Restart server to init changes";
                this.MSG.Visible = true;
                mainServer.setLocalNetwork(localNetwork.Checked);
            }
            saveSettings();
        }
    }
}


