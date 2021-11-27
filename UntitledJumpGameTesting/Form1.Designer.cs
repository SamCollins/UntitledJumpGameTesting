
namespace UntitledJumpGameTesting
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GenerateButton = new System.Windows.Forms.Button();
            this.NumSidesTextbox = new System.Windows.Forms.TextBox();
            this.NumSidesLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GenerateButton
            // 
            this.GenerateButton.Location = new System.Drawing.Point(676, 12);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(94, 29);
            this.GenerateButton.TabIndex = 0;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // NumSidesTextbox
            // 
            this.NumSidesTextbox.Location = new System.Drawing.Point(98, 6);
            this.NumSidesTextbox.Name = "NumSidesTextbox";
            this.NumSidesTextbox.Size = new System.Drawing.Size(58, 27);
            this.NumSidesTextbox.TabIndex = 1;
            this.NumSidesTextbox.TextChanged += new System.EventHandler(this.NumSidesTextbox_TextChanged);
            // 
            // NumSidesLabel
            // 
            this.NumSidesLabel.AutoSize = true;
            this.NumSidesLabel.Location = new System.Drawing.Point(12, 9);
            this.NumSidesLabel.Name = "NumSidesLabel";
            this.NumSidesLabel.Size = new System.Drawing.Size(80, 20);
            this.NumSidesLabel.TabIndex = 2;
            this.NumSidesLabel.Text = "Num Sides";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 753);
            this.Controls.Add(this.NumSidesLabel);
            this.Controls.Add(this.NumSidesTextbox);
            this.Controls.Add(this.GenerateButton);
            this.Name = "Form1";
            this.Text = "Platform Layout";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.TextBox NumSidesTextbox;
        private System.Windows.Forms.Label NumSidesLabel;
    }
}

