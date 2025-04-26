#nullable enable
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using ImGuiNET;
using ImGuiNET.Unity;
using Entropy.Scripts.Utilities;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace Entropy.Scripts.UI;

[RequireComponent(typeof(GraphicRaycaster), typeof(Canvas))]
public class ImGuiHost : Graphic
{
	[SerializeField]
	private RenderTexture? _ImGuiTargetRenderTexture;
	private List<Rect> activeUiRects = [];
	private List<Rect> uiRects = [];

	public static StyleAsset DefaultStyle
	{
		get;
		set;
	}

	static ImGuiHost()
	{
		DefaultStyle = new StyleAsset()
		{
			Alpha = 1,
			DisabledAlpha = 0.8f,
			WindowPadding = { x = 8, y = 8 },
			WindowRounding = 11,
			WindowBorderSize = 1,
			WindowMinSize = { x = 32, y = 32 },
			WindowTitleAlign = { x = 0.5f, y = 0.5f },
			WindowMenuButtonPosition = ImGuiDir.Left,
			ChildRounding = 4,
			ChildBorderSize = 1,
			PopupRounding = 4,
			PopupBorderSize = 1,
			FramePadding = { x = 8, y = 4},
			FrameRounding = 4,
			FrameBorderSize = 0,
			ItemSpacing = { x = 4, y = 4},
			ItemInnerSpacing = { x = 4, y = 4},
			CellPadding = { x = 4, y = 4},
			TouchExtraPadding = { x = 0, y = 0},
			IndentSpacing = 20,
			ColumnsMinSpacing = 6,
			ScrollbarSize = 11,
			ScrollbarRounding = 4,
			GrabMinSize = 20,
			GrabRounding = 4,
			LogSliderDeadzone = 0,
			TabRounding = 4,
			TabBorderSize = 0,
			TabMinWidthForCloseButton = 19,
			ColorButtonPosition = ImGuiDir.Right,
			ButtonTextAlign = { x = 0.5f, y = 0.5f },
			SelectableTextAlign = { x = 0, y = 0 },
			DisplayWindowPadding = { x = 20, y = 20 },
			DisplaySafeAreaPadding = { x = 3, y = 3 },
			MouseCursorScale = 1,
			AntiAliasedLines = true,
			AntiAliasedLinesUseTex = true,
			AntiAliasedFill = true,
			CurveTessellationTol = 0.6f,
			CircleTessellationMaxError = 1,
			Colors = [
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 0.50f, g: 0.50f, b: 0.50f, a: 1.00f),
				new(r: 0.16f, g: 0.16f, b: 0.20f, a: 1.00f),
				new(r: 0.11f, g: 0.11f, b: 0.13f, a: 1.00f),
				new(r: 0.08f, g: 0.08f, b: 0.08f, a: 1.00f),
				new(r: 0.43f, g: 0.43f, b: 0.50f, a: 1.00f),
				new(r: 0.00f, g: 0.00f, b: 0.00f, a: 1.00f),
				new(r: 0.13f, g: 0.13f, b: 0.16f, a: 1.00f),
				new(r: 0.34f, g: 0.34f, b: 0.35f, a: 1.00f),
				new(r: 0.44f, g: 0.25f, b: 0.24f, a: 1.00f),
				new(r: 0.16f, g: 0.16f, b: 0.20f, a: 1.00f),
				new(r: 0.44f, g: 0.25f, b: 0.24f, a: 1.00f),
				new(r: 0.00f, g: 0.00f, b: 0.00f, a: 1.00f),
				new(r: 0.14f, g: 0.14f, b: 0.14f, a: 1.00f),
				new(r: 0.02f, g: 0.02f, b: 0.02f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 0.78f, g: 0.78f, b: 0.78f, a: 1.00f),
				new(r: 1.00f, g: 0.40f, b: 0.09f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 1.00f, g: 0.40f, b: 0.09f, a: 1.00f),
				new(r: 0.11f, g: 0.11f, b: 0.13f, a: 1.00f),
				new(r: 0.34f, g: 0.34f, b: 0.35f, a: 1.00f),
				new(r: 0.44f, g: 0.25f, b: 0.24f, a: 1.00f),
				new(r: 1.00f, g: 0.40f, b: 0.09f, a: 1.00f),
				new(r: 0.34f, g: 0.34f, b: 0.35f, a: 1.00f),
				new(r: 0.44f, g: 0.25f, b: 0.24f, a: 1.00f),
				new(r: 0.43f, g: 0.43f, b: 0.50f, a: 1.00f),
				new(r: 0.10f, g: 0.40f, b: 0.75f, a: 1.00f),
				new(r: 0.10f, g: 0.40f, b: 0.75f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 0.78f, g: 0.78f, b: 0.78f, a: 1.00f),
				new(r: 0.11f, g: 0.11f, b: 0.13f, a: 1.00f),
				new(r: 0.34f, g: 0.34f, b: 0.35f, a: 1.00f),
				new(r: 1.00f, g: 0.40f, b: 0.09f, a: 1.00f),
				new(r: 0.07f, g: 0.10f, b: 0.15f, a: 1.00f),
				new(r: 0.44f, g: 0.25f, b: 0.24f, a: 1.00f),
				new(r: 0.61f, g: 0.61f, b: 0.61f, a: 1.00f),
				new(r: 1.00f, g: 0.43f, b: 0.35f, a: 1.00f),
				new(r: 0.90f, g: 0.70f, b: 0.00f, a: 1.00f),
				new(r: 1.00f, g: 0.60f, b: 0.00f, a: 1.00f),
				new(r: 0.19f, g: 0.19f, b: 0.20f, a: 1.00f),
				new(r: 0.31f, g: 0.31f, b: 0.35f, a: 1.00f),
				new(r: 0.23f, g: 0.23f, b: 0.25f, a: 1.00f),
				new(r: 0.00f, g: 0.00f, b: 0.00f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 0.26f, g: 0.59f, b: 0.98f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 0.00f, a: 1.00f),
				new(r: 0.26f, g: 0.59f, b: 0.98f, a: 1.00f),
				new(r: 1.00f, g: 1.00f, b: 1.00f, a: 1.00f),
				new(r: 0.80f, g: 0.80f, b: 0.80f, a: 1.00f),
				new(r: 0.80f, g: 0.80f, b: 0.80f, a: 1.00f),
			],
		};
		PrepareImGui();
	}
	public static void Init() { }

	private static void PrepareImGui()
	{
		var imGuiHostObject = new GameObject("ImGui", typeof(RectTransform), typeof(DearImGui), typeof(Camera), typeof(ImGuiHost), typeof(GraphicRaycaster));
		EntropyPlugin.LogWarning("Previous NullReferenceException may be safely ignored, this is a known issue with ImGui.NET.Unity created in runtime.");
		DontDestroyOnLoad(imGuiHostObject);
		var imGui = imGuiHostObject.GetComponent<DearImGui>();
		var imGuiTraverse = new Traverse(imGui);
		var camera = imGuiHostObject.GetComponent<Camera>();
		var imGuiHost = imGuiHostObject.GetComponent<ImGuiHost>();
		var canvas = imGuiHostObject.GetComponent<Canvas>();
		var graphicRaycaster = imGuiHostObject.GetComponent<GraphicRaycaster>();
		imGuiTraverse.Field<Camera>("_camera").Value = camera;
		imGuiTraverse.Field<RenderUtils.RenderType>("_rendererType").Value = RenderUtils.RenderType.Mesh;
		imGuiTraverse.Field<Platform.Type>("_platformType").Value = Platform.Type.InputManager;
		imGuiTraverse.Field<FontAtlasConfigAsset>("_fontAtlasConfiguration").Value = Resources.FindObjectsOfTypeAll<FontAtlasConfigAsset>().First();
		imGuiTraverse.Field<CursorShapesAsset>("_cursorShapes").Value = Resources.FindObjectsOfTypeAll<CursorShapesAsset>().First();
		imGuiTraverse.Field<ShaderResourcesAsset>("_shaders").Value = Resources.FindObjectsOfTypeAll<ShaderResourcesAsset>().First();
		imGuiTraverse.Field<StyleAsset>("_style").Value = DefaultStyle;
		imGuiTraverse.Field<IOConfig>("_initialConfiguration").Value = new()
		{
			KeyboardNavigation = true,
			GamepadNavigation = false,
			NavSetMousePos = false,
			NavNoCaptureKeyboard = false,
			DoubleClickTime = 0.3f,
			DoubleClickMaxDist = 6,
			DragThreshold = 6,
			KeyRepeatDelay = 0.275f,
			KeyRepeatRate = 0.05f,
			FontGlobalScale = 1,
			FontAllowUserScaling = false,
			TextCursorBlink = true,
			ResizeFromEdges = true,
			MoveFromTitleOnly = true,
			MemoryCompactTimer = 0,
		};
		// Since DearImGui.Awake fails when created in runtime, we need to invoke it manually after we initialized the component:
		imGuiTraverse.Method("Awake").GetValue();
		// Also, Awake failure causes components to automatically disable, so we also need to enable it.
		imGui.enabled = true;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.clear;
		camera.orthographic = true;

		imGuiHost.raycastTarget = true;

		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.pixelPerfect = true;
		canvas.sortingOrder = 32000;

		graphicRaycaster.ignoreReversedGraphics = true;
		graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.All;
		graphicRaycaster.blockingMask = int.MaxValue;

	}
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
			this._ImGuiTargetRenderTexture = new RenderTexture(Screen.width, Screen.height, this._ImGuiTargetRenderTexture?.depth ?? 16);
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
		if(open)
		{
			ImGui.Begin(name, ref open);
		}
		this.opened = open;
	}

	public ImGuiWindow(string name, ref bool open, ImGuiWindowFlags flags)
	{
		this.windowName = name;
		if(open)
		{
			ImGui.Begin(name, ref open, flags);
		}
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