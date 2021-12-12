
namespace UntitledJumpGameTesting
{
    partial class MainForm
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
            this.TimeDisplay = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.PlatMinRadLabel = new System.Windows.Forms.Label();
            this.PlatMinRadTextbox = new System.Windows.Forms.TextBox();
            this.PlatMaxRadLabel = new System.Windows.Forms.Label();
            this.PlatMaxRadTextbox = new System.Windows.Forms.TextBox();
            this.ToggleDebugButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // GenerateButton
            // 
            this.GenerateButton.Location = new System.Drawing.Point(676, 9);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(100, 30);
            this.GenerateButton.TabIndex = 0;
            this.GenerateButton.Text = "Generate";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // NumSidesTextbox
            // 
            this.NumSidesTextbox.Location = new System.Drawing.Point(98, 6);
            this.NumSidesTextbox.Name = "NumSidesTextbox";
            this.NumSidesTextbox.Size = new System.Drawing.Size(50, 27);
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
            // TimeDisplay
            // 
            this.TimeDisplay.AutoSize = true;
            this.TimeDisplay.Location = new System.Drawing.Point(12, 36);
            this.TimeDisplay.Name = "TimeDisplay";
            this.TimeDisplay.Size = new System.Drawing.Size(103, 20);
            this.TimeDisplay.TabIndex = 4;
            this.TimeDisplay.Text = "Time: 00:00.00";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(676, 45);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(100, 30);
            this.StartButton.TabIndex = 5;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // PlatMinRadLabel
            // 
            this.PlatMinRadLabel.AutoSize = true;
            this.PlatMinRadLabel.Location = new System.Drawing.Point(170, 9);
            this.PlatMinRadLabel.Name = "PlatMinRadLabel";
            this.PlatMinRadLabel.Size = new System.Drawing.Size(111, 20);
            this.PlatMinRadLabel.TabIndex = 6;
            this.PlatMinRadLabel.Text = "Plat Min Radius";
            // 
            // PlatMinRadTextbox
            // 
            this.PlatMinRadTextbox.Location = new System.Drawing.Point(288, 6);
            this.PlatMinRadTextbox.Name = "PlatMinRadTextbox";
            this.PlatMinRadTextbox.Size = new System.Drawing.Size(50, 27);
            this.PlatMinRadTextbox.TabIndex = 7;
            this.PlatMinRadTextbox.TextChanged += new System.EventHandler(this.PlatMinRadTextbox_TextChanged);
            // 
            // PlatMaxRadLabel
            // 
            this.PlatMaxRadLabel.AutoSize = true;
            this.PlatMaxRadLabel.Location = new System.Drawing.Point(371, 9);
            this.PlatMaxRadLabel.Name = "PlatMaxRadLabel";
            this.PlatMaxRadLabel.Size = new System.Drawing.Size(114, 20);
            this.PlatMaxRadLabel.TabIndex = 8;
            this.PlatMaxRadLabel.Text = "Plat Max Radius";
            // 
            // PlatMaxRadTextbox
            // 
            this.PlatMaxRadTextbox.Location = new System.Drawing.Point(492, 6);
            this.PlatMaxRadTextbox.Name = "PlatMaxRadTextbox";
            this.PlatMaxRadTextbox.Size = new System.Drawing.Size(50, 27);
            this.PlatMaxRadTextbox.TabIndex = 9;
            this.PlatMaxRadTextbox.TextChanged += new System.EventHandler(this.PlatMaxRadTextbox_TextChanged);
            // 
            // ToggleDebugButton
            // 
            this.ToggleDebugButton.Location = new System.Drawing.Point(626, 81);
            this.ToggleDebugButton.Name = "ToggleDebugButton";
            this.ToggleDebugButton.Size = new System.Drawing.Size(150, 30);
            this.ToggleDebugButton.TabIndex = 10;
            this.ToggleDebugButton.Text = "Debugging: Off";
            this.ToggleDebugButton.UseVisualStyleBackColor = true;
            this.ToggleDebugButton.Click += new System.EventHandler(this.ToggleDebugButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 753);
            this.Controls.Add(this.ToggleDebugButton);
            this.Controls.Add(this.PlatMaxRadTextbox);
            this.Controls.Add(this.PlatMaxRadLabel);
            this.Controls.Add(this.PlatMinRadTextbox);
            this.Controls.Add(this.PlatMinRadLabel);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.TimeDisplay);
            this.Controls.Add(this.NumSidesLabel);
            this.Controls.Add(this.NumSidesTextbox);
            this.Controls.Add(this.GenerateButton);
            this.Name = "MainForm";
            this.Text = "Platform Layout";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button GenerateButton;
        private System.Windows.Forms.TextBox NumSidesTextbox;
        private System.Windows.Forms.Label NumSidesLabel;
        private System.Windows.Forms.Label TimeDisplay;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Label PlatMinRadLabel;
        private System.Windows.Forms.TextBox PlatMinRadTextbox;
        private System.Windows.Forms.Label PlatMaxRadLabel;
        private System.Windows.Forms.TextBox PlatMaxRadTextbox;
        private System.Windows.Forms.Button ToggleDebugButton;
    }
}

