using System;
using System.Collections.Generic;
using System.Diagnostics;
using Basics.Geom;
using PntOp = Basics.Geom.PointOperator;

namespace Dhm
{
  public class ContourSorter
  {
    public enum Progress
    {
      FallDirAssigned = 1,
      HillAssigned = 2,
      NeighborhoodDerived = 3,
      ParallelAssigned = 4,
      LoopUpChecked = 5,
      LoopDownChecked = 6,
      HeightAssigned = 7,

      Error = -1,
      InvalidReverse = -2,
      ErrorFallDir = -3,
      Intersection = -4
    }

    public delegate void ProgressHandler(object sender, ProgressEventArgs args);
    public class ProgressEventArgs : EventArgs
    {
      public ProgressEventArgs(Progress progress, Contour contour)
      {
        Contour = contour;
        Progress = progress;

        Cancel = false;
      }

      public Progress Progress { get; }

      public Contour Contour { get; }

      public bool Cancel { get; set; }
    }

    private class HeightComparer : IComparer<KeyValuePair<Contour, NeighborInfo>>
    {
      public int Compare(KeyValuePair<Contour, NeighborInfo> x,
        KeyValuePair<Contour, NeighborInfo> y)
      {
        return x.Value.Height.CompareTo(y.Value.Height);
      }
    }

    private List<ContourType> _contourTypeList;
    private double _fuzzy = 0.1;

    public event ProgressHandler ProgressChanged;

    public double Fuzzy
    {
      get { return _fuzzy; }
      set { _fuzzy = value; }
    }

    private Mesh _mesh;
    private IList<Contour> _contours;

    private IEnumerable<IGeometry> GetGeometries(IEnumerable<Contour> contours)
    {
      foreach (var contour in contours)
      {
        yield return contour.Polyline;
      }
    }
    public Progress Execute(IList<Contour> rawContours, List<FallDir> fallDirs)
    {
      _contours = rawContours;

      Mesh mesh = new Mesh();
      mesh.InitSize(GetGeometries(rawContours));
      mesh.Fuzzy = _fuzzy;
      _mesh = mesh;

      _contourTypeList = AddElements(mesh, rawContours);
      _contourTypeList.Sort();

      foreach (var pnt in mesh.Points(null))
      {
        int n = 0;
        foreach (var line in mesh.GetLinesAt(pnt))
        {
          if (line.GetTag(out bool reverse) != null)
          {
            n++;
          }
        }
        if (n > 2)
        {
          //throw new InvalidOperationException("Intersection at " + pnt.Point);
        }
      }

      List<Contour> contours = Extract(mesh);

      _contours = contours;

      mesh = new Mesh();
      mesh.InitSize(GetGeometries(_contours));
      _mesh = mesh;
      mesh.Fuzzy = _fuzzy;

      AddElements(mesh, contours);

      AssignFallDirs(mesh, fallDirs);
      AssignHills(contours);
      GetNeighborhood(mesh);
      AssignParallel(contours);
      if (CheckLoops(contours, true) > 0)
      { return Progress.LoopUpChecked - 1; }

      if (CheckLoops(contours, false) > 0)
      { return Progress.LoopDownChecked - 1; }

      AssignHeights(contours);
      return Progress.HeightAssigned;
    }

    public Mesh Mesh
    {
      get { return _mesh; }
    }
    public IList<Contour> Contours
    {
      get { return _contours; }
    }
    protected void OnProgressChanged(Progress progress, Contour contour)
    {
      if (ProgressChanged != null)
      { OnProgressChanged(new ProgressEventArgs(progress, contour)); }
    }
    protected void OnProgressChanged(ProgressEventArgs args)
    {
      ProgressChanged?.Invoke(this, args);
    }

    private void AssignHeights(IList<Contour> contours)
    {
      Contour startContour;
      int iType = 0;
      do
      {
        startContour = MaxContour(contours, _contourTypeList[iType]);
        iType++;
      } while (startContour == null);

      startContour.HeightIndex = contours.Count * _contourTypeList[0].Intervall;
      startContour.MaxHeightIndex = startContour.HeightIndex;
      startContour.MinHeightIndex = startContour.HeightIndex;

      Contour c = GetNextHeight(contours);
      while (c != null)
      {
        OnProgressChanged(Progress.HeightAssigned, c);

        c = GetNextHeight(contours);

        //ExportOcd(Hk.Result + i + ".ocd", HeightAssignedContours(contours));
        //ExportShp(Hk.Result + i + ".shp", HeightAssignedContours(contours));
      }
    }

    private Contour GetNextHeight(IList<Contour> contours)
    {
      Contour next = GetNextHeightInter(contours);
      if (next != null)
      { return next; }

      next = GetNextHeightExtra(contours);
      return next;
    }

    private class Nb
    {
      public Contour Neighbor { get; }
      public NeighborInfo Info { get; }
      public Nb(Contour neighbor, NeighborInfo info)
      {
        Neighbor = neighbor;
        Info = info;
      }
    }
    private Contour GetNextHeightExtra(IList<Contour> contours)
    {
      // prepare unassigned contours
      Dictionary<Contour, IList<Nb>> unassigned =
        new Dictionary<Contour, IList<Nb>>();
      foreach (var contour in contours)
      {
        foreach (var pair in contour.Neighbors)
        {
          if (pair.Key.HeightIndex.HasValue == false)
          {
            if (unassigned.TryGetValue(pair.Key, out IList<Nb> neighbors) == false)
            {
              neighbors = new List<Nb>();
              unassigned.Add(pair.Key, neighbors);
            }
            neighbors.Add(new Nb(contour, pair.Value));
          }
        }
      }

      if (unassigned.Count == 0)
      { return null; }

      // iterate all unassigned contours 
      Contour maxSingleCnt = null;
      double maxSingleW = 0;
      int singleH = 0;

      Contour maxMultiCnt = null;
      double maxMultiW = 0;
      int multiH = 0;
      foreach (var pair in unassigned)
      {
        SortedList<int, double> hList = new SortedList<int, double>();
        Contour ua = pair.Key;
        foreach (var nb in pair.Value)
        {
          Contour cn = nb.Neighbor;
          NeighborInfo cn2Ua = nb.Info;

          if (cn.Orientation == Orientation.Unknown)
          { continue; }
          if (cn.HeightIndex.HasValue == false)
          { continue; }

          int dh;
          if ((cn.Orientation == Orientation.LeftSideDown) == cn2Ua.LeftSide)
          { dh = -1; }
          else
          { dh = 1; }

          // remark : "down" not needed if (dh != 0)
          int h = NeighborInfo.NextHeight(cn.HeightIndex.Value, dh, false, cn.Type, ua.Type);
          if (hList.TryGetValue(h, out double w) == false)
          { hList.Add(h, 0); }
          hList[h] += Math.Abs(cn2Ua.Weight);
        }

        // try find max value
        foreach (var hPair in hList)
        {
          int h = hPair.Key;
          double w = hPair.Value;

          if (hList.Count == 1)
          {
            if (w > maxSingleW)
            {
              maxSingleCnt = ua;
              maxSingleW = w;
              singleH = h;
            }
          }
          else
          {
            if (w > maxMultiW)
            {
              maxMultiCnt = ua;
              maxMultiW = w;
              multiH = h;
            }

          }
        }
      }

      if (maxSingleCnt != null)
      {
        maxSingleCnt.HeightIndex = singleH;
        return maxSingleCnt;
      }

      if (maxMultiCnt != null)
      {
        maxMultiCnt.HeightIndex = multiH;
        return maxMultiCnt;
      }

      return null;
    }


    private Contour GetNextHeightInter(IList<Contour> contours)
    {
      double maxSingleWeight = 0;
      Contour maxSingleContour = null;
      int maxSingleH = 0;

      double maxMultiWeight = 0;
      Contour maxMultiContour = null;
      int maxMultiH = 0;

      HeightComparer cmpr = new HeightComparer();

      foreach (var contour in contours)
      {
        if (contour.MaxHeightIndex != null &&
          contour.MaxHeightIndex == contour.MinHeightIndex)
        {
          contour.HeightIndex = contour.MaxHeightIndex;
          continue;
        }

        bool incomplete = false;
        List<KeyValuePair<Contour, NeighborInfo>> checkNb = new List<KeyValuePair<Contour, NeighborInfo>>();

        foreach (var pair in contour.Neighbors)
        {
          Contour neighbor = pair.Key;
          if (neighbor.HeightIndex == null)
          { incomplete = true; }
          else if (neighbor.Neighbors.TryGetValue(contour, out NeighborInfo nb2Ct))
          {
            NeighborInfo ct2Nb = pair.Value;
            if (contour.Orientation == Orientation.Unknown ||
              neighbor.Orientation == Orientation.Unknown)
            {
              continue;
            }
            ct2Nb.CalcHeight(contour, neighbor, nb2Ct);
            checkNb.Add(pair);
          }
        }

        if (checkNb.Count == 0)
        { continue; }

        checkNb.Sort(cmpr);

        List<double[]> hList = new List<double[]>();
        int h0 = checkNb[0].Value.Height;
        double w = 0;

        foreach (var pair in checkNb)
        {
          if (pair.Value.Height != h0)
          {
            hList.Add(new[] { h0, w });
            h0 = pair.Value.Height;
          }

          w += Math.Abs(pair.Value.Weight);
          if (pair.Value.MaxHeight != null &&
            (contour.MaxHeightIndex == null ||
            contour.MaxHeightIndex > pair.Value.MaxHeight))
          { contour.MaxHeightIndex = pair.Value.MaxHeight; }

          if (pair.Value.MinHeight != null &&
            (contour.MinHeightIndex == null ||
            contour.MinHeightIndex < pair.Value.MinHeight))
          { contour.MinHeightIndex = pair.Value.MinHeight; }
        }

        hList.Add(new[] { h0, w });

        if (contour.MaxHeightIndex != null &&
          contour.MinHeightIndex != null)
        {
          if (!(contour.MaxHeightIndex >= contour.MinHeightIndex))
          {
            IBox b = contour.Polyline.Extent;
            string msg = string.Format("Max Height Idx < Min Height Idx ({0} < {1})" +
              Environment.NewLine + "Contour Extent: {2} {3}",
              contour.MaxHeightIndex, contour.MinHeightIndex, b.Min, b.Max);
            OnProgressChanged(new ProgressEventArgs(Progress.Error, contour));
            //throw new InvalidProgramException(msg);
          }
          if (contour.MaxHeightIndex == contour.MinHeightIndex &&
            incomplete == false)
          {
            Debug.Assert(contour.HeightIndex == null ||
              contour.HeightIndex == contour.MinHeightIndex);

            if (contour.HeightIndex == null)
            {
              contour.HeightIndex = contour.MinHeightIndex;
              OnProgressChanged(new ProgressEventArgs(Progress.HeightAssigned, contour));
            }
            continue;
          }
        }

        if (contour.MinHeightIndex != null)
        {
          while (hList.Count > 0 && hList[0][0] < contour.MinHeightIndex)
          { hList.RemoveAt(0); }
        }

        if (contour.MaxHeightIndex != null)
        {
          while (hList.Count > 0 &&
            hList[hList.Count - 1][0] > contour.MaxHeightIndex)
          { hList.RemoveAt(hList.Count - 1); }
        }

        if (hList.Count == 1 && hList[0][1] > maxSingleWeight &&
          hList[0][0] != contour.HeightIndex)
        {
          maxSingleWeight = hList[0][1];
          maxSingleContour = contour;
          maxSingleH = (int)hList[0][0];
        }

        foreach (var pair in hList)
        {
          if (pair[1] > maxMultiWeight &&
            pair[0] != contour.HeightIndex)
          {
            maxMultiWeight = pair[1];
            maxMultiContour = contour;
            maxMultiH = (int)pair[0];
          }
        }
      }

      Contour maxContour = null;
      if (maxSingleContour != null)
      {
        maxContour = maxSingleContour;
        maxContour.HeightIndex = maxSingleH;
      }
      else if (maxMultiContour != null)
      {
        maxContour = maxMultiContour;
        maxContour.HeightIndex = maxMultiH;
      }

      return maxContour;
    }

    private Contour MaxContour(IList<Contour> contours, ContourType type)
    {
      Contour maxContour = null;
      double maxWeight = 0;
      foreach (var contour in contours)
      {
        if (contour.Orientation == Orientation.Unknown ||
          contour.Type != type)
        { continue; }

        foreach (var pair in contour.Neighbors)
        {
          NeighborInfo info = pair.Value;

          if (Math.Abs(info.Weight) > maxWeight)
          {
            maxWeight = Math.Abs(info.Weight);
            maxContour = contour;
          }
        }
      }
      return maxContour;
    }

    private int CheckLoops(List<Contour> contours, bool up)
    {
      foreach (var contour in contours)
      {
        contour.LoopChecked = Contour.LoopState.Unknown;
      }

      int loops = 0;
      foreach (var contour in contours)
      {
        if (contour.Orientation == Orientation.Unknown ||
          contour.LoopChecked != Contour.LoopState.Unknown)
        { continue; }

        HasLoops(contour, up, ref loops);
      }
      return loops;
    }

    private bool HasLoops(Contour contour, bool up, ref int loops)
    {
      if (contour.LoopChecked == Contour.LoopState.Checking)
      {
        contour.LoopChecked = Contour.LoopState.Loop;
        loops++;
        return true;
      }
      if (contour.LoopChecked != Contour.LoopState.Unknown)
      {
        return false;
      }

      if (contour.Orientation == Orientation.Unknown)
      { throw new InvalidProgramException("Orientation not set"); }

      contour.LoopChecked = Contour.LoopState.Checking;
      bool checkLeft = ((contour.Orientation == Orientation.LeftSideDown) != up);
      foreach (var pair in contour.Neighbors)
      {
        Contour neighbor = pair.Key;
        if (neighbor.Orientation == Orientation.Unknown)
        { continue; }
        if (pair.Value.LeftSide != checkLeft)
        { continue; }

        if (neighbor.Neighbors.TryGetValue(contour, out NeighborInfo info) == false)
        { continue; }
        if (((neighbor.Orientation == Orientation.LeftSideDown) == info.LeftSide) != up)
        { continue; }

        if (HasLoops(neighbor, up, ref loops))
        {
          if (contour.LoopChecked != Contour.LoopState.Checking)
          {
            if ((contour.LoopChecked == Contour.LoopState.Loop) == false)
            { throw new InvalidProgramException("Invalid value for LoopChecked"); }

            return false;
          }
          contour.LoopChecked = Contour.LoopState.Loop;
          return true;
        }
      }
      contour.LoopChecked = Contour.LoopState.NoLoop;
      return false;
    }

    private void AssignParallel(List<Contour> contours)
    {
      Contour contour = FindMaxParallel(contours, out double length);

      while (contour != null)
      {
        if (contour.Orientation != Orientation.Unknown)
        { throw new InvalidProgramException("Orientation already set"); }

        if (length > 0)
        { contour.Orientation = Orientation.LeftSideDown; }
        else
        { contour.Orientation = Orientation.RightSideDown; }

        OnProgressChanged(Progress.ParallelAssigned, contour);
        contour = FindMaxParallel(contours, out length);
      }
    }

    private Contour FindMaxParallel(List<Contour> contours, out double maxLength)
    {
      maxLength = 0;
      Contour maxContour = null;

      foreach (var contour in contours)
      {
        if (contour.Orientation != Orientation.Unknown)
        { continue; }
        double lPos = 0;
        double lNeg = 0;

        foreach (var pair in contour.Neighbors)
        {
          if (pair.Key.Orientation == Orientation.Unknown)
          { continue; }

          double w = pair.Value.Weight;
          if ((pair.Key.Orientation == Orientation.LeftSideDown) == (w > 0))
          { lPos += Math.Abs(w); }
          else
          { lNeg -= Math.Abs(w); }
        }

        double length = 0;
        if (lPos != 0 || lNeg != 0)
        { length = (lPos + lNeg) / (lPos - lNeg) * Math.Abs(lPos + lNeg); }

        if (Math.Abs(length) > Math.Abs(maxLength))
        {
          maxLength = length;
          maxContour = contour;
        }
      }

      return maxContour;
    }

    private void GetNeighborhood(Mesh mesh)
    {
      foreach (var contour in _contours)
      { contour.Neighbors.Clear(); }

      foreach (var line in mesh.Lines(null))
      {
        if (!(line.GetTag(out bool isReverse) is Contour contour))
        { continue; }

        if (line.LeftTri == null || line.RightTri == null)
        { continue; }

        Point end = Point.CastOrCreate(line.End);
        Point2D p0 = (Point2D)(end - line.Start);
        Mesh.MeshLine lLeft = -(line.GetNextTriLine());
        Mesh.MeshLine lRight = -((-line).GetNextTriLine());

        if (lLeft.GetTag(out bool t) != null)
        { continue; }
        if (lRight.GetTag(out t) != null)
        { continue; }

        if (GetContour(lLeft, out Contour cLeft, out bool firstReverseLeft) == false)
        { continue; }
        if (cLeft == contour)
        { continue; }

        if (GetContour(lRight, out Contour cRight, out bool firstReverseRight) == false)
        { continue; }
        if (cRight == contour)
        { continue; }

        if (cRight == cLeft)
        { //TODO: mark as warning
          continue;
        }

        //if (contour.Id == 50 && (cLeft.Id == 48 || cRight.Id == 48))
        //{ }
        double l = line.Length();
        double fLeft = p0.VectorProduct(PntOp.Sub(lLeft.Start, line.Start));
        double fRight = p0.VectorProduct(PntOp.Sub(lRight.Start, line.Start));
        double fPara = 1 - 2 * Math.Abs(0.5 - (fLeft / (fLeft - fRight)));
        fPara *= fPara;

        if (contour.Neighbors.TryGetValue(cLeft, out NeighborInfo values) == false)
        {
          values = new NeighborInfo();
          contour.Neighbors.Add(cLeft, values);
        }
        values.Length += l;
        if (isReverse == firstReverseLeft)
        { values.AddWeight(+l * fPara, !isReverse); }
        else
        { values.AddWeight(-l * fPara, !isReverse); }

        if (contour.Neighbors.TryGetValue(cRight, out values) == false)
        {
          values = new NeighborInfo();
          contour.Neighbors.Add(cRight, values);
        }
        values.Length += l;
        if (isReverse == firstReverseRight)
        { values.AddWeight(-l * fPara, isReverse); }
        else
        { values.AddWeight(+l * fPara, isReverse); }
      }
    }

    private bool GetContour(Mesh.MeshLine line, out Contour contour, out bool firstReverse)
    {
      Mesh.MeshLine l = line.GetNextPointLine();
      contour = null;
      firstReverse = false;
      int i = 0;

      while (l.HasEqualBaseLine(line) == false)
      {
        if (l.GetTag(out bool isReverse) is Contour tag)
        {
          if (contour == null)
          {
            contour = tag;
            firstReverse = isReverse;
          }
          else
          {
            if (contour != tag)
            { return false; }
            if (firstReverse == isReverse)
            {
              OnProgressChanged(Progress.InvalidReverse, contour);
              //  throw new InvalidOperationException("Error in software design assumption");
            }
          }
          i++;
        }
        l = l.GetNextPointLine();
      }
      return (i == 2);
    }

    public List<Contour> LoopContours(IList<Contour> contours)
    {
      List<Contour> selList = new List<Contour>();
      foreach (var contour in contours)
      {
        if (contour.Orientation != Orientation.Unknown &&
          contour.LoopChecked == Contour.LoopState.Loop)
        {
          selList.Add(contour);
        }
      }
      return selList;
    }

    public List<Contour> LeftSideAssignedContours(IList<Contour> contours)
    {
      List<Contour> selList = new List<Contour>();
      foreach (var contour in contours)
      {
        if (contour.Orientation != Orientation.Unknown)
        {
          selList.Add(contour);
        }
      }
      return selList;
    }

    public List<Contour> HeightAssignedContours(IList<Contour> contours)
    {
      List<Contour> selList = new List<Contour>();
      foreach (var contour in contours)
      {
        if (contour.HeightIndex != null)
        {
          selList.Add(contour);
        }
      }
      return selList;
    }

    private List<ContourType> AddElements(Mesh mesh, IList<Contour> contourList)
    {
      List<ContourType> typeList = new List<ContourType>();
      foreach (var contour in contourList)
      {
        if (typeList.Contains(contour.Type) == false)
        { typeList.Add(contour.Type); }

        Polyline line = contour.Polyline;
        mesh.Add(line.Points.First.Value);
      }
      foreach (var contour in contourList)
      {
        Polyline line = contour.Polyline;
        mesh.Add(line.Points.Last.Value);
      }

      foreach (var contour in contourList)
      {
        Polyline line = contour.Polyline;
        Mesh.MeshPoint p0 = null;
        foreach (var p1 in line.Points)
        {
          //OnProgressChanged(null);
          p0 = mesh.Add(p1, p0);
        }
      }

      //OnProgressChanged(null);
      int i = 0;
      foreach (var contour in contourList)
      {
        Polyline line = contour.Polyline;
        mesh.Add(line, contour, NewPoint);
        i++;
        //OnProgressChanged(null);
      }
      //OnProgressChanged(null);

      return typeList;
    }

    private void AssignFallDirs(Mesh mesh, List<FallDir> fallDirs)
    {
      foreach (var fallDir in fallDirs)
      {
        bool isReverse;
        Contour contour;

        IPoint fallPoint = fallDir.Point;
        Mesh.MeshLine l0 = mesh.FindLine(fallPoint);
        if (l0.LeftTri == null)
        {
          ErrorFallDir(fallDir);
          continue;
        }
        Mesh.Tri tri = l0.LeftTri;

        double d20 = 2.0;
        l0 = null;
        for (int i = 0; i < 3; i++)
        {
          Mesh.MeshLine lp = tri[i];
          Mesh.MeshLine l = lp;
          do
          {
            contour = l.GetTag(out isReverse) as Contour;
            if (contour != null)
            {
              double d2 = l.Distance2(fallPoint);
              if (d2 < d20)
              {
                d20 = d2;
                l0 = l;
              }
            }
            l = l.GetNextPointLine();
          }
          while (l.HasEqualBaseLine(lp) == false);
        }

        if (l0 == null)
        {
          ErrorFallDir(fallDir);
          continue;
        }

        Point fallConstructed = PntOp.Sub(l0.End, l0.Start);
        double vecProd =
          fallConstructed.X * Math.Sin(fallDir.Direction + Math.PI / 2) -
          fallConstructed.Y * Math.Cos(fallDir.Direction + Math.PI / 2);

        if (fallConstructed.Project(Geometry.ToXY).OrigDist2() * 0.5 > vecProd * vecProd)
        {
          ErrorFallDir(fallDir);
          continue;
        }

        contour = (Contour)l0.GetTag(out isReverse);
        if (isReverse == (vecProd < 0))
        { contour.Orientation = Orientation.LeftSideDown; }
        else
        { contour.Orientation = Orientation.RightSideDown; }

        OnProgressChanged(Progress.FallDirAssigned, contour);
      }
    }

    private void AssignHills(IList<Contour> contours)
    {
      foreach (var contour in contours)
      {
        if (contour.Orientation != Orientation.Unknown)
        { continue; }
        Polyline line = contour.Polyline;
        if (PntOp.Dist2(line.Points.First.Value, line.Points.Last.Value) > 0)
        { continue; }

        double xMax = line.Extent.Max.X;
        LinkedListNode<IPoint> node = line.Points.First;
        while (node != null)
        {
          if (node.Value.X == xMax)
          {
            IPoint p1 = node.Value;
            IPoint p2 = node.Next.Value;
            IPoint p0;
            if (node.Previous != null)
            { p0 = node.Previous.Value; }
            else
            {
              LinkedListNode<IPoint> node0 = line.Points.Last.Previous;
              p0 = node0.Value;
            }
            if (p2.X == xMax && p0.X == xMax)
            {
              Debug.Assert(node.Previous == null,
                "Error in software design assumption");
              LinkedListNode<IPoint> node0 = node.Next.Next;
              while (node0.Value.X == xMax)
              { node0 = node0.Next; }
              p2 = node0.Value;
            }

            if ((PntOp.Sub(p1, p0)).VectorProduct(PntOp.Sub(p2, p1)) < 0)
            { contour.Orientation = Orientation.LeftSideDown; }
            else
            { contour.Orientation = Orientation.RightSideDown; }

            OnProgressChanged(Progress.HillAssigned, contour);
            break;
          }

          node = node.Next;
        }
        if (contour.Orientation == Orientation.Unknown)
        {
          throw new InvalidProgramException(
            "Error in software design assumption: Orientation not set");
        }
      }
    }

    private void ErrorFallDir(FallDir fallDir)
    {
      Point2D p0 = fallDir.Point;
      Point2D p1 = p0.Clone();
      p1.X += Math.Cos(fallDir.Direction); ;
      p1.Y += Math.Sin(fallDir.Direction); ;
      OnProgressChanged(Progress.ErrorFallDir,
        new Contour(-1, Basics.Geom.Polyline.Create(new[] { p0, p1 }),
        new ContourType(0, 0)));
      //int orig = fallDir.Symbol;
      //fallDir.Symbol = 999009;
      // writer.Append(fallDir);
      //fallDir.Symbol = orig;
    }

    private IPoint NewPoint(Mesh mesh, Line line, object tag,
      Mesh.MeshLine cross, double crossFactor)
    {
      Point end = Point.CastOrCreate(cross.End);
      IPoint result = cross.Start + crossFactor * (end - cross.Start);

      object crossTag = cross.GetTag(out bool revers);
      if (crossTag != null)
      {
        InfoContour intersect = new InfoContour(
          Basics.Geom.Polyline.Create(new[] { cross.Start, cross.End }));
        intersect.Involveds.Add(crossTag as Contour);
        intersect.Involveds.Add(tag as Contour);
        OnProgressChanged(Progress.Intersection, intersect);
      }
      return result;
    }

    internal class InfoContour : Contour
    {
      public InfoContour(Polyline line)
        : base(-1, line, new ContourType(0, 0))
      {
        Involveds = new List<Contour>();
      }

      public List<Contour> Involveds { get; }
    }
    private class ComparableTagged : IComparable<Mesh.MeshLine>
    {
      public int CompareTo(Mesh.MeshLine other)
      {
        if (other.GetTag(out bool reverse) != null)
        { return 0; }
        else
        { return 1; }
      }
    }
    private class ComparerSymbol : IComparer<Mesh.MeshLine>
    {
      public int Compare(Mesh.MeshLine x, Mesh.MeshLine y)
      {
        Contour elemX = (Contour)x.GetTag(out bool reverse);
        Contour elemY = (Contour)y.GetTag(out reverse);

        return elemX.Type.CompareTo(elemY.Type);
      }
    }

    private List<Contour> Extract(Mesh mesh)
    {
      List<Contour> contours = new List<Contour>();

      ComparableTagged cprTagged = new ComparableTagged();
      ComparerSymbol cprSymbol = new ComparerSymbol();

      int id = 1;
      foreach (var lineList in mesh.LineStrings(cprTagged, cprSymbol))
      {
        ContourType symbol = ((Contour)lineList.First.Value.GetTag(out bool reverse)).Type;
        Polyline line = Polyline(lineList, reverse);
        if (line.Points.Count == 2 && line.Project(Geometry.ToXY).Length() < 4)
        { continue; }

        Contour contour = new Contour(id, line, symbol);
        id++;
        contours.Add(contour);
      }
      return contours;
    }

    private Polyline Polyline(LinkedList<Mesh.MeshLine> lineList, bool reverse)
    {
      Polyline line = new Polyline();
      if (reverse == false)
      {
        foreach (var l in lineList)
        { line.Add(l.Start); }
        line.Add(lineList.Last.Value.End);
      }
      else
      {
        LinkedListNode<Mesh.MeshLine> current = lineList.Last;
        line.Add(current.Value.End);
        while (current != null)
        {
          line.Add(current.Value.Start);
          current = current.Previous;
        }
      }
      return line;
    }
  }
}
