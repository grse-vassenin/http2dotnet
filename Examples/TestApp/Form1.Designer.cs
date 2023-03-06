namespace TestApp
{
    partial class UplinkForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UplinkForm));
            this.runServer_btn = new System.Windows.Forms.Button();
            this.stopServer_btn = new System.Windows.Forms.Button();
            this.requestBody_txt = new System.Windows.Forms.TextBox();
            this.replyBody_txt = new System.Windows.Forms.TextBox();
            this.sendRequest_btn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.requestPath_txt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // runServer_btn
            // 
            this.runServer_btn.Location = new System.Drawing.Point(12, 12);
            this.runServer_btn.Name = "runServer_btn";
            this.runServer_btn.Size = new System.Drawing.Size(75, 23);
            this.runServer_btn.TabIndex = 0;
            this.runServer_btn.Text = "Run Uplik Server";
            this.runServer_btn.UseVisualStyleBackColor = true;
            this.runServer_btn.Click += new System.EventHandler(this.runServer_btn_Click);
            // 
            // stopServer_btn
            // 
            this.stopServer_btn.Enabled = false;
            this.stopServer_btn.Location = new System.Drawing.Point(12, 70);
            this.stopServer_btn.Name = "stopServer_btn";
            this.stopServer_btn.Size = new System.Drawing.Size(75, 23);
            this.stopServer_btn.TabIndex = 1;
            this.stopServer_btn.Text = "Stop Uplink";
            this.stopServer_btn.UseVisualStyleBackColor = true;
            this.stopServer_btn.Click += new System.EventHandler(this.stopServer_btn_Click);
            // 
            // requestBody_txt
            // 
            this.requestBody_txt.Location = new System.Drawing.Point(93, 38);
            this.requestBody_txt.Multiline = true;
            this.requestBody_txt.Name = "requestBody_txt";
            this.requestBody_txt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.requestBody_txt.Size = new System.Drawing.Size(354, 271);
            this.requestBody_txt.TabIndex = 2;
            this.requestBody_txt.Text = resources.GetString("requestBody_txt.Text");
            this.requestBody_txt.WordWrap = false;
            // 
            // replyBody_txt
            // 
            this.replyBody_txt.Location = new System.Drawing.Point(453, 38);
            this.replyBody_txt.Multiline = true;
            this.replyBody_txt.Name = "replyBody_txt";
            this.replyBody_txt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.replyBody_txt.Size = new System.Drawing.Size(351, 271);
            this.replyBody_txt.TabIndex = 3;
            // 
            // sendRequest_btn
            // 
            this.sendRequest_btn.Enabled = false;
            this.sendRequest_btn.Location = new System.Drawing.Point(12, 41);
            this.sendRequest_btn.Name = "sendRequest_btn";
            this.sendRequest_btn.Size = new System.Drawing.Size(75, 23);
            this.sendRequest_btn.TabIndex = 4;
            this.sendRequest_btn.Text = "Send";
            this.sendRequest_btn.UseVisualStyleBackColor = true;
            this.sendRequest_btn.Click += new System.EventHandler(this.sendRequest_btn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(93, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Path:";
            // 
            // requestPath_txt
            // 
            this.requestPath_txt.Location = new System.Drawing.Point(131, 12);
            this.requestPath_txt.Name = "requestPath_txt";
            this.requestPath_txt.Size = new System.Drawing.Size(673, 20);
            this.requestPath_txt.TabIndex = 6;
            this.requestPath_txt.Text = "/onvif/uplink_service";
            // 
            // UplinkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 321);
            this.Controls.Add(this.requestPath_txt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.sendRequest_btn);
            this.Controls.Add(this.replyBody_txt);
            this.Controls.Add(this.requestBody_txt);
            this.Controls.Add(this.stopServer_btn);
            this.Controls.Add(this.runServer_btn);
            this.Name = "UplinkForm";
            this.Text = "Uplink test";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button runServer_btn;
        private System.Windows.Forms.Button stopServer_btn;
        private System.Windows.Forms.TextBox requestBody_txt;
        private System.Windows.Forms.TextBox replyBody_txt;
        private System.Windows.Forms.Button sendRequest_btn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox requestPath_txt;
    }
}

