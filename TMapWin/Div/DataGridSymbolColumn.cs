using System.Drawing;
using System.Windows.Forms;
using TMap;

namespace TMapWin.Div
{
  public class DataGridViewSymbolCell : DataGridViewCell
  {
    private bool _unique;

    public bool Unique
    {
      get { return _unique; }
      set { _unique = value; }
    }

    protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle,
      int rowIndex, Size constraintSize)
    {
      Symbol symbol = GetSymbol(GetValue(rowIndex));
      Size Extents;
      if (symbol == null)
      { Extents = new Size(20, 12); }
      else
      {
        double dSize = symbol.Size();
        Extents = new Size((int)(2 * dSize + 2), (int)dSize + 2);
      }
      Extents.Width += 1;
      Extents.Height += 0;
      return Extents;
    }
    public override System.Type FormattedValueType
    {
      get
      {
        return typeof(object);
      }
    }
    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds,
      int rowIndex, DataGridViewElementStates cellState, object value,
      object formattedValue, string errorText,
      DataGridViewCellStyle cellStyle,
      DataGridViewAdvancedBorderStyle advancedBorderStyle,
      DataGridViewPaintParts paintParts)
    {
      Symbol symbol = GetSymbol(value);
      PaintSymbol(graphics, cellBounds, symbol, cellStyle.BackColor);
      PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
    }

    private void PaintSymbol(Graphics g, Rectangle bounds, Symbol symbol,
      Color backColor)
    {
      g.FillRectangle(new SolidBrush(backColor), bounds);
      if (symbol == null)
      { return; }

      IDrawable pDrawable = new TMapGraphics(g, bounds);
      symbol.Draw(pDrawable);
    }

    public override object Clone()
    {
      DataGridViewSymbolCell clone = new DataGridViewSymbolCell();
      return clone;
    }

    private Symbol GetSymbol(object value)
    {
      if (value is Symbol)
      { return value as Symbol; }
      else if (value is ISymbolPart part)
      {
        Symbol symbol = new Symbol(part);
        return symbol;
      }
      else
      { return null; }
    }

  }

  public class DataGridSymbolOColumn : DataGridColumnStyle
  {
    //
    // all cells in the column have the same data source

    //
    public DataGridSymbolOColumn()
    {
      Init();
    }

    private void Init()
    {
    }

    #region overridden DataGridColumnStyle
    //
    // Abort Changes
    //
    protected override void Abort(int RowNum)
    {
      EndEdit();
    }
    //
    // Commit Changes
    //
    protected override bool Commit(CurrencyManager DataSource, int RowNum)
    {
      EndEdit();
      return true;
    }

    //
    // Remove focus
    //
    protected override void ConcedeFocus()
    {
    }

    //
    // Edit Grid
    //
    protected override void Edit(CurrencyManager source, int rowNum,
      Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
    {
    }
    protected override int GetMinimumHeight()
    {
      return 12;
    }

    protected override int GetPreferredHeight(Graphics g, object value)
    {
      Symbol symbol = GetSymbol(value);
      int iHeight;
      if (symbol == null)
      { iHeight = 12; }
      else
      { iHeight = (int)symbol.Size() + 2; }
      return iHeight;
    }
    protected override Size GetPreferredSize(Graphics g, object value)
    {
      Symbol symbol = GetSymbol(value);
      Size Extents;
      if (symbol == null)
      { Extents = new Size(20, 12); }
      else
      {
        double dSize = symbol.Size();
        Extents = new Size((int)(2 * dSize + 2), (int)dSize + 2);
      }
      Extents.Width += DataGridTableGridLineWidth;
      Extents.Height += 0;
      return Extents;
    }
    protected override void Paint(Graphics g, Rectangle bounds,
      CurrencyManager source, int rowNum)
    {
      Symbol symbol = GetSymbol(GetColumnValueAtRow(source, rowNum));
      PaintSymbol(g, bounds, symbol, false);
    }
    protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source,
      int rowNum, bool alignToRight)
    {
      Symbol symbol = GetSymbol(GetColumnValueAtRow(source, rowNum));
      PaintSymbol(g, bounds, symbol, alignToRight);
    }
    protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source,
      int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
    {
      Symbol symbol = GetSymbol(GetColumnValueAtRow(source, rowNum));
      PaintSymbol(g, bounds, symbol, backBrush, foreBrush, alignToRight);
    }
    protected override void SetDataGridInColumn(DataGrid Value)
    {
      base.SetDataGridInColumn(Value);
    }
    protected override void UpdateUI(CurrencyManager Source, int rowNum, string InstantText)
    {
      //      object val = GetColumnValueAtRow(Source, rowNum);
      //      string txt = GetText(val);
      //      mCombo.Text = txt;
      //      if(InstantText!=null) 
      //      {
      //        mCombo.Text = InstantText;
      //      }
    }
    //    protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
    //    {
    //      mSource = source;
    //      return base.GetColumnValueAtRow (source, rowNum);
    //    }

    #endregion

    #region Helper Methods
    private Symbol GetSymbol(object value)
    {
      if (value is Symbol)
      { return value as Symbol; }
      else if (value is ISymbolPart part)
      {
        Symbol symbol = new Symbol(part);
        return symbol;
      }
      else
      { return null; }
    }
    private int DataGridTableGridLineWidth
    {
      get
      {
        if (DataGridTableStyle.GridLineStyle == DataGridLineStyle.Solid)
        { return 1; }
        else
        { return 0; }
      }
    }
    public void EndEdit()
    {
      Invalidate();
    }

    private void PaintSymbol(Graphics g, Rectangle bounds, Symbol symbol, bool alignToRight)
    {
      Brush pBackBrush = new SolidBrush(DataGridTableStyle.BackColor);
      Brush pForeBrush = new SolidBrush(DataGridTableStyle.ForeColor);
      PaintSymbol(g, bounds, symbol, pBackBrush, pForeBrush, alignToRight);
    }
    private void PaintSymbol(Graphics g, Rectangle bounds, Symbol symbol,
      Brush backBrush, Brush foreBrush, bool alignToRight)
    {
      g.FillRectangle(backBrush, bounds);
      if (symbol == null)
      { return; }

      IDrawable pDrawable = new TMapGraphics(g, bounds);
      symbol.Draw(pDrawable);
    }

    #endregion
  }
}
