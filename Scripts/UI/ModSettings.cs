using BepInEx.Configuration;
using UnityEngine;
using ImGuiNET;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Entropy.Scripts.Utilities;
using UnityEngine.UI;
using Assets.Scripts.UI;
using UnityEngine.EventSystems;

namespace Entropy.Scripts.UI;

[RequireComponent(typeof(GraphicRaycaster), typeof(Canvas))]
public class ModSettings : MonoBehaviour
{
	private static bool _modSettingsDisplayed;
	private static bool _demoWindowDisplayed;
	private bool _patchedUI;
	private string[]? _comboValuesCache;

	[UsedImplicitly]
    private void OnEnable()
	{
		ImGuiUn.Layout += OnLayout;
		SceneManager.sceneLoaded += OnSceneLoaded;
		GetComponent<Canvas>().sortingOrder = 32000;
		OnSceneLoaded(SceneManager.GetActiveScene(), SceneManager.loadedSceneCount > 0 ? LoadSceneMode.Single : LoadSceneMode.Additive);
	}

	[UsedImplicitly]
    void OnDisable()
	{
		ImGuiUn.Layout -= OnLayout;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(scene.name == "Base")
		{
			var alertCanvas = GameObject.Find("AlertCanvas")?.transform;
			var panelSettings = alertCanvas?.Find("PanelSettings");
			var panelServerWindow = panelSettings?.Find("PanelServerWindow");
			var buttonGrid = panelServerWindow?.Find("ButtonGrid");
			var buttonAudio = buttonGrid?.Find("ButtonAudio");

			if(!buttonAudio.IsValid())
				Plugin.LogWarning("Game UI has updated, cannot add \"Mod settings\" button!");
			var buttonModSettings = Instantiate(buttonAudio, buttonAudio!.transform.parent)!;

			var buttonText = buttonModSettings.Find("ButtonText");
			buttonText.GetComponent<LocalizedText>().StringKey = "SettingsMenuModSettings";

			var toggle = buttonModSettings.GetComponent<Toggle>();
			var uiAudioComponent = buttonModSettings.GetComponent<UIAudioComponent>();
			toggle.onValueChanged.RemoveAllListeners();
			for(var i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++)
				toggle.onValueChanged.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
			toggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(uiAudioComponent.PlayToggleSound));
			toggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(ToggleModSettings));

			this._patchedUI = true;
		}
	}

	private void ToggleModSettings(bool arg) => _modSettingsDisplayed = arg;

	void OnLayout()
	{
		ImGui.SetNextWindowPos(new Vector2(0, 0), ImGuiCond.Always);
		ImGui.SetNextWindowSize(new Vector2(Screen.width, Screen.height), ImGuiCond.Always);

		ImGui.Begin("::Overlay",
		ImGuiWindowFlags.NoTitleBar
		| ImGuiWindowFlags.NoResize
		| ImGuiWindowFlags.NoMove
		| ImGuiWindowFlags.NoScrollbar
		| ImGuiWindowFlags.NoBackground
		| ImGuiWindowFlags.NoCollapse
		| ImGuiWindowFlags.NoDecoration
		| ImGuiWindowFlags.NoSavedSettings
		| ImGuiWindowFlags.NoNavFocus
		| ImGuiWindowFlags.NoBringToFrontOnFocus);
		if(!this._patchedUI)
		{
			// If patching the game's UI wasn't successfull for some reason, display top left corner buttons.
			if(ImGui.Button("Mod Settings"))
				ImGui.OpenPopup("mod_settings");
			ImGui.SameLine();
			if(ImGui.Button("Debug"))
				ImGui.OpenPopup("Debug");
			ImGui.SameLine();
			if(ImGui.Button("DearImGui"))
				ImGui.OpenPopup("dear_imgui");

			if(ImGui.BeginPopup("mod_settings"))
			{
				if(ImGui.Selectable(nameof(Entropy)))
					ShowModSettings();
				ImGui.EndPopup();
			}
			if(ImGui.BeginPopup("dear_imgui"))
			{
				if(ImGui.Selectable("Show Demo Window"))
					_demoWindowDisplayed = true;
				ImGui.EndPopup();
			}
			if(ImGui.BeginPopup("Debug"))
			{

			}
			ImGui.End();
		}
		if (_demoWindowDisplayed)
		{
			ImGui.ShowDemoWindow(ref _demoWindowDisplayed);
			var window = ImGui.FindWindowByName("Dear ImGui Demo");
			if(window.WasActive)
			{
				ImGuiHost.PushWindowRect(new Rect(window.Pos, window.ContentSize));
			}
		}
		using (new ImGuiWindow("Mod Settings", ref _modSettingsDisplayed, ImGuiWindowFlags.AlwaysAutoResize))
			if(_modSettingsDisplayed)
			{
				foreach(PatchCategory category in Enum.GetValues(typeof(PatchCategory)))
				{
					if(category == PatchCategory.None)
						continue;
					var categoryName = category.GetDisplayName();
					if(ImGui.CollapsingHeader(categoryName))
					{
						var enabled = Plugin.Config.Features[category].Value;
						ImGui.Checkbox("Enabled", ref enabled);
						Plugin.Config.Features[category].Value = enabled;
						foreach(var config in Plugin.Config)
						{
							if ((config.Value is ConfigEntry<bool> entry && Plugin.Config.Features.ContainsValue(entry)) || config.Key.Section != categoryName)
								continue;
							if(config.Value is ConfigEntry<bool> boolEntry)
							{
								enabled = boolEntry.Value;
								ImGui.Checkbox(config.Key.Key, ref enabled);
								boolEntry.Value = enabled;
							}
							else if(config.Value is ConfigEntry<int> intEntry)
							{
								var value = intEntry.Value;
								if (config.Value.Description.AcceptableValues is AcceptableValueRange<int> range)
								{
									if (ImGui.DragInt(config.Key.Key, ref value, 1, range.MinValue, range.MaxValue))
										intEntry.Value = value;
								}
								else if (config.Value.Description.AcceptableValues is AcceptableValueList<int> list)
								{
									PrepareComboCache(list.AcceptableValues);
									value = Array.IndexOf(list.AcceptableValues, intEntry.Value);
									if (ImGui.Combo(config.Key.Key, ref value, this._comboValuesCache, list.AcceptableValues.Length))
										intEntry.Value = list.AcceptableValues[value];
								}
								else
								{
									if (ImGui.InputInt(config.Key.Key, ref value))
										intEntry.Value = value;
								}
							}
							else if(config.Value is ConfigEntry<float> floatEntry)
							{
								var value = floatEntry.Value;
								if (config.Value.Description.AcceptableValues is AcceptableValueRange<float> range)
								{
									if (ImGui.DragFloat(config.Key.Key, ref value, 1f, range.MinValue, range.MaxValue))
										floatEntry.Value = value;
								}
								else if (config.Value.Description.AcceptableValues is AcceptableValueList<float> list)
								{
									PrepareComboCache(list.AcceptableValues);
									var index = Array.IndexOf(list.AcceptableValues, floatEntry.Value);
									if (ImGui.Combo(config.Key.Key, ref index, this._comboValuesCache, list.AcceptableValues.Length))
										floatEntry.Value = list.AcceptableValues[index];
								}
								else
								{
									if (ImGui.InputFloat (config.Key.Key, ref value))
										floatEntry.Value = value;
								}
							}
							else if(config.Value is ConfigEntry<string> stringEntry)
							{
								var value = stringEntry.Value;
								if (config.Value.Description.AcceptableValues is AcceptableValueList<string> list)
								{
									var index = Array.IndexOf(list.AcceptableValues, stringEntry.Value);
									if (ImGui.Combo(config.Key.Key, ref index, list.AcceptableValues, list.AcceptableValues.Length))
										stringEntry.Value = list.AcceptableValues[index];
								}
								else
								{
									if (ImGui.InputText(config.Key.Key, ref value, 65535))
										stringEntry.Value = value;
								}
							}
							else
							{
								if(config.Value.SettingType.IsEnum)
								{
									var acceptableValuesArray = Enum.GetValues(config.Value.SettingType);
									if (this._comboValuesCache == null)
									{
										this._comboValuesCache = acceptableValuesArray.OfType<Enum>().Select(v => v!.ToString()).ToArray();
										return;
									}
									if (this._comboValuesCache.Length < acceptableValuesArray.Length)
										Array.Resize(ref this._comboValuesCache, acceptableValuesArray.Length);
									for (int i = 0; i < acceptableValuesArray.Length; i++)
									{
										Enum value = (Enum)acceptableValuesArray.GetValue(i);
										this._comboValuesCache[i] = value.GetDisplayName();
									}
									var index = Array.IndexOf(acceptableValuesArray, config.Value.BoxedValue);
									if (ImGui.Combo(config.Key.Key, ref index, this._comboValuesCache, acceptableValuesArray.Length))
										config.Value.BoxedValue = acceptableValuesArray.GetValue(index);
								}
							}
							if (!string.IsNullOrEmpty(config.Value.Description.Description))
								HelpMarker(config.Value.Description.Description);
						}
					}
				}
			}
	}
	public static void ShowModSettings() => _modSettingsDisplayed = true;

	private void PrepareComboCache<T>(T[] values)
	{
		if (this._comboValuesCache == null)
		{
			this._comboValuesCache = values.Select(v => v!.ToString()).ToArray();
			return;
		}
		if (this._comboValuesCache.Length < values.Length)
			Array.Resize(ref this._comboValuesCache, values.Length);
		for (int i = 0; i < values.Length; i++)
			this._comboValuesCache[i] = values[i]!.ToString();
	}
	private void HelpMarker(string desc)
	{
		ImGui.SameLine();
		ImGui.TextDisabled("(?)");
		if (ImGui.IsItemHovered())
		{
			ImGui.BeginTooltip();
			ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
			ImGui.TextUnformatted(desc);
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}
}
