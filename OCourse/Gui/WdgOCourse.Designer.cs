namespace OCourse.Gui
{
  partial class WdgOCourse
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
      if (disposing)
      {
        components?.Dispose();
        _bindingSource?.Dispose();
        _wdgVariations?.Dispose();
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgOCourse));
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this.btnCourse = new System.Windows.Forms.Button();
      this._txtCourse = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this._lstCourses = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this._lblProgress = new System.Windows.Forms.Label();
      this.btnBackCalc = new System.Windows.Forms.Button();
      this.chkOnTop = new System.Windows.Forms.CheckBox();
      this.pnlCourse = new System.Windows.Forms.Panel();
      this._cntSection = new OCourse.Gui.CntSection();
      this.btnExport = new System.Windows.Forms.Button();
      this.btnImport = new System.Windows.Forms.Button();
      this.dlgSave = new System.Windows.Forms.SaveFileDialog();
      this.splCourse = new System.Windows.Forms.SplitContainer();
      this.splVars = new System.Windows.Forms.SplitContainer();
      this.btnRefreshSection = new System.Windows.Forms.Button();
      this.lblVariations = new System.Windows.Forms.Label();
      this._lstVarBuilders = new System.Windows.Forms.ComboBox();
      this.dgvInfo = new System.Windows.Forms.DataGridView();
      this.mnuVarExport = new System.Windows.Forms.MenuStrip();
      this.mniVarExport = new System.Windows.Forms.ToolStripMenuItem();
      this.mniVarExpOcad = new System.Windows.Forms.ToolStripMenuItem();
      this.mniVarExpCsv = new System.Windows.Forms.ToolStripMenuItem();
      this.mnuPermExport = new System.Windows.Forms.MenuStrip();
      this.mniPermExport = new System.Windows.Forms.ToolStripMenuItem();
      this.mniPermExpOcad = new System.Windows.Forms.ToolStripMenuItem();
      this.varPermExpCsv = new System.Windows.Forms.ToolStripMenuItem();
      this.lblEstime = new System.Windows.Forms.Label();
      this.txtEstimate = new System.Windows.Forms.TextBox();
      this.lblCat = new System.Windows.Forms.Label();
      this._lstCats = new System.Windows.Forms.ComboBox();
      this._txtMax = new System.Windows.Forms.TextBox();
      this.lblMax = new System.Windows.Forms.Label();
      this.lblMin = new System.Windows.Forms.Label();
      this._txtMin = new System.Windows.Forms.TextBox();
      this.lblPermuts = new System.Windows.Forms.Label();
      this.btnCalcPermut = new System.Windows.Forms.Button();
      this.dgvPermut = new System.Windows.Forms.DataGridView();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.btnCreateScripts = new System.Windows.Forms.Button();
      this.btnCalcSelection = new System.Windows.Forms.Button();
      this._lstVelo = new System.Windows.Forms.ComboBox();
      this.btnRelay = new System.Windows.Forms.Button();
      this.btnTrack = new System.Windows.Forms.Button();
      this.btnPermutations = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this._lstDisplayType = new System.Windows.Forms.ComboBox();
      this.mnuAll = new System.Windows.Forms.MenuStrip();
      this.mniSettings = new System.Windows.Forms.ToolStripMenuItem();
      this.mniOpen = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSave = new System.Windows.Forms.ToolStripMenuItem();
      this.mniSaveAs = new System.Windows.Forms.ToolStripMenuItem();
      this._cntConfig = new LeastCostPathUI.CntConfigView();
      this.splModelCourse = new System.Windows.Forms.SplitContainer();
      this.pnlCourse.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splCourse)).BeginInit();
      this.splCourse.Panel1.SuspendLayout();
      this.splCourse.Panel2.SuspendLayout();
      this.splCourse.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splVars)).BeginInit();
      this.splVars.Panel1.SuspendLayout();
      this.splVars.Panel2.SuspendLayout();
      this.splVars.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvInfo)).BeginInit();
      this.mnuVarExport.SuspendLayout();
      this.mnuPermExport.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvPermut)).BeginInit();
      this.mnuAll.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splModelCourse)).BeginInit();
      this.splModelCourse.Panel1.SuspendLayout();
      this.splModelCourse.Panel2.SuspendLayout();
      this.splModelCourse.SuspendLayout();
      this.SuspendLayout();
      // 
      // btnCourse
      // 
      this.btnCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCourse.Image = ((System.Drawing.Image)(resources.GetObject("btnCourse.Image")));
      this.btnCourse.Location = new System.Drawing.Point(780, 25);
      this.btnCourse.Name = "btnCourse";
      this.btnCourse.Size = new System.Drawing.Size(24, 24);
      this.btnCourse.TabIndex = 8;
      this.btnCourse.UseVisualStyleBackColor = true;
      this.btnCourse.Click += new System.EventHandler(this.BtnCourse_Click);
      // 
      // _txtCourse
      // 
      this._txtCourse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtCourse.Location = new System.Drawing.Point(88, 27);
      this._txtCourse.Name = "_txtCourse";
      this._txtCourse.Size = new System.Drawing.Size(686, 20);
      this._txtCourse.TabIndex = 7;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 30);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(59, 13);
      this.label3.TabIndex = 6;
      this.label3.Text = "Course File";
      // 
      // _lstCourses
      // 
      this._lstCourses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstCourses.FormattingEnabled = true;
      this._lstCourses.Location = new System.Drawing.Point(83, 5);
      this._lstCourses.Name = "_lstCourses";
      this._lstCourses.Size = new System.Drawing.Size(103, 21);
      this._lstCourses.TabIndex = 9;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(3, 8);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(40, 13);
      this.label4.TabIndex = 10;
      this.label4.Text = "Course";
      // 
      // _lblProgress
      // 
      this._lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this._lblProgress.AutoSize = true;
      this._lblProgress.Location = new System.Drawing.Point(85, 427);
      this._lblProgress.Name = "_lblProgress";
      this._lblProgress.Size = new System.Drawing.Size(48, 13);
      this._lblProgress.TabIndex = 15;
      this._lblProgress.Text = "Progress";
      // 
      // btnBackCalc
      // 
      this.btnBackCalc.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnBackCalc.Location = new System.Drawing.Point(15, 316);
      this.btnBackCalc.Name = "btnBackCalc";
      this.btnBackCalc.Size = new System.Drawing.Size(54, 46);
      this.btnBackCalc.TabIndex = 16;
      this.btnBackCalc.Text = "Back Calc";
      this.btnBackCalc.UseVisualStyleBackColor = true;
      this.btnBackCalc.Click += new System.EventHandler(this.BtnBackCalc_Click);
      // 
      // chkOnTop
      // 
      this.chkOnTop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.chkOnTop.AutoSize = true;
      this.chkOnTop.Location = new System.Drawing.Point(12, 426);
      this.chkOnTop.Name = "chkOnTop";
      this.chkOnTop.Size = new System.Drawing.Size(62, 17);
      this.chkOnTop.TabIndex = 17;
      this.chkOnTop.Text = "On Top";
      this.chkOnTop.UseVisualStyleBackColor = true;
      this.chkOnTop.CheckedChanged += new System.EventHandler(this.ChkOnTop_CheckedChanged);
      // 
      // pnlCourse
      // 
      this.pnlCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.pnlCourse.AutoScroll = true;
      this.pnlCourse.BackColor = System.Drawing.SystemColors.Window;
      this.pnlCourse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.pnlCourse.Controls.Add(this._cntSection);
      this.pnlCourse.Location = new System.Drawing.Point(0, 30);
      this.pnlCourse.Name = "pnlCourse";
      this.pnlCourse.Size = new System.Drawing.Size(236, 218);
      this.pnlCourse.TabIndex = 19;
      // 
      // _cntSection
      // 
      this._cntSection.BackColor = System.Drawing.SystemColors.Window;
      this._cntSection.BoldCombination = null;
      this._cntSection.Course = null;
      this._cntSection.Location = new System.Drawing.Point(1, 1);
      this._cntSection.Name = "_cntSection";
      this._cntSection.Size = new System.Drawing.Size(160, 271);
      this._cntSection.TabIndex = 18;
      this._cntSection.Vm = null;
      // 
      // btnExport
      // 
      this.btnExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnExport.Location = new System.Drawing.Point(15, 368);
      this.btnExport.Name = "btnExport";
      this.btnExport.Size = new System.Drawing.Size(54, 23);
      this.btnExport.TabIndex = 20;
      this.btnExport.Text = "Export";
      this.btnExport.UseVisualStyleBackColor = true;
      this.btnExport.Click += new System.EventHandler(this.BtnExport_Click);
      // 
      // btnImport
      // 
      this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnImport.Location = new System.Drawing.Point(15, 397);
      this.btnImport.Name = "btnImport";
      this.btnImport.Size = new System.Drawing.Size(54, 23);
      this.btnImport.TabIndex = 21;
      this.btnImport.Text = "Import";
      this.btnImport.UseVisualStyleBackColor = true;
      this.btnImport.Click += new System.EventHandler(this.BtnImport_Click);
      // 
      // splCourse
      // 
      this.splCourse.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splCourse.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splCourse.Location = new System.Drawing.Point(0, 0);
      this.splCourse.Name = "splCourse";
      // 
      // splCourse.Panel1
      // 
      this.splCourse.Panel1.Controls.Add(this.pnlCourse);
      this.splCourse.Panel1.Controls.Add(this._lstCourses);
      this.splCourse.Panel1.Controls.Add(this.label4);
      // 
      // splCourse.Panel2
      // 
      this.splCourse.Panel2.Controls.Add(this.splVars);
      this.splCourse.Size = new System.Drawing.Size(685, 248);
      this.splCourse.SplitterDistance = 236;
      this.splCourse.TabIndex = 22;
      // 
      // splVars
      // 
      this.splVars.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splVars.Location = new System.Drawing.Point(0, 0);
      this.splVars.Name = "splVars";
      this.splVars.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splVars.Panel1
      // 
      this.splVars.Panel1.Controls.Add(this.btnRefreshSection);
      this.splVars.Panel1.Controls.Add(this.lblVariations);
      this.splVars.Panel1.Controls.Add(this._lstVarBuilders);
      this.splVars.Panel1.Controls.Add(this.dgvInfo);
      this.splVars.Panel1.Controls.Add(this.mnuVarExport);
      // 
      // splVars.Panel2
      // 
      this.splVars.Panel2.Controls.Add(this.mnuPermExport);
      this.splVars.Panel2.Controls.Add(this.lblEstime);
      this.splVars.Panel2.Controls.Add(this.txtEstimate);
      this.splVars.Panel2.Controls.Add(this.lblCat);
      this.splVars.Panel2.Controls.Add(this._lstCats);
      this.splVars.Panel2.Controls.Add(this._txtMax);
      this.splVars.Panel2.Controls.Add(this.lblMax);
      this.splVars.Panel2.Controls.Add(this.lblMin);
      this.splVars.Panel2.Controls.Add(this._txtMin);
      this.splVars.Panel2.Controls.Add(this.lblPermuts);
      this.splVars.Panel2.Controls.Add(this.btnCalcPermut);
      this.splVars.Panel2.Controls.Add(this.dgvPermut);
      this.splVars.Size = new System.Drawing.Size(445, 248);
      this.splVars.SplitterDistance = 141;
      this.splVars.TabIndex = 1;
      // 
      // btnRefreshSection
      // 
      this.btnRefreshSection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRefreshSection.Enabled = false;
      this.btnRefreshSection.Image = global::OCourse.Properties.Resources.calc_recalc;
      this.btnRefreshSection.Location = new System.Drawing.Point(422, 3);
      this.btnRefreshSection.Name = "btnRefreshSection";
      this.btnRefreshSection.Size = new System.Drawing.Size(22, 23);
      this.btnRefreshSection.TabIndex = 39;
      this.ttp.SetToolTip(this.btnRefreshSection, "Reset calculated route for selected section");
      this.btnRefreshSection.UseVisualStyleBackColor = true;
      this.btnRefreshSection.Click += new System.EventHandler(this.BtnRefreshSection_Click);
      // 
      // lblVariations
      // 
      this.lblVariations.AutoSize = true;
      this.lblVariations.Location = new System.Drawing.Point(3, 6);
      this.lblVariations.Name = "lblVariations";
      this.lblVariations.Size = new System.Drawing.Size(53, 13);
      this.lblVariations.TabIndex = 37;
      this.lblVariations.Text = "Variations";
      // 
      // _lstVarBuilders
      // 
      this._lstVarBuilders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstVarBuilders.FormattingEnabled = true;
      this._lstVarBuilders.Location = new System.Drawing.Point(62, 5);
      this._lstVarBuilders.Name = "_lstVarBuilders";
      this._lstVarBuilders.Size = new System.Drawing.Size(102, 21);
      this._lstVarBuilders.TabIndex = 36;
      // 
      // dgvInfo
      // 
      this.dgvInfo.AllowUserToAddRows = false;
      this.dgvInfo.AllowUserToOrderColumns = true;
      this.dgvInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dgvInfo.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.dgvInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.dgvInfo.DefaultCellStyle = dataGridViewCellStyle2;
      this.dgvInfo.Location = new System.Drawing.Point(0, 30);
      this.dgvInfo.Name = "dgvInfo";
      this.dgvInfo.ReadOnly = true;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.dgvInfo.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.dgvInfo.RowHeadersVisible = false;
      this.dgvInfo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.dgvInfo.Size = new System.Drawing.Size(445, 108);
      this.dgvInfo.TabIndex = 0;
      this.dgvInfo.CurrentCellChanged += new System.EventHandler(this.DgvInfo_CurrentCellChanged);
      // 
      // mnuVarExport
      // 
      this.mnuVarExport.Dock = System.Windows.Forms.DockStyle.None;
      this.mnuVarExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniVarExport});
      this.mnuVarExport.Location = new System.Drawing.Point(169, 3);
      this.mnuVarExport.Name = "mnuVarExport";
      this.mnuVarExport.Size = new System.Drawing.Size(60, 24);
      this.mnuVarExport.TabIndex = 40;
      this.mnuVarExport.Text = "menuStrip1";
      // 
      // mniVarExport
      // 
      this.mniVarExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniVarExpOcad,
            this.mniVarExpCsv});
      this.mniVarExport.Name = "mniVarExport";
      this.mniVarExport.Size = new System.Drawing.Size(52, 20);
      this.mniVarExport.Text = "Export";
      // 
      // mniVarExpOcad
      // 
      this.mniVarExpOcad.Image = global::OCourse.Properties.Resources.exp_course;
      this.mniVarExpOcad.Name = "mniVarExpOcad";
      this.mniVarExpOcad.Size = new System.Drawing.Size(116, 22);
      this.mniVarExpOcad.Text = "OCAD...";
      this.mniVarExpOcad.Click += new System.EventHandler(this.BtnExportCourses_Click);
      // 
      // mniVarExpCsv
      // 
      this.mniVarExpCsv.Image = global::OCourse.Properties.Resources.exp_csv;
      this.mniVarExpCsv.Name = "mniVarExpCsv";
      this.mniVarExpCsv.Size = new System.Drawing.Size(116, 22);
      this.mniVarExpCsv.Text = "Text...";
      this.mniVarExpCsv.Click += new System.EventHandler(this.BtnExportCsv_Click);
      // 
      // mnuPermExport
      // 
      this.mnuPermExport.Dock = System.Windows.Forms.DockStyle.None;
      this.mnuPermExport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniPermExport});
      this.mnuPermExport.Location = new System.Drawing.Point(346, 21);
      this.mnuPermExport.Name = "mnuPermExport";
      this.mnuPermExport.Size = new System.Drawing.Size(60, 24);
      this.mnuPermExport.TabIndex = 41;
      this.mnuPermExport.Text = "menuStrip1";
      // 
      // mniPermExport
      // 
      this.mniPermExport.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniPermExpOcad,
            this.varPermExpCsv});
      this.mniPermExport.Name = "mniPermExport";
      this.mniPermExport.Size = new System.Drawing.Size(52, 20);
      this.mniPermExport.Text = "Export";
      // 
      // mniPermExpOcad
      // 
      this.mniPermExpOcad.Image = global::OCourse.Properties.Resources.exp_course;
      this.mniPermExpOcad.Name = "mniPermExpOcad";
      this.mniPermExpOcad.Size = new System.Drawing.Size(116, 22);
      this.mniPermExpOcad.Text = "OCAD...";
      this.mniPermExpOcad.Click += new System.EventHandler(this.BtnExportPermutOcad_Click);
      // 
      // varPermExpCsv
      // 
      this.varPermExpCsv.Image = global::OCourse.Properties.Resources.exp_csv;
      this.varPermExpCsv.Name = "varPermExpCsv";
      this.varPermExpCsv.Size = new System.Drawing.Size(116, 22);
      this.varPermExpCsv.Text = "Text...";
      this.varPermExpCsv.Click += new System.EventHandler(this.BtnExportPermutCsv_Click);
      // 
      // lblEstime
      // 
      this.lblEstime.AutoSize = true;
      this.lblEstime.Location = new System.Drawing.Point(3, 28);
      this.lblEstime.Name = "lblEstime";
      this.lblEstime.Size = new System.Drawing.Size(14, 13);
      this.lblEstime.TabIndex = 20;
      this.lblEstime.Text = "~";
      this.ttp.SetToolTip(this.lblEstime, "Estimated count of permutations");
      // 
      // txtEstimate
      // 
      this.txtEstimate.Location = new System.Drawing.Point(24, 23);
      this.txtEstimate.Name = "txtEstimate";
      this.txtEstimate.ReadOnly = true;
      this.txtEstimate.Size = new System.Drawing.Size(47, 20);
      this.txtEstimate.TabIndex = 19;
      this.ttp.SetToolTip(this.txtEstimate, "Estimated count of permutations");
      // 
      // lblCat
      // 
      this.lblCat.AutoSize = true;
      this.lblCat.Location = new System.Drawing.Point(94, 2);
      this.lblCat.Name = "lblCat";
      this.lblCat.Size = new System.Drawing.Size(49, 13);
      this.lblCat.TabIndex = 18;
      this.lblCat.Text = "Category";
      // 
      // _lstCats
      // 
      this._lstCats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstCats.FormattingEnabled = true;
      this._lstCats.Location = new System.Drawing.Point(149, 1);
      this._lstCats.Name = "_lstCats";
      this._lstCats.Size = new System.Drawing.Size(103, 21);
      this._lstCats.TabIndex = 17;
      // 
      // _txtMax
      // 
      this._txtMax.Location = new System.Drawing.Point(261, 25);
      this._txtMax.Name = "_txtMax";
      this._txtMax.Size = new System.Drawing.Size(47, 20);
      this._txtMax.TabIndex = 15;
      // 
      // lblMax
      // 
      this.lblMax.AutoSize = true;
      this.lblMax.Location = new System.Drawing.Point(208, 28);
      this.lblMax.Name = "lblMax";
      this.lblMax.Size = new System.Drawing.Size(44, 13);
      this.lblMax.TabIndex = 14;
      this.lblMax.Text = "Max. Nr";
      // 
      // lblMin
      // 
      this.lblMin.AutoSize = true;
      this.lblMin.Location = new System.Drawing.Point(94, 28);
      this.lblMin.Name = "lblMin";
      this.lblMin.Size = new System.Drawing.Size(41, 13);
      this.lblMin.TabIndex = 13;
      this.lblMin.Text = "Min. Nr";
      // 
      // _txtMin
      // 
      this._txtMin.Location = new System.Drawing.Point(149, 25);
      this._txtMin.Name = "_txtMin";
      this._txtMin.Size = new System.Drawing.Size(47, 20);
      this._txtMin.TabIndex = 12;
      // 
      // lblPermuts
      // 
      this.lblPermuts.AutoSize = true;
      this.lblPermuts.Location = new System.Drawing.Point(3, 2);
      this.lblPermuts.Name = "lblPermuts";
      this.lblPermuts.Size = new System.Drawing.Size(68, 13);
      this.lblPermuts.TabIndex = 11;
      this.lblPermuts.Text = "Permutations";
      // 
      // btnCalcPermut
      // 
      this.btnCalcPermut.Image = global::OCourse.Properties.Resources.calc_permuts;
      this.btnCalcPermut.Location = new System.Drawing.Point(321, 23);
      this.btnCalcPermut.Name = "btnCalcPermut";
      this.btnCalcPermut.Size = new System.Drawing.Size(22, 22);
      this.btnCalcPermut.TabIndex = 2;
      this.ttp.SetToolTip(this.btnCalcPermut, "Calculate permutions");
      this.btnCalcPermut.UseVisualStyleBackColor = true;
      this.btnCalcPermut.Click += new System.EventHandler(this.BtnCalcPermut_Click);
      // 
      // dgvPermut
      // 
      this.dgvPermut.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.dgvPermut.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.dgvPermut.Location = new System.Drawing.Point(0, 51);
      this.dgvPermut.Name = "dgvPermut";
      this.dgvPermut.Size = new System.Drawing.Size(445, 52);
      this.dgvPermut.TabIndex = 0;
      this.ttp.SetToolTip(this.dgvPermut, "Select combinations to be shown in map");
      this.dgvPermut.SelectionChanged += new System.EventHandler(this.DgvPermut_SelectionChanged);
      // 
      // btnCreateScripts
      // 
      this.btnCreateScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnCreateScripts.Location = new System.Drawing.Point(15, 264);
      this.btnCreateScripts.Name = "btnCreateScripts";
      this.btnCreateScripts.Size = new System.Drawing.Size(54, 46);
      this.btnCreateScripts.TabIndex = 32;
      this.btnCreateScripts.Text = "Create\r\nScripts";
      this.ttp.SetToolTip(this.btnCreateScripts, "Create Bat file that can be uses as input for LeastCostPathUI.exe");
      this.btnCreateScripts.UseVisualStyleBackColor = true;
      this.btnCreateScripts.Click += new System.EventHandler(this.BtnCreateScripts_Click);
      // 
      // btnCalcSelection
      // 
      this.btnCalcSelection.Enabled = false;
      this.btnCalcSelection.Location = new System.Drawing.Point(15, 213);
      this.btnCalcSelection.Name = "btnCalcSelection";
      this.btnCalcSelection.Size = new System.Drawing.Size(54, 46);
      this.btnCalcSelection.TabIndex = 26;
      this.btnCalcSelection.Text = "Calc ...-...";
      this.btnCalcSelection.UseVisualStyleBackColor = true;
      this.btnCalcSelection.Click += new System.EventHandler(this.BtnCalcSelection_Click);
      // 
      // _lstVelo
      // 
      this._lstVelo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstVelo.FormattingEnabled = true;
      this._lstVelo.Location = new System.Drawing.Point(643, 158);
      this._lstVelo.Name = "_lstVelo";
      this._lstVelo.Size = new System.Drawing.Size(136, 21);
      this._lstVelo.TabIndex = 27;
      this._lstVelo.Visible = false;
      // 
      // btnRelay
      // 
      this.btnRelay.Location = new System.Drawing.Point(644, 202);
      this.btnRelay.Name = "btnRelay";
      this.btnRelay.Size = new System.Drawing.Size(75, 23);
      this.btnRelay.TabIndex = 29;
      this.btnRelay.Text = "Relay";
      this.btnRelay.UseVisualStyleBackColor = true;
      this.btnRelay.Visible = false;
      this.btnRelay.Click += new System.EventHandler(this.BtnRelay_Click);
      // 
      // btnTrack
      // 
      this.btnTrack.Location = new System.Drawing.Point(644, 260);
      this.btnTrack.Name = "btnTrack";
      this.btnTrack.Size = new System.Drawing.Size(75, 23);
      this.btnTrack.TabIndex = 30;
      this.btnTrack.Text = "Track";
      this.btnTrack.UseVisualStyleBackColor = true;
      this.btnTrack.Visible = false;
      this.btnTrack.Click += new System.EventHandler(this.BtnTrack_Click);
      // 
      // btnPermutations
      // 
      this.btnPermutations.Location = new System.Drawing.Point(644, 231);
      this.btnPermutations.Name = "btnPermutations";
      this.btnPermutations.Size = new System.Drawing.Size(80, 23);
      this.btnPermutations.TabIndex = 31;
      this.btnPermutations.Text = "Permutations";
      this.btnPermutations.UseVisualStyleBackColor = true;
      this.btnPermutations.Visible = false;
      this.btnPermutations.Click += new System.EventHandler(this.BtnPermutations_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.Location = new System.Drawing.Point(660, 422);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(113, 23);
      this.btnCancel.TabIndex = 34;
      this.btnCancel.Text = "Cancel Calculation";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Visible = false;
      this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // _lstDisplayType
      // 
      this._lstDisplayType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this._lstDisplayType.FormattingEnabled = true;
      this._lstDisplayType.Location = new System.Drawing.Point(12, 127);
      this._lstDisplayType.Name = "_lstDisplayType";
      this._lstDisplayType.Size = new System.Drawing.Size(62, 21);
      this._lstDisplayType.TabIndex = 35;
      // 
      // mnuAll
      // 
      this.mnuAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniSettings});
      this.mnuAll.Location = new System.Drawing.Point(0, 0);
      this.mnuAll.Name = "mnuAll";
      this.mnuAll.Size = new System.Drawing.Size(818, 24);
      this.mnuAll.TabIndex = 36;
      this.mnuAll.Text = "menuStrip1";
      // 
      // mniSettings
      // 
      this.mniSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniOpen,
            this.mniSave,
            this.mniSaveAs});
      this.mniSettings.Name = "mniSettings";
      this.mniSettings.Size = new System.Drawing.Size(61, 20);
      this.mniSettings.Text = "&Settings";
      // 
      // mniOpen
      // 
      this.mniOpen.Name = "mniOpen";
      this.mniOpen.Size = new System.Drawing.Size(114, 22);
      this.mniOpen.Text = "Open";
      this.mniOpen.Click += new System.EventHandler(this.MniOpen_Click);
      // 
      // mniSave
      // 
      this.mniSave.Name = "mniSave";
      this.mniSave.Size = new System.Drawing.Size(114, 22);
      this.mniSave.Text = "&Save";
      this.mniSave.Click += new System.EventHandler(this.MniSave_Click);
      // 
      // mniSaveAs
      // 
      this.mniSaveAs.Name = "mniSaveAs";
      this.mniSaveAs.Size = new System.Drawing.Size(114, 22);
      this.mniSaveAs.Text = "Save As";
      this.mniSaveAs.Click += new System.EventHandler(this.MniSaveAs_Click);
      // 
      // _cntConfig
      // 
      this._cntConfig.ConfigVm = null;
      this._cntConfig.Dock = System.Windows.Forms.DockStyle.Fill;
      this._cntConfig.Location = new System.Drawing.Point(0, 0);
      this._cntConfig.Name = "_cntConfig";
      this._cntConfig.Size = new System.Drawing.Size(685, 115);
      this._cntConfig.TabIndex = 33;
      // 
      // splModelCourse
      // 
      this.splModelCourse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.splModelCourse.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      this.splModelCourse.Location = new System.Drawing.Point(88, 53);
      this.splModelCourse.Name = "splModelCourse";
      this.splModelCourse.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splModelCourse.Panel1
      // 
      this.splModelCourse.Panel1.Controls.Add(this._cntConfig);
      // 
      // splModelCourse.Panel2
      // 
      this.splModelCourse.Panel2.Controls.Add(this.splCourse);
      this.splModelCourse.Size = new System.Drawing.Size(685, 367);
      this.splModelCourse.SplitterDistance = 115;
      this.splModelCourse.TabIndex = 37;
      // 
      // WdgOCourse
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(818, 449);
      this.Controls.Add(this.splModelCourse);
      this.Controls.Add(this.btnCourse);
      this.Controls.Add(this._txtCourse);
      this.Controls.Add(this.label3);
      this.Controls.Add(this._lstDisplayType);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnCreateScripts);
      this.Controls.Add(this.btnPermutations);
      this.Controls.Add(this.btnTrack);
      this.Controls.Add(this.btnRelay);
      this.Controls.Add(this._lstVelo);
      this.Controls.Add(this.btnCalcSelection);
      this.Controls.Add(this.btnImport);
      this.Controls.Add(this.btnExport);
      this.Controls.Add(this.chkOnTop);
      this.Controls.Add(this.btnBackCalc);
      this.Controls.Add(this._lblProgress);
      this.Controls.Add(this.mnuAll);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MainMenuStrip = this.mnuAll;
      this.Name = "WdgOCourse";
      this.Text = "OCourse";
      this.Load += new System.EventHandler(this.WdgOCourse_Load);
      this.pnlCourse.ResumeLayout(false);
      this.splCourse.Panel1.ResumeLayout(false);
      this.splCourse.Panel1.PerformLayout();
      this.splCourse.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splCourse)).EndInit();
      this.splCourse.ResumeLayout(false);
      this.splVars.Panel1.ResumeLayout(false);
      this.splVars.Panel1.PerformLayout();
      this.splVars.Panel2.ResumeLayout(false);
      this.splVars.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splVars)).EndInit();
      this.splVars.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.dgvInfo)).EndInit();
      this.mnuVarExport.ResumeLayout(false);
      this.mnuVarExport.PerformLayout();
      this.mnuPermExport.ResumeLayout(false);
      this.mnuPermExport.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.dgvPermut)).EndInit();
      this.mnuAll.ResumeLayout(false);
      this.mnuAll.PerformLayout();
      this.splModelCourse.Panel1.ResumeLayout(false);
      this.splModelCourse.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splModelCourse)).EndInit();
      this.splModelCourse.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.OpenFileDialog dlgOpen;
    private System.Windows.Forms.Button btnCourse;
    private System.Windows.Forms.TextBox _txtCourse;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.ComboBox _lstCourses;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label _lblProgress;
    private System.Windows.Forms.Button btnBackCalc;
    private System.Windows.Forms.CheckBox chkOnTop;
    private CntSection _cntSection;
    private System.Windows.Forms.Panel pnlCourse;
    private System.Windows.Forms.Button btnExport;
    private System.Windows.Forms.Button btnImport;
    private System.Windows.Forms.SaveFileDialog dlgSave;
    private System.Windows.Forms.SplitContainer splCourse;
    private System.Windows.Forms.DataGridView dgvInfo;
    private System.Windows.Forms.SplitContainer splVars;
    private System.Windows.Forms.DataGridView dgvPermut;
    private System.Windows.Forms.ToolTip ttp;
    private System.Windows.Forms.Button btnCalcSelection;
    private System.Windows.Forms.ComboBox _lstVelo;
    private System.Windows.Forms.Button btnRelay;
    private System.Windows.Forms.Button btnTrack;
    private System.Windows.Forms.Button btnPermutations;
    private System.Windows.Forms.Button btnCreateScripts;
    private LeastCostPathUI.CntConfigView _cntConfig;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.TextBox _txtMax;
    private System.Windows.Forms.Label lblMax;
    private System.Windows.Forms.Label lblMin;
    private System.Windows.Forms.TextBox _txtMin;
    private System.Windows.Forms.Label lblPermuts;
    private System.Windows.Forms.Button btnCalcPermut;
    private System.Windows.Forms.Label lblCat;
    private System.Windows.Forms.ComboBox _lstCats;
    private System.Windows.Forms.ComboBox _lstVarBuilders;
    private System.Windows.Forms.Label lblVariations;
    private System.Windows.Forms.ComboBox _lstDisplayType;
    private System.Windows.Forms.Label lblEstime;
    private System.Windows.Forms.TextBox txtEstimate;
    private System.Windows.Forms.MenuStrip mnuAll;
    private System.Windows.Forms.ToolStripMenuItem mniSettings;
    private System.Windows.Forms.ToolStripMenuItem mniOpen;
    private System.Windows.Forms.ToolStripMenuItem mniSave;
    private System.Windows.Forms.ToolStripMenuItem mniSaveAs;
    private System.Windows.Forms.Button btnRefreshSection;
    private System.Windows.Forms.SplitContainer splModelCourse;
    private System.Windows.Forms.MenuStrip mnuVarExport;
    private System.Windows.Forms.ToolStripMenuItem mniVarExport;
    private System.Windows.Forms.ToolStripMenuItem mniVarExpOcad;
    private System.Windows.Forms.ToolStripMenuItem mniVarExpCsv;
    private System.Windows.Forms.MenuStrip mnuPermExport;
    private System.Windows.Forms.ToolStripMenuItem mniPermExport;
    private System.Windows.Forms.ToolStripMenuItem mniPermExpOcad;
    private System.Windows.Forms.ToolStripMenuItem varPermExpCsv;
  }
}

