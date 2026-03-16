using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuickSound;

public static partial class Extension {


	public static bool GetBit (this ulong value, int index) => (value & (1UL << index)) != 0;
	public static bool GetBit (this int value, int index) => (value & (1 << index)) != 0;
	public static bool GetBit (this uint value, int index) => (value & (1U << index)) != 0;
	public static bool GetBit (this ushort value, int index) => (value & (1 << index)) != 0;
	public static bool GetBit (this byte value, int index) => (value & (1 << index)) != 0;

	public static void SetBit (this ref ulong value, int index, bool bitValue) {
		if (index < 0 || index > 63) return;
		var val = 1UL << index;
		value = bitValue ? (value | val) : (value & ~val);
	}
	public static void SetBit (this ref int value, int index, bool bitValue) {
		if (index < 0 || index > 31) return;
		var val = 1 << index;
		value = bitValue ? (value | val) : (value & ~val);
	}
	public static void SetBit (this ref uint value, int index, bool bitValue) {
		if (index < 0 || index > 31) return;
		uint val = (uint)(1 << index);
		value = bitValue ? (value | val) : (value & ~val);
	}
	public static void SetBit (this ref byte value, int index, bool bitValue) {
		if (index < 0 || index > 7) return;
		var val = 1 << index;
		value = (byte)(bitValue ? (value | val) : (value & ~val));
	}

	public static bool Almost (this float a, float b) => Util.Approximately(a, b);
	public static bool AlmostZero (this float a) => Util.Abs(a) < Util.Max(1E-06f * Util.Max(Abs(a), 0f), Util.Epsilon * 8f);
	public static bool NotAlmost (this float a, float b) => !Util.Approximately(a, b);
	public static bool NotAlmostZero (this float a) => Util.Abs(a) >= Util.Max(1E-06f * Util.Max(Abs(a), 0f), Util.Epsilon * 8f);

	public static bool GreaterOrAlmost (this float a, float b) => a > b || a.Almost(b);
	public static bool LessOrAlmost (this float a, float b) => a < b || a.Almost(b);


	public static int UDivide (this int value, int step) =>
			value > 0 || value % step == 0 ?
			value / step :
			value / step - 1;


	public static int UMod (this int value, int step) =>
		value > 0 || value % step == 0 ?
		value % step :
		value % step + step;


	public static long UMod (this long value, long step) =>
		value > 0 || value % step == 0 ?
		value % step :
		value % step + step;


	public static int UCeil (this int value, int step) =>
		value % step == 0 ? value :
		value > 0 ? value - (value % step) + step :
		value - (value % step);


	public static int UFloor (this int value, int step) =>
		value % step == 0 ? value :
		value > 0 ? value - (value % step) :
		value - (value % step) - step;

	public static int Distance (this int value, int target) => Util.Abs(value - target);

	public static int CeilDivide (this int value, int target) => value / target + (value % target == 0 ? 0 : 1);

	public static int Abs (this int value) => value > 0 ? value : -value;
	public static long Abs (this long value) => value > 0 ? value : -value;
	public static float Abs (this float value) => value > 0 ? value : -value;

	public static int RoundToInt (this float a) => (int)System.MathF.Round(a);
	public static int CeilToInt (this float a) => (int)System.MathF.Ceiling(a);
	public static int FloorToInt (this float a) => (int)System.MathF.Floor(a);
	public static float Floor (this float a) => System.MathF.Floor(a);
	public static float Ceil (this float a) => System.MathF.Ceiling(a);
	public static float Round (this float a) => System.MathF.Round(a);



	public static float UMod (this float value, float gap) =>
		value > 0 || value % gap == 0 ?
		value % gap :
		value % gap + gap;


	public static float UCeil (this float value, float gap) => value % gap == 0 ? value :
		value > 0 ? value - (value % gap) + gap :
		value - (value % gap);


	public static float UFloor (this float value, float gap) => value % gap == 0 ? value :
		value > 0 ? value - (value % gap) :
		value - (value % gap) - gap;



	public static int Clamp (this int a, int min, int max) => a < min ? min : a > max ? max : a;


	public static int ReverseClamp (this int a, int min, int max) {
		if (a <= min || a >= max) return a;
		int center = (min + max) / 2;
		return a < center ? min : max;
	}

	public static float Clamp (this float a, float min, float max) => a < min ? min : a > max ? max : a;

	public static long Clamp (this long a, long min, long max) => a < min ? min : a > max ? max : a;

	public static float Clamp01 (this float value) => value < 0f ? 0f : value > 1f ? 1f : value;

	public static int GreaterOrEquel (this int value, int target) => value > target ? value : target;
	public static long GreaterOrEquel (this long value, long target) => value > target ? value : target;
	public static int LessOrEquel (this int value, int target) => value < target ? value : target;

	public static int GreaterOrEquelThanZero (this int value) => value > 0 ? value : 0;
	public static int LessOrEquelThanZero (this int value) => value < 0 ? value : 0;

	public static int Sign (this int i) => i >= 0 ? 1 : -1;
	public static int Sign3 (this int i) => i == 0 ? 0 : i > 0 ? 1 : -1;


	public static int MoveTowards (this int current, int target, int maxDelta) {
		if (Util.Abs(target - current) <= maxDelta) {
			return target;
		}
		return current + (target - current).Sign() * maxDelta;
	}

	public static int MoveTowards (this int current, int target, int positiveDelta, int negativeDelta) => current.MoveTowards(
		target, Util.Abs(target) > Util.Abs(current) ? positiveDelta : negativeDelta
	);

	public static bool InRangeInclude (this int value, int min, int max) => value >= min && value <= max;
	public static bool InRangeExclude (this int value, int min, int max) => value > min && value < max;


	public static int LerpTo (this int from, int to, float lerp01) => from + ((to - from) * lerp01).RoundToInt();

	public static int LerpTo (this int from, int to, int lerpRate) {
		int result;
		try {
			checked {
				result = from + ((to - from) * lerpRate / 1000);
			}
		} catch (System.OverflowException) {
			result = from + (int)((to - from) / 1000f * lerpRate);
		}
		if (result == from && from != to && lerpRate != 0) {
			return to > from ? from + 1 : from - 1;
		}
		return result;
	}

	public static float LerpWithGap (this float from, float to, float lerp, float gap) => Util.Abs(from - to) > gap ? Util.LerpUnclamped(from, to, lerp) : to;


	public static int PingPong (this int value, int length) {
		value = value.UMod(length * 2);
		return length - Util.Abs(value - length);
	}


	public static float PingPong (this float value, float length) {
		value = value.UMod(length * 2);
		return length - Util.Abs(value - length);
	}


	public static int PingPong (this int value, int min, int max) {
		int length = max - min;
		value = value.UMod(length * 2);
		return length - Util.Abs(value - length) + min;
	}

	public static int DigitCount (this int n) {
		if (n >= 0) {
			if (n < 10) return 1;
			if (n < 100) return 2;
			if (n < 1000) return 3;
			if (n < 10000) return 4;
			if (n < 100000) return 5;
			if (n < 1000000) return 6;
			if (n < 10000000) return 7;
			if (n < 100000000) return 8;
			if (n < 1000000000) return 9;
			return 10;
		} else {
			if (n > -10) return 1;
			if (n > -100) return 2;
			if (n > -1000) return 3;
			if (n > -10000) return 4;
			if (n > -100000) return 5;
			if (n > -1000000) return 6;
			if (n > -10000000) return 7;
			if (n > -100000000) return 8;
			if (n > -1000000000) return 9;
			return 10;
		}
	}

	public static int Sign (this bool value) => value ? 1 : -1;

}
