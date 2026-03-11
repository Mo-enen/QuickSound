using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Raylib_cs;

public static class Flow {

	public static int WindowWidth = 1000;
	public static int WindowHeight = 1000;
	public static int WindowX = -1;
	public static int WindowY = -1;
	public static bool RequireMaximize = false;
	public static Font DefaultFont;
	public static string SavingFolder;
	public static string DevName;

	public static void Run (Action start, Action update, Action quit, string devName = "RayFlow") {
		DevName = devName;
		Flow.Init();
		start?.Invoke();
		Flow.Loop(update);
		Flow.Quit();
		quit?.Invoke();
	}

	private static void Init () {
#if DEBUG
		Console.Clear();
#endif
		// Load Config
		SavingFolder = CombinePaths(Environment.GetFolderPath(
			Environment.SpecialFolder.LocalApplicationData),
			DevName,
			typeof(Flow).Assembly.GetName().Name
		);
		string path = CombinePaths(SavingFolder, "Config.txt");
		CreateFolder(SavingFolder);
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
		Raylib.SetTraceLogLevel(TraceLogLevel.Warning);
		Raylib.SetWindowState(ConfigFlags.AlwaysRunWindow | ConfigFlags.ResizableWindow);
		Raylib.InitWindow(WindowWidth, WindowHeight, "");
		Raylib.SetTargetFPS(48);
		Raylib.InitAudioDevice();
		Raylib.SetExitKey(Raylib_cs.KeyboardKey.Null);
		Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
#if DEBUG
		DevUtil.Debug_OnAppStart();
#endif

		// Setup Window
		var assembly = Assembly.GetExecutingAssembly();
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
		if (Flow.RequireMaximize) {
			Raylib.MaximizeWindow();
		}
		Raylib.SetWindowPosition(WindowX, WindowY);
		Raylib.SetWindowSize(WindowWidth, WindowHeight);
		Raylib.SetWindowTitle(GetDisplayName(assembly.GetName().Name));
		Raylib.SetWindowFocused();

		// Resource
		using (var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Icon.png")) {
			if (stream != null) {
				using var reader = new BinaryReader(stream);
				var pngBytes = reader.ReadBytes((int)stream.Length);
				var img = Raylib.LoadImageFromMemory(".png", pngBytes);
				Raylib.SetWindowIcon(img);
				Raylib.UnloadImage(img);
			}
		}
		DefaultFont = Raylib.GetFontDefault();

	}

	private static void Loop (Action update) {
		while (!Raylib.WindowShouldClose()) {
			if (!Raylib.IsWindowMinimized()) {
				Raylib.BeginDrawing();
				update?.Invoke();
			}
			Raylib.EndDrawing();
		}
	}

	private static void Quit () {
		Raylib.CloseAudioDevice();
		string path = CombinePaths(SavingFolder, "Config.txt");
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
#if DEBUG
		DevUtil.Debug_OnAppEnd();
#endif
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

}
