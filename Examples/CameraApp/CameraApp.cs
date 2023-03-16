using System;
using System.Windows.Forms;

namespace CameraApp
{
    internal static class CameraApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CameraForm());
        }
    }
}
