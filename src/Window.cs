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




	#region --- SUB ---


	private class ResultLine {
		public string BaseName;
		public string Name;
		public string Path;
		public int PathID;
		public ResultLine (string path, string audioRootPath) {
			Path = path;
			PathID = path.AngeHash();
			Name = Util.GetNameWithoutExtension(path);
			if (Util.TryGetRelativePath(audioRootPath, Util.GetParentPath(path), out string rPath)) {
				BaseName = rPath;
			} else {
				BaseName = Util.GetParentPath(Path);
			}
		}
	}


	#endregion




	#region --- VAR ---


	// Api
	public override string DeveloperName => "Moenen";


	// Data
	private readonly List<ResultLine> SearchResults = [];
	private string AudioRootPath;
	private string SearchingText = "";
	private bool SearchingInputActive = false;


	#endregion




	#region --- MSG ---


	public override void Start () {
		AudioRootPath = Util.GetParentPath(System.Environment.ProcessPath);
		Wave.WaveCacheRoot = Util.CombinePaths(SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
		ImportAllAudio(forceImport: false);
		PerformSearch();
	}


	public override void Update () {
		Update_Toolbar();
		ImGui.Spacing();
		Update_SearchResult();
	}


	private void Update_Toolbar () {

		// Search Input
		bool newActive = GUI.Input("##", ref SearchingText, width: -512);
		if (SearchingInputActive && !newActive) {
			PerformSearch();
		}
		SearchingInputActive = newActive;
		SearchIcon();

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
		if (SearchResults.Count == 0) {
			ImGui.Spacing();
			GUI.Label("(No Result)", 0);
			return;
		}

		// Top Bar
		using (new ChildScope(0, 56)) {

		}

		// Result List
		using (new ChildScope(0, 0, 96, 6)) {
			for (int i = 0; i < SearchResults.Count; i++) {
				var line = SearchResults[i];

				GUI.Label(line.BaseName, 0);

				ImGui.SameLine(512);
				GUI.Label(line.Name, 0);

				ImGui.SameLine(1024);
				GUI.Label("(Wave)", 0);

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


	private void PerformSearch () {

		//SearchingText
		SearchResults.Clear();



		/////////////////////////////
		for (int i = 0; i < 1024; i++) {
			SearchResults.Add(new ResultLine($"{AudioRootPath}/Test/Test/Test A/Test {i}", AudioRootPath));
		}
		/////////////////////////////



		// Finish
		GC.Collect();
	}


	private void ImportAllAudio (bool forceImport = false) {



	}


	private void SaveCurrentAudio () {



	}


	// UTL
	private void SearchIcon () {
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


	#endregion




}
