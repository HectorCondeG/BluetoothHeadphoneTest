using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // Header
        private Panel panelHeader;
        private Label labelTitle;
        private Label labelSubtitle;
        private Label labelFolio;
        private Label labelDateTime;

        // Progress
        private Panel panelProgress;
        private Label labelProgress;
        private ProgressBar progressBarMain;

        // Main content
        private Panel panelContent;
        public Panel panelTestArea;

        // Footer
        private Panel panelFooter;
        private Button btnPass;
        private Button btnFail;
        private Button btnReset;
        private Label labelStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            if (disposing) _globalHook?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();

            // ── FORM ──────────────────────────────────────────────
            this.Text = "PRUEBA DE AUDÍFONOS BLUETOOTH";
            this.Size = new Size(900, 700);
            this.MinimumSize = new Size(900, 700);
            this.BackColor = Color.FromArgb(18, 24, 38);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 10f);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Load += MainForm_Load;

            // ── HEADER ────────────────────────────────────────────
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(12, 16, 28),
                Padding = new Padding(20, 0, 20, 0)
            };

            labelTitle = new Label
            {
                Text = "⬡  SISTEMA DE PRUEBA — AUDÍFONOS BLUETOOTH",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255),
                AutoSize = false,
                Size = new Size(580, 40),
                Location = new Point(20, 12),
                TextAlign = ContentAlignment.MiddleLeft
            };

            labelSubtitle = new Label
            {
                Text = "Línea de Control de Calidad — Maquila",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(120, 140, 180),
                AutoSize = false,
                Size = new Size(400, 24),
                Location = new Point(20, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };

            labelFolio = new Label
            {
                Text = "FOLIO: ---",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 0),
                AutoSize = false,
                Size = new Size(200, 30),
                Location = new Point(660, 12),
                TextAlign = ContentAlignment.MiddleRight
            };

            labelDateTime = new Label
            {
                Text = DateTime.Now.ToString("dd/MMM/yyyy  HH:mm"),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(120, 140, 180),
                AutoSize = false,
                Size = new Size(200, 24),
                Location = new Point(660, 46),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelHeader.Controls.AddRange(new Control[] { labelTitle, labelSubtitle, labelFolio, labelDateTime });
            
            // Timer para fecha/hora
            var clockTimer = new System.Windows.Forms.Timer(components) { Interval = 1000, Enabled = true };
            clockTimer.Tick += (s, e) => RefreshDateTime();

            // ── PROGRESS ──────────────────────────────────────────
            panelProgress = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(22, 30, 50),
                Padding = new Padding(20, 8, 20, 8)
            };

            labelProgress = new Label
            {
                Text = "Prueba 1 de 6",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 212, 255),
                AutoSize = false,
                Size = new Size(160, 28),
                Location = new Point(20, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            progressBarMain = new ProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Size = new Size(580, 18),
                Location = new Point(190, 14),
                Style = ProgressBarStyle.Continuous
            };

            panelProgress.Controls.AddRange(new Control[] { labelProgress, progressBarMain });

            // ── CONTENT ───────────────────────────────────────────
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(18, 24, 38),
                Padding = new Padding(20)
            };

            panelTestArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            panelContent.Controls.Add(panelTestArea);

            // ── FOOTER ────────────────────────────────────────────
            panelFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = Color.FromArgb(12, 16, 28),
                Padding = new Padding(20, 10, 20, 10)
            };

            btnPass = new Button
            {
                Text = "✔  APROBADO",
                Size = new Size(160, 46),
                Location = new Point(20, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 160, 80),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnPass.FlatAppearance.BorderSize = 0;
            btnPass.Visible = false;

            btnFail = new Button
            {
                Text = "✘  FALLIDO",
                Size = new Size(160, 46),
                Location = new Point(196, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 40, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFail.FlatAppearance.BorderSize = 0;
            btnFail.Visible = false;

            btnReset = new Button
            {
                Text = "↺  REINICIAR",
                Size = new Size(140, 46),
                Location = new Point(372, 13),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 60, 90),
                ForeColor = Color.FromArgb(180, 200, 240),
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.Click += (s, e) => stepManager?.OnReset();

            labelStatus = new Label
            {
                Text = "Esperando inicio de prueba...",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(120, 140, 180),
                AutoSize = false,
                Size = new Size(280, 46),
                Location = new Point(530, 13),
                TextAlign = ContentAlignment.MiddleRight
            };

            panelFooter.Controls.AddRange(new Control[] { btnPass, btnFail, btnReset, labelStatus });

            // ── ASSEMBLE ──────────────────────────────────────────
            this.Controls.Add(panelContent);
            this.Controls.Add(panelProgress);
            this.Controls.Add(panelHeader);
            this.Controls.Add(panelFooter);

            // Expose refs
            this.BtnPass = btnPass;
            this.BtnFail = btnFail;
            this.LabelStatus = labelStatus;

            this.ResumeLayout(false);
        }

        // Public refs for TestStepManager
        public Button BtnPass { get; private set; }
        public Button BtnFail { get; private set; }
        public Label LabelStatus { get; private set; }
    }
}
