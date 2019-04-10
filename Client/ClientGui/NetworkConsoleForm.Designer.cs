namespace ClientGui
{
    partial class NetworkConsoleForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.sendButton = new System.Windows.Forms.Button();
            this.autoScrollCheckbox = new System.Windows.Forms.CheckBox();
            this.newLineCheckbox = new System.Windows.Forms.CheckBox();
            this.inOutCheckbox = new System.Windows.Forms.CheckBox();
            this.showLineCheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.AcceptsReturn = true;
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(12, 58);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(800, 453);
            this.textBox1.TabIndex = 0;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // textBox2
            // 
            this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox2.Location = new System.Drawing.Point(12, 12);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(647, 31);
            this.textBox2.TabIndex = 1;
            // 
            // sendButton
            // 
            this.sendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sendButton.Enabled = false;
            this.sendButton.Location = new System.Drawing.Point(677, 10);
            this.sendButton.Name = "sendButton";
            this.sendButton.Size = new System.Drawing.Size(135, 42);
            this.sendButton.TabIndex = 2;
            this.sendButton.Text = "Send";
            this.sendButton.UseVisualStyleBackColor = true;
            this.sendButton.Click += new System.EventHandler(this.sendButton_Click);
            // 
            // autoScrollCheckbox
            // 
            this.autoScrollCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autoScrollCheckbox.AutoSize = true;
            this.autoScrollCheckbox.Checked = true;
            this.autoScrollCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoScrollCheckbox.Location = new System.Drawing.Point(12, 517);
            this.autoScrollCheckbox.Name = "autoScrollCheckbox";
            this.autoScrollCheckbox.Size = new System.Drawing.Size(148, 29);
            this.autoScrollCheckbox.TabIndex = 3;
            this.autoScrollCheckbox.Text = "Auto Scroll";
            this.autoScrollCheckbox.UseVisualStyleBackColor = true;
            // 
            // newLineCheckbox
            // 
            this.newLineCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.newLineCheckbox.AutoSize = true;
            this.newLineCheckbox.Checked = true;
            this.newLineCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.newLineCheckbox.Location = new System.Drawing.Point(166, 517);
            this.newLineCheckbox.Name = "newLineCheckbox";
            this.newLineCheckbox.Size = new System.Drawing.Size(185, 29);
            this.newLineCheckbox.TabIndex = 4;
            this.newLineCheckbox.Text = "Send \"\\n\" as \\n";
            this.newLineCheckbox.UseVisualStyleBackColor = true;
            // 
            // inOutCheckbox
            // 
            this.inOutCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.inOutCheckbox.AutoSize = true;
            this.inOutCheckbox.Checked = true;
            this.inOutCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.inOutCheckbox.Location = new System.Drawing.Point(378, 517);
            this.inOutCheckbox.Name = "inOutCheckbox";
            this.inOutCheckbox.Size = new System.Drawing.Size(220, 29);
            this.inOutCheckbox.TabIndex = 5;
            this.inOutCheckbox.Text = "Show in/out (->/<-)";
            this.inOutCheckbox.UseVisualStyleBackColor = true;
            // 
            // showLineCheckbox
            // 
            this.showLineCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.showLineCheckbox.AutoSize = true;
            this.showLineCheckbox.Checked = true;
            this.showLineCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showLineCheckbox.Location = new System.Drawing.Point(609, 517);
            this.showLineCheckbox.Name = "showLineCheckbox";
            this.showLineCheckbox.Size = new System.Drawing.Size(187, 29);
            this.showLineCheckbox.TabIndex = 6;
            this.showLineCheckbox.Text = "Show \\n as \\r\\n";
            this.showLineCheckbox.UseVisualStyleBackColor = true;
            // 
            // NetworkConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 558);
            this.Controls.Add(this.showLineCheckbox);
            this.Controls.Add(this.inOutCheckbox);
            this.Controls.Add(this.newLineCheckbox);
            this.Controls.Add(this.autoScrollCheckbox);
            this.Controls.Add(this.sendButton);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Name = "NetworkConsoleForm";
            this.Text = "Network Console";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button sendButton;
        private System.Windows.Forms.CheckBox autoScrollCheckbox;
        private System.Windows.Forms.CheckBox newLineCheckbox;
        private System.Windows.Forms.CheckBox inOutCheckbox;
        private System.Windows.Forms.CheckBox showLineCheckbox;
    }
}