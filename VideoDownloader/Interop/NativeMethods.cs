using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VideoDownloader.Interop
{
    /// <summary>
    /// Centralized P/Invoke declarations for Windows kernel32 APIs.
    /// Previously duplicated in MainForm.cs and YtDlpService.cs.
    /// </summary>
    internal static class NativeMethods
    {
        public const uint ThreadSuspendResume = 0x0002;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Suspends all threads of the given process.
        /// </summary>
        public static void PauseProcess(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                foreach (ProcessThread thread in process.Threads)
                {
                    var hThread = OpenThread(ThreadSuspendResume, false, (uint)thread.Id);
                    if (hThread != IntPtr.Zero)
                    {
                        SuspendThread(hThread);
                        CloseHandle(hThread);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Resumes all threads of the given process.
        /// </summary>
        public static void ResumeProcess(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                foreach (ProcessThread thread in process.Threads)
                {
                    var hThread = OpenThread(ThreadSuspendResume, false, (uint)thread.Id);
                    if (hThread != IntPtr.Zero)
                    {
                        ResumeThread(hThread);
                        CloseHandle(hThread);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Force-kills a process and its entire child process tree.
        /// </summary>
        public static void KillProcessTree(int pid)
        {
            if (pid == 0) return;
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/T /F /PID {pid}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                })?.WaitForExit();
            }
            catch { }
        }
    }
}
