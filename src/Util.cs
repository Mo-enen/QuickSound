using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace QuickSound;

static partial class Util {

	// File
	public static void TextToFile (string data, string path, Encoding encoding, bool append = false) {
		CreateFolder(GetParentPath(path));
		using FileStream fs = new(path, append ? FileMode.Append : FileMode.Create);
		using StreamWriter sw = new(fs, encoding);
		sw.Write(data);
		fs.Flush();
		sw.Close();
		fs.Close();
	}

	public static IEnumerable<string> ForAllLinesInFile (string path, Encoding encoding) {
		if (!FileExists(path)) yield break;
		using StreamReader sr = new(path, encoding);
		while (sr.Peek() >= 0) yield return sr.ReadLine();
	}

	public static void CreateFolder (string path) {
		if (string.IsNullOrEmpty(path) || FolderExists(path)) return;
		string pPath = GetParentPath(path);
		if (!FolderExists(pPath)) {
			CreateFolder(pPath);
		}
		Directory.CreateDirectory(path);
	}

	public static IEnumerable<string> EnumerateFiles (string path, bool topOnly, string searchPattern) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateFiles(path, searchPattern, option)) {
			yield return str;
		}
	}
	public static IEnumerable<string> EnumerateFiles (string path, bool topOnly, params string[] searchPatterns) {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		if (searchPatterns == null || searchPatterns.Length == 0) {
			foreach (var filePath in Directory.EnumerateFiles(path, "*", option)) {
				yield return filePath;
			}
		} else {
			foreach (var pattern in searchPatterns) {
				foreach (var filePath in Directory.EnumerateFiles(path, pattern, option)) {
					yield return filePath;
				}
			}
		}
	}

	public static IEnumerable<string> EnumerateFolders (string path, bool topOnly, string searchPattern = "*") {
		if (!FolderExists(path)) yield break;
		var option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
		foreach (string str in Directory.EnumerateDirectories(path, searchPattern, option)) {
			yield return str;
		}
	}

	public static bool CopyFolder (string from, string to, bool copySubDirs, bool ignoreHidden, bool overrideFile = false) {

		// Get the subdirectories for the specified directory.
		DirectoryInfo dir = new(from);

		if (!dir.Exists) return false;

		DirectoryInfo[] dirs = dir.GetDirectories();
		// If the destination directory doesn't exist, create it.
		if (!Directory.Exists(to)) {
			Directory.CreateDirectory(to);
		}

		// Get the files in the directory and copy them to the new location.
		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files) {
			try {
				string tempPath = Path.Combine(to, file.Name);
				if (!ignoreHidden || (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
					file.CopyTo(tempPath, overrideFile);
				}
			} catch { }
		}

		// If copying subdirectories, copy them and their contents to new location.
		if (copySubDirs) {
			foreach (DirectoryInfo subdir in dirs) {
				try {
					string temppath = Path.Combine(to, subdir.Name);
					if (!ignoreHidden || (subdir.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) {
						CopyFolder(subdir.FullName, temppath, copySubDirs, ignoreHidden, overrideFile);
					}
				} catch { }
			}
		}
		return true;
	}

	// File-Date
	public static long GetFileModifyDate (string path) {
		if (!FileExists(path)) return 0;
		return File.GetLastWriteTime(path).ToFileTimeUtc();
	}

	public static long GetFolderModifyDate (string path) {
		if (!FolderExists(path)) return 0;
		return Directory.GetLastWriteTime(path).ToFileTimeUtc();
	}

	public static void SetFolderModifyDate (string path, long fileTime) {
		if (!FolderExists(path)) return;
		Directory.SetLastWriteTime(path, System.DateTime.FromFileTimeUtc(fileTime));
	}

	public static long GetFileCreationDate (string path) {
		if (!FileExists(path)) return 0;
		return File.GetCreationTime(path).ToFileTimeUtc();
	}

	public static long GetFolderCreationDate (string path) {
		if (!FolderExists(path)) return 0;
		return Directory.GetCreationTime(path).ToFileTimeUtc();
	}

	public static void SetFileModifyDate (string path, long fileTime) {
		if (!FileExists(path)) return;
		File.SetLastWriteTime(path, System.DateTime.FromFileTime(fileTime));
	}

	public static long GetCurrentFileTime () => System.DateTime.UtcNow.ToFileTimeUtc();

	public static bool NotEnd (this BinaryReader reader) => reader.BaseStream.Position < reader.BaseStream.Length;

	// Path
	public static string GetParentPath (string path) {
		if (string.IsNullOrEmpty(path)) return "";
		var info = Directory.GetParent(path);
		return info != null ? info.FullName : "";
	}

	public static string CombinePaths (string path1, string path2) => Path.Combine(path1, path2);
	public static string CombinePaths (string path1, string path2, string path3) => Path.Combine(path1, path2, path3);

	public static string GetExtensionWithDot (string path) => Path.GetExtension(path);//.txt

	public static string GetNameWithoutExtension (string path) => Path.GetFileNameWithoutExtension(path);

	public static bool FolderExists (string path) => Directory.Exists(path);

	public static bool FileExists (string path) => !string.IsNullOrEmpty(path) && File.Exists(path);

	public static string GetDisplayName (string name) {

		// Remove "m_" at Start
		if (name.Length > 2 && name[0] == 'm' && name[1] == '_') {
			name = name[2..];
		}

		// Replace "_" to " "
		name = name.Replace('_', ' ');

		// Add " " Space Between "a Aa"
		for (int i = 0; i < name.Length - 1; i++) {
			char a = name[i];
			char b = name[i + 1];
			if (
				char.IsLetter(a) &&
				(char.IsLetter(b) || char.IsNumber(b)) &&
				!char.IsUpper(a) &&
				(char.IsUpper(b) || char.IsNumber(b))
			) {
				name = name.Insert(i + 1, " ");
				i++;
			}
		}

		return name;
	}

	public static bool TryGetRelativePath (string relativeTo, string path, out string relativePath) {
		try {
			relativePath = Path.GetRelativePath(relativeTo, path);
			return true;
		} catch (System.Exception ex) { Debug.LogException(ex); }
		relativePath = "";
		return false;
	}

	public static string ChangeExtension (string path, string newEx) => Path.ChangeExtension(path, newEx);//txt or .txt

	// AngeliA Hash Code
	public static string AngeName (this System.Type type) {
		string name = type.IsNested ? $"{type.DeclaringType.Name}.{type.Name}" : type.Name;
		if (char.IsLower(name[0])) name = name[1..];
		return name;
	}

	public static int AngeHash (this System.Type type) => type.AngeName().AngeHash();

	public static int AngeHash (this string str) {
		const int p = 31;
		const int m = 1837465129;
		int hash_value = 0;
		int p_pow = 1;
		for (int i = 0; i < str.Length; i++) {
			char c = str[i];
			hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
			p_pow = (p_pow * p) % m;
		}
		return hash_value == 0 ? 1 : hash_value;
	}

	public static int AngeHash (this string str, int start, int length) {
		const int p = 31;
		const int m = 1837465129;
		int hash_value = 0;
		int p_pow = 1;
		int end = start + length;
		int arrLen = str.Length;
		for (int i = start; i < end; i++) {
			char c = str[i % arrLen];
			hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
			p_pow = (p_pow * p) % m;
		}
		return hash_value == 0 ? 1 : hash_value;
	}

	public static int AngeHash (this char[] arr, int start, int length) {
		const int p = 31;
		const int m = 1837465129;
		int hash_value = 0;
		int p_pow = 1;
		int end = start + length;
		int arrLen = arr.Length;
		for (int i = start; i < end; i++) {
			char c = arr[i % arrLen];
			hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
			p_pow = (p_pow * p) % m;
		}
		return hash_value == 0 ? 1 : hash_value;
	}

	public static int AngeReverseHash (this char[] arr, int start, int length) {
		const int p = 31;
		const int m = 1837465129;
		int hash_value = 0;
		int p_pow = 1;
		int end = start + length;
		int arrLen = arr.Length;
		for (int i = end - 1; i >= start; i--) {
			char c = arr[i % arrLen];
			hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
			p_pow = (p_pow * p) % m;
		}
		return hash_value == 0 ? 1 : hash_value;
	}

}