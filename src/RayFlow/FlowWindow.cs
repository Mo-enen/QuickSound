using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;

namespace RayFlow;

public abstract class FlowWindow {

	// VAR
	public abstract string DeveloperName { get; }
	public string SavingFolder => System.IO.Path.Combine(Environment.GetFolderPath(
		Environment.SpecialFolder.LocalApplicationData),
		DeveloperName,
		typeof(Flow).Assembly.GetName().Name
	);
	public int Width { get; internal set; }
	public int Height { get; internal set; }
	public MouseCursor RequireCursor { get; set; } = MouseCursor.Default;

	// MSG
	public abstract void Start ();
	public abstract void Update ();
	public abstract void Quit ();

	// API
	protected bool Button (string label, Vector2? size = null, Vector4? bodyColor = null, Vector4? labelColor = null) {
		bodyColor ??= new(0.3f, 0.3f, 0.3f, 1f);
		labelColor ??= new(1f, 1f, 1f, 1f);
		using var a = new StyleColorScope(ImGuiCol.Button, bodyColor.Value);
		using var b = new StyleColorScope(ImGuiCol.ButtonHovered, bodyColor.Value - new Vector4(0.03f, 0.03f, 0.03f, 0f));
		using var c = new StyleColorScope(ImGuiCol.ButtonActive, bodyColor.Value - new Vector4(0.06f, 0.06f, 0.06f, 0f));
		using var d = new StyleColorScope(ImGuiCol.Text, labelColor.Value);
		bool result = size.HasValue ? ImGui.Button(label, size.Value) : ImGui.Button(label);
		if (ImGui.IsItemHovered()) {
			RequireCursor = MouseCursor.PointingHand;
		}
		return result;
	}


}
