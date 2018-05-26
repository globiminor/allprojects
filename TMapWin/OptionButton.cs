using System;
using System.Drawing;
using System.Windows.Forms;

namespace TMapWin
{
  /// <summary>
  /// Summary description for OptionButton.
  /// </summary>
  public class OptionButton : Button
  {
    private bool _checked;
    private bool _group = true;
    private Bitmap _checkedBackgroud;
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container _components = null;

    public OptionButton()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      _checkedBackgroud = new Bitmap(2,2,System.Drawing.Imaging.PixelFormat.Format24bppRgb);

      _checkedBackgroud.SetPixel(0,0,Color.White);
      _checkedBackgroud.SetPixel(0,1,Color.Gray);
      _checkedBackgroud.SetPixel(1,0,Color.Gray);
      _checkedBackgroud.SetPixel(1,1,Color.White);
      // TODO: Add any initialization after the InitializeComponent call
    }

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
      if( disposing )
      {
        if(_components != null)
        {
          _components.Dispose();
        }
      }
      base.Dispose( disposing );
    }

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      _components = new System.ComponentModel.Container();
    }
    #endregion

    protected override void OnClick(EventArgs e)
    {
      if (_group == false)
      { Checked = !_checked; }
      else
      {
        Checked = true;
        foreach (Control cntr in Parent.Controls)
        {
          if (cntr is OptionButton obtn && obtn != this)
          { obtn.Checked = false; }
        }
      }
      base.OnClick (e);
    }

    public bool Group
    {
      get
      { return _group; }
      set
      { _group = value; }
    }
    public bool Checked
    {
      get
      { return _checked; }
      set
      {
        _checked = value;
        if (_checked)
        {
          BackgroundImage = _checkedBackgroud;
          FlatStyle = FlatStyle.Flat;
        }
        else
        { 
          BackgroundImage = null;
          FlatStyle = FlatStyle.Standard;
        }
      }
    }
  }
}
