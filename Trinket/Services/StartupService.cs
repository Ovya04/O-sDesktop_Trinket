using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Trinket.Services;

/// <summary>
/// Manages the "Start with Windows" preference via a shortcut (.lnk) file
/// in the user's Startup folder - not the registry Run key. A visible,
/// user-removable shortcut is more transparent (and less likely to trigger
/// antivirus suspicion) than a hidden registry entry, for the same effect.
/// Fully opt-in: nothing here runs unless the user explicitly enables it.
/// </summary>
public static class StartupService
{
    private const string ShortcutName = "Trinket.lnk";

    private static string StartupFolderPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.Startup);

    private static string ShortcutFilePath =>
        Path.Combine(StartupFolderPath, ShortcutName);

    public static bool IsEnabled => File.Exists(ShortcutFilePath);

    public static void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            CreateShortcut();
        }
        else
        {
            RemoveShortcut();
        }
    }

    private static void CreateShortcut()
    {
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
        {
            return;
        }

        var shellLinkType = Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046"))!;
        var shellLink = (IShellLink)Activator.CreateInstance(shellLinkType)!;

        shellLink.SetPath(exePath);
        shellLink.SetWorkingDirectory(Path.GetDirectoryName(exePath) ?? string.Empty);
        shellLink.SetDescription("Trinket - a small charm that hangs from your desktop");

        var persistFile = (IPersistFile)shellLink;
        persistFile.Save(ShortcutFilePath, false);
    }

    private static void RemoveShortcut()
    {
        if (File.Exists(ShortcutFilePath))
        {
            File.Delete(ShortcutFilePath);
        }
    }

    // --- COM interop for shortcut (.lnk) creation ---

    [ComImport]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile, int cchMaxPath, IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        int IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, int dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
    }
}