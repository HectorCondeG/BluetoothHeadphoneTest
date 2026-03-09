using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    public class SummaryPanel : Panel
    {
        public event Action OnRestart;

        private static readonly Color BgDark      = Color.FromArgb(18, 24, 38);
        private static readonly Color BgCard      = Color.FromArgb(26, 34, 54);
        private static readonly Color BgRow       = Color.FromArgb(22, 30, 50);
        private static readonly Color BgRowAlt    = Color.FromArgb(30, 40, 64);
        private static readonly Color AccentCyan  = Color.FromArgb(0, 212, 255);
        private static readonly Color AccentGreen = Color.FromArgb(0, 210, 120);
        private static readonly Color AccentRed   = Color.FromArgb(230, 60, 60);
        private static readonly Color AccentYellow= Color.FromArgb(255, 200, 0);
        private static readonly Color TextPrimary = Color.FromArgb(230, 240, 255);
        private static readonly Color TextMuted   = Color.FromArgb(110, 130, 170);

        public SummaryPanel(TestSession session)
        {
            BackColor = BgDark;
            Dock      = DockStyle.Fill;

            bool passed    = session.AllPassed;
            int  passCount = 0;
            foreach (var r in session.Records) if (r.Result == TestResult.Pass) passCount++;
            var duration = DateTime.Now - session.StartTime;

            // ── Scrollable card ──────────────────────────────────────────────
            var scroll = new Panel
            {
                Dock          = DockStyle.Fill,
                AutoScroll    = true,
                BackColor     = BgDark,
                Padding       = new Padding(20)
            };

            var card = new Panel
            {
                BackColor = BgCard,
                Width     = 860,
                Height    = 520,
                Location  = new Point(20, 20),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 60, 100), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            scroll.Controls.Add(card);
            scroll.Resize += (s, e) =>
            {
                card.Width  = Math.Max(600, scroll.ClientSize.Width - 40);
                RelayoutCard(card);
            };

            Controls.Add(scroll);
            BuildCard(card, session, passed, passCount, duration);
        }

        private void BuildCard(Panel card, TestSession session,
                               bool passed, int passCount, TimeSpan duration)
        {
            card.Controls.Clear();
            int w = card.Width;
            int y = 16;

            // ── Dispositivo y MAC ────────────────────────────────────────────
            var lblDev = new Label
            {
                Text      = $"Dispositivo: {session.SelectedDevice?.Name ?? "—"}",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize  = false,
                Size      = new Size(w - 40, 28),
                Location  = new Point(20, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblDev);
            y += 30;

            var lblMac = new Label
            {
                Text      = $"MAC: {session.SelectedDevice?.Address ?? "—"}",
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = TextMuted,
                AutoSize  = false,
                Size      = new Size(w - 40, 24),
                Location  = new Point(20, y),
                TextAlign = ContentAlignment.MiddleLeft
            };
            card.Controls.Add(lblMac);
            y += 32;

            // ── Separator ────────────────────────────────────────────────────
            var sep = new Panel
            {
                BackColor = Color.FromArgb(40, 60, 100),
                Location  = new Point(20, y),
                Size      = new Size(w - 40, 1),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(sep);
            y += 12;

            // ── Column headers ───────────────────────────────────────────────
            var hdrPanel = new Panel
            {
                BackColor = Color.FromArgb(14, 20, 36),
                Location  = new Point(20, y),
                Size      = new Size(w - 40, 30),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            AddRowLabel(hdrPanel, "PRUEBA",    0,   w - 280, new Font("Segoe UI", 9f, FontStyle.Bold), AccentCyan);
            AddRowLabel(hdrPanel, "RESULTADO", w - 280, 120, new Font("Segoe UI", 9f, FontStyle.Bold), AccentCyan, ContentAlignment.MiddleCenter);
            AddRowLabel(hdrPanel, "HORA",      w - 160, 140, new Font("Segoe UI", 9f, FontStyle.Bold), AccentCyan, ContentAlignment.MiddleRight);
            card.Controls.Add(hdrPanel);
            y += 34;

            // ── Result rows ──────────────────────────────────────────────────
            int i = 0;
            foreach (var rec in session.Records)
            {
                bool ok = rec.Result == TestResult.Pass;
                bool na = rec.Result == TestResult.Pending;

                var row = new Panel
                {
                    BackColor = i % 2 == 0 ? BgRow : BgRowAlt,
                    Location  = new Point(20, y),
                    Size      = new Size(w - 40, 42),
                    Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var accent = new Panel
                {
                    BackColor = ok ? AccentGreen : (na ? TextMuted : AccentRed),
                    Location  = new Point(0, 0),
                    Size      = new Size(4, 42)
                };
                row.Controls.Add(accent);

                AddRowLabel(row, rec.Name, 14, w - 300,
                    new Font("Segoe UI", 10.5f), TextPrimary);

                string resText  = ok ? "✔  PASS" : (na ? "—  N/A" : "✘  FAIL");
                Color  resColor = ok ? AccentGreen : (na ? TextMuted : AccentRed);
                AddRowLabel(row, resText, w - 280, 120,
                    new Font("Segoe UI", 10f, FontStyle.Bold), resColor,
                    ContentAlignment.MiddleCenter);

                string timeText = rec.Timestamp.HasValue
                    ? rec.Timestamp.Value.ToString("HH:mm:ss") : "--:--:--";
                AddRowLabel(row, timeText, w - 160, 140,
                    new Font("Segoe UI", 9.5f), TextMuted,
                    ContentAlignment.MiddleRight);

                card.Controls.Add(row);
                y += 46;
                i++;
            }

            y += 16;

            // ── Buttons ──────────────────────────────────────────────────────
            var btnNew  = MakeButton("↺  NUEVA PRUEBA", Color.FromArgb(0, 80, 120),  20,  y, 190, 44);
            var btnSave = MakeButton("💾  GUARDAR TXT",  Color.FromArgb(0, 100, 60), 224, y, 190, 44);
            btnNew.Click  += (s, e) => OnRestart?.Invoke();
            btnSave.Click += (s, e) => SaveTxtReport(session, passed, passCount, duration);
            card.Controls.AddRange(new Control[] { btnNew, btnSave });

            card.Height = y + 44 + 20;
        }

        private void RelayoutCard(Panel card)
        {
            // Rebuild card when width changes
            if (card.Tag is TestSession session)
                BuildCard(card, session,
                    session.AllPassed,
                    CountPassed(session),
                    DateTime.Now - session.StartTime);
        }

        private int CountPassed(TestSession s)
        {
            int c = 0;
            foreach (var r in s.Records) if (r.Result == TestResult.Pass) c++;
            return c;
        }

        private void AddRowLabel(Panel parent, string text, int x, int width,
            Font font, Color color,
            ContentAlignment align = ContentAlignment.MiddleLeft)
        {
            var lbl = new Label
            {
                Text      = text,
                Font      = font,
                ForeColor = color,
                AutoSize  = false,
                Size      = new Size(width, parent.Height),
                Location  = new Point(x, 0),
                TextAlign = align,
                Padding   = new Padding(align == ContentAlignment.MiddleLeft ? 8 : 0, 0, 4, 0)
            };
            parent.Controls.Add(lbl);
        }

        private Button MakeButton(string text, Color bg, int x, int y, int w, int h)
        {
            var btn = new Button
            {
                Text      = text,
                Size      = new Size(w, h),
                Location  = new Point(x, y),
                FlatStyle = FlatStyle.Flat,
                BackColor = bg,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ── TXT Report ───────────────────────────────────────────────────────
        private void SaveTxtReport(TestSession session, bool passed,
                                   int passCount, TimeSpan duration)
        {
            using var dlg = new SaveFileDialog
            {
                Title      = "Guardar reporte de prueba",
                Filter     = "Archivo de texto (*.txt)|*.txt",
                FileName   = $"Prueba_{session.Folio}_{session.StartTime:yyyyMMdd_HHmm}.txt",
                DefaultExt = "txt"
            };

            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                var sb = new System.Text.StringBuilder();
                string sep = new string('═', 54);
                string sep2= new string('─', 54);

                sb.AppendLine($"Dispositivo : {session.SelectedDevice?.Name ?? "—"}");
                sb.AppendLine($"MAC         : {session.SelectedDevice?.Address ?? "—"}");
                sb.AppendLine();
                sb.AppendLine(sep2);
                sb.AppendLine($"  {"PRUEBA",-32} {"RESULTADO",-10} {"HORA"}");
                sb.AppendLine(sep2);

                foreach (var rec in session.Records)
                {
                    string res  = rec.Result == TestResult.Pass ? "PASS" :
                                  rec.Result == TestResult.Fail ? "FAIL" : "N/A";
                    string time = rec.Timestamp.HasValue
                        ? rec.Timestamp.Value.ToString("HH:mm:ss") : "--:--:--";
                    sb.AppendLine($"  {rec.Name,-32} {res,-10} {time}");
                }

                sb.AppendLine(sep2);

                File.WriteAllText(dlg.FileName, sb.ToString(),
                    System.Text.Encoding.UTF8);

                MessageBox.Show(
                    $"Reporte guardado correctamente:\n{dlg.FileName}",
                    "Reporte guardado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al guardar el reporte:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
