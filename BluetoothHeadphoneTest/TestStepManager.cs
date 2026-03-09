using System;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    public class TestStepManager
    {
        public const int TotalTests = 6;

        private readonly MainForm form;
        private TestPanel currentPanel;

        public TestStepManager(MainForm form)
        {
            this.form = form;
        }

        public void Initialize()
        {
            form.Session.Reset();
            // Re-register hotkeys in case they were lost (e.g. after another app grabbed them)
            AppCommandRouter.Unregister();
            AppCommandRouter.Register(form.Handle);
            ShowTest(0);
        }

        public void ShowTest(int index)
        {
            form.Session.CurrentTestIndex = index;
            form.UpdateOperatorPanel();

            currentPanel?.Dispose();
            form.panelTestArea.Controls.Clear();

            if (index >= TotalTests)
            {
                ShowSummary();
                return;
            }

            TestPanel panel;
            switch (index)
            {
                case 0: panel = new BluetoothConnectionPanel(); break;
                case 1: panel = new PlayPausePanel(); break;
                case 2: panel = new PreviousTrackPanel(); break;
                case 3: panel = new NextTrackPanel(); break;
                case 4: panel = new VolumeUpPanel(); break;
                case 5: panel = new VolumeDownPanel(); break;
                default: return;
            }

            // Wire auto-detection result
            panel.TestCompleted += (passed) => OnTestAutoCompleted(index, passed);

            currentPanel = panel;
            currentPanel.Dock = DockStyle.Fill;
            form.panelTestArea.Controls.Add(currentPanel);

            // Buttons hidden during auto-test, only Reset is visible
            form.BtnPass.Visible   = false;
            form.BtnFail.Visible   = false;
            form.LabelStatus.Text  = $"Prueba activa: {form.Session.Records[index].Name}";
        }

        private void OnTestAutoCompleted(int idx, bool passed)
        {
            if (idx != form.Session.CurrentTestIndex) return;

            form.Session.Records[idx].Result    = passed ? TestResult.Pass : TestResult.Fail;
            form.Session.Records[idx].Timestamp = DateTime.Now;
            form.LabelStatus.Text = passed ? "✔ Aprobado — continuando..." : "✘ Fallido — continuando...";

            var timer = new System.Windows.Forms.Timer { Interval = 800 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                ShowTest(idx + 1);
            };
            timer.Start();
        }

        public void OnReset()
        {
            var confirm = MessageBox.Show(
                "¿Desea reiniciar todas las pruebas?\nSe generará un nuevo folio.",
                "Confirmar reinicio",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes) Initialize();
        }

        private void ShowSummary()
        {
            form.BtnPass.Visible  = false;
            form.BtnFail.Visible  = false;
            form.LabelStatus.Text = "Secuencia completa.";

            var summaryPanel = new SummaryPanel(form.Session);
            summaryPanel.OnRestart += Initialize;
            summaryPanel.Dock = DockStyle.Fill;
            form.panelTestArea.Controls.Add(summaryPanel);
        }
    }
}
