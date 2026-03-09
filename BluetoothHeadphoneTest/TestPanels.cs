using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace BluetoothHeadphoneTest
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  BASE PANEL
    // ═══════════════════════════════════════════════════════════════════════════
    public abstract class TestPanel : Panel
    {
        protected static readonly Color BgDark       = Color.FromArgb(18, 24, 38);
        protected static readonly Color BgCard       = Color.FromArgb(26, 34, 54);
        protected static readonly Color AccentCyan   = Color.FromArgb(0, 212, 255);
        protected static readonly Color AccentYellow = Color.FromArgb(255, 200, 0);
        protected static readonly Color AccentGreen  = Color.FromArgb(0, 210, 120);
        protected static readonly Color AccentRed    = Color.FromArgb(230, 60, 60);
        protected static readonly Color AccentOrange = Color.FromArgb(255, 140, 0);
        protected static readonly Color TextPrimary  = Color.FromArgb(230, 240, 255);
        protected static readonly Color TextMuted    = Color.FromArgb(110, 130, 170);

        public event Action<bool> TestCompleted;

        protected Panel card;
        protected Label labelTestNumber;
        protected Label labelTestName;
        protected Label labelIcon;
        protected Panel stepsPanel;
        protected Label labelStatusIndicator;
        protected Panel statusBar;

        // Mini player widget shown on relevant tests
        protected MiniPlayerWidget Player;

        protected TestPanel(int number, string name, string icon, bool withPlayer = false)
        {
            BackColor = BgDark;
            Padding   = new Padding(24);

            card = new Panel { BackColor = BgCard, Dock = DockStyle.Fill };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(40, 60, 100), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            labelTestNumber = new Label
            {
                Text = $"PRUEBA {number} / {TestStepManager.TotalTests}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AccentCyan, BackColor = Color.FromArgb(0, 40, 60),
                AutoSize = false, Size = new Size(160, 28), Location = new Point(24, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelIcon = new Label
            {
                Text = icon, Font = new Font("Segoe UI Emoji", 38f),
                ForeColor = AccentYellow, AutoSize = false,
                Size = new Size(80, 76), Location = new Point(24, 58),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelTestName = new Label
            {
                Text = name, Font = new Font("Segoe UI", 17f, FontStyle.Bold),
                ForeColor = TextPrimary, AutoSize = false,
                Size = new Size(660, 48), Location = new Point(112, 66),
                TextAlign = ContentAlignment.MiddleLeft
            };

            statusBar = new Panel
                { BackColor = Color.FromArgb(22, 30, 50), Height = 50, Dock = DockStyle.Bottom };
            labelStatusIndicator = new Label
            {
                Text = "⏳  Esperando acción del audífono...",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AccentYellow, AutoSize = false, Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            statusBar.Controls.Add(labelStatusIndicator);

            int stepsTop = withPlayer ? 152 : 152;
            stepsPanel = new Panel
            {
                BackColor = Color.FromArgb(22, 30, 50),
                Location  = new Point(24, stepsTop),
                Size      = new Size(800, withPlayer ? 160 : 240),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            card.Controls.AddRange(new Control[]
                { labelTestNumber, labelIcon, labelTestName, stepsPanel, statusBar });

            if (withPlayer)
            {
                Player = new MiniPlayerWidget
                {
                    Location = new Point(24, stepsTop + stepsPanel.Height + 10),
                    Size     = new Size(820, 110),
                    Anchor   = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                card.Controls.Add(Player);
                card.Resize += (s, e) =>
                {
                    stepsPanel.Size = new Size(card.Width - 48, 160);
                    Player.Size     = new Size(card.Width - 48, 110);
                    Player.Location = new Point(24, stepsTop + stepsPanel.Height + 10);
                };
            }
            else
            {
                card.Resize += (s, e) =>
                    stepsPanel.Size = new Size(card.Width - 48, card.Height - 230);
            }

            Controls.Add(card);
        }

        protected void SetStatus(string text, Color color)
        {
            if (InvokeRequired) { Invoke(new Action(() => SetStatus(text, color))); return; }
            labelStatusIndicator.Text      = text;
            labelStatusIndicator.ForeColor = color;
        }

        protected void AutoPass(string message)
        {
            SetStatus("✔  " + message, AccentGreen);
            var t = new System.Windows.Forms.Timer { Interval = 1400 };
            t.Tick += (s, e) => { t.Stop(); TestCompleted?.Invoke(true); };
            t.Start();
        }

        protected void AutoFail(string message)
        {
            SetStatus("✘  " + message, AccentRed);
            var t = new System.Windows.Forms.Timer { Interval = 1500 };
            t.Tick += (s, e) => { t.Stop(); TestCompleted?.Invoke(false); };
            t.Start();
        }

        protected Label MakeStep(int num, string text, int yOffset)
        {
            var badge = new Label
            {
                Text = num.ToString(), Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = BgDark, BackColor = AccentCyan,
                AutoSize = false, Size = new Size(30, 30),
                Location = new Point(20, yOffset + 2),
                TextAlign = ContentAlignment.MiddleCenter
            };
            badge.Paint += (s, e) =>
            {
                using var path = new GraphicsPath();
                path.AddEllipse(0, 0, ((Label)s).Width - 1, ((Label)s).Height - 1);
                ((Label)s).Region = new Region(path);
            };
            var lbl = new Label
            {
                Text = text, Font = new Font("Segoe UI", 10.5f), ForeColor = TextPrimary,
                AutoSize = false, Size = new Size(stepsPanel.Width - 80, 36),
                Location = new Point(62, yOffset), TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            stepsPanel.Controls.Add(badge);
            stepsPanel.Controls.Add(lbl);
            stepsPanel.Resize += (s, e) => lbl.Size = new Size(stepsPanel.Width - 80, 36);
            return lbl;
        }

        protected Label MakeInfo(string text, int y)
        {
            var lbl = new Label
            {
                Text = "ℹ  " + text, Font = new Font("Segoe UI", 9.5f),
                ForeColor = AccentCyan, BackColor = Color.FromArgb(10, 36, 50),
                AutoSize = false, Size = new Size(stepsPanel.Width - 40, 30),
                Location = new Point(20, y), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            stepsPanel.Controls.Add(lbl);
            stepsPanel.Resize += (s, e) => lbl.Size = new Size(stepsPanel.Width - 40, 30);
            return lbl;
        }

        protected Label MakeWarning(string text, int y)
        {
            var lbl = new Label
            {
                Text = "⚠  " + text, Font = new Font("Segoe UI", 9.5f, FontStyle.Italic),
                ForeColor = AccentYellow, BackColor = Color.FromArgb(40, 36, 10),
                AutoSize = false, Size = new Size(stepsPanel.Width - 40, 30),
                Location = new Point(20, y), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            stepsPanel.Controls.Add(lbl);
            stepsPanel.Resize += (s, e) => lbl.Size = new Size(stepsPanel.Width - 40, 30);
            return lbl;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  MINI PLAYER WIDGET — visible en pruebas de Play/Pausa, Anterior, Siguiente
    // ═══════════════════════════════════════════════════════════════════════════
    public class MiniPlayerWidget : Panel
    {
        public AudioPlayer Audio { get; private set; }

        private Label _lblTrack;
        private Label _lblState;
        private Label _lblLastCmd;
        private Panel _vizBar;
        private System.Windows.Forms.Timer _vizTimer;

        private static readonly Color Bg      = Color.FromArgb(14, 20, 36);
        private static readonly Color Cyan    = Color.FromArgb(0, 212, 255);
        private static readonly Color Green   = Color.FromArgb(0, 210, 120);
        private static readonly Color Yellow  = Color.FromArgb(255, 200, 0);
        private static readonly Color Muted   = Color.FromArgb(110, 130, 170);
        private static readonly Color Orange  = Color.FromArgb(255, 140, 0);

        private int _vizPhase = 0;

        public MiniPlayerWidget()
        {
            BackColor   = Bg;
            BorderStyle = BorderStyle.None;
            Padding     = new Padding(10);

            // State badge
            _lblState = new Label
            {
                Text = "▶  REPRODUCIENDO",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Green, AutoSize = false,
                Size = new Size(200, 28), Location = new Point(10, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Track name
            _lblTrack = new Label
            {
                Text = "♪  Pista 1 — Do (261 Hz)",
                Font = new Font("Segoe UI", 10f), ForeColor = Cyan,
                AutoSize = false, Size = new Size(380, 28), Location = new Point(220, 8),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Last command received
            _lblLastCmd = new Label
            {
                Text = "Último comando: (ninguno)",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic), ForeColor = Muted,
                AutoSize = false, Size = new Size(Width - 20, 24), Location = new Point(10, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Visualizer bar (fake VU meter — shows audio is active)
            _vizBar = new Panel
            {
                BackColor = Color.FromArgb(20, 30, 50),
                Location  = new Point(10, 70),
                Size      = new Size(Width - 20, 28),
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _vizBar.Paint += DrawViz;

            Controls.AddRange(new Control[] { _lblState, _lblTrack, _lblLastCmd, _vizBar });
            Resize += (s, e) =>
            {
                _lblLastCmd.Size = new Size(Width - 20, 24);
                _vizBar.Size     = new Size(Width - 20, 28);
            };

            // Create and start audio
            Audio = new AudioPlayer();

            Audio.StateChanged += OnStateChanged;
            Audio.TrackChanged += OnTrackChanged;

            // Subscribe to all media commands to show last cmd label
            AppCommandRouter.OnMediaKey += OnCommand;

            // Register as active player for AppCommandRouter
            AppCommandRouter.ActivePlayer = Audio;

            // Start playing
            Audio.Play();

            // Visualizer animation
            _vizTimer = new System.Windows.Forms.Timer { Interval = 80 };
            _vizTimer.Tick += (s, e) =>
            {
                if (Audio.State == AudioPlayer.PlayerState.Playing)
                {
                    _vizPhase++;
                    _vizBar.Invalidate();
                }
            };
            _vizTimer.Start();
        }

        private void OnStateChanged(AudioPlayer.PlayerState state)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnStateChanged(state))); return; }
            switch (state)
            {
                case AudioPlayer.PlayerState.Playing:
                    _lblState.Text = "▶  REPRODUCIENDO"; _lblState.ForeColor = Green; break;
                case AudioPlayer.PlayerState.Paused:
                    _lblState.Text = "⏸  PAUSADO"; _lblState.ForeColor = Orange; break;
                case AudioPlayer.PlayerState.Stopped:
                    _lblState.Text = "⏹  DETENIDO"; _lblState.ForeColor = Muted; break;
            }
            _vizBar.Invalidate();
        }

        private void OnTrackChanged(int track)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnTrackChanged(track))); return; }
            _lblTrack.Text = $"♪  {Audio.TrackName}";
        }

        private void OnCommand(System.Windows.Forms.Keys key)
        {
            if (InvokeRequired) { Invoke(new Action(() => OnCommand(key))); return; }
            string name = key switch
            {
                System.Windows.Forms.Keys.MediaPlayPause    => "Play / Pausa",
                System.Windows.Forms.Keys.MediaNextTrack    => "Pista Siguiente ▶▶",
                System.Windows.Forms.Keys.MediaPreviousTrack=> "Pista Anterior ◀◀",
                System.Windows.Forms.Keys.VolumeUp          => "Subir Volumen +",
                System.Windows.Forms.Keys.VolumeDown        => "Bajar Volumen −",
                _                                           => key.ToString()
            };
            _lblLastCmd.Text      = $"Último comando detectado: {name}";
            _lblLastCmd.ForeColor = Cyan;
        }

        private void DrawViz(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            var g     = e.Graphics;
            var panel = (Panel)sender;
            g.Clear(Color.FromArgb(20, 30, 50));

            if (Audio.State != AudioPlayer.PlayerState.Playing)
            {
                using var b = new SolidBrush(Color.FromArgb(40, 50, 70));
                g.FillRectangle(b, 2, panel.Height / 2 - 1, panel.Width - 4, 2);
                return;
            }

            int bars  = 40;
            float barW = (float)(panel.Width - 4) / bars;
            var rng   = new Random(42);
            for (int i = 0; i < bars; i++)
            {
                double h = (Math.Sin((_vizPhase * 0.18) + i * 0.45) * 0.5 + 0.5)
                         * (Math.Sin((_vizPhase * 0.07) + i * 0.9) * 0.3 + 0.7)
                         * (panel.Height - 4);
                float x = 2 + i * barW;
                float y = (float)(panel.Height - 2 - h);
                using var b = new SolidBrush(Color.FromArgb(0, 180 + (int)(h * 2), 200));
                g.FillRectangle(b, x, y, barW - 1, (float)h);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AppCommandRouter.OnMediaKey -= OnCommand;
                if (AppCommandRouter.ActivePlayer == Audio)
                    AppCommandRouter.ActivePlayer = null;
                _vizTimer?.Dispose();
                Audio?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  1. BLUETOOTH CONNECTION
    // ═══════════════════════════════════════════════════════════════════════════
    public class BluetoothConnectionPanel : TestPanel
    {
        private System.Windows.Forms.Timer _pollTimer;
        private int _attempts = 0;
        private const int MAX_ATTEMPTS = 30;
        private string _targetMac;
        private Label _lblDeviceName;

        public BluetoothConnectionPanel() : base(1, "Conexión Bluetooth", "🔵")
        {
            MakeStep(1, "Encienda los audífonos — el LED debe parpadear en AZUL.", 14);
            MakeStep(2, "Asegúrese de que el audífono ya esté PAREADO con este equipo.", 56);
            MakeStep(3, "Active el Bluetooth en el audífono para que se conecte.", 98);
            MakeStep(4, "El sistema detectará la conexión automáticamente.", 140);

            _lblDeviceName = new Label
            {
                Text = "Dispositivo objetivo: (cargando...)",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AccentCyan, BackColor = Color.FromArgb(0, 30, 50),
                AutoSize = false, Size = new Size(stepsPanel.Width - 40, 28),
                Location = new Point(20, 188), TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            stepsPanel.Controls.Add(_lblDeviceName);
            MakeInfo("Tiempo máximo de espera: 30 segundos.", 224);
            SetStatus("⏳  Buscando dispositivo Bluetooth conectado...", AccentYellow);

            _pollTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _pollTimer.Tick += PollBluetooth;
            _pollTimer.Start();
        }

        private void PollBluetooth(object sender, EventArgs e)
        {
            _attempts++;
            if (_targetMac == null)
            {
                var form = FindForm() as MainForm;
                var dev  = form?.Session?.SelectedDevice;
                _targetMac = dev?.Address ?? "";
                if (_lblDeviceName != null && dev != null)
                    _lblDeviceName.Text = $"Dispositivo objetivo: {dev.Name}  ({dev.Address})";
            }

            bool connected = string.IsNullOrEmpty(_targetMac)
                ? BluetoothDetector.GetPairedDevices().Count > 0
                : BluetoothDetector.IsDeviceConnected(_targetMac);

            if (connected)
            {
                _pollTimer.Stop();
                AutoPass("Dispositivo Bluetooth conectado correctamente");
                return;
            }
            SetStatus($"⏳  Esperando conexión... ({_attempts}/{MAX_ATTEMPTS})", AccentYellow);
            if (_attempts >= MAX_ATTEMPTS)
            {
                _pollTimer.Stop();
                AutoFail("El dispositivo no se conectó en el tiempo límite");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _pollTimer?.Dispose();
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  2. PLAY / PAUSE  — detecta por cambio de estado del reproductor
    // ═══════════════════════════════════════════════════════════════════════════
    public class PlayPausePanel : TestPanel
    {
        private System.Windows.Forms.Timer _timeout;
        private bool _pauseDetected  = false;
        private bool _resumeDetected = false;
        private bool _done           = false;
        private Label _lblStep1, _lblStep2;

        public PlayPausePanel() : base(2, "Play / Pausa", "⏯", withPlayer: true)
        {
            MakeStep(1, "El reproductor está activo. Presione Play/Pausa en el audífono.", 14);
            MakeStep(2, "El audio debe PAUSAR — el reproductor lo mostrará.", 56);
            MakeStep(3, "Presione nuevamente — el audio debe REANUDAR.", 98);

            SetStatus("⏳  Presione Play/Pausa en el audífono para pausar el audio...", AccentYellow);

            // Detect via player state change — works regardless of HOW the headphone sends the signal
            Player.Audio.StateChanged += OnPlayerStateChanged;

            _timeout = new System.Windows.Forms.Timer { Interval = 25000 };
            _timeout.Tick += (s, e) =>
            {
                _timeout.Stop();
                if (!_done) AutoFail("No se detectó respuesta de Play/Pausa en el tiempo límite");
            };
            _timeout.Start();
        }

        private void OnPlayerStateChanged(AudioPlayer.PlayerState state)
        {
            if (_done) return;
            if (InvokeRequired) { Invoke(new Action(() => OnPlayerStateChanged(state))); return; }

            if (!_pauseDetected && state == AudioPlayer.PlayerState.Paused)
            {
                _pauseDetected = true;
                SetStatus("✔  Pausa detectada — presione nuevamente para reanudar...", AccentOrange);
                return;
            }

            if (_pauseDetected && !_resumeDetected && state == AudioPlayer.PlayerState.Playing)
            {
                _resumeDetected = true;
                _done           = true;
                _timeout.Stop();
                AutoPass("Play y Pausa detectados correctamente");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _done = true;
                if (Player?.Audio != null)
                    Player.Audio.StateChanged -= OnPlayerStateChanged;
                _timeout?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  3. PREVIOUS TRACK — detecta por cambio de pista hacia atrás
    // ═══════════════════════════════════════════════════════════════════════════
    public class PreviousTrackPanel : TestPanel
    {
        private System.Windows.Forms.Timer _timeout;
        private bool _done = false;
        private int  _startTrack;

        public PreviousTrackPanel() : base(3, "Canción Anterior ◀◀", "⏮", withPlayer: true)
        {
            MakeStep(1, "El reproductor está en la Pista 2 o 3.", 14);
            MakeStep(2, "Presione el botón de Pista Anterior en el audífono.", 56);
            MakeStep(3, "El reproductor mostrará el cambio de pista automáticamente.", 98);

            SetStatus("⏳  Esperando comando de Pista Anterior...", AccentYellow);

            // Move to track 2 so previous is detectable
            Player.Audio.NextTrack();
            _startTrack = Player.Audio.Track;

            Player.Audio.TrackChanged += OnTrackChanged;

            _timeout = new System.Windows.Forms.Timer { Interval = 20000 };
            _timeout.Tick += (s, e) =>
            {
                _timeout.Stop();
                if (!_done) AutoFail("No se detectó cambio de Pista Anterior");
            };
            _timeout.Start();
        }

        private void OnTrackChanged(int track)
        {
            if (_done) return;
            if (InvokeRequired) { Invoke(new Action(() => OnTrackChanged(track))); return; }

            // Any backwards track change counts
            bool wentBack = (track == (_startTrack + 2) % 3) || track < _startTrack;
            if (wentBack)
            {
                _done = true;
                _timeout.Stop();
                AutoPass($"Pista Anterior detectada — cambió a {Player.Audio.TrackName}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _done = true;
                if (Player?.Audio != null) Player.Audio.TrackChanged -= OnTrackChanged;
                _timeout?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  4. NEXT TRACK — detecta por cambio de pista hacia adelante
    // ═══════════════════════════════════════════════════════════════════════════
    public class NextTrackPanel : TestPanel
    {
        private System.Windows.Forms.Timer _timeout;
        private bool _done = false;
        private int  _startTrack;

        public NextTrackPanel() : base(4, "Canción Siguiente ▶▶", "⏭", withPlayer: true)
        {
            MakeStep(1, "El reproductor está en la Pista 1.", 14);
            MakeStep(2, "Presione el botón de Pista Siguiente en el audífono.", 56);
            MakeStep(3, "El reproductor mostrará el cambio de pista automáticamente.", 98);

            SetStatus("⏳  Esperando comando de Pista Siguiente...", AccentYellow);

            _startTrack = Player.Audio.Track;
            Player.Audio.TrackChanged += OnTrackChanged;

            _timeout = new System.Windows.Forms.Timer { Interval = 20000 };
            _timeout.Tick += (s, e) =>
            {
                _timeout.Stop();
                if (!_done) AutoFail("No se detectó cambio de Pista Siguiente");
            };
            _timeout.Start();
        }

        private void OnTrackChanged(int track)
        {
            if (_done) return;
            if (InvokeRequired) { Invoke(new Action(() => OnTrackChanged(track))); return; }

            bool wentForward = track == (_startTrack + 1) % 3;
            if (wentForward)
            {
                _done = true;
                _timeout.Stop();
                AutoPass($"Pista Siguiente detectada — cambió a {Player.Audio.TrackName}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _done = true;
                if (Player?.Audio != null) Player.Audio.TrackChanged -= OnTrackChanged;
                _timeout?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  5. VOLUME UP
    // ═══════════════════════════════════════════════════════════════════════════
    public class VolumeUpPanel : TestPanel
    {
        private VolumeMonitor _monitor;
        private System.Windows.Forms.Timer _timeout;
        private ProgressBar _volBar;
        private Label _volLabel;
        private Label _lblTarget;
        private float _startVolume;
        private float _targetVolume;
        private const float REQUIRED_DELTA = 20f;
        private bool _completed;

        public VolumeUpPanel() : base(5, "Subir Volumen (+)", "🔊")
        {
            MakeStep(1, "El sistema registra el volumen inicial automáticamente.", 14);
            MakeStep(2, "Presione y mantenga el botón (+) del audífono.", 56);
            MakeStep(3, "Suba el volumen al menos 20 puntos por encima del nivel inicial.", 98);

            _lblTarget = new Label
            {
                Text = "Registrando nivel inicial...",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = AccentYellow,
                AutoSize = false, Size = new Size(stepsPanel.Width - 40, 28),
                Location = new Point(20, 144), TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var lblBarTitle = new Label
            {
                Text = "NIVEL ACTUAL:", Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextMuted, AutoSize = false, Size = new Size(140, 24),
                Location = new Point(20, 182), TextAlign = ContentAlignment.MiddleLeft
            };
            _volBar = new ProgressBar
            {
                Minimum = 0, Maximum = 100, Value = 50,
                Size = new Size(stepsPanel.Width - 180, 24), Location = new Point(20, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _volLabel = new Label
            {
                Text = "50%", Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AccentYellow, AutoSize = false, Size = new Size(70, 24),
                Location = new Point(stepsPanel.Width - 150, 210),
                TextAlign = ContentAlignment.MiddleCenter, Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            stepsPanel.Controls.AddRange(new Control[] { _lblTarget, lblBarTitle, _volBar, _volLabel });
            stepsPanel.Resize += (s, e) =>
            {
                _volBar.Size  = new Size(stepsPanel.Width - 180, 24);
                _volLabel.Location = new Point(stepsPanel.Width - 150, 210);
                _lblTarget.Size = new Size(stepsPanel.Width - 40, 28);
            };

            _monitor = new VolumeMonitor();
            _startVolume  = _monitor.CurrentVolume;
            _targetVolume = Math.Min(_startVolume + REQUIRED_DELTA, 100f);
            _lblTarget.Text = $"Nivel inicial: {_startVolume:F0}%   →   Meta mínima: {_targetVolume:F0}%";
            UpdateBar(_startVolume);
            SetStatus($"⏳  Suba el volumen hasta {_targetVolume:F0}% o más...", AccentYellow);

            _monitor.VolumeChanged += OnVolumeChanged;

            _timeout = new System.Windows.Forms.Timer { Interval = 25000 };
            _timeout.Tick += (s, e) =>
            {
                _timeout.Stop();
                if (!_completed) AutoFail("No se alcanzó el nivel de volumen requerido");
            };
            _timeout.Start();
        }

        private void OnVolumeChanged(float vol)
        {
            if (_completed) return;
            if (InvokeRequired) { Invoke(new Action(() => OnVolumeChanged(vol))); return; }
            UpdateBar(vol);
            SetStatus($"⏳  Volumen: {vol:F0}%  →  Meta: {_targetVolume:F0}%", AccentOrange);
            if (vol >= _targetVolume)
            {
                _completed = true;
                _timeout.Stop();
                AutoPass($"Volumen subido correctamente a {vol:F0}%");
            }
        }

        private void UpdateBar(float vol)
        {
            _volBar.Value = (int)Math.Max(0, Math.Min(100, vol));
            _volLabel.Text = $"{vol:F0}%";
            _volLabel.ForeColor = vol >= _targetVolume ? AccentGreen : AccentYellow;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _monitor?.Dispose(); _timeout?.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  6. VOLUME DOWN
    // ═══════════════════════════════════════════════════════════════════════════
    public class VolumeDownPanel : TestPanel
    {
        private VolumeMonitor _monitor;
        private System.Windows.Forms.Timer _timeout;
        private ProgressBar _volBar;
        private Label _volLabel;
        private Label _lblTarget;
        private float _startVolume;
        private float _targetVolume;
        private const float REQUIRED_DELTA = 20f;
        private bool _completed;

        public VolumeDownPanel() : base(6, "Bajar Volumen (−)", "🔉")
        {
            MakeStep(1, "El sistema registra el volumen inicial automáticamente.", 14);
            MakeStep(2, "Presione y mantenga el botón (−) del audífono.", 56);
            MakeStep(3, "Baje el volumen al menos 20 puntos por debajo del nivel inicial.", 98);

            _lblTarget = new Label
            {
                Text = "Registrando nivel inicial...",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = AccentYellow,
                AutoSize = false, Size = new Size(stepsPanel.Width - 40, 28),
                Location = new Point(20, 144), TextAlign = ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            var lblBarTitle = new Label
            {
                Text = "NIVEL ACTUAL:", Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = TextMuted, AutoSize = false, Size = new Size(140, 24),
                Location = new Point(20, 182), TextAlign = ContentAlignment.MiddleLeft
            };
            _volBar = new ProgressBar
            {
                Minimum = 0, Maximum = 100, Value = 50,
                Size = new Size(stepsPanel.Width - 180, 24), Location = new Point(20, 210),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _volLabel = new Label
            {
                Text = "50%", Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = AccentYellow, AutoSize = false, Size = new Size(70, 24),
                Location = new Point(stepsPanel.Width - 150, 210),
                TextAlign = ContentAlignment.MiddleCenter, Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            stepsPanel.Controls.AddRange(new Control[] { _lblTarget, lblBarTitle, _volBar, _volLabel });
            stepsPanel.Resize += (s, e) =>
            {
                _volBar.Size  = new Size(stepsPanel.Width - 180, 24);
                _volLabel.Location = new Point(stepsPanel.Width - 150, 210);
                _lblTarget.Size = new Size(stepsPanel.Width - 40, 28);
            };

            _monitor = new VolumeMonitor();
            _startVolume  = _monitor.CurrentVolume;
            _targetVolume = Math.Max(_startVolume - REQUIRED_DELTA, 0f);
            _lblTarget.Text = $"Nivel inicial: {_startVolume:F0}%   →   Meta máxima: {_targetVolume:F0}%";
            UpdateBar(_startVolume);
            SetStatus($"⏳  Baje el volumen hasta {_targetVolume:F0}% o menos...", AccentYellow);

            _monitor.VolumeChanged += OnVolumeChanged;

            _timeout = new System.Windows.Forms.Timer { Interval = 25000 };
            _timeout.Tick += (s, e) =>
            {
                _timeout.Stop();
                if (!_completed) AutoFail("No se alcanzó el nivel de volumen requerido");
            };
            _timeout.Start();
        }

        private void OnVolumeChanged(float vol)
        {
            if (_completed) return;
            if (InvokeRequired) { Invoke(new Action(() => OnVolumeChanged(vol))); return; }
            UpdateBar(vol);
            SetStatus($"⏳  Volumen: {vol:F0}%  →  Meta: {_targetVolume:F0}%", AccentOrange);
            if (vol <= _targetVolume)
            {
                _completed = true;
                _timeout.Stop();
                AutoPass($"Volumen bajado correctamente a {vol:F0}%");
            }
        }

        private void UpdateBar(float vol)
        {
            _volBar.Value = (int)Math.Max(0, Math.Min(100, vol));
            _volLabel.Text = $"{vol:F0}%";
            _volLabel.ForeColor = vol <= _targetVolume ? AccentGreen : AccentYellow;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _monitor?.Dispose(); _timeout?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
