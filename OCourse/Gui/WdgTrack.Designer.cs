namespace OCourse.Gui
{
  partial class WdgTrack
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
      this.lblPath = new System.Windows.Forms.Label();
      this.txtPath = new System.Windows.Forms.TextBox();
      this.btnPath = new System.Windows.Forms.Button();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.txtCourse = new System.Windows.Forms.TextBox();
      this.lblOcad = new System.Windows.Forms.Label();
      this.lblResult = new System.Windows.Forms.Label();
      this.txtResult = new System.Windows.Forms.TextBox();
      this.btnResult = new System.Windows.Forms.Button();
      this.lblNormalized = new System.Windows.Forms.Label();
      this.txtNormalized = new System.Windows.Forms.TextBox();
      this.btnNormalized = new System.Windows.Forms.Button();
      this.btnCreateNormalized = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.chkGps = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.lstCourse = new System.Windows.Forms.ComboBox();
      this.chkChoice = new System.Windows.Forms.CheckBox();
      this.btnRearrange = new System.Windows.Forms.CheckBox();
      this.btnRg = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // lblPath
      // 
      this.lblPath.AutoSize = true;
      this.lblPath.Location = new System.Drawing.Point(27, 222);
      this.lblPath.Name = "lblPath";
      this.lblPath.Size = new System.Drawing.Size(42, 13);
      this.lblPath.TabIndex = 0;
      this.lblPath.Text = "Pathfile";
      // 
      // txtPath
      // 
      this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtPath.Location = new System.Drawing.Point(86, 219);
      this.txtPath.Name = "txtPath";
      this.txtPath.ReadOnly = true;
      this.txtPath.Size = new System.Drawing.Size(508, 20);
      this.txtPath.TabIndex = 1;
      // 
      // btnPath
      // 
      this.btnPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPath.Location = new System.Drawing.Point(600, 217);
      this.btnPath.Name = "btnPath";
      this.btnPath.Size = new System.Drawing.Size(24, 23);
      this.btnPath.TabIndex = 2;
      this.btnPath.Text = "...";
      this.btnPath.UseVisualStyleBackColor = true;
      this.btnPath.Click += new System.EventHandler(this.btnPath_Click);
      // 
      // dlgOpen
      // 
      this.dlgOpen.Filter = "GPS Files|*.gpx;*.tcx|All files|*.*";
      // 
      // txtCourse
      // 
      this.txtCourse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCourse.Location = new System.Drawing.Point(111, 6);
      this.txtCourse.Name = "txtCourse";
      this.txtCourse.ReadOnly = true;
      this.txtCourse.Size = new System.Drawing.Size(513, 20);
      this.txtCourse.TabIndex = 14;
      // 
      // lblOcad
      // 
      this.lblOcad.AutoSize = true;
      this.lblOcad.Location = new System.Drawing.Point(12, 9);
      this.lblOcad.Name = "lblOcad";
      this.lblOcad.Size = new System.Drawing.Size(70, 13);
      this.lblOcad.TabIndex = 13;
      this.lblOcad.Text = "Course (.ocd)";
      // 
      // lblResult
      // 
      this.lblResult.AutoSize = true;
      this.lblResult.Location = new System.Drawing.Point(9, 69);
      this.lblResult.Name = "lblResult";
      this.lblResult.Size = new System.Drawing.Size(66, 13);
      this.lblResult.TabIndex = 22;
      this.lblResult.Text = "Result (.csv)";
      // 
      // txtResult
      // 
      this.txtResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtResult.Location = new System.Drawing.Point(108, 66);
      this.txtResult.Name = "txtResult";
      this.txtResult.Size = new System.Drawing.Size(513, 20);
      this.txtResult.TabIndex = 21;
      // 
      // btnResult
      // 
      this.btnResult.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnResult.Location = new System.Drawing.Point(627, 64);
      this.btnResult.Name = "btnResult";
      this.btnResult.Size = new System.Drawing.Size(25, 23);
      this.btnResult.TabIndex = 20;
      this.btnResult.Text = "...";
      this.btnResult.UseVisualStyleBackColor = true;
      this.btnResult.Click += new System.EventHandler(this.btnResult_Click);
      // 
      // lblNormalized
      // 
      this.lblNormalized.AutoSize = true;
      this.lblNormalized.Location = new System.Drawing.Point(9, 125);
      this.lblNormalized.Name = "lblNormalized";
      this.lblNormalized.Size = new System.Drawing.Size(80, 13);
      this.lblNormalized.TabIndex = 25;
      this.lblNormalized.Text = "Complete (.csv)";
      // 
      // txtNormalized
      // 
      this.txtNormalized.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtNormalized.Location = new System.Drawing.Point(108, 122);
      this.txtNormalized.Name = "txtNormalized";
      this.txtNormalized.Size = new System.Drawing.Size(513, 20);
      this.txtNormalized.TabIndex = 24;
      // 
      // btnNormalized
      // 
      this.btnNormalized.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnNormalized.Location = new System.Drawing.Point(627, 120);
      this.btnNormalized.Name = "btnNormalized";
      this.btnNormalized.Size = new System.Drawing.Size(25, 23);
      this.btnNormalized.TabIndex = 23;
      this.btnNormalized.Text = "...";
      this.btnNormalized.UseVisualStyleBackColor = true;
      this.btnNormalized.Click += new System.EventHandler(this.btnNormalized_Click);
      // 
      // btnCreateNormalized
      // 
      this.btnCreateNormalized.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCreateNormalized.Location = new System.Drawing.Point(108, 171);
      this.btnCreateNormalized.Name = "btnCreateNormalized";
      this.btnCreateNormalized.Size = new System.Drawing.Size(150, 23);
      this.btnCreateNormalized.TabIndex = 26;
      this.btnCreateNormalized.Text = "Create Result";
      this.btnCreateNormalized.UseVisualStyleBackColor = true;
      this.btnCreateNormalized.Click += new System.EventHandler(this.btnCreateNormalized_Click);
      // 
      // chkGps
      // 
      this.chkGps.AutoSize = true;
      this.chkGps.Location = new System.Drawing.Point(299, 148);
      this.chkGps.Name = "chkGps";
      this.chkGps.Size = new System.Drawing.Size(109, 17);
      this.chkGps.TabIndex = 27;
      this.chkGps.Text = "Create GPS track";
      this.chkGps.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 35);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(40, 13);
      this.label4.TabIndex = 30;
      this.label4.Text = "Course";
      // 
      // lstCourse
      // 
      this.lstCourse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.lstCourse.FormattingEnabled = true;
      this.lstCourse.Location = new System.Drawing.Point(111, 32);
      this.lstCourse.Name = "lstCourse";
      this.lstCourse.Size = new System.Drawing.Size(110, 21);
      this.lstCourse.TabIndex = 29;
      // 
      // chkChoice
      // 
      this.chkChoice.AutoSize = true;
      this.chkChoice.Location = new System.Drawing.Point(111, 148);
      this.chkChoice.Name = "chkChoice";
      this.chkChoice.Size = new System.Drawing.Size(164, 17);
      this.chkChoice.TabIndex = 31;
      this.chkChoice.Text = "Create Route Finder\'s Choice";
      this.chkChoice.UseVisualStyleBackColor = true;
      // 
      // btnRearrange
      // 
      this.btnRearrange.AutoSize = true;
      this.btnRearrange.Location = new System.Drawing.Point(414, 148);
      this.btnRearrange.Name = "btnRearrange";
      this.btnRearrange.Size = new System.Drawing.Size(71, 17);
      this.btnRearrange.TabIndex = 32;
      this.btnRearrange.Text = "rearrange";
      this.btnRearrange.UseVisualStyleBackColor = true;
      // 
      // btnRg
      // 
      this.btnRg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRg.Location = new System.Drawing.Point(299, 171);
      this.btnRg.Name = "btnRg";
      this.btnRg.Size = new System.Drawing.Size(150, 23);
      this.btnRg.TabIndex = 33;
      this.btnRg.Text = "To RouteGadget";
      this.btnRg.UseVisualStyleBackColor = true;
      this.btnRg.Click += new System.EventHandler(this.btnRg_Click);
      // 
      // WdgTrack
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(660, 262);
      this.Controls.Add(this.btnRg);
      this.Controls.Add(this.btnRearrange);
      this.Controls.Add(this.chkChoice);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.lstCourse);
      this.Controls.Add(this.chkGps);
      this.Controls.Add(this.btnCreateNormalized);
      this.Controls.Add(this.lblNormalized);
      this.Controls.Add(this.txtNormalized);
      this.Controls.Add(this.btnNormalized);
      this.Controls.Add(this.lblResult);
      this.Controls.Add(this.txtResult);
      this.Controls.Add(this.btnResult);
      this.Controls.Add(this.txtCourse);
      this.Controls.Add(this.lblOcad);
      this.Controls.Add(this.btnPath);
      this.Controls.Add(this.txtPath);
      this.Controls.Add(this.lblPath);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgTrack";
      this.Text = "Path";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label lblPath;
    private System.Windows.Forms.TextBox txtPath;
    private System.Windows.Forms.Button btnPath;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.TextBox txtCourse;
    private System.Windows.Forms.Label lblOcad;
    private System.Windows.Forms.Label lblResult;
    private System.Windows.Forms.TextBox txtResult;
    private System.Windows.Forms.Button btnResult;
    private System.Windows.Forms.Label lblNormalized;
    private System.Windows.Forms.TextBox txtNormalized;
    private System.Windows.Forms.Button btnNormalized;
    private System.Windows.Forms.Button btnCreateNormalized;
    private System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.CheckBox chkGps;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox lstCourse;
    private System.Windows.Forms.CheckBox chkChoice;
    private System.Windows.Forms.CheckBox btnRearrange;
    private System.Windows.Forms.Button btnRg;
  }
}