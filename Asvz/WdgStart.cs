using System;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Basics.Geom;
using Ocad;
using Asvz.Sola;
using System.Collections.Generic;
using System.Security.AccessControl;
using Ocad.StringParams;
using System.Drawing;

namespace Asvz
{
  /// <summary>
  /// Summary description for Form1.
  /// </summary>
  public class WdgStart : Form
  {
    private Button _btnGetClipboard;
    private Label _lblFormat;
    private Button _toSymbol;
    private Button _btnClip;
    private Button _btnCreate;
    private TextBox _txtCreate;
    private OpenFileDialog _dlgOpen;
    private Button _btnOpen;
    private Button _btnTest;
    private Button _btnUebergabe;
    private TextBox _txtStrecke;
    private Label _label1;
    private Button _btnDetail;
    private TextBox _txtUebergabe;
    private TextBox _txtDetail;
    private TextBox _txtDetNr;
    private TextBox _txtBis;
    private CheckBox _chkDistance;
    private Button _btnFull;
    private Button _btnStrecke;
    private Button _btnStreckeExp;
    private TextBox _txtExpStrecke;
    private CheckBox _chkDamen;
    private TextBox _txtTransport;
    private Button _btnTransport;
    private Button _btnEinsatz;
    private TextBox _txtBewilligung;
    private DateTimePicker _dtLaufdatum;
    private Button _btnKml;
    private GroupBox _grpUebergabe;
    private GroupBox _grpStrecke;
    private GroupBox _grpGesamt;
    private Button _btnTransportGesamt;
    private Button _btnTestForch;
    private Button _btnTestDuo;
    private Button _btnBegleit;
    private TextBox _txtBegleit;
    private Button _btnGpx;
    private ToolTip _ttp;
    private CheckBox _chkStartZiel;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container _components = null;

    public WdgStart()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      _txtCreate.Text = Ddx.WorkDir + _txtCreate.Text;
      _txtUebergabe.Text = Ddx.WorkDir + _txtUebergabe.Text;
      _txtDetail.Text = Ddx.WorkDir + _txtDetail.Text;
      _txtTransport.Text = Ddx.WorkDir + _txtTransport.Text;
      _txtExpStrecke.Text = Ddx.WorkDir + _txtExpStrecke.Text;
      _txtBegleit.Text = Ddx.WorkDir + _txtBegleit.Text;
      //
      // TODO: Add any constructor code after InitializeComponent call
      //
      _txtBewilligung.Text = Ddx.Bewilligung;
      _dtLaufdatum.Value = Ddx.Laufdatum;
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_components != null)
        {
          _components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this._components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WdgStart));
      this._btnGetClipboard = new System.Windows.Forms.Button();
      this._lblFormat = new System.Windows.Forms.Label();
      this._toSymbol = new System.Windows.Forms.Button();
      this._btnClip = new System.Windows.Forms.Button();
      this._btnCreate = new System.Windows.Forms.Button();
      this._txtCreate = new System.Windows.Forms.TextBox();
      this._dlgOpen = new System.Windows.Forms.OpenFileDialog();
      this._btnOpen = new System.Windows.Forms.Button();
      this._btnTest = new System.Windows.Forms.Button();
      this._btnUebergabe = new System.Windows.Forms.Button();
      this._txtStrecke = new System.Windows.Forms.TextBox();
      this._label1 = new System.Windows.Forms.Label();
      this._btnDetail = new System.Windows.Forms.Button();
      this._txtUebergabe = new System.Windows.Forms.TextBox();
      this._txtDetail = new System.Windows.Forms.TextBox();
      this._txtDetNr = new System.Windows.Forms.TextBox();
      this._txtBis = new System.Windows.Forms.TextBox();
      this._chkDistance = new System.Windows.Forms.CheckBox();
      this._btnFull = new System.Windows.Forms.Button();
      this._btnStrecke = new System.Windows.Forms.Button();
      this._btnStreckeExp = new System.Windows.Forms.Button();
      this._txtExpStrecke = new System.Windows.Forms.TextBox();
      this._chkDamen = new System.Windows.Forms.CheckBox();
      this._txtTransport = new System.Windows.Forms.TextBox();
      this._btnTransport = new System.Windows.Forms.Button();
      this._btnEinsatz = new System.Windows.Forms.Button();
      this._txtBewilligung = new System.Windows.Forms.TextBox();
      this._dtLaufdatum = new System.Windows.Forms.DateTimePicker();
      this._btnKml = new System.Windows.Forms.Button();
      this._grpUebergabe = new System.Windows.Forms.GroupBox();
      this._grpStrecke = new System.Windows.Forms.GroupBox();
      this._chkStartZiel = new System.Windows.Forms.CheckBox();
      this._btnBegleit = new System.Windows.Forms.Button();
      this._txtBegleit = new System.Windows.Forms.TextBox();
      this._grpGesamt = new System.Windows.Forms.GroupBox();
      this._btnTransportGesamt = new System.Windows.Forms.Button();
      this._btnTestForch = new System.Windows.Forms.Button();
      this._btnTestDuo = new System.Windows.Forms.Button();
      this._btnGpx = new System.Windows.Forms.Button();
      this._ttp = new System.Windows.Forms.ToolTip(this._components);
      this._grpUebergabe.SuspendLayout();
      this._grpStrecke.SuspendLayout();
      this._grpGesamt.SuspendLayout();
      this.SuspendLayout();
      // 
      // _btnGetClipboard
      // 
      this._btnGetClipboard.Location = new System.Drawing.Point(16, 48);
      this._btnGetClipboard.Name = "_btnGetClipboard";
      this._btnGetClipboard.Size = new System.Drawing.Size(75, 24);
      this._btnGetClipboard.TabIndex = 0;
      this._btnGetClipboard.Text = "Make hole";
      this._btnGetClipboard.Click += new System.EventHandler(this.BtnGetClipboard_Click);
      // 
      // _lblFormat
      // 
      this._lblFormat.Location = new System.Drawing.Point(8, 397);
      this._lblFormat.Name = "_lblFormat";
      this._lblFormat.Size = new System.Drawing.Size(83, 95);
      this._lblFormat.TabIndex = 1;
      this._lblFormat.Text = "Format";
      // 
      // _toSymbol
      // 
      this._toSymbol.Location = new System.Drawing.Point(16, 80);
      this._toSymbol.Name = "_toSymbol";
      this._toSymbol.Size = new System.Drawing.Size(75, 40);
      this._toSymbol.TabIndex = 2;
      this._toSymbol.Text = "Object To Symbol";
      this._toSymbol.Click += new System.EventHandler(this.ToSymbol_Click);
      // 
      // _btnClip
      // 
      this._btnClip.Location = new System.Drawing.Point(16, 16);
      this._btnClip.Name = "_btnClip";
      this._btnClip.Size = new System.Drawing.Size(75, 24);
      this._btnClip.TabIndex = 3;
      this._btnClip.Text = "Clip";
      this._btnClip.Click += new System.EventHandler(this.BtnClip_Click);
      // 
      // _btnCreate
      // 
      this._btnCreate.Location = new System.Drawing.Point(9, 97);
      this._btnCreate.Name = "_btnCreate";
      this._btnCreate.Size = new System.Drawing.Size(72, 23);
      this._btnCreate.TabIndex = 4;
      this._btnCreate.Text = "Profil";
      this._btnCreate.Click += new System.EventHandler(this.BtnCreate_Click);
      // 
      // _txtCreate
      // 
      this._txtCreate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtCreate.Location = new System.Drawing.Point(112, 16);
      this._txtCreate.Name = "_txtCreate";
      this._txtCreate.Size = new System.Drawing.Size(327, 20);
      this._txtCreate.TabIndex = 5;
      this._txtCreate.Text = "OCAD Vorlagen\\sola10k.ocd";
      // 
      // _dlgOpen
      // 
      this._dlgOpen.Filter = "OCAD files|*.ocd";
      // 
      // _btnOpen
      // 
      this._btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this._btnOpen.Location = new System.Drawing.Point(445, 16);
      this._btnOpen.Name = "_btnOpen";
      this._btnOpen.Size = new System.Drawing.Size(24, 24);
      this._btnOpen.TabIndex = 6;
      this._btnOpen.Text = "...";
      this._btnOpen.Click += new System.EventHandler(this.BtnOpen_Click);
      // 
      // _btnTest
      // 
      this._btnTest.Location = new System.Drawing.Point(16, 126);
      this._btnTest.Name = "_btnTest";
      this._btnTest.Size = new System.Drawing.Size(75, 24);
      this._btnTest.TabIndex = 7;
      this._btnTest.Text = "Test";
      this._btnTest.Click += new System.EventHandler(this.BtnTest_Click);
      // 
      // _btnUebergabe
      // 
      this._btnUebergabe.Location = new System.Drawing.Point(9, 19);
      this._btnUebergabe.Name = "_btnUebergabe";
      this._btnUebergabe.Size = new System.Drawing.Size(72, 24);
      this._btnUebergabe.TabIndex = 8;
      this._btnUebergabe.Text = "Übergabe";
      this._btnUebergabe.Click += new System.EventHandler(this.BtnUebergabe_Click);
      // 
      // _txtStrecke
      // 
      this._txtStrecke.Location = new System.Drawing.Point(192, 48);
      this._txtStrecke.Name = "_txtStrecke";
      this._txtStrecke.Size = new System.Drawing.Size(24, 20);
      this._txtStrecke.TabIndex = 9;
      this._txtStrecke.Text = "1";
      this._txtStrecke.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this._txtStrecke.TextChanged += new System.EventHandler(this.TxtStrecke_TextChanged);
      // 
      // _label1
      // 
      this._label1.Location = new System.Drawing.Point(109, 52);
      this._label1.Name = "_label1";
      this._label1.Size = new System.Drawing.Size(48, 16);
      this._label1.TabIndex = 10;
      this._label1.Text = "Strecke";
      // 
      // _btnDetail
      // 
      this._btnDetail.Location = new System.Drawing.Point(9, 49);
      this._btnDetail.Name = "_btnDetail";
      this._btnDetail.Size = new System.Drawing.Size(72, 24);
      this._btnDetail.TabIndex = 11;
      this._btnDetail.Text = "Detail";
      this._btnDetail.Click += new System.EventHandler(this.BtnDetail_Click);
      // 
      // _txtUebergabe
      // 
      this._txtUebergabe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtUebergabe.Location = new System.Drawing.Point(87, 20);
      this._txtUebergabe.Name = "_txtUebergabe";
      this._txtUebergabe.Size = new System.Drawing.Size(270, 20);
      this._txtUebergabe.TabIndex = 12;
      this._txtUebergabe.Text = "Exp_Uebergabe";
      // 
      // _txtDetail
      // 
      this._txtDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtDetail.Location = new System.Drawing.Point(87, 52);
      this._txtDetail.Name = "_txtDetail";
      this._txtDetail.Size = new System.Drawing.Size(270, 20);
      this._txtDetail.TabIndex = 13;
      this._txtDetail.Text = "Exp_Detail";
      // 
      // _txtDetNr
      // 
      this._txtDetNr.Location = new System.Drawing.Point(87, 78);
      this._txtDetNr.Name = "_txtDetNr";
      this._txtDetNr.Size = new System.Drawing.Size(32, 20);
      this._txtDetNr.TabIndex = 14;
      // 
      // _txtBis
      // 
      this._txtBis.Location = new System.Drawing.Point(256, 48);
      this._txtBis.Name = "_txtBis";
      this._txtBis.Size = new System.Drawing.Size(24, 20);
      this._txtBis.TabIndex = 15;
      this._txtBis.Text = "1";
      this._txtBis.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      // 
      // _chkDistance
      // 
      this._chkDistance.Checked = true;
      this._chkDistance.CheckState = System.Windows.Forms.CheckState.Checked;
      this._chkDistance.Location = new System.Drawing.Point(87, 97);
      this._chkDistance.Name = "_chkDistance";
      this._chkDistance.Size = new System.Drawing.Size(88, 24);
      this._chkDistance.TabIndex = 16;
      this._chkDistance.Text = "Totaldistanz";
      // 
      // _btnFull
      // 
      this._btnFull.Location = new System.Drawing.Point(9, 19);
      this._btnFull.Name = "_btnFull";
      this._btnFull.Size = new System.Drawing.Size(72, 23);
      this._btnFull.TabIndex = 17;
      this._btnFull.Text = "Gesamtplan";
      this._btnFull.Click += new System.EventHandler(this.BtnFull_Click);
      // 
      // _btnStrecke
      // 
      this._btnStrecke.Location = new System.Drawing.Point(9, 13);
      this._btnStrecke.Name = "_btnStrecke";
      this._btnStrecke.Size = new System.Drawing.Size(115, 26);
      this._btnStrecke.TabIndex = 18;
      this._btnStrecke.Text = "Strecke aufdatieren";
      this._ttp.SetToolTip(this._btnStrecke, "Strecke aus Gesamtplan übernehmen");
      this._btnStrecke.Click += new System.EventHandler(this.BtnStrecke_Click);
      // 
      // _btnStreckeExp
      // 
      this._btnStreckeExp.Location = new System.Drawing.Point(9, 45);
      this._btnStreckeExp.Name = "_btnStreckeExp";
      this._btnStreckeExp.Size = new System.Drawing.Size(115, 24);
      this._btnStreckeExp.TabIndex = 19;
      this._btnStreckeExp.Text = "Strecke exportieren";
      this._btnStreckeExp.Click += new System.EventHandler(this.BtnStreckeExp_Click);
      // 
      // _txtExpStrecke
      // 
      this._txtExpStrecke.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtExpStrecke.Location = new System.Drawing.Point(130, 48);
      this._txtExpStrecke.Name = "_txtExpStrecke";
      this._txtExpStrecke.Size = new System.Drawing.Size(227, 20);
      this._txtExpStrecke.TabIndex = 20;
      this._txtExpStrecke.Text = "Exp_Strecke";
      // 
      // _chkDamen
      // 
      this._chkDamen.AutoSize = true;
      this._chkDamen.Location = new System.Drawing.Point(130, 19);
      this._chkDamen.Name = "_chkDamen";
      this._chkDamen.Size = new System.Drawing.Size(60, 17);
      this._chkDamen.TabIndex = 21;
      this._chkDamen.Text = "Damen";
      // 
      // _txtTransport
      // 
      this._txtTransport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtTransport.Location = new System.Drawing.Point(87, 104);
      this._txtTransport.Name = "_txtTransport";
      this._txtTransport.Size = new System.Drawing.Size(270, 20);
      this._txtTransport.TabIndex = 23;
      this._txtTransport.Text = "Exp_Transport";
      // 
      // _btnTransport
      // 
      this._btnTransport.Location = new System.Drawing.Point(9, 101);
      this._btnTransport.Name = "_btnTransport";
      this._btnTransport.Size = new System.Drawing.Size(72, 24);
      this._btnTransport.TabIndex = 22;
      this._btnTransport.Text = "Transport";
      this._btnTransport.Click += new System.EventHandler(this.BtnTransport_Click);
      // 
      // _btnEinsatz
      // 
      this._btnEinsatz.Location = new System.Drawing.Point(9, 48);
      this._btnEinsatz.Name = "_btnEinsatz";
      this._btnEinsatz.Size = new System.Drawing.Size(72, 23);
      this._btnEinsatz.TabIndex = 24;
      this._btnEinsatz.Text = "Einsatzplan";
      this._btnEinsatz.Click += new System.EventHandler(this.BtnEinsatz_Click);
      // 
      // _txtBewilligung
      // 
      this._txtBewilligung.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtBewilligung.Location = new System.Drawing.Point(87, 50);
      this._txtBewilligung.Name = "_txtBewilligung";
      this._txtBewilligung.Size = new System.Drawing.Size(270, 20);
      this._txtBewilligung.TabIndex = 25;
      // 
      // _dtLaufdatum
      // 
      this._dtLaufdatum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._dtLaufdatum.Location = new System.Drawing.Point(87, 76);
      this._dtLaufdatum.Name = "_dtLaufdatum";
      this._dtLaufdatum.Size = new System.Drawing.Size(270, 20);
      this._dtLaufdatum.TabIndex = 26;
      // 
      // _btnKml
      // 
      this._btnKml.Location = new System.Drawing.Point(112, 494);
      this._btnKml.Name = "_btnKml";
      this._btnKml.Size = new System.Drawing.Size(81, 23);
      this._btnKml.TabIndex = 27;
      this._btnKml.Text = "Kml";
      this._btnKml.Click += new System.EventHandler(this.BtnKml_Click);
      // 
      // _grpUebergabe
      // 
      this._grpUebergabe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._grpUebergabe.Controls.Add(this._btnUebergabe);
      this._grpUebergabe.Controls.Add(this._btnDetail);
      this._grpUebergabe.Controls.Add(this._txtUebergabe);
      this._grpUebergabe.Controls.Add(this._txtDetail);
      this._grpUebergabe.Controls.Add(this._txtDetNr);
      this._grpUebergabe.Controls.Add(this._txtTransport);
      this._grpUebergabe.Controls.Add(this._btnTransport);
      this._grpUebergabe.Location = new System.Drawing.Point(112, 71);
      this._grpUebergabe.Name = "_grpUebergabe";
      this._grpUebergabe.Size = new System.Drawing.Size(363, 129);
      this._grpUebergabe.TabIndex = 28;
      this._grpUebergabe.TabStop = false;
      this._grpUebergabe.Text = "Übergabestellen";
      // 
      // _grpStrecke
      // 
      this._grpStrecke.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._grpStrecke.Controls.Add(this._chkStartZiel);
      this._grpStrecke.Controls.Add(this._btnBegleit);
      this._grpStrecke.Controls.Add(this._txtBegleit);
      this._grpStrecke.Controls.Add(this._btnCreate);
      this._grpStrecke.Controls.Add(this._chkDistance);
      this._grpStrecke.Controls.Add(this._btnStreckeExp);
      this._grpStrecke.Controls.Add(this._btnStrecke);
      this._grpStrecke.Controls.Add(this._chkDamen);
      this._grpStrecke.Controls.Add(this._txtExpStrecke);
      this._grpStrecke.Location = new System.Drawing.Point(112, 206);
      this._grpStrecke.Name = "_grpStrecke";
      this._grpStrecke.Size = new System.Drawing.Size(363, 125);
      this._grpStrecke.TabIndex = 29;
      this._grpStrecke.TabStop = false;
      this._grpStrecke.Text = "Strecke";
      // 
      // chkStartZiel
      // 
      this._chkStartZiel.AutoSize = true;
      this._chkStartZiel.Location = new System.Drawing.Point(206, 19);
      this._chkStartZiel.Name = "chkStartZiel";
      this._chkStartZiel.Size = new System.Drawing.Size(70, 17);
      this._chkStartZiel.TabIndex = 24;
      this._chkStartZiel.Text = "Start/Ziel";
      this._ttp.SetToolTip(this._chkStartZiel, "Streckenstart/-ziel aus Gesamtplan übernehmen");
      // 
      // _btnBegleit
      // 
      this._btnBegleit.Location = new System.Drawing.Point(9, 71);
      this._btnBegleit.Name = "_btnBegleit";
      this._btnBegleit.Size = new System.Drawing.Size(115, 24);
      this._btnBegleit.TabIndex = 22;
      this._btnBegleit.Text = "Begleit exportieren";
      this._btnBegleit.Click += new System.EventHandler(this.BtnBegleit_Click);
      // 
      // _txtBegleit
      // 
      this._txtBegleit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._txtBegleit.Location = new System.Drawing.Point(130, 74);
      this._txtBegleit.Name = "_txtBegleit";
      this._txtBegleit.Size = new System.Drawing.Size(227, 20);
      this._txtBegleit.TabIndex = 23;
      this._txtBegleit.Text = "Exp_Begleit";
      // 
      // _grpGesamt
      // 
      this._grpGesamt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this._grpGesamt.Controls.Add(this._btnTransportGesamt);
      this._grpGesamt.Controls.Add(this._btnFull);
      this._grpGesamt.Controls.Add(this._btnEinsatz);
      this._grpGesamt.Controls.Add(this._txtBewilligung);
      this._grpGesamt.Controls.Add(this._dtLaufdatum);
      this._grpGesamt.Location = new System.Drawing.Point(112, 337);
      this._grpGesamt.Name = "_grpGesamt";
      this._grpGesamt.Size = new System.Drawing.Size(363, 151);
      this._grpGesamt.TabIndex = 30;
      this._grpGesamt.TabStop = false;
      this._grpGesamt.Text = "Gesamt";
      // 
      // _btnTransportGesamt
      // 
      this._btnTransportGesamt.Location = new System.Drawing.Point(9, 110);
      this._btnTransportGesamt.Name = "_btnTransportGesamt";
      this._btnTransportGesamt.Size = new System.Drawing.Size(72, 24);
      this._btnTransportGesamt.TabIndex = 27;
      this._btnTransportGesamt.Text = "Transport";
      this._btnTransportGesamt.Click += new System.EventHandler(this.BtnTransportGesamt_Click);
      // 
      // _btnTestForch
      // 
      this._btnTestForch.Location = new System.Drawing.Point(16, 156);
      this._btnTestForch.Name = "_btnTestForch";
      this._btnTestForch.Size = new System.Drawing.Size(75, 24);
      this._btnTestForch.TabIndex = 31;
      this._btnTestForch.Text = "Test Forch";
      this._btnTestForch.Click += new System.EventHandler(this.BtnTestForch_Click);
      // 
      // _btnTestDuo
      // 
      this._btnTestDuo.Location = new System.Drawing.Point(16, 220);
      this._btnTestDuo.Name = "_btnTestDuo";
      this._btnTestDuo.Size = new System.Drawing.Size(75, 24);
      this._btnTestDuo.TabIndex = 32;
      this._btnTestDuo.Text = "Test DUO";
      this._btnTestDuo.Click += new System.EventHandler(this.BtnTestDuo_Click);
      // 
      // _btnGpx
      // 
      this._btnGpx.Location = new System.Drawing.Point(199, 494);
      this._btnGpx.Name = "_btnGpx";
      this._btnGpx.Size = new System.Drawing.Size(81, 23);
      this._btnGpx.TabIndex = 33;
      this._btnGpx.Text = "Gpx";
      this._btnGpx.Click += new System.EventHandler(this.BtnGpx_Click);
      // 
      // WdgStart
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(478, 530);
      this.Controls.Add(this._btnGpx);
      this.Controls.Add(this._btnTestDuo);
      this.Controls.Add(this._btnTestForch);
      this.Controls.Add(this._grpGesamt);
      this.Controls.Add(this._grpStrecke);
      this.Controls.Add(this._grpUebergabe);
      this.Controls.Add(this._btnKml);
      this.Controls.Add(this._txtBis);
      this.Controls.Add(this._txtStrecke);
      this.Controls.Add(this._txtCreate);
      this.Controls.Add(this._label1);
      this.Controls.Add(this._btnTest);
      this.Controls.Add(this._btnOpen);
      this.Controls.Add(this._btnClip);
      this.Controls.Add(this._toSymbol);
      this.Controls.Add(this._lblFormat);
      this.Controls.Add(this._btnGetClipboard);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "WdgStart";
      this.Text = "SOLA";
      this._grpUebergabe.ResumeLayout(false);
      this._grpUebergabe.PerformLayout();
      this._grpStrecke.ResumeLayout(false);
      this._grpStrecke.PerformLayout();
      this._grpGesamt.ResumeLayout(false);
      this._grpGesamt.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
      CustomExceptionHandler eh = new CustomExceptionHandler();

      // Adds the event handler to to the event.
      Application.ThreadException += eh.OnThreadException;

      // Runs the application.
      Application.Run(new WdgStart());
    }


    #region events
    private void BtnGetClipboard_Click(object sender, EventArgs e)
    {
      try
      {
        // Create a new instance of the DataObject interface.
        IDataObject data = Clipboard.GetDataObject();
        string[] sFormats = data.GetFormats();
        int i = 0;
        foreach (var s in sFormats)
        {
          _lblFormat.Text = string.Format("{0}: {1}", i, s);
          _lblFormat.Refresh();
          //        System.Threading.Thread.Sleep(2000);
          i++;
        }
        string sFormat = "OCAD9 Object";
        Stream x = (Stream)data.GetData(sFormat);
        //        x = data.GetData("OCAD9 Symbol");

        _lblFormat.Text = "--";
        // TODO: Adapt x

        MemoryStream memStream = new MemoryStream((int)x.Length);
        for (i = 0; i < 8; i++)
        {
          memStream.WriteByte((byte)x.ReadByte());
        }
        int n0 = (x.ReadByte() << 0) + (x.ReadByte() << 8) + (x.ReadByte() << 16) + (x.ReadByte() << 24);
        x.Seek(40 + n0 * 8 + 8, SeekOrigin.Begin);
        int n1 = (x.ReadByte() << 0) + (x.ReadByte() << 8) + (x.ReadByte() << 16) + (x.ReadByte() << 24);

        memStream.WriteByte((byte)(n0 + n1));
        memStream.WriteByte(0);
        memStream.WriteByte(0);
        memStream.WriteByte(0); // TODO
        x.Seek(12, SeekOrigin.Begin);

        for (i = 12; i < 40 + n0 * 8; i++)
        {
          memStream.WriteByte((byte)x.ReadByte());
        }
        // inner ring
        x.Seek(40 + n0 * 8 + 40, SeekOrigin.Begin);
        for (i = 0; i < 4; i++)
        {
          memStream.WriteByte((byte)x.ReadByte());
        }
        memStream.WriteByte((byte)(x.ReadByte() | 2));
        for (i = 5; i < n1 * 8; i++)
        {
          memStream.WriteByte((byte)x.ReadByte());
        }

        for (i = 0; i < n1 * 256; i++)
        { // Add some more data
          memStream.WriteByte((byte)x.ReadByte());
        }

        data = new DataObject(sFormat, memStream);
        Clipboard.SetDataObject(data);
        // If the data is text, then set the text of the 
        // TextBox to the text in the Clipboard.
        //      if (data.GetDataPresent(DataFormats..Text))
        //        textBox1.Text = data.GetData(DataFormats.Text).ToString();
      }
      catch (Exception exp)
      { Debug.WriteLine(exp.Message); }

    }

    private void ToSymbol_Click(object sender, EventArgs e)
    {
      try
      {
        // Create a new instance of the DataObject interface.
        IDataObject data = Clipboard.GetDataObject();
        string sFormat = "OCAD9 Object";
        object pData = data.GetData(sFormat);
        ArrayList pInList = new ArrayList();
        _lblFormat.Text = "";

        if (pData == null)
        {
          _lblFormat.Text = "Ungültige Selektion";
          return;
        }

        // read clipboard
        OcadReader reader = OcadReader.Open((Stream)pData, ocadVersion: 9);
        reader.ReadElement(out MapElement elem);
        int size = 0;
        while (elem != null)
        {
          size += elem.PointCount() * 8 + 40;
          pInList.Add(elem);
          reader.ReadElement(out elem);
        }

        MemoryStream memStream = new MemoryStream(size);
        OcadWriter pWriter = OcadWriter.AppendTo(memStream, 9);

        foreach (var pElem in pInList)
        {
          pWriter.Write((Ocad.Symbol.SymbolGraphics)pElem);
        }
        for (int i = 0; i < 40; i++) // add some more data
        { memStream.WriteByte(0); }

        data = new DataObject("OCAD9 Symbol Graphics", memStream);
        Clipboard.SetDataObject(data);

        _lblFormat.Text = "Anpassen erfolgreich";
      }
      catch (Exception exp)
      { Debug.WriteLine(exp.Message); }
    }

    private void BtnClip_Click(object sender, EventArgs e)
    {
      try
      {
        // Create a new instance of the DataObject interface.
        IDataObject data = Clipboard.GetDataObject();
        string sFormat = "OCAD9 Object";
        object pData = data.GetData(sFormat);
        List<GeoElement> inList = new List<GeoElement>();
        _lblFormat.Text = "";

        if (pData == null)
        {
          _lblFormat.Text = "Ungültige Selektion";
          return;
        }

        // read clipboard
        OcadReader pReader = OcadReader.Open((Stream)pData, 9);
        pReader.ReadElement(out GeoElement elem);
        while (elem != null)
        {
          if (elem.Geometry is Area == false)
          {
            _lblFormat.Text = "Kein Flächenelement";
            return;
          }
          inList.Add(elem);
          pReader.ReadElement(out elem);
        }

        // groesste Geometry finden
        int nElem = inList.Count;
        if (nElem < 2)
        {
          _lblFormat.Text = "Nur eine Fläche";
          return;
        }

        Box boxMax = null;
        GeoElement maxElement = null;
        foreach (var inElem in inList)
        {
          if (boxMax == null)
          {
            boxMax = new Box(inElem.Geometry.Extent);
            maxElement = inElem;
            continue;
          }
          IBox elemBox = inElem.Geometry.Extent;
          if (elemBox.Min.X < boxMax.Min.X &&
            elemBox.Min.Y < boxMax.Min.Y &&
            elemBox.Max.X > boxMax.Max.X &&
            elemBox.Max.Y > boxMax.Max.Y)
          {
            boxMax = new Box(elemBox);
            maxElement = inElem;
          }
          else if (elemBox.Min.X <= boxMax.Min.X ||
            elemBox.Min.Y <= boxMax.Min.Y ||
            elemBox.Max.X >= boxMax.Max.X ||
            elemBox.Max.Y >= boxMax.Max.Y)
          {
            boxMax.Include(elemBox);
            maxElement = null;
          }
        }
        if (maxElement == null)
        {
          _lblFormat.Text = "Kein grösstes Element gefunden";
          return;
        }

        Area pAreaMax = (Area)maxElement.Geometry;
        foreach (var pElem in inList)
        {
          if (pElem == maxElement)
          { continue; }

          Area pArea = (Area)pElem.Geometry;
          if (pArea.Border.Count != 1)
          {
            _lblFormat.Text = "Inneres Polygon besteht nicht aus genau einer Linie";
            return;
          }

          pAreaMax.Border.Add(pArea.Border[0]);
        }

        MemoryStream memStream = new MemoryStream(maxElement.PointCount() * 8 + 40);
        OcadWriter writer = OcadWriter.AppendTo(memStream, ocadVersion: 9);
        writer.WriteElement(maxElement);
        for (int i = 0; i < 40; i++)
        { memStream.WriteByte(0); }

        data = new DataObject(sFormat, memStream);
        Clipboard.SetDataObject(data);

        _lblFormat.Text = "Anpassen erfolgreich";

      }
      catch (Exception exp)
      { Debug.WriteLine(exp.Message); }


    }

    private void BtnOpen_Click(object sender, EventArgs e)
    {
      try
      {
        _dlgOpen.FileName = _txtCreate.Text;
        if (_dlgOpen.ShowDialog(this) == DialogResult.OK)
        { _txtCreate.Text = _dlgOpen.FileName; }
      }
      catch (Exception exp)
      { Debug.WriteLine(exp.Message); }
    }

    private void BtnCreate_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;

      try
      {
        SolaData data = new SolaData(_txtCreate.Text, Ddx.DhmPfad);

        SolaProfile pProfile = new SolaProfile(data);
        int iVon = Convert.ToInt32(_txtStrecke.Text) - 1;

        if (int.TryParse(_txtBis.Text, out int iBis) == false)
        { iBis = iVon + 1; }

        List<string> outFiles = new List<string>();
        if (_chkDistance.Checked)
        {
          string name = string.Format("{0}_{1}_{2}", "profile", iVon + 1, iBis);
          outFiles.Add(GetProfile(pProfile, name, iVon, iBis, true));
        }
        else
        {
          for (int i = iVon; i < iBis; i++)
          {
            string name = string.Format("{0}_{1:00}", "strecke", i + 1);
            outFiles.Add(GetProfile(pProfile, name, i, i + 1, false));
          }
        }
        string expProfile = GetExpProfile();
        Ocad.Scripting.Utils.CreatePdf(Path.Combine(expProfile, "CreatePdf.xml"), outFiles);
      }
      catch (Exception exp)
      { Debug.WriteLine(exp.Message); }
      finally
      { Cursor = Cursors.Default; }
    }

    private string GetExpProfile()
    {
      string path =
        Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(_txtCreate.Text)),
        "Exp_Profile");
      return path;
    }
    private string GetProfile(SolaProfile profil, string name, int von, int bis, bool totalDistanz)
    {
      string expProfile = GetExpProfile();
      string sResult = Path.Combine(expProfile, name);
      Kategorie kat = Kategorie.Default;

      if (_chkDamen.Checked)
      {
        sResult += "_D";
        kat = Kategorie.Damen;
      }
      sResult += ".ocd";

      profil.WriteProfiles(
        Path.Combine(Path.GetDirectoryName(_txtCreate.Text), "profile9.ocd"),
        sResult, von, bis, totalDistanz, kat);

      return sResult;
    }

    private void BtnUebergabe_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        string dir = _txtUebergabe.Text;
        if (Directory.Exists(dir) == false)
        { Directory.CreateDirectory(dir); }

        SolaData strecken = new SolaData(_txtCreate.Text, null);
        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);

        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          string ocdFile = "Ue_" + Path.GetFileName(Ddx.Uebergabe[iStrecke].Vorlage);
          string result = Path.Combine(dir, ocdFile);

          {
            Uebergabe ue = new Uebergabe(strecken.Strecken, iStrecke);
            ue.Write(result);
          }
        }

        Ocad.Scripting.Utils.CreatePdf(dir);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnDetail_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        SolaData pStrecken = new SolaData(_txtCreate.Text, null);

        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);

        int iDetailMin = 0;
        int iDetailMax = 1;
        if (string.IsNullOrEmpty(_txtDetNr.Text) == false)
        {
          iDetailMin = Convert.ToInt32(_txtDetNr.Text);
          iDetailMax = iDetailMin;
        }

        string dir = _txtDetail.Text;
        if (Directory.Exists(dir) == false)
        { Directory.CreateDirectory(dir); }

        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          for (int iDetail = iDetailMin; iDetail <= iDetailMax; iDetail++)
          {
            Detail pDet = new Detail(iStrecke, iDetail);
            pDet.Write(Path.Combine(dir, "Det_" +
              Path.GetFileNameWithoutExtension(Ddx.Uebergabe[iStrecke].Vorlage) +
              "_" + iDetail + ".ocd"));
          }
        }

        Ocad.Scripting.Utils.CreatePdf(dir);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnTransport_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        SolaData data = new SolaData(_txtCreate.Text, null);

        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);

        string dir = _txtTransport.Text;

        if (Directory.Exists(dir) == false)
        { Directory.CreateDirectory(dir); }

        List<string> ocdFiles = new List<string>();
        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          UebergabeTransport trans = new UebergabeTransport(data, iStrecke);
          string name = Path.GetFileNameWithoutExtension(Ddx.Uebergabe[iStrecke].Vorlage);

          trans.CompleteStrecken();
          string ocdFile = Path.Combine(dir, "Tra_" + name + ".ocd");
          trans.Write(ocdFile);

          ocdFiles.Add(ocdFile);

          if (trans.TransFrom != null)
          {
            string from = Path.GetFileNameWithoutExtension(Ddx.Uebergabe[trans.TransFrom.From].Vorlage);
            string gpxName = string.Format("Tra_gpx_von_{0}_nach_{1}.gpx", from, name);
            trans.TransFrom.ExportGpx(Path.Combine(dir, gpxName));

            string kmlName = string.Format("Tra_kml_von_{0}_nach_{1}.kml", from, name);
            trans.TransFrom.ExportKml(Path.Combine(dir, kmlName), from, name);
          }
          if (trans.TransTo != null)
          {
            string to = Path.GetFileNameWithoutExtension(Ddx.Uebergabe[trans.TransTo.To].Vorlage);
            string gpxName = string.Format("Tra_gpx_von_{0}_nach_{1}.gpx", name, to);
            trans.TransTo.ExportGpx(Path.Combine(dir, gpxName));

            string kmlName = string.Format("Tra_kml_von_{0}_nach_{1}.kml", name, to);
            trans.TransTo.ExportKml(Path.Combine(dir, kmlName), name, to);
          }
        }
        string script = Path.Combine(dir, "CreatePdf.xml");
        Ocad.Scripting.Utils.CreatePdf(script, ocdFiles);
      }
      catch (Exception exp)
      { MessageBox.Show(exp.Message + "\n" + exp.StackTrace); }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnFull_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        string orig = _txtCreate.Text;
        Gesamtplan pGesamt = new Gesamtplan(orig, Ddx.DhmPfad);
        string s = Path.Combine(Path.GetDirectoryName(_txtCreate.Text), "gesamtplan.ocd");
        pGesamt.WriteStrecken(s);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private string BewilligungGesamt(string text)
    {
      string bew = string.Format("Reproduziert mit Bewilligung von swisstopo ({0})", text);
      return bew;
    }

    private void BtnEinsatz_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        string orig = _txtCreate.Text;
        Gesamtplan gesamt = new Gesamtplan(orig, Ddx.DhmPfad);
        string s = Path.GetDirectoryName(_txtCreate.Text) + Path.DirectorySeparatorChar +
          "sola-karte.ocd";
        gesamt.WriteEinsatz(s, BewilligungGesamt(_txtBewilligung.Text), _dtLaufdatum.Value);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnTransportGesamt_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        string orig = _txtCreate.Text;
        Gesamtplan pGesamt = new Gesamtplan(orig, Ddx.DhmPfad);
        string s = Path.GetDirectoryName(_txtCreate.Text) + Path.DirectorySeparatorChar +
          "transport.ocd";
        pGesamt.WriteTransport(s, BewilligungGesamt(_txtBewilligung.Text), _dtLaufdatum.Value);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnKml_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        SolaData data = new SolaData(_txtCreate.Text, Ddx.DhmPfad);
        data.KmlConfig.IncludeLookAt = true;
        data.KmlConfig.IncludeMarks = true;
        data.ExportKml("Sola.kml");

        data.KmlConfig.IncludeLookAt = false;
        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);
        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          data.ExportKml(iStrecke, string.Format("Strecke {0}.kml", iStrecke));
        }
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnStrecke_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);
        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          Streckenplan strecke = new Streckenplan(_txtCreate.Text,
            Ddx.DhmPfad, iStrecke);

          Kategorie kat = Kategorie.Default;
          if (_chkDamen.Checked)
          { kat = Kategorie.Damen; }

          strecke.Update(kat, _chkStartZiel.Checked);
        }
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnBegleit_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        ExportStrecke(_txtBegleit.Text, Streckenplan.Plantyp.Begleiter);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void BtnStreckeExp_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        ExportStrecke(_txtExpStrecke.Text, Streckenplan.Plantyp.Laeufer);
      }
      finally
      { Cursor = Cursors.Default; }
    }

    private void ExportStrecke(string exportDir, Streckenplan.Plantyp plantyp)
    {
      int iVon = Convert.ToInt32(_txtStrecke.Text);
      int iBis = Convert.ToInt32(_txtBis.Text);
      string orig = _txtCreate.Text;

      List<string> vorlagen = new List<string>();
      for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
      {
        string vorlage = Ddx.Strecken[iStrecke - 1].Vorlage;
        vorlagen.Add(vorlage);
      }
      Ocad.Scripting.Utils.Optimize(vorlagen, Path.Combine(Path.GetDirectoryName(vorlagen[0]), "Optimize.xml"));

      List<string> outFiles = new List<string>();
      for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
      {
        if (Directory.Exists(exportDir) == false)
        { Directory.CreateDirectory(exportDir); }

        Streckenplan strecke = new Streckenplan(orig, null, iStrecke);
        string outFile = Path.Combine(exportDir, Path.GetFileName(Ddx.Strecken[iStrecke - 1].Vorlage));
        string bewilligung = string.Format("Bewilligung swisstopo ({0})", _txtBewilligung.Text);
        strecke.Export(outFile, bewilligung, plantyp);

        outFiles.Add(outFile);
      }
      Ocad.Scripting.Utils.CreatePdf(Path.Combine(exportDir, "CreatePdf.xml"), outFiles);
    }

    private void BtnTest_Click(object sender, EventArgs e)
    {
      string ocdFile = Path.Combine(Path.GetDirectoryName(Ddx.Uebergabe[0].Vorlage), "Uebergabe_overview.ocd");
      using (OcadReader reader = OcadReader.Open(ocdFile))
      {
        Setup setup = reader.ReadSetup();
        foreach (var index in reader.ReadStringParamIndices())
        {
          if (index.Type != StringType.Template)
          { continue; }
          string s = reader.ReadStringParam(index);
          TemplatePar p = new TemplatePar(s);
          string name = Path.GetFileNameWithoutExtension(p.Name);
          if (!name.EndsWith("_"))
          { continue; }
          p.WriteWorldFile(setup);
        }
      }
    }
    private void TxtStrecke_TextChanged(object sender, EventArgs e)
    {
      _txtBis.Text = _txtStrecke.Text;
    }

    private void BtnTestForch_Click(object sender, EventArgs e)
    {
      string dir = @"C:\daten\ASVZ\Forchlauf\2016\Vorlagen\";
      string ocd = Path.Combine(dir, "Forchlauf10k.ocd");
      string dhm = Ddx.DhmPfad;
      Forchlauf.ForchData data = new Forchlauf.ForchData(ocd, dhm);

      data.ExportKml(@"C:\daten\temp\forchlauf.kml");

      string template = Path.GetDirectoryName(_txtCreate.Text) + Path.DirectorySeparatorChar +
        "profile9.ocd";

      Forchlauf.ForchProfile p = new Forchlauf.ForchProfile(data);
      p.WriteProfile(template, @"C:\daten\temp\profil_kurz.ocd", Forchlauf.Kategorie.Kurz);
      p.WriteProfile(template, @"C:\daten\temp\profil_mittel.ocd", Forchlauf.Kategorie.Mittel);
      p.WriteProfile(template, @"C:\daten\temp\profil_lang.ocd", Forchlauf.Kategorie.Lang);

      Forchlauf.ForchCategorie cat;
      Forchlauf.ForchLayout layout;

      string original = Path.Combine(dir, "Layout.ocd");
      string copy;

      List<string> layouts = new List<string>();
      {
        copy = Path.Combine(dir, "LayoutLang.ocd");
        layouts.Add(copy);
        cat = data.GetKategorie(Asvz.Forchlauf.Kategorie.Lang);
        File.Copy(original, copy, true);
        layout = new Forchlauf.ForchLayout(copy);
        layout.Update(cat);
      }

      {
        copy = Path.Combine(dir, "LayoutMittel.ocd");
        layouts.Add(copy);
        cat = data.GetKategorie(Asvz.Forchlauf.Kategorie.Mittel);
        File.Copy(original, copy, true);
        layout = new Forchlauf.ForchLayout(copy);
        layout.Update(cat);
      }

      {
        copy = Path.Combine(dir, "LayoutKurz.ocd");
        layouts.Add(copy);
        File.Copy(original, copy, true);
        cat = data.GetKategorie(Asvz.Forchlauf.Kategorie.Kurz);
        layout = new Forchlauf.ForchLayout(copy);
        layout.Update(cat);
      }

      {
        copy = Path.Combine(dir, "LayoutAll.ocd");
        layouts.Add(copy);
        File.Copy(original, copy, true);
        layout = new Forchlauf.ForchLayout(copy);
        layout.Update(data);

      }

      Ocad.Scripting.Utils.Optimize(layouts, Path.Combine(Path.GetDirectoryName(layouts[0]), "Optimize.xml"));
    }

    private void BtnTestDuo_Click(object sender, EventArgs e)
    {
      //Asvz.SolaDuo.DuoData.CreateDtm();

      string ocd = @"C:\daten\ASVZ\SOLA_Duo\2013\Vorlagen\Strecke.ocd";
      string dhm = "C:\\Daten\\ASVZ\\Daten\\Dhm\\solaDuoDhm.grd";

      SolaDuo.DuoData data = new SolaDuo.DuoData(ocd, dhm);

      data.ExportKml(@"C:\daten\ASVZ\SOLA_Duo\2013\Export\Sola_duo.kml");

      {
        //byte[] r = new byte[256];
        //byte[] g = new byte[256];
        //byte[] b = new byte[256];
        //for (int i = 0; i < 256; i++)
        //{
        //  r[i] = (byte)i;
        //  g[i] = (byte)i;
        //  b[i] = (byte)i;
        //}
        //Grid.DoubleGrid grd = data.Dhm;
        //Grid.DoubleGrid shade = grd.HillShading(HillShading);
        //Grid.ImageGrid.GridToTif((shade * 255).ToIntGrid(),
        //  @"C:\daten\ASVZ\SOLA_Duo\2009\Vorlagen\Shade.tif", r, g, b);
      }

      data.ExportDetails(@"C:\daten\ASVZ\SOLA_Duo\2013\Vorlagen\Vorlage_Details.ocd",
        @"C:\daten\ASVZ\SOLA_Duo\2013\Export\");

      string template = Path.Combine(@"C:\daten\ASVZ\Daten\Vorlagen", "profile9.ocd");

      Asvz.SolaDuo.DuoProfile p = new Asvz.SolaDuo.DuoProfile(data);
      p.WriteProfile(template, @"C:\daten\ASVZ\SOLA_Duo\2013\Export\profil.ocd", Asvz.SolaDuo.Kategorie.Strecke);


      template = @"C:\daten\ASVZ\SOLA_Duo\2013\Vorlagen\symbols_VECTOR200.ocd";
      string copy = @"C:\daten\ASVZ\SOLA_Duo\2013\Vorlagen\VECTOR200.ocd";
      File.Copy(template, copy, true);
      Asvz.SolaDuo.DuoMap duoData = new Asvz.SolaDuo.DuoMap(copy);

      duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_Road.shp");
      //duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_Access.shp");
      duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_Ramp.shp");

      duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_Railway.shp");

      //duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_Building.shp");

      duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_LandCover.shp");

      duoData.Import(@"C:\daten\ASVZ\Daten\VECTOR200\VEC200_FlowingWater.shp");

      duoData.Dispose();
    }

    public void Application_ThreadException(object sender,
      ThreadExceptionEventArgs e)
    {
      MessageBox.Show(e.Exception.Message + "\n" + e.Exception.StackTrace);
    }

    #endregion

    private void BtnGpx_Click(object sender, EventArgs e)
    {
      Cursor = Cursors.WaitCursor;
      try
      {
        SolaData data = new SolaData(_txtCreate.Text, Ddx.DhmPfad);

        int iVon = Convert.ToInt32(_txtStrecke.Text);
        int iBis = Convert.ToInt32(_txtBis.Text);
        List<Categorie> allCats = new List<Categorie>();
        for (int iStrecke = iVon; iStrecke <= iBis; iStrecke++)
        {
          Strecke s = data.Strecken[iStrecke - 1];
          data.ExportGpx(string.Format("Strecke {0}.gpx", iStrecke), new[] { s.Categories[0] });
          data.ExportKmGpx(string.Format("Strecke {0}.km.gpx", iStrecke), s.Categories[0]);
          allCats.Add(s.Categories[0]);

          for (int iCat = 1; iCat < s.Categories.Count; iCat++)
          {
            data.ExportGpx(string.Format("Strecke {0}_{1}.gpx", iStrecke, iCat), new[] { s.Categories[iCat] });
            data.ExportKmGpx(string.Format("Strecke {0}_{1}.km.gpx", iStrecke, iCat), s.Categories[iCat]);
          }
        }

        if (allCats.Count > 1)
        {
          data.ExportGpx($"Strecken {iVon}_{iBis}_joined.gpx", allCats, joined: true);
          data.ExportGpx($"Strecken {iVon}_{iBis}.gpx", allCats, joined: false);
        }
      }
      finally
      { Cursor = Cursors.Default; }

    }
  }
  internal class CustomExceptionHandler
  {

    // Handles the exception event.
    public void OnThreadException(object sender, ThreadExceptionEventArgs t)
    {
      DialogResult result = DialogResult.Cancel;
      try
      {
        result = ShowThreadExceptionDialog(t.Exception);
      }
      catch
      {
        try
        {
          MessageBox.Show("Fatal Error", "Fatal Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
        }
        finally
        {
          Application.Exit();
        }
      }

      // Exits the program when the user clicks Abort.
      if (result == DialogResult.Abort)
        Application.Exit();
    }

    // Creates the error message and displays it.
    private DialogResult ShowThreadExceptionDialog(Exception e)
    {
      string errorMsg = "An error occurred please contact the adminstrator with the following information:\n\n";
      errorMsg = errorMsg + e.Message + "\n\nStack Trace:\n" + e.StackTrace;
      return MessageBox.Show(errorMsg, "Application Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
    }
  }
}
