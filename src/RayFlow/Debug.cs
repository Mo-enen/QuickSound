using System;
using System.Collections;
using System.Collections.Generic;

namespace RayFlow;

public static class Debug {


	// Log
	public static void Log (params object[] objs) {
		Console.ResetColor();
		foreach (var obj in objs) {
			Console.Write(obj != null ? obj.ToString() : "(null)");
			Console.Write(" ");
		}
		Console.WriteLine("");
	}
	public static void Log (object obj) => Log(obj != null ? obj.ToString() : "(null)");
	public static void Log (string msg) {
		Console.ResetColor();
		Console.WriteLine(msg);
	}


	// Warning
	public static void LogWarning (params object[] objs) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Yellow;
		foreach (var obj in objs) {
			Console.Write(obj != null ? obj.ToString() : "(null)");
			Console.Write(" ");
		}
		Console.WriteLine("");
		Console.ResetColor();
	}
	public static void LogWarning (object obj) => LogWarning(obj != null ? obj.ToString() : "(null)");
	public static void LogWarning (string msg) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine(msg);
		Console.ResetColor();
	}


	// Error
	public static void LogError (params object[] objs) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Red;
		foreach (var obj in objs) {
			Console.Write(obj != null ? obj.ToString() : "(null)");
			Console.Write(" ");
		}
		Console.WriteLine("");
		Console.ResetColor();
	}
	public static void LogError (object obj) => LogError(obj != null ? obj.ToString() : "(null)");
	public static void LogError (string msg) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(msg);
		Console.ResetColor();
	}


	// Success
	public static void LogSuccess (params object[] objs) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Green;
		foreach (var obj in objs) {
			Console.Write(obj != null ? obj.ToString() : "(null)");
			Console.Write(" ");
		}
		Console.WriteLine("");
		Console.ResetColor();
	}
	public static void LogSuccess (object obj) => LogSuccess(obj != null ? obj.ToString() : "(null)");
	public static void LogSuccess (string msg) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine(msg);
		Console.ResetColor();
	}


	// Exp
	public static void LogException (Exception ex) {
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(ex.Source);
		Console.WriteLine(ex.GetType().Name);
		Console.WriteLine(ex.Message);
		Console.WriteLine(ex.StackTrace);
		Console.WriteLine();
		Console.ResetColor();
	}


}
