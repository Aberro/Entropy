using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using Entropy.Common.Attributes;
using Entropy.Common.UI;
using HarmonyLib;
using ImGuiNET.Unity;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using TextureManager = ImGuiNET.Unity.TextureManager;

namespace Entropy.Common;

// Patch to apply Stationeers style to SLP config panel.
[HarmonyPatch]
public static class SLPPatches
{
	[AutoConfigDefinition("Apply Stationeers-like style to StationeersLaunchPad configuration panels", DisplayName = "Restyle StationeersLaunchPad panels")]
	private static bool RestyleSLP { get; set; }
	private static bool _shouldRestoreOriginalStyle;

	// This patch is a bit tricky as it's application could crash the game because it patches a method that would be in stack the moment
	// patching happens, and worse than that - it would be in ImGui drawing process. So, we can't apply changes immediately.
	// Instead, we define the patch config entry for Postfix method only, and use Prepare to get the entry and read it's value and perform patching
	private static Regex _methodsFilter = new("^<(DrawWorkshopConfig|DrawSettingsWindow)>", RegexOptions.Compiled);
	public static IEnumerable<MethodBase> TargetMethods()
	{
		// Here methods we need to patch are actually callbacks, so find them by specific name.
		// But since we need to patch two of them in the same way - could use regex as well.
		// Just filter out return type and parameters to be sure those are right methods.
		var SLP = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "StationeersLaunchPad");
		var types = SLP.GetTypes().Where(t => t.FullName.Contains("ConfigPanel")).ToArray();
		var result = types.SelectMany(t =>
			AccessTools.GetDeclaredMethods(t).Where(m => _methodsFilter.IsMatch(m.Name) && m.ReturnType == typeof(void) && m.GetParameters().Length == 0)).ToArray();
		if (result.Length != 2)
		{
			CommonMod.Instance.Logger.LogWarning("Failed to find SLP ConfigPanel methods to patch styling");
			return Array.Empty<MethodBase>();
		}
		return result;
	}
	[HarmonyPrefix]
	public static void Prefix()
	{
		if (RestyleSLP)
		{
			ImGuiHelper.ApplyStationeersStyle();
			_shouldRestoreOriginalStyle = true;
		}
	}

	[HarmonyPostfix]
	public static void Postfix()
	{
		if (_shouldRestoreOriginalStyle)
		{
			ImGuiHelper.RestoreDefaultStyle();
			_shouldRestoreOriginalStyle = false;
		}
	}
	public static void PrepareUnpatch()
	{
		ImGuiHelper.RestoreDefaultStyle();
	}
}

[HarmonyPatch]
public static class Patches
{
	private static bool _imGuiShaderPatched;
	//[HarmonyPatch(typeof(ImGuiManager), "CreateRenderTexture")]
	//[HarmonyPrefix]
	//public static bool ImGuiManagerCreateRenderTexturePrefix(
	//	ImGuiManager __instance, 
	//	ref RenderTexture ___renderTexture,
	//	Camera ___overlayCamera,
	//	RawImage ___outputRawImage)
	//{
	//	___renderTexture = new RenderTexture(Screen.width, Screen.height, GraphicsFormat.R8G8B8A8_UNorm, GraphicsFormat.D24_UNorm_S8_UInt);
	//	___overlayCamera.targetTexture = ___renderTexture;
	//	___outputRawImage.texture = ___renderTexture;
	//	return false;
	//}

	/// <summary>
	/// Patch ImGui shader with our shader with fixed alpha blending.
	/// </summary>
	[HarmonyPatch(typeof(ImGuiManager), "OnEnable")]
	[HarmonyPrefix]
	public static void DearImGuiAwakePrefix(
		ImGuiManager __instance,
		ref ShaderResourcesAsset ____shaders,
		ref ImGuiRendererMesh ___igRenderer,
		ref TextureManager ___igTextureManager)
	{
		if (__instance is null || ____shaders is null || _imGuiShaderPatched)
			return;
		var bundleFile = Path.Combine(Path.GetDirectoryName(typeof(CommonMod).Assembly.Location), "GameData", "entropy.asset");
		if (!File.Exists(bundleFile))
		{
			CommonMod.Instance.Logger.LogError($"Could not find `{bundleFile}' asset bundle!");
			_imGuiShaderPatched = true;
			return;
		}
		var bundle = AssetBundle.LoadFromFile(bundleFile);
		Shader shader = null!;
		try
		{
			shader = bundle.LoadAsset<Shader>("Assets/Shaders/DearImGui-Mesh.shader");
		}
		catch { }
		if (shader is null)
		{
			CommonMod.Instance.Logger.LogError($"Could not find `{"DearImGui - Mesh.shader"}' shader resource");
			_imGuiShaderPatched = true;
			return;
		}
		____shaders.shaders.mesh = shader;
		___igRenderer = new ImGuiRendererMesh(____shaders, ___igTextureManager);
		_imGuiShaderPatched = true;
		Traverse.Create(__instance).Method("CreateRenderTexture").GetValue();
	}

	[HarmonyPatch(typeof(ImGuiManager), "PrepareImGuiFrame")]
	[HarmonyPostfix]
	public static void ImGuiManagerPrepareImGuiFramePostfix()
	{
		ImGuiHost.Draw();
	}
	/// <summary>
	/// Suppress exceptions during saving process, so the user could retry, or reload a save.
	/// Otherwise, the game cannot save anymore whatsoever until restarted.
	/// </summary>
	[HarmonyPatch(typeof(SaveHelper), "Save", 
		typeof(DirectoryInfo),
		typeof(string),
		typeof(bool),
		typeof(CancellationToken))]
	[HarmonyFinalizer]
	public static Exception? SaveHelperSaveFinalizer(Exception __exception, ref UniTask<SaveResult> __result)
	{
		SaveTaskWrapper(__result);
		saveTaskCompletionSource = new UniTaskCompletionSource<SaveResult>();
		__result = saveTaskCompletionSource.Task;
		if (__exception is not null)
		{
			__result = new UniTask<SaveResult>(SaveResult.Fail(__exception.Message));
			CommonMod.Instance.Logger.LogException(__exception);
		}
		return null;
	}

	private static async void SaveTaskWrapper(UniTask<SaveResult> originalTask)
	{
		try
		{
			var result = await originalTask.AsTask().ConfigureAwait(true);
			saveTaskCompletionSource?.TrySetResult(result);
		} catch(Exception e)
		{
			saveTaskCompletionSource?.TrySetResult(SaveResult.Fail(e.Message));
			CommonMod.Instance.Logger.LogException(e);
		}
	}
	private static UniTaskCompletionSource<SaveResult>? saveTaskCompletionSource;
}