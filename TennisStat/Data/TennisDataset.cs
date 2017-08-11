using Basics.Data;
using System.Data;

namespace TennisStat.Data
{
  public class TennisDataset : DataSet
  {
    private static object _lock = new object();
    private static TennisDataset _instance;

    public TournierTable TournierTable { get; private set; }
    public EventTable EventTable { get; private set; }
    public MatchFullTable MatchFullTable { get; private set; }
    public SetTable SetTable { get; private set; }
    public PlayerTable PlayerTable { get; private set; }

    public TypedRelation<TournierTable.Row, EventTable.Row> TournierEventRelation { get; private set; }

    public static TennisDataset Instance
    {
      get
      {
        if (_instance == null)
        {
          lock (_lock)
          {
            if (_instance == null)
            {
              _instance = new TennisDataset();
            }
          }
        }
        return _instance;
      }
    }

    public TennisDataset()
      : this(null)
    {
    }

    public TennisDataset(object context)
    {
      TournierTable = new TournierTable();
      Tables.Add(TournierTable);

      EventTable = new EventTable();
      Tables.Add(EventTable);

      MatchFullTable = new MatchFullTable();
      Tables.Add(MatchFullTable);

      SetTable = new SetTable();
      Tables.Add(SetTable);

      PlayerTable = new PlayerTable();
      Tables.Add(PlayerTable);

      TournierEventRelation = new TypedRelation<TournierTable.Row, EventTable.Row>(
        "TournierEventRelation",
        TournierTable.Columns[TournierTable.IdTournierColumn.Name],
        EventTable.Columns[EventTable.FkTournierColumn.Name]);
      Relations.Add(TournierEventRelation);

      EnforceConstraints = false;
    }

    private DataView _playerKeyView;
    private DataView PlayerKeyView
    {
      get
      {
        if (_playerKeyView == null)
        {
          _playerKeyView = new DataView(PlayerTable);
          _playerKeyView.Sort = PlayerTable.KeyNameColumn.Name;
        }
        return _playerKeyView;
      }
    }
    public PlayerTable.Row GetOrCreatePlayer(string keyName)
    {
      int i = PlayerKeyView.Find(keyName);
      PlayerTable.Row row;
      if (i < 0)
      {
        row = PlayerTable.NewRow();
        row.KeyName = keyName;
        PlayerTable.AddRow(row);
        row.AcceptChanges();
      }
      else { row = (PlayerTable.Row)PlayerKeyView[i].Row; }

      return row;
    }

    private DataView _tournierKeyView;
    private DataView TournierKeyView
    {
      get
      {
        if (_tournierKeyView == null)
        {
          _tournierKeyView = new DataView(TournierTable);
          _tournierKeyView.Sort = TournierTable.NameColumn.Name;
        }
        return _tournierKeyView;
      }
    }
    private DataView _tournierIdView;
    private DataView TournierIdView
    {
      get { return _tournierIdView ?? (_tournierIdView = new DataView(TournierTable) { Sort = TournierTable.IdTournierColumn.Name }); }
    }

    public TournierTable.Row GetOrCreateTournier(int id)
    {
      int i = TournierIdView.Find(id);
      TournierTable.Row row;
      if (i < 0)
      {
        row = TournierTable.NewRow();
        row.IdTournier = id;
        TournierTable.AddRow(row);
        row.AcceptChanges();
      }
      else { row = (TournierTable.Row)TournierIdView[i].Row; }

      return row;
    }

    public TournierTable.Row GetOrCreateTournier(string tournierName)
    {
      int i = TournierKeyView.Find(tournierName);
      TournierTable.Row row;
      if (i < 0)
      {
        row = TournierTable.NewRow();
        row.Name = tournierName;
        TournierTable.AddRow(row);
        row.AcceptChanges();
      }
      else { row = (TournierTable.Row)TournierKeyView[i].Row; }

      return row;
    }

    private DataView _eventKeyView;
    private DataView EventKeyView
    {
      get
      {
        if (_eventKeyView == null)
        {
          _eventKeyView = new DataView(EventTable);
          //_eventKeyView.Sort = EventTable.FullInfoColumn.Name;
        }
        return _eventKeyView;
      }
    }
    public EventTable.Row GetOrCreateEvent(TournierTable.Row tournier, string eventInfo)
    {
      int i = EventKeyView.Find(eventInfo);
      EventTable.Row row;
      if (i < 0)
      {
        row = EventTable.NewRow();
        //row.FullInfo = eventInfo;
        row.FkTournier = tournier.IdTournier;
        EventTable.AddRow(row);
        row.AcceptChanges();
      }
      else { row = (EventTable.Row)EventKeyView[i].Row; }

      return row;
    }

    private DataView _matchKeyView;
    private DataView MatchKeyView
    {
      get
      {
        if (_matchKeyView == null)
        {
          _matchKeyView = new DataView(MatchFullTable);
          _matchKeyView.Sort = string.Format("{0},{1},{2}",
            MatchFullTable.FkPlayerXColumn.Name,
            MatchFullTable.FkPlayerYColumn.Name,
            MatchFullTable.RoundColumn.Name);
        }
        return _matchKeyView;
      }
    }
    public MatchFullTable.Row GetOrCreateMatch(PlayerTable.Row x, PlayerTable.Row y, string round)
    {
      int ix = x.IdPlayer;
      int iy = y.IdPlayer;
      if (ix > iy)
      {
        int t = ix;
        ix = iy;
        iy = t;
      }
      int i = MatchKeyView.Find(new object[] { ix, iy, round });
      MatchFullTable.Row row;
      if (i < 0)
      {
        row = MatchFullTable.NewRow();
        row.FkPlayerX = ix;
        row.FkPlayerY = iy;
        MatchFullTable.RoundColumn.SetValue(row, round);
        MatchFullTable.AddRow(row);
        row.AcceptChanges();
      }
      else { row = (MatchFullTable.Row)MatchKeyView[i].Row; }

      return row;
    }

  }
}
