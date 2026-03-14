using System.Collections;
using System.Collections.Generic;
using System;
using ImGuiNET;
using System.Numerics;

namespace RayFlow;

public readonly struct EnableScope : IDisposable {
	private readonly bool Enable;
	public EnableScope () : this(true) { }
	public EnableScope (bool enable) {
		Enable = enable;
		if (!enable) {
			ImGui.BeginDisabled();
		}
	}
	public void Dispose () {
		if (!Enable) {
			ImGui.EndDisabled();
		}
	}
}


public readonly struct GroupScope : IDisposable {
	public GroupScope () => ImGui.BeginGroup();
	public void Dispose () => ImGui.EndGroup();
}


public readonly struct ChildScope : IDisposable {
	private readonly bool Success;
	public static uint DynamicID;
	public ChildScope () : this(0f, 0f, 42f, 6f, GuiColor.Clear) { }
	public ChildScope (float width, float height, float scrollBarSize = 42, float scrollbarRounding = 6, GuiColor bgColor = GuiColor.Clear) {
		using var a = new StyleScope(ImGuiStyleVar.ScrollbarRounding, scrollbarRounding);
		using var b = new StyleScope(ImGuiStyleVar.ScrollbarSize, scrollBarSize);
		using var c = new StyleColorScope(ImGuiCol.ChildBg, bgColor.ToVec4());
		Success = ImGui.BeginChild(DynamicID, new Vector2(width, height));
		DynamicID++;
	}
	public void Dispose () {
		if (Success) {
			ImGui.EndChild();
		}
	}
}


public readonly struct IdScope : IDisposable {
	public IdScope () : this(default) { }
	public IdScope (int id) => ImGui.PushID(id);
	public void Dispose () => ImGui.PopID();
}


public readonly struct WidthScope : IDisposable {
	public WidthScope () : this(default) { }
	public WidthScope (int width) => ImGui.PushItemWidth(width);
	public void Dispose () => ImGui.PopItemWidth();
}



public readonly struct FontScope : IDisposable {
	public FontScope () : this(default) { }
	public FontScope (ImFontPtr font) => ImGui.PushFont(font);
	public void Dispose () => ImGui.PopFont();
}


public readonly struct StyleColorScope : IDisposable {
	public StyleColorScope () : this(default, GuiColor.White) { }
	public StyleColorScope (ImGuiCol col, GuiColor color) => ImGui.PushStyleColor(col, color.ToVec4());
	public StyleColorScope (ImGuiCol col, Vector4 value) => ImGui.PushStyleColor(col, value);
	public void Dispose () => ImGui.PopStyleColor();
}


public readonly struct StyleScope : IDisposable {
	public StyleScope (ImGuiStyleVar var, float value) => ImGui.PushStyleVar(var, value);
	public StyleScope (ImGuiStyleVar var, Vector2 value) => ImGui.PushStyleVar(var, value);
	public void Dispose () => ImGui.PopStyleVar();
}
