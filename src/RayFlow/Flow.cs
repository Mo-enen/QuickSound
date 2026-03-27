using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace RayFlow;

public static class Flow {

	// Data
	private static int WindowWidth = 1000;
	private static int WindowHeight = 1000;
	private static int WindowX = -1;
	private static int WindowY = -1;
	private static bool RequireMaximize = false;
	private static ImFontPtr MainFontPtr;
	public static Music Music;

	// API
	public static void Run () {
		if (GetWindow() is not FlowWindow window) return;
		Init(window);
		Loop(window);
		Quit(window);
	}

	// MSG
	private static FlowWindow GetWindow () {
		if (Assembly.GetEntryAssembly() is not Assembly entry) return null;
		var windowType = typeof(FlowWindow);
		foreach (var type in entry.ExportedTypes) {
			if (type.IsSubclassOf(windowType)) {
				if (System.Activator.CreateInstance(type) is not FlowWindow fWindow) continue;
				return fWindow;
			}
		}
		return null;
	}

	private static void Init (FlowWindow window) {
#if DEBUG
		Console.Clear();
#endif
		// Load Config
		string savingFolder = window.SavingFolder;
		string path = CombinePaths(savingFolder, "Config.txt");
		CreateFolder(savingFolder);
		foreach (string line in ForAllLinesInFile(path)) {
			int cIndex = line.IndexOf(':');
			if (cIndex < 0 || cIndex + 1 >= line.Length) continue;
			if (line.StartsWith("WindowX:") && int.TryParse(line[(cIndex + 1)..], out int x)) {
				WindowX = x;
				continue;
			}
			if (line.StartsWith("WindowY:") && int.TryParse(line[(cIndex + 1)..], out int y)) {
				WindowY = y;
				continue;
			}
			if (line.StartsWith("WindowWidth:") && int.TryParse(line[(cIndex + 1)..], out int width)) {
				WindowWidth = width;
				continue;
			}
			if (line.StartsWith("WindowHeight:") && int.TryParse(line[(cIndex + 1)..], out int height)) {
				WindowHeight = height;
				continue;
			}
			if (line.StartsWith("Maximized:") && bool.TryParse(line[(cIndex + 1)..], out bool maximized)) {
				RequireMaximize = maximized;
				continue;
			}
		}

		// Init Raylib
#if DEBUG
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
#else
		Raylib.SetTraceLogLevel(TraceLogLevel.None);
#endif
		Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
		Raylib.SetWindowState(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
		Raylib.InitWindow(WindowWidth, WindowHeight, "");
		Raylib.SetTargetFPS(30);
		Raylib.InitAudioDevice();
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);
		Raylib.DisableEventWaiting();
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
#if DEBUG
		DevUtil.Debug_OnAppStart();
#endif

		// Setup Window
		var assembly = Assembly.GetExecutingAssembly();
		string assemblyName = assembly.GetName().Name;
		int monitor = Raylib.GetCurrentMonitor();
		int monitorW = Raylib.GetMonitorWidth(monitor);
		int monitorH = Raylib.GetMonitorHeight(monitor);
		WindowWidth = Math.Clamp(WindowWidth, 200, 4000);
		WindowHeight = Math.Clamp(WindowHeight, 200, 4000);
		if (WindowX < 0 || WindowY < 0) {
			WindowX = monitorW / 2 - WindowWidth / 2;
			WindowY = monitorH / 2 - WindowHeight / 2;
		}
		WindowX = Math.Clamp(WindowX, 0, Math.Max(1, monitorW - WindowWidth));
		WindowY = Math.Clamp(WindowY, 0, Math.Max(1, monitorH - WindowHeight));
		if (RequireMaximize) {
			Raylib.MaximizeWindow();
		}
		Raylib.SetWindowPosition(WindowX, WindowY);
		Raylib.SetWindowSize(WindowWidth, WindowHeight);
		Raylib.SetWindowTitle(GetDisplayName(assemblyName));
		Raylib.SetWindowFocused();

		// Resource
		using (var stream = assembly.GetManifestResourceStream($"{assemblyName}.res.Icon.png")) {
			if (stream != null) {
				using var reader = new BinaryReader(stream);
				var pngBytes = reader.ReadBytes((int)stream.Length);
				var img = Raylib.LoadImageFromMemory(".png", pngBytes);
				Raylib.SetWindowIcon(img);
				Raylib.UnloadImage(img);
			}
		}

		// Embedded Font
		byte[] FontBytes = null;
		using (var stream = assembly.GetManifestResourceStream($"{assemblyName}.res.Font.ttf")) {
			using var reader = new BinaryReader(stream);
			FontBytes = reader.ReadBytes((int)stream.Length);
		}

		// GUI Setup
		rlImGui.Setup(darkTheme: true, enableDocking: false);

		// Init Font
		var ImGuiContext = ImGui.CreateContext();
		ImGui.SetCurrentContext(ImGuiContext);

		unsafe {

			var nativeIO = ImGuiNative.igGetIO();
			nativeIO->WantSaveIniSettings = 0;
			nativeIO->IniFilename = null;
			nativeIO->LogFilename = null;
			nativeIO->ConfigDebugIniSettings = 0;
			var imguiIO = ImGuiNative.ImGuiIO_ImGuiIO();
			imguiIO->WantSaveIniSettings = 0;
			imguiIO->IniFilename = null;
			imguiIO->LogFilename = null;
			imguiIO->ConfigDebugIniSettings = 0;

			var io = ImGui.GetIO();
			fixed (byte* p = FontBytes) {
				MainFontPtr = io.Fonts.AddFontFromMemoryTTF(
					(nint)p, FontBytes.Length, 48, default, io.Fonts.GetGlyphRangesDefault()
				);
			}
			FontBytes = null;
			io.Fonts.GetTexDataAsRGBA32(out byte* out_pixels, out int out_width, out int out_height, out int _);
			Image image = default;
			image.Data = out_pixels;
			image.Width = out_width;
			image.Height = out_height;
			image.Mipmaps = 1;
			image.Format = PixelFormat.UncompressedR8G8B8A8;
			var FontTexture = Raylib.LoadTextureFromImage(image);
			io.Fonts.SetTexID(new IntPtr(FontTexture.Id));
		}

		// Final
		window.Start();
		DeleteGuiIniFile();
		GC.Collect();

	}

	private static void Loop (FlowWindow window) {
		while (!Raylib.WindowShouldClose()) {
			if (!Raylib.IsWindowMinimized()) {
				// Begin Draw
				Raylib.UpdateMusicStream(Music);

				Raylib.BeginDrawing();
				if (Raylib.IsFileDropped()) {
					window.OnFileDropped(Raylib.GetDroppedFiles());
				}
				Raylib.ClearBackground(window.BackgroundColor);
				rlImGui.Begin();
				ImGui.Begin(
					"Main",
					ImGuiWindowFlags.NoCollapse |
					ImGuiWindowFlags.NoDecoration |
					ImGuiWindowFlags.NoDocking |
					ImGuiWindowFlags.NoMove |
					ImGuiWindowFlags.NoBackground |
					ImGuiWindowFlags.NoSavedSettings |
					ImGuiWindowFlags.NoResize |
					ImGuiWindowFlags.NoScrollbar |
					ImGuiWindowFlags.NoScrollWithMouse
				);
				GUI.Begin();
				try {
					WindowWidth = Raylib.GetScreenWidth();
					WindowHeight = Raylib.GetScreenHeight();
					using var _ = new FontScope(MainFontPtr);
					window.Width = WindowWidth - window.WindowPadding * 2;
					window.Height = WindowHeight - window.WindowPadding * 2;
					ImGui.SetWindowPos(new(window.WindowPadding, window.WindowPadding));
					ImGui.SetWindowSize(new(window.Width, window.Height));
					ImGui.SetWindowFontScale(1f);
					GUI.RequireCursor = MouseCursor.Default;
					window.Update();
					Raylib.SetMouseCursor(GUI.RequireCursor);
				} catch (Exception ex) { Debug.LogError(ex); }
				ImGui.End();
				rlImGui.End();
			}
			Raylib.EndDrawing();
		}
	}

	private static void Quit (FlowWindow window) {
		if (Raylib.IsMusicValid(Music)) {
			Raylib.UnloadMusicStream(Music);
		}
		Raylib.CloseAudioDevice();
		string path = CombinePaths(window.SavingFolder, "Config.txt");
		var builder = new StringBuilder();
		if (!Raylib.IsWindowMinimized()) {
			builder.AppendLine($"WindowX:{Raylib.GetWindowPosition().X}");
			builder.AppendLine($"WindowY:{Raylib.GetWindowPosition().Y}");
			builder.AppendLine($"WindowWidth:{Raylib.GetScreenWidth()}");
			builder.AppendLine($"WindowHeight:{Raylib.GetScreenHeight()}");
		} else {
			builder.AppendLine($"WindowX:{Raylib.GetScreenWidth() / 4}");
			builder.AppendLine($"WindowY:{Raylib.GetScreenHeight() / 4}");
			builder.AppendLine($"WindowWidth:{WindowWidth}");
			builder.AppendLine($"WindowHeight:{WindowHeight}");
		}
		builder.AppendLine($"Maximized:{Raylib.IsWindowMaximized()}");
		TextToFile(builder.ToString(), path);
		window.Quit();
#if DEBUG
		DevUtil.Debug_OnAppEnd();
#endif
		rlImGui.Shutdown();
		Raylib.CloseWindow();
		DeleteGuiIniFile();
	}

	// UTL
	private static string CombinePaths (string path1, string path2) => Path.Combine(path1, path2);
	private static string CombinePaths (string path1, string path2, string path3) => Path.Combine(path1, path2, path3);

	private static void CreateFolder (string path) {
		if (string.IsNullOrEmpty(path) || FolderExists(path)) return;
		string pPath = GetParentPath(path);
		if (!FolderExists(pPath)) {
			CreateFolder(pPath);
		}
		Directory.CreateDirectory(path);
	}

	private static bool FolderExists (string path) => Directory.Exists(path);

	private static string GetParentPath (string path) {
		if (string.IsNullOrEmpty(path)) return "";
		var info = Directory.GetParent(path);
		return info != null ? info.FullName : "";
	}

	private static IEnumerable<string> ForAllLinesInFile (string path) {
		if (!FileExists(path)) yield break;
		using StreamReader sr = new(path, Encoding.UTF8);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}

	private static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);

	private static string GetDisplayName (string name) {

		// Add " " Space Between "a Aa"
		for (int i = 0; i < name.Length - 1; i++) {
			char a = name[i];
			char b = name[i + 1];
			if (
				char.IsLetter(a) &&
				(char.IsLetter(b) || char.IsNumber(b)) &&
				!char.IsUpper(a) &&
				(char.IsUpper(b) || char.IsNumber(b))
			) {
				name = name.Insert(i + 1, " ");
				i++;
			}
		}

		return name;
	}

	private static void TextToFile (string data, string path, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, Encoding.UTF8);
		sw.Write(data);
		fs.Flush();
		sw.Close();
		fs.Close();
	}

	private static void DeleteGuiIniFile () {
		try {
			string iniPath = "imgui.ini";
			if (FileExists(iniPath)) {
				File.Delete(iniPath);
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
	}

}
