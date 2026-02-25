using Assets.Scripts;
using Cysharp.Threading.Tasks;
using Entropy.Common.Mods;
using HarmonyLib;
using ImGuiNET.Unity;
using JetBrains.Annotations;
using System.Reflection;
using UnityEngine;

namespace Entropy.Common.UI;

public static class ImGuiHost //: MonoBehaviour
{
	private static readonly List<(EntropyModBase Mod, Action Draw)> _drawCallbacks = [];
	internal static void Draw()
	{
		foreach (var callback in _drawCallbacks)
		{
			try
			{
				callback.Draw();
			} catch (Exception ex)
			{
				CommonMod.Instance.Logger.LogError($"Exception in ImGui draw callback: {ex}");
			}
		}
	}
	public static void RegisterDrawCallback(EntropyModBase mod, Action callback)
	{
		if (_drawCallbacks.Any(c => c.Mod == mod && c.Draw == callback))
			return;
		_drawCallbacks.Add((mod, callback));
	}
	public static void UnregisterDrawCallback(EntropyModBase mod, Action callback)
	{
		_drawCallbacks.RemoveAll(c => c.Mod == mod && c.Draw == callback);
	}
}