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
            ((System.ComponentModel.ISupportInitialize)(this.Port)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(41, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(308, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "Simple Web Server";
            this.label1.Click += new System.EventHandler(this.label1_Click);
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
            this.label2.Location = new System.Drawing.Point(86, 199);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 17);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port:";
            // 
            // Port
            // 
            this.Port.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Port.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Port.Location = new System.Drawing.Point(130, 197);
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
            this.ChooseDirectory.Location = new System.Drawing.Point(113, 279);
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
            this.ServingPath.Location = new System.Drawing.Point(62, 243);
            this.ServingPath.Name = "ServingPath";
            this.ServingPath.Size = new System.Drawing.Size(125, 17);
            this.ServingPath.TabIndex = 7;
            this.ServingPath.Text = "Currently Serving: ";
            // 
            // PUT
            // 
            this.PUT.AutoSize = true;
            this.PUT.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.PUT.Location = new System.Drawing.Point(48, 344);
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
            this.DELETE.Location = new System.Drawing.Point(48, 372);
            this.DELETE.Name = "DELETE";
            this.DELETE.Size = new System.Drawing.Size(168, 21);
            this.DELETE.TabIndex = 9;
            this.DELETE.Text = "Allow Delete Requests";
            this.DELETE.UseVisualStyleBackColor = true;
            this.DELETE.CheckStateChanged += new System.EventHandler(this.DELETE_CheckStateChanged);
            // 
            // WebServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 661);
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
    }
}

