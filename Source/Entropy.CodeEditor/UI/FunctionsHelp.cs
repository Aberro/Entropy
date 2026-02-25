using Assets.Scripts;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.UI;
using Cysharp.Threading.Tasks;
using Entropy.Common.UI;
using Entropy.Common.UI.ImGUI;
using Entropy.Common.Utils;
using HarmonyLib;
using ImGuiNET;
using JetBrains.Annotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UI.ImGuiUi.ImGuiWindows;
using UnityEngine;
using Animator = UnityEngine.Animator;
using Color = UnityEngine.Color;
using ImGuiWindow = UI.ImGuiUi.ImGuiWindows.ImGuiWindow;
using ImGuiWindowFlags = Entropy.Common.UI.ImGUI.ImGuiWindowFlags;

namespace Entropy.CodeEditor.UI;

public abstract class HelpWindow(string title) : ImGuiWindow(title, new Vector2(640, 640)), IModal
{
	protected class HelpReference
	{
		public Color TitleColor { get; set; }
		public int ReferenceValue1 { get; set; }
		public int ReferenceValue2 { get; set; }
		public string ReferenceType { get; set; }
		public Sprite Image { get; set; }

		public string Title { get; set; }
		public string Type { get; set; }
		public string Description { get; set; }
		public HelpReference(string title, Color titleColor, string description, Sprite defaultItemImage, string referenceType, int ref1 = 0, int ref2 = 0)
		{
			Title = title;
			TitleColor = titleColor;
			ReferenceType = referenceType;
			ReferenceValue1 = ref1;
			ReferenceValue2 = ref2;
			Image = defaultItemImage;
			Title = ProgrammableChip.StripColorTags(title).Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase);
			Type = ProgrammableChip.StripColorTags(referenceType).Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase);
			Description = ProgrammableChip.StripColorTags(description).Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase);
		}
		public HelpReference(ScriptCommand command, Sprite defaultItemImage)
			: this(ProgrammableChip.GetCommandExample(command), Color.yellow, ProgrammableChip.GetCommandDescription(command), defaultItemImage, "Instruction", (int)command, Assets.Scripts.UI.HelpReference.CommandHash)
		{ }
		public HelpReference(ProgrammableChip.Constant constant, Sprite defaultItemImage)
			: this(constant.GetName(), Color.FromString("#20B2AA"), constant.Description, defaultItemImage, "Constant", constant.Hash, Assets.Scripts.UI.HelpReference.InstructionHash)
		{ }
	}
	private readonly List<HelpReference> _helpReferences = new();
	private readonly Regex _searchRegex = new("[^a-zA-Z0-9-]+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
	private HelpReference[]? _filteredReferences;
	private string _search = "";

	protected List<HelpReference> HelpReferences => _helpReferences;
	protected IEnumerable<HelpReference> FilteredReferences => _filteredReferences as IEnumerable<HelpReference> ?? _helpReferences;
	public override ImGuiNET.ImGuiWindowFlags Flags => ImGuiNET.ImGuiWindowFlags.NoScrollbar;
	protected abstract Sprite DefaultItemImage { get; }
	protected abstract Sprite DefaultItemImage2 { get; }
	public bool UnlockCursor => true;

	public override void OnOpen()
	{
		MouseModeController.AddModal(this);
	}
	public override void OnClose() 
	{
		MouseModeController.RemoveModal(this);
	}
	public unsafe override void DrawContent()
	{
		if(ImGui.InputTextWithHint("", "Search", ref _search, 128))
		{
			Search(_search);
		}
		ImGuiHelper.Child("listBox", false, ImGuiWindowFlags.AlwaysVerticalScrollbar, () =>
		{
			// Use this to convert from local space to screen space.
			var zeroPoint = ImGui.GetCursorScreenPos();
			//ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().x);
			//ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 2);
			if(_filteredReferences is not null && _filteredReferences.Length != 0)
			{
				ImGui.Text("No results");
			}
			foreach (var reference in FilteredReferences)
			{
				// Start the border at top-left corner
				var borderTL = ImGui.GetCursorPos();
				// Add 5px margin inside the border
				ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(5, 5));
				ImGuiHelper.Group(() =>
				{
					ImGuiHelper.Image(reference.Image.texture);
					ImGui.SameLine();
					ImGuiHelper.Group(() =>
					{
						ImGui.TextColored(reference.TitleColor, reference.Title);
						ImGui.TextColored(Color.gray, reference.Type);
						ImGui.TextWrapped(reference.Description);
					});
					ImGui.SameLine();
					// Add 5px margin to the right and bottom side of the item.
					ImGui.Dummy(new Vector2(5, 5));
				});
				// We want the border to stretch horizontally. And vertically the cursor pos should be just below the current item.
				var borderBR = new Vector2(ImGui.GetContentRegionAvail().x, ImGui.GetCursorPosY());
				// Add vertical padding between item.
				ImGui.Dummy(new Vector2(0, 5));
				var col = Color.FromVector4(*ImGui.GetStyleColorVec4(ImGuiCol.Border)).ToUint();
				var drawList = ImGui.GetWindowDrawList();
				// Convert to screen coordinates
				borderTL += zeroPoint;
				borderBR += zeroPoint;
				drawList.AddRect(borderTL, borderBR, col, 5, 2);
			}
			//ImGui.PopStyleVar();
		});
	}

	protected HelpReference MakeHelpReference(
	  string format,
	  Color titleColor,
	  string command,
	  string description,
	  string category = "Instruction")
	{
		var helpReference = new HelpReference(
			ProgrammableChip.SetString(format, command),
			titleColor,
			description,
			DefaultItemImage,
			category,
			Animator.StringToHash(command),
			Assets.Scripts.UI.HelpReference.InstructionHash);
		return helpReference;
	}

	private void Search(string searchText)
	{
		if (string.IsNullOrEmpty(searchText))
		{
			_filteredReferences = null;
			return;
		}
		var doExtendedSearch = true;
		switch (this)
		{
			case FunctionsHelp:
				if (searchText.Length < 10)
				{
					if (Enum.TryParse<ScriptCommand>(searchText, out var result))
					{
						DoLiteralSearch((int) result);
						doExtendedSearch = false;
					}
					foreach (var allConstant in ProgrammableChip.AllConstants)
					{
						if (allConstant == searchText)
						{
							DoLiteralSearch(allConstant.Hash);
							doExtendedSearch = false;
						}
					}
					break;
				}
				break;
			case VariablesHelp:
				if (searchText.Length < 24)
				{
					var hash = Animator.StringToHash(searchText);
					foreach (var current in ProgrammableChip.InternalEnums)
					{
						if (current.IsHashType(hash))
							DoCategorySearch(hash);
						if (current.TryParse(searchText))
							DoLiteralSearch(hash);
					}
				}
				break;
			case SlotVariablesHelp:
				if (searchText.Length < 24 && Enum.TryParse<LogicSlotType>(searchText, out var result1))
					DoLiteralSearch((int) result1);
				break;
		}
		if (searchText.Length <= 2)
			return;
		var match = Regex.Match(searchText, "^\\w+", RegexOptions.IgnorePatternWhitespace);
		var firstWord = string.Empty;
		if (match.Success)
			firstWord = match.Value;
		var list1 = searchText.Split(',', StringSplitOptions.None).ToList();
		var hashes = new List<int>(list1.Count);
		var pattern = new StringBuilder();
		for (var index1 = 0; index1 < list1.Count; ++index1)
		{
			var name = list1[index1];
			if (!string.IsNullOrEmpty(name))
			{
				if (index1 > 0)
					pattern.Append('|');
				var list2 = name.Split(' ', StringSplitOptions.None).ToList();
				for (var index2 = list2.Count - 1; index2 >= 0; --index2)
				{
					list2[index2] = this._searchRegex.Replace(list2[index2], string.Empty);
					if (string.IsNullOrEmpty(list2[index2]))
						list2.RemoveAt(index2);
				}
				if (list2.Count > 0 && this is VariablesHelp)
				{
					var hash = Animator.StringToHash(name);
					var flag = false;
					foreach (var internalEnum in ProgrammableChip.InternalEnums)
					{
						if (internalEnum.IsHashType(hash))
						{
							hashes.Add(hash);
							flag = true;
							break;
						}
					}
					if (flag)
						break;
				}
				for (var index3 = 0; index3 < list2.Count; ++index3)
					pattern.Append("(?=.*" + list2[index3] + ")");
			}
		}
		_filteredReferences = null;
		DoSearch(firstWord, pattern.ToString(), hashes, doExtendedSearch);

		void DoLiteralSearch(int value) =>
			_filteredReferences = _helpReferences.Where(x => x.ReferenceValue1 == value).ToArray();
		void DoCategorySearch(int value) =>
			_filteredReferences = _helpReferences.Where(x => x.ReferenceValue2 == value).ToArray();
		void DoSearch(string firstWord, string pattern, List<int> hashes, bool doExtendedSearch)
		{
			if (string.IsNullOrEmpty(pattern))
			{
				_filteredReferences = null;
				return;
			}
			_filteredReferences = this.HelpReferences.Where(helpReference =>
			{
				if (hashes.Count > 0)
				{
					var flag = false;
					foreach (var hash in hashes)
					{
						if (hash != 0 && helpReference.ReferenceValue2 == hash)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
						return false;
				}
				if (Match(helpReference.Title, firstWord))
					return true;
				else if (doExtendedSearch && (Match(helpReference.Title, pattern) || Match(helpReference.Description, pattern)))
					return true;
				return false;
			}).ToArray();
			return;
		}
		bool Match(string value, string pattern) =>
			!string.IsNullOrEmpty(value) && value.Length <= byte.MaxValue
				&& Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
	}
}
public class VariablesHelp : HelpWindow
{
	static Sprite _defaultItemImage = null!, _defaultItemImage2 = null!;
	protected override Sprite DefaultItemImage => _defaultItemImage;
	protected override Sprite DefaultItemImage2 => _defaultItemImage2;
	static VariablesHelp()
	{
		var helpWindow = Resources.FindObjectsOfTypeAll<ScriptHelpWindow>().FirstOrDefault(x => x.name.Contains("HelpVariables"));
		if (helpWindow is not null)
		{
			_defaultItemImage = helpWindow.DefaultItemImage;
			_defaultItemImage2 = helpWindow.DefaultItemImage2;
		}
		else
			CodeEditorMod.Instance.Logger.LogError("Could not find ScriptHelpWindow prefab");
	}
	public VariablesHelp() : base("Variables")
	{
		foreach (var scriptEnum in ProgrammableChip.InternalEnums)
		{
			var scriptEnumType = scriptEnum.GetType();
			// Expected types are ProgrammableChip.BasicEnum<T> and ProgrammableChip.ScriptEnum<T>
			if (!scriptEnumType.IsGenericType || scriptEnumType.GenericTypeArguments.Length != 1)
			{
				CodeEditorMod.Instance.Logger.LogError("ProgrammableChip.InternalEnums contains an unexpected type!");
				continue;
			}
			var enumType = scriptEnumType.GenericTypeArguments[0];
			var typedPageWrapper = typeof(MakePageWrapper<>).MakeGenericType(enumType);
			var pageWrapper = (IPageWrapper)Activator.CreateInstance(typedPageWrapper, scriptEnum);

			var num = scriptEnum.Count();
			for (var i = 0; i < num; ++i)
			{
				var helpReference = pageWrapper.MakePage(i, DefaultItemImage);
				if (helpReference is not null)
				{
					helpReference.Image = this.DefaultItemImage;
					HelpReferences.Add(helpReference);
				}
			}
		}
	}
	public static HelpWindow Open()
	{
		var result = new VariablesHelp();
		ImGuiWindowManager.Open(result);
		return result;
	}
	interface IPageWrapper
	{
		HelpReference? MakePage(int i, Sprite defaultItemSprite);
	}
#pragma warning disable CA1812
	private class MakePageWrapper<T> : IPageWrapper where T : struct, Enum, IConvertible
#pragma warning restore CA1812
	{
		private readonly IScriptEnum _enum;
		private T[] _types;
		private string[] _names;
		private Func<T, bool>? _isDeprecated;
		private Color? _color;
		private string _typeString;
		private Func<T, string>? _getDescription;

		public MakePageWrapper(IScriptEnum @enum)
		{
			_enum = @enum;
			var traverse = Traverse.Create(_enum);
			_types = traverse.Field("_types").GetValue<T[]>();
			_names = traverse.Field("_names").GetValue<string[]>();
			_isDeprecated = traverse.Field("_isDeprecated").GetValue<Func<T, bool>>();
			var colorStr = traverse.Field("_color").GetValue<string>();
			_color = string.IsNullOrEmpty(colorStr) ? null : Color.FromString(colorStr);
			_typeString = traverse.Field("_typeString").GetValue<string>();
			var field = traverse.Field("_getDescription");
			if(field.FieldExists())
				_getDescription = field.GetValue<Func<T, string>>();
		}

		//private IScriptEnum _enum = @enum;
		private ProgrammableChip.BasicEnum<T>? BasicEnum => _enum as ProgrammableChip.BasicEnum<T>;
		private ProgrammableChip.ScriptEnum<T>? ScriptEnum => _enum as ProgrammableChip.ScriptEnum<T>;

		public HelpReference? MakePage(int i, Sprite defaultItemImage)
		{
			var type = _types[i];
			if ((_isDeprecated != null ? (_isDeprecated(type) ? 1 : 0) : 0) != 0)
				return null;
			var titleColor = _color ?? Color.FromUInt(0x808080);
			string referenceType;
			if (ScriptEnum is not null)
				referenceType = typeof(T).Name;
			else
				referenceType = "Constant";
			var referenceValue1 = Animator.StringToHash(_names[i]);
			int referenceValue2;
			if (ScriptEnum is not null)
				referenceValue2 = Animator.StringToHash(typeof(T).Name);
			else
				referenceValue2 = Animator.StringToHash(_typeString);
			string description;
			if (ScriptEnum is not null)
			{
				description = (_getDescription != null ? _getDescription(type) : null) ?? string.Empty;
			}
			else
			{
				description = string.Empty;
			}
			return new HelpReference(_names[i], titleColor, description, defaultItemImage, referenceType, referenceValue1, referenceValue2);
		}
	}
}
public class FunctionsHelp : HelpWindow
{
	static Sprite _defaultItemImage = null!, _defaultItemImage2 = null!;
	protected override Sprite DefaultItemImage => _defaultItemImage;
	protected override Sprite DefaultItemImage2 => _defaultItemImage2;
	static FunctionsHelp()
	{
		var helpWindow = Resources.FindObjectsOfTypeAll<ScriptHelpWindow>().FirstOrDefault(x => x.name.Contains("HelpFunctions"));
		if (helpWindow is not null)
		{
			_defaultItemImage = helpWindow.DefaultItemImage;
			_defaultItemImage2 = helpWindow.DefaultItemImage2;
		}
		else
			CodeEditorMod.Instance.Logger.LogError("Could not find ScriptHelpWindow prefab");
	}
	public FunctionsHelp() : base("Functions")
	{
		List<ScriptCommand> scriptCommandList = [..EnumCollections.ScriptCommands.Values];
		scriptCommandList.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
		HelpReferences.Add(MakeHelpReference("{0}{14}", Color.FromString("#20B2AA"), "$", "any valid hex characters after this will be parsed together as a hex value. You can use underscores to help with readability, but they have no functional use. As an example, $F will parse as 15.", "Macro"));
		HelpReferences.Add(MakeHelpReference("{0}{14}", Color.FromString("#20B2AA"), "%", "any valid binary numbers (0 or 1) will be parsed together as a binary value. You can use underscores to help with readability, but they have no functional use. As an example, %1111 or %11_11 will parse as 15.", "Macro"));
		HelpReferences.Add(MakeHelpReference("{0}", Color.FromString("#A0A0A0"), "HASH(\"...\")", "any text inside will be hashed to an integer before processing takes place. Use this to generate integer values for use wherever hashes are required.", "Macro"));
		HelpReferences.Add(MakeHelpReference("{0}", Color.FromString("#A0A0A0"), "STR(\"...\")", "any text inside will be packed into 53 usable bits of the 64 bit backing number that can be used for drawing text. This limits its use to 6 characters. ", "Macro"));
		foreach (var allConstant in ProgrammableChip.AllConstants)
		{
			var helpReference = new HelpReference(allConstant, DefaultItemImage2);
			HelpReferences.Add(helpReference);
		}
		HelpReferences.Add(MakeHelpReference("{0} {7}", Color.FromString("#A0A0A0"), "#", "any characters on line after this ignored"));
		foreach (var command in scriptCommandList)
		{
			if (LogicBase.IsDeprecated(command))
				continue;
			var helpReference = new HelpReference(command, DefaultItemImage);
			HelpReferences.Add(helpReference);
		}
	}
	public static HelpWindow Open()
	{
		var result = new FunctionsHelp();
		ImGuiWindowManager.Open(result);
		return result;
	}
}
public class SlotVariablesHelp : HelpWindow
{
	static Sprite _defaultItemImage = null!, _defaultItemImage2 = null!;
	protected override Sprite DefaultItemImage => _defaultItemImage;
	protected override Sprite DefaultItemImage2 => _defaultItemImage2;
	static SlotVariablesHelp()
	{
		var helpWindow = Resources.FindObjectsOfTypeAll<ScriptHelpWindow>().FirstOrDefault(x => x.name.Contains("HelpSlotVariables"));
		if (helpWindow is not null)
		{
			_defaultItemImage = helpWindow.DefaultItemImage;
			_defaultItemImage2 = helpWindow.DefaultItemImage2;
		}
		else
			CodeEditorMod.Instance.Logger.LogError("Could not find ScriptHelpWindow prefab");
	}
	public SlotVariablesHelp() : base("Slot Variables")
	{
		foreach (var logicSlotType in Logicable.LogicSlotTypes)
		{
			if (logicSlotType != LogicSlotType.None && !LogicBase.IsDeprecated(logicSlotType))
			{
				HelpReferences.Add(new HelpReference(
					logicSlotType.ToString(),
					Color.FromString("#FFA500"),
					LogicBase.GetLogicDescription(logicSlotType),
					this.DefaultItemImage,
					"LogicSlotType",
					(int) logicSlotType));
			}
		}
	}
	public static HelpWindow Open()
	{
		var result = new SlotVariablesHelp();
		ImGuiWindowManager.Open(result);
		return result;
	}
}
