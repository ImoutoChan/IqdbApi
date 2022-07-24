namespace IqdbManualTests
{
    partial class FrmManualTests
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
            this.txbUrl = new System.Windows.Forms.TextBox();
            this.lblURL = new System.Windows.Forms.Label();
            this.lblFile = new System.Windows.Forms.Label();
            this.txbFileName = new System.Windows.Forms.TextBox();
            this.btnSearchByUrl = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.btnSearchByFile = new System.Windows.Forms.Button();
            this.btnSearchByFile_3d = new System.Windows.Forms.Button();
            this.btnSearchByUrl_3d = new System.Windows.Forms.Button();
            this.txbSearchResults = new System.Windows.Forms.TextBox();
            this.picResult = new System.Windows.Forms.PictureBox();
            this.lblSearching = new System.Windows.Forms.Label();
            this.chkHttps = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).BeginInit();
            this.SuspendLayout();
            // 
            // txbUrl
            // 
            this.txbUrl.Location = new System.Drawing.Point(73, 32);
            this.txbUrl.Name = "txbUrl";
            this.txbUrl.Size = new System.Drawing.Size(366, 23);
            this.txbUrl.TabIndex = 0;
            // 
            // lblURL
            // 
            this.lblURL.AutoSize = true;
            this.lblURL.Location = new System.Drawing.Point(33, 35);
            this.lblURL.Name = "lblURL";
            this.lblURL.Size = new System.Drawing.Size(34, 17);
            this.lblURL.TabIndex = 1;
            this.lblURL.Text = "URL:";
            // 
            // lblFile
            // 
            this.lblFile.AutoSize = true;
            this.lblFile.Location = new System.Drawing.Point(33, 78);
            this.lblFile.Name = "lblFile";
            this.lblFile.Size = new System.Drawing.Size(30, 17);
            this.lblFile.TabIndex = 3;
            this.lblFile.Text = "File:";
            // 
            // txbFileName
            // 
            this.txbFileName.Location = new System.Drawing.Point(73, 75);
            this.txbFileName.Name = "txbFileName";
            this.txbFileName.Size = new System.Drawing.Size(366, 23);
            this.txbFileName.TabIndex = 2;
            // 
            // btnSearchByUrl
            // 
            this.btnSearchByUrl.Location = new System.Drawing.Point(545, 32);
            this.btnSearchByUrl.Name = "btnSearchByUrl";
            this.btnSearchByUrl.Size = new System.Drawing.Size(75, 23);
            this.btnSearchByUrl.TabIndex = 4;
            this.btnSearchByUrl.Text = "Search";
            this.btnSearchByUrl.UseVisualStyleBackColor = true;
            this.btnSearchByUrl.Click += new System.EventHandler(this.btnSearchByUrl_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(445, 75);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFile.TabIndex = 5;
            this.btnSelectFile.Text = "Select";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // btnSearchByFile
            // 
            this.btnSearchByFile.Location = new System.Drawing.Point(545, 75);
            this.btnSearchByFile.Name = "btnSearchByFile";
            this.btnSearchByFile.Size = new System.Drawing.Size(75, 23);
            this.btnSearchByFile.TabIndex = 6;
            this.btnSearchByFile.Text = "Search";
            this.btnSearchByFile.UseVisualStyleBackColor = true;
            this.btnSearchByFile.Click += new System.EventHandler(this.btnSearchByFile_Click);
            // 
            // btnSearchByFile_3d
            // 
            this.btnSearchByFile_3d.Location = new System.Drawing.Point(644, 75);
            this.btnSearchByFile_3d.Name = "btnSearchByFile_3d";
            this.btnSearchByFile_3d.Size = new System.Drawing.Size(75, 23);
            this.btnSearchByFile_3d.TabIndex = 8;
            this.btnSearchByFile_3d.Text = "Search3d";
            this.btnSearchByFile_3d.UseVisualStyleBackColor = true;
            this.btnSearchByFile_3d.Click += new System.EventHandler(this.btnSearchByFile_3d_Click);
            // 
            // btnSearchByUrl_3d
            // 
            this.btnSearchByUrl_3d.Location = new System.Drawing.Point(644, 32);
            this.btnSearchByUrl_3d.Name = "btnSearchByUrl_3d";
            this.btnSearchByUrl_3d.Size = new System.Drawing.Size(75, 23);
            this.btnSearchByUrl_3d.TabIndex = 7;
            this.btnSearchByUrl_3d.Text = "Search3d";
            this.btnSearchByUrl_3d.UseVisualStyleBackColor = true;
            this.btnSearchByUrl_3d.Click += new System.EventHandler(this.btnSearchByUrl_3d_Click);
            // 
            // txbSearchResults
            // 
            this.txbSearchResults.Location = new System.Drawing.Point(73, 125);
            this.txbSearchResults.Multiline = true;
            this.txbSearchResults.Name = "txbSearchResults";
            this.txbSearchResults.Size = new System.Drawing.Size(447, 301);
            this.txbSearchResults.TabIndex = 9;
            // 
            // picResult
            // 
            this.picResult.Location = new System.Drawing.Point(545, 125);
            this.picResult.Name = "picResult";
            this.picResult.Size = new System.Drawing.Size(174, 301);
            this.picResult.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picResult.TabIndex = 10;
            this.picResult.TabStop = false;
            // 
            // lblSearching
            // 
            this.lblSearching.AutoSize = true;
            this.lblSearching.Location = new System.Drawing.Point(545, 105);
            this.lblSearching.Name = "lblSearching";
            this.lblSearching.Size = new System.Drawing.Size(0, 17);
            this.lblSearching.TabIndex = 11;
            this.lblSearching.Visible = false;
            // 
            // chkHttps
            // 
            this.chkHttps.AutoSize = true;
            this.chkHttps.Checked = true;
            this.chkHttps.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHttps.Location = new System.Drawing.Point(452, 34);
            this.chkHttps.Name = "chkHttps";
            this.chkHttps.Size = new System.Drawing.Size(58, 21);
            this.chkHttps.TabIndex = 12;
            this.chkHttps.Text = "Https";
            this.chkHttps.UseVisualStyleBackColor = true;
            // 
            // FrmManualTests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 450);
            this.Controls.Add(this.chkHttps);
            this.Controls.Add(this.lblSearching);
            this.Controls.Add(this.picResult);
            this.Controls.Add(this.txbSearchResults);
            this.Controls.Add(this.btnSearchByFile_3d);
            this.Controls.Add(this.btnSearchByUrl_3d);
            this.Controls.Add(this.btnSearchByFile);
            this.Controls.Add(this.btnSelectFile);
            this.Controls.Add(this.btnSearchByUrl);
            this.Controls.Add(this.lblFile);
            this.Controls.Add(this.txbFileName);
            this.Controls.Add(this.lblURL);
            this.Controls.Add(this.txbUrl);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmManualTests";
            this.Text = "Iqdb Manual Tests";
            ((System.ComponentModel.ISupportInitialize)(this.picResult)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox txbUrl;
        private Label lblURL;
        private Label lblFile;
        private TextBox txbFileName;
        private Button btnSearchByUrl;
        private Button btnSelectFile;
        private Button btnSearchByFile;
        private Button btnSearchByFile_3d;
        private Button btnSearchByUrl_3d;
        private TextBox txbSearchResults;
        private PictureBox picResult;
        private Label lblSearching;
        private CheckBox chkHttps;
    }
}