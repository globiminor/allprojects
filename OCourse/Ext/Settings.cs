using System.Collections.Generic;
using System.Data;
using System.IO;
using Basics.Data;
using Ocad;

namespace OCourse.Ext
{
  class Settings
  {
    public readonly LayoutTable Layout;
    private string _courseFile;
    private Settings()
    {
      Layout = new LayoutTable();
    }
    public string CourseFile
    {
      get { return _courseFile; }
      set { _courseFile = value; }
    }

    public static Settings GetSettings(string orig)
    {
      using (OcadReader reader = OcadReader.Open(orig))
      {
        Settings settings = GetSettings(reader, null);
        return settings;
      }
    }

    public static Settings GetSettings(OcadReader reader, IList<ElementIndex> indexList)
    {
      foreach (var element in reader.EnumMapElements(indexList: indexList))
      {
        if (element.Symbol == 723000)
        {
          string eText = element.Text;

          MemoryStream stream = new MemoryStream();
          MemoryStream read = new MemoryStream();
          using (TextWriter w = new StreamWriter(stream))
          {
            w.WriteLine("<Settings xmlns=\"http://tempuri.org/Settings.xsd\">");
            w.WriteLine(eText);
            w.WriteLine("</Settings>");
            w.Flush();

            stream.WriteTo(read);
          }

          read.Seek(0, SeekOrigin.Begin);

          DataSet importDs = new DataSet();
          importDs.ReadXml(read, XmlReadMode.InferSchema);

          Settings settings = new Settings();
          foreach (var row in importDs.Tables[settings.Layout.TableName].Rows.Enum())
          {
            settings.Layout.ImportRow(row);
          }
          return settings;
        }
      }
      return null;
    }

    public class LayoutTable : DataTable
    {
      public class Row : DataRow
      {
        internal Row(DataRowBuilder builder)
          : base(builder)
        { }

        public string Id
        {
          get { return IdColumn.GetValue(this); }
          set { IdColumn.SetValue(this, value); }
        }
        public bool Default
        {
          get { return DefaultColumn.GetValue(this); }
          set { DefaultColumn.SetValue(this, value); }
        }
        public bool IsDefaultNull()
        {
          return DefaultColumn.IsNull(this);
        }
        public string Course
        {
          get { return CourseColumn.GetValue(this); }
          set { CourseColumn.SetValue(this, value); }
        }
      }

      public static readonly TypedColumn<string> IdColumn = new TypedColumn<string>("Id");
      public static readonly TypedColumn<bool> DefaultColumn = new TypedColumn<bool>("Default");
      public static readonly TypedColumn<string> CourseColumn = new TypedColumn<string>("Course");

      public LayoutTable()
        : base("Layout")
      {
        Columns.Add(IdColumn.CreateColumn());
        Columns.Add(DefaultColumn.CreateColumn());
        Columns.Add(CourseColumn.CreateColumn());
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

      public Row AddRow()
      {
        Row row = NewRow();
        Rows.Add(row);
        return row;
      }
    }

  }
}
