namespace OCourse.Gui
{
  partial class WdgPermutations
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
      this.components = new System.ComponentModel.Container();
      this.txtCourse = new System.Windows.Forms.TextBox();
      this.lblOcad = new System.Windows.Forms.Label();
      this.btnCreateRaw = new System.Windows.Forms.Button();
      this.lblRawParent = new System.Windows.Forms.Label();
      this.txtRawParent = new System.Windows.Forms.TextBox();
      this.btnRawParent = new System.Windows.Forms.Button();
      this.lblRawDir = new System.Windows.Forms.Label();
      this.txtRawDir = new System.Windows.Forms.TextBox();
      this.btnRawDir = new System.Windows.Forms.Button();
      this.tt = new System.Windows.Forms.ToolTip(this.components);
      this.lblClean = new System.Windows.Forms.Label();
      this.txtClean = new System.Windows.Forms.TextBox();
      this.btnVariations = new System.Windows.Forms.Button();
      this.lblCategory = new System.Windows.Forms.Label();
      this.txtCategory = new System.Windows.Forms.TextBox();
      this.btnCategory = new System.Windows.Forms.Button();
      this.txtCleanTemplates = new System.Windows.Forms.TextBox();
      this.btnCleanTemplates = new System.Windows.Forms.Button();
      this.lblCleanTemplates = new System.Windows.Forms.Label();
      this.btnCreateClean = new System.Windows.Forms.Button();
      this.label4 = new System.Windows.Forms.Label();
      this.lstCourse = new System.Windows.Forms.ComboBox();
      this.btnCreateCat = new System.Windows.Forms.Button();
      this.chkConstant = new System.Windows.Forms.CheckBox();
      this.txtRunners = new System.Windows.Forms.TextBox();
      this.chkNr = new System.Windows.Forms.CheckBox();
      this.chkCodes = new System.Windows.Forms.CheckBox();
      this.btnBahnexport = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.SuspendLayout();
      // 
      // txtCourse
      // 
      this.txtCourse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCourse.Location = new System.Drawing.Point(111, 6);
      this.txtCourse.Name = "txtCourse";
      this.txtCourse.ReadOnly = true;
      this.txtCourse.Size = new System.Drawing.Size(513, 20);
      this.txtCourse.TabIndex = 12;
      // 
      // lblOcad
      // 
      this.lblOcad.AutoSize = true;
      this.lblOcad.Location = new System.Drawing.Point(12, 9);
      this.lblOcad.Name = "lblOcad";
      this.lblOcad.Size = new System.Drawing.Size(70, 13);
      this.lblOcad.TabIndex = 11;
      this.lblOcad.Text = "Course (.ocd)";
      // 
      // btnCreateRaw
      // 
      this.btnCreateRaw.Location = new System.Drawing.Point(111, 62);
      this.btnCreateRaw.Name = "btnCreateRaw";
      this.btnCreateRaw.Size = new System.Drawing.Size(110, 23);
      this.btnCreateRaw.TabIndex = 16;
      this.btnCreateRaw.Text = "Create Raw Parent";
      this.btnCreateRaw.UseVisualStyleBackColor = true;
      this.btnCreateRaw.Click += new System.EventHandler(this.btnCreateRaw_Click);
      // 
      // lblRawParent
      // 
      this.lblRawParent.AutoSize = true;
      this.lblRawParent.Location = new System.Drawing.Point(12, 94);
      this.lblRawParent.Name = "lblRawParent";
      this.lblRawParent.Size = new System.Drawing.Size(93, 13);
      this.lblRawParent.TabIndex = 19;
      this.lblRawParent.Text = "Raw Parent (.ocd)";
      // 
      // txtRawParent
      // 
      this.txtRawParent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtRawParent.Location = new System.Drawing.Point(111, 91);
      this.txtRawParent.Name = "txtRawParent";
      this.txtRawParent.Size = new System.Drawing.Size(513, 20);
      this.txtRawParent.TabIndex = 18;
      // 
      // btnRawParent
      // 
      this.btnRawParent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRawParent.Location = new System.Drawing.Point(630, 89);
      this.btnRawParent.Name = "btnRawParent";
      this.btnRawParent.Size = new System.Drawing.Size(25, 23);
      this.btnRawParent.TabIndex = 17;
      this.btnRawParent.Text = "...";
      this.btnRawParent.UseVisualStyleBackColor = true;
      // 
      // lblRawDir
      // 
      this.lblRawDir.AutoSize = true;
      this.lblRawDir.Location = new System.Drawing.Point(12, 155);
      this.lblRawDir.Name = "lblRawDir";
      this.lblRawDir.Size = new System.Drawing.Size(74, 13);
      this.lblRawDir.TabIndex = 22;
      this.lblRawDir.Text = "Raw Directory";
      this.tt.SetToolTip(this.lblRawDir, "Directory containing raw variation files.\r\nCreate in OCAD->Bahnlegung->Export->Ba" +
              "hnkarten");
      // 
      // txtRawDir
      // 
      this.txtRawDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtRawDir.Location = new System.Drawing.Point(111, 152);
      this.txtRawDir.Name = "txtRawDir";
      this.txtRawDir.Size = new System.Drawing.Size(513, 20);
      this.txtRawDir.TabIndex = 21;
      this.tt.SetToolTip(this.txtRawDir, "Directory containing raw variation files.");
      // 
      // btnRawDir
      // 
      this.btnRawDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRawDir.Location = new System.Drawing.Point(630, 150);
      this.btnRawDir.Name = "btnRawDir";
      this.btnRawDir.Size = new System.Drawing.Size(25, 23);
      this.btnRawDir.TabIndex = 20;
      this.btnRawDir.Text = "...";
      this.tt.SetToolTip(this.btnRawDir, "Directory containing raw variation files.");
      this.btnRawDir.UseVisualStyleBackColor = true;
      // 
      // lblClean
      // 
      this.lblClean.AutoSize = true;
      this.lblClean.Location = new System.Drawing.Point(12, 181);
      this.lblClean.Name = "lblClean";
      this.lblClean.Size = new System.Drawing.Size(50, 13);
      this.lblClean.TabIndex = 25;
      this.lblClean.Text = "Clean Dir";
      this.tt.SetToolTip(this.lblClean, "Directory containing variation with layout grafics.\r\n");
      // 
      // txtClean
      // 
      this.txtClean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtClean.Location = new System.Drawing.Point(111, 178);
      this.txtClean.Name = "txtClean";
      this.txtClean.Size = new System.Drawing.Size(513, 20);
      this.txtClean.TabIndex = 24;
      this.tt.SetToolTip(this.txtClean, "Directory containing variation with layout grafics.");
      // 
      // btnVariations
      // 
      this.btnVariations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnVariations.Location = new System.Drawing.Point(630, 176);
      this.btnVariations.Name = "btnVariations";
      this.btnVariations.Size = new System.Drawing.Size(25, 23);
      this.btnVariations.TabIndex = 23;
      this.btnVariations.Text = "...";
      this.tt.SetToolTip(this.btnVariations, "Directory containing variation with layout grafics.");
      this.btnVariations.UseVisualStyleBackColor = true;
      // 
      // lblCategory
      // 
      this.lblCategory.AutoSize = true;
      this.lblCategory.Location = new System.Drawing.Point(12, 290);
      this.lblCategory.Name = "lblCategory";
      this.lblCategory.Size = new System.Drawing.Size(65, 13);
      this.lblCategory.TabIndex = 31;
      this.lblCategory.Text = "Category Dir";
      this.tt.SetToolTip(this.lblCategory, "Directory containing variation with layout grafics.\r\n");
      // 
      // txtCategory
      // 
      this.txtCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCategory.Location = new System.Drawing.Point(111, 287);
      this.txtCategory.Name = "txtCategory";
      this.txtCategory.Size = new System.Drawing.Size(513, 20);
      this.txtCategory.TabIndex = 30;
      this.tt.SetToolTip(this.txtCategory, "Directory containing variation with layout grafics.");
      // 
      // btnCategory
      // 
      this.btnCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCategory.Location = new System.Drawing.Point(630, 285);
      this.btnCategory.Name = "btnCategory";
      this.btnCategory.Size = new System.Drawing.Size(25, 23);
      this.btnCategory.TabIndex = 29;
      this.btnCategory.Text = "...";
      this.tt.SetToolTip(this.btnCategory, "Directory containing variation with layout grafics.");
      this.btnCategory.UseVisualStyleBackColor = true;
      // 
      // txtCleanTemplates
      // 
      this.txtCleanTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtCleanTemplates.Location = new System.Drawing.Point(111, 203);
      this.txtCleanTemplates.Name = "txtCleanTemplates";
      this.txtCleanTemplates.Size = new System.Drawing.Size(513, 20);
      this.txtCleanTemplates.TabIndex = 37;
      this.tt.SetToolTip(this.txtCleanTemplates, "Directory containing variation with layout grafics.");
      // 
      // btnCleanTemplates
      // 
      this.btnCleanTemplates.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCleanTemplates.Location = new System.Drawing.Point(630, 201);
      this.btnCleanTemplates.Name = "btnCleanTemplates";
      this.btnCleanTemplates.Size = new System.Drawing.Size(25, 23);
      this.btnCleanTemplates.TabIndex = 38;
      this.btnCleanTemplates.Text = "...";
      this.tt.SetToolTip(this.btnCleanTemplates, "Directory containing variation with layout grafics.");
      this.btnCleanTemplates.UseVisualStyleBackColor = true;
      // 
      // lblCleanTemplates
      // 
      this.lblCleanTemplates.AutoSize = true;
      this.lblCleanTemplates.Location = new System.Drawing.Point(12, 206);
      this.lblCleanTemplates.Name = "lblCleanTemplates";
      this.lblCleanTemplates.Size = new System.Drawing.Size(86, 13);
      this.lblCleanTemplates.TabIndex = 39;
      this.lblCleanTemplates.Text = "Clean Templates";
      this.tt.SetToolTip(this.lblCleanTemplates, "File containing all pairs of variations with layout grafics\r\n");
      // 
      // btnCreateClean
      // 
      this.btnCreateClean.Location = new System.Drawing.Point(111, 229);
      this.btnCreateClean.Name = "btnCreateClean";
      this.btnCreateClean.Size = new System.Drawing.Size(110, 23);
      this.btnCreateClean.TabIndex = 26;
      this.btnCreateClean.Text = "Create Clean Parts";
      this.btnCreateClean.UseVisualStyleBackColor = true;
      this.btnCreateClean.Click += new System.EventHandler(this.btnCreateClean_Click);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(12, 34);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(40, 13);
      this.label4.TabIndex = 28;
      this.label4.Text = "Course";
      // 
      // lstCourse
      // 
      this.lstCourse.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.lstCourse.FormattingEnabled = true;
      this.lstCourse.Location = new System.Drawing.Point(111, 31);
      this.lstCourse.Name = "lstCourse";
      this.lstCourse.Size = new System.Drawing.Size(110, 21);
      this.lstCourse.TabIndex = 27;
      this.lstCourse.TextChanged += new System.EventHandler(this.lstCourse_TextChanged);
      // 
      // btnCreateCat
      // 
      this.btnCreateCat.Location = new System.Drawing.Point(111, 313);
      this.btnCreateCat.Name = "btnCreateCat";
      this.btnCreateCat.Size = new System.Drawing.Size(110, 23);
      this.btnCreateCat.TabIndex = 32;
      this.btnCreateCat.Text = "Create Category";
      this.btnCreateCat.UseVisualStyleBackColor = true;
      this.btnCreateCat.Click += new System.EventHandler(this.btnCreateCat_Click);
      // 
      // chkConstant
      // 
      this.chkConstant.AutoSize = true;
      this.chkConstant.Location = new System.Drawing.Point(273, 317);
      this.chkConstant.Name = "chkConstant";
      this.chkConstant.Size = new System.Drawing.Size(239, 17);
      this.chkConstant.TabIndex = 33;
      this.chkConstant.Text = "Use constant number of runners per category";
      this.chkConstant.UseVisualStyleBackColor = true;
      // 
      // txtRunners
      // 
      this.txtRunners.Location = new System.Drawing.Point(519, 315);
      this.txtRunners.Name = "txtRunners";
      this.txtRunners.Size = new System.Drawing.Size(28, 20);
      this.txtRunners.TabIndex = 34;
      this.txtRunners.Text = "5";
      this.txtRunners.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // chkNr
      // 
      this.chkNr.AutoSize = true;
      this.chkNr.Checked = true;
      this.chkNr.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkNr.Location = new System.Drawing.Point(273, 233);
      this.chkNr.Name = "chkNr";
      this.chkNr.Size = new System.Drawing.Size(63, 17);
      this.chkNr.TabIndex = 35;
      this.chkNr.Text = "Show #";
      this.chkNr.UseVisualStyleBackColor = true;
      // 
      // chkCodes
      // 
      this.chkCodes.AutoSize = true;
      this.chkCodes.Location = new System.Drawing.Point(362, 233);
      this.chkCodes.Name = "chkCodes";
      this.chkCodes.Size = new System.Drawing.Size(117, 17);
      this.chkCodes.TabIndex = 36;
      this.chkCodes.Text = "Show Control Code";
      this.chkCodes.UseVisualStyleBackColor = true;
      // 
      // btnBahnexport
      // 
      this.btnBahnexport.Location = new System.Drawing.Point(111, 258);
      this.btnBahnexport.Name = "btnBahnexport";
      this.btnBahnexport.Size = new System.Drawing.Size(110, 23);
      this.btnBahnexport.TabIndex = 40;
      this.btnBahnexport.Text = "Bahnexport V8";
      this.btnBahnexport.UseVisualStyleBackColor = true;
      this.btnBahnexport.Click += new System.EventHandler(this.btnBahnexport_Click);
      // 
      // dlgSave
      // 
      this.dlgSave.Filter = "*.txt|*.txt";
      // 
      // dlgOpen
      // 
      this.dlgOpen.FileName = "openFileDialog1";
      // 
      // WdgPermutations
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(667, 348);
      this.Controls.Add(this.btnBahnexport);
      this.Controls.Add(this.lblCleanTemplates);
      this.Controls.Add(this.btnCleanTemplates);
      this.Controls.Add(this.txtCleanTemplates);
      this.Controls.Add(this.chkCodes);
      this.Controls.Add(this.chkNr);
      this.Controls.Add(this.chkConstant);
      this.Controls.Add(this.txtRunners);
      this.Controls.Add(this.btnCreateCat);
      this.Controls.Add(this.lblCategory);
      this.Controls.Add(this.txtCategory);
      this.Controls.Add(this.btnCategory);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.lstCourse);
      this.Controls.Add(this.btnCreateClean);
      this.Controls.Add(this.lblClean);
      this.Controls.Add(this.txtClean);
      this.Controls.Add(this.btnVariations);
      this.Controls.Add(this.lblRawDir);
      this.Controls.Add(this.txtRawDir);
      this.Controls.Add(this.btnRawDir);
      this.Controls.Add(this.lblRawParent);
      this.Controls.Add(this.txtRawParent);
      this.Controls.Add(this.btnRawParent);
      this.Controls.Add(this.btnCreateRaw);
      this.Controls.Add(this.txtCourse);
      this.Controls.Add(this.lblOcad);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgPermutations";
      this.Text = "Permutations";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox txtCourse;
    private System.Windows.Forms.Label lblOcad;
    private System.Windows.Forms.Button btnCreateRaw;
    private System.Windows.Forms.Label lblRawParent;
    private System.Windows.Forms.TextBox txtRawParent;
    private System.Windows.Forms.Button btnRawParent;
    private System.Windows.Forms.Label lblRawDir;
    private System.Windows.Forms.ToolTip tt;
    private System.Windows.Forms.TextBox txtRawDir;
    private System.Windows.Forms.Button btnRawDir;
    private System.Windows.Forms.Label lblClean;
    private System.Windows.Forms.TextBox txtClean;
    private System.Windows.Forms.Button btnVariations;
    private System.Windows.Forms.Button btnCreateClean;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox lstCourse;
    private System.Windows.Forms.Label lblCategory;
    private System.Windows.Forms.TextBox txtCategory;
    private System.Windows.Forms.Button btnCategory;
    private System.Windows.Forms.Button btnCreateCat;
    private System.Windows.Forms.CheckBox chkConstant;
    private System.Windows.Forms.TextBox txtRunners;
    private System.Windows.Forms.CheckBox chkNr;
    private System.Windows.Forms.CheckBox chkCodes;
    private System.Windows.Forms.TextBox txtCleanTemplates;
    private System.Windows.Forms.Button btnCleanTemplates;
    private System.Windows.Forms.Label lblCleanTemplates;
    private System.Windows.Forms.Button btnBahnexport;
    private System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.OpenFileDialog dlgOpen;
  }
}