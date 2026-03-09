using System;
using System.Windows.Forms;

namespace BluetoothHeadphoneTest
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Mostrar selección de dispositivo antes de iniciar pruebas
            using var selectForm = new DeviceSelectForm();
            if (selectForm.ShowDialog() != DialogResult.OK)
                return;  // Operador cerró sin seleccionar

            var mainForm = new MainForm();
            mainForm.Session.SelectedDevice = selectForm.SelectedDevice;
            Application.Run(mainForm);
        }
    }
}
