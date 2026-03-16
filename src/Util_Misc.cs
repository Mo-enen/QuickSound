using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickSound;

public static partial class Util {


	// VAR
	private const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct MathfInternal {
		public static volatile float FloatMinNormal = 1.17549435E-38f;
		public static volatile float FloatMinDenormal = float.Epsilon;
		public static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;
	}

	public const float Rad2Deg = 57.29578f;
	public const float Deg2Rad = PI / 180f;
	public const float PI = 3.14159274f;
	internal static readonly float Epsilon = MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal;
	private static int QuickRandomSeed = 73633632;




	// Misc
	public static int GetAngeHashForClassName (string className) {
		if (string.IsNullOrEmpty(className)) return 0;
		if (char.IsLower(className[0])) className = className[1..];
		return className.AngeHash();
	}


	public static void AddEnvironmentVariable (string key, string value) {
		string oldPath = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process) ?? "";
		Environment.SetEnvironmentVariable(
			key, oldPath.Insert(0, $"{value};"), System.EnvironmentVariableTarget.Process
		);
	}


	public static bool TryGetIntFromString (string str, int startIndex, out int value, out int endIndex) {
		value = 0;
		for (endIndex = startIndex; endIndex < str.Length; endIndex++) {
			char c = str[endIndex];
			if (!char.IsNumber(c)) break;
			value = value * 10 + (c - '0');
		}
		return startIndex != endIndex;
	}


	public static void QuickSort<T> (T[] array, int min, int max, IComparer<T> comparer) {
		int lo = min;
		int hi = max;
		T pvt = array[(min + max) / 2];
		while (lo <= hi) {
			while (comparer.Compare(array[lo], pvt) < 0) lo++;
			while (comparer.Compare(array[hi], pvt) > 0) hi--;
			if (lo > hi) break;
			(array[lo], array[hi]) = (array[hi], array[lo]);
			lo++;
			hi--;
		}
		if (min < hi) QuickSort(array, min, hi, comparer);
		if (lo < max) QuickSort(array, lo, max, comparer);
	}
	public static void QuickSort<T> (Span<T> array, int min, int max, IComparer<T> comparer) {
		int lo = min;
		int hi = max;
		T pvt = array[(min + max) / 2];
		while (lo <= hi) {
			while (comparer.Compare(array[lo], pvt) < 0) lo++;
			while (comparer.Compare(array[hi], pvt) > 0) hi--;
			if (lo > hi) break;
			(array[lo], array[hi]) = (array[hi], array[lo]);
			lo++;
			hi--;
		}
		if (min < hi) QuickSort(array, min, hi, comparer);
		if (lo < max) QuickSort(array, lo, max, comparer);
	}


	public static int QuickRandom (int min, int max) => (QuickRandomSeed = QuickRandomWithSeed(QuickRandomSeed)).UMod((max - min).GreaterOrEquel(1)) + min;
	public static float QuickRandomF (float min, float max) => LerpUnclamped(min, max, QuickRandom().Abs() / (float)int.MaxValue);
	public static int QuickRandom () => QuickRandomSeed = QuickRandomWithSeed(QuickRandomSeed);
	public static int QuickRandomSign () => QuickRandom() % 2 == 0 ? 1 : -1;
	public static float QuickRandomWithSeed01 (int seed) => QuickRandomWithSeed(seed).Abs() / (float)int.MaxValue;
	public static int QuickRandomWithSeed (int seed, int min, int max) => QuickRandomWithSeed(seed).UMod((max - min).GreaterOrEquel(1)) + min;
	public static int QuickRandomWithSeed (int seed) {
		seed = (seed * 1103515245 + 12345) % 23456789;
		seed = (seed * 16807) % 2147483647;
		seed = (seed ^ (seed >> 16)) % 2147483647;
		seed = (seed * 2127912213) % 2147483647;
		return seed;
	}
	public static int QuickRandomWithSeed (long seed, int min, int max) => QuickRandomWithSeed(seed).UMod((max - min).GreaterOrEquel(1)) + min;
	public static int QuickRandomWithSeed (long seed) {
		seed = (seed * 12234503515245 + 72456224) % 2223423456789;
		seed = (seed * 168689307) % 21470543323483647;
		seed = (seed ^ (seed >> 23)) % 4243214724483647;
		seed = (seed * 212791213672213) % 214748223573647;
		return (int)seed;
	}


	public static int FindNextStringStep (string content, int start, bool toRight) {
		int result = start;
		int delta = toRight ? 1 : -1;
		bool flag = false;
		if (string.IsNullOrEmpty(content)) return -1;
		start = start.Clamp(0, content.Length - 1);
		for (int i = start; i < content.Length && i >= 0; i += delta) {
			result = i;
			char c = content[i];
			if (char.IsWhiteSpace(c) || (!char.IsLetter(c) && !char.IsNumber(c) && c != '_')) {
				if (flag) return i;
			} else {
				flag = true;
			}
		}
		return result + (toRight ? 1 : 0);
	}




	[MethodImpl(INLINE)]
	public static float Remap (float l, float r, float newL, float newR, float t) => l == r ? newL : Lerp(newL, newR, (t - l) / (r - l));


	[MethodImpl(INLINE)]
	public static float RemapUnclamped (float l, float r, float newL, float newR, float t) => l == r ? newL : newL + (newR - newL) * ((t - l) / (r - l));

	[MethodImpl(INLINE)]
	public static int Remap (int l, int r, int newL, int newR, int t) {
		int rangeA = newL;
		int rangeB = newR;
		if (rangeA > rangeB) {
			(rangeA, rangeB) = (rangeB, rangeA);
		}
		return RemapUnclamped(l, r, newL, newR, t).Clamp(rangeA, rangeB);
	}

	public static int RemapUnclamped (int l, int r, int newL, int newR, int t) {
		if (l == r) return newL;
		int deltaNew = newR - newL;
		int deltaT = t - l;
		int deltaR = r - l;
		try {
			return checked(newL + deltaNew * deltaT / deltaR);
		} catch {
			if (deltaNew.Abs() > deltaT.Abs()) {
				return newL + deltaNew / deltaR * deltaT;
			} else {
				return newL + deltaT / deltaR * deltaNew;
			}
		}
	}

	public static long RemapUnclamped (long l, long r, long newL, long newR, long t) {
		if (l == r) return newL;
		long deltaNew = newR - newL;
		long deltaT = t - l;
		long deltaR = r - l;
		try {
			return checked(newL + deltaNew * deltaT / deltaR);
		} catch {
			if (deltaNew.Abs() > deltaT.Abs()) {
				return newL + deltaNew / deltaR * deltaT;
			} else {
				return newL + deltaT / deltaR * deltaNew;
			}
		}
	}


	[MethodImpl(INLINE)]
	public static float InverseLerp (float from, float to, float value) {
		if (from != to) {
			return ((value - from) / (to - from)).Clamp01();
		}
		return 0f;
	}

	[MethodImpl(INLINE)]
	public static float InverseLerpUnclamped (float from, float to, float value) {
		if (from != to) {
			return (value - from) / (to - from);
		}
		return 0f;
	}

	[MethodImpl(INLINE)]
	public static float PingPong (float t, float length) {
		t = Repeat(t, length * 2f);
		return length - (t - length).Abs();
	}

	[MethodImpl(INLINE)] public static float Repeat (float t, float length) => (t - (t / length).FloorToInt() * length).Clamp(0, length);

	[MethodImpl(INLINE)] public static float Lerp (float a, float b, float t) => a + (b - a) * t.Clamp01();

	[MethodImpl(INLINE)] public static float LerpUnclamped (float a, float b, float t) => a + (b - a) * t;

	[MethodImpl(INLINE)]
	public static float LerpAngle (float a, float b, float t) {
		float delta = Repeat(b - a, 360);
		if (delta > 180)
			delta -= 360;
		return a + delta * Clamp01(t);
	}

	[MethodImpl(INLINE)]
	public static float LerpAngleUnclamped (float a, float b, float t) {
		float delta = Repeat(b - a, 360);
		if (delta > 180)
			delta -= 360;
		return a + delta * t;
	}


	[MethodImpl(INLINE)] public static float Atan (float x, float y) => (float)Math.Atan2(y, x) * Rad2Deg;


	// Min Int
	[MethodImpl(INLINE)]
	public static int Min (int a, int b) => (a < b) ? a : b;
	[MethodImpl(INLINE)]
	public static int Min (int a, int b, int c) {
		int ab = (a < b) ? a : b;
		return (ab < c) ? ab : c;
	}
	[MethodImpl(INLINE)]
	public static int Min (int a, int b, int c, int d) {
		int ab = (a < b) ? a : b;
		int abc = (ab < c) ? ab : c;
		return (abc < d) ? abc : d;
	}
	[MethodImpl(INLINE)]
	public static int Min (int a, int b, int c, int d, int e) {
		int ab = (a < b) ? a : b;
		int abc = (ab < c) ? ab : c;
		int abcd = (abc < d) ? abc : d;
		return (abcd < e) ? abcd : e;
	}
	[MethodImpl(INLINE)]
	public static int Min (int a, int b, int c, int d, int e, int f) {
		int ab = (a < b) ? a : b;
		int abc = (ab < c) ? ab : c;
		int abcd = (abc < d) ? abc : d;
		int abcde = (abcd < e) ? abcd : e;
		return (abcde < f) ? abcde : f;
	}


	// Min Float
	[MethodImpl(INLINE)]
	public static float Min (float a, float b) => (a < b) ? a : b;
	[MethodImpl(INLINE)]
	public static float Min (float a, float b, float c) {
		float ab = (a < b) ? a : b;
		return (ab < c) ? ab : c;
	}
	[MethodImpl(INLINE)]
	public static float Min (float a, float b, float c, float d) {
		float ab = (a < b) ? a : b;
		float abc = (ab < c) ? ab : c;
		return (abc < d) ? abc : d;
	}
	[MethodImpl(INLINE)]
	public static float Min (float a, float b, float c, float d, float e) {
		float ab = (a < b) ? a : b;
		float abc = (ab < c) ? ab : c;
		float abcd = (abc < d) ? abc : d;
		return (abcd < e) ? abcd : e;
	}
	[MethodImpl(INLINE)]
	public static float Min (float a, float b, float c, float d, float e, float f) {
		float ab = (a < b) ? a : b;
		float abc = (ab < c) ? ab : c;
		float abcd = (abc < d) ? abc : d;
		float abcde = (abcd < e) ? abcd : e;
		return (abcde < f) ? abcde : f;
	}


	// Max Int
	[MethodImpl(INLINE)]
	public static int Max (int a, int b) => (a > b) ? a : b;
	[MethodImpl(INLINE)]
	public static int Max (int a, int b, int c) {
		int ab = (a > b) ? a : b;
		return (ab > c) ? ab : c;
	}


	// Max Float
	[MethodImpl(INLINE)]
	public static float Max (float a, float b) => (a > b) ? a : b;
	[MethodImpl(INLINE)]
	public static float Max (float a, float b, float c) {
		float ab = (a > b) ? a : b;
		return (ab > c) ? ab : c;
	}
	[MethodImpl(INLINE)]
	public static float Max (float a, float b, float c, float d) {
		float ab = (a > b) ? a : b;
		float abc = (ab > c) ? ab : c;
		return (abc > d) ? abc : d;
	}
	[MethodImpl(INLINE)]
	public static float Max (float a, float b, float c, float d, float e) {
		float ab = (a > b) ? a : b;
		float abc = (ab > c) ? ab : c;
		float abcd = (abc > d) ? abc : d;
		return (abcd > e) ? abcd : e;
	}
	[MethodImpl(INLINE)]
	public static float Max (float a, float b, float c, float d, float e, float f) {
		float ab = (a > b) ? a : b;
		float abc = (ab > c) ? ab : c;
		float abcd = (abc > d) ? abc : d;
		float abcde = (abcd > e) ? abcd : e;
		return (abcde > f) ? abcde : f;
	}




	// 
	[MethodImpl(INLINE)] public static float Sin (float radAngle) => (float)Math.Sin(radAngle);

	[MethodImpl(INLINE)] public static float Cos (float radAngle) => (float)Math.Cos(radAngle);

	[MethodImpl(INLINE)] public static float Tan (float radAngle) => (float)Math.Tan(radAngle);

	[MethodImpl(INLINE)] public static int Abs (int value) => value > 0 ? value : -value;

	[MethodImpl(INLINE)] public static float Abs (float value) => value > 0f ? value : -value;

	[MethodImpl(INLINE)] public static bool Approximately (float a, float b) => Abs(b - a) < Max(1E-06f * Max(Abs(a), Abs(b)), Epsilon * 8f);

	[MethodImpl(INLINE)] public static int Clamp (int a, int min, int max) => a < min ? min : a > max ? max : a;

	[MethodImpl(INLINE)] public static float Clamp (float a, float min, float max) => a < min ? min : a > max ? max : a;

	[MethodImpl(INLINE)] public static float Clamp01 (float value) => value < 0f ? 0f : value > 1f ? 1f : value;

	[MethodImpl(INLINE)] public static float Pow (float f, float p) => (float)Math.Pow(f, p);

	[MethodImpl(INLINE)] public static float Sqrt (float f) => (float)Math.Sqrt(f);

	[MethodImpl(INLINE)] public static int RoundToInt (float value) => (int)Math.Round(value);

	[MethodImpl(INLINE)] public static int CeilToInt (float value) => (int)Math.Ceiling(value);

	[MethodImpl(INLINE)] public static int FloorToInt (float value) => (int)Math.Floor(value);



	// Pointer
	public static unsafe byte ReadByte (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadByte(ref p);
	}
	public static unsafe sbyte ReadSByte (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadSByte(ref p);
	}
	public static unsafe bool ReadBool (ref byte* p, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		return ReadBool(ref p);
	}
	public static unsafe char ReadChar (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadChar(ref p);
	}
	public static unsafe short ReadShort (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadShort(ref p);
	}
	public static unsafe ushort ReadUShort (ref byte* p, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		return ReadUShort(ref p);
	}
	public static unsafe int ReadInt (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadInt(ref p);
	}
	public static unsafe uint ReadUInt (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadUInt(ref p);
	}
	public static unsafe float ReadFloat (ref byte* p, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		return ReadFloat(ref p);
	}
	public static unsafe long ReadLong (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadLong(ref p);
	}
	public static unsafe ulong ReadULong (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadULong(ref p);
	}
	public static unsafe double ReadDouble (ref byte* p, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		return ReadDouble(ref p);
	}
	public static unsafe byte[] ReadBytes (ref byte* p, int length, byte* end) {
		if (p > end - length + 1) throw new IndexOutOfRangeException();
		return ReadBytes(ref p, length);
	}

	public static unsafe byte ReadByte (ref byte* p) {
		byte result = *p;
		p++;
		return result;
	}
	public static unsafe sbyte ReadSByte (ref byte* p) {
		sbyte result = *(sbyte*)p;
		p++;
		return result;
	}
	public static unsafe bool ReadBool (ref byte* p) {
		bool result = *p == 1;
		p++;
		return result;
	}
	public static unsafe char ReadChar (ref byte* p) {
		char result = *(char*)p;
		p += 2;
		return result;
	}
	public static unsafe short ReadShort (ref byte* p) {
		short result = *(short*)p;
		p += 2;
		return result;
	}
	public static unsafe ushort ReadUShort (ref byte* p) {
		ushort result = *(ushort*)p;
		p += 2;
		return result;
	}
	public static unsafe int ReadInt (ref byte* p) {
		int result = *(int*)p;
		p += 4;
		return result;
	}
	public static unsafe uint ReadUInt (ref byte* p) {
		uint result = *(uint*)p;
		p += 4;
		return result;
	}
	public static unsafe float ReadFloat (ref byte* p) {
		float result = *(float*)p;
		p += 4;
		return result;
	}
	public static unsafe long ReadLong (ref byte* p) {
		long result = *(long*)p;
		p += 8;
		return result;
	}
	public static unsafe ulong ReadULong (ref byte* p) {
		ulong result = *(ulong*)p;
		p += 8;
		return result;
	}
	public static unsafe double ReadDouble (ref byte* p) {
		double result = *(double*)p;
		p += 8;
		return result;
	}
	public static unsafe byte[] ReadBytes (ref byte* p, int length) {
		var result = new byte[length];
		for (int i = 0; i < length; i++) {
			result[i] = *p;
			p++;
		}
		return result;
	}


	public static unsafe void Write (ref byte* p, byte value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, sbyte value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, bool value, byte* end) {
		if (p > end) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, char value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, short value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, ushort value, byte* end) {
		if (p > end - 1) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, int value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, uint value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, float value, byte* end) {
		if (p > end - 3) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, long value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, ulong value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, double value, byte* end) {
		if (p > end - 7) throw new IndexOutOfRangeException();
		Write(ref p, value);
	}
	public static unsafe void Write (ref byte* p, byte[] bytes, int length, byte* end) {
		if (p > end - length + 1) throw new IndexOutOfRangeException();
		Write(ref p, bytes, length);
	}


	public static unsafe void Write (ref byte* p, byte value) {
		*p = value;
		p++;
	}
	public static unsafe void Write (ref byte* p, sbyte value) {
		var _p = (sbyte*)p;
		*_p = value;
		p++;
	}
	public static unsafe void Write (ref byte* p, bool value) {
		*p = (byte)(value ? 1 : 0);
		p++;
	}
	public static unsafe void Write (ref byte* p, char value) {
		var _p = (char*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, short value) {
		var _p = (short*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, ushort value) {
		var _p = (ushort*)p;
		*_p = value;
		p += 2;
	}
	public static unsafe void Write (ref byte* p, int value) {
		var _p = (int*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, uint value) {
		var _p = (uint*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, float value) {
		var _p = (float*)p;
		*_p = value;
		p += 4;
	}
	public static unsafe void Write (ref byte* p, long value) {
		var _p = (long*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, ulong value) {
		var _p = (ulong*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, double value) {
		var _p = (double*)p;
		*_p = value;
		p += 8;
	}
	public static unsafe void Write (ref byte* p, byte[] bytes, int length) {
		length = length < 0 ? bytes.Length : length;
		for (int i = 0; i < length; i++) {
			*p = bytes[i];
			p++;
		}
	}



}
