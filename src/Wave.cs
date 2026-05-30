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
	private static readonly byte[] DEFAULT_DATA = [0];
	private const int SCL = 60;

	// Api
	public static string WaveCacheRoot = "";
	public byte[] Data { get; private set; } = DEFAULT_DATA;


	#endregion




	#region --- API ---


	public void LoadFromFile (string path) {
		using var stream = File.OpenRead(path);
		using var reader = new BinaryReader(stream);
		int waveLen = reader.ReadInt32();
		Data = new byte[waveLen];
		for (int i = 0; i < waveLen; i++) {
			Data[i] = reader.ReadByte();
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

		float* samples = Raylib.LoadWaveSamples(wave);

		try {
			// Calculate Wave and Write
			if (string.IsNullOrEmpty(waveFilePath)) {
				Raylib.UnloadWave(wave);
				return false;
			}
			Util.CreateFolder(Util.GetParentPath(waveFilePath));
			using var stream = File.Create(waveFilePath);
			using var writer = new BinaryWriter(stream);
			result = new Wave();
			int sCount = (int)wave.SampleCount;
			int channelCount = (int)wave.Channels;
			int currentWaveIndex = 0;
			float currentWaveSample = 0f;
			int WAVE_LEN = Util.Max(sCount / SCL, 16);
			writer.Write((int)WAVE_LEN);
			result.Data = new byte[WAVE_LEN];
			for (int i = 0; i < sCount; i++) {
				// Get Sample
				float sample = 0f;
				for (int ch = 0; ch < channelCount; ch++) {
					float v = samples[i * channelCount + ch];
					sample = MathF.Max(MathF.Abs(v), sample);
				}
				// Merge Sample
				int waveIndex = (int)((double)i * WAVE_LEN / sCount);
				if (waveIndex == currentWaveIndex) {
					// Merge
					currentWaveSample = MathF.Max(sample, currentWaveSample);
					if (i == sCount - 1) {
						byte b = (byte)Math.Clamp(currentWaveSample * 255, 0, 255);
						result.Data[Math.Clamp(currentWaveIndex, 0, WAVE_LEN - 1)] = b;
					}
				} else {
					// Change
					currentWaveIndex = waveIndex;
					byte b = (byte)Math.Clamp(currentWaveSample * 255, 0, 255);
					result.Data[Math.Clamp(currentWaveIndex, 0, WAVE_LEN - 1)] = b;
					currentWaveSample = 0f;
				}
			}
			// Write To File
			for (int i = 0; i < WAVE_LEN; i++) {
				writer.Write((byte)result.Data[i]);
			}
#if DEBUG
			// Msg
			Debug.LogSuccess("Wave Loaded:", Util.GetNameWithoutExtension(audioFilePath));
			Debug.LogSuccess("ChannelCount:", channelCount);
			Debug.LogSuccess("SampleCount:", sCount, "\n");
#endif
		} catch (System.Exception ex) { Debug.LogException(ex); }

		// Final
		Util.SetFileCreateDate(waveFilePath, date);
		Raylib.UnloadWaveSamples(samples);
		Raylib.UnloadWave(wave);
		return true;
	}


	public static string GetWaveDataPath (string audioFilePath) {
		if (audioFilePath.Length <= 2 || audioFilePath[1] != ':') return "";
		long superHash = audioFilePath.SuperAngeHash();
		return Util.CombinePaths(WaveCacheRoot, superHash.ToString());
	}


	#endregion




}
