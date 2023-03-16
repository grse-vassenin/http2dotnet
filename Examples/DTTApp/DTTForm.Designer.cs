namespace DTTApp
{
    partial class DTTForm
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
            this.log_txt = new System.Windows.Forms.TextBox();
            this.close_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // log_txt
            // 
            this.log_txt.Location = new System.Drawing.Point(12, 12);
            this.log_txt.Multiline = true;
            this.log_txt.Name = "log_txt";
            this.log_txt.Size = new System.Drawing.Size(419, 397);
            this.log_txt.TabIndex = 0;
            // 
            // close_btn
            // 
            this.close_btn.Location = new System.Drawing.Point(13, 415);
            this.close_btn.Name = "close_btn";
            this.close_btn.Size = new System.Drawing.Size(119, 23);
            this.close_btn.TabIndex = 1;
            this.close_btn.Text = "Close connection";
            this.close_btn.UseVisualStyleBackColor = true;
            this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
            // 
            // UplinkForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.close_btn);
            this.Controls.Add(this.log_txt);
            this.Name = "UplinkForm";
            this.Text = "UplinkForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox log_txt;
        private System.Windows.Forms.Button close_btn;
    }
}