using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RayFlow;

namespace QuickSound;

public static class WavePool {

	// Api
	public static int CurrentRequireCount => WaveRequiring.Count;

	// Data
	private static readonly Dictionary<int, Wave> Pool = [];
	private static readonly Queue<(int pathID, string audioFilePath)> WaveRequiring = [];

	// API
	public static void StartBackgroundLoop () => Task.Run(BackgroundRequiringLoop);

	public static bool TryGetWave (int pathID, out Wave wave) => Pool.TryGetValue(pathID, out wave) && wave != null;

	public static void RequireWave (int pathID, string audioFilePath) {
		if (Pool.ContainsKey(pathID)) return;
		Pool.Add(pathID, null);
		WaveRequiring.Enqueue((pathID, audioFilePath));
	}

	// LGC
	private static void BackgroundRequiringLoop () {
		while (true) {
			try {
				while (WaveRequiring.TryDequeue(out var requirData)) {
					var (pathID, audioPath) = requirData;
					string dataPath = Wave.GetWaveDataPath(audioPath);
					if (Util.FileExists(dataPath)) {
						var wave = new Wave();
						wave.LoadFromFile(dataPath);
						Pool[pathID] = wave;
					} else if (Wave.CreateWavForAudioFile(audioPath, dataPath, out var wave)) {
						Pool[pathID] = wave;
					}
				}
				Thread.Sleep(20);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}

}
