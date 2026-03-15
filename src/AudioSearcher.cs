using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RayFlow;

namespace QuickSound;


public class SearchResultLine {
	public string BaseName;
	public string Name;
	public string Path;
	public int PathID;
	public float StartTime01;
	public float EndTime01;
	public SearchResultLine (string path, string audioRootPath) {
		Path = path;
		PathID = path.AngeHash();
		Name = Util.GetNameWithoutExtension(path);
		if (Util.TryGetRelativePath(audioRootPath, Util.GetParentPath(path), out string rPath)) {
			BaseName = rPath;
		} else {
			BaseName = Util.GetParentPath(Path);
		}
		StartTime01 = 0f;
		EndTime01 = 1f;
	}
}



public class AudioSearcher {


	// Api
	public readonly List<SearchResultLine> SearchResults = [];
	public bool Imported { get; private set; } = false;
	public bool Importing { get; private set; } = false;
	public int ImportPathCount { get; private set; } = 0;
	public string LastImportedPath { get; private set; } = "";
	public string AudioRootPath { get; private set; } = "";

	// Data
	private int SearchStamp = 0;
	private string[] SearchPatterns = null;

	// API
	public void ImportAsync (string audioRoot, bool forceImport) {
		if (Importing) return;
		Imported = false;
		AudioRootPath = audioRoot;
		SearchResults.Clear();
		SearchStamp++;
		if (forceImport) {

			// TODO
			// Remove Cache File

		}
		Task.Run(BackgroundImport);
	}

	public void PerformSearch (string searchingText) {

		if (!Imported) return;
		if (string.IsNullOrEmpty(AudioRootPath)) return;

		searchingText ??= "";

		SearchResults.Clear();
		SearchStamp++;
		SearchPatterns = searchingText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
		Task.Run(BackgroundSearch);

		/////////////////////////////

		string testA = @"E:\Audio\8-Bit Adventure\LOOP_Chaos Powerhouse.wav";
		string testB = @"E:\Audio\8-Bit Adventure\LOOP_Feel-Good Victory.wav";
		string testC = @"E:\Audio\8-Bit Adventure\LOOP_Mysterious Cave.wav";
		string testD = @"E:\Audio\99 Sound Effects\WAV\Drone - Alien VHS.wav";
		string testE = @"E:\Audio\99 Sound Effects\WAV\Drone - Ocean Cave.wav";
		string testF = @"E:\Audio\99 Sound Effects\WAV\Impact - Cease Fire.wav";
		string testG = @"E:\Audio\99 Sound Effects\WAV\Impact - Jaw Breaker.wav";
		SearchResults.Add(new SearchResultLine(testA, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testB, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testC, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testD, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testE, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testF, AudioRootPath));
		SearchResults.Add(new SearchResultLine(testG, AudioRootPath));

		/////////////////////////////

	}

	// LGC
	private void BackgroundImport () {
		Importing = true;
		ImportPathCount = 0;
		try {




			// Final
			Imported = true;
			PerformSearch("");
		} catch (System.Exception ex) { Debug.LogException(ex); }
		Importing = false;
	}

	private void BackgroundSearch () {

		int stamp = SearchStamp;




		// Change Check
		if (stamp != SearchStamp) return;


		// Search




	}


}
