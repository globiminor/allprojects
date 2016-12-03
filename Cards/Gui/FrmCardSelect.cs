using Cards.Vm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Cards.Gui
{
  public partial class FrmCardSelect : Form
  {
    private List<Card> _availableCards;
    private Dictionary<string, int> _suitesTops;

    public FrmCardSelect()
    {
      InitializeComponent();
    }

    public Card SelectedCard { get; private set; }

    public void SetSelection(CardsVm cardsVm)
    {
      List<Card> cards = cardsVm.Game.Clone().Init();

      IComparer<Card> cmpr = new Card.CardComparer();
      cards.Sort(cmpr);
      foreach (CardPosition pos in cardsVm.Cards.GetCardPositions())
      {
        if (pos.Card.Suite == null)
        { continue; }

        int i = cards.BinarySearch(pos.Card, cmpr);
        cards.RemoveAt(i);
      }
      Dictionary<string, int> suites = new Dictionary<string, int>();
      int wMax = 0;
      foreach (Card card in cards)
      {
        wMax = Math.Max(wMax, card.Height.H);
        if (!suites.ContainsKey(card.Suite.Code))
        { suites[card.Suite.Code] = suites.Count; }
      }

      _availableCards = cards;
      _suitesTops = suites;

      Height = (int)((_suitesTops.Count * _cardHeight + 2 * _h0) * _spacing);
      Width = (int)(((wMax + 1) * _cardWidth + _h0) * _spacing);
    }

    private int _cardHeight = 20;
    private int _cardWidth = 20;
    private double _spacing = 1.1;
    private double _w0 = 0;
    private double _h0 = 0.2;

    private void FrmCardSelect_Paint(object sender, PaintEventArgs e)
    {
      CntCards.PaintCards(e.Graphics, GetPositions(), _cardHeight, _cardWidth, Font);
    }

    private IEnumerable<CardPosition> GetPositions()
    {
      if (_availableCards == null)
      { yield break; }

      foreach (Card card in _availableCards)
      {
        CardPosition pos = new CardPosition();
        pos.Card = card;
        pos.Visible = true;
        double l = card.Height.H * _spacing;
        double t = _suitesTops[card.Suite.Code] * _spacing + _w0;
        pos.Left = l;
        pos.Top = t;

        yield return pos;
      }
    }

    private void FrmCardSelect_Click(object sender, EventArgs e)
    {
      Point p = PointToClient(MousePosition);      
      double x = (double)p.X / _cardWidth;
      double y = (double)p.Y / _cardHeight;
      foreach (CardPosition pos in GetPositions())
      {
        if (pos.Left < x && pos.Left + 1 > x
          && pos.Top < y && pos.Top + 1 > y)
        {
          SelectedCard = pos.Card;
          break;
        }
      }

      Close();
    }
  }
}
