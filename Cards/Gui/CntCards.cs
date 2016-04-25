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
      CardWidth = 20;
      CardHeight = 30;
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
      { b = Brushes.White; }
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
        string suite = card.Card.Suite.Code;
        if (suite == "H")
        { suiteColor = Color.Red; }
        else if (suite == "E")
        { suiteColor = Color.Blue; }
        else if (suite == "K")
        { suiteColor = Color.Green; }
        else if (suite == "S")
        { suiteColor = Color.Black; }
        else
        { suiteColor = Color.Gray; }

        rect.Height = Font.Height;
        TextRenderer.DrawText(e.Graphics, card.Card.Height.Code, Font, rect, suiteColor);
      }
    }

    private Point _down;
    private void CntCards_MouseDown(object sender, MouseEventArgs e)
    {
      _down = e.Location;
    }

    private void CntCards_MouseUp(object sender, MouseEventArgs e)
    {
      Move(_down, e.Location);
    }

    private void Move(Point from, Point to)
    {
      if (CardsVm == null)
      { return; }

      CardsVm.Move((double)from.X / CardWidth, (double)from.Y / CardHeight,
        (double)to.X / CardWidth, (double)to.Y / CardHeight);

      Invalidate();
    }
  }
}
