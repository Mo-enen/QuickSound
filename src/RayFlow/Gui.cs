using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Raylib_cs;

namespace RayFlow;


public enum GuiColor {
	Clear, White, WhiteAlmost, LightGrey, Grey, DarkGrey, Black,
	Red, Orange, Yellow, OliveYellow, Green, AzureGreen, Cyan, VividCyan, Blue, Purple, Pink, LotusPink,
}


public static class GUI {


	// VAR
	public static MouseCursor RequireCursor { get; set; } = MouseCursor.Default;


	// API
	public static bool Button (string label, float? width = null, GuiColor bodyColor = GuiColor.VividCyan, GuiColor labelColor = GuiColor.WhiteAlmost, bool interactable = true) {
		using var e = new EnableScope(interactable);
		using var a = new StyleColorScope(ImGuiCol.Button, interactable ? bodyColor.ToVec4() : bodyColor.Grey(0.3f));
		using var b = new StyleColorScope(ImGuiCol.ButtonHovered, interactable ? bodyColor.Mult(0.95f) : bodyColor.Grey(0.3f));
		using var c = new StyleColorScope(ImGuiCol.ButtonActive, interactable ? bodyColor.Mult(0.9f) : bodyColor.Grey(0.3f));
		using var d = new StyleColorScope(ImGuiCol.Text, interactable ? labelColor.ToVec4() : labelColor.Grey(0.3f));
		width ??= 0f;
		bool result = ImGui.Button(label, new Vector2(width.Value, 0f));
		if (ImGui.IsItemHovered()) {
			RequireCursor = MouseCursor.PointingHand;
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
		if (string.IsNullOrEmpty(label)) label = "##";
		ImGui.SetNextItemWidth(-256);
		ImGui.SetNextItemWidth(width);
		ImGui.InputText(label, ref text, maxLen);
		if (ImGui.IsItemHovered()) {
			RequireCursor = MouseCursor.IBeam;
		}
		return ImGui.IsItemActive();
	}


	// Geometry
	public static void DrawCircle (float centerX, float centerY, float radius, GuiColor color = GuiColor.White, float thickness = 2f, int segment = 32) => ImGui.GetWindowDrawList().AddCircle(new Vector2(centerX, centerY), radius, ImGui.GetColorU32(color.ToVec4()), segment, thickness);

	public static void DrawFilledCircle (float centerX, float centerY, float radius, GuiColor color = GuiColor.White, int segment = 32) => ImGui.GetWindowDrawList().AddCircleFilled(new Vector2(centerX, centerY), radius, ImGui.GetColorU32(color.ToVec4()), segment);

	public static void DrawRect (float minX, float minY, float maxX, float maxY, GuiColor color = GuiColor.White, float thickness = 2f, float round = 0) => ImGui.GetWindowDrawList().AddRect(new Vector2(minX, minY), new Vector2(maxX, maxY), ImGui.GetColorU32(color.ToVec4()), round, ImDrawFlags.None, thickness);

	public static void DrawFilledRect (float minX, float minY, float maxX, float maxY, GuiColor color = GuiColor.White, float round = 0) => ImGui.GetWindowDrawList().AddRectFilled(new Vector2(minX, minY), new Vector2(maxX, maxY), ImGui.GetColorU32(color.ToVec4()), round);

	public static void DrawLine (float fromX, float fromY, float toX, float toY, GuiColor color = GuiColor.White, float thickness = 2f) => ImGui.GetWindowDrawList().AddLine(new Vector2(fromX, fromY), new Vector2(toX, toY), ImGui.GetColorU32(color.ToVec4()), thickness);


	// UTL
	public static Vector4 ToVec4 (this GuiColor color) => color switch {
		GuiColor.White => new(1f, 1f, 1f, 1f),
		GuiColor.WhiteAlmost => new(0.9f, 0.9f, 0.9f, 1f),
		GuiColor.LightGrey => new(0.618f, 0.618f, 0.618f, 1f),
		GuiColor.Grey => new(0.5f, 0.5f, 0.5f, 1f),
		GuiColor.DarkGrey => new(0.35f, 0.35f, 0.35f, 1f),
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
