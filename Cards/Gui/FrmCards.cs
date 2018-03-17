using System.Windows.Forms;
using Cards.Vm;
using GuiUtils;
using Basics;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Cards.Gui
{
  public partial class FrmCards : Form, ICardsView
  {
    private readonly BindingSource _bindingSource;
    private CardsVm _vm;
    public FrmCards()
    {
      InitializeComponent();

      components = new System.ComponentModel.Container();
      _bindingSource = new BindingSource(components);
    }

    public CardsVm Vm
    {
      get { return _vm; }
      set
      {
        _vm = value;
        cntCards.CardsVm = _vm;

        bool notBound = _bindingSource.DataSource == null;
        _bindingSource.DataSource = _vm;

        if (notBound)
        {
          this.Bind(x => x.Text, _bindingSource, nameof(_vm.Title),
            true, DataSourceUpdateMode.Never);

          txtMoves.Bind(x => x.Text, _bindingSource, nameof(_vm.Moves),
            true, DataSourceUpdateMode.Never);

          txtPoints.Bind(x => x.Text, _bindingSource, nameof(_vm.Points),
            true, DataSourceUpdateMode.Never);

          txtDisp.Bind(x => x.Text, _bindingSource, nameof(_vm.DisplaySecs),
            true, DataSourceUpdateMode.OnPropertyChanged);

          txtInfo.Bind(x => x.Text, _bindingSource, nameof(_vm.Info),
            true, DataSourceUpdateMode.OnPropertyChanged);

          btnSolve.Bind(x => x.Text, _bindingSource, nameof(_vm.SolveText),
            true, DataSourceUpdateMode.Never);
        }
      }
    }

    void ICardsView.Clear()
    {
    }

    public void RefreshCards()
    {
      if (InvokeRequired)
      {
        Invoke(new MethodInvoker(RefreshCards));
        return;
      }
      cntCards.Refresh();

      if (System.Diagnostics.Debugger.IsAttached)
      { Application.DoEvents(); }
    }

    private void FrmCards_Load(object sender, System.EventArgs e)
    {
      if (Vm != null)
      { Vm.InitView(); }
    }

    private void btnSolve_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      Vm.ToggleSolve();
    }

    private void btnRevert_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      Vm.Revert();
      Invalidate();
      cntCards.Invalidate();
    }

    private void btnSlow_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      Vm.DisplaySecs = Vm.DisplaySecs * 1.2;
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      Vm.DisplaySecs = Vm.DisplaySecs / 1.2;
    }

    private void mniSpider4_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      Vm.SetSpider4();

      Invalidate();
      cntCards.Invalidate();
    }

    private void mniTriPeaks_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }

      Vm.SetTriPeaks();

      Invalidate();
      cntCards.Invalidate();
    }

    private void mniSave_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      SaveFileDialog dlg = new SaveFileDialog();
      if (dlg.ShowDialog() != DialogResult.OK)
      { return; }

      Vm.Save(dlg.FileName);
    }

    private void mniLoad_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      string load;
      using (OpenFileDialog dlg = new OpenFileDialog())
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        { return; }
        load = dlg.FileName;
      }

      Vm.Load(load);
    }

    private void mniNew_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }
      string load;
      using (SaveFileDialog dlg = new SaveFileDialog())
      {
        if (dlg.ShowDialog() != DialogResult.OK)
        { return; }
        load = dlg.FileName;
      }

      Vm.New(load);

      Invalidate();
      cntCards.Invalidate();
    }

    private void mniCapture_Click(object sender, System.EventArgs e)
    {
      if (Vm == null)
      { return; }

      Macro.Processor proc = new Macro.Processor();
      foreach (Macro.WindowPtr window in Macro.Processor.GetChildWindows(IntPtr.Zero))
      {
        if (window.GetWindowText() == "Microsoft Solitaire Collection")
        {
          Rectangle rect = Macro.Processor.GetWindowRect(window.HWnd);

          Bitmap bmpScreen = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppPArgb);
          Graphics gfxScreen = Graphics.FromImage(bmpScreen);

          gfxScreen.CopyFromScreen(rect.Left, rect.Top, 0, 0, 
            new Size(rect.Width, rect.Height), CopyPixelOperation.SourceCopy);

          GetCards(bmpScreen);
          //bmpScreen.Save("C:\\temp\\msc.png", ImageFormat.Png);
        }
      }
    }
    private void GetCards(Bitmap bitmap)
    {

    }
  }
}
