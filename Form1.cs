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
        bool running = false;
        public WebServer() {
            InitializeComponent();
        }
        private void toggleServer() {
            if (this.running) {
                mainServer = new Server();
            } 
            else if (mainServer != null)
            {
                mainServer.Terminate();
                mainServer = null;
            }
        }

        private void label1_Click(object sender, EventArgs e) {

        }
        bool actionInProgress = false;
        private void button1_Click(object sender, EventArgs e) {
            if (actionInProgress) return;
            actionInProgress = true;
            running = !running;
            toggleServer();
            this.button1.Text = running ? "Running" : "Not Running";
            actionInProgress = false;
        }
    }
}


