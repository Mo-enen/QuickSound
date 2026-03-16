using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RayFlow;

namespace QuickSound;

public static class WavePool {

	// Data
	private static readonly Dictionary<int, Wave> Pool = [];
	private static (int pathID, string path) MajorRequirement = (0, "");
	private static HashSet<int> NoCacheSet = [];

	// API
	public static void StartBackgroundLoop () => Task.Run(BackgroundRequiringLoop);

	public static bool TryGetWave (int pathID, out Wave wave) => Pool.TryGetValue(pathID, out wave);

	public static void RequireWave (int pathID, string audioPath) {
		if (Pool.ContainsKey(pathID)) return;
		// Cache Check
		if (!NoCacheSet.Contains(pathID)) {
			string dataPath = Wave.GetWaveDataPath(audioPath);
			if (Util.FileExists(dataPath)) {
				long dataDate = Util.GetFileCreationDate(dataPath);
				long audioDate = Util.GetFileModifyDate(audioPath);
				if (audioDate == dataDate) {
					// Load From Cache
					var wave = new Wave();
					wave.LoadFromFile(dataPath);
					Pool[pathID] = wave;
					return;
				}
			} else {
				// Create Cache
				NoCacheSet.Add(pathID);
			}
		}
		// Create Wave
		if (MajorRequirement.pathID != 0) return;
		Pool.Add(pathID, null);
		MajorRequirement = (pathID, audioPath);
	}

	// LGC
	private static void BackgroundRequiringLoop () {
		while (true) {
			try {
				if (MajorRequirement.pathID != 0) {
					string audioPath = MajorRequirement.path;
					int pathID = MajorRequirement.pathID;
					string dataPath = Wave.GetWaveDataPath(audioPath);
					long audioDate = Util.GetFileModifyDate(audioPath);
					if (Wave.CreateWavForAudioFile(audioPath, dataPath, audioDate, out var wave)) {
						Pool[pathID] = wave;
					}
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			MajorRequirement = (0, "");
			Thread.Sleep(5);
		}
	}

}
