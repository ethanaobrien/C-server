using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;

namespace WebServer {
    public partial class WebServer : Form {
        Server mainServer;
        private string pathToServe;
        private int port = 8080;
        private bool running = false;
        public WebServer() {
            InitializeComponent();
            loadSettings();
        }
        private void toggleServer() {
            if (this.running) {
                startServer();
            } 
            else if (mainServer != null)
            {
                mainServer.Terminate();
                mainServer = null;
            }
        }
        private void startServer()
        {
            mainServer = new Server(pathToServe, port, PUT.Checked, DELETE.Checked);
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
            byte[] data = Encoding.UTF8.GetBytes(port+"\n"+pathToServe+"\n"+(PUT.Checked?"1":"0")+"\n" + (DELETE.Checked ? "1" : "0") + "\n");
            fs.Write(data, 0, data.Length);
            fs.Close();
        }
            catch (Exception e){}
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
                    Console.WriteLine(cl);
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
                    i++;
                }
                this.ServingPath.Text = "Currently Serving: " + pathToServe;
                this.Port.Value = new decimal(new int[] { port, 0, 0, 0 });
            }
            catch (Exception e){}
        }
        private void label1_Click(object sender, EventArgs e) {

        }
        bool actionInProgress = false;
        private void button1_Click(object sender, EventArgs e) {
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
            else
            {
                Environment.Exit(1);
            }
        }

        private void ChooseDirectory_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (mainServer != null)
                {
                    mainServer.setMainPath(dialog.SelectedPath);
                }
                pathToServe = dialog.SelectedPath;
                this.ServingPath.Text = "Currently Serving: "+dialog.SelectedPath;
                saveSettings();
            }
        }

        private void Port_ValueChanged(object sender, EventArgs e)
        {
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
    }
}


