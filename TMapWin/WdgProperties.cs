using Basics.Data;
using Basics.Forms;
using System;
using System.Data;
using System.Windows.Forms;
using TData;
using TMap;

namespace TMapWin
{
  public partial class WdgProperties : Form
  {
    private MapData _data;
    private DataRow _rowLine;
    private ISymbolisationView _symbolisation;
    private SymbolTable _symbolTable;
    private WdgSymbol _wdgSymbol = null;

    BindingSource _bindingSource;
    /// <summary>
    /// Required designer variable.
    /// </summary>

    public WdgProperties()
    {
      InitializeComponent();

      _bindingSource = new BindingSource(components);

      Init();
    }

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
      if (_bindingSource.DataSource == null && data != null)
      {
        _bindingSource.DataSource = data;
        txtTransparency.Bind(x => x.Text, _bindingSource, nameof(data.Transparency),
            true, DataSourceUpdateMode.OnPropertyChanged);
      }
      else
      {
        _bindingSource.DataSource = data;
      }

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
      { sym = Symbol.DefaultPoint(); }
      else if (t == typeof(Basics.Geom.Polyline))
      { sym = Symbol.Create(1, withSymbolPart: true); }
      else if (t == typeof(Basics.Geom.Area))
      { sym = Symbol.Create(2, withSymbolPart: true); }
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

        sym = Symbol.Create(topology, withSymbolPart: true);
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
