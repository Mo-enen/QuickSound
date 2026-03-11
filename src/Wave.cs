using System.Collections;
using System.Collections.Generic;
using Raylib_cs;

namespace QuickSound;

public class Wave {




	#region --- VAR ---


	// Const
	public const int WAVE_LEN = 256;
	private static readonly Wave TempWave = new();

	// Api
	public readonly float[] Data = new float[WAVE_LEN];


	#endregion




	#region --- MSG ---


	public Wave () {

	}


	#endregion




	#region --- API ---


	public void LoadFromFile (string path) {

	}


	public void SaveToFile (string path) {

	}


	// Audio
	public static void SaveWaveFileFromAudioFile (string audioFilePath) {
		if (!Util.FileExists(audioFilePath)) return;
		var wave = Raylib.LoadWave(audioFilePath);
		if (!Raylib.IsWaveValid(wave)) return;
		try {



		} catch (System.Exception ex) { Debug.LogException(ex); }
		Raylib.UnloadWave(wave);
	}


	#endregion




	#region --- LGC ---



	#endregion




}
