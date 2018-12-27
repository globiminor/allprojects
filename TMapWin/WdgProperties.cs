using System;
using System.Data;
using System.Windows.Forms;
using TMap;
using TData;
using Basics.Data;
using Basics.Forms;

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
    private WdgSymbol _wdgSymbol = null;
    /// <summary>
    /// designer controls
    /// </summary>
    private TabControl _tabDisplay;
    private TabPage _tpgSymbol;
    private DataGridView _grdSymbols;
    private Button _btnUp;
    private Button _btnDown;
    private TabPage _tpgDisplay;
    private CheckBox _chkVisible;
    private TextBox _txtWhere;
    private Label _lblWhere;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container _components = null;

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
        if (_components != null)
        {
          _components.Dispose();
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
      this._tabDisplay = new System.Windows.Forms.TabControl();
      this._tpgDisplay = new System.Windows.Forms.TabPage();
      this._chkVisible = new System.Windows.Forms.CheckBox();
      this._tpgSymbol = new System.Windows.Forms.TabPage();
      this._btnDown = new System.Windows.Forms.Button();
      this._btnUp = new System.Windows.Forms.Button();
      this._grdSymbols = new System.Windows.Forms.DataGridView();
      this._lblWhere = new System.Windows.Forms.Label();
      this._txtWhere = new System.Windows.Forms.TextBox();
      this._tabDisplay.SuspendLayout();
      this._tpgDisplay.SuspendLayout();
      this._tpgSymbol.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this._grdSymbols)).BeginInit();
      this.SuspendLayout();
      // 
      // tabDisplay
      // 
      this._tabDisplay.Controls.Add(this._tpgDisplay);
      this._tabDisplay.Controls.Add(this._tpgSymbol);
      this._tabDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
      this._tabDisplay.Location = new System.Drawing.Point(0, 0);
      this._tabDisplay.Name = "tabDisplay";
      this._tabDisplay.SelectedIndex = 0;
      this._tabDisplay.Size = new System.Drawing.Size(336, 273);
      this._tabDisplay.TabIndex = 0;
      // 
      // tpgDisplay
      // 
      this._tpgDisplay.Controls.Add(this._txtWhere);
      this._tpgDisplay.Controls.Add(this._lblWhere);
      this._tpgDisplay.Controls.Add(this._chkVisible);
      this._tpgDisplay.Location = new System.Drawing.Point(4, 22);
      this._tpgDisplay.Name = "tpgDisplay";
      this._tpgDisplay.Padding = new System.Windows.Forms.Padding(3);
      this._tpgDisplay.Size = new System.Drawing.Size(328, 247);
      this._tpgDisplay.TabIndex = 1;
      this._tpgDisplay.Text = "Display";
      this._tpgDisplay.UseVisualStyleBackColor = true;
      // 
      // chkVisible
      // 
      this._chkVisible.AutoSize = true;
      this._chkVisible.Location = new System.Drawing.Point(6, 6);
      this._chkVisible.Name = "chkVisible";
      this._chkVisible.Size = new System.Drawing.Size(56, 17);
      this._chkVisible.TabIndex = 0;
      this._chkVisible.Text = "Visible";
      this._chkVisible.UseVisualStyleBackColor = true;
      this._chkVisible.CheckedChanged += new System.EventHandler(this.ChkVisible_CheckedChanged);
      // 
      // tpgSymbol
      // 
      this._tpgSymbol.Controls.Add(this._btnDown);
      this._tpgSymbol.Controls.Add(this._btnUp);
      this._tpgSymbol.Controls.Add(this._grdSymbols);
      this._tpgSymbol.Location = new System.Drawing.Point(4, 22);
      this._tpgSymbol.Name = "tpgSymbol";
      this._tpgSymbol.Size = new System.Drawing.Size(328, 247);
      this._tpgSymbol.TabIndex = 0;
      this._tpgSymbol.Text = "Symology";
      this._tpgSymbol.UseVisualStyleBackColor = true;
      // 
      // btnDown
      // 
      this._btnDown.Image = ((System.Drawing.Image)(resources.GetObject("btnDown.Image")));
      this._btnDown.Location = new System.Drawing.Point(296, 56);
      this._btnDown.Name = "btnDown";
      this._btnDown.Size = new System.Drawing.Size(24, 24);
      this._btnDown.TabIndex = 4;
      this._btnDown.Click += new System.EventHandler(this.BtnDown_Click);
      // 
      // btnUp
      // 
      this._btnUp.Image = ((System.Drawing.Image)(resources.GetObject("btnUp.Image")));
      this._btnUp.Location = new System.Drawing.Point(296, 32);
      this._btnUp.Name = "btnUp";
      this._btnUp.Size = new System.Drawing.Size(24, 23);
      this._btnUp.TabIndex = 3;
      this._btnUp.Click += new System.EventHandler(this.BtnUp_Click);
      // 
      // grdSymbols
      // 
      this._grdSymbols.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._grdSymbols.Location = new System.Drawing.Point(8, 8);
      this._grdSymbols.Name = "grdSymbols";
      this._grdSymbols.RowHeadersWidth = 16;
      this._grdSymbols.Size = new System.Drawing.Size(280, 232);
      this._grdSymbols.TabIndex = 2;
      this._grdSymbols.DoubleClick += new System.EventHandler(this.GrdSymbols_DoubleClick);
      this._grdSymbols.MouseUp += new System.Windows.Forms.MouseEventHandler(this.GrdSymbols_MouseUp);
      // 
      // lblWhere
      // 
      this._lblWhere.AutoSize = true;
      this._lblWhere.Location = new System.Drawing.Point(6, 42);
      this._lblWhere.Name = "lblWhere";
      this._lblWhere.Size = new System.Drawing.Size(39, 13);
      this._lblWhere.TabIndex = 1;
      this._lblWhere.Text = "Where";
      // 
      // txtWhere
      // 
      this._txtWhere.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtWhere.Location = new System.Drawing.Point(51, 39);
      this._txtWhere.Name = "txtWhere";
      this._txtWhere.Size = new System.Drawing.Size(271, 20);
      this._txtWhere.TabIndex = 2;
      this._txtWhere.TextChanged += new System.EventHandler(this.TxtWhere_TextChanged);
      // 
      // WdgProperties
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(336, 273);
      this.Controls.Add(this._tabDisplay);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Name = "WdgProperties";
      this.ShowInTaskbar = false;
      this.Text = "Properties";
      this.VisibleChanged += new System.EventHandler(this.WdgProperties_VisibleChanged);
      this._tabDisplay.ResumeLayout(false);
      this._tpgDisplay.ResumeLayout(false);
      this._tpgDisplay.PerformLayout();
      this._tpgSymbol.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this._grdSymbols)).EndInit();
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
      _grdSymbols.Columns.Add(symCol);

      DataGridViewTextBoxColumn txtCol = new DataGridViewTextBoxColumn();
      txtCol.DataPropertyName = "Condition";
      txtCol.HeaderText = "Condition";
      txtCol.Width = 120;
      txtCol.MinimumWidth = txtCol.Width;
      txtCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      _grdSymbols.Columns.Add(txtCol);

      _grdSymbols.AutoGenerateColumns = false;
    }

    public void SetData(MapData data)
    {
      if (_symbolTable != null)
      { _symbolTable.ColumnChanging -= Symbol_ColumnChanging; }

      _data = data;
      _rowLine = null;
      _symbolisation = null;
      _symbolTable = null;
      _chkVisible.Checked = _data.Visible;
      _txtWhere.ReadOnly = true;
      _txtWhere.Text = "";

      _grdSymbols.DataSource = null;

      if (_data is TableMapData tblData)
      {
        if (tblData.Data != null)
        {
          TTable tbl = tblData.Data;
          _txtWhere.Text = tbl.Where;
          _txtWhere.ReadOnly = false;
          _rowLine = tbl.FullSchema.Clone().NewRow();
          _rowLine[tblData.Symbolisation.GeometryColumn.ColumnName] = new Basics.Geom.Polyline();

          _symbolisation = tblData.Symbolisation;
          _symbolTable = (SymbolTable)_symbolisation.SymbolList.Table;

          _grdSymbols.DataSource = _symbolisation.SymbolList;
          _symbolTable.ColumnChanging += Symbol_ColumnChanging;
        }
      }
      else if (_data is GridMapData grData)
      {
        if (grData.Data != null)
        {
          GridSymbolisation sym = new GridSymbolisation(grData);
          _symbolisation = sym;
          _symbolTable = (SymbolTable)sym.SymbolList.Table;
          _grdSymbols.DataSource = sym.SymbolList;
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
        foreach (var col in geomColumn.Table.Columns.Enum())
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
      if (_grdSymbols.SelectedRows.Count != 1)
      {
        _btnUp.Enabled = false;
        _btnDown.Enabled = false;
        return;
      }
      if (_grdSymbols.SelectedRows[0].Index != 0)
      { _btnUp.Enabled = true; }
      else
      { _btnUp.Enabled = false; }
      if (_grdSymbols.SelectedRows[_grdSymbols.SelectedRows.Count - 1].Index < _grdSymbols.Rows.Count - 2)
      { _btnDown.Enabled = true; }
      else
      { _btnDown.Enabled = false; }
    }

    private void OpenSymbol()
    {
      int iRow = _grdSymbols.CurrentRow.Index;
      if (_wdgSymbol == null)
      { _wdgSymbol = new WdgSymbol(); }

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
      _wdgSymbol.SetSymbol(sym);
      if (_wdgSymbol.ShowDialog() == DialogResult.OK)
      {
        SymbolTable.SymbolColumn.SetValue(_symbolisation.SymbolList[iRow].Row, _wdgSymbol.NewSymbol);
      }
      _grdSymbols.Invalidate();
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
      foreach (var row in _grdSymbols.SelectedRows.Enum())
      { bSel[row.Index] = true; }
      return bSel;
    }
    #region events

    private void GrdSymbols_MouseUp(object sender, MouseEventArgs e)
    {
      try
      {
        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void GrdSymbols_DoubleClick(object sender, EventArgs e)
    {
      try
      {
        OpenSymbol();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void BtnUp_Click(object sender, EventArgs e)
    {
      try
      {
        DataView pSymbols = _symbolisation.SymbolList;
        bool[] bIsSelected = GetSelectedRows();

        if (_grdSymbols.SelectedRows.Count > 0)
        {
          int idx = _grdSymbols.SelectedRows[0].Index;

          SymbolTable.Row rowUp = (SymbolTable.Row)pSymbols[idx].Row;
          SymbolTable.Row rowDown = (SymbolTable.Row)pSymbols[idx - 1].Row;

          // Selection is lost here
          int pos = rowUp.Position;
          rowUp.Position = rowDown.Position;
          rowDown.Position = pos;

          _grdSymbols.ClearSelection();
          _grdSymbols.Rows[idx - 1].Selected = true;
        }

        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }

    private void BtnDown_Click(object sender, EventArgs e)
    {
      try
      {
        DataView pSymbols = _symbolisation.SymbolList;
        bool[] bIsSelected = GetSelectedRows();

        if (_grdSymbols.SelectedRows.Count > 0)
        {
          int idx = _grdSymbols.SelectedRows[0].Index;

          SymbolTable.Row rowDown = (SymbolTable.Row)pSymbols[idx].Row;
          SymbolTable.Row rowUp = (SymbolTable.Row)pSymbols[idx + 1].Row;

          // Selection is lost here
          int pos = rowUp.Position;
          rowUp.Position = rowDown.Position;
          rowDown.Position = pos;

          _grdSymbols.ClearSelection();
          _grdSymbols.Rows[idx + 1].Selected = true;
        }

        // TODO: reset selection

        SetNavigation();
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace, "Error"); }
    }
    private void ChkVisible_CheckedChanged(object sender, EventArgs e)
    {
      if (_data != null)
      { _data.Visible = _chkVisible.Checked; }
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

    private void TxtWhere_TextChanged(object sender, EventArgs e)
    {
      if (_data is TableMapData tblData && tblData.Data != null)
      { tblData.Data.Where = _txtWhere.Text; }
    }
  }
}
