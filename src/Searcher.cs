using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace QuickSound;


public class Searcher {




	#region --- SUB ---


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


	private enum PatternType { Contains, Remove, }


	#endregion




	#region --- VAR ---


	// Api
	public readonly List<SearchResultLine> SearchResults = [];
	public bool Imported { get; private set; } = false;
	public bool Importing { get; private set; } = false;
	public bool Searching { get; private set; } = false;
	public int ImportPathCount => ImportedPaths.Count;
	public string ImportingMsg { get; private set; } = "";
	public string AudioRootPath { get; private set; } = "";
	public string SearchedText { get; private set; } = "";

	// Data
	private readonly List<(string path, string name)> ImportedPaths = [];
	private readonly List<(string pat, PatternType type)> SearchPatterns = [];
	private int SearchStamp = 0;
	private string SavingFolderPath;


	#endregion




	#region --- API ---


	public void ImportAsync (string audioRoot, string savingRoot, bool forceImport) {
		if (Importing) return;
		Imported = false;
		AudioRootPath = audioRoot;
		SavingFolderPath = savingRoot;
		ImportedPaths.Clear();
		SearchResults.Clear();
		SearchStamp++;
		// Cache File
		long audioRootPathID = audioRoot.SuperAngeHash();
		string pathsCachePathRoot = Util.CombinePaths(savingRoot, "Paths");
		Util.CreateFolder(pathsCachePathRoot);
		if (forceImport) {
			string pathsCachePath = Util.CombinePaths(pathsCachePathRoot, $"{audioRootPathID.ToString()}.txt");
			if (Util.FileExists(pathsCachePath)) {
				File.Delete(pathsCachePath);
			}
		}
		// Run Background Task
		Task.Run(BackgroundImport);
	}


	public void PerformSearch (string searchingText) {
		if (!Imported) return;
		if (string.IsNullOrEmpty(AudioRootPath)) return;
		Searching = true;
		searchingText ??= "";
		SearchResults.Clear();
		SearchStamp++;
		SearchedText = searchingText;
		SearchPatterns.Clear();
		var patterns = searchingText.ToLower().Split(' ', System.StringSplitOptions.RemoveEmptyEntries) ?? [];
		foreach (string pat in patterns) {
			if (pat.StartsWith('-')) {
				SearchPatterns.Add((pat[1..], PatternType.Remove));
			} else {
				SearchPatterns.Add((pat, PatternType.Contains));
			}
		}
		Task.Run(BackgroundSearch);
	}


	public bool CheckAudioRoot (string audioRoot) => AudioRootPath == audioRoot;


	#endregion




	#region --- LGC ---


	private void BackgroundImport () {
		try {
			Importing = true;
			ImportedPaths.Clear();
			string pathsCachePathRoot = Util.CombinePaths(SavingFolderPath, "Paths");
			string pathsCachePath = Util.CombinePaths(pathsCachePathRoot, $"{AudioRootPath.SuperAngeHash()}.txt");
			if (Util.FileExists(pathsCachePath)) {
				// Load From Cache
				ImportingMsg = "Load From Cache File";
				using var stream = File.OpenRead(pathsCachePath);
				using var reader = new StreamReader(stream);
				while (reader.NotEnd()) {
					string path = reader.ReadLine();
					ImportedPaths.Add((path, Util.GetNameWithoutExtension(path).ToLower()));
				}
			} else {
				var dupPool = new Dictionary<string, List<string>>();
				// Search from Audio Root
				foreach (var path in Util.EnumerateFiles(AudioRootPath, false, true, "*.wav", "*.mp3", "*.ogg")) {
					// Check for Duplicate
					string name = Util.GetNameWithoutExtension(path);
					if (dupPool.TryGetValue(name, out List<string> dupPaths)) {
						bool isDup = false;
						foreach (string dupPath in dupPaths) {
							if (Util.GetExtensionWithDot(path) == Util.GetExtensionWithDot(dupPath)) continue;
							if (!Util.TryGetRelativePath(AudioRootPath, path, out string rPath)) continue;
							if (!Util.TryGetRelativePath(AudioRootPath, dupPath, out string rPathD)) continue;
							int indexRoot = rPath.IndexOf(Path.DirectorySeparatorChar);
							int indexRootD = rPathD.IndexOf(Path.DirectorySeparatorChar);
							if (indexRoot < 0 || indexRootD < 0) continue;
							if (rPath[..indexRoot] != rPathD[..indexRootD]) continue;
							isDup = true;
							break;
						}
						if (isDup) continue;
					} else {
						dupPool.Add(name, [path]);
					}
					// Add
					ImportedPaths.Add((path, Util.GetNameWithoutExtension(path).ToLower()));
					ImportingMsg = path;
				}
				// Save File
				using var stream = File.Create(pathsCachePath);
				using var writer = new StreamWriter(stream);
				ImportingMsg = "Saving Cache File";
				foreach (var (path, name) in ImportedPaths) {
					writer.WriteLine(path);
				}
			}
			// Final
			Imported = true;
			ImportingMsg = "";
			PerformSearch("");
		} catch (System.Exception ex) { LogEx(ex); }
		Importing = false;
	}


	private void BackgroundSearch () {
		Searching = true;
		try {
			int stamp = SearchStamp;
			bool hasRemove = false;
			foreach (var (_, type) in SearchPatterns) {
				if (type == PatternType.Remove) {
					hasRemove = true;
					break;
				}
			}
			// Search
			foreach (var (path, name) in ImportedPaths) {
				// Search Check
				if (SearchPatterns.Count > 0) {
					bool contains = false;
					bool remove = false;
					foreach (var (pat, type) in SearchPatterns) {
						switch (type) {
							case PatternType.Contains:
								if (name.Contains(pat)) contains = true;
								break;
							case PatternType.Remove:
								if (name.Contains(pat)) remove = true;
								break;
						}
						if (hasRemove) {
							if (remove) break;
						} else {
							if (contains) break;
						}
					}
					if (!contains || remove) continue;
				}
				if (stamp != SearchStamp) return;
				// Add Path
				SearchResults.Add(new SearchResultLine(path, AudioRootPath));
			}
		} catch (System.Exception ex) { LogEx(ex); }
		Searching = false;
	}


	private static void LogEx (System.Exception ex) {
		System.Console.BackgroundColor = System.ConsoleColor.Black;
		System.Console.ForegroundColor = System.ConsoleColor.Red;
		System.Console.WriteLine(ex.Source);
		System.Console.WriteLine(ex.GetType().Name);
		System.Console.WriteLine(ex.Message);
		System.Console.WriteLine(ex.StackTrace);
		System.Console.WriteLine();
		System.Console.ResetColor();
	}


	#endregion




}
