using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using RayFlow;
using ImGuiNET;
using rlImGui_cs;
using Wave = QuickSound.Wave;
using System.IO;

namespace QuickSound;


public class Window : FlowWindow {




	#region --- VAR ---


	// Api
	public override string DeveloperName => "Moenen";
	public override int WindowPadding => 96;

	// Data
	private readonly Searcher Searcher = new();
	private readonly HashSet<int> PlayedPathIDs = [];
	private string SearchingText = "";
	private bool RootInputActive = false;
	private bool ExportInputActive = false;
	private bool SearchingInputActive = false;
	private bool RequireSetScroll = false;
	private bool Dragged = false;
	private bool LastExportSuccess;
	private int SelectingIndex = -1;
	private int DraggingWaveEdgeIndex = 0;
	private int MusicPathID;
	private float DraggingEdgeOffsetX;
	private float LastExportedTime = -100f;
	private float ContentScrollY = 0f;
	private string ResultCountText = "";
	private int PrevResultCount = -1;

	// Setting
	private string AudioRootPath = "";
	private string ExportPath = "";


	#endregion




	#region --- MSG ---


	public override void Start () {
		LoadSettingFromFile();
		Wave.WaveCacheRoot = Util.CombinePaths(SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
		Searcher.ImportAsync(AudioRootPath, SavingFolder, forceImport: false);
		SelectingIndex = -1;
	}


	public override void Update () {
		Update_Toolbar();
		Update_Content();
		Update_Hotkey();
	}


	private void Update_Toolbar () {

		const float BTN_W = 256;
		const float GAP = 20;
		if (string.IsNullOrWhiteSpace(ExportPath)) {
			ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		// =========== First Row ===========

		// Root Label
		const string ROOT_LABEL = "Audio Folder:";
		float labelW = ImGui.CalcTextSize(ROOT_LABEL).X + 24;
		GUI.Label(ROOT_LABEL, 0, GuiColor.DarkGrey);

		// Root Input
		ImGui.SameLine(labelW);
		bool rootActive = GUI.Input("##Root", ref AudioRootPath, width: -BTN_W - GAP, bodyColor: GuiColor.DarkGrey);
		if (RootInputActive && !rootActive) {
			AudioRootPath = AudioRootPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			Searcher.ImportAsync(AudioRootPath, SavingFolder, forceImport: false);
			SelectingIndex = -1;
			ContentScrollY = 0f;
			RequireSetScroll = true;
			StopMusic();
		}
		RootInputActive = rootActive;

		// Import Button
		ImGui.SameLine();
		ImGui.Spacing();
		ImGui.SameLine();
		bool rootPathChanged = !string.IsNullOrEmpty(AudioRootPath) && !Searcher.CheckAudioRoot(AudioRootPath);
		if (GUI.Button(
			rootPathChanged ? "Import" : "Reimport",
			width: BTN_W,
			bodyColor: rootPathChanged ? (GUI.Time % 0.8f < 0.4f ? GuiColor.Green : GuiColor.AzureGreen) : GuiColor.DarkGrey,
			enable: !string.IsNullOrEmpty(AudioRootPath)
		)) {
			AudioRootPath = AudioRootPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			Searcher.ImportAsync(AudioRootPath, SavingFolder, forceImport: true);
			SelectingIndex = -1;
			StopMusic();
		}
		ImGui.Dummy(new(0, 8));



		// =========== Second Row ===========
		if (!string.IsNullOrEmpty(AudioRootPath) && Searcher.Imported && Searcher.ImportPathCount > 0) {
			// Export Label
			GUI.Label("Export To:", 0, GuiColor.DarkGrey);

			// Export Input
			ImGui.SameLine(labelW);
			bool exportActive = GUI.Input(
				"##Export",
				ref ExportPath,
				width: -BTN_W - BTN_W - GAP - GAP,
				bodyColor: GuiColor.DarkGrey
			);
			if (ExportInputActive && !exportActive) {
				ExportPath = ExportPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}
			ExportInputActive = exportActive;

			// Export Button
			ImGui.SameLine();
			ImGui.Spacing();
			ImGui.SameLine();
			if (GUI.Button(
				"Open",
				width: BTN_W,
				bodyColor: GuiColor.DarkGrey
			)) {
				Raylib.OpenURL(ExportPath);
			}

			// Export Button
			ImGui.SameLine();
			ImGui.Spacing();
			ImGui.SameLine();
			bool expEnable = !string.IsNullOrEmpty(ExportPath) && SelectingIndex >= 0;
			var exColor = expEnable ? GuiColor.DarkGreen : GuiColor.DarkGrey;
			bool justExpoted = GUI.Time < LastExportedTime + 2f;
			if (justExpoted) {
				if (LastExportSuccess) {
					exColor = GUI.Time % 0.4f < 0.2f ? exColor : GuiColor.Green;
				} else {
					exColor = GUI.Time % 0.4f < 0.2f ? GuiColor.Red : GuiColor.DarkRed;
				}
			}
			if (GUI.Button(
				justExpoted ? (LastExportSuccess ? "Exported!!" : "Export Fail") : "Export",
				width: BTN_W,
				bodyColor: exColor,
				enable: expEnable
			)) {
				StopMusic();
				ExportSelectingAudio();
			}

		}


		// =========== Final ===========
		ImGui.Dummy(new(0, 64));

	}


	private void Update_Content () {

		// Search Bar
		if (!string.IsNullOrEmpty(AudioRootPath) && Searcher.Imported && Searcher.ImportPathCount > 0) {

			// Search Input
			float winWidth = ImGui.GetWindowWidth();
			float width = winWidth * 0.618f;

			ImGui.Dummy(new Vector2((winWidth - width) / 2f, 1));
			ImGui.SameLine();
			bool active = GUI.Input("##Search", ref SearchingText, width: width);
			if (SearchingInputActive && !active && SearchingText != Searcher.SearchedText) {
				StopMusic();
				Searcher.PerformSearch(SearchingText);
				SelectingIndex = -1;
				ContentScrollY = 0f;
				RequireSetScroll = true;
			}
			SearchingInputActive = active;
			DrawSearchIcon();
			ImGui.Dummy(new Vector2(0, 24));

		}

		// Import Check
		if (Searcher.Importing) {
			float winWidth = ImGui.GetWindowWidth();
			ImGui.Spacing();
			GUI.Label($"Importing... ({Searcher.ImportPathCount})", winWidth, GuiColor.White);
			GUI.Label(Searcher.ImportingMsg, winWidth, GuiColor.Grey);
		} else if (
			!Searcher.Imported ||
			string.IsNullOrEmpty(Searcher.AudioRootPath) ||
			Searcher.ImportPathCount == 0
		) {
			float winWidth = ImGui.GetWindowWidth();
			ImGui.Spacing();
			GUI.Label("Drag and drop a folder here to import all audio files inside", winWidth, GuiColor.Grey);
			return;
		}

		if (!Searcher.Imported) return;
		if (string.IsNullOrEmpty(Searcher.AudioRootPath)) return;

		// No Result Check
		if (Searcher.SearchResults.Count == 0) {
			float winWidth = ImGui.GetWindowWidth();
			ImGui.Spacing();
			if (Searcher.Searching) {
				GUI.Label("Searching...", winWidth);
			} else {
				GUI.Label("(No Result)", winWidth);
			}
			return;
		}

		// List Content
		const float BASE_W = 560;
		const float NAME_W = 860;
		const float ITEM_H = 64;
		const float ITEM_PADDING = 24;
		const float WAVE_BORD = 4;
		const float SCROLL_BAR_W = 96;
		bool anyClick = false;
		if (!GUI.MouseLeftHolding) {
			DraggingWaveEdgeIndex = 0;
		}

		// Right Click
		if (GUI.MouseRightDown) {
			StopMusic();
		}

		// Top Bar
		using (new ChildScope(0, 56, bgColor: GuiColor.BlackAlmost)) {
			GUI.Label("  Category", BASE_W, GuiColor.DarkGrey);
			ImGui.SameLine(BASE_W);
			GUI.Label("  Name", NAME_W, GuiColor.DarkGrey);
			ImGui.SameLine(BASE_W + NAME_W);
			GUI.Label("  Wave", 1024, GuiColor.DarkGrey);
			ImGui.SameLine();
			if (Searcher.SearchResults.Count != PrevResultCount) {
				PrevResultCount = Searcher.SearchResults.Count;
				ResultCountText = $"result: {Searcher.SearchResults.Count}";
			}
			var resLabelSize = ImGui.CalcTextSize(ResultCountText);
			GUI.Label(ResultCountText, -resLabelSize.X - 24, GuiColor.DarkGrey);
		}

		// Result List
		float scrollMaxY;
		Vector2 winSize;
		using (new StyleScope(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f)))
		using (new StyleScope(ImGuiStyleVar.ButtonTextAlign, new Vector2(0f, 0f)))
		using (new ChildScope(-SCROLL_BAR_W, 0, 96, 6, windowFlags: ImGuiWindowFlags.NoScrollbar)) {

			winSize = ImGui.GetWindowSize();
			var winPos = ImGui.GetWindowPos();
			bool winHv = ImGui.IsMouseHoveringRect(winPos, winPos + winSize);
			var mousePos = GUI.MouseLeftHolding ? GUI.MouseLeftDownPos : GUI.MousePos;
			scrollMaxY = ImGui.GetScrollMaxY();
			if (RequireSetScroll) {
				RequireSetScroll = false;
				ContentScrollY = ContentScrollY.Clamp(0f, scrollMaxY);
				ImGui.SetScrollY(ContentScrollY);
			} else {
				ContentScrollY = ImGui.GetScrollY();
			}
			for (int i = 0; i < Searcher.SearchResults.Count; i++) {
				var line = Searcher.SearchResults[i];
				bool selecting = SelectingIndex == i;
				var textColor = selecting ? GuiColor.Green : GuiColor.Grey;

				// Size Checker
				ImGui.Dummy(new Vector2(1, ITEM_H));
				var min = ImGui.GetItemRectMin();
				var max = ImGui.GetItemRectMax();
				max.X = ImGui.GetContentRegionAvail().X + winPos.X;
				bool hovering = winHv && mousePos.X < max.X && mousePos.Y >= min.Y && mousePos.Y < max.Y;
				if (hovering) {
					textColor = selecting ? GuiColor.Green : GuiColor.White;
				}

				// Last In Range Check
				if (min.Y > winPos.Y + winSize.Y) {
					ImGui.Dummy(new Vector2(1, ITEM_H * (Searcher.SearchResults.Count - i - 1)));
					break;
				} else if (max.Y < winPos.Y) {
					continue;
				}

				// Base
				ImGui.SameLine();
				GUI.Label(line.BaseName, BASE_W - ITEM_PADDING, textColor);

				// Tint
				if (i % 2 == 1) {
					GUI.DrawFilledRect(min.X, min.Y, max.X, max.Y, GuiColor.White, 0.03f);
				}

				// Name
				ImGui.SameLine(BASE_W);
				float nameX = ImGui.GetCursorPosX() + winPos.X;
				GUI.HighlightLabel(line.Name, line.Keyword, NAME_W - ITEM_PADDING, textColor);
				GUI.DrawLine(nameX - ITEM_PADDING / 2, min.Y, nameX - ITEM_PADDING / 2, max.Y, GuiColor.Grey, 0.15f, 2f);

				// Played Mark
				if (PlayedPathIDs.Contains(line.PathID)) {
					GUI.DrawFilledCircle(nameX - 8f, min.Y + ITEM_H / 2, ITEM_H * 0.1f, GuiColor.DarkGreen, 1f, 24);
				}

				// Wave
				ImGui.SameLine(BASE_W + NAME_W);
				float waveX = winPos.X + ImGui.GetCursorPosX();
				bool waveDragging = false;
				bool wavClicked = false;
				waveDragging = GUI.Button("", out wavClicked, max.X - waveX, ITEM_H, GuiColor.Clear, returnHolding: true);
				WaveField(
					i, waveX + WAVE_BORD, min.Y + WAVE_BORD, max.X - waveX - WAVE_BORD, max.Y - min.Y - WAVE_BORD,
					waveDragging, wavClicked, winPos.Y, winPos.Y + winSize.Y
				);

				// Press
				bool press = false;
				if (winHv && ImGui.IsMouseHoveringRect(min, new(waveX, max.Y)) && GUI.MouseLeftDown) {
					press = true;
				}

				if (!winHv) {
					waveDragging = false;
					wavClicked = false;
				}

				// Change Selection
				if (press || waveDragging) {
					if (SelectingIndex != i) {
						SelectingIndex = i;
						LastExportedTime = -100f;
					}
				}

				// Play Wave on Press
				if (press) {
					PlaySelectingWave();
				}

				// Wave Click
				if (wavClicked) {
					PlaySelectingWave((mousePos.X - waveX - WAVE_BORD) / (max.X - waveX - WAVE_BORD));
				}

				anyClick = anyClick || press || waveDragging || wavClicked;
			}
			GUI.Label("", 0);
			GUI.Label("", 0);
			GUI.Label("", 0);
		}

		// Scrollbar
		float currentScroll = ContentScrollY;
		ImGui.SameLine();
		float handleSize = winSize.Y / (ITEM_H * Searcher.SearchResults.Count.GreaterOrEquel(1)) * winSize.Y;
		using (new StyleColorScope(ImGuiCol.SliderGrab, GuiColor.DarkGrey))
		using (new StyleColorScope(ImGuiCol.SliderGrabActive, GuiColor.DarkGrey))
		using (new StyleColorScope(ImGuiCol.FrameBg, GuiColor.Clear))
		using (new StyleColorScope(ImGuiCol.FrameBgHovered, GuiColor.ClearAlmost))
		using (new StyleColorScope(ImGuiCol.FrameBgActive, GuiColor.ClearAlmost))
		using (new StyleScope(ImGuiStyleVar.GrabRounding, 4f))
		using (new StyleScope(ImGuiStyleVar.GrabMinSize, handleSize.Clamp(24, winSize.Y))) {
			ImGui.VSliderFloat(
				"##Scrollbar", new Vector2(SCROLL_BAR_W, winSize.Y), ref currentScroll, scrollMaxY, 0f, "",
				ImGuiSliderFlags.AlwaysClamp
			);
		}

		if (currentScroll.NotAlmost(ContentScrollY)) {
			RequireSetScroll = true;
			ContentScrollY = currentScroll;
		}

		// No Drag Check
		if (!GUI.MouseLeftHolding) {
			Dragged = false;
		}

		// Stop Music Check
		if (IsMusicPlaying()) {
			// Stop when Click Outside
			if (!anyClick && ImGui.IsMouseClicked(ImGuiMouseButton.Left)) {
				StopMusic();
			}
			// Stop when Reach End
			if (SelectingIndex >= 0 && SelectingIndex < Searcher.SearchResults.Count) {
				var line = Searcher.SearchResults[SelectingIndex];
				float playTime = Raylib.GetMusicTimePlayed(Flow.Music);
				float dur = Raylib.GetMusicTimeLength(Flow.Music);
				if (playTime > dur * line.EndTime01) {
					StopMusic();
				}
			} else {
				StopMusic();
			}
		}

	}


	private void Update_Hotkey () {

		if (!Searcher.Imported) return;
		if (ImGui.GetIO().WantCaptureKeyboard) return;

		bool ctrl = Raylib.IsKeyDown(KeyboardKey.LeftControl);
		bool alt = Raylib.IsKeyDown(KeyboardKey.LeftAlt);
		bool shift = Raylib.IsKeyDown(KeyboardKey.LeftShift);

		// Play/Stop Music
		if (Raylib.IsKeyPressed(KeyboardKey.Space) && !ctrl && !shift && !alt) {
			if (IsMusicPlaying()) {
				StopMusic();
			} else {
				PlaySelectingWave();
			}
		}

		// Export
		if (Raylib.IsKeyPressed(KeyboardKey.E) && ctrl && !shift && !alt) {
			ExportSelectingAudio();
		}

		// Reset All Clamp Time
		if (Raylib.IsKeyPressed(KeyboardKey.R) && ctrl && shift && !alt) {
			foreach (var line in Searcher.SearchResults) {
				line.StartTime01 = 0f;
				line.EndTime01 = 1f;
			}
		}

		// Reset Played
		if (Raylib.IsKeyPressed(KeyboardKey.R) && ctrl && shift && !alt) {
			PlayedPathIDs.Clear();
		}

		// Reset Internal Buffer
		ImGui.GetIO().ClearInputKeys();

	}


	public override void OnFileDropped (string[] paths) {
		if (paths == null || paths.Length == 0) return;
		foreach (var path in paths) {
			if (!Util.FolderExists(path)) continue;
			AudioRootPath = path;
			Searcher.ImportAsync(path, SavingFolder, true);
			break;
		}
	}


	public override void Quit () => SaveSettingToFile();


	#endregion




	#region --- LGC ---


	// Workflow
	private void LoadSettingFromFile () {
		string path = Util.CombinePaths(SavingFolder, "Setting.txt");
		AudioRootPath = "";
		if (!Util.FileExists(path)) return;
		using var stream = File.OpenRead(path);
		using var reader = new StreamReader(stream);
		AudioRootPath = reader.ReadLine();
		ExportPath = reader.ReadLine();
	}


	private void SaveSettingToFile () {
		string path = Util.CombinePaths(SavingFolder, "Setting.txt");
		using var stream = File.Create(path);
		using var writer = new StreamWriter(stream);
		writer.WriteLine(AudioRootPath);
		writer.WriteLine(ExportPath);
	}


	private void ExportSelectingAudio () {

		if (SelectingIndex < 0 || SelectingIndex >= Searcher.SearchResults.Count) return;

		ExportPath = ExportPath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		var line = Searcher.SearchResults[SelectingIndex];
		if (!Util.FileExists(line.Path)) return;

		string name = Util.GetNameWithExtension(line.Path);
		string exportFilePath = Util.CombinePaths(ExportPath, name);

		if (line.StartTime01 > 0.001f || line.EndTime01 < 0.999f) {
			// Require Crop
			// Load Wave
			var wave = Raylib.LoadWave(line.Path);
			if (!Raylib.IsWaveValid(wave)) return;

			// Crop Wave
			int startFrame = (int)(line.StartTime01 * wave.SampleCount);
			int endFrame = (int)(line.EndTime01 * wave.SampleCount);
			if (startFrame > 5 || endFrame < wave.SampleCount - 5) {
				Raylib.WaveCrop(ref wave, startFrame, endFrame);
			}

			// Save Wave
			LastExportSuccess = Raylib.ExportWave(wave, Util.ChangeExtension(exportFilePath, ".wav"));
			LastExportedTime = GUI.Time;
			Raylib.UnloadWave(wave);
#if DEBUG
			if (LastExportSuccess) {
				Debug.LogSuccess($"Sound Exported: {exportFilePath}");
			} else {
				Debug.LogWarning($"Export Failed");
			}
#endif
		} else {
			// Copy File
			LastExportSuccess = Util.CopyFile(line.Path, exportFilePath);
			LastExportedTime = GUI.Time;
#if DEBUG
			if (LastExportSuccess) {
				Debug.LogSuccess($"Sound Copyed: {exportFilePath}");
			} else {
				Debug.LogWarning($"Copy Failed");
			}
#endif
		}

	}


	// Music
	private void PlaySelectingWave (float playTime01 = -1f, bool playFromStartWhenPlayTimeNearEnd = true) {
		if (SelectingIndex < 0 || SelectingIndex >= Searcher.SearchResults.Count) return;
		var line = Searcher.SearchResults[SelectingIndex];
		playTime01 = playTime01 < -0.5f ? line.StartTime01 : playTime01;
		playTime01 = Math.Clamp(playTime01, 0f, 1f);
		PlayedPathIDs.Add(line.PathID);
		// Play Audio
		var music = Flow.Music;
		if (MusicPathID != line.PathID) {
			MusicPathID = line.PathID;
			if (Raylib.IsMusicValid(music)) {
				Raylib.UnloadMusicStream(music);
			}
			music = Raylib.LoadMusicStream(line.Path);
		}
		if (Raylib.IsMusicValid(music)) {
			music.Looping = false;
			float dur = Raylib.GetMusicTimeLength(music);
			float time = playTime01 * dur;
			// Near End Check
			if (playFromStartWhenPlayTimeNearEnd && Math.Abs(time - line.EndTime01 * dur) < 0.1f) {
				time = line.StartTime01 * dur;
			}
			// Seek
			Raylib.SeekMusicStream(music, time);
			Raylib.PlayMusicStream(music);
		}
		Flow.Music = music;
	}


	private void StopMusic () {
		var music = Flow.Music;
		if (!Raylib.IsMusicValid(music)) return;
		if (Raylib.IsMusicStreamPlaying(music)) {
			Raylib.PauseMusicStream(music);
		}
	}


	private bool IsMusicPlaying () => Raylib.IsMusicValid(Flow.Music) && Raylib.IsMusicStreamPlaying(Flow.Music);


	// UI
	private void WaveField (int index, float x, float y, float width, float height, bool dragging, bool click, float clampMinY, float clampMaxY) {

		var line = Searcher.SearchResults[index];
		if (!WavePool.TryGetWave(line.PathID, out var wave)) {
			WavePool.RequireWave(line.PathID, line.Path);
		}

		float leftEdgeX = x + width * line.StartTime01;
		float rightEdgeX = x + width * line.EndTime01;
		var mouseX = GUI.MousePos.X;
		var mouseY = GUI.MousePos.Y;
		bool leftEdgeChanged = leftEdgeX > x;
		bool rightEdgeChanged = rightEdgeX < x + width - 1;

		// BG
		GUI.DrawFilledRect(
			leftEdgeX, y, rightEdgeX, y + height, GuiColor.White,
			wave == null ? Util.PingPong(GUI.Time, 1f) / 5f : 0.1f,
			4f
		);

		// Content
		var waveColor = SelectingIndex == index ? GuiColor.Green : GuiColor.DarkWhite;
		var outsideWaveColor = GuiColor.DarkGrey;
		float thickness = (width / Wave.WAVE_LEN) + 1f;
		float centerY = y + height / 2f;
		GUI.DrawLine(x, centerY, leftEdgeX, centerY, outsideWaveColor, 1f, 1.5f);
		GUI.DrawLine(rightEdgeX, centerY, x + width, centerY, outsideWaveColor, 1f, 1.5f);
		GUI.DrawLine(leftEdgeX, centerY, rightEdgeX, centerY, waveColor, 1f, 1.5f);
		if (wave != null) {
			for (int i = 0; i < Wave.WAVE_LEN; i++) {
				float lineX = x + width * i / Wave.WAVE_LEN;
				float lineH = height * wave.Data[i] / 512f;
				var color = lineX >= leftEdgeX && lineX <= rightEdgeX ? waveColor.ToVec4() : outsideWaveColor.ToVec4();
				float yMin = Math.Clamp(centerY - lineH, clampMinY, clampMaxY);
				float yMax = Math.Clamp(centerY + lineH, clampMinY, clampMaxY);
				Raylib.DrawLineEx(
					new Vector2(lineX, yMin),
					new Vector2(lineX, yMax),
					thickness,
					new Color(color.X, color.Y, color.Z, color.W)
				);
			}
		}

		// Hover Logic
		int cursorIndex = 0;
		if (mouseY >= y && mouseY <= y + height) {
			// Cursor
			const float EDGE_CUR_GAP = 16;
			if (mouseX > leftEdgeX - EDGE_CUR_GAP && mouseX < leftEdgeX + EDGE_CUR_GAP) {
				// Edge L
				GUI.RequireCursor = MouseCursor.ResizeEw;
				cursorIndex = 2;
				if (GUI.MouseLeftDown) {
					DraggingEdgeOffsetX = mouseX - leftEdgeX;
				}
			} else if (mouseX > rightEdgeX - EDGE_CUR_GAP && mouseX < rightEdgeX + EDGE_CUR_GAP) {
				// Edge R
				GUI.RequireCursor = MouseCursor.ResizeEw;
				cursorIndex = 3;
				if (GUI.MouseLeftDown) {
					DraggingEdgeOffsetX = mouseX - rightEdgeX;
				}
			} else if (mouseX > x && mouseX < x + width) {
				// Inside
				GUI.RequireCursor = MouseCursor.Crosshair;
				cursorIndex = 1;
			}
			// Mid Click
			if (GUI.MouseMidDown) {
				line.StartTime01 = 0f;
				line.EndTime01 = 1f;
			}
		}

		// Draw Edge Line
		if (SelectingIndex == index) {
			GUI.DrawLine(
				leftEdgeX - 4f, y, leftEdgeX - 4f, y + height,
				cursorIndex == 2 ? GuiColor.Orange : GuiColor.Green,
				1f, 4f
			);
		}
		if (SelectingIndex == index) {
			GUI.DrawLine(
				rightEdgeX + 4f, y, rightEdgeX + 4f, y + height,
				cursorIndex == 3 ? GuiColor.Orange : GuiColor.Green,
				1f, 4f
			);
		}

		// Draw Playing Line
		var music = Flow.Music;
		if (SelectingIndex == index && IsMusicPlaying()) {
			float playTime = Raylib.GetMusicTimePlayed(music);
			float dur = Raylib.GetMusicTimeLength(music);
			float playX = x + width * playTime / dur;
			GUI.DrawLine(
				playX, y, playX, y + height,
				GuiColor.Red,
				1f, 4f
			);
		}

		// Dragging
		if (dragging) {
			// Start
			if (DraggingWaveEdgeIndex == 0) {
				DraggingWaveEdgeIndex = cursorIndex;
			}
			if (Math.Abs(mouseX - GUI.MouseLeftDownPos.X) > 12) {
				Dragged = true;
			}
			// Dragging
			switch (DraggingWaveEdgeIndex) {
				// Left
				case 2:
					line.StartTime01 = (Math.Clamp(mouseX - DraggingEdgeOffsetX, x, rightEdgeX) - x) / width;
					Dragged = true;
					GUI.RequireCursor = MouseCursor.ResizeEw;
					break;
				// Right
				case 3:
					line.EndTime01 = (Math.Clamp(mouseX - DraggingEdgeOffsetX, leftEdgeX, x + width) - x) / width;
					Dragged = true;
					GUI.RequireCursor = MouseCursor.ResizeEw;
					break;
				// Inside
				default:
					if (!Dragged) break;
					// Dragged
					float startMouseX = GUI.MouseLeftDownPos.X;
					float endMouseX = mouseX;
					if (endMouseX < startMouseX) {
						(startMouseX, endMouseX) = (endMouseX, startMouseX);
					}
					line.StartTime01 = (Math.Clamp(startMouseX, x, x + width) - x) / width;
					line.EndTime01 = (Math.Clamp(endMouseX, startMouseX, x + width) - x) / width;
					GUI.RequireCursor = MouseCursor.Crosshair;
					break;
			}
		}
		if (click && !Dragged) {
			if (mouseX < rightEdgeX) {
				line.StartTime01 = (Math.Clamp(mouseX, x, rightEdgeX) - x) / width;
			}
		}

	}


	private void DrawSearchIcon () {
		var fieldMin = ImGui.GetItemRectMin();
		var fieldMax = ImGui.GetItemRectMax();
		float size = fieldMax.Y - fieldMin.Y;
		// Circle
		GUI.DrawCircle(
			fieldMax.X - size * 0.7f,
			fieldMax.Y - size * 0.58f,
			size * 0.25f,
			GuiColor.Grey,
			thickness: 4f
		);
		// Line
		GUI.DrawLine(
			fieldMax.X - size * 0.53f,
			fieldMax.Y - size * 0.43f,
			fieldMax.X - size * 0.33f,
			fieldMax.Y - size * 0.23f,
			GuiColor.Grey,
			thickness: 5.5f
		);
	}


	#endregion




}
