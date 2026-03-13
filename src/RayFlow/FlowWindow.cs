using System;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;

namespace RayFlow;

public abstract class FlowWindow {

	// VAR
	public abstract string DeveloperName { get; }
	public virtual int WindowPadding => 42;
	public string SavingFolder => System.IO.Path.Combine(Environment.GetFolderPath(
		Environment.SpecialFolder.LocalApplicationData),
		DeveloperName,
		typeof(Flow).Assembly.GetName().Name
	);
	public int Width { get; internal set; }
	public int Height { get; internal set; }

	// MSG
	public abstract void Start ();
	public abstract void Update ();
	public abstract void Quit ();

}
