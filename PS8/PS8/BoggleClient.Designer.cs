namespace PS8
{
    partial class BoggleClient
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
            this.domainNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.domainNameLabel = new System.Windows.Forms.Label();
            this.playerNameTextBox = new System.Windows.Forms.TextBox();
            this.playerNameLabel = new System.Windows.Forms.Label();
            this.registerButton = new System.Windows.Forms.Button();
            this.joinGameButton = new System.Windows.Forms.Button();
            this.gameDurationLabel = new System.Windows.Forms.Label();
            this.gameDurationTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // domainNameTextBox
            // 
            this.domainNameTextBox.Location = new System.Drawing.Point(174, 63);
            this.domainNameTextBox.Name = "domainNameTextBox";
            this.domainNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.domainNameTextBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Ubuntu", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(146, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 27);
            this.label1.TabIndex = 1;
            this.label1.Text = "Play Boggle";
            // 
            // domainNameLabel
            // 
            this.domainNameLabel.AutoSize = true;
            this.domainNameLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.domainNameLabel.Location = new System.Drawing.Point(40, 62);
            this.domainNameLabel.Name = "domainNameLabel";
            this.domainNameLabel.Size = new System.Drawing.Size(113, 21);
            this.domainNameLabel.TabIndex = 2;
            this.domainNameLabel.Text = "Domain name";
            // 
            // playerNameTextBox
            // 
            this.playerNameTextBox.Location = new System.Drawing.Point(174, 107);
            this.playerNameTextBox.Name = "playerNameTextBox";
            this.playerNameTextBox.Size = new System.Drawing.Size(212, 20);
            this.playerNameTextBox.TabIndex = 3;
            // 
            // playerNameLabel
            // 
            this.playerNameLabel.AutoSize = true;
            this.playerNameLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playerNameLabel.Location = new System.Drawing.Point(40, 106);
            this.playerNameLabel.Name = "playerNameLabel";
            this.playerNameLabel.Size = new System.Drawing.Size(102, 21);
            this.playerNameLabel.TabIndex = 4;
            this.playerNameLabel.Text = "Player name";
            // 
            // registerButton
            // 
            this.registerButton.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.registerButton.Location = new System.Drawing.Point(174, 147);
            this.registerButton.Name = "registerButton";
            this.registerButton.Size = new System.Drawing.Size(80, 33);
            this.registerButton.TabIndex = 5;
            this.registerButton.Text = "Register";
            this.registerButton.UseVisualStyleBackColor = true;
            this.registerButton.Click += new System.EventHandler(this.registerButton_Click);
            // 
            // joinGameButton
            // 
            this.joinGameButton.Enabled = false;
            this.joinGameButton.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.joinGameButton.Location = new System.Drawing.Point(174, 256);
            this.joinGameButton.Name = "joinGameButton";
            this.joinGameButton.Size = new System.Drawing.Size(101, 52);
            this.joinGameButton.TabIndex = 6;
            this.joinGameButton.Text = "Join Game";
            this.joinGameButton.UseVisualStyleBackColor = true;
            // 
            // gameDurationLabel
            // 
            this.gameDurationLabel.AutoSize = true;
            this.gameDurationLabel.Enabled = false;
            this.gameDurationLabel.Font = new System.Drawing.Font("Ubuntu", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameDurationLabel.Location = new System.Drawing.Point(40, 204);
            this.gameDurationLabel.Name = "gameDurationLabel";
            this.gameDurationLabel.Size = new System.Drawing.Size(120, 21);
            this.gameDurationLabel.TabIndex = 8;
            this.gameDurationLabel.Text = "Game duration";
            // 
            // gameDurationTextBox
            // 
            this.gameDurationTextBox.Enabled = false;
            this.gameDurationTextBox.Location = new System.Drawing.Point(174, 205);
            this.gameDurationTextBox.Name = "gameDurationTextBox";
            this.gameDurationTextBox.Size = new System.Drawing.Size(212, 20);
            this.gameDurationTextBox.TabIndex = 7;
            // 
            // BoggleClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 348);
            this.Controls.Add(this.gameDurationLabel);
            this.Controls.Add(this.gameDurationTextBox);
            this.Controls.Add(this.joinGameButton);
            this.Controls.Add(this.registerButton);
            this.Controls.Add(this.playerNameLabel);
            this.Controls.Add(this.playerNameTextBox);
            this.Controls.Add(this.domainNameLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.domainNameTextBox);
            this.Name = "BoggleClient";
            this.Text = "Boggle Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox domainNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label domainNameLabel;
        private System.Windows.Forms.TextBox playerNameTextBox;
        private System.Windows.Forms.Label playerNameLabel;
        private System.Windows.Forms.Button registerButton;
        private System.Windows.Forms.Button joinGameButton;
        private System.Windows.Forms.Label gameDurationLabel;
        private System.Windows.Forms.TextBox gameDurationTextBox;
    }
}

