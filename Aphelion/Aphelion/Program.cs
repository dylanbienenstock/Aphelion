#define ERRORSBREAK

using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Aphelion
{
#if WINDOWS || XBOX
    public static class Program
    {
        [STAThread]
        static void Main()
        {
#if !ERRORSBREAK
            try
            {
#endif
                int? registryKey1 = (int?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\XNA\Framework\v4.0", "Installed", 0);
                int? registryKey2 = (int?)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\XNA\Framework\v4.0", "Installed", 0);

                if (!(registryKey1 == 1 || registryKey2 == 1))
                {
                    Process.Start("msiexec.exe", "/i xnafx40_redist.msi /passive").WaitForExit();
                }

                using (Aphelion game = new Aphelion())
                {
#if !ERRORSBREAK
                    try
                    {
#endif
                        game.Run();
#if !ERRORSBREAK
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex);
                    }
#endif
                }
#if !ERRORSBREAK
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
#endif
        }

        static void HandleException(Exception ex)
        {
            StackTrace trace = new StackTrace(ex, true);
            StackFrame frame = trace.GetFrame(0);

            MessageBox.Show(frame.GetFileName() + " @ Line " + frame.GetFileLineNumber() + "\n" + ex.Message, "Aphelion - Error");
            Clipboard.SetText(ex.ToString());
        }
    }
#endif
}

