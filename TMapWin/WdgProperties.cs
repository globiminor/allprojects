using System;
using System.Data;
using System.Windows.Forms;
using TMap;
using TData;

namespace TMapWin
{
  /// <summary>
  /// Summary description for WdgProperties.
  /// </summary>
  public class WdgProperties : Form
  {
    private MapData _data;
    private DataRow _rowLine;
    private ISymbolisationView _symbolisation;
    private SymbolTable _symbolTable;
    private WdgSymbol wdgSymbol = null;
    /// <summary>
    /// designer controls
    /// </summary>
    private TabControl tabDisplay;
    private TabPage tpgSymbol;
    private DataGridView grdSymbols;
    private Button btnUp;
    private Button btnDown;
    private TabPage tpgDisplay;
    private CheckBox chkVisible;
    private TextBox txtWhere;
    private Label lblWhere;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public WdgProperties()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      Init();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgProperties));
      this.tabDisplay = new System.Windows.Forms.TabControl();
      this.tpgDisplay = new System.Windows.Forms.TabPage();
      this.chkVisible = new System.Windows.Forms.CheckBox();
      this.tpgSymbol = new System.Windows.Forms.TabPage();
      this.btnDown = new System.Windows.Forms.Button();
      this.btnUp = new System.Windows.Forms.Button();
      this.grdSymbols = new System.Windows.Forms.DataGridView();
      this.lblWhere = new System.Windows.Forms.Label();
      this.txtWhere = new System.Windows.Forms.TextBox();
      this.tabDisplay.SuspendLayout();
      this.tpgDisplay.SuspendLayout();
      this.tpgSymbol.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbols)).BeginInit();
      this.SuspendLayout();
      // 
      // tabDisplay
      // 
      this.tabDisplay.Controls.Add(this.tpgDisplay);
      this.tabDisplay.Controls.Add(this.tpgSymbol);
      this.tabDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabDisplay.Location = new System.Drawing.Point(0, 0);
      this.tabDisplay.Name = "tabDisplay";
      this.tabDisplay.SelectedIndex = 0;
      this.tabDisplay.Size = new System.Drawing.Size(336, 273);
      this.tabDisplay.TabIndex = 0;
      // 
      // tpgDisplay
      // 
      this.tpgDisplay.Controls.Add(this.txtWhere);
      this.tpgDisplay.Controls.Add(this.lblWhere);
      this.tpgDisplay.Controls.Add(this.chkVisible);
      this.tpgDisplay.Location = new System.Drawing.Point(4, 22);
      this.tpgDisplay.Name = "tpgDisplay";
      this.tpgDisplay.Padding = new System.Windows.Forms.Padding(3);
      this.tpgDisplay.Size = new System.Drawing.Size(328, 247);
      this.tpgDisplay.TabIndex = 1;
      this.tpgDisplay.Text = "Display";
      this.tpgDisplay.UseVisualStyleBackColor = true;
      // 
      // chkVisible
      // 
      this.chkVisible.AutoSize = true;
      this.chkVisible.Location = new System.Drawing.Point(6, 6);
      this.chkVisible.Name = "chkVisible";
      this.chkVisible.Size = new System.Drawing.Size(56, 17);
      this.chkVisible.TabIndex = 0;
      this.chkVisible.Text = "Visible";
      this.chkVisible.UseVisualStyleBackColor = true;
      this.chkVisible.CheckedChanged += new System.EventHandler(this.chkVisible_CheckedChanged);
      // 
      // tpgSymbol
      // 
      this.tpgSymbol.Controls.Add(this.btnDown);
      this.tpgSymbol.Controls.Add(this.btnUp);
      this.tpgSymbol.Controls.Add(this.grdSymbols);
      this.tpgSymbol.Location = new System.Drawing.Point(4, 22);
      this.tpgSymbol.Name = "tpgSymbol";
      this.tpgSymbol.Size = new System.Drawing.Size(328, 247);
      this.tpgSymbol.TabIndex = 0;
      this.tpgSymbol.Text = "Symology";
      this.tpgSymbol.UseVisualStyleBackColor = true;
      // 
      // btnDown
      // 
      this.btnDown.Image = ((System.Drawing.Image)(resources.GetObject("btnDown.Image")));
      this.btnDown.Location = new System.Drawing.Point(296, 56);
      this.btnDown.Name = "btnDown";
      this.btnDown.Size = new System.Drawing.Size(24, 24);
      this.btnDown.TabIndex = 4;
      this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
      // 
      // btnUp
      // 
      this.btnUp.Image = ((System.Drawing.Image)(resources.GetObject("btnUp.Image")));
      this.btnUp.Location = new System.Drawing.Point(296, 32);
      this.btnUp.Name = "btnUp";
      this.btnUp.Size = new System.Drawing.Size(24, 23);
      this.btnUp.TabIndex = 3;
      this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
      // 
      // grdSymbols
      // 
      this.grdSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grdSymbols.Location = new System.Drawing.Point(8, 8);
      this.grdSymbols.Name = "grdSymbols";
      this.grdSymbols.RowHeadersWidth = 16;
      this.grdSymbols.Size = new System.Drawing.Size(280, 232);
      this.grdSymbols.TabIndex = 2;
      this.grdSymbols.DoubleClick += new System.EventHandler(this.grdSymbols_DoubleClick);
      this.grdSymbols.MouseUp += new System.Windows.Forms.MouseEventHandler(this.grdSymbols_MouseUp);
      // 
      // lblWhere
      // 
      this.lblWhere.AutoSize = true;
      this.lblWhere.Location = new System.Drawing.Point(6, 42);
      this.lblWhere.Name = "lblWhere";
      this.lblWhere.Size = new System.Drawing.Size(39, 13);
      this.lblWhere.TabIndex = 1;
      this.lblWhere.Text = "Where";
      // 
      // txtWhere
      // 
      this.txtWhere.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.txtWhere.Location = new System.Drawing.Point(51, 39);
      this.txtWhere.Name = "txtWhere";
      this.txtWhere.Size = new System.Drawing.Size(271, 20);
      this.txtWhere.TabIndex = 2;
      this.txtWhere.TextChanged += new System.EventHandler(this.txtWhere_TextChanged);
      // 
      // WdgProperties
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(336, 273);
      this.Controls.Add(this.tabDisplay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgProperties";
      this.ShowInTaskbar = false;
      this.Text = "Properties";
      this.VisibleChanged += new System.EventHandler(this.WdgProperties_VisibleChanged);
      this.tabDisplay.ResumeLayout(false);
      this.tpgDisplay.ResumeLayout(false);
      this.tpgDisplay.PerformLayout();
      this.tpgSymbol.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.grdSymbols)).EndInit();
      this.ResumeLayout(false);

    }
    #endregion

    private void Init()
    {
      DataGridViewColumn symCol = new DataGridViewColumn(new Div.DataGridViewSymbolCell());
      symCol.DataPropertyName = "Symbol";
      symCol.HeaderText = "Symbol";
      symCol.Width = 80;
      symCol.MinimumWidth = 60;
      grdSymbols.Columns.Add(symCol);

      DataGridViewTextBoxColumn txtCol = new DataGridViewTextBoxColumn();
      txtCol.DataPropertyName = "Condition";
      txtCol.HeaderText = "Condition";
      txtCol.Width = 120;
      txtCol.MinimumWidth = txtCol.Width;
      txtCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      grdSymbols.Columns.Add(txtCol);

      grdSymbols.AutoGenerateColumns = false;
    }

    public void SetData(MapData data)
    {
      if (_symbolTable != null)
      { _symbolTable.ColumnChanging -= Symbol_ColumnChanging; }

      _data = data;
      _rowLine = null;
      _symbolisation = null;
      _symbolTable = null;
      chkVisible.Checked = _data.Visible;
      txtWhere.ReadOnly = true;
      txtWhere.Text = "";

      grdSymbols.DataSource = null;

      if (_data is TableMapData)
      {
        TableMapData tblData = (TableMapData)_data;
        if (tblData.Data != null)
        {
          TTable tbl = tblData.Data;
          txtWhere.Text = tbl.Where;
          txtWhere.ReadOnly = false;
          _rowLine = tbl.FullSchema.Clone().NewRow();
          _rowLine[tblData.Symbolisation.GeometryColumn.ColumnName] = new Basics.Geom.Polyline();

          _symbolisation = tblData.Symbolisation;
          _symbolTable = (SymbolTable)_symbolisation.SymbolList.Table;

          grdSymbols.DataSource = _symbolisation.SymbolList;
          _symbolTable.ColumnChanging += Symbol_ColumnChanging;
        }
      }
      else if (_data is GridMapData)
      {
        GridMapData grData = (GridMapData)_data;
        if (grData.Data != null)
        {
          GridSymbolisation sym = new GridSymbolisation(grData);
          _symbolisation = sym;
          _symbolTable = (SymbolTable)sym.SymbolList.Table;
          grdSymbols.DataSource = sym.SymbolList;
          sym.SymbolList.Table.ColumnChanging += Symbol_ColumnChanging;
        }
      }
    }
    private class GridSymbolisation : ISymbolisationView
    {
      private readonly DataView _symbolList;
      public GridSymbolisation(GridMapData mapData)
      {
        SymbolTable symTbl = new SymbolTable();
        symTbl.AddRow(new Symbol(new SymbolPartGrid(mapData.Symbolisation)), "true", 1);
        _symbolList = new DataView(symTbl);
        _symbolList.AllowNew = false;
        _symbolList.AllowEdit = true;
        _symbolList.AllowDelete = false;
      }
      public DataView SymbolList
      {
        get { return _symbolList; }
      }
    }

    private void Symbol_ColumnChanging(object sender, DataColumnChangeEventArgs e)
    {
      if (e.Column != _symbolTable.Columns[SymbolTable.ConditionColumn.Name])
      { return; }

      SymbolTable.Row row = (SymbolTable.Row)e.Row;
      TableMapData grData = (TableMapData)_data;
      DataColumn geomColumn = grData.Data.DefaultGeometryColumn;
      try
      {
        row.InitConditionView(new DataView(geomColumn.Table), (string)e.ProposedValue, geomColumn);
      }
      catch (Exception exp)
      {
        string columns = Environment.NewLine;
        foreach (DataColumn col in geomColumn.Table.Columns)
        {
          columns += string.Format("{0} \t({1})" + Environment.NewLine, col.ColumnName, col.DataType);
        }
        columns = Environment.NewLine + "Available Columns:" + Environment.NewLine + columns;

        MessageBox.Show(exp.Message + Environment.NewLine + columns, "Invalid Condition",
          MessageBoxButtons.OK, MessageBoxIcon.Warning);
        e.ProposedValue = row.Condition;
      }
    }

    private void SetNavigation()
    {
      if (grdSymbols.SelectedRows.Count != 1)
      {
        btnUp.Enabled = false;
        btnDown.Enabled = false;
        return;
      }
      if (grdSymbols.SelectedRows[0].Index != 0)
      { btnUp.Enabled = true; }
      else
      { btnUp.Enabled = false; }
      if (grdSymbols.SelectedRows[grdSymbols.SelectedRows.Count - 1].Index < grdSymbols.Rows.Count - 2)
      { btnDown.Enabled = true; }
      else
      { btnDown.Enabled = false; }
    }

    private void OpenSymbol()
    {
      int iRow = grdSymbols.CurrentRow.Index;
      if (wdgSymbol == null)
      { wdgSymbol = new WdgSymbol(); }

      bool isNull = _symbolisation.SymbolList[iRow].Row.IsNull(SymbolTable.SymbolColumn.Name);
      Symbol sym;
      if (!isNull)
      {
        sym = SymbolTable.SymbolColumn.GetValue(_symbolisation.SymbolList[iRow]);
      }
      else
      {
        sym = GetDefaultSymbol();
        if (sym == null) { return; }
      }
      wdgSymbol.SetSymbol(sym);
      if (wdgSymbol.ShowDialog() == DialogResult.OK)
      {
        SymbolTable.SymbolColumn.SetValue(_symbolisation.SymbolList[iRow].Row, wdgSymbol.NewSymbol);
      }
      grdSymbols.Invalidate();
    }

    private Symbol GetDefaultSymbol()
    {
      Symbol sym;
      Type t = ((TableMapData)_data).Data.DefaultGeometryColumn.DataType;
      if (typeof(Basics.Geom.IPoint).IsAssignableFrom(t))
      { sym = Symbol.DefaultPoint(_rowLine); }
      else if (t == typeof(Basics.Geom.Polyline))
      { sym = Symbol.Create(_rowLine, 1); }
      else if (t == typeof(Basics.Geom.Area))
      { sym = Symbol.Create(_rowLine, 2); }
      else
      {
        DataTable tbl = new DataTable();
        tbl.Columns.Add("Name", typeof(string));
        tbl.Columns.Add("Topology", typeof(int));
        tbl.Rows.Add("Point", 0);
        tbl.Rows.Add("Line", 1);
        tbl.Rows.Add("Area", 2);
        tbl.AcceptChanges();
        WdgAuswahl wdg = new WdgAuswahl();
        wdg.SetData("Symboltyp", "Symboltyp", tbl.DefaultView, "Name");
        if (wdg.ShowDialog() != DialogResult.OK)
        { return null; }
        int topology = (int)wdg.SelectedRow["Topology"];

        sym = Symbol.Create(_rowLine, topology);
      }
      return sym;
    }

    private bool[] GetSelectedRows()
    {
      int nRows = _symbolisation.SymbolList.Count;
      bool[] bSel = new bool[nRows];
      foreach (DataGridViewRow row in grdSymbols.SelectedRows)
      { bSel[row.Index] = true; }
      return bSel;
    }
    #region events

    private void grdSymbols_MouseUp(object sender, MouseEventArgs e)
    {
      try
      {
        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void grdSymbols_DoubleClick(object sender, EventArgs e)
    {
      try
      {
        OpenSymbol();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void btnUp_Click(object sender, EventArgs e)
    {
      try
      {
        DataView pSymbols = _symbolisation.SymbolList;
        bool[] bIsSelected = GetSelectedRows();

        if (grdSymbols.SelectedRows.Count > 0)
        {
          int idx = grdSymbols.SelectedRows[0].Index;

          SymbolTable.Row rowUp = (SymbolTable.Row)pSymbols[idx].Row;
          SymbolTable.Row rowDown = (SymbolTable.Row)pSymbols[idx - 1].Row;

          // Selection is lost here
          int pos = rowUp.Position;
          rowUp.Position = rowDown.Position;
          rowDown.Position = pos;

          grdSymbols.ClearSelection();
          grdSymbols.Rows[idx - 1].Selected = true;
        }

        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void btnDown_Click(object sender, EventArgs e)
    {
      try
      {
        DataView pSymbols = _symbolisation.SymbolList;
        bool[] bIsSelected = GetSelectedRows();

        if (grdSymbols.SelectedRows.Count > 0)
        {
          int idx = grdSymbols.SelectedRows[0].Index;

          SymbolTable.Row rowDown = (SymbolTable.Row)pSymbols[idx].Row;
          SymbolTable.Row rowUp = (SymbolTable.Row)pSymbols[idx + 1].Row;

          // Selection is lost here
          int pos = rowUp.Position;
          rowUp.Position = rowDown.Position;
          rowDown.Position = pos;

          grdSymbols.ClearSelection();
          grdSymbols.Rows[idx + 1].Selected = true;
        }

        // TODO: reset selection

        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }
    private void chkVisible_CheckedChanged(object sender, EventArgs e)
    {
      if (_data != null)
      { _data.Visible = chkVisible.Checked; }
    }

    private void WdgProperties_VisibleChanged(object sender, EventArgs e)
    {
      if (Visible == false)
      {
        if (_symbolTable != null)
        {
          _symbolTable.ColumnChanging -= Symbol_ColumnChanging; ;
        }
      }
    }

    #endregion

    private void txtWhere_TextChanged(object sender, EventArgs e)
    {
      TableMapData tblData = _data as TableMapData;
      if (tblData != null && tblData.Data != null)
      { tblData.Data.Where = txtWhere.Text; }
    }
  }
}
