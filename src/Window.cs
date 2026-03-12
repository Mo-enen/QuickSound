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

	// Data
	private string AudioRootPath;


	#endregion




	#region --- MSG ---


	public override void Start () {
		AudioRootPath = Util.GetParentPath(System.Environment.ProcessPath);
		Wave.WaveCacheRoot = Util.CombinePaths(SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
	}


	public override void Update () {

		if (Button(" Test ")) {
			Debug.Log("Test");
		}


	}


	public override void Quit () {

	}


	#endregion




	#region --- LGC ---



	#endregion




}
