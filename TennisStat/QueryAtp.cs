using Basics.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Text;
using TennisStat.Data;

namespace TennisStat
{
  public class QueryAtp
  {
    public Dictionary<string, int> GetMatchCounts()
    {
      string result;
      using (WebClient webClient = new WebClient())
      {
        string url = string.Format(@"http://www.atpworldtour.com/en/content/ajax/fedex-performance-full-table/career/All/All");

        byte[] resultStream = webClient.UploadValues(url, "POST", new NameValueCollection());
        result = Encoding.UTF8.GetString(resultStream);
      }
      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(result);

      Dictionary<string, int> matchDict = new Dictionary<string, int>();
      HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//tbody[@id='winLossTableContent']");
      foreach (HtmlNode player in nodes[0].SelectNodes("tr"))
      {
        string playerRef = player.SelectSingleNode("td[@class='player-cell']").SelectSingleNode("a").Attributes["href"].Value;
        string stats = player.SelectSingleNode("td[@class='fifty-two-week-win-loss-cell']").InnerText;
        IList<string> winLoss = stats.Split('-');
        int nMatches = int.Parse(winLoss[0]) + int.Parse(winLoss[1]);

        matchDict.Add(playerRef, nMatches);
      }

      return matchDict;
    }
    public void GetResults(PlayerTable.Row player, int year)
    {
      using (WebClient webClient = new WebClient())
      {
        string url = string.Format(@"http://www.atpworldtour.com/{0}.aspx?t=pa&y={1}&m=s&e=0",
          player.KeyName, year);

        byte[] resultStream = webClient.UploadValues(url, "POST", new NameValueCollection());

        string result = Encoding.UTF8.GetString(resultStream);
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(result);
        // HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div");
        HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='commonProfileContainer']");

        foreach (HtmlNode node in nodes)
        {
          HtmlNodeCollection pnodes = node.SelectNodes("p[@class='bioPlayActivityInfo']");
          if (pnodes == null)
          { continue; }

          HtmlNodeCollection tnodes = node.SelectNodes("div/table/tbody/tr");
          if (tnodes == null)
          { continue; }

          HtmlNode tournierNode = node.ChildNodes[2];
          HtmlNode eventNode = node.ChildNodes[3];
          TournierTable.Row tournier = ((TennisDataset)player.Table.DataSet).GetOrCreateTournier(tournierNode.InnerText);
          EventTable.Row eventRow = ((TennisDataset)player.Table.DataSet).GetOrCreateEvent(tournier, eventNode.InnerText);

          foreach (HtmlNode tnode in tnodes)
          {
            HtmlNodeCollection tds = tnode.SelectNodes("td");
            if (tds == null || tds.Count < 4)
            { continue; }
            if (tds[0].Attributes.Contains("width"))
            { continue; }

            string round = tds[0].InnerText;

            string opp = tds[1].InnerText;
            if (opp.Split()[0] == "Bye")
            { continue; }
            string oppKey = tds[1].SelectSingleNode("a").Attributes["href"].Value;

            PlayerTable.Row oppPlayer = ((TennisDataset)player.Table.DataSet).GetOrCreatePlayer(oppKey);
            int oppRanking;
            int.TryParse(tds[2].InnerText, out oppRanking);

            MatchFullTable.Row match = ((TennisDataset)player.Table.DataSet).GetOrCreateMatch(player, oppPlayer, round);
            string rawScore = tds[3].InnerText;
          }
        }
      }
    }

    public void Synch(TournierTable tournierTable, DbDataAdapter ada)
    {
      ada.SelectCommand.CommandText = $"SELECT * FROM {tournierTable.TableName}";
      TournierTable dbTournier = new TournierTable();
      ada.Fill(dbTournier);
      DataView vTounier = new DataView(dbTournier) { Sort = $"{TournierTable.IdTournierColumn.Name}" };
      foreach (TournierTable.Row rowTournier in tournierTable.Rows)
      {
        rowTournier.AcceptChanges();
        if (vTounier.Find(rowTournier.IdTournier) < 0)
        { rowTournier.SetAdded(); }
      }

      Updater upd = new Updater(ada, Updater.CommandFormatMode.Auto);
      upd.UpdateDB(null, tournierTable);
    }

    public void Synch(EventTable eventTable, DbDataAdapter ada)
    {
      ada.SelectCommand.CommandText = $"SELECT * FROM {eventTable.TableName}";
      EventTable dbEvent = new EventTable();
      ada.Fill(dbEvent);
      eventTable.BeginLoadData();
      DataView vTounier = new DataView(dbEvent) { Sort = $"{EventTable.FkTournierColumn.Name},{EventTable.EndDateColumn.Name}" };
      foreach (EventTable.Row rowEvent in eventTable.Rows)
      {
        rowEvent.AcceptChanges();
        if (EventTable.FkTournierColumn.IsNull(rowEvent))
        { continue; }

        int idx = vTounier.Find(new object[] { rowEvent.FkTournier, rowEvent.EndDate });
        if (idx < 0)
        { rowEvent.SetAdded(); }
        else
        {
          EventTable.IdEventColumn.SetValue(rowEvent, EventTable.IdEventColumn.GetValue(vTounier[idx].Row));
          rowEvent.AcceptChanges();
        }
      }

      Updater upd = new Updater(ada, Updater.CommandFormatMode.Auto | Updater.CommandFormatMode.NoKeysReturn);
      upd.UpdateDB(null, eventTable);

      eventTable.EndLoadData();
    }

    public void Synch(MatchFullTable matchTable, DbDataAdapter ada)
    {
      Updater upd = new Updater(ada, Updater.CommandFormatMode.Auto | Updater.CommandFormatMode.NoKeysReturn);
      upd.UpdateDB(null, matchTable);
    }


    public bool GetResults(EventTable.Row evt)
    {
      string result;
      using (WebClient webClient = new WebClient())
      {
        string url = $"http://www.atpworldtour.com{evt.ResultRef}";

        byte[] resultStream = webClient.UploadValues(url, "POST", new NameValueCollection());

        result = Encoding.UTF8.GetString(resultStream);
      }
      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(result);

      HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//table[@class='day-table']");
      if (nodes == null)
      { return false; }
      if (nodes?.Count != 1) throw new InvalidOperationException("day table");
      HtmlNode nodeTable = nodes[0];

      TennisDataset td = null;
      string round = null;
      foreach (HtmlNode rec in nodeTable.ChildNodes)
      {
        if (rec.Name == "#text") { continue; }
        if (rec.Name == "thead")
        {
          round = rec.SelectNodes("tr")[0].InnerText.Trim();
          td = td ?? (TennisDataset)evt.Table.DataSet;
        }
        if (rec.Name == "tbody")
        {
          HtmlNodeCollection matches = rec.SelectNodes("tr");
          foreach (HtmlNode match in matches)
          {
            HtmlNodeCollection cols = match.SelectNodes("td");
            string winNat = cols[1].SelectNodes("img")?[0].Attributes["alt"].Value;
            string winRef = cols[2].SelectNodes("a")[0].Attributes["href"].Value;
            string winName = cols[2].SelectNodes("a")[0].InnerText.Trim();
            string cmp = cols[3].SelectNodes("span")[0].InnerText.Trim();

            string lostNat = cols[5].SelectNodes("img")?[0].Attributes["alt"].Value;
            string lostRef = cols[6].SelectNodes("a")?[0].Attributes["href"].Value;
            string lostName = cols[6].SelectNodes("a")?[0].InnerText.Trim();
            string score = cols[7].SelectNodes("a")?[0].InnerText.Trim();

            if (cmp != "Defeats" && cols[6].InnerText.Trim() != "Bye1")
            {
              if (lostNat == null && lostRef == null && lostName == null && score == null)
              { }
              else
              { throw new InvalidProgramException($"Unexpected '{cmp}'"); }
            }

            td = td ?? (TennisDataset)evt.Table.DataSet;

            MatchFullTable.Row mRow = td.MatchFullTable.NewRow();
            MatchFullTable.FkEventColumn.SetValue(mRow, evt.IdEvent);
            MatchFullTable.RoundColumn.SetValue(mRow, round);

            MatchFullTable.RefXColumn.SetValue(mRow, winRef);
            MatchFullTable.NameXColumn.SetValue(mRow, winName);
            MatchFullTable.NatXColumn.SetValue(mRow, winNat);

            MatchFullTable.RefYColumn.SetValue(mRow, lostRef);
            MatchFullTable.NameYColumn.SetValue(mRow, lostName);
            MatchFullTable.NatYColumn.SetValue(mRow, lostNat);

            MatchFullTable.ScoreColumn.SetValue(mRow, score);

            MatchFullTable.XWinnerColumn.SetValue(mRow, true);

            td.MatchFullTable.AddRow(mRow);
            //mRow.AcceptChanges();

            //CreateSets(td, MatchTable.IdMatchColumn.GetValue(mRow), score);
          }
        }
      }
      return true;
    }

    private void CreateSets(TennisDataset td, int idMatch, string score)
    {
      if (string.IsNullOrEmpty(score))
      { return; }

      IList<string> sets = score.Split();
      int setNr = 0;
      foreach (string set in sets)
      {
        setNr++;
        SetTable.Row setRow = td.SetTable.NewRow();
        SetTable.FkMatchColumn.SetValue(setRow, idMatch);
        SetTable.SetNrColumn.SetValue(setRow, setNr);
        if (set.Length == 2)
        {
          SetTable.GamesXColumn.SetValue(setRow, int.Parse(set.Substring(0, 1)));
          SetTable.GamesYColumn.SetValue(setRow, int.Parse(set.Substring(1)));
        }
        else if (set == "(W/O)")
        { SetTable.SpezialColumn.SetValue(setRow, set); }
        else if (set == "(RET)")
        { SetTable.SpezialColumn.SetValue(setRow, set); }
        else if (set == "(DEF)")
        { SetTable.SpezialColumn.SetValue(setRow, set); }
        else
        {
          int set_ = int.Parse(set);
          int split = 10;
          while (Math.Abs((set_ / split) - (set_ % split)) != 2)
          {
            split *= 10;
            if (split > 100000)
            {
              string tie = set.Substring(0, 2);
              if (tie == "67" || tie == "76" || tie == "75" || tie == "64") // 75 ??
              {
                SetTable.GamesXColumn.SetValue(setRow, int.Parse(set.Substring(0, 1)));
                SetTable.GamesYColumn.SetValue(setRow, int.Parse(set.Substring(1, 1)));

                SetTable.TieBreakColumn.SetValue(setRow, int.Parse(set.Substring(2)));
              }
              else
              { throw new InvalidOperationException("split"); }
              break;
            }
          }

          SetTable.GamesXColumn.SetValue(setRow, set_ / split);
          SetTable.GamesYColumn.SetValue(setRow, set_ % split);
        }

        td.SetTable.AddRow(setRow);
        setRow.AcceptChanges();
      }
    }

    public IEnumerable<EventTable.Row> GetTournaments(int year, TennisDataset ds)
    {
      string result;
      using (WebClient webClient = new WebClient())
      {
        string url = $"http://www.atpworldtour.com/en/scores/results-archive?year={year}";

        byte[] resultStream = webClient.UploadValues(url, "POST", new NameValueCollection());

        result = Encoding.UTF8.GetString(resultStream);
      }
      HtmlDocument doc = new HtmlDocument();
      doc.LoadHtml(result);

      HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//table[@class='results-archive-table mega-table']");
      if (nodes?.Count != 1) throw new InvalidOperationException("archive table");
      HtmlNode nodeTable = nodes[0];
      nodes = nodeTable.SelectNodes("//tbody");
      if (nodes?.Count != 1) throw new InvalidOperationException("table entries");
      HtmlNode nodeEvents = nodes[0];

      foreach (HtmlNode nodeEvent in nodeEvents.ChildNodes)
      {
        EventTable.Row eventRow = GetEvent(nodeEvent, ds);
        if (eventRow != null)
        { yield return eventRow; }
      }
    }

    private EventTable.Row GetEvent(HtmlNode nodeEvent, TennisDataset ds)
    {
      string title = null;
      string location = null;
      DateTime date = default(DateTime);
      string sglRef = null;
      string dblRef = null;
      string importance = null;
      string resultRef = null;
      string environ = null;
      string surface = null;

      foreach (HtmlNode eventInfo in nodeEvent.ChildNodes)
      {
        if (eventInfo.Attributes["class"]?.Value == "tourney-badge-wrapper")
        {
          string img = eventInfo.SelectNodes("img")?[0].Attributes["src"]?.Value;
          if (img != null)
          { importance = img.Split('_')[1]; }
        }
        if (eventInfo.Attributes["class"]?.Value == "title-content")
        {
          foreach (HtmlNode titleNode in eventInfo.ChildNodes)
          {
            if (titleNode.Attributes["class"]?.Value == "tourney-title")
            { title = titleNode.InnerText.Trim(); }
            if (titleNode.Attributes["class"]?.Value == "tourney-location")
            { location = titleNode.InnerText.Trim(); }
            if (titleNode.Attributes["class"]?.Value == "tourney-dates")
            {
              IList<string> sDate = titleNode.InnerText.Trim().Split('.');
              if (sDate.Count != 1)
              { date = new DateTime(int.Parse(sDate[0]), int.Parse(sDate[1]), int.Parse(sDate[2])); }
            }
          }
        }
        if (eventInfo.Attributes["class"]?.Value == "tourney-details")
        {
          foreach (HtmlNode detailNode in eventInfo.ChildNodes)
          {
            if (detailNode.Attributes["href"] != null && detailNode.Name == "a")
            {
              resultRef = detailNode.Attributes["href"].Value;
            }

            if (detailNode.Attributes["class"]?.Value == "info-area")
            {
              foreach (HtmlNode areaNode in detailNode.ChildNodes)
              {
                if (areaNode.Attributes["class"]?.Value == "item-details")
                {
                  bool sgl = false;
                  bool dbl = false;
                  foreach (HtmlNode detNode in areaNode.ChildNodes)
                  {
                    if (detNode.InnerText.Trim() == "SGL")
                    { sgl = true; dbl = false; }
                    if (detNode.InnerText.Trim() == "DBL")
                    { sgl = false; dbl = true; }
                    if (detNode.InnerText.Trim() == "Outdoor")
                    { environ = "Outdoor"; }
                    if (detNode.InnerText.Trim() == "Indoor")
                    { environ = "Indoor"; }
                    if (detNode.Attributes["href"] != null)
                    {
                      if (sgl) { sglRef = detNode.Attributes["href"].Value; }
                      if (dbl) { dblRef = detNode.Attributes["href"].Value; }
                    }
                    if (environ != null && detNode.Attributes["class"]?.Value == "item-value" && detNode.Name == "span")
                    {
                      surface = detNode.InnerText.Trim();
                    }
                  }
                }
              }
            }
          }
        }

      }

      if (title != null)
      {
        TournierTable.Row tournier = null;
        if (resultRef != null)
        {
          IList<string> parts = resultRef.Split('/');
          string name = parts[4];
          int id = int.Parse(parts[5]);
          //int year = int.Parse(parts[6]);

          tournier = ds.GetOrCreateTournier(id);
          if (tournier.IsNull(TournierTable.NameColumn.Name))
          { tournier.Name = name; }
          else if (tournier.Name != name)
          { }
        }
        else if (title.Equals("nice", StringComparison.InvariantCultureIgnoreCase))
        {
          int id = 6120;
          tournier = ds.GetOrCreateTournier(id);
          tournier.Name = title;
        }

        EventTable.Row eventRow = ds.EventTable.NewRow();
        if (tournier != null)
        { eventRow.FkTournier = tournier.IdTournier; }
        EventTable.ImportanceColumn.SetValue(eventRow, importance);
        eventRow.EndDate = date;
        eventRow.Place = location;
        eventRow.Environ = environ;
        eventRow.Surface = surface;

        eventRow.ResultRef = resultRef;

        ds.EventTable.AddRow(eventRow);
        eventRow.AcceptChanges();

        return eventRow;
      }
      return null;
    }
  }
}
