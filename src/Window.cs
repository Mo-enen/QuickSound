using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using RayFlow;
using ImGuiNET;
using rlImGui_cs;
using Wave = QuickSound.Wave;

namespace QuickSound;


public class Window : FlowWindow {




	#region --- VAR ---


	// Api
	public override string DeveloperName => "Moenen";
	public override int WindowPadding => 2;

	// Data
	private readonly AudioSearcher Searcher = new();
	private string AudioRootPath;
	private string SearchingText = "";
	private bool SearchingInputActive = false;
	private int SelectingIndex = -1;


	#endregion




	#region --- MSG ---


	public override void Start () {
		AudioRootPath = Util.GetParentPath(System.Environment.ProcessPath);
		Wave.WaveCacheRoot = Util.CombinePaths(SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
		ImportAllAudio(forceImport: false);
		Searcher.PerformSearch("", AudioRootPath);
		SelectingIndex = -1;
	}


	public override void Update () {
		Update_Toolbar();
		ImGui.Spacing();
		Update_SearchResult();
	}


	private void Update_Toolbar () {

		// Search Input
		bool newActive = GUI.Input("##", ref SearchingText, width: -512 - 16);
		if (SearchingInputActive && !newActive) {
			Searcher.PerformSearch(SearchingText, AudioRootPath);
			SelectingIndex = -1;
		}
		SearchingInputActive = newActive;
		DrawSearchIcon();

		// Import Button
		ImGui.SameLine();
		if (GUI.Button("Import", width: 256)) {
			ImportAllAudio(forceImport: true);
		}

		// Save Button
		ImGui.SameLine();
		if (
			GUI.Button("Save", width: 256) ||
			(Raylib.IsKeyPressed(KeyboardKey.S) && Raylib.IsKeyDown(KeyboardKey.LeftControl))
		) {
			SaveCurrentAudio();
		}

	}


	private void Update_SearchResult () {

		// No Result
		if (Searcher.SearchResults.Count == 0) {
			ImGui.Spacing();
			GUI.Label("(No Result)", 0);
			return;
		}

		// Top Bar
		const float BASE_W = 560;
		const float NAME_W = 512;
		using (new ChildScope(0, 56, bgColor: GuiColor.BlackAlmost)) {
			GUI.Label("Category", 0, GuiColor.DarkGrey);
			GUI.LineOnLastItem(false);
			ImGui.SameLine(BASE_W);
			GUI.Label("Name", 0, GuiColor.DarkGrey);
			GUI.LineOnLastItem(false);
			ImGui.SameLine(BASE_W + NAME_W);
			GUI.Label("Wave", 0, GuiColor.DarkGrey);
			GUI.LineOnLastItem(false);
		}

		// Result List
		using (new StyleScope(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 0f)))
		using (new StyleScope(ImGuiStyleVar.ButtonTextAlign, new Vector2(0f, 0f)))
		using (new ChildScope(0, 0, 96, 6)) {
			const float ITEM_H = 64;
			const float ITEM_PADDING = 24;
			for (int i = 0; i < Searcher.SearchResults.Count; i++) {
				var line = Searcher.SearchResults[i];
				bool highlight = SelectingIndex == i;
				var textColor = highlight ? GuiColor.Green : GuiColor.DarkWhite;

				// Base
				bool press = false;
				press = GUI.Button(line.BaseName, BASE_W - ITEM_PADDING, ITEM_H, GuiColor.Clear, textColor);
				var min = ImGui.GetItemRectMin();
				var max = ImGui.GetItemRectMax();
				max.X = ImGui.GetContentRegionAvail().X;

				// Name
				ImGui.SameLine(BASE_W);
				press = GUI.Button(line.Name, NAME_W - ITEM_PADDING, ITEM_H, GuiColor.Clear, textColor) || press;

				// Wave
				ImGui.SameLine(BASE_W + NAME_W);
				float waveX = ImGui.GetCursorPosX();
				bool wavePress = GUI.Button("", max.X - waveX, ITEM_H, GuiColor.Clear);
				const float WBORD = 4;
				DrawWave(i, waveX + WBORD, min.Y + WBORD, max.X - waveX - WBORD, max.Y - min.Y - WBORD);

				// Hover
				var mousePos = GUI.MouseLeftDown ? GUI.PrevMouseLeftDownPos : GUI.MousePos;
				if (mousePos.X < max.X && mousePos.Y >= min.Y && mousePos.Y < max.Y) {
					GUI.DrawFilledRect(min.X, min.Y, max.X, max.Y, GUI.MouseLeftDown ? GuiColor.Green : GuiColor.White, 0.05f);
				}

				// Press
				if (press) {
					SelectingIndex = i;
					PlaySelectingWave();
				}

				// Wave Press
				if (wavePress) {
					SelectingIndex = i;

					PlaySelectingWave();
				}

			}
			GUI.Label("", 0);
			GUI.Label("", 0);
			GUI.Label("", 0);
		}

	}


	public override void Quit () {

	}


	#endregion




	#region --- LGC ---


	private void ImportAllAudio (bool forceImport = false) {



	}


	private void SaveCurrentAudio () {



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


	private void DrawWave (int index, float x, float y, float width, float height) {

		// BG
		GUI.DrawFilledRect(x, y, x + width, y + height, GuiColor.White, 0.1f, 4f);

		// Content
		const float WAVE_PADDING = 12;
		x += WAVE_PADDING;
		width -= WAVE_PADDING * 2;
		var line = Searcher.SearchResults[index];
		if (!WavePool.TryGetWave(line.PathID, out var wave)) {
			WavePool.RequireWave(line.PathID, line.Path);
			return;
		}
		if (wave == null) return;
		var waveColor = SelectingIndex == index ? GuiColor.Green : GuiColor.WhiteAlmost;
		float thickness = (width / Wave.WAVE_LEN) + 1f;
		float centerY = y + height / 2f;
		GUI.DrawLine(x, centerY, x + width, centerY, waveColor, 1f, 1.5f);
		for (int i = 0; i < Wave.WAVE_LEN; i++) {
			float lineX = x + width * i / Wave.WAVE_LEN;
			float lineH = height * wave.Data[i] / 512f;
			GUI.DrawLine(lineX, centerY - lineH, lineX, centerY + lineH, waveColor, 1f, thickness);
		}

	}


	#endregion




}
