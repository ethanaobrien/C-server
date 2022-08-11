namespace WebServer
{
    partial class WebServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.Status = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.NumericUpDown();
            this.ChooseDirectory = new System.Windows.Forms.Button();
            this.ServingPath = new System.Windows.Forms.Label();
            this.PUT = new System.Windows.Forms.CheckBox();
            this.DELETE = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.viewOnGithub = new System.Windows.Forms.Label();
            this.CORS = new System.Windows.Forms.CheckBox();
            this.AutoIndex = new System.Windows.Forms.CheckBox();
            this.URL = new System.Windows.Forms.Label();
            this.MSG = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Port)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(58, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Simple C Server";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(200, 112);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 48);
            this.button1.TabIndex = 1;
            this.button1.Text = "Toggle";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Status
            // 
            this.Status.AutoSize = true;
            this.Status.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Status.Location = new System.Drawing.Point(61, 125);
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(116, 24);
            this.Status.TabIndex = 2;
            this.Status.Text = "Not Running";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(86, 181);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port:";
            // 
            // Port
            // 
            this.Port.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Port.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Port.Location = new System.Drawing.Point(130, 179);
            this.Port.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.Port.Minimum = new decimal(new int[] {
            80,
            0,
            0,
            0});
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(107, 23);
            this.Port.TabIndex = 5;
            this.Port.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
            this.Port.ValueChanged += new System.EventHandler(this.Port_ValueChanged);
            // 
            // ChooseDirectory
            // 
            this.ChooseDirectory.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.ChooseDirectory.Location = new System.Drawing.Point(113, 309);
            this.ChooseDirectory.Name = "ChooseDirectory";
            this.ChooseDirectory.Size = new System.Drawing.Size(140, 39);
            this.ChooseDirectory.TabIndex = 6;
            this.ChooseDirectory.Text = "Choose Directory";
            this.ChooseDirectory.UseVisualStyleBackColor = true;
            this.ChooseDirectory.Click += new System.EventHandler(this.ChooseDirectory_Click);
            // 
            // ServingPath
            // 
            this.ServingPath.AutoSize = true;
            this.ServingPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.ServingPath.Location = new System.Drawing.Point(45, 266);
            this.ServingPath.Name = "ServingPath";
            this.ServingPath.Size = new System.Drawing.Size(125, 17);
            this.ServingPath.TabIndex = 7;
            this.ServingPath.Text = "Currently Serving: ";
            // 
            // PUT
            // 
            this.PUT.AutoSize = true;
            this.PUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.PUT.Location = new System.Drawing.Point(48, 368);
            this.PUT.Name = "PUT";
            this.PUT.Size = new System.Drawing.Size(148, 21);
            this.PUT.TabIndex = 8;
            this.PUT.Text = "Allow Put Requests";
            this.PUT.UseVisualStyleBackColor = true;
            this.PUT.CheckStateChanged += new System.EventHandler(this.PUT_CheckStateChanged);
            // 
            // DELETE
            // 
            this.DELETE.AutoSize = true;
            this.DELETE.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.DELETE.Location = new System.Drawing.Point(48, 395);
            this.DELETE.Name = "DELETE";
            this.DELETE.Size = new System.Drawing.Size(168, 21);
            this.DELETE.TabIndex = 9;
            this.DELETE.Text = "Allow Delete Requests";
            this.DELETE.UseVisualStyleBackColor = true;
            this.DELETE.CheckStateChanged += new System.EventHandler(this.DELETE_CheckStateChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(45, 617);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 17);
            this.label3.TabIndex = 10;
            this.label3.Text = "C Server Version 2.3";
            // 
            // viewOnGithub
            // 
            this.viewOnGithub.AutoSize = true;
            this.viewOnGithub.Cursor = System.Windows.Forms.Cursors.Hand;
            this.viewOnGithub.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.viewOnGithub.ForeColor = System.Drawing.SystemColors.Highlight;
            this.viewOnGithub.Location = new System.Drawing.Point(233, 617);
            this.viewOnGithub.Name = "viewOnGithub";
            this.viewOnGithub.Size = new System.Drawing.Size(105, 17);
            this.viewOnGithub.TabIndex = 11;
            this.viewOnGithub.Text = "View on GitHub";
            this.viewOnGithub.Click += new System.EventHandler(this.viewOnGithub_Click);
            // 
            // CORS
            // 
            this.CORS.AutoSize = true;
            this.CORS.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.CORS.Location = new System.Drawing.Point(48, 422);
            this.CORS.Name = "CORS";
            this.CORS.Size = new System.Drawing.Size(147, 21);
            this.CORS.TabIndex = 12;
            this.CORS.Text = "Set CORS headers";
            this.CORS.UseVisualStyleBackColor = true;
            this.CORS.CheckStateChanged += new System.EventHandler(this.CORS_CheckStateChanged);
            // 
            // AutoIndex
            // 
            this.AutoIndex.AutoSize = true;
            this.AutoIndex.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.AutoIndex.Location = new System.Drawing.Point(48, 449);
            this.AutoIndex.Name = "AutoIndex";
            this.AutoIndex.Size = new System.Drawing.Size(174, 21);
            this.AutoIndex.TabIndex = 13;
            this.AutoIndex.Text = "Auto Render index.html";
            this.AutoIndex.UseVisualStyleBackColor = true;
            this.AutoIndex.CheckStateChanged += new System.EventHandler(this.AutoIndex_CheckStateChanged);
            // 
            // URL
            // 
            this.URL.AutoSize = true;
            this.URL.Cursor = System.Windows.Forms.Cursors.Hand;
            this.URL.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.URL.ForeColor = System.Drawing.SystemColors.Highlight;
            this.URL.Location = new System.Drawing.Point(86, 227);
            this.URL.Name = "URL";
            this.URL.Size = new System.Drawing.Size(276, 17);
            this.URL.TabIndex = 14;
            this.URL.Text = "Open http://localhost:8080 in your browser";
            this.URL.Visible = false;
            this.URL.Click += new System.EventHandler(this.URL_Click);
            // 
            // MSG
            // 
            this.MSG.AutoSize = true;
            this.MSG.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MSG.Location = new System.Drawing.Point(83, 227);
            this.MSG.Name = "MSG";
            this.MSG.Size = new System.Drawing.Size(87, 17);
            this.MSG.TabIndex = 15;
            this.MSG.Text = "Not Running";
            // 
            // WebServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 661);
            this.Controls.Add(this.MSG);
            this.Controls.Add(this.URL);
            this.Controls.Add(this.AutoIndex);
            this.Controls.Add(this.CORS);
            this.Controls.Add(this.viewOnGithub);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.DELETE);
            this.Controls.Add(this.PUT);
            this.Controls.Add(this.ServingPath);
            this.Controls.Add(this.ChooseDirectory);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Status);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.MaximumSize = new System.Drawing.Size(410, 700);
            this.MinimumSize = new System.Drawing.Size(410, 700);
            this.Name = "WebServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple Web Server";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WebServer_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.Port)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label Status;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown Port;
        private System.Windows.Forms.Button ChooseDirectory;
        private System.Windows.Forms.Label ServingPath;
        private System.Windows.Forms.CheckBox PUT;
        private System.Windows.Forms.CheckBox DELETE;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label viewOnGithub;
        private System.Windows.Forms.CheckBox CORS;
        private System.Windows.Forms.CheckBox AutoIndex;
        private System.Windows.Forms.Label URL;
        private System.Windows.Forms.Label MSG;
    }
}

