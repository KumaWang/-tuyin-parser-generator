using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace test_bench
{
    class TestHelper
    {
        [DllImport("shell32.dll")]
        public static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);

        public static void OpenImage(string imagePath)
        {
            //Process[] processes = Process.GetProcessesByName("mspaint");
            //foreach (Process cp in processes)
            //    if (cp.MainWindowTitle.Contains(Path.GetFileName(imagePath))) //检查进程名称是否为 "X - 画图"
            //        cp.Kill();

            var exePathReturnValue = new StringBuilder();
            FindExecutable(Path.GetFileName(imagePath), Path.GetDirectoryName(imagePath), exePathReturnValue);
            var exePath = exePathReturnValue.ToString();
            var arguments = "\"" + imagePath + "\"";

            // Handle cases where the default application is photoviewer.dll.
            if (Path.GetFileName(exePath).Equals("photoviewer.dll", StringComparison.InvariantCultureIgnoreCase))
            {
                arguments = "\"" + exePath + "\", ImageView_Fullscreen " + imagePath;
                exePath = "rundll32";
            }

            var process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = arguments;

            process.Start();
        }
    }
}
