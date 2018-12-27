using Basics.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TMapWin
{
  public partial class WdgCustom : Form
  {
    private Attribute _attribute;

    public WdgCustom()
    {
      InitializeComponent();
      //_settings = new TMapWin.Properties.Settings();
      //_attribute = _settings.Plugins;
      //if (_attribute == null)
      //{
      //  _attribute = new Attribute();
      //  _settings.Plugins = _attribute;
      //}
      _attribute = new Attribute();
      DataGridViewColumn col = new DataGridViewTextBoxColumn();
      col.HeaderText = "Type";
      col.DataPropertyName = _attribute.TblICustom.NameColumn.ColumnName;
      col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
      dgCustom.Columns.Add(col);
      dgCustom.AutoGenerateColumns = false;

      dgCustom.DataSource = _attribute.TblICustom;
    }

    public IEnumerable<Attribute.TblICustomRow> AddPlugins()
    {
      if (chkAdd.Enabled && chkAdd.Checked)
      {

        foreach (var row in dgCustom.SelectedRows.Enum())
        {
          DataRowView vrow = (DataRowView)row.DataBoundItem;
          yield return (Attribute.TblICustomRow)vrow.Row;
        }
      }
    }

    public Attribute.TblICustomRow RunPlugin()
    {
      if (chkRun.Enabled == false || chkRun.Checked == false)
      { return null; }

      DataRowView rv = (DataRowView)dgCustom.SelectedRows[0].DataBoundItem;
      Attribute.TblICustomRow row = (Attribute.TblICustomRow)rv.Row;

      return row;
    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
      dlgOpenAssembly.Filter = "Assembly (*.dll,*.exe) | *.dll;*.exe";
      if (dlgOpenAssembly.ShowDialog() == DialogResult.OK)
      {
        _attribute.Clear();

        string assemblyName = dlgOpenAssembly.FileName;
        txtAssembly.Text = assemblyName;
        txtAssembly.ReadOnly = true;
        System.Reflection.Assembly assembly =
          System.Reflection.Assembly.LoadFile(assemblyName);
        foreach (var type in assembly.GetExportedTypes())
        {
          if (typeof(TMap.ICommand).IsAssignableFrom(type))
          {
            _attribute.TblICustom.AddTblICustomRow(assemblyName, type.FullName, type.Name);
          }
        }
        _attribute.AcceptChanges();
      }
    }

    private Rectangle _dragBoxFromMouseDown;
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
        _dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                       e.Y - (dragSize.Height / 2)), dragSize);
      }
      else
      {
        // Reset the rectangle if the mouse is not over an item in the ListBox.
        _dragBoxFromMouseDown = Rectangle.Empty;
      }
    }

    private void DgCustom_MouseMove(object sender, MouseEventArgs e)
    {
      if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
      { return; }

      // If the mouse moves outside the rectangle, start the drag.
      if (_dragBoxFromMouseDown == Rectangle.Empty ||
            _dragBoxFromMouseDown.Contains(e.X, e.Y))
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
      _dragBoxFromMouseDown = Rectangle.Empty;
    }

    private void DgCustom_SelectionChanged(object sender, EventArgs e)
    {
      if (dgCustom.SelectedRows == null)
      {
        chkRun.Enabled = false;
        chkAdd.Enabled = false;
        btnOk.Enabled = false;

        return;
      }
      chkRun.Enabled = (dgCustom.SelectedRows.Count == 1);
      chkAdd.Enabled = (dgCustom.SelectedRows.Count > 0);
      btnOk.Enabled = true;
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
  }
}