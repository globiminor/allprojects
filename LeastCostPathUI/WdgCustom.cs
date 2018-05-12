using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Grid;
using Microsoft.CSharp;
using Grid.Lcp;

namespace LeastCostPathUI
{
  public partial class WdgCustom : Form
  {
    private class TypeHelper
    {
      private readonly Type _type;

      public TypeHelper(Type type)
      {
        _type = type;
      }

      public Type Type => _type;
      public string Name => _type.Name;
    }

    public WdgCustom()
    {
      InitializeComponent();

      DataGridViewColumn col = new DataGridViewTextBoxColumn
      {
        HeaderText = "Member",
        DataPropertyName = "Name",
        AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
      };
      dgCustom.Columns.Add(col);
      dgCustom.AutoGenerateColumns = false;

      Type t = typeof(ITvmCalc);
      MethodInfo miBase = t.GetMethods()[0];

      StringBuilder sbLbl = new StringBuilder();
      StringBuilder sbTtp = new StringBuilder();
      sbTtp.Append($"public {miBase.ReturnType.Name} {nameof(ITvmCalc.Calc)}(");
      sbLbl.Append($"public {miBase.ReturnType.Name} {nameof(ITvmCalc.Calc)}(");
      bool first = true;
      foreach (ParameterInfo paramInfo in miBase.GetParameters())
      {
        if (!first)
        {
          sbLbl.Append(", ");
          sbTtp.AppendLine(", ");
          sbTtp.Append("  ");
        }
        sbLbl.AppendFormat("{0}", paramInfo.Name);
        sbTtp.AppendFormat("{0} {1}", paramInfo.ParameterType.Name, paramInfo.Name);
        first = false;
      }
      sbLbl.Append(") {");
      sbTtp.Append(")");
      lblDeclare.Text = sbLbl.ToString();
      ttp.SetToolTip(lblDeclare, sbTtp.ToString());
      SetLayout();
    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
      dlgOpenAssembly.Filter = "Assembly (*.dll,*.exe) | *.dll;*.exe";
      if (string.IsNullOrEmpty(dlgOpenAssembly.InitialDirectory))
      {
        dlgOpenAssembly.InitialDirectory =
          System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      }
      if (dlgOpenAssembly.ShowDialog() == DialogResult.OK)
      {
        string assemblyName = dlgOpenAssembly.FileName;
        txtAssembly.Text = assemblyName;
        txtAssembly.ReadOnly = true;

        Assembly assembly = Assembly.LoadFile(assemblyName);
        List<Type> types = GetCostProviders<ICostProvider>(assembly);
        List<TypeHelper> m = new List<TypeHelper>(types.Count);
        foreach (Type type in types)
        { m.Add(new TypeHelper(type)); }

        dgCustom.DataSource = m;
      }
    }

    private List<Type> GetCostProviders<T>(Assembly assembly)
    {
      List<Type> types = new List<Type>();
      Type baseType = typeof(T);
      foreach (Type type in assembly.GetTypes())
      {
        if (baseType.IsAssignableFrom(type) &&
          type.IsAbstract == false && type.GetConstructor(Type.EmptyTypes) != null)
        {
          types.Add(type);
        }
      }
      return types;
    }

    private void SetLayout()
    {
      grpCustom.Enabled = optCustom.Checked;
      grpAssembly.Enabled = optExisting.Checked;
      if (optExisting.Checked)
      {
        btnOk.Enabled = (dgCustom.SelectedRows != null && dgCustom.SelectedRows.Count == 1);
      }
      else
      {
        btnOk.Enabled = true;
      }
    }

    private Rectangle dragBoxFromMouseDown;

    public ITvmCalc TvmCalc { get; set; }

    private void DgCustom_MouseDown(object sender, MouseEventArgs e)
    {
      // Get the index of the item the mouse is below.
      int iDown = dgCustom.HitTest(e.X, e.Y).RowIndex;

      if (iDown >= 0 && iDown < dgCustom.RowCount)
      {

        // Remember the point where the mouse down occurred. The DragSize indicates
        // the size that the mouse can move before a drag event should be started.                
        Size dragSize = SystemInformation.DragSize;

        // Create a rectangle using the DragSize, with the mouse position being
        // at the center of the rectangle.
        dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                       e.Y - (dragSize.Height / 2)), dragSize);
      }
      else
      {
        // Reset the rectangle if the mouse is not over an item in the ListBox.
        dragBoxFromMouseDown = Rectangle.Empty;
      }
    }

    private void DgCustom_MouseMove(object sender, MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
      { return; }

      // If the mouse moves outside the rectangle, start the drag.
      if (dragBoxFromMouseDown == Rectangle.Empty ||
            dragBoxFromMouseDown.Contains(e.X, e.Y))
      { return; }

      // Proceed with the drag-and-drop, passing in the list item.                    
      DragDropEffects dropEffect = dgCustom.DoDragDrop(
        dgCustom.CurrentRow, DragDropEffects.Copy);

      // If the drag operation was a move then remove the item.
      if (dropEffect == DragDropEffects.Move)
      {
      }
    }

    private void DgCustom_MouseUp(object sender, MouseEventArgs e)
    {
      dragBoxFromMouseDown = Rectangle.Empty;
    }

    private void DgCustom_SelectionChanged(object sender, EventArgs e)
    {
      SetLayout();
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
      if (optExisting.Checked)
      {
        TypeHelper meth = dgCustom.SelectedRows[0].DataBoundItem as TypeHelper;
        if (meth == null)
        { return; }

        TvmCalc = (ITvmCalc)Activator.CreateInstance(meth.Type, nonPublic: true);
      }
      else if (optCustom.Checked)
      {
        TvmCalc = GetCustomCostCalculator();
        if (TvmCalc == null)
        { return; }
      }

      DialogResult = DialogResult.OK;
      Close();
    }

    private ITvmCalc GetCustomCostCalculator()
    {

      Type scc = typeof(ITvmCalc);
      StringBuilder meth = new StringBuilder();
      meth.AppendLine("using System;");
      meth.AppendLine($"using {scc.Namespace};");
      meth.AppendLine($"public class Custom : {nameof(ITvmCalc)}");
      meth.AppendLine("{");
      meth.AppendLine($"{ttp.GetToolTip(lblDeclare)}");
      meth.AppendLine("{");
      meth.AppendLine(txtCode.Text);
      meth.AppendLine("}");
      string propType = scc.GetProperty(nameof(ITvmCalc.MinUnitCost)).PropertyType.Name;
      meth.AppendLine($"public {propType} {nameof(ITvmCalc.MinUnitCost)}");
      meth.AppendLine("{ get { return 0.0; } }");
      meth.AppendLine("}");


      Assembly assembly = CreateAssembly(meth.ToString());
      if (assembly == null)
      { return null; }

      IList<Type> types = GetCostProviders<ITvmCalc>(assembly);
      return (ITvmCalc)Activator.CreateInstance(types[0], nonPublic: true);
    }

    private static Assembly CreateAssembly(string code)
    {
      CompilerResults results;
      CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
      //string Output = "Out.exe";

      CompilerParameters parameters = new CompilerParameters();
      // TODO:
      parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
      parameters.ReferencedAssemblies.Add(typeof(ITvmCalc).Assembly.Location);
      parameters.GenerateInMemory = true;
      //Make sure we generate an EXE, not a DLL
      //parameters.GenerateExecutable = true;
      //parameters.OutputAssembly = Output;
      results = codeProvider.CompileAssemblyFromSource(parameters, code);

      if (results.Errors.Count > 0)
      {
        StringBuilder error = new StringBuilder();
        foreach (CompilerError CompErr in results.Errors)
        {
          error.AppendFormat("Line number {0}: {1};",
            CompErr.Line, CompErr.ErrorText);
          error.AppendLine();
          error.AppendLine();
        }
        MessageBox.Show(error.ToString());
        return null;
      }
      return results.CompiledAssembly;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void Opt_CheckedChanged(object sender, EventArgs e)
    {
      SetLayout();
    }

    private void DgCustom_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
      if (e.RowIndex < 0 || e.RowIndex >= dgCustom.Rows.Count)
      { return; }
      if (dgCustom.SelectedRows.Count != 1)
      { return; }

      TypeHelper meth = dgCustom.SelectedRows[0].DataBoundItem as TypeHelper;
      if (meth == null)
      { return; }

      TvmCalc = (ITvmCalc)Activator.CreateInstance(meth.Type, nonPublic: true);

      DialogResult = DialogResult.OK;
      Close();
    }
  }
}