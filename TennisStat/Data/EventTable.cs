using Basics.Data;
using System;
using System.Data;

namespace TennisStat.Data
{
  public class EventTable : DataTable
  {
    public class Row : DataRow
    {
      internal Row(DataRowBuilder builder)
        : base(builder)
      { }

      public int IdEvent
      { get { return IdEventColumn.GetValue(this); } }

      public int FkTournier
      {
        get { return FkTournierColumn.GetValue(this); }
        set { FkTournierColumn.SetValue(this, value); }
      }

      public DateTime EndDate
      {
        get { return EndDateColumn.GetValue(this); }
        set { EndDateColumn.SetValue(this, value); }
      }

      public string Place
      {
        get { return PlaceColumn.GetValue(this); }
        set { PlaceColumn.SetValue(this, value); }
      }

      public string Environ
      {
        get { return EnvironColumn.GetValue(this); }
        set { EnvironColumn.SetValue(this, value); }
      }

      public string Surface
      {
        get { return SurfaceColumn.GetValue(this); }
        set { SurfaceColumn.SetValue(this, value); }
      }

      //public string FullInfo
      //{
      //  get { return FullInfoColumn.GetValue(this); }
      //  set { FullInfoColumn.SetValue(this, value); }
      //}

      public string ResultRef
      {
        get { return ResultRefColumn.GetValue(this); }
        set { ResultRefColumn.SetValue(this, value); }
      }

    }

    public static readonly TypedColumn<int> IdEventColumn = new TypedColumn<int>("IdEvent");
    public static readonly TypedColumn<int> FkTournierColumn = new TypedColumn<int>("FkTournier");
    public static readonly TypedColumn<string> ImportanceColumn = new TypedColumn<string>("Importance");
    public static readonly TypedColumn<string> PlaceColumn = new TypedColumn<string>("Place");
    public static readonly TypedColumn<string> EnvironColumn = new TypedColumn<string>("Environ");
    public static readonly TypedColumn<string> SurfaceColumn = new TypedColumn<string>("Surface");
    public static readonly TypedColumn<DateTime> EndDateColumn = new TypedColumn<DateTime>("EndDate");
    public static readonly TypedColumn<string> ResultRefColumn = new TypedColumn<string>("ResultRef");
    //public static readonly TypedColumn<string> FullInfoColumn = new TypedColumn<string>("FullInfo");

    public EventTable()
      : base("Event")
    {
      IdEventColumn.CreatePrimaryKey(this);
      Columns.Add(FkTournierColumn.CreateColumn());
      Columns.Add(ImportanceColumn.CreateColumn());
      Columns.Add(PlaceColumn.CreateColumn());
      Columns.Add(EnvironColumn.CreateColumn());
      Columns.Add(SurfaceColumn.CreateColumn());
      Columns.Add(EndDateColumn.CreateColumn());
      Columns.Add(ResultRefColumn.CreateColumn());
      //Columns.Add(FullInfoColumn.CreateColumn());
    }

    public new Row NewRow()
    {
      return (Row)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
      Row row = new Row(builder);
      return row;
    }

    public void AddRow(Row row)
    {
      Rows.Add(row);
    }
  }
}
