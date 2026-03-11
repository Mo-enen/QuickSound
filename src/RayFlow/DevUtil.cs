#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Raylib_cs;

public static partial class DevUtil {

	private const int SW_MAXIMIZE = 3;
	private const int SW_MINIMIZE = 6;
	private const uint SWP_NOSIZE = 0x0001;
	private const uint SWP_NOZORDER = 0x0004;

	public static void Debug_OnAppStart () {
		int mIndex = -1;
		int monitorCount = Raylib.GetMonitorCount();
		if (monitorCount > 1) {
			mIndex = 1;
			for (int i = 1; i < monitorCount; i++) {
				var pos = Raylib.GetMonitorPosition(i);
				if (pos.X < 0) {
					mIndex = i;
					break;
				}
			}
		}
		Process.GetProcessesByName("WindowsTerminal").ToList().ForEach(item => {
			// Move Terminal Window to Left Monitor
			if (mIndex >= 0) {
				var mPos = Raylib.GetMonitorPosition(mIndex);
				SetWindowPos(
					item.MainWindowHandle, IntPtr.Zero, (int)mPos.X, (int)mPos.Y,
					Raylib.GetMonitorWidth(mIndex), Raylib.GetMonitorHeight(mIndex),
					SWP_NOZORDER
				);
			}
			// Maximize
			ShowWindow(item.MainWindowHandle, SW_MAXIMIZE);
		});
	}

	public static void Debug_OnAppEnd () {
		// Minimize Terminal Window on Quit
		Process.GetProcessesByName(
			"WindowsTerminal"
		).ToList().ForEach(item => {
			ShowWindow(item.MainWindowHandle, SW_MINIMIZE);
		});
	}

	// For Minimize Window
	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool ShowWindow (IntPtr hWnd, int nCmdShow);

	// Set Window Pos
	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

}
#endif
