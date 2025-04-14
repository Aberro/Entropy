#nullable enable

using System;
using BepInEx.Configuration;
using UnityEngine;
using ImGuiNET;
using System.Linq;
using Assets.Scripts.Util;
using System.Collections.Generic;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Assets.Scripts;

namespace Entropy.Assets.Scripts.Assets.Scripts.UI
{
	public class ModSettings : MonoBehaviour
	{
		private static bool ModSettingsDisplayed;
		private static bool DemoWindowDisplayed;

		private void OnEnable()
		{
			ImGuiUn.Layout += OnLayout;
		}

		void OnDisable()
		{
			ImGuiUn.Layout -= OnLayout;
		}
		private string[]? _comboValuesCache;
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
			{
				if (ImGui.Button("Mod Settings"))
					ImGui.OpenPopup("mod_settings");
				ImGui.SameLine();
				if (ImGui.Button("Debug"))
					ImGui.OpenPopup("Debug");
				ImGui.SameLine();
				if (ImGui.Button("DearImGui"))
					ImGui.OpenPopup("dear_imgui");

				if (ImGui.BeginPopup("mod_settings"))
				{
					if (ImGui.Selectable(nameof(Entropy)))
						ShowModSettings();
					ImGui.EndPopup();
				}
				if (ImGui.BeginPopup("dear_imgui"))
				{
					if (ImGui.Selectable("Show Demo Window"))
						DemoWindowDisplayed = true;
					ImGui.EndPopup();
				}
				if (ImGui.BeginPopup("Debug"))
				{

				}
				ImGui.End();
			}
			if (DemoWindowDisplayed)
			{
				ImGui.ShowDemoWindow(ref DemoWindowDisplayed);
			}
			if (ModSettingsDisplayed && ImGui.Begin("Mod Settings", ref ModSettingsDisplayed, ImGuiWindowFlags.AlwaysAutoResize))
			{
				foreach(PatchCategory category in Enum.GetValues(typeof(PatchCategory)))
				{
					if(category == PatchCategory.None)
						continue;
					var categoryName = category.GetDisplayName();
					if(ImGui.CollapsingHeader(categoryName))
					{
						bool enabled = Plugin.Config.Features[category].Value;
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
				ImGui.End();
			}
		}
		public static void ShowModSettings() => ModSettingsDisplayed = true;

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
}
