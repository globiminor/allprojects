
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using TennisStat.Data;

namespace TennisStat
{
  [TestClass]
  public class TestAtp
  {
    [TestMethod]
    public void CanGetYear()
    {
      TennisDataset ds = new TennisDataset();
      QueryAtp q = new QueryAtp();
      for (int year = 1965; year < 2018; year++)
      {
        foreach (var evtRow in q.GetTournaments(year, ds))
        {
          //q.GetResults(evtRow);
        }
      }
      using (MySqlConnection conn = new MySqlConnection())
      {
        conn.ConnectionString = "Server=localhost;database=tennis";
        conn.Open();
        using (MySqlCommand cmd = new MySqlCommand())
        {
          cmd.Connection = conn;
          MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
          q.Synch(ds.TournierTable, adapter);
          q.Synch(ds.EventTable, adapter);
        }
      }
    }

    [TestMethod]
    public void GetResults()
    {
      TennisDataset ds = new TennisDataset();
      QueryAtp q = new QueryAtp();
      //for (int year = 1965; year < 2018; year++)
      //{
      //  foreach (var evtRow in q.GetTournaments(year, ds))
      //  {
      //    //q.GetResults(evtRow);
      //  }
      //}
      using (MySqlConnection conn = new MySqlConnection())
      {
        conn.ConnectionString = "Server=localhost;database=tennis";
        conn.Open();
        using (MySqlCommand cmd = new MySqlCommand())
        {
          cmd.Connection = conn;
          MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
          cmd.CommandText = "SELECT * FROM event";
          adapter.Fill(ds.EventTable);

          cmd.CommandText = $"SELECT distinct({MatchFullTable.FkEventColumn.Name}) FROM {ds.MatchFullTable.TableName}";
          adapter.Fill(ds.MatchFullTable);
          ds.MatchFullTable.AcceptChanges();

          DataView vMatch = new DataView(ds.MatchFullTable) { Sort = MatchFullTable.FkEventColumn.Name };

          DateTime maxDate = new DateTime(1978, 1, 1);
          foreach (EventTable.Row evt in ds.EventTable.Rows)
          {
            if (evt.EndDate > maxDate)
            { continue; }
            if (vMatch.Find(evt.IdEvent) >= 0)
            { continue; }

            if (EventTable.ResultRefColumn.IsNull(evt))
            { continue; }

            q.GetResults(evt);
          }
          //q.Synch(ds.TournierTable, adapter);
          //q.Synch(ds.EventTable, adapter);
          q.Synch(ds.MatchFullTable, adapter);
        }
      }
    }

    [TestMethod]
    public void CanGetYear1999()
    {
      TennisDataset ds = new TennisDataset();
      QueryAtp q = new QueryAtp();
      foreach (var evtRow in q.GetTournaments(1999, ds))
      {
        q.GetResults(evtRow);
      }
    }

    [TestMethod]
    public void CanGetAllYears()
    {
      TennisDataset ds = new TennisDataset();
      QueryAtp q = new QueryAtp();
      for (int year = 1915; year < 2016; year++)
      {
        foreach (var href in q.GetTournaments(year, ds))
        {
        }
      }
    }

    [TestMethod]
    public void CanSelectMariaDb()
    {
      using (MySqlConnection conn = new MySqlConnection())
      {
        conn.ConnectionString = "Server=localhost;database=tennis";

        conn.Open();

        MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM Tournier", conn);
        TournierTable tbl = new TournierTable();
        adapter.Fill(tbl);
      }
    }
  }
}
