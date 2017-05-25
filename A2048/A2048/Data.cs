
using System;
using System.Collections.Generic;

namespace A2048
{
  public class Grid
  {
    private enum Direction { None = 0, Up = 1, Down = 2, Left = 3, Right = 4 }
    private int _size;
    private List<List<int>> _values;
    private Random _r;
    private double _w2 = 15.0 / 16.0;

    private List<byte> _moves;

    public Grid()
    {
      _r = new Random((int)DateTime.Now.Ticks);
      Size = 4;
    }

    public int Size
    {
      get { return _size; }
      set
      {
        _size = value;
        Reset();
      }
    }

    public void Reset()
    { Reset(addValues: true); }

    private void Reset(bool addValues)
    {
      _values = new List<List<int>>(Size);
      _moves = new List<byte>();

      for (int iRow = 0; iRow < Size; iRow++)
      {

        List<int> row = new List<int>(Size);
        _values.Add(row);
        for (int iCol = 0; iCol < Size; iCol++)
        {
          row.Add(0);
        }
      }

      if (addValues)
      {
        AddValue(Direction.None);
        AddValue(Direction.None);
      }
    }

    private void Move(Direction dir, bool addValue = true)
    {
      IEnumerable<List<int[]>> cellsEnum;
      if (dir == Direction.Down) cellsEnum = MoveDownCells();
      else if (dir == Direction.Up) cellsEnum = MoveUpCells();
      else if (dir == Direction.Left) cellsEnum = MoveLeftCells();
      else if (dir == Direction.Right) cellsEnum = MoveRightCells();
      else throw new InvalidOperationException($"Invalid dir {dir}");

      bool changed = false;
      foreach (List<int[]> cells in cellsEnum)
      {
        List<int> comb = new List<int>();
        List<int> oldVals = new List<int>();
        foreach (int[] cell in cells)
        {
          int val = GetValue(cell[0], cell[1]);
          oldVals.Add(val);
          if (val > 1)
          { comb.Add(val); }
        }

        if (comb.Count == 0)
        { continue; }

        List<int> reduced = Reduce(comb);
        if (reduced.Count == oldVals.Count)
        { continue; }

        for (int iPos = 0; iPos < cells.Count; iPos++)
        {
          int newVal = iPos < reduced.Count ? reduced[iPos] : 0;
          if (newVal != oldVals[iPos])
          {
            int[] cell = cells[iPos];
            _values[cell[0]][cell[1]] = newVal;

            changed = true;
          }
        }
      }

      if (changed && addValue)
      {
        AddValue(dir);
      }
    }

    public void MoveUp()
    { Move(Direction.Up); }
    private IEnumerable<List<int[]>> MoveUpCells()
    {
      for (int iCol = 0; iCol < Size; iCol++)
      {
        List<int[]> cells = new List<int[]>();
        for (int iRow = 0; iRow < Size; iRow++)
        {
          cells.Add(new[] { iRow, iCol });
        }
        yield return cells;
      }
    }

    public void MoveDown()
    { Move(Direction.Down); }
    private IEnumerable<List<int[]>> MoveDownCells()
    {
      for (int iCol = 0; iCol < Size; iCol++)
      {
        List<int[]> cells = new List<int[]>();
        for (int iRow = Size - 1; iRow >= 0; iRow--)
        {
          cells.Add(new[] { iRow, iCol });
        }
        yield return cells;
      }
    }

    public void MoveLeft()
    { Move(Direction.Left); }
    private IEnumerable<List<int[]>> MoveLeftCells()
    {
      for (int iRow = 0; iRow < Size; iRow++)
      {
        List<int[]> cells = new List<int[]>();
        for (int iCol = 0; iCol < Size; iCol++)
        {
          cells.Add(new[] { iRow, iCol });
        }
        yield return cells;
      }
    }

    public void MoveRight()
    { Move(Direction.Right); }
    private IEnumerable<List<int[]>> MoveRightCells()
    {
      for (int iRow = 0; iRow < Size; iRow++)
      {
        List<int[]> cells = new List<int[]>();
        for (int iCol = Size - 1; iCol >= 0; iCol--)
        {
          cells.Add(new[] { iRow, iCol });
        }
        yield return cells;
      }
    }

    private List<int> Reduce(List<int> full)
    {
      List<int> reduced = new List<int>();
      int i = 0;
      while (i < full.Count)
      {
        if (i < full.Count - 1 && full[i] == full[i + 1])
        {
          reduced.Add(2 * full[i]);
          i++;
        }
        else
        {
          reduced.Add(full[i]);
        }
        i++;
      }
      return reduced;
    }

    private void AddValue(Direction dir)
    {
      List<int[]> freePos = new List<int[]>(Size * Size);
      for (int iRow = 0; iRow < Size; iRow++)
      {
        for (int iCol = 0; iCol < Size; iCol++)
        {
          int posValue = GetValue(iRow, iCol);
          if (posValue == 0)
          {
            freePos.Add(new[] { iRow, iCol });
          }
        }
      }
      int nPos = freePos.Count;
      int pos = _r.Next(nPos);

      int value = _r.NextDouble() <= _w2 ? 2 : 4;
      int[] cell = freePos[pos];

      AddValue(cell, value, dir);
    }

    private void AddValue(int[] cell, int value, Direction dir)
    {
      _values[cell[0]][cell[1]] = value;

      if (Size <= 4)
      {
        int move = cell[0] + 4 * cell[1];
        move = (value == 2 ? 0 : 1) + 2 * move;
        move = (dir == Direction.None ? 1 : 2 * ((int)dir - 1)) + 8 * move;
        _moves.Add((byte)move);
      }
    }

    public void Undo()
    {
      if (_moves.Count < 3)
      { return; }

      _moves.RemoveAt(_moves.Count - 1);
      foreach (int move in Replay()) { }
    }
    public IEnumerable<int> Replay()
    {
      List<byte> moves = _moves;
      Reset(addValues: false);

      foreach (int move in moves)
      {
        Direction dir;
        int value;
        int[] cell;

        int t = move;
        if (t % 2 == 1) dir = Direction.None;
        else dir = (Direction)((t % 8) / 2 + 1);
        t = move / 8;
        value = (t % 2 == 0) ? 2 : 4;

        t = t / 2;
        int iRow = t % 4;
        int iCol = t / 4;
        cell = new int[] { iRow, iCol };

        if (dir != Direction.None)
        {
          Move(dir, addValue: false);
        }
        AddValue(cell, value, dir);

        yield return move;
      }
    }

    public int GetValue(int iRow, int iCol)
    {
      return _values[iRow][iCol];
    }
  }
}