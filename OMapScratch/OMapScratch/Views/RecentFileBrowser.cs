using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class RecentFileBrowser : LinearLayout
  {
    private LinearLayout _tabs;
    private FileBrowser _browser;
    private LinearLayout _recents;
    private RadioButton _default;

    public RecentFileBrowser(Context context)
      : base(context)
    {
      Orientation = Orientation.Vertical;
      {
        LinearLayout tabs = new LinearLayout(context);
        tabs.Orientation = Orientation.Horizontal;
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          tabs.LayoutParameters = lprams;
        }
        AddView(tabs);
        tabs.SetBackgroundColor(Android.Graphics.Color.LightGray);
        _tabs = tabs;

        {
          RadioGroup grp = new RadioGroup(Context);
          grp.Orientation = Orientation.Horizontal;
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
            grp.LayoutParameters = lprams;
          }
          tabs.AddView(grp);

          {
            RadioButton recent = new RadioButton(Context);
            {
              RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent);
              recent.LayoutParameters = lprams;
            }

            recent.Text = "Recent";
            recent.Click += (s, e) =>
            {
              _recents.Visibility = ViewStates.Visible;
              _browser.Visibility = ViewStates.Invisible;
              PostInvalidate();
            };
            grp.AddView(recent);

            _default = recent;
          }
          {
            RadioButton browse = new RadioButton(Context);
            browse.Text = "Browse";
            browse.Click += (s, e) =>
            {
              _recents.Visibility = ViewStates.Invisible;
              _browser.Visibility = ViewStates.Visible;
              PostInvalidate();
            };
            grp.AddView(browse);
          }
        }
        {
          View dummy = new View(Context);
          {
            LayoutParams lprams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.MatchParent, 1f);
            dummy.LayoutParameters = lprams;
          }
          tabs.AddView(dummy);
        }
        {
          Button cancel = new Button(Context);
          cancel.SetAllCaps(false);
          cancel.Text = "Cancel";
          cancel.Click += (s, e) =>
          {
            Visibility = ViewStates.Invisible;
          };
          tabs.AddView(cancel);
        }

      }
      {
        RelativeLayout content = new RelativeLayout(context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          content.LayoutParameters = lprams;
        }

        AddView(content);

        {
          FileBrowser browser = new FileBrowser(context);
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            browser.LayoutParameters = lprams;
          }
          content.AddView(browser);
          _browser = browser;
        }
        {
          LinearLayout recents = new LinearLayout(context);
          recents.Orientation = Orientation.Vertical;
          {
            RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            recents.LayoutParameters = lprams;
          }
          content.AddView(recents);
          _recents = recents;
        }

      }
    }

    public IList<string> Filter
    {
      get { return _browser.Filter; }
      set { _browser.Filter = value; }
    }

    public void SetDirectory(string dir)
    {
      _browser.SetDirectory(dir);
    }

    public void Show(IList<string> recents, System.Action<string> selected)
    {
      _recents.RemoveAllViews();

      bool hasRecents = recents?.Count > 0;
      _tabs.Visibility = hasRecents ? ViewStates.Visible : ViewStates.Gone;

      _browser.Show((file) =>
      {
        Visibility = ViewStates.Invisible;
        PostInvalidate();
        selected(file);
      });

      if (hasRecents)
      {
        _default.Checked = true;

        foreach (string recent in recents)
        {
          FileButton fileButton = new FileButton(Context, recent, false);
          fileButton.TextAlignment = TextAlignment.TextStart;
          fileButton.Text = System.IO.Path.GetFileName(recent);
          fileButton.Click += (s, e) =>
          {
            Visibility = ViewStates.Invisible;
            PostInvalidate();
            selected(recent);
          };
          _recents.AddView(fileButton);
        }
        _browser.Visibility = ViewStates.Invisible;
        _recents.Visibility = ViewStates.Visible;
      }

      Visibility = ViewStates.Visible;
    }
  }
}