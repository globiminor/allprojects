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
      PaintCards(e.Graphics, CardsVm.GetCardPositions(), CardWidth, CardHeight, Font);
    }

    public static void PaintCards(Graphics graphics, IEnumerable<CardPosition> cards,
      int cardWidth, int cardHeight, Font font)
    {
      foreach (CardPosition cardPos in cards)
      { PaintCard(graphics, cardPos, cardWidth, cardHeight, font); }
    }
    public static void PaintCard(Graphics graphics, CardPosition card, int cardWidth, int cardHeight, Font font)
    {
      Rectangle rect = new Rectangle((int)(card.Left * cardWidth), (int)(card.Top * cardHeight), cardWidth, cardHeight);
      PaintCard(graphics, card.Card, card.Visible, font, rect);
    }
    public static void PaintCard(Graphics graphics, Card card, bool visible, Font font, Rectangle rect)
    { 
      Brush b;
      if (visible)
      {
        b = Brushes.White;
        if (card.Suite == null)
        { b = Brushes.Orange; }
      }
      else
      { b = Brushes.DarkGray; }

      graphics.FillRectangle(b, rect);

      Pen p = Pens.Black;
      graphics.DrawRectangle(p, rect);

      if (visible)
      { PaintCard(graphics, card, font, rect); }
    }

    private static void PaintCard(Graphics graphics, Card card, Font font, Rectangle rect)
    {
      if (card.Suite == null)
      { return; }

      Color suiteColor;

      string suite = card.Suite.Code;
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

      string txt = symbol + card.Height.Code;
      rect.Height = font.Height;
      TextRenderer.DrawText(graphics, txt, font, rect, suiteColor);
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
          Card setCard = GetCard(from);
          if (setCard != null)
          {
            unknown.Card.Replace(setCard);
            CardsVm.Game.Save();
            Invalidate();
          }
          return;
        }
      }

      CardsVm.Move(fx, fy, tx, ty);

      Invalidate();
    }

    private Card GetCard(Point from)
    {
      if (CardsVm == null)
      { return null; }

      FrmCardSelect frm = new FrmCardSelect();
      frm.SetSelection(CardsVm);

      frm.ShowInTaskbar = false;
      Point screen = PointToScreen(from);
      frm.Top = screen.Y;
      frm.Left = screen.X - frm.Width / 2;

      frm.ShowDialog(TopLevelControl);

      return frm.SelectedCard;
    }
  }
}
