using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Cards.Vm;

namespace Cards.Gui
{
  public partial class CntCards : UserControl
  {
    public CntCards()
    {
      CardWidth = 30;
      CardHeight = 45;
      InitializeComponent();
    }

    public CardsVm CardsVm { get; set; }
    public int CardWidth { get; set; }
    public int CardHeight { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
      e.Graphics.Clear(Color.Green);
      PaintCards(e);
      base.OnPaint(e);
    }

    private void PaintCards(PaintEventArgs e)
    {
      if (CardsVm == null) { return; }

      foreach (CardPosition cardPos in CardsVm.GetCardPositions())
      {
        PaintCard(cardPos, e);
      }
    }

    private void PaintCard(CardPosition card, PaintEventArgs e)
    {
      Brush b;
      if (card.Visible)
      {
        b = Brushes.White;
        if (card.Card.Suite == null)
        { b = Brushes.Orange; }
      }
      else
      { b = Brushes.DarkGray; }
      Pen p = Pens.Black;

      Rectangle rect = new Rectangle((int)(card.Left * CardWidth), (int)(card.Top * CardHeight),
        CardWidth, CardHeight);

      e.Graphics.FillRectangle(b, rect);
      e.Graphics.DrawRectangle(p, rect);

      if (card.Visible)
      {
        Color suiteColor;
        if (card.Card.Suite != null)
        {
          string suite = card.Card.Suite.Code;
          string symbol = string.Empty;
          if (suite == "H")
          {
            suiteColor = Color.Red;
            symbol = "\u2665";
          }
          else if (suite == "E")
          {
            suiteColor = Color.Blue;
            symbol = "\u2666";
          }
          else if (suite == "K")
          {
            suiteColor = Color.Green;
            symbol = "\u2663";
          }
          else if (suite == "S")
          {
            suiteColor = Color.Black;
            symbol = "\u2660";
          }
          else
          { suiteColor = Color.Gray; }

          string txt = symbol + card.Card.Height.Code;
          rect.Height = Font.Height;
          TextRenderer.DrawText(e.Graphics, txt, Font, rect, suiteColor);
        }
      }
    }

    private Point _down;
    private void CntCards_MouseDown(object sender, MouseEventArgs e)
    {
      _down = e.Location;
    }

    private void CntCards_MouseUp(object sender, MouseEventArgs e)
    {
      MoveCard(_down, e.Location);
    }

    private ComboBox _txtEdit;
    private ComboBox TxtEdit
    {
      get { return _txtEdit ?? (_txtEdit = InitEdit()); }
    }
    private ComboBox InitEdit()
    {
      ComboBox txt = new ComboBox();
      txt.DropDownStyle = ComboBoxStyle.DropDownList;
      txt.DrawMode = DrawMode.OwnerDrawVariable;
      txt.DrawItem += Txt_DrawItem;
      return txt;
    }

    private void Txt_DrawItem(object sender, DrawItemEventArgs e)
    {
      throw new NotImplementedException();
    }

    private void MoveCard(Point from, Point to)
    {
      if (CardsVm == null)
      { return; }

      double fx = (double)from.X / CardWidth;
      double fy = (double)from.Y / CardHeight;
      double tx = (double)to.X / CardWidth;
      double ty = (double)to.Y / CardHeight;

      if (fx == tx && fy == ty)
      {
        CardPosition unknown = CardsVm.GetUnknownCard(fx, fy);
        if (unknown != null)
        {
          ComboBox txt = TxtEdit;
          if (!Controls.Contains(txt))
          { Controls.Add(txt); }

          EditVm vm = new EditVm();
          txt.Top = (int)(unknown.Top * CardHeight);
          txt.Left = (int)(unknown.Left * CardWidth);
          txt.Width = CardWidth;
          txt.Height = CardHeight;
          txt.Visible = true;
          txt.DataSource = vm.Cards;
          return;
        }
      }

      CardsVm.Move(fx, fy, tx, ty);

      Invalidate();
    }

    private class EditVm
    {
      private List<string> _suites = new List<string> { "H,E,K,S" };
      public List<string> Cards { get { return _suites; } }
    }
  }
}
