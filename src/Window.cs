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
	private readonly AudioSearcher Searcher = new();
	private string SearchingText = "";
	private bool SearchingInputActive = false;
	private int SelectingIndex = -1;
	private int DraggingEdgeIndex = 0;
	private bool Dragged = false;
	private float DraggingEdgeOffsetX;

	// Setting
	private string AudioRootPath = "";
	private string ExportPath = "";


	#endregion




	#region --- MSG ---


	public override void Start () {
		LoadSettingFromFile();
		Wave.WaveCacheRoot = Util.CombinePaths(SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
		Searcher.ImportAsync(AudioRootPath, forceImport: false);
		SelectingIndex = -1;
	}


	public override void Update () {
		Update_Toolbar();
		Update_Content();
	}


	private void Update_Toolbar () {

		const float BTN_W = 256;
		if (string.IsNullOrWhiteSpace(ExportPath)) {
			ExportPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		}

		// =========== First Row ===========

		// Root Label
		const string ROOT_LABEL = "Search Root:";
		float labelW = ImGui.CalcTextSize(ROOT_LABEL).X + 24;
		GUI.Label(ROOT_LABEL, 0, GuiColor.DarkGrey);

		// Root Input
		ImGui.SameLine(labelW);
		GUI.Input("##Root", ref AudioRootPath, width: -BTN_W, bodyColor: GuiColor.DarkGrey);

		// Import Button
		ImGui.SameLine();
		ImGui.Spacing();
		ImGui.SameLine();
		bool rootPathChanged = !string.IsNullOrEmpty(AudioRootPath) && AudioRootPath != Searcher.AudioRootPath;
		if (GUI.Button(
			rootPathChanged ? "Import" : "Reimport",
			width: BTN_W,
			bodyColor: rootPathChanged ? (ImGui.GetTime() % 0.8f < 0.4f ? GuiColor.Green : GuiColor.AzureGreen) : GuiColor.DarkGrey,
			enable: !string.IsNullOrEmpty(AudioRootPath)
		)) {
			Searcher.ImportAsync(AudioRootPath, forceImport: true);
			SelectingIndex = -1;
		}
		ImGui.Dummy(new(0, 8));



		// =========== Second Row ===========
		if (!string.IsNullOrEmpty(AudioRootPath)) {
			// Export Label
			GUI.Label("Export:", 0, GuiColor.DarkGrey);

			// Export Input
			ImGui.SameLine(labelW);
			GUI.Input("##Export", ref ExportPath, width: -BTN_W, bodyColor: GuiColor.DarkGrey);

			// Export Button
			ImGui.SameLine();
			ImGui.Spacing();
			ImGui.SameLine();
			bool expEnable = !string.IsNullOrEmpty(ExportPath) && SelectingIndex >= 0;
			if (GUI.Button(
				"Export",
				width: BTN_W,
				bodyColor: expEnable ? GuiColor.DarkGreen : GuiColor.DarkGrey,
				enable: expEnable
			)) {
				ExportCurrentAudio();
			}

		}
		ImGui.Dummy(new(0, 8));


		// =========== Third Row ===========
		if (!string.IsNullOrEmpty(AudioRootPath)) {

			// Search Label
			GUI.Label("Search:", 0, GuiColor.DarkGrey);

			// Search Input
			ImGui.SameLine(labelW);
			bool endEdit = GUI.Input("##Search", ref SearchingText, width: -BTN_W);
			if (SearchingInputActive && !endEdit) {
				Searcher.PerformSearch(SearchingText);
				SelectingIndex = -1;
			}
			SearchingInputActive = endEdit;
			DrawSearchIcon();

		}

		// =========== Final ===========
		ImGui.Dummy(new(0, 64));

	}


	private void Update_Content () {

		if (string.IsNullOrEmpty(AudioRootPath)) return;

		// Importing
		if (!Searcher.Imported && Searcher.Importing) {
			ImGui.Spacing();
			GUI.Label($"Importing... ({Searcher.ImportPathCount})", 0, GuiColor.White);
			GUI.Label(Searcher.LastImportedPath, 0, GuiColor.Grey);
			return;
		}

		// No Result
		if (Searcher.SearchResults.Count == 0) {
			ImGui.Spacing();
			GUI.Label("(No Result)", 0);
			return;
		}

		const float BASE_W = 560;
		const float NAME_W = 512;
		const float ITEM_H = 64;
		const float ITEM_PADDING = 24;
		const float WAVE_BORD = 4;
		if (!GUI.MouseLeftHolding) {
			DraggingEdgeIndex = 0;
		}

		// Top Bar
		using (new ChildScope(0, 56, bgColor: GuiColor.BlackAlmost)) {
			GUI.Label("Category", 0, GuiColor.DarkGrey);
			ImGui.SameLine(BASE_W);
			GUI.Label("Name", 0, GuiColor.DarkGrey);
			ImGui.SameLine(BASE_W + NAME_W);
			GUI.Label("Wave", 0, GuiColor.DarkGrey);
		}

		// Result List
		using var _ = new StyleScope(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f));
		using var __ = new StyleScope(ImGuiStyleVar.ButtonTextAlign, new Vector2(0f, 0f));
		using var ___ = new ChildScope(0, 0, 96, 6);
		for (int i = 0; i < Searcher.SearchResults.Count; i++) {
			var line = Searcher.SearchResults[i];
			bool selecting = SelectingIndex == i;
			var textColor = selecting ? GuiColor.Green : GuiColor.DarkWhite;

			// Base
			bool press = false;
			press = GUI.Button(line.BaseName, BASE_W - ITEM_PADDING, ITEM_H, GuiColor.Clear, textColor);
			var min = ImGui.GetItemRectMin();
			var max = ImGui.GetItemRectMax();
			max.X = ImGui.GetContentRegionAvail().X + WindowPadding;

			// Tint
			if (i % 2 == 1) {
				GUI.DrawFilledRect(min.X, min.Y, max.X, max.Y, GuiColor.White, 0.03f);
			}

			// Name
			ImGui.SameLine(BASE_W);
			press = GUI.Button(line.Name, NAME_W - ITEM_PADDING, ITEM_H, GuiColor.Clear, textColor) || press;

			// Wave
			ImGui.SameLine(BASE_W + NAME_W);
			float waveX = ImGui.GetWindowPos().X + ImGui.GetCursorPosX();
			bool waveDragging = GUI.Button("", out bool wavClicked, max.X - waveX, ITEM_H, GuiColor.Clear, returnHolding: true);
			WaveField(
				i, waveX + WAVE_BORD, min.Y + WAVE_BORD, max.X - waveX - WAVE_BORD, max.Y - min.Y - WAVE_BORD,
				waveDragging, wavClicked
			);

			// Hover
			var mousePos = GUI.MouseLeftHolding ? GUI.MouseLeftDownPos : GUI.MousePos;
			if (mousePos.X < max.X && mousePos.Y >= min.Y && mousePos.Y < max.Y) {
				GUI.DrawFilledRect(min.X, min.Y, waveX, max.Y, GUI.MouseLeftHolding ? GuiColor.Green : GuiColor.White, 0.05f);
			}

			// Press
			if (press) {
				SelectingIndex = i;
				PlaySelectingWave();
			}

			// Wave Press
			if (waveDragging) {
				SelectingIndex = i;

				PlaySelectingWave();
			}

		}
		GUI.Label("", 0);
		GUI.Label("", 0);
		GUI.Label("", 0);
		if (!GUI.MouseLeftHolding) {
			Dragged = false;
		}

	}


	public override void OnFileDropped (string[] paths) {
		if (paths == null || paths.Length == 0) return;
		foreach (var path in paths) {
			if (!Util.FolderExists(path)) continue;
			Searcher.ImportAsync(path, true);
			break;
		}
	}


	public override void Quit () {
		SaveSettingToFile();
	}


	#endregion




	#region --- LGC ---


	// Setting
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


	// Misc
	private void ExportCurrentAudio () {
		if (SelectingIndex < 0 || SelectingIndex >= Searcher.SearchResults.Count) return;
		var line = Searcher.SearchResults[SelectingIndex];
		Debug.Log(line.Path);

	}


	private void PlaySelectingWave () {

	}


	// UTL
	private void DrawSearchIcon () {
		var fieldMin = ImGui.GetItemRectMin();
		var fieldMax = ImGui.GetItemRectMax();
		float size = fieldMax.Y - fieldMin.Y;
		// Circle
		GUI.DrawCircle(
			fieldMax.X - size * 0.7f,
			fieldMax.Y - size * 0.58f,
			size * 0.25f,
			GuiColor.DarkGrey,
			4f
		);
		// Line
		GUI.DrawLine(
			fieldMax.X - size * 0.53f,
			fieldMax.Y - size * 0.43f,
			fieldMax.X - size * 0.33f,
			fieldMax.Y - size * 0.23f,
			GuiColor.DarkGrey,
			5.5f
		);
	}


	private void WaveField (int index, float x, float y, float width, float height, bool dragging, bool click) {

		var line = Searcher.SearchResults[index];
		if (!WavePool.TryGetWave(line.PathID, out var wave)) {
			WavePool.RequireWave(line.PathID, line.Path);
			return;
		}
		if (wave == null) return;

		float leftEdgeX = x + width * line.StartTime01;
		float rightEdgeX = x + width * line.EndTime01;
		var mouseX = GUI.MousePos.X;
		var mouseY = GUI.MousePos.Y;
		bool leftEdgeChanged = leftEdgeX > x;
		bool rightEdgeChanged = rightEdgeX < x + width - 1;

		// BG
		GUI.DrawFilledRect(leftEdgeX, y, rightEdgeX, y + height, GuiColor.White, 0.1f, 4f);

		// Content
		var waveColor = SelectingIndex == index ? GuiColor.Green : GuiColor.WhiteAlmost;
		var outsideWaveColor = GuiColor.DarkGrey;
		float thickness = (width / Wave.WAVE_LEN) + 1f;
		float centerY = y + height / 2f;
		GUI.DrawLine(x, centerY, leftEdgeX, centerY, outsideWaveColor, 1f, 1.5f);
		GUI.DrawLine(rightEdgeX, centerY, x + width, centerY, outsideWaveColor, 1f, 1.5f);
		GUI.DrawLine(leftEdgeX, centerY, rightEdgeX, centerY, waveColor, 1f, 1.5f);
		for (int i = 0; i < Wave.WAVE_LEN; i++) {
			float lineX = x + width * i / Wave.WAVE_LEN;
			float lineH = height * wave.Data[i] / 512f;
			var color = lineX >= leftEdgeX && lineX <= rightEdgeX ? waveColor : outsideWaveColor;
			GUI.DrawLine(lineX, centerY - lineH, lineX, centerY + lineH, color, 1f, thickness);
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
			// Right Click
			if (GUI.MouseRightDown) {
				line.StartTime01 = 0f;
				line.EndTime01 = 1f;
			}
			// Mid Click
			if (GUI.MouseMidDown && SelectingIndex == index) {
				ExportCurrentAudio();
			}
		}

		// Draw Edge Line
		if (SelectingIndex == index) {
			GUI.DrawLine(
				leftEdgeX, y, leftEdgeX, y + height,
				cursorIndex == 2 ? GuiColor.Orange : GuiColor.Green,
				1f, 4f
			);
		}
		if (SelectingIndex == index) {
			GUI.DrawLine(
				rightEdgeX, y, rightEdgeX, y + height,
				cursorIndex == 3 ? GuiColor.Orange : GuiColor.Green,
				1f, 4f
			);
		}

		// Dragging
		if (dragging) {
			// Start
			if (DraggingEdgeIndex == 0) {
				DraggingEdgeIndex = cursorIndex;
			}
			if (Math.Abs(mouseX - GUI.MouseLeftDownPos.X) > 12) {
				Dragged = true;
			}
			// Dragging
			switch (DraggingEdgeIndex) {
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


	#endregion




}
