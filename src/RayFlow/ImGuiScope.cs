using System.Collections;
using System.Collections.Generic;
using System;
using ImGuiNET;


public readonly struct DisableScope : IDisposable {
	public DisableScope () => ImGui.BeginDisabled();
	public void Dispose () => ImGui.EndDisabled();
}


public readonly struct GroupScope : IDisposable {
	public GroupScope () => ImGui.BeginGroup();
	public void Dispose () => ImGui.EndGroup();
}


public readonly struct ChildScope : IDisposable {
	public ChildScope () : this(default) { }
	public ChildScope (uint id) => ImGui.BeginChild(id);
	public void Dispose () => ImGui.EndChild();
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
