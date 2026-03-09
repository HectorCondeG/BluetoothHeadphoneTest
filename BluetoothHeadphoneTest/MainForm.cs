using System;
using System.Drawing;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    public partial class MainForm : Form
    {
        private TestSession _session;
        public TestStepManager stepManager;

        // Hook global activo durante toda la sesión
        private GlobalKeyHook _globalHook;

        public MainForm()
        {
            InitializeComponent();
            _session    = new TestSession();
            stepManager = new TestStepManager(this);
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Arrancar hook global — canal secundario (teclado físico)
            _globalHook = new GlobalKeyHook();
            stepManager.Initialize();
            UpdateOperatorPanel();
        }

        /// <summary>
        /// Canal principal para audífonos BT: WM_APPCOMMAND (AVRCP).
        /// Se llama para TODOS los mensajes de la ventana.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            AppCommandRouter.ProcessMessage(ref m);
            base.WndProc(ref m);
        }

        public void UpdateOperatorPanel()
        {
            var devName = _session.SelectedDevice != null
                ? $"  |  {_session.SelectedDevice.Name}" : "";
            labelFolio.Text    = $"FOLIO: {_session.Folio}{devName}";
            labelDateTime.Text = DateTime.Now.ToString("dd/MMM/yyyy  HH:mm");
            labelProgress.Text =
                $"Prueba {_session.CurrentTestIndex + 1} de {TestStepManager.TotalTests}";
            progressBarMain.Value = Math.Min(
                _session.CurrentTestIndex * 100 / TestStepManager.TotalTests, 100);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            AppCommandRouter.Register(this.Handle);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            AppCommandRouter.Unregister();
            _globalHook?.Dispose();
            base.OnFormClosed(e);
        }

        public void RefreshDateTime() =>
            labelDateTime.Text = DateTime.Now.ToString("dd/MMM/yyyy  HH:mm");

        public TestSession Session => _session;
    }
}
