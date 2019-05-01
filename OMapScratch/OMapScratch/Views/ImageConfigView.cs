using Android.Graphics;
using Android.Views;
using Android.Widget;
using Basics.Views;
using System;
using System.Collections.Generic;

namespace OMapScratch.Views
{
  public class ImageConfigView : LinearLayout
  {
    private class DisplayView : View, IGeoImagesContainer
    {
      private readonly GeoImageVm _baseImage;
      private readonly IReadOnlyList<GeoImageVm> _combinations;

      public DisplayView(GeoImageVm baseImge, IReadOnlyList<GeoImageVm> combinations, Android.Content.Context context)
        : base(context)
      {
        _baseImage = baseImge;
        _combinations = combinations;
      }

      void IGeoImagesContainer.Invalidate() => PostInvalidate();

      protected override void OnDraw(Canvas canvas)
      {
        Utils.Try(() =>
        {
          for (int iComb = _combinations.Count - 1; iComb >= 0; iComb--)
          {
            Draw(canvas, _combinations[iComb]);
          }
          Draw(canvas, _baseImage);
        });
      }
      private void Draw(Canvas canvas, GeoImageVm vm)
      {
        if (!vm.Visible)
        { return; }
        if (vm.Opacity <= 0)
        { return; }
        if (vm.Bitmap == null)
        { return; }

        using (Paint p = new Paint())
        {
          Color color = Color.White;
          p.Color = color;
          IGeoImageViewUtils.SetColor(p, vm);
          canvas.DrawBitmap(vm.Bitmap, 0, 0, p);
        }
      }

      public void InitDisplay(MatrixPrj prj, int width, int height)
      {
        InitDisplay(prj, _baseImage, width, height);
        foreach (var img in _combinations)
        {
          InitDisplay(prj, img, width, height);
        }
      }
      private void InitDisplay(MatrixPrj prj, GeoImageVm img, int width, int height)
      {
        img.Container = this;
        img.Projection = prj;
        img.Width = width;
        img.Height = height;
      }
    }

    private readonly ViewGroup _parent;
    private readonly GeoImageVm _baseVm;
    private readonly List<GeoImageVm> _combinations;


    public ImageConfigView(ViewGroup parent, GeoImageComb editView, GeoImageViews baseImage, IReadOnlyList<GeoImageViews> combinations, MatrixPrj prj)
      : base(parent.Context)
    {
      _parent = parent;
      _combinations = new List<GeoImageVm>();
      foreach (var combi in combinations)
      {
        if (combi == baseImage)
        { continue; }
        GeoImageVm vm = new GeoImageVm(null, combi, null) { Opacity = 100, Visible = false, VisibleEnabled = true };
        _combinations.Add(vm);
      }
      _baseVm = new GeoImageVm(editView, baseImage, _combinations) { Opacity = 50, Visible = true, VisibleEnabled = false };

      SetBackgroundColor(Color.White);

      LinearLayout allLayout = new LinearLayout(Context)
      { Orientation = Orientation.Vertical, };
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        allLayout.LayoutParameters = lprams;
        allLayout.Visibility = ViewStates.Visible;
      }
      AddView(allLayout);

      {
        LinearLayout confirmLayout = new LinearLayout(Context)
        { Orientation = Orientation.Horizontal };
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          confirmLayout.LayoutParameters = lprams;
        }
        allLayout.AddView(confirmLayout);
        InitConfirm(confirmLayout);
      }

      {
        LinearLayout editLayout = new LinearLayout(Context)
        { Orientation = Orientation.Vertical };
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
          editLayout.LayoutParameters = lprams;
        }
        allLayout.AddView(editLayout);
        InitDisplay(editLayout, prj);
        InitEdit(editLayout);
      }
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);

      _baseVm.Dispose();
      foreach (var vm in _combinations)
      { vm.Dispose(); }
    }

    private void InitDisplay(LinearLayout parent, MatrixPrj prj)
    {
      DisplayView mapDisplay = new DisplayView(_baseVm, _combinations, Context);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.Width = (int)(60 * Utils.GetMmPixel(mapDisplay));
        lprams.Height = (int)(30 * Utils.GetMmPixel(mapDisplay));

        mapDisplay.LayoutParameters = lprams;
        mapDisplay.Visibility = ViewStates.Visible;

        mapDisplay.InitDisplay(prj, lprams.Width, lprams.Height);
      }
      parent.AddView(mapDisplay);
    }
    private void InitEdit(LinearLayout parent)
    {
      LinearLayout imageList = new LinearLayout(Context)
      { Orientation = Orientation.Vertical };
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        imageList.LayoutParameters = lprams;
        imageList.Visibility = ViewStates.Visible;
      }
      parent.AddView(imageList);

      AddImage(_baseVm, imageList);

      ScrollView scroll = new ScrollView(Context);
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        scroll.LayoutParameters = lprams;
      }
      imageList.AddView(scroll);

      LinearLayout browse = new LinearLayout(Context);
      browse.Orientation = Orientation.Vertical;
      browse.ScrollBarStyle = ScrollbarStyles.InsideOverlay;
      {
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        browse.LayoutParameters = lprams;
      }
      scroll.AddView(browse);

      foreach (var imgVm in _combinations)
      {
        AddImage(imgVm, browse);
      }
    }
    private void InitConfirm(LinearLayout layout)
    {
      {
        Button okBtn = new Button(Context)
        { Text = "OK" };

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.Width = (int)(30 * Utils.GetMmPixel(okBtn));
        okBtn.LayoutParameters = lprams;

        okBtn.Click += (bs, be) => Utils.Try(() =>
        {
          _baseVm.Save();
          _parent.RemoveView(this);
          Dispose();
        });
        layout.AddView(okBtn);
      }
      {
        Button cancelBtn = new Button(Context)
        { Text = "Cancel" };

        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        lprams.Width = (int)(30 * Utils.GetMmPixel(cancelBtn));
        cancelBtn.LayoutParameters = lprams;

        cancelBtn.Click += (bs, be) => Utils.Try(() =>
        {
          _parent.RemoveView(this);
          Dispose();
        });
        layout.AddView(cancelBtn);
      }
    }
    private void AddImage(GeoImageVm vm, ViewGroup imageList)
    {
      LinearLayout imageRow = new LinearLayout(Context);
      {
        imageRow.Orientation = Orientation.Horizontal;
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        imageRow.LayoutParameters = lprams;
        imageList.AddView(imageRow);
      }
      {
        CheckBox chkVisible = new CheckBox(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          chkVisible.LayoutParameters = lprams;
        }
        imageRow.AddView(chkVisible);

        chkVisible.BindToChecked(vm, nameof(vm.Visible));
        chkVisible.Enabled = vm.VisibleEnabled;
      }
      {
        Button defaultBtn = new Button(Context)
        { Text = vm.Name };
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.Width = (int)(25 * Utils.GetMmPixel(defaultBtn));
          defaultBtn.LayoutParameters = lprams;
        }
        defaultBtn.Click += (bs, be) => Utils.Try(() =>
        {
          //            MapVm.LoadLocalImage(imgViews.DefaultView, _mapView.InversElemMatrix, _mapView.Width, _mapView.Height);
        });
        imageRow.AddView(defaultBtn);
      }
      LinearLayout sliders = new LinearLayout(Context);
      {
        sliders.Orientation = Orientation.Vertical;
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        sliders.LayoutParameters = lprams;
        imageRow.AddView(sliders);
      }

      {
        SeekBar opacitySlider = AddSlider(sliders, "O");
        opacitySlider.BindToProgress(vm, nameof(vm.Opacity));
      }

      {
        SeekBar graySlider = AddSlider(sliders, "G");
        graySlider.BindToProgress(vm, nameof(vm.Gray));
        graySlider.BindToEnabled(vm, nameof(vm.GrayEnabled));
      }

      {
        SeekBar colorRotSlider = AddSlider(sliders, "C");
        colorRotSlider.BindToProgress(vm, nameof(vm.ColorRotation));
        colorRotSlider.BindToEnabled(vm, nameof(vm.ColorRotationEnabled));
      }
    }

    private SeekBar AddSlider(ViewGroup sliders, string name)
    {
      LinearLayout l = new LinearLayout(Context);
      {
        l.Orientation = Orientation.Horizontal;
        RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        l.LayoutParameters = lprams;
        sliders.AddView(l);
      }
      {
        TextView lbl = new TextView(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lbl.LayoutParameters = lprams;
        }
        lbl.Text = name;
        l.AddView(lbl);
      }
      {
        SeekBar slider = new SeekBar(Context);
        {
          RelativeLayout.LayoutParams lprams = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
          lprams.Width = (int)(30 * Utils.GetMmPixel(slider));
          slider.LayoutParameters = lprams;
        }
        l.AddView(slider);
        return slider;
      }
    }
  }
}