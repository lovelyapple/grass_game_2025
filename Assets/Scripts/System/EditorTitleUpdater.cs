using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR_WIN
[InitializeOnLoad]
public class EditorTitleUpdater
{
    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool IsWindow(IntPtr hWnd);
    }

    static string ProjectRootPath = "";
    static IntPtr WindowHandle = IntPtr.Zero;
    static EditorTitleUpdater()
    {
        var path = Application.dataPath;
        ProjectRootPath = path.Replace("/Assets", "");
        EditorApplication.update += UpdateCaption;
    }
    static void UpdateCaption()
    {
        if (!User32.IsWindow(WindowHandle))
        {
            WindowHandle = GetSelfWindowHandle();
            if (WindowHandle == IntPtr.Zero)
                return;
        }

        var length = User32.GetWindowTextLength(WindowHandle);
        var sb = new StringBuilder(length + 1);
        User32.GetWindowText(WindowHandle, sb, sb.Capacity);
        var title = sb.ToString();

        if (!title.Contains(ProjectRootPath))
        {
            User32.SetWindowText(WindowHandle, $"{title} - [{ProjectRootPath}]");
        }
    }

    public static IntPtr GetSelfWindowHandle()
    {
        Process currentProcess = Process.GetCurrentProcess();
        return currentProcess.MainWindowHandle;
    }
}
#endif