using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;

namespace QuickSound;

public class QWave {




	#region --- VAR ---


	// Const
	public const int WAVE_LEN = 256;

	// Api
	public readonly float[] Data = new float[WAVE_LEN];


	#endregion




	#region --- MSG ---


	public QWave () {

	}


	#endregion




	#region --- API ---


	public void LoadFromFile (string path) {

	}


	public void SaveToFile (string path) {

	}


	// Audio
	public static unsafe void SaveWaveFileFromAudioFile (string audioFilePath, string waveFolderPath) {
		if (!Util.FileExists(audioFilePath)) return;
		var wave = Raylib.LoadWave(audioFilePath);
		if (!Raylib.IsWaveValid(wave)) return;
		if (wave.SampleCount == 0 || wave.Channels == 0) return;
		string name = Util.GetNameWithoutExtension(audioFilePath);
		string waveFilePath = Util.CombinePaths(waveFolderPath, name);
		using var stream = File.Create(waveFilePath);
		using var writer = new BinaryWriter(stream);
		try {
			Debug.Log(
				$"name:\"{name}\",",
				$"ch:{wave.Channels},",
				$"count:{wave.SampleCount},",
				$"size:{wave.SampleSize},",
				$"rate:{wave.SampleRate}"
			);
			var data16 = (Half*)wave.Data;
			var data32 = (float*)wave.Data;
			var data64 = (double*)wave.Data;
			int sCount = (int)wave.SampleCount;
			int sSize = (int)wave.SampleSize;
			int channelCount = (int)wave.Channels;
			int singleChLen = sCount / channelCount;
			for (int i = 0; i < singleChLen; i++) {
				// Get Sample
				float sample = 0f;
				int chOffset = sCount / channelCount;
				for (int ch = 0; ch < channelCount; ch++) {
					int index = i + ch * chOffset;
					float v =
						sSize == 16 ? (float)data16[index] :
						sSize == 32 ? data32[index] :
						sSize == 64 ? (float)data64[index] :
						0;
					sample += MathF.Abs(v) / ch;
				}
				// Merge Sample




			}
		} catch (System.Exception ex) { Debug.LogException(ex); }
		Raylib.UnloadWave(wave);
	}


	#endregion




	#region --- LGC ---



	#endregion




}
