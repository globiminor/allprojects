using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Basics.Geom;
using Basics.Geom.Projection;

namespace Basics.Data
{
  public abstract class DbBaseCommand : DbCommand
  {
    #region nested classes

    protected class FieldExpression : ExpressionInfo
    {
      string _fieldName;

      public FieldExpression(string expression, BaseCommand parent)
        : base(expression, parent)
      {
        _fieldName = expression;
      }

      public string FieldName
      {
        get { return _fieldName; }
      }
    }

    protected class ExpressionInfo
    {
      private string _expression;
      private BaseCommand _parent;

      private List<ParameterInfo> _params;
      private List<Function> _funcList;
      private DataRow _row;

      public ExpressionInfo(string expression, BaseCommand parent)
      {
        if (expression == null)
        { throw new ArgumentNullException("expression"); }
        if (parent == null)
        { throw new ArgumentNullException("parent"); }

        _expression = expression;
        _parent = parent;
      }

      public string Expression
      {
        get { return _expression; }
      }

      public List<ParameterInfo> Parameters
      {
        get
        {
          if (_params == null)
          { Init(_expression); }
          return _params;
        }
      }

      public List<Function> Functions
      {
        get
        {
          if (_funcList == null)
          { Init(_expression); }
          return _funcList;
        }
      }

      private void Init(string text)
      {
        int position = 0;
        int n = text.Length;

        List<ParameterInfo> paramList = new List<ParameterInfo>();
        List<Function> funcList = new List<Function>();
        Init(paramList, funcList, text, position, null, n, _parent);
        _params = paramList;
        _funcList = funcList;
      }

      public static int Init(List<ParameterInfo> paramList, List<Function> funcList,
        string text, int position, IList<char> end, int n, BaseCommand parent)
      {
        while (position < n)
        {
          if (end != null)
          {
            foreach (char c in end)
            {
              if (text[position] == c)
              { return position; }
            }
          }
          if (text[position] == '\'')
          {
            position = ReadString(text, position, n);
          }
          if (text[position] == '(')
          {
            int funcEnd = position - 1;

            position = Init(paramList, funcList, text, position + 1, new char[] { ')' }, n, parent);
            string funcParams = text.Substring(funcEnd + 1, position - funcEnd);

            while (funcEnd >= 0 && char.IsWhiteSpace(text[funcEnd]))
            { funcEnd--; }
            if (funcEnd >= 0 && (char.IsLetterOrDigit(text[funcEnd]) || text[funcEnd] == '_'))
            {
              int funcStart = funcEnd;
              while (funcStart >= 0 && (char.IsLetterOrDigit(text[funcStart]) || text[funcStart] == '_'))
              { funcStart--; }

              funcStart++;
              string funcName = text.Substring(funcStart, funcEnd + 1 - funcStart);
              funcList.Add(new Function(funcName, funcParams, funcStart, position, parent));
            }

          }
          if (text[position] == '?')
          {
            paramList.Add(new ParameterInfo(position, parent));
          }
          if (text[position] == ':')
          {
            int pos = ReadParameter(text, position);
            string paramName = text.Substring(position + 1, pos - position);
            paramList.Add(new ParameterInfo(position, paramName, parent));
            position = pos;
          }

          position++;
        }
        return position;
      }

      private static int ReadString(string text, int position, int n)
      {
        position++;
        while (position < n && text[position] != '\'')
        { position++; }
        if (position >= n)
        { throw new InvalidOperationException(ErrorText("missing end string separator", text, position)); }
        return position;
      }

      private static int ReadParameter(string text, int position)
      {
        position++;
        while (position < text.Length && char.IsLetterOrDigit(text[position]))
        {
          position++;
        }
        return position - 1;
      }


      public ParameterInfo GetParameter(int position)
      {
        ParameterInfo before = null;
        foreach (ParameterInfo parameter in Parameters)
        {
          if (parameter.Position <= position)
          { before = parameter; }
        }
        return before;
      }

      private DataRow Row
      {
        get
        {
          if (_row == null)
          {
            // create a template row

            // Add parameter columns
            DataTable tbl = new DataTable();
            StringBuilder expression = new StringBuilder(_expression);
            char currentChar = 'A';
            List<ParameterInfo> parameters = Parameters;
            int nParam = parameters.Count;
            for (int iParam = nParam - 1; iParam >= 0; iParam--)
            {
              ParameterInfo parameter = parameters[iParam];
              string columnName = ":" + currentChar;
              tbl.Columns.Add(columnName, typeof(object));
              expression.Replace("?", columnName, parameter.Position, 1);

              currentChar++;
            }

            // add attributes
            int i = 0;
            int n = _expression.Length;
            while (i < n)
            {
              char c = _expression[i];
              i++;
              if (c == '\'')
              {
                while (_expression[i] != '\'')
                { i++; }
                i++;

                continue;
              }
              else if (char.IsLetter(c) || c == '_')
              {
                int j = i - 1;
                while (i < n)
                {
                  char d = _expression[i];
                  i++;
                  if (char.IsLetterOrDigit(d) == false && d != '_' && d != '.')
                  {
                    break;
                  }
                }
                string attr = _expression.Substring(j, i - j);
                tbl.Columns.Add(attr, typeof(object));
              }

            }

            tbl.Columns.Add("__Expression", typeof(object), expression.ToString());

            _row = tbl.NewRow();
            tbl.Rows.Add(_row);
            tbl.AcceptChanges();
          }
          return _row;
        }
      }

      public object Value(DataRow r)
      {
        DataRow row = Row;
        int i = 0;
        foreach (ParameterInfo parameter in Parameters)
        {
          row[i] = parameter.DbParameter.Value;
          i++;
        }
        int n = row.Table.Columns.Count - 1;
        for (int j = i; j < n; j++)
        {
          int iField = r.Table.Columns.IndexOf(row.Table.Columns[j].ColumnName);
          if (iField >= 0)
          { row[j] = r[iField]; }
        }
        return row[n];
      }
    }

    protected class SelectCommand : WhereCommand
    {
      List<FieldExpression> _fields;

      public SelectCommand(DbBaseCommand baseCommand)
        : base(baseCommand)
      {
        string text;
        int position;
        InitCommand("SELECT", out text, out position);

        _fields = new List<FieldExpression>();
        string next = null;
        bool nextField = true;
        while (nextField)
        {
          StringBuilder fieldExpression = new StringBuilder();
          GetExpression(fieldExpression, text, ref position, out next, new char[] { ',' }, "FROM");

          FieldExpression field = new FieldExpression(fieldExpression.ToString(), this);

          _fields.Add(field);

          nextField = false;
          if (next == ",")
          {
            nextField = true;
            position++;
          }
        }

        string from = next;
        if (from == null || from.Equals("FROM", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected FROM", text, position)); }

        BaseCmnd.InitTable(text, ref position);

        InitWhere(text, position, true);
      }

      protected override IEnumerable<ParameterInfo> ParameterInfos
      {
        get
        {
          foreach (FieldExpression field in _fields)
          {
            foreach (ParameterInfo param in field.Parameters)
            {
              yield return param;
            }
          }

          foreach (ParameterInfo param in WhereParameterInfos)
          {
            yield return param;
          }
        }
      }

      public List<FieldExpression> Fields
      {
        get { return _fields; }
      }
    }

    protected class InsertCommand : BaseCommand
    {
      Dictionary<FieldInfo, ExpressionInfo> _fields;
      Dictionary<ExpressionInfo, ParameterInfo> _returnValues;

      public InsertCommand(DbBaseCommand baseCommand)
        : base(baseCommand)
      {

        string text;
        int position;
        InitCommand("INSERT", out text, out position);

        string into = GetPart(text, ref position);
        if (into.Equals("INTO", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected INTO", text, position)); }

        BaseCmnd.InitTable(text, ref position);

        InitInsertValues(text, ref position);

        InitReturnValues(text, ref position);
      }

      private void InitInsertValues(string text, ref int position)
      {
        bool nextField;

        List<string> fields = new List<string>();
        nextField = true;
        if (text[position] == '(')
        {

          position++;
          while (nextField)
          {
            string field = GetField(text, ref position, BaseCmnd.FullSchema, ',', ')').Trim();
            fields.Add(field);

            GetPart(text, ref position, ',', ')');

            nextField = (text[position] == ',');
            if (nextField)
            { position++; }
          }

          if (text[position] != ')')
          { throw new InvalidOperationException(ErrorText("Missing )", text, position)); }
          position++;
        }
        else
        {
          { throw new NotImplementedException("Cannot handle INSERT without explicit fields"); }
        }

        string values = GetPart(text, ref position, '(').Trim();
        if (values.Equals("VALUES", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected VALUES", text, position)); }

        GetPart(text, ref position, '(');
        if (text[position] != '(')
        { throw new InvalidOperationException(ErrorText("Expected (", text, position)); }
        position++;

        List<ExpressionInfo> expressions = new List<ExpressionInfo>();
        nextField = true;
        while (nextField)
        {
          string next;
          StringBuilder expressionBuilder = new StringBuilder();
          GetExpression(expressionBuilder, text, ref position, out next, new char[] { ',', ')' });
          expressions.Add(new ExpressionInfo(expressionBuilder.ToString(), this));

          nextField = (text[position] == ',');
          if (nextField)
          { position++; }
        }

        if (text[position] != ')')
        { throw new InvalidOperationException(ErrorText("Missing )", text, position)); }
        position++;


        int n = expressions.Count;
        if (fields.Count != n)
        { throw new InvalidOperationException(ErrorText("Different number of INSERT fields AND values ", text, position)); }

        _fields = new Dictionary<FieldInfo, ExpressionInfo>();
        for (int i = 0; i < n; i++)
        {
          _fields.Add(new FieldInfo(fields[i]), expressions[i]);
        }
      }

      private void InitReturnValues(string text, ref int position)
      {
        string returning = GetPart(text, ref position);
        if (position < text.Length)
        {
          List<ExpressionInfo> fields = new List<ExpressionInfo>();

          bool nextField;

          if (returning.Trim().Equals("RETURNING", StringComparison.InvariantCultureIgnoreCase) == false)
          { throw new InvalidOperationException(ErrorText("Expected RETURNING", text, position)); }

          nextField = true;
          string next = null;
          while (nextField)
          {
            StringBuilder valueBuilder = new StringBuilder();
            GetExpression(valueBuilder, text, ref position, out next, new char[] { ',' }, "INTO");

            fields.Add(new ExpressionInfo(valueBuilder.ToString(), this));

            if (next != null && next == ",")
            {
              nextField = true;
              position++;
            }
            else
            { nextField = false; }
          }

          if (next == null || next.Equals("INTO", StringComparison.InvariantCultureIgnoreCase) == false)
          { throw new InvalidOperationException(ErrorText("Expected INTO", text, position)); }

          List<ParameterInfo> valueParams = new List<ParameterInfo>();
          nextField = true;
          while (nextField)
          {
            string value = GetPart(text, ref position, ',').Trim();
            if (string.IsNullOrEmpty(value))
            { throw new InvalidOperationException(ErrorText("Empty Value", text, position)); }

            if (value == "?")
            { valueParams.Add(new ParameterInfo(position, this)); }
            else
            { throw new InvalidOperationException(ErrorText("No parameter given", text, position)); }

            if (position < text.Length)
            { nextField = (text[position] == ','); }
            else
            { nextField = false; }

            if (nextField)
            { position++; }
          }

          _returnValues = new Dictionary<ExpressionInfo, ParameterInfo>();
          int n = valueParams.Count;
          if (fields.Count != n)
          { throw new InvalidOperationException(ErrorText("Different number of RETURN fields AND parameters ", text, position)); }
          for (int i = 0; i < n; i++)
          {
            _returnValues.Add(fields[i], valueParams[i]);
          }
        }
      }

      protected override IEnumerable<ParameterInfo> ParameterInfos
      {
        get
        {
          foreach (ExpressionInfo values in _fields.Values)
          {
            foreach (ParameterInfo param in values.Parameters)
            { yield return param; }
          }

          foreach (ExpressionInfo values in _returnValues.Keys)
          {
            foreach (ParameterInfo param in values.Parameters)
            { yield return param; }
          }

          foreach (ParameterInfo param in _returnValues.Values)
          {
            yield return param;
          }

        }
      }

      public Dictionary<int, object> GetInputFields(DataRow row)
      {
        Dictionary<int, object> values = new Dictionary<int, object>();
        foreach (KeyValuePair<FieldInfo, ExpressionInfo> pair in _fields)
        {
          object value = pair.Value.Value(row);

          values.Add(pair.Key.FieldIndex(row), value);
        }
        return values;
      }

      public Dictionary<DbParameter, object> GetReturnValues(DataRow row)
      {
        Dictionary<DbParameter, object> values = new Dictionary<DbParameter, object>();

        if (_returnValues == null)
        { return values; }

        foreach (KeyValuePair<ExpressionInfo, ParameterInfo> pair in _returnValues)
        {
          object value = pair.Key.Value(row);
          values.Add(pair.Value.DbParameter, value);
        }
        return values;
      }
    }

    protected class UpdateCommand : WhereCommand
    {
      Dictionary<FieldInfo, ExpressionInfo> _updateFields;

      public UpdateCommand(DbBaseCommand baseCommand)
        : base(baseCommand)
      {
        string text;
        int position;
        InitCommand("UPDATE", out text, out position);

        BaseCmnd.InitTable(text, ref position);

        bool nextField;

        string set = GetPart(text, ref position);
        if (set.Equals("SET", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected SET", text, position)); }


        _updateFields = new Dictionary<FieldInfo, ExpressionInfo>();

        nextField = true;
        string next = "";
        while (nextField)
        {

          string field = GetPart(text, ref position, '=').Trim();
          if (text[position] != '=')
          { throw new InvalidOperationException(ErrorText("Expected =", text, position)); }

          position++;

          StringBuilder expressionBuilder = new StringBuilder();
          GetExpression(expressionBuilder, text, ref position, out next, new char[] { ',' }, "WHERE");

          ExpressionInfo expression = new ExpressionInfo(expressionBuilder.ToString(), this);
          _updateFields.Add(new FieldInfo(field), expression);

          if (next == ",")
          {
            nextField = true;
            position++;
          }
          else
          { nextField = false; }
        }

        if (position >= text.Length)
        { return; }
        if (next.Equals("WHERE", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected WHERE", text, position)); }

        InitWhere(text, position, false);
      }

      public IEnumerable<ParameterInfo> UpdateParameterInfos
      {
        get
        {
          foreach (ExpressionInfo value in _updateFields.Values)
          {
            foreach (ParameterInfo param in value.Parameters)
            {
              yield return param;
            }
          }
        }
      }

      protected override IEnumerable<ParameterInfo> ParameterInfos
      {
        get
        {
          foreach (ParameterInfo param in UpdateParameterInfos)
          {
            yield return param;
          }

          foreach (ParameterInfo param in WhereParameterInfos)
          {
            yield return param;
          }
        }
      }

      public Dictionary<int, object> GetInputFields(DataRow row)
      {
        Dictionary<int, object> values = new Dictionary<int, object>();
        foreach (KeyValuePair<FieldInfo, ExpressionInfo> pair in _updateFields)
        {
          object value = pair.Value.Value(row);

          int idx = pair.Key.FieldIndex(row);
          if (idx < 0)
          {
            throw new InvalidOperationException(string.Format("Field {0} not in Table {1}",
             pair.Key.Name, row.Table.TableName));
          }
          values.Add(idx, value);
        }
        return values;
      }
    }

    protected class DeleteCommand : WhereCommand
    {
      public DeleteCommand(DbBaseCommand baseCommand)
        : base(baseCommand)
      {
        string text;
        int position;
        InitCommand("DELETE", out text, out position);

        string next;
        next = GetPart(text, ref position);

        string from = next;
        if (from.Equals("FROM", StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Expected FROM", text, position)); }

        BaseCmnd.InitTable(text, ref position);

        InitWhere(text, position, true);
      }
    }

    protected abstract class WhereCommand : BaseCommand
    {
      private ExpressionInfo _where;
      private bool? _isWhereOid;
      private int _oid;
      private DbParameter _whereParam;

      private DataView _validateView;
      private Dictionary<string, int> _validateColumns;

      public WhereCommand(DbBaseCommand baseCommand)
        : base(baseCommand)
      { }

      protected void InitWhere(string text, int position, bool checkKeyword)
      {
        if (position < text.Length)
        {
          if (checkKeyword)
          {
            string where = GetPart(text, ref position);
            if (where.Equals("WHERE", StringComparison.InvariantCultureIgnoreCase) == false)
            { throw new InvalidOperationException(ErrorText("Expected WHERE", text, position)); }
          }

          string next;
          StringBuilder whereBuilder = new StringBuilder();
          GetExpression(whereBuilder, text, ref position, out next, null);

          _where = new ExpressionInfo(whereBuilder.ToString(), this);
        }
      }

      private void InitValidateView(IDataRecord row)
      {
        DataTable validateSchema = Utils.GetTemplateTable(BaseCmnd.FullSchema);

        string upperWhere = _where.Expression.ToUpper();
        _validateColumns = new Dictionary<string, int>();
        foreach (DataColumn col in validateSchema.Columns)
        {
          if (!upperWhere.Contains(col.ColumnName.ToUpper()))
          { continue; }

          int idx = row.GetOrdinal(col.ColumnName);
          _validateColumns.Add(col.ColumnName, idx);
        }
        int iPar = 0;
        foreach (ParameterInfo par in _where.Parameters)
        {
          string name;
          if (string.IsNullOrEmpty(par.Name) == false)
          {
            name = ":" + par.Name;
          }
          else
          {
            name = ":" + iPar;
          }
          if (validateSchema.Columns.IndexOf(name) < 0 && par.DbParameter.Value != null)
          {
            validateSchema.Columns.Add(name, par.DbParameter.Value.GetType());
          }
        }

        StringBuilder validateWhere = new StringBuilder(_where.Expression);

        char iFunction = (char)0;
        foreach (Function func in _where.Functions)
        {
          if (func.IsQueryHandled)
          {
            func.ReplaceByTrue(validateWhere);
          }
          else if (func.IsImplemented)
          {
            int s = func.Start;
            validateWhere[s] = ':';
            validateWhere[s + 1] = 'f';
            validateWhere[s + 2] = (char)(iFunction + 'A');
            for (int i = s + 3; i <= func.End; i++)
            {
              validateWhere[i] = ' ';
            }
            validateSchema.Columns.Add(validateWhere.ToString(s, 3), func.ResultType);
          }
          iFunction++;
        }

        _validateView = new DataView(validateSchema);
        _validateView.RowFilter = validateWhere.ToString();
      }

      public bool Validate(IDataRecord row)
      {
        if (_where == null)
        { return true; }

        if (_validateView == null)
        { InitValidateView(row); }

        DataTable validateSchema = _validateView.Table;
        validateSchema.Clear();
        DataRow validateRow = validateSchema.NewRow();
        foreach (KeyValuePair<string, int> pair in _validateColumns)
        {
          validateRow[pair.Key] = row.GetValue(pair.Value);
        }
        foreach (ParameterInfo param in _where.Parameters)
        {
          if (param.DbParameter.Value == null)
          { continue; }
          validateRow[":" + param.Name] = param.DbParameter.Value;
        }
        int iFunction = 0;
        foreach (Function f in _where.Functions)
        {
          if (f.IsQueryHandled)
          { }
          else if (f.IsImplemented)
          {
            string fCol = string.Format(":f{0}", (char)(iFunction + 'A'));
            validateRow[fCol] = f.Execute(validateRow);
          }
          iFunction++;
        }
        validateSchema.Rows.Add(validateRow);
        bool valid = _validateView.Count == 1;
        validateSchema.Clear();

        return valid;
      }

      public ExpressionInfo WhereExp
      {
        get { return _where; }
      }

      public string Where
      {
        get
        {
          if (_where == null)
          { return null; }
          return _where.Expression;
        }
      }
      public bool TryGetWhereOid(out int oid)
      {
        oid = -1;
        if (_isWhereOid.HasValue == false)
        {
          _isWhereOid = false;

          int position = 0;
          string expression = _where.Expression;
          string field = GetPart(expression, ref position, '=');

          if (position >= expression.Length || expression[position] != '=')
          { return false; }

          IList<DataColumn> primKey = BaseCmnd.FullSchema.PrimaryKey;
          if (primKey.Count != 1 || primKey[0].ColumnName.Equals(field, StringComparison.InvariantCultureIgnoreCase) == false)
          { return false; }

          position++;

          string rest = expression.Substring(position).Trim();
          if (int.TryParse(rest, out oid))
          {
            _oid = oid;
            _isWhereOid = true;
            return true;
          }
          else if (rest == "?")
          {
            _whereParam = _where.Parameters[0].DbParameter;
            oid = (int)_whereParam.Value;
            _isWhereOid = true;
            return true;
          }
          else
          { return false; }
        }

        if (_isWhereOid.Value == false)
        { return false; }

        if (_whereParam != null)
        { oid = (int)_whereParam.Value; }
        else
        { oid = _oid; }

        return true;
      }

      protected override IEnumerable<ParameterInfo> ParameterInfos
      {
        get
        {
          foreach (ParameterInfo param in WhereParameterInfos)
          {
            yield return param;
          }
        }
      }

      protected IEnumerable<ParameterInfo> WhereParameterInfos
      {
        get
        {
          if (_where == null)
          { yield break; }

          foreach (ParameterInfo param in _where.Parameters)
          { yield return param; }
        }
      }
    }

    protected abstract class BaseCommand
    {
      protected class FieldInfo
      {
        public readonly string Name;
        private DataTable _lastTable;
        private int _lastIndex;

        public FieldInfo(string name)
        {
          Name = name;
        }

        public int FieldIndex(DataRow row)
        {
          DataTable tbl = row.Table;
          if (tbl != _lastTable)
          {
            _lastTable = tbl;
            _lastIndex = tbl.Columns.IndexOf(Name);
          }
          return _lastIndex;
        }
      }

      DbBaseCommand _baseCommand;

      public BaseCommand(DbBaseCommand baseCommand)
      {
        _baseCommand = baseCommand;
      }
      public DbBaseCommand BaseCmnd
      {
        get { return _baseCommand; }
      }

      protected abstract IEnumerable<ParameterInfo> ParameterInfos { get; }

      public void AssignDbParameters()
      {
        int iParam = 0;
        DbParameterCollection paramList = BaseCmnd.Parameters;
        int nParams = paramList.Count;

        foreach (ParameterInfo paramInfo in ParameterInfos)
        {
          string paramName = paramInfo.Name;
          if (string.IsNullOrEmpty(paramName))
          {
            if (iParam >= nParams)
            { throw new InvalidOperationException("Parameters do not match"); }
            paramInfo.Assign(paramList[iParam]);
            iParam++;
          }
          else
          {
            bool success = false;
            foreach (DbParameter dbParam in paramList)
            {
              if (dbParam.ParameterName == paramName)
              {
                paramInfo.Assign(dbParam);
                success = true;
                break;
              }
            }
            if (success == false)
            {
              throw new InvalidOperationException(
                string.Format("Parameter :{0} do not in Parameters", paramName));
            }

          }
        }
      }

      protected void InitCommand(string command, out  string text, out int position)
      {
        text = _baseCommand.CommandText.Trim();
        position = 0;

        string next = GetPart(text, ref position);
        if (next.Equals(command, StringComparison.InvariantCultureIgnoreCase) == false)
        { throw new InvalidOperationException(ErrorText("Invalid Keyword " + next, text, position)); }
      }
    }

    protected class Function
    {
      private delegate object FunctionHandler(IList<object> values);
      private static Dictionary<string, FunctionHandler> _functionHandlers;

      static Function()
      {
        _functionHandlers = new Dictionary<string, FunctionHandler>();
        _functionHandlers.Add("ST_Intersects", ST_Intersects);
      }

      private static object ST_Intersects(IList<object> values)
      {
        IGeometry geom0 = (IGeometry)values[0];
        IGeometry geom1 = (IGeometry)values[1];
        if (!geom0.Extent.Intersects(geom1.Extent))
        { return false; }
        if (geom1 is IBox)
        {
          geom0 = geom0.Project(new ToXY());
        }
        bool intersects = GeometryOperator.Intersects(geom0, geom1);
        return intersects;
      }

      private string _name;
      private string _paramString;
      private readonly int _start;
      private readonly int _end;
      private BaseCommand _baseCmd;
      private List<string> _parameters;
      private List<Function> _functions;
      private List<ParameterInfo> _params;
      public Function(string name, string paramList, int start, int end, BaseCommand baseCmd)
      {
        _name = name;
        _paramString = paramList;
        _start = start;
        _end = end;
        _baseCmd = baseCmd;
      }

      public string Name
      { get { return _name; } }

      private List<string> Parameters
      {
        get
        {
          if (_parameters == null)
          {
            List<string> parameters = new List<string>();
            string paramString = _paramString.Substring(1, _paramString.Length - 2);
            int n = paramString.Length;
            int pos1 = 0;
            while (pos1 < n)
            {
              int pos0 = pos1;
              _functions = new List<Function>();
              _params = new List<ParameterInfo>();
              pos1 = ExpressionInfo.Init(_params, _functions, paramString, pos1, new char[] { ',' }, n, _baseCmd);
              string parameter = paramString.Substring(pos0, pos1 - pos0);
              parameters.Add(parameter.Trim());
              pos1++;
            }
            _parameters = parameters;
          }
          return _parameters;
        }
      }

      public bool IsQueryHandled
      {
        get { return false; }
      }

      public int Start
      {
        get { return _start; }
      }

      public int End
      {
        get { return _end; }
      }

      public bool IsImplemented
      {
        get { return _functionHandlers.ContainsKey(_name); }
      }

      public Type ResultType
      {
        get { return typeof(bool); }
      }

      public object Execute(DataRow row)
      {
        List<object> values = new List<object>();
        foreach (string parameter in Parameters)
        {
          values.Add(row[parameter]);
        }
        object value = _functionHandlers[_name](values);
        return value;
      }

      public void ReplaceByTrue(StringBuilder where)
      {
        int s = Start;
        where[s] = '1';
        where[s + 1] = '=';
        where[s + 2] = '1';
        for (int i = s + 3; i <= End; i++)
        {
          where[i] = ' ';
        }
      }
    }
    protected class ParameterInfo
    {
      private int _position;
      private string _name;
      private DbParameter _dbParameter;
      private BaseCommand _assignedTo;

      public ParameterInfo(int position, BaseCommand assignedTo)
      {
        _position = position;
        _assignedTo = assignedTo;
      }
      public ParameterInfo(int position, string name, BaseCommand assignedTo)
      {
        _position = position;
        _name = name;
        _assignedTo = assignedTo;
      }

      public int Position
      { get { return _position; } }
      public string Name
      { get { return _name; } }

      public DbParameter DbParameter
      {
        get
        {
          if (_dbParameter == null)
          { _assignedTo.AssignDbParameters(); }
          return _dbParameter;
        }
      }

      public void Assign(DbParameter parameter)
      {
        _dbParameter = parameter;
      }

    }

    #endregion

    private string _text_;

    private string _tableName;

    private BaseCommand _baseCommand;
    private DbBaseParameterCollection _parameters;
    private DbBaseConnection _connection;
    private DbTransaction _transaction;

    private DataTable _fullSchema;


    public DbBaseCommand(DbBaseConnection connection)
    {
      _connection = connection;
    }

    public new DbBaseConnection Connection
    {
      get { return _connection; }
    }

    protected override DbConnection DbConnection
    {
      get { return _connection; }
      set
      {
        DbBaseConnection connection = (DbBaseConnection)value;
        _connection = connection;
      }
    }

    public override string CommandText
    {
      get { return _text_; }
      set
      {
        Clear();
        DbParameterCollection.Clear();
        _text_ = value;
      }
    }

    public string GetStandardSqlText()
    {
      WhereCommand cmd = BaseCmd as WhereCommand;

      if (cmd == null)
      { return CommandText; }

      if (cmd.WhereExp != null)
      {
        IList<ParameterInfo> prmList = cmd.WhereExp.Parameters;
        IList<Function> fctList = cmd.WhereExp.Functions;
        StringBuilder whereBuilder = new StringBuilder(cmd.Where);

        foreach (Function function in fctList)
        {
          if (function.Name == "ST_Intersects")
          {
            function.ReplaceByTrue(whereBuilder);
          }
        }
        int position = 0;
        while (!GetPart(CommandText, ref position).Equals("WHERE", StringComparison.InvariantCultureIgnoreCase))
        {
          continue;
        }
        string sql = string.Format("{0} {1}", CommandText.Substring(0, position), whereBuilder);
        return sql;
      }
      return CommandText;
    }

    public bool Validate(IDataRecord row)
    {
      WhereCommand cmd = BaseCmd as WhereCommand;
      if (cmd == null)
      { throw new InvalidOperationException("Unhandled command " + CommandText); }

      return cmd.Validate(row);
    }

    public override int CommandTimeout
    {
      get { throw new Exception("The method or operation is not implemented."); }
      set { throw new Exception("The method or operation is not implemented."); }
    }

    protected override DbTransaction DbTransaction
    {
      get { return _transaction; }
      set { _transaction = value; }
    }

    public override CommandType CommandType
    {
      get { throw new Exception("The method or operation is not implemented."); }
      set { throw new Exception("The method or operation is not implemented."); }
    }

    public new DbBaseParameter CreateParameter()
    { return new DbBaseParameter(); }
    protected override DbParameter CreateDbParameter()
    { return CreateParameter(); }

    public new DbBaseParameterCollection Parameters
    {
      get { return (DbBaseParameterCollection)DbParameterCollection; }
    }
    protected override DbParameterCollection DbParameterCollection
    {
      get
      {
        if (_parameters == null)
        {
          _parameters = new DbBaseParameterCollection();
          Prepare();
        }
        return _parameters;
      }
    }

    public override UpdateRowSource UpdatedRowSource
    {
      get { return UpdateRowSource.OutputParameters; }
      set { throw new Exception("The method or operation is not implemented."); }
    }

    public string TableName
    {
      get
      {
        if (_tableName == null)
        { Prepare(); }
        return _tableName;
      }
    }

    protected BaseCommand BaseCmd
    {
      get
      {
        if (_baseCommand == null)
        { Prepare(); }
        return _baseCommand;
      }
    }

    private DataTable FullSchema
    {
      get
      {
        if (_fullSchema == null)
        { _fullSchema = GetFullSchema(); }
        return _fullSchema;
      }
    }
    private DataTable GetFullSchema()
    {
      DbCommand schemaCommand = Connection.CreateCommand();
      schemaCommand.CommandText = "SELECT * FROM " + _tableName;
      DbDataReader reader = schemaCommand.ExecuteReader(CommandBehavior.SchemaOnly);
      DataTable fullSchema = DbBaseReader.GetSchema(reader);
      return fullSchema;
    }

    #region TableName
    public static string EncapsulateTableName(string tableName)
    {
      string s = string.Format("'{0}'", tableName);
      return s;
    }

    protected void InitTable(string text, ref int position)
    {
      string table = GetPart(text, ref position, '(');
      // Remark: Must correspond to EsriDbCommand.EncapsulateTableName
      table = table.Trim('\'');

      _tableName = table;
    }
    #endregion

    public override void Cancel()
    {
      Clear();
      throw new Exception("The method or operation is not implemented.");
    }

    protected abstract int ExecuteUpdate(UpdateCommand update);
    protected abstract int ExecuteInsert(InsertCommand insert);
    protected abstract int ExecuteDelete(DeleteCommand delete);

    public override int ExecuteNonQuery()
    {
      BaseCommand cmd = BaseCmd;

      if (cmd is UpdateCommand)
      {
        UpdateCommand update = (UpdateCommand)cmd;
        return ExecuteUpdate(update);
      }
      else if (cmd is InsertCommand)
      {
        InsertCommand insert = (InsertCommand)cmd;
        return ExecuteInsert(insert);
      }
      else if (cmd is DeleteCommand)
      {
        DeleteCommand delete = (DeleteCommand)cmd;
        return ExecuteDelete(delete);
      }
      else
      { throw new NotImplementedException("Unhandled CommandType " + cmd.GetType()); }
    }

    public override object ExecuteScalar()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    public override bool DesignTimeVisible
    {
      get { return false; }
      set { throw new Exception("The method or operation is not implemented."); }
    }

    public SchemaColumnsTable GetSchemaTable()
    {
      if (BaseCmd is SelectCommand)
      {
        SelectCommand selCmd = (SelectCommand)BaseCmd;

        SchemaColumnsTable table = Connection.GetTableSchema(TableName);
        SchemaColumnsTable schema = new SchemaColumnsTable();
        foreach (FieldExpression field in selCmd.Fields)
        {
          if (field.FieldName == "*")
          {
            return table;
          }
          SchemaColumnsTable.Row column = null;
          foreach (SchemaColumnsTable.Row c in table.Rows)
          {
            if (c.ColumnName.Equals(field.FieldName, StringComparison.InvariantCultureIgnoreCase))
            {
              column = c;
              break;
            }
          }
          if (column == null)
          {
            throw new InvalidOperationException("Cannot find field " + field.FieldName +
              " in table " + TableName);
          }
          schema.AddSchemaColumn(column.ColumnName, column.DataType);
        }
        return schema;
      }
      return null;
    }
    public sealed override void Prepare()
    {
      Clear();

      string text = CommandText;
      if (string.IsNullOrEmpty(text))
      { return; }

      text = text.Trim();
      int position = 0;
      string command = GetPart(text, ref position);
      BaseCommand baseCommand;

      if (command.Equals("SELECT", StringComparison.InvariantCultureIgnoreCase))
      {
        baseCommand = new SelectCommand(this);
      }
      else if (command.Equals("INSERT", StringComparison.InvariantCultureIgnoreCase))
      {
        baseCommand = new InsertCommand(this);
      }
      else if (command.Equals("UPDATE", StringComparison.InvariantCultureIgnoreCase))
      {
        baseCommand = new UpdateCommand(this);
      }
      else if (command.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase))
      {
        baseCommand = new DeleteCommand(this);
      }
      else
      {
        throw new NotImplementedException("Unhandled command " + command);
      }

      _baseCommand = baseCommand;
    }

    protected virtual void Clear()
    {
      _tableName = null;
      _baseCommand = null;

      _fullSchema = null;
    }

    private static string GetField(string text, ref int position, DataTable table, params char[] separators)
    {
      string field = GetPart(text, ref position, separators).Trim();
      int iField = table.Columns.IndexOf(field);

      if (iField < 0)
      { throw new InvalidOperationException(ErrorText("Field " + field + " not found ", text, position)); }

      return field;
    }

    protected static void GetExpression(StringBuilder expression, string text,
      ref int position, out string next,
      IList<char> separator, params string[] keywords)
    {
      if (expression == null)
      { throw new ArgumentNullException("expression"); }

      List<char> seps = new List<char>();
      if (separator != null)
      {
        foreach (char sep in separator)
        { seps.Add(sep); }
      }
      seps.Add('(');

      while (true)
      {
        string part = GetPart(text, ref position, seps);
        if (part == null)
        {
          next = null;
          return;
        }
        foreach (string keyword in keywords)
        {
          if (part.Equals(keyword, StringComparison.InvariantCultureIgnoreCase))
          {
            next = keyword;
            return;
          }
        }

        if (expression.Length > 0)
        { expression.Append(" "); }
        expression.Append(part);

        if (position >= text.Length)
        {
          next = null;
          return;
        }
        if (separator != null && separator.Contains(text[position]))
        {
          next = text[position].ToString();
          return;
        }

        if (position < text.Length && text[position] == '(')
        {
          string n;
          position++;
          expression.Append("(");
          GetExpression(expression, text, ref position, out n, new char[] { ')' });
          if (position >= text.Length || text[position] != ')')
          { throw new InvalidOperationException(ErrorText("Unbalanced ()", text, position)); }
          expression.Append(")");
          position++;
        }
      }
    }

    protected static string GetPart(string text, ref int position, params char[] separators)
    {
      List<char> c = new List<char>(separators);
      string part = GetPart(text, ref position, c);
      return part;
    }

    protected static string GetPart(string text, ref int position, IList<char> separators)
    {
      int start = position;
      int iPos = position;
      IList<char> seps = separators;
      int n = text.Length;

      while (iPos < n && char.IsWhiteSpace(text[iPos]))
      { iPos++; }
      if (iPos >= n)
      { return null; }

      string part;
      if (iPos < n && text[iPos] == '\'')
      {
        iPos++;
        while (iPos < n && text[iPos] != '\'')
        { iPos++; }
        if (iPos >= n)
        { throw new InvalidOperationException(ErrorText("missing end string separator", text, start)); }
        part = text.Substring(start, iPos - start + 1);

        iPos++;
      }
      else
      {
        while (iPos < n && char.IsWhiteSpace(text[iPos]) == false &&
          (seps == null || seps.Contains(text[iPos]) == false))
        { iPos++; }
        part = text.Substring(start, iPos - start);
      }

      while (iPos < n && char.IsWhiteSpace(text[iPos]))
      {
        iPos++;
      }

      position = iPos;

      part = part.Trim();
      return part;
    }

    protected static string ErrorText(string message, string command, int position)
    {
      string error = string.Format("{0} in" + Environment.NewLine + "{1}" +
        Environment.NewLine + "near" + Environment.NewLine + "{2}",
        message, command, command.Substring(0, position));
      return error;
    }
  }
}
