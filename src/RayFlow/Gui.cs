using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Raylib_cs;

namespace RayFlow;


public enum GuiColor {
	Clear, White, WhiteAlmost, DarkWhite, LightGrey, Grey, DarkGrey, BlackAlmost, Black,
	Red, Orange, Yellow, OliveYellow, Green, AzureGreen, Cyan, VividCyan, Blue, Purple, Pink, LotusPink,
}


public static class GUI {


	// VAR
	public static MouseCursor RequireCursor { get; set; } = MouseCursor.Default;
	public static int ButtonDynamicID;
	public static bool MouseLeftDown;
	public static Vector2 PrevMouseLeftDownPos;
	public static Vector2 MousePos;
	private static bool PrevMouseLeftDown;


	// MSG
	internal static void Begin () {
		ButtonDynamicID = 1;
		ChildScope.DynamicID = 1;
		PrevMouseLeftDown = MouseLeftDown;
		MouseLeftDown = ImGui.IsMouseDown(ImGuiMouseButton.Left);
		MousePos = ImGui.GetMousePos();
		if (MouseLeftDown && !PrevMouseLeftDown) {
			PrevMouseLeftDownPos = MousePos;
		}
	}


	// API
	public static bool Button (string label, float? width = null, float? height = null, GuiColor bodyColor = GuiColor.VividCyan, GuiColor labelColor = GuiColor.WhiteAlmost, bool interactable = true) {
		using var a = new StyleColorScope(ImGuiCol.Button, interactable ? bodyColor.ToVec4() : bodyColor.Grey(0.3f));
		using var b = new StyleColorScope(ImGuiCol.ButtonHovered, interactable ? bodyColor.Mult(0.95f) : bodyColor.Grey(0.3f));
		using var c = new StyleColorScope(ImGuiCol.ButtonActive, interactable ? bodyColor.Mult(0.9f) : bodyColor.Grey(0.3f));
		using var d = new StyleColorScope(ImGuiCol.Text, interactable ? labelColor.ToVec4() : labelColor.Grey(0.3f));
		using var e = new EnableScope(interactable);
		using var f = new IdScope(ButtonDynamicID);
		using var g = new StyleScope(ImGuiStyleVar.FrameRounding, 4f);
		ButtonDynamicID++;
		width ??= 0f;
		height ??= 0f;
		bool result = false;
		ImGui.Button(label, new Vector2(width.Value, height.Value));
		if (ImGui.IsItemHovered()) {
			RequireCursor = MouseCursor.PointingHand;
			result = !PrevMouseLeftDown && MouseLeftDown;
		}
		return result;
	}

	public static void Label (string text, float width, GuiColor color = GuiColor.WhiteAlmost) {
		using var _ = new StyleColorScope(ImGuiCol.Text, color);
		ImGui.SetNextItemWidth(width);
		ImGui.LabelText(text, "");
	}

	public static bool Input (string label, ref string text, uint maxLen = 64, float width = -1f, GuiColor bodyColor = GuiColor.VividCyan, GuiColor color = GuiColor.WhiteAlmost) {
		using var _ = new StyleColorScope(ImGuiCol.Text, color);
		using var __ = new StyleColorScope(ImGuiCol.FrameBg, bodyColor.Mult(0.4f));
		using var ___ = new StyleScope(ImGuiStyleVar.FrameRounding, 4f);
		if (string.IsNullOrEmpty(label)) label = "##";
		ImGui.SetNextItemWidth(-256);
		ImGui.SetNextItemWidth(width);
		ImGui.InputText(label, ref text, maxLen);
		if (ImGui.IsItemHovered()) {
			RequireCursor = MouseCursor.IBeam;
		}
		return ImGui.IsItemActive();
	}

	public static void LineOnLastItem (bool right, float thickness = 2, float shiftX = -16, GuiColor color = GuiColor.DarkGrey) {
		var min = ImGui.GetItemRectMin();
		var max = ImGui.GetItemRectMax();
		float x = right ? max.X : min.X;
		DrawLine(x + shiftX, max.Y, x + shiftX, min.Y, color, thickness);
	}


	// Geometry
	public static void DrawCircle (float centerX, float centerY, float radius, GuiColor color = GuiColor.White, float alpha = 1f, float thickness = 2f, int segment = 32) => ImGui.GetWindowDrawList().AddCircle(new Vector2(centerX, centerY), radius, ImGui.GetColorU32(color.Alpha(alpha)), segment, thickness);

	public static void DrawFilledCircle (float centerX, float centerY, float radius, GuiColor color = GuiColor.White, float alpha = 1f, int segment = 32) => ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(centerX, centerY), radius, ImGui.GetColorU32(color.Alpha(alpha)), segment);

	public static void DrawRect (float minX, float minY, float maxX, float maxY, GuiColor color = GuiColor.White, float alpha = 1f, float thickness = 2f, float round = 0) => ImGui.GetWindowDrawList().AddRect(new Vector2(minX, minY), new Vector2(maxX, maxY), ImGui.GetColorU32(color.Alpha(alpha)), round, ImDrawFlags.None, thickness);

	public static void DrawFilledRect (float minX, float minY, float maxX, float maxY, GuiColor color = GuiColor.White, float alpha = 1f, float round = 0) => ImGui.GetWindowDrawList().AddRectFilled(new Vector2(minX, minY), new Vector2(maxX, maxY), ImGui.GetColorU32(color.Alpha(alpha)), round);

	public static void DrawLine (float fromX, float fromY, float toX, float toY, GuiColor color = GuiColor.White, float alpha = 1f, float thickness = 2f) => ImGui.GetWindowDrawList().AddLine(new Vector2(fromX, fromY), new Vector2(toX, toY), ImGui.GetColorU32(color.Alpha(alpha)), thickness);


	// UTL
	public static Vector4 ToVec4 (this GuiColor color) => color switch {
		GuiColor.White => new(1f, 1f, 1f, 1f),
		GuiColor.WhiteAlmost => new(0.9f, 0.9f, 0.9f, 1f),
		GuiColor.DarkWhite => new(0.8f, 0.8f, 0.8f, 1f),
		GuiColor.LightGrey => new(0.618f, 0.618f, 0.618f, 1f),
		GuiColor.Grey => new(0.5f, 0.5f, 0.5f, 1f),
		GuiColor.DarkGrey => new(0.35f, 0.35f, 0.35f, 1f),
		GuiColor.BlackAlmost => new(0.1f, 0.1f, 0.1f, 1f),
		GuiColor.Black => new(0, 0, 0, 1f),
		GuiColor.Red => new(1, 0, 0, 1f),
		GuiColor.Orange => new(1, 0.5f, 0, 1f),
		GuiColor.Yellow => new(1, 1, 0, 1f),
		GuiColor.OliveYellow => new(0.5f, 1, 0, 1f),
		GuiColor.Green => new(0, 1, 0, 1f),
		GuiColor.AzureGreen => new(0, 1, 0.5f, 1f),
		GuiColor.Cyan => new(0, 1, 1, 1f),
		GuiColor.VividCyan => new(0, 0.5f, 1, 1f),
		GuiColor.Blue => new(0, 0, 1, 1f),
		GuiColor.Purple => new(0.5f, 0, 1, 1f),
		GuiColor.Pink => new(1, 0, 1, 1f),
		GuiColor.LotusPink => new(1, 0, 0.5f, 1f),
		_ or GuiColor.Clear => new(0, 0, 0, 0)
	};

	public static Vector4 Mult (this GuiColor color, float amount = 0.95f) => color.ToVec4() * new Vector4(amount, amount, amount, 1f);

	public static Vector4 Alpha (this GuiColor color, float alpha) => color.ToVec4() * new Vector4(1f, 1f, 1f, alpha);

	public static Vector4 Grey (this GuiColor color, float alpha = -1f) {
		var v = color.ToVec4();
		float rgb = (v.X + v.Y + v.Z) / 3f;
		v.X = rgb;
		v.Y = rgb;
		v.Z = rgb;
		if (alpha > -0.5f) v.W = alpha;
		return v;
	}

}
