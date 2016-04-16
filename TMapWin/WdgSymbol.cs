using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using TMap;

namespace TMapWin
{
  public partial class WdgSymbol : Form
  {
    private Symbol _inSymbol;
    private Symbol _newSymbol;

    private SymbolPartTable _tblParts;
    private DataView _vwParts;

    public WdgSymbol()
    {
      InitializeComponent();

      Init();
    }

    private void Init()
    {
      grdSymbolPart.AutoGenerateColumns = false;

      _tblParts = new SymbolPartTable();
      _tblParts.RowChanged += tblParts_RowChanged;

      DataGridViewColumn pSymCol = new DataGridViewColumn(new Div.DataGridViewSymbolCell());
      pSymCol.DataPropertyName = SymbolPartTable.PartColumn.Name;
      pSymCol.HeaderText = "SymbolPart";
      pSymCol.Width = 120;
      grdSymbolPart.Columns.Add(pSymCol);

      //Attribute aDs = new Attribute();
      //_tblPoints = aDs.TblPoints;

      //grdPoints.Columns.Clear();
      //grdPoints.AutoGenerateColumns = false;
      //DataGridViewTextBoxColumn colTxt = new DataGridViewTextBoxColumn();
      //colTxt.DataPropertyName = _tblPoints.XColumn.ColumnName;
      //colTxt.DefaultCellStyle.Format = "N2";
      //colTxt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      //colTxt.Width = 20;
      //colTxt.MinimumWidth = colTxt.Width;
      //colTxt.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      //grdPoints.Columns.Add(colTxt);

      //colTxt = new DataGridViewTextBoxColumn();
      //colTxt.DataPropertyName = _tblPoints.YColumn.ColumnName;
      //colTxt.DefaultCellStyle.Format = "N2";
      //colTxt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
      //colTxt.Width = 20;
      //colTxt.MinimumWidth = colTxt.Width;
      //colTxt.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      //grdPoints.Columns.Add(colTxt);

      //grdPoints.DataSource = _tblPoints;

      //_tblPoints.RowChanged += tblPoints_RowChanged;
      //_tblPoints.RowDeleted += tblPoints_RowChanged;

      //txtWidth.TextChanged += new EventHandler(txtWidth_TextChanged);
      //txtScale.TextChanged += new EventHandler(txtScale_TextChanged);
      //txtRotate.TextChanged += new EventHandler(txtRotate_TextChanged);
    }

    public void SetSymbol(Symbol symbol)
    {
      _inSymbol = symbol;
      pnlSymbol.Invalidate();

      if (_inSymbol == null)
      { return; }
      grdSymbolPart.DataSource = null;

      _tblParts.Clear();
      int iRow = 0;
      foreach (ISymbolPart part in _inSymbol)
      {
        _tblParts.AddRow(iRow, part);
        iRow++;
      }
      _tblParts.AcceptChanges();
      _vwParts = new DataView(_tblParts);

      grdSymbolPart.DataSource = _vwParts;

      SetSymbolPart(0);
    }

    public Symbol NewSymbol
    {
      get { return _newSymbol; }
    }

    private void SetSymbolPart(int index)
    {
      if (index < 0 || index >= _vwParts.Count)
      { return; }

      ISymbolPart symPart = SymbolPartTable.PartColumn.GetValue(_vwParts[index]);

      pgrSymbol.SelectedObject = symPart.EditProperties;
    }

    #region events
    void tblParts_RowChanged(object sender, DataRowChangeEventArgs e)
    {
      _newSymbol = new Symbol(_inSymbol.Topology);
      foreach (SymbolPartTable.Row row in _tblParts.Rows)
      {
        ISymbolPart p = row.Part;
        if (p != null)
        { _newSymbol.Add(p); }
      }
      pnlSymbol.Invalidate();
    }

    private void pnlSymbol_Paint(object sender, PaintEventArgs e)
    {
      e.Graphics.Clear(Color.White);

      if (_newSymbol == null)
      { return; }

      Div.TMapGraphics pGraphics = new Div.TMapGraphics(e.Graphics,
        new Rectangle(2, 2, pnlSymbol.Width - 4, pnlSymbol.Height - 4));
      _newSymbol.Draw(pGraphics);
    }

    private void pgrSymbol_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
      pnlSymbol.Invalidate();
      grdSymbolPart.InvalidateRow(grdSymbolPart.CurrentRow.Index);
    }

    private void grdSymbolPart_SelectionChanged(object sender, EventArgs e)
    {
      SetSymbolPart(grdSymbolPart.CurrentRow.Index);
    }

    private void btnApply_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    #endregion
  }
}