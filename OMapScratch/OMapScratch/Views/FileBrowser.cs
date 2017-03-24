using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class FileBrowser : LinearLayout
  {
    private LinearLayout _browse;
    private FileButton _top;
    private System.Action<string> _onSuccess;

    public IList<string> Filter { get; set; }
    public FileBrowser(Context context)
      : base(context)
    {
      Orientation = Orientation.Vertical;
      {
        LinearLayout menu = new LinearLayout(context);
        menu.Orientation = Orientation.Horizontal;
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          menu.LayoutParameters = lprams;
        }
        AddView(menu);
        {
          FileButton top = new FileButton(context, "..");
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          top.LayoutParameters = lprams;
          top.Text = "..";
          top.SetBackgroundColor(Color.LightSteelBlue);
          top.Click += (s, e) => 
          {
            if (top.FullPath.Contains("0"))
            { SetDirectory(top.FullPath); }
          };
          _top = top;
          menu.AddView(_top);
        }
        {
          Button btnCancel = new Button(context);
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          lprams.MarginStart = 10;
          btnCancel.LayoutParameters = lprams;
          btnCancel.Text = "Cancel";
          btnCancel.SetBackgroundColor(Color.LightPink);
          btnCancel.Click += (s, e) => { Visibility = ViewStates.Invisible; };
          menu.AddView(btnCancel);
        }
      }

      {
        ScrollView scroll = new ScrollView(context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          scroll.LayoutParameters = lprams;
        }
        AddView(scroll);

        {
          LinearLayout browse = new LinearLayout(context);
          browse.Orientation = Orientation.Vertical;
          browse.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            browse.LayoutParameters = lprams;
          }
          scroll.AddView(browse);
          _browse = browse;
        }
      }
    }

    public bool ShowHiddenEntries { get; set; }

    private IEnumerable<string> Select(IEnumerable<string> candidates, bool useFilter)
    {
      List<string> entries = new List<string>();
      foreach (string candidate in candidates)
      {
        if (!ShowHiddenEntries && System.IO.Path.GetFileName(candidate).StartsWith("."))
        { continue; }
        bool valid = true;
        if (useFilter && Filter != null)
        {
          valid = false;
          foreach (string filter in Filter)
          {
            if (candidate.EndsWith(filter, System.StringComparison.CurrentCultureIgnoreCase))
            {
              valid = true;
              break;
            }
          }
        }
        if (!valid)
        { continue; }
        entries.Add(candidate);
      }
      entries.Sort();
      return entries;
    }
    public void Show(System.Action<string> onSuccess)
    {
      _onSuccess = onSuccess;
      Visibility = ViewStates.Visible;
    }
    public void SetDirectory(string path)
    {
      LinearLayout browse = _browse;
      browse.RemoveAllViews();

      _top.FullPath = System.IO.Path.GetDirectoryName(path);
      foreach (string fsi in Select(System.IO.Directory.EnumerateFiles(path), true))
      {
        FileButton fileButton = new FileButton(Context, fsi);
        fileButton.TextAlignment = TextAlignment.TextStart;
        fileButton.SetBackgroundColor(Color.LightGreen);
        fileButton.Text = System.IO.Path.GetFileName(fsi);
        fileButton.Click += (s, e) =>
        {
          Visibility = ViewStates.Invisible;
          PostInvalidate();
          _onSuccess(fsi);
        };
        browse.AddView(fileButton);
      }
      foreach (string fsi in Select(System.IO.Directory.EnumerateDirectories(path), false))
      {
        FileButton fileButton = new FileButton(Context, fsi);
        fileButton.TextAlignment = TextAlignment.TextStart;
        fileButton.SetBackgroundColor(Color.LightBlue);
        fileButton.Text = System.IO.Path.GetFileName(fsi);
        fileButton.Click += (s, e) =>
        {
          SetDirectory(fsi);
        };
        browse.AddView(fileButton);
      }
      browse.PostInvalidate();
    }

    private class FileButton : Button
    {
      public FileButton(Context context, string fullPath)
        : base(context)
      { }
      public string FullPath { get; set; }
    }

  }
}