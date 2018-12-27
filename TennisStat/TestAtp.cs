
using Basics.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

          DateTime minDate = new DateTime(2014, 1, 1);
          DateTime maxDate = new DateTime(2017, 8, 15);
          foreach (var row in ds.EventTable.Rows)
          {
            EventTable.Row evt = (EventTable.Row)row;
            if (evt.EndDate < minDate)
            { continue; }
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
    public void GetPlayers()
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

          cmd.CommandText = $"SELECT distinct({MatchFullTable.RefYColumn.Name}) FROM {ds.MatchFullTable.TableName} WHERE {MatchFullTable.FkPlayerYColumn} is null";
          adapter.Fill(ds.MatchFullTable);
          ds.MatchFullTable.AcceptChanges();

          foreach (var row in ds.MatchFullTable.Rows)
          {
            MatchFullTable.Row matchPlayer = (MatchFullTable.Row)row;
            if (MatchFullTable.RefYColumn.IsNull(matchPlayer))
            { continue; }

            string playerRef = MatchFullTable.RefYColumn.GetValue(matchPlayer);
            IList<string> parts = playerRef.Split('/');
            string keyName = parts[parts.Count - 2];
            cmd.CommandText = $"INSERT INTO {ds.PlayerTable.TableName} ({PlayerTable.KeyNameColumn}) VALUES ('{keyName}')";
            cmd.ExecuteNonQuery();

            cmd.CommandText = "SELECT LAST_INSERT_ID()";
            int idPlayer = Convert.ToInt32(cmd.ExecuteScalar());

            //cmd.CommandText = $"UPDATE {ds.MatchFullTable.TableName} SET {MatchFullTable.FkPlayerXColumn} = {idPlayer} WHERE {MatchFullTable.RefXColumn} = '{playerRef}'";
            //cmd.ExecuteNonQuery();

            cmd.CommandText = $"UPDATE {ds.MatchFullTable.TableName} SET {MatchFullTable.FkPlayerYColumn} = {idPlayer} WHERE {MatchFullTable.RefYColumn} = '{playerRef}'";
            cmd.ExecuteNonQuery();
          }
        }
      }

    }

    [TestMethod]
    public void GetMatchCounts()
    {
      QueryAtp q = new QueryAtp();
      q.GetMatchCounts();
    }

    [TestMethod]
    public void GetPlayerRefs()
    {
      TennisDataset ds = new TennisDataset();
      QueryAtp q = new QueryAtp();

      using (MySqlConnection conn = new MySqlConnection())
      {
        conn.ConnectionString = "Server=localhost;database=tennis";
        conn.Open();
        using (MySqlCommand cmd = new MySqlCommand())
        {
          cmd.Connection = conn;
          MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);

          cmd.CommandText = $"SELECT * FROM {ds.MatchFullTable.TableName}";
          adapter.Fill(ds.MatchFullTable);
          ds.MatchFullTable.AcceptChanges();

          cmd.CommandText = $"SELECT * FROM {ds.PlayerTable.TableName}";
          adapter.Fill(ds.PlayerTable);
          ds.PlayerTable.AcceptChanges();
          DataView vPlayer = new DataView(ds.PlayerTable) { Sort = PlayerTable.IdPlayerColumn.Name };

          foreach (var row in ds.MatchFullTable.Rows)
          {
            MatchFullTable.Row matchPlayer = (MatchFullTable.Row)row;
            if (MatchFullTable.RefYColumn.IsNull(matchPlayer))
            { continue; }

            string name = MatchFullTable.NameXColumn.GetValue(matchPlayer);
            string playerRef = MatchFullTable.RefXColumn.GetValue(matchPlayer);
            int idPlayer = MatchFullTable.FkPlayerXColumn.GetValue(matchPlayer);

            SetPlayer(vPlayer, idPlayer, name, playerRef);
          }

          Updater upd = new Updater(adapter, Updater.CommandFormatMode.Auto | Updater.CommandFormatMode.NoKeysReturn);
          upd.UpdateDB(null, ds.PlayerTable);
        }
      }
    }

    private void SetPlayer(DataView vPlayer, int idPlayer, string name, string playerRef)
    {
      PlayerTable.Row player = (PlayerTable.Row)vPlayer[vPlayer.Find(idPlayer)].Row;

      if (PlayerTable.NameColumn.IsNull(player))
      { PlayerTable.NameColumn.SetValue(player, name); }
      if (PlayerTable.NameColumn.GetValue(player) != name)
      { }

      if (PlayerTable.UrlRefColumn.IsNull(player))
      { PlayerTable.UrlRefColumn.SetValue(player, playerRef); }
      if (PlayerTable.UrlRefColumn.GetValue(player) != playerRef)
      { }

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
