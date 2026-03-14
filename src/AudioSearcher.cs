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
	public SearchResultLine (string path, string audioRootPath) {
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



public class AudioSearcher {

	// VAR
	private int SearchStamp = 0;
	private string[] SearchPatterns = null;
	public readonly List<SearchResultLine> SearchResults = [];

	// MSG
	public void PerformSearch (string searchingText, string audioRootPath) {

		searchingText ??= "";

		SearchResults.Clear();
		SearchStamp++;
		SearchPatterns = searchingText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
		Task.Run(BackgroundSearch);

		/////////////////////////////

		string testA = @"D:\C#\QuickSound\Build\Test Audio\8-Bit Adventure\LOOP_Chaos Powerhouse.wav";
		string testB = @"D:\C#\QuickSound\Build\Test Audio\8-Bit Adventure\LOOP_Feel-Good Victory.wav";
		string testC = @"D:\C#\QuickSound\Build\Test Audio\8-Bit Adventure\LOOP_Mysterious Cave.wav";
		string testD = @"D:\C#\QuickSound\Build\Test Audio\99 Sound Effects\Drone - Alien VHS.wav";
		string testE = @"D:\C#\QuickSound\Build\Test Audio\99 Sound Effects\Drone - Ocean Cave.wav";
		string testF = @"D:\C#\QuickSound\Build\Test Audio\99 Sound Effects\Impact - Cease Fire.wav";
		string testG = @"D:\C#\QuickSound\Build\Test Audio\99 Sound Effects\Impact - Jaw Breaker.wav";
		SearchResults.Add(new SearchResultLine(testA, audioRootPath));
		SearchResults.Add(new SearchResultLine(testB, audioRootPath));
		SearchResults.Add(new SearchResultLine(testC, audioRootPath));
		SearchResults.Add(new SearchResultLine(testD, audioRootPath));
		SearchResults.Add(new SearchResultLine(testE, audioRootPath));
		SearchResults.Add(new SearchResultLine(testF, audioRootPath));
		SearchResults.Add(new SearchResultLine(testG, audioRootPath));

		/////////////////////////////

	}

	private void BackgroundSearch () {

		int stamp = SearchStamp;



		// Change Check
		if (stamp != SearchStamp) {

		}

		// Search




	}

}
