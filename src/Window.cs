using System;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using QuickSound;
using Wave = QuickSound.Wave;

Flow.Run(Window.Start, Window.Update, Window.Quit, devName: "Moenen");

public static class Window {


	// VAR
	private static string AudioRootPath;


	// MSG
	public static void Start () {
		AudioRootPath = Util.GetParentPath(System.Environment.ProcessPath);
		Wave.WaveCacheRoot = Util.CombinePaths(Flow.SavingFolder, "Wave");
		WavePool.StartBackgroundLoop();
	}


	public static void Update () {





	}


	public static void Quit () {

	}

}
