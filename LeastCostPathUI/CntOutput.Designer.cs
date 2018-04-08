namespace LeastCostPathUI
{
  partial class CntOutput
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
      _disposed = true;
      _cancelled = true;

      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CntOutput));
      this.grpRoute = new System.Windows.Forms.GroupBox();
      this.lblMinOffset = new System.Windows.Forms.Label();
      this.txtMinOffset = new System.Windows.Forms.TextBox();
      this.lblSlower = new System.Windows.Forms.Label();
      this.txtSlower = new System.Windows.Forms.TextBox();
      this.btnRouteShp = new System.Windows.Forms.Button();
      this.txtRouteShp = new System.Windows.Forms.TextBox();
      this.chkRouteShp = new System.Windows.Forms.CheckBox();
      this.btnRoute = new System.Windows.Forms.Button();
      this.txtRoute = new System.Windows.Forms.TextBox();
      this.chkRoute = new System.Windows.Forms.CheckBox();
      this.cntRoute = new LeastCostPathUI.CntOutGrid();
      this.lblPctOffset = new System.Windows.Forms.Label();
      this.lblPctSlower = new System.Windows.Forms.Label();
      this.grpTo = new System.Windows.Forms.GroupBox();
      this.cntTo = new LeastCostPathUI.CntOutGrid();
      this.grpFrom = new System.Windows.Forms.GroupBox();
      this.cntFrom = new LeastCostPathUI.CntOutGrid();
      this.grpExtent = new System.Windows.Forms.GroupBox();
      this.lblXMax = new System.Windows.Forms.Label();
      this.lblYMin = new System.Windows.Forms.Label();
      this.lblXMin = new System.Windows.Forms.Label();
      this.lblYMax = new System.Windows.Forms.Label();
      this.txtYMin = new System.Windows.Forms.TextBox();
      this.txtXMax = new System.Windows.Forms.TextBox();
      this.txtXMin = new System.Windows.Forms.TextBox();
      this.txtYMax = new System.Windows.Forms.TextBox();
      this.lblProgress = new System.Windows.Forms.Label();
      this.lblStep = new System.Windows.Forms.Label();
      this.ttp = new System.Windows.Forms.ToolTip(this.components);
      this.grpRoute.SuspendLayout();
      this.grpTo.SuspendLayout();
      this.grpFrom.SuspendLayout();
      this.grpExtent.SuspendLayout();
      this.SuspendLayout();
      // 
      // grpRoute
      // 
      this.grpRoute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpRoute.Controls.Add(this.lblMinOffset);
      this.grpRoute.Controls.Add(this.txtMinOffset);
      this.grpRoute.Controls.Add(this.lblSlower);
      this.grpRoute.Controls.Add(this.txtSlower);
      this.grpRoute.Controls.Add(this.btnRouteShp);
      this.grpRoute.Controls.Add(this.txtRouteShp);
      this.grpRoute.Controls.Add(this.chkRouteShp);
      this.grpRoute.Controls.Add(this.btnRoute);
      this.grpRoute.Controls.Add(this.txtRoute);
      this.grpRoute.Controls.Add(this.chkRoute);
      this.grpRoute.Controls.Add(this.cntRoute);
      this.grpRoute.Controls.Add(this.lblPctOffset);
      this.grpRoute.Controls.Add(this.lblPctSlower);
      this.grpRoute.Location = new System.Drawing.Point(3, 382);
      this.grpRoute.Name = "grpRoute";
      this.grpRoute.Size = new System.Drawing.Size(416, 175);
      this.grpRoute.TabIndex = 20;
      this.grpRoute.TabStop = false;
      this.grpRoute.Text = "Route";
      // 
      // lblMinOffset
      // 
      this.lblMinOffset.AutoSize = true;
      this.lblMinOffset.Location = new System.Drawing.Point(283, 147);
      this.lblMinOffset.Name = "lblMinOffset";
      this.lblMinOffset.Size = new System.Drawing.Size(54, 13);
      this.lblMinOffset.TabIndex = 20;
      this.lblMinOffset.Text = "min Offset";
      // 
      // txtMinOffset
      // 
      this.txtMinOffset.Location = new System.Drawing.Point(343, 144);
      this.txtMinOffset.Name = "txtMinOffset";
      this.txtMinOffset.Size = new System.Drawing.Size(39, 20);
      this.txtMinOffset.TabIndex = 19;
      this.txtMinOffset.Text = "5";
      this.txtMinOffset.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // lblSlower
      // 
      this.lblSlower.AutoSize = true;
      this.lblSlower.Location = new System.Drawing.Point(140, 147);
      this.lblSlower.Name = "lblSlower";
      this.lblSlower.Size = new System.Drawing.Size(62, 13);
      this.lblSlower.TabIndex = 17;
      this.lblSlower.Text = "Max Slower";
      // 
      // txtSlower
      // 
      this.txtSlower.Location = new System.Drawing.Point(205, 144);
      this.txtSlower.Name = "txtSlower";
      this.txtSlower.Size = new System.Drawing.Size(39, 20);
      this.txtSlower.TabIndex = 16;
      this.txtSlower.Text = "20";
      this.txtSlower.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // btnRouteShp
      // 
      this.btnRouteShp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRouteShp.Image = ((System.Drawing.Image)(resources.GetObject("btnRouteShp.Image")));
      this.btnRouteShp.Location = new System.Drawing.Point(384, 92);
      this.btnRouteShp.Name = "btnRouteShp";
      this.btnRouteShp.Size = new System.Drawing.Size(20, 20);
      this.btnRouteShp.TabIndex = 15;
      this.btnRouteShp.Click += new System.EventHandler(this.BtnRouteShp_Click);
      // 
      // txtRouteShp
      // 
      this.txtRouteShp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtRouteShp.Location = new System.Drawing.Point(143, 92);
      this.txtRouteShp.Name = "txtRouteShp";
      this.txtRouteShp.Size = new System.Drawing.Size(239, 20);
      this.txtRouteShp.TabIndex = 14;
      this.txtRouteShp.Text = "*r.shp";
      // 
      // chkRouteShp
      // 
      this.chkRouteShp.AutoSize = true;
      this.chkRouteShp.Location = new System.Drawing.Point(25, 94);
      this.chkRouteShp.Name = "chkRouteShp";
      this.chkRouteShp.Size = new System.Drawing.Size(89, 17);
      this.chkRouteShp.TabIndex = 13;
      this.chkRouteShp.Text = "Route Shape";
      // 
      // btnRoute
      // 
      this.btnRoute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.btnRoute.Image = ((System.Drawing.Image)(resources.GetObject("btnRoute.Image")));
      this.btnRoute.Location = new System.Drawing.Point(384, 118);
      this.btnRoute.Name = "btnRoute";
      this.btnRoute.Size = new System.Drawing.Size(20, 20);
      this.btnRoute.TabIndex = 12;
      this.btnRoute.Click += new System.EventHandler(this.BtnRoute_Click);
      // 
      // txtRoute
      // 
      this.txtRoute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.txtRoute.Location = new System.Drawing.Point(143, 118);
      this.txtRoute.Name = "txtRoute";
      this.txtRoute.Size = new System.Drawing.Size(239, 20);
      this.txtRoute.TabIndex = 11;
      this.txtRoute.Text = "*r.tif";
      // 
      // chkRoute
      // 
      this.chkRoute.AutoSize = true;
      this.chkRoute.Location = new System.Drawing.Point(25, 120);
      this.chkRoute.Name = "chkRoute";
      this.chkRoute.Size = new System.Drawing.Size(87, 17);
      this.chkRoute.TabIndex = 10;
      this.chkRoute.Text = "Route Image";
      // 
      // cntRoute
      // 
      this.cntRoute.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cntRoute.Location = new System.Drawing.Point(8, 14);
      this.cntRoute.Name = "cntRoute";
      this.cntRoute.ShowCoordinates = false;
      this.cntRoute.Size = new System.Drawing.Size(400, 72);
      this.cntRoute.TabIndex = 0;
      this.cntRoute.X = 0;
      this.cntRoute.Y = 0;
      // 
      // lblPctOffset
      // 
      this.lblPctOffset.AutoSize = true;
      this.lblPctOffset.Location = new System.Drawing.Point(381, 147);
      this.lblPctOffset.Name = "lblPctOffset";
      this.lblPctOffset.Size = new System.Drawing.Size(15, 13);
      this.lblPctOffset.TabIndex = 21;
      this.lblPctOffset.Text = "%";
      // 
      // lblPctSlower
      // 
      this.lblPctSlower.AutoSize = true;
      this.lblPctSlower.Location = new System.Drawing.Point(244, 147);
      this.lblPctSlower.Name = "lblPctSlower";
      this.lblPctSlower.Size = new System.Drawing.Size(15, 13);
      this.lblPctSlower.TabIndex = 18;
      this.lblPctSlower.Text = "%";
      // 
      // grpTo
      // 
      this.grpTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpTo.Controls.Add(this.cntTo);
      this.grpTo.Location = new System.Drawing.Point(3, 236);
      this.grpTo.Name = "grpTo";
      this.grpTo.Size = new System.Drawing.Size(416, 144);
      this.grpTo.TabIndex = 19;
      this.grpTo.TabStop = false;
      this.grpTo.Text = "To";
      // 
      // cntTo
      // 
      this.cntTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cntTo.Location = new System.Drawing.Point(8, 19);
      this.cntTo.Name = "cntTo";
      this.cntTo.ShowCoordinates = true;
      this.cntTo.Size = new System.Drawing.Size(400, 120);
      this.cntTo.TabIndex = 0;
      this.cntTo.X = 0;
      this.cntTo.Y = 0;
      // 
      // grpFrom
      // 
      this.grpFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpFrom.Controls.Add(this.cntFrom);
      this.grpFrom.Location = new System.Drawing.Point(3, 90);
      this.grpFrom.Name = "grpFrom";
      this.grpFrom.Size = new System.Drawing.Size(416, 144);
      this.grpFrom.TabIndex = 18;
      this.grpFrom.TabStop = false;
      this.grpFrom.Text = "From";
      // 
      // cntFrom
      // 
      this.cntFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.cntFrom.Location = new System.Drawing.Point(8, 18);
      this.cntFrom.Name = "cntFrom";
      this.cntFrom.ShowCoordinates = true;
      this.cntFrom.Size = new System.Drawing.Size(400, 120);
      this.cntFrom.TabIndex = 0;
      this.cntFrom.X = 0;
      this.cntFrom.Y = 0;
      // 
      // grpExtent
      // 
      this.grpExtent.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                  | System.Windows.Forms.AnchorStyles.Right)));
      this.grpExtent.Controls.Add(this.lblXMax);
      this.grpExtent.Controls.Add(this.lblYMin);
      this.grpExtent.Controls.Add(this.lblXMin);
      this.grpExtent.Controls.Add(this.lblYMax);
      this.grpExtent.Controls.Add(this.txtYMin);
      this.grpExtent.Controls.Add(this.txtXMax);
      this.grpExtent.Controls.Add(this.txtXMin);
      this.grpExtent.Controls.Add(this.txtYMax);
      this.grpExtent.Location = new System.Drawing.Point(3, 3);
      this.grpExtent.Name = "grpExtent";
      this.grpExtent.Size = new System.Drawing.Size(416, 85);
      this.grpExtent.TabIndex = 17;
      this.grpExtent.TabStop = false;
      this.grpExtent.Text = "Extent";
      // 
      // lblXMax
      // 
      this.lblXMax.Location = new System.Drawing.Point(238, 39);
      this.lblXMax.Name = "lblXMax";
      this.lblXMax.Size = new System.Drawing.Size(40, 16);
      this.lblXMax.TabIndex = 12;
      this.lblXMax.Text = "X max";
      // 
      // lblYMin
      // 
      this.lblYMin.Location = new System.Drawing.Point(136, 60);
      this.lblYMin.Name = "lblYMin";
      this.lblYMin.Size = new System.Drawing.Size(40, 16);
      this.lblYMin.TabIndex = 11;
      this.lblYMin.Text = "Y min";
      // 
      // lblXMin
      // 
      this.lblXMin.AutoSize = true;
      this.lblXMin.Location = new System.Drawing.Point(39, 39);
      this.lblXMin.Name = "lblXMin";
      this.lblXMin.Size = new System.Drawing.Size(33, 13);
      this.lblXMin.TabIndex = 10;
      this.lblXMin.Text = "X min";
      // 
      // lblYMax
      // 
      this.lblYMax.Location = new System.Drawing.Point(136, 18);
      this.lblYMax.Name = "lblYMax";
      this.lblYMax.Size = new System.Drawing.Size(40, 16);
      this.lblYMax.TabIndex = 9;
      this.lblYMax.Text = "Y max";
      // 
      // txtYMin
      // 
      this.txtYMin.Location = new System.Drawing.Point(184, 57);
      this.txtYMin.Name = "txtYMin";
      this.txtYMin.Size = new System.Drawing.Size(104, 20);
      this.txtYMin.TabIndex = 7;
      this.txtYMin.Text = "0";
      this.txtYMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtYMin.Validating += new System.ComponentModel.CancelEventHandler(this.TxtDouble_Validating);
      // 
      // txtXMax
      // 
      this.txtXMax.Location = new System.Drawing.Point(286, 36);
      this.txtXMax.Name = "txtXMax";
      this.txtXMax.Size = new System.Drawing.Size(104, 20);
      this.txtXMax.TabIndex = 6;
      this.txtXMax.Text = "1";
      this.txtXMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtXMax.Validating += new System.ComponentModel.CancelEventHandler(this.TxtDouble_Validating);
      // 
      // txtXMin
      // 
      this.txtXMin.Location = new System.Drawing.Point(80, 36);
      this.txtXMin.Name = "txtXMin";
      this.txtXMin.Size = new System.Drawing.Size(104, 20);
      this.txtXMin.TabIndex = 5;
      this.txtXMin.Text = "0";
      this.txtXMin.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtXMin.Validating += new System.ComponentModel.CancelEventHandler(this.TxtDouble_Validating);
      // 
      // txtYMax
      // 
      this.txtYMax.Location = new System.Drawing.Point(184, 15);
      this.txtYMax.Name = "txtYMax";
      this.txtYMax.Size = new System.Drawing.Size(104, 20);
      this.txtYMax.TabIndex = 4;
      this.txtYMax.Text = "1";
      this.txtYMax.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.txtYMax.Validating += new System.ComponentModel.CancelEventHandler(this.TxtDouble_Validating);
      // 
      // lblProgress
      // 
      this.lblProgress.AutoSize = true;
      this.lblProgress.Location = new System.Drawing.Point(80, 564);
      this.lblProgress.Name = "lblProgress";
      this.lblProgress.Size = new System.Drawing.Size(60, 13);
      this.lblProgress.TabIndex = 22;
      this.lblProgress.Text = "<Progress>";
      // 
      // lblStep
      // 
      this.lblStep.AutoSize = true;
      this.lblStep.Location = new System.Drawing.Point(8, 564);
      this.lblStep.Name = "lblStep";
      this.lblStep.Size = new System.Drawing.Size(41, 13);
      this.lblStep.TabIndex = 21;
      this.lblStep.Text = "<Step>";
      // 
      // CntOutput
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.lblProgress);
      this.Controls.Add(this.lblStep);
      this.Controls.Add(this.grpRoute);
      this.Controls.Add(this.grpTo);
      this.Controls.Add(this.grpFrom);
      this.Controls.Add(this.grpExtent);
      this.Name = "CntOutput";
      this.Size = new System.Drawing.Size(422, 586);
      this.grpRoute.ResumeLayout(false);
      this.grpRoute.PerformLayout();
      this.grpTo.ResumeLayout(false);
      this.grpFrom.ResumeLayout(false);
      this.grpExtent.ResumeLayout(false);
      this.grpExtent.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.GroupBox grpRoute;
    private CntOutGrid cntRoute;
    private System.Windows.Forms.GroupBox grpTo;
    private CntOutGrid cntTo;
    private System.Windows.Forms.GroupBox grpFrom;
    private CntOutGrid cntFrom;
    private System.Windows.Forms.GroupBox grpExtent;
    private System.Windows.Forms.Label lblXMax;
    private System.Windows.Forms.Label lblYMin;
    private System.Windows.Forms.Label lblXMin;
    private System.Windows.Forms.Label lblYMax;
    private System.Windows.Forms.TextBox txtYMin;
    private System.Windows.Forms.TextBox txtXMax;
    private System.Windows.Forms.TextBox txtXMin;
    private System.Windows.Forms.TextBox txtYMax;
    private System.Windows.Forms.Label lblProgress;
    private System.Windows.Forms.Label lblStep;
    private System.Windows.Forms.Button btnRoute;
    private System.Windows.Forms.TextBox txtRoute;
    private System.Windows.Forms.CheckBox chkRoute;
    private System.Windows.Forms.Button btnRouteShp;
    private System.Windows.Forms.TextBox txtRouteShp;
    private System.Windows.Forms.CheckBox chkRouteShp;
    private System.Windows.Forms.Label lblPctSlower;
    private System.Windows.Forms.Label lblSlower;
    private System.Windows.Forms.TextBox txtSlower;
    private System.Windows.Forms.Label lblPctOffset;
    private System.Windows.Forms.Label lblMinOffset;
    private System.Windows.Forms.TextBox txtMinOffset;
    private System.Windows.Forms.ToolTip ttp;
  }
}
