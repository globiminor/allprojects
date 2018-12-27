
using System.Collections.Generic;
using System.Text;

namespace Basics.Num
{
  public class SpareMatrix
  {
    private readonly int _nCol;

    private class Cell
    {
      public double V;
      public readonly int Col;

      public Cell(double v, int col)
      {
        V = v;
        Col = col;
      }
    }

    private class Row : LinkedList<Cell>
    {
      public string ToFullString(int nCol)
      {
        StringBuilder s = new StringBuilder();
        int iCol = 0;
        foreach (var cell in this)
        {
          while (iCol < cell.Col)
          {
            s.Append("0;");
            iCol++;
          }
          s.AppendFormat("{0};", cell.V);
          iCol++;
        }

        while (iCol < nCol)
        {
          s.Append("0;");
          iCol++;
        }
        return s.ToString();
      }
    }

    private Row[] _rows;

    private Row GetRow(int idx)
    {
      Row r = _rows[idx];
      if (r == null)
      {
        r = new Row();
        _rows[idx] = r;
      }
      return r;
    }

    public int NRow
    {
      get { return _rows.Length; }
    }

    public int NCol
    {
      get { return _nCol; }
    }

    public double this[int iRow, int iCol]
    {
      get
      {
        Row r = _rows[iRow];
        if (r == null)
        { return 0; }

        foreach (var cell in r)
        {
          int col = cell.Col;
          if (col > iCol)
          { return 0; }

          if (col == iCol)
          { return cell.V; }
        }

        return 0;
      }
      set
      {
        Row r = GetRow(iRow);

        for (LinkedListNode<Cell> n = r.First; n != null; n = n.Next)
        {
          Cell current = n.Value;
          int currentCol = current.Col;
          if (currentCol == iCol)
          {
            if (value == 0)
            { r.Remove(n); }
            else
            {
              current.V = value;
              return;
            }
          }

          if (currentCol > iCol)
          {
            if (value == 0)
            { return; }

            Cell v = new Cell(value, iCol);
            r.AddBefore(n, v);
            return;
          }
        }

        if (value == 0)
        { return; }

        {
          Cell cell = new Cell(value, iCol);
          r.AddLast(cell);
        }
      }
    }
    public SpareMatrix(int nRow, int nCol)
    {
      _rows = new Row[nRow];
      _nCol = nCol;
    }

    public override string ToString()
    {
      string s = string.Format("Matrix, Rows:{0}, Cols:{1}", NRow, NCol);
      return s;
    }
    public string ToFullString()
    {
      StringBuilder s = new StringBuilder(ToString());
      s.AppendLine();
      for (int iRow = 0; iRow < NRow; iRow++)
      {
        s.AppendLine(GetRow(iRow).ToFullString(NCol));
      }
      return s.ToString();
    }

    public SpareMatrix T()
    {
      SpareMatrix m1 = new SpareMatrix(NCol, NRow);

      for (int iR0 = 0; iR0 < NRow; iR0++)
      {
        Row r0 = _rows[iR0];
        if (r0 == null)
        { continue; }

        foreach (var cell0 in r0)
        {
          Row r1 = m1.GetRow(cell0.Col);
          r1.AddLast(new Cell(cell0.V, iR0));
        }
      }

      return m1;
    }

    void Scalar(double f)
    {
      foreach (var row in _rows)
      {
        foreach (var cell in row)
        {
          cell.V *= f;
        }
      }
    }

    public SpareMatrix Add(SpareMatrix m1)
    {
      if (NRow != m1.NRow || NCol != m1.NCol)
      {
        return null;
      }

      SpareMatrix m2 = new SpareMatrix(NRow, NCol);

      for (int i = 0; i < m1.NRow; i++)
      {
        Row row2 = m2.GetRow(i);
        IEnumerator<Cell> c0 = GetRow(i).GetEnumerator();
        IEnumerator<Cell> c1 = m1.GetRow(i).GetEnumerator();

        bool next0 = c0.MoveNext();
        bool next1 = c1.MoveNext();
        while (next0 || next1)
        {
          Cell cell0 = next0 ? c0.Current : null;
          Cell cell1 = next1 ? c1.Current : null;

          int cmp = (next0 ? cell0.Col : int.MaxValue).CompareTo(next1 ? cell1.Col : int.MaxValue);
          if (cmp < 0)
          {
            row2.AddLast(new Cell(cell0.V, cell0.Col));
            next0 = c0.MoveNext();
          }
          else if (cmp == 0)
          {
            row2.AddLast(new Cell(cell0.V + cell1.V, cell0.Col));
            next0 = c0.MoveNext();
            next1 = c1.MoveNext();
          }
          else
          {
            row2.AddLast(new Cell(cell1.V, cell1.Col));
            next1 = c1.MoveNext();
          }
        }
      }
      return m2;
    }

    /// <summary>
    /// multiplication of this  m1
    /// </summary>
    public SpareMatrix Mul(SpareMatrix m1)
    {
      SpareMatrix m0 = this;

      if (m0.NCol != m1.NRow)
      {
        return null;
      }

      SpareMatrix m2 = new SpareMatrix(m0.NRow, m1.NCol);

      int nRow0 = m0.NRow;

      for (int iRow0 = 0; iRow0 < nRow0; iRow0++)
      {
        Row r0 = m0._rows[iRow0];
        if (r0 == null)
        { continue; }

        foreach (var cell0 in r0)
        {
          int col0 = cell0.Col;

          Row r1 = m1._rows[col0];
          if (r1 == null)
          { continue; }

          Row r2 = m2.GetRow(iRow0);
          LinkedListNode<Cell> n2 = r2.First;

          double v0 = cell0.V;

          foreach (var cell1 in r1)
          {
            double v2 = v0 * cell1.V;
            int col1 = cell1.Col;

            while (n2 != null && n2.Value.Col < col1)
            {
              n2 = n2.Next;
            }

            if (n2 == null)
            {
              r2.AddLast(new Cell(v2, col1));
            }
            else if (n2.Value.Col > col1)
            {
              r2.AddBefore(n2, new Cell(v2, col1));
            }
            else if (n2.Value.Col == col1)
            {
              n2.Value.V += v2;
            }
          }
        }
      }

      return m2;
    }

    /// <summary>
    ///                       T
    /// multiplication of this  m1
    /// </summary>
    public SpareMatrix TMul(SpareMatrix m1)
    {
      SpareMatrix m0 = this;

      if (m0.NRow != m1.NRow)
      {
        return null;
      }

      SpareMatrix m2 = new SpareMatrix(m0.NCol, m1.NCol);

      int nRow0 = m0.NRow;

      for (int iRow0 = 0; iRow0 < nRow0; iRow0++)
      {
        Row r0 = m0._rows[iRow0];
        if (r0 == null)
        { continue; }

        Row r1 = m1._rows[iRow0];
        if (r1 == null)
        { continue; }

        foreach (var cell0 in r0)
        {
          int col0 = cell0.Col;

          Row r2 = m2.GetRow(col0);
          LinkedListNode<Cell> n2 = r2.First;

          double v0 = cell0.V;

          foreach (var cell1 in r1)
          {
            double v2 = v0 * cell1.V;
            int col1 = cell1.Col;

            while (n2 != null && n2.Value.Col < col1)
            {
              n2 = n2.Next;
            }

            if (n2 == null)
            {
              r2.AddLast(new Cell(v2, col1));
            }
            else if (n2.Value.Col > col1)
            {
              r2.AddBefore(n2, new Cell(v2, col1));
            }
            else if (n2.Value.Col == col1)
            {
              n2.Value.V += v2;
            }
          }
        }
      }

      return m2;
    }

    /// <summary>
    ///                           T
    /// multiplication of this  m1
    /// </summary>
    public SpareMatrix MulT(SpareMatrix m1)
    {
      SpareMatrix m0 = this;

      if (m0.NCol != m1.NRow)
      {
        return null;
      }

      int nRow0 = m0.NRow;
      int nRow1 = m1.NRow;

      SpareMatrix m2 = new SpareMatrix(nRow0, nRow1);

      for (int iRow0 = 0; iRow0 < nRow0; iRow0++)
      {
        Row r0 = m0._rows[iRow0];
        if (r0 == null)
        { continue; }

        Row r2 = m2.GetRow(iRow0);

        for (int iRow1 = 0; iRow1 < nRow1; iRow1++)
        {
          Row r1 = m1._rows[iRow1];
          if (r1 == null)
          { continue; }

          LinkedListNode<Cell> n0 = r0.First;
          LinkedListNode<Cell> n1 = r1.First;

          double v = 0;
          bool hasValue = false;
          while (n0 != null && n1 != null)
          {
            int col0 = n0.Value.Col;
            int col1 = n1.Value.Col;
            if (col0 < col1)
            {
              n0 = n0.Next;
            }
            else if (col0 == col1)
            {
              v += n0.Value.V * n1.Value.V;
              hasValue = true;
              n0 = n0.Next;
              n1 = n1.Next;
            }
            else
            {
              n1 = n1.Next;
            }
          }
          if (hasValue)
          {
            r2.AddLast(new Cell(v, iRow1));
          }
        }
      }

      return m2;
    }

    ///
    // iteration Gauss-Seidel
    public double ApproxSolve(IList<double> x)
    {
      double vv;
      vv = 0;

      for (int iRow = 0; iRow < NRow; iRow++)
      {
        double t = 0;
        double? ex = null;
        foreach (var cell in _rows[iRow])
        {
          int col = cell.Col;
          double v = cell.V;
          t += v * x[col];
          if (col == iRow && v != 0)
          { ex = v; }
        }
        if (ex.HasValue)
        {
          double v = t / ex.Value;
          x[iRow] = -v;
          vv += v * v;
        }
      }
      return vv;
    }
  }
}
