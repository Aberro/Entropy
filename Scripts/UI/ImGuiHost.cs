#nullable enable
using Entropy.Scripts.Utilities;
using ImGuiNET;
using ImGuiNET.Unity;
using JetBrains.Annotations;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;

namespace Entropy.Scripts.UI;

[RequireComponent(typeof(GraphicRaycaster), typeof(Canvas))]
public class ImGuiHost : Graphic
{
	[SerializeField]
	private RenderTexture? _ImGuiTargetRenderTexture;
	private List<Rect> activeUiRects = [];
	private List<Rect> uiRects = [];

	public static ImGuiHost Instance { get; private set; } = null!;

	public static void PushWindowRect(Rect rect)
	{
		if(Instance == null)
			return;
		// Convert from top-left to bottom-left coordinate systems
		Instance.uiRects.Add(new Rect(rect.x, Screen.height - (rect.y + rect.height), rect.width, rect.height));
	}

	protected override void Awake()
	{
		Instance = this;
		ImGuiUn.Layout += ImGuiUn_Layout;
		// ImGui always going to be rendered on top of Unity UI, so ensure it has highest sorting order
		GetComponent<Canvas>().sortingOrder = 32000;
	}

	private void ImGuiUn_Layout()
	{
		var tmp = this.activeUiRects;
		this.activeUiRects = this.uiRects;
		this.uiRects = tmp;
		this.uiRects.Clear();
	}

	// Update is called once per frame
	[UsedImplicitly]
	[MemberNotNull(nameof(_ImGuiTargetRenderTexture))]
	void Update()
	{
		if (!this._ImGuiTargetRenderTexture.IsValid() || this._ImGuiTargetRenderTexture.width != Screen.width || this._ImGuiTargetRenderTexture.height != Screen.height)
		{
			this._ImGuiTargetRenderTexture = new RenderTexture(Screen.width, Screen.height, this._ImGuiTargetRenderTexture?.depth ?? 8);
			GetComponent<DearImGui>().Camera.targetTexture = this._ImGuiTargetRenderTexture;
		}
	}

	[UsedImplicitly]
	private void OnGUI()
	{
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this._ImGuiTargetRenderTexture);
	}
	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
	}

	public override bool Raycast(Vector2 sp, Camera eventCamera)
	{
		foreach(var rect in this.activeUiRects)
			if(rect.Contains(sp))
				return true;
		return false;
	}
}

public class ImGuiWindow : IDisposable
{
	private string windowName;
	private bool opened = true;
	public ImGuiWindow(string name)
	{
		this.windowName = name;
		ImGui.Begin(name, ref this.opened);
	}

	public ImGuiWindow(string name, ImGuiWindowFlags flags)
	{
		this.windowName = name;
		ImGui.Begin(name, ref this.opened, flags);
	}

	public ImGuiWindow(string name, ref bool open)
	{
		this.windowName = name;
		ImGui.Begin(name, ref open);
		this.opened = open;
	}

	public ImGuiWindow(string name, ref bool open, ImGuiWindowFlags flags)
	{
		this.windowName = name;
		ImGui.Begin(name, ref open, flags);
		this.opened = open;
	}

	public void Dispose()
	{
		if(!this.opened)
			return;
		ImGui.End();
		var window = ImGui.FindWindowByName(this.windowName);
		if(window.WasActive)
		{
			ImGuiHost.PushWindowRect(new Rect(window.Pos, window.ContentSize));
		}
	}
}