using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    public class DeviceSelectForm : Form
    {
        // El dispositivo que eligió el operador
        public BluetoothDeviceInfo SelectedDevice { get; private set; }

        private static readonly Color BgDark       = Color.FromArgb(18, 24, 38);
        private static readonly Color BgCard       = Color.FromArgb(26, 34, 54);
        private static readonly Color AccentCyan   = Color.FromArgb(0, 212, 255);
        private static readonly Color AccentGreen  = Color.FromArgb(0, 210, 120);
        private static readonly Color AccentYellow = Color.FromArgb(255, 200, 0);
        private static readonly Color TextPrimary  = Color.FromArgb(230, 240, 255);
        private static readonly Color TextMuted    = Color.FromArgb(110, 130, 170);

        private ListBox listDevices;
        private Button  btnRefresh;
        private Button  btnStart;
        private Label   lblStatus;
        private Label   lblInstruction;
        private List<BluetoothDeviceInfo> _devices = new List<BluetoothDeviceInfo>();

        public DeviceSelectForm()
        {
            InitUI();
            LoadDevices();

        }

        private void InitUI()
        {
            Text            = "Seleccionar Dispositivo Bluetooth";
            Size            = new Size(560, 520);
            MinimumSize     = new Size(560, 520);
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = BgDark;
            ForeColor       = TextPrimary;
            Font            = new Font("Segoe UI", 10f);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;

            // ── Header ──────────────────────────────────────────────
            var panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = Color.FromArgb(12, 16, 28)
            };

            var lblTitle = new Label
            {
                Text      = "🔵  SELECCIÓN DE DISPOSITIVO",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = AccentCyan,
                AutoSize  = false,
                Size      = new Size(520, 40),
                Location  = new Point(20, 12),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblSub = new Label
            {
                Text      = "Seleccione el audífono a probar antes de iniciar la secuencia.",
                Font      = new Font("Segoe UI", 9f),
                ForeColor = TextMuted,
                AutoSize  = false,
                Size      = new Size(520, 24),
                Location  = new Point(20, 50),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // ── Instruction ─────────────────────────────────────────
            lblInstruction = new Label
            {
                Text      = "Dispositivos Bluetooth pareados en este equipo:",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize  = false,
                Size      = new Size(520, 28),
                Location  = new Point(20, 96),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ── Device List ─────────────────────────────────────────
            listDevices = new ListBox
            {
                Location         = new Point(20, 130),
                Size             = new Size(520, 220),
                BackColor        = BgCard,
                ForeColor        = TextPrimary,
                Font             = new Font("Segoe UI", 11f),
                BorderStyle      = BorderStyle.FixedSingle,
                ItemHeight       = 36,
                DrawMode         = DrawMode.OwnerDrawFixed
            };
            listDevices.DrawItem         += ListDevices_DrawItem;
            listDevices.DoubleClick      += (s, e) => TryStart();
            listDevices.SelectedIndexChanged += (s, e) => UpdateButtons();

            // ── Status ──────────────────────────────────────────────
            lblStatus = new Label
            {
                Text      = "🔄  Buscando dispositivos...",
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = AccentYellow,
                AutoSize  = false,
                Size      = new Size(520, 28),
                Location  = new Point(20, 360),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ── Buttons ─────────────────────────────────────────────
            btnRefresh = new Button
            {
                Text      = "↺  Actualizar",
                Size      = new Size(150, 44),
                Location  = new Point(20, 400),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(40, 50, 80),
                ForeColor = Color.FromArgb(180, 200, 240),
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadDevices();

            btnStart = new Button
            {
                Text      = "▶  Iniciar Pruebas",
                Size      = new Size(200, 44),
                Location  = new Point(340, 400),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 100, 60),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Enabled   = false
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += (s, e) => TryStart();

            // ── Assemble ────────────────────────────────────────────
            Controls.AddRange(new Control[] {
                panelHeader, lblInstruction, listDevices,
                lblStatus, btnRefresh, btnStart });
        }

        private void LoadDevices()
        {
            lblStatus.Text      = "🔄  Actualizando lista...";
            lblStatus.ForeColor = AccentYellow;

            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                var devices = BluetoothDetector.GetPairedDevices();
                Invoke(new Action(() =>
                {
                    _devices = devices;
                    listDevices.Items.Clear();

                    if (devices.Count == 0)
                    {
                        listDevices.Items.Add("(No se encontraron dispositivos pareados)");
                        lblStatus.Text      = "⚠  No hay dispositivos BT pareados. Parée el audífono primero.";
                        lblStatus.ForeColor = Color.FromArgb(230, 60, 60);
                    }
                    else
                    {
                        foreach (var d in devices)
                            listDevices.Items.Add(d);

                        int connected = 0;
                        foreach (var d in devices) if (d.IsConnected) connected++;

                        lblStatus.Text = connected > 0
                            ? $"✔  {devices.Count} dispositivo(s) encontrado(s), {connected} conectado(s)."
                            : $"ℹ  {devices.Count} dispositivo(s) pareado(s). Ninguno conectado aún.";
                        lblStatus.ForeColor = connected > 0 ? AccentGreen : AccentYellow;
                    }

                    UpdateButtons();
                }));
            });
        }

        private void UpdateButtons()
        {
            bool validSelection = listDevices.SelectedIndex >= 0
                               && listDevices.SelectedItem is BluetoothDeviceInfo;
            btnStart.Enabled   = validSelection;
            btnStart.BackColor = validSelection
                ? Color.FromArgb(0, 130, 70)
                : Color.FromArgb(30, 50, 40);
        }

        private void TryStart()
        {
            if (listDevices.SelectedItem is BluetoothDeviceInfo dev)
            {
                SelectedDevice = dev;
                DialogResult   = DialogResult.OK;
                Close();
            }
        }

        private void ListDevices_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();

            var item = listDevices.Items[e.Index];
            bool selected = (e.State & DrawItemState.Selected) != 0;

            var bgColor = selected
                ? Color.FromArgb(0, 60, 90)
                : (e.Index % 2 == 0 ? BgCard : Color.FromArgb(30, 40, 62));

            using var bgBrush = new SolidBrush(bgColor);
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

            if (item is BluetoothDeviceInfo dev)
            {
                // Status dot
                var dotColor = dev.IsConnected ? AccentGreen : Color.FromArgb(100, 110, 130);
                using var dotBrush = new SolidBrush(dotColor);
                e.Graphics.FillEllipse(dotBrush,
                    e.Bounds.Left + 12, e.Bounds.Top + 12, 12, 12);

                // Device name
                using var nameBrush = new SolidBrush(TextPrimary);
                e.Graphics.DrawString(dev.Name,
                    new Font("Segoe UI", 11f, FontStyle.Bold), nameBrush,
                    e.Bounds.Left + 34, e.Bounds.Top + 4);

                // MAC + status
                string sub = dev.IsConnected
                    ? $"{dev.Address}  •  Conectado"
                    : $"{dev.Address}  •  No conectado";
                using var subBrush = new SolidBrush(dev.IsConnected ? AccentGreen : TextMuted);
                e.Graphics.DrawString(sub,
                    new Font("Segoe UI", 8.5f), subBrush,
                    e.Bounds.Left + 34, e.Bounds.Top + 18);
            }
            else
            {
                using var b = new SolidBrush(TextMuted);
                e.Graphics.DrawString(item.ToString(),
                    new Font("Segoe UI", 10f, FontStyle.Italic), b,
                    e.Bounds.Left + 12, e.Bounds.Top + 8);
            }

            e.DrawFocusRectangle();
        }

        protected override void Dispose(bool disposing)
        {

            base.Dispose(disposing);
        }
    }
}
