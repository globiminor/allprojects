using System.ComponentModel;
using System.Drawing;
using Basics.Geom;
using System.Data;

namespace TMap
{
  public class SymbolPartLine : SymbolPart, ILineWidthPart, IScaleablePart
  {
    private double _lineWidth;

    public SymbolPartLine(DataRow templateRow)
      : base(templateRow)
    {
      DrawLevel = 1;
    }

    public override int Topology
    { get { return 1; } }

    private Rectangle _rect = new Rectangle();
    private TTT _test = new TTT();
    private Pen _pen = new Pen(Color.Red);

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public Pen Pen
    {
      get { return _pen; }
      set { _pen = value; }
    }
    public TTT Test
    {
      get { return _test; }
    }
    public Rectangle Rect
    {
      get { return _rect; }
      set { _rect = value; }
    }

    private double _angle = 0;
    private bool _scale;

    [Editor("AngleEditor.AngleEditor, TMapWin", //, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
      typeof(System.Drawing.Design.UITypeEditor))]
    public double Angle
    {
      get { return _angle; }
      set { _angle = value; }
    }

    public override void Draw(IGeometry geometry, DataRow properties, IDrawable drawable)
    {
      if (geometry is Curve)
      {
        Curve c = geometry as Curve;
        Polyline t = new Polyline();
        t.Add(c);
        geometry = t;
      }
      //  int dashI,drawI;
      //  double dashFactor,dash0Diff,scaleF;
      //  int i;
      Polyline line = geometry as Polyline;
      if (line == null)
      { return; }
      //  if (sp->nDash > 0) {
      //    drawI = SymGetDash0Index(&dashI,&dash0Diff,
      //      sp->nDash,sp->dashLength,sp->dashOffset);
      //    if (sp->scale <= 0. && transMatrix != 0) {
      //      scaleF = transMatrix[0];
      //    } else {
      //      scaleF = 1.;
      //    }
      //    if (sp->dashAdjust) {
      //      dashFactor = SymGetDashFactor(
      //        G2LineLength(nLineP,xLineP,yLineP) * scaleF,
      //        sp->nDash,sp->dashLength,sp->dashOffset);
      //    } else {
      //      dashFactor = 1.;
      //    }
      //  }

      //  if (sp->type & SYM_LINE_TYPE || sp->type & SYM_DASH_POINT_TYPE) {
      //    if (sp->nDash <= 0 && sp->type & SYM_LINE_TYPE) {
      //      if (drawable.ExtentMatrix == null) {
      int nSeg = line.Segments.Count;
      if (nSeg == 0)
      { return; }

      Curve pLine = null;
      Polyline drawLine = null;
      foreach (Curve curve in line.Segments)
      {
        if (pLine == null)
        {
          pLine = curve;
          drawLine = new Polyline();
          drawLine.Add(curve);
        }
        else if (curve.GetType() != pLine.GetType())
        {
          drawable.DrawLine(drawLine.Project(drawable.Projection), this);
          pLine = curve;
          drawLine = new Polyline();
          drawLine.Add(curve);
        }
        else
        {
          drawLine.Add(curve);
        }
      }
      if (drawLine != null)
      {
        drawable.DrawLine(drawLine.Project(drawable.Projection), this);
      }

      //      } else {
      //        int s0;
      //        int n0;
      //        int n1;
      //        double *x,*y;
      //        
      //        x = xLineP;
      //        y = yLineP;
      //        n0 = nLineP;
      //        while (G2GetLineInBox(n0,x,y,
      //            extentMat[0],extentMat[2],extentMat[1],extentMat[3],&s0,&n1)) {
      //          x += s0;
      //          y += s0;
      //          SymTransLine(n1,x,y,0.,1.,transMatrix,&symNP,&symMaxP,
      //            &symTransX,&symTransY);
      //          drawFct(sp,symNP,symTransX,symTransY);
      //          x += n1 - 1;
      //          y += n1 - 1;
      //          n0 -= n1 + s0 - 1;
      //        }
      //      }
      //    } else {
      //      int tn;
      //      double t0,t1,l,j;
      //      double *tx,*ty;
      //      int dashEnd;
      //
      //      t0 = 0.;
      //      tn = nLineP;
      //      tx = xLineP;
      //      ty = yLineP;
      //      dashEnd = 0;
      //
      //      l = dashFactor * (sp->dashLength[dashI] - dash0Diff) / scaleF;
      //      while (tn > 1 && !dashEnd) {
      //        dashEnd = SymCutLine(tn,tx,ty,t0,l,&i,&t1);
      //        if (sp->type & SYM_LINE_TYPE) {
      //          if (drawI) {
      //            j = i + 2;
      //            if (t1 <= 0.) {
      //              j = i + 1;
      //	    }
      //            SymTransLine(j,tx,ty,t0,t1,transMatrix,
      //              &symNP,&symMaxP,&symTransX,&symTransY);
      //            drawFct(sp,symNP,symTransX,symTransY);
      //	  }
      //        } else if (sp->type & SYM_DASH_POINT_TYPE) {
      //          if (!dashEnd) {
      //            if (extentMat == 0 || SymIsLinePointVisible(tx[i],ty[i],
      //                tx[i + 1],ty[i + 1],t1,transMatrix,extentMat,sp)) {
      //              if (sp->directPoints) {
      //                SymLinePointToLine(sp->nPoint,sp->pointX,sp->pointY,
      //                  tx[i],ty[i],tx[i + 1],ty[i + 1],t1,transMatrix,sp->scale,
      //                  sp->dashMat,&symNP,&symMaxP,&symTransX,&symTransY);
      //              } else {
      //                SymPointToLine(sp->nPoint,sp->pointX,sp->pointY,
      //                  tx[i] + t1 * (tx[i + 1] - tx[i]),
      //                  ty[i] + t1 * (ty[i + 1] - ty[i]),
      //                  transMatrix,sp->scale,sp->dashMat,&symNP,&symMaxP,
      //                  &symTransX,&symTransY);
      //              }
      //              drawFct(sp,symNP,symTransX,symTransY);
      //            }
      //	  }
      //	}
      //        drawI = !drawI;
      //        tx = tx + i;
      //        ty = ty + i;
      //        t0 = t1;
      //        tn -= i;
      //        dashI ++;
      //        if (dashI >= sp->nDash) {
      //          dashI = 0;
      //	}
      //        l = dashFactor * sp->dashLength[dashI] / scaleF;
      //      }
      //    }
      //  } 
      //  if (sp->type & SYM_START_POINT_TYPE) {
      //    if (sp->directPoints && nLineP > 1) {
      //      if (extentMat == 0 || SymIsLinePointVisible(xLineP[0],yLineP[0],
      //          xLineP[1],yLineP[1],0.,transMatrix,extentMat,sp)) {
      //        SymLinePointToLine(sp->nPoint,sp->pointX,sp->pointY,
      //          xLineP[0],yLineP[0],xLineP[1],yLineP[1],0.,transMatrix,sp->scale,
      //          sp->startMat,&symNP,&symMaxP,&symTransX,&symTransY);
      //        drawFct(sp,symNP,symTransX,symTransY);
      //      }
      //    } else {
      //      if (extentMat == 0 || SymIsPointVisible(xLineP[0],yLineP[0],
      //          transMatrix,extentMat,sp)) {
      //        SymPointToLine(sp->nPoint,sp->pointX,sp->pointY,xLineP[0],yLineP[0],
      //          transMatrix,sp->scale,sp->startMat,&symNP,&symMaxP,
      //          &symTransX,&symTransY);
      //        drawFct(sp,symNP,symTransX,symTransY);
      //      }
      //    }
      //  }
      //  if ((sp->type & SYM_END_POINT_TYPE) && nLineP > 1) {
      //    i = nLineP - 1;
      //    if (extentMat == 0 ||  SymIsLinePointVisible(xLineP[i - 1],yLineP[i - 1],
      //        xLineP[i],yLineP[i],1.,transMatrix,extentMat,sp)) {
      //      if (sp->directPoints) {
      //        SymLinePointToLine(sp->nPoint,sp->pointX,sp->pointY,
      //          xLineP[i - 1],yLineP[i - 1],xLineP[i],yLineP[i],1.,transMatrix,
      //          sp->scale,sp->endMat,&symNP,&symMaxP,&symTransX,&symTransY);
      //      } else {
      //        SymPointToLine(sp->nPoint,sp->pointX,sp->pointY,xLineP[i],yLineP[i],
      //          transMatrix,sp->scale,sp->endMat,&symNP,&symMaxP,
      //          &symTransX,&symTransY);
      //      }
      //      drawFct(sp,symNP,symTransX,symTransY);
      //    }
      //  }
      //  if (sp->type & SYM_VERTEX_TYPE) {
      //    double dx,dy;
      //    for (i = 1; i < nLineP - 1; i++) {
      //      if (sp->directPoints) {
      //        dx = xLineP[i + 1] - xLineP[i - 1];
      //        dy = yLineP[i + 1] - yLineP[i - 1];
      //      } else {
      //        dx = 0.;
      //        dy = 0.;
      //      }
      //      if (extentMat == 0 || SymIsLinePointVisible(xLineP[i],yLineP[i],
      //          xLineP[i] + dx,yLineP[i] + dy,0.,transMatrix,extentMat,sp)) {
      //        SymLinePointToLine(sp->nPoint,sp->pointX,sp->pointY,
      //          xLineP[i],yLineP[i],xLineP[i] + dx,yLineP[i] + dy,0.,transMatrix,
      //          sp->scale,sp->vertexMat,&symNP,&symMaxP,&symTransX,&symTransY);
      //        drawFct(sp,symNP,symTransX,symTransY);
      //      }
      //    }
      //  }
      //}
    }

    public override double Size()
    {
      return _lineWidth;
    }

    public double LineWidth
    {
      get { return _lineWidth; }
      set { _lineWidth = value; }
    }

    public bool Scale
    {
      get { return _scale; }
      set { _scale = value; }
    }
  }
}