using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TMapWin
{
  public partial class WdgAuswahl : Form
  {
    public WdgAuswahl()
    {
      InitializeComponent();
    }

    public void SetData(string title, string desc, DataView data, string display)
    {
      Text = title;
      lblAuswahl.Text = desc;
      lstAuswahl.DataSource = data;
      lstAuswahl.DisplayMember = display;
    }

    public DataRow SelectedRow
    {
      get
      {
        DataRowView vRow = (DataRowView)lstAuswahl.SelectedItem;
        if (vRow == null)
        { return null; }
        return vRow.Row;
      }
    }
    private void btnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}