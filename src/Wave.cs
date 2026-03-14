using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Raylib_cs;
using RayFlow;

namespace QuickSound;

public class Wave {




	#region --- VAR ---


	// Const
	public const int WAVE_LEN = 256;

	// Api
	public static string WaveCacheRoot = "";
	public readonly byte[] Data = new byte[WAVE_LEN];


	#endregion




	#region --- API ---


	public void LoadFromFile (string path) {
		using var stream = File.OpenRead(path);
		using var reader = new BinaryReader(stream);
		for (int i = 0; i < WAVE_LEN && reader.NotEnd(); i++) {
			Data[i] = reader.ReadByte();
		}
	}


	public void SaveToFile (string path) {
		using var stream = File.Create(path);
		using var writer = new BinaryWriter(stream);
		for (int i = 0; i < WAVE_LEN; i++) {
			writer.Write((byte)Data[i]);
		}
	}


	public static unsafe bool CreateWavForAudioFile (string audioFilePath, string waveFilePath, long date, out Wave result) {

		result = null;
		if (!Util.FileExists(audioFilePath)) return false;
		var wave = Raylib.LoadWave(audioFilePath);
		if (!Raylib.IsWaveValid(wave)) {
			Debug.LogWarning($"Fail to load wave data: {audioFilePath}");
			return false;
		}
		if (wave.SampleCount == 0 || wave.Channels == 0) return false;

		// Calculate Wave and Write
		float* samples = Raylib.LoadWaveSamples(wave);
		if (string.IsNullOrEmpty(waveFilePath)) return false;
		Util.CreateFolder(Util.GetParentPath(waveFilePath));
		using var stream = File.Create(waveFilePath);
		using var writer = new BinaryWriter(stream);
		result = new Wave();
		try {
			Debug.Log(
				$"from:\"{audioFilePath}\"",
				$"\nto:\"{waveFilePath}\"",
				$"\nch:{wave.Channels},",
				$"count:{wave.SampleCount},",
				$"rate:{wave.SampleRate}\n"
			);
			int sCount = (int)wave.SampleCount;
			int channelCount = (int)wave.Channels;
			int currentWaveIndex = 0;
			float currentWaveSample = 0f;

			for (int i = 0; i < sCount; i++) {
				// Get Sample
				float sample = 0f;
				for (int ch = 0; ch < channelCount; ch++) {
					float v = samples[i * channelCount + ch];
					sample = MathF.Max(MathF.Abs(v), sample);
				}
				// Merge Sample
				int waveIndex = i * WAVE_LEN / sCount;
				if (waveIndex == currentWaveIndex) {
					// Merge
					currentWaveSample = MathF.Max(sample, currentWaveSample);
					if (i == sCount - 1) {
						byte b = (byte)Math.Clamp(currentWaveSample * 255, 0, 255);
						writer.Write(b);
						result.Data[Math.Clamp(currentWaveIndex, 0, WAVE_LEN - 1)] = b;
					}
				} else {
					// Change
					currentWaveIndex = waveIndex;
					byte b = (byte)Math.Clamp(currentWaveSample * 255, 0, 255);
					writer.Write(b);
					result.Data[Math.Clamp(currentWaveIndex, 0, WAVE_LEN - 1)] = b;
					currentWaveSample = 0f;
				}
			}
		} catch (System.Exception ex) { Debug.LogException(ex); }

		// Final
		Util.SetFileCreateDate(waveFilePath, date);
		Raylib.UnloadWaveSamples(samples);
		Raylib.UnloadWave(wave);
		return true;
	}


	public static string GetWaveDataPath (string audioFilePath) {
		if (audioFilePath.Length <= 2 || audioFilePath[1] != ':') return "";
		audioFilePath = audioFilePath.Remove(1, 1);
		audioFilePath = Util.ChangeExtension(audioFilePath, "").TrimEnd('.');
		return Util.CombinePaths(WaveCacheRoot, audioFilePath);
	}


	#endregion




}
