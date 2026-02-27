using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.UI;
using Entropy.CodeEditor.UI;
using Entropy.CodeEditor.UI.TextEditor;
using Entropy.Common.Attributes;
using Entropy.Common.Services;
using Entropy.Common.UI;
using Entropy.Common.UI.ImGUI;
using Entropy.Common.Utils;
using HarmonyLib;
using ImGuiNET;
using System.Globalization;
using System.Text.RegularExpressions;
using UI.ImGuiUi.ImGuiWindows;
using UnityEngine;
using ImGuiWindow = UI.ImGuiUi.ImGuiWindows.ImGuiWindow;
using ImGuiWindowFlags = Entropy.Common.UI.ImGUI.ImGuiWindowFlags;
using TextEditor = Entropy.CodeEditor.UI.TextEditor.TextEditor;

namespace Entropy.CodeEditor;

public class SourceCodeEditor : ImGuiWindow, IModal
{
	private enum ColorSchemes
	{
		Light,
		Dark,
		Retro,
		User,
	}
	private static class IC10LanguageDefinition
	{
		private static Regex IC10TokenizerRegex = new(@"^(?:[ \t]*)(?<label>[a-z_][a-z0-9_.]*(?:[ \t]*)(?::))?(?:[ \t]*)((?<instruction>[a-z_][a-z0-9_.]*)((?:[ \t]*)((?<hash>HASH\(""""[^""""\r\n]+""""\))|(?<register>(dr*|r+)\d{1,2})|(?<id>[a-z_][a-z0-9_.]*)|(?<number>(-?[0-9]+(.[0-9]+)?([e][+\-]?[0-9]+)?)|(\$[0-9a-f]+))))*)?(?:[ \t]*)(?<comment>\#[^\r\n]*)?",
			RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
		public static LanguageDefinition Definition => new()
		{
			Name = "IC10",
			Keywords = [.. ProcessorProvider.ListOfCommands()],
			PreprocIdentifiers = ProcessorProvider.ListOfIdentifiers().ToDictionary(x => x.Key, x => new Identifier { Declaration = x.Value }),
			AutoIndentation = true,
			SingleLineComment = "#",
			CaseSensitive = false,
			Tokenize = Tokenizer,
		};

		private static IEnumerable<(ArraySegment<char>, PaletteIndex)> Tokenizer(char[] line, int start, int length)
		{
			var str = new string(line, start, length);
			var match = IC10TokenizerRegex.Match(str);

			return ReturnCaptures(line, match, "label", PaletteIndex.Preprocessor)
				.Concat(ReturnCaptures(line, match, "instruction", PaletteIndex.Keyword))
				.Concat(ReturnCaptures(line, match, "hash", PaletteIndex.CharLiteral))
				.Concat(ReturnCaptures(line, match, "register", PaletteIndex.Keyword))
				.Concat(ReturnCaptures(line, match, "id", PaletteIndex.Identifier))
				.Concat(ReturnCaptures(line, match, "number", PaletteIndex.Number))
				.Concat(ReturnCaptures(line, match, "comment", PaletteIndex.Comment));
		}

		private static IEnumerable<(ArraySegment<char>, PaletteIndex)> ReturnCaptures(char[] buffer, Match match, string groupName, PaletteIndex color)
		{
			var collection = match.Groups[groupName];
			var captures = collection.Captures;
			if (collection.Success)
			{
				foreach (Capture capture in captures)
					yield return (new ArraySegment<char>(buffer, capture.Index, capture.Length), color);
			}
		}
	}

	private readonly List<ImGuiWindow> _openedWindows = [];
	private TextEditor _textEditor = null!;
	private Traverse _motherboardTraverse = null!;
	private List<ICircuitHolder> _circuitHolders = null!;
	private ICircuitHolder? _selectedCircuitHolder = null!;
	[AutoConfigDefinition("Automatically pause the game while code editor is open", DisplayName = "Autopause game", DefaultValue = true)]
	private static bool AutoPause { get; set; }
	[AutoConfigDefinition("Color scheme for the code editor", DisplayName = "Color scheme", DefaultValue = ColorSchemes.Dark)]
	private static ColorSchemes ColorScheme { get; set; }
	[AutoConfigDefinition("", 800, 600, Visible = false)]
	private static Vector2 WindowSize { get; set; }
	[AutoConfigDefinition("", 200, 50, Visible = false)]
	private static Vector2 WindowPosition { get; set; }

	[AutoConfigDefinition("", Visible = false)]
	private static Color Default { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Keyword { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Number { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color String { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color CharLiteral { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Punctuation { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Preprocessor { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Identifier { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color KnownIdentifier { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color PreprocIdentifier { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Comment { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color MultiLineComment { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Background { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Cursor { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Selection { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color ErrorMarker { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color Breakpoint { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color LineNumber { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color CurrentLineFill { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color CurrentLineFillInactive { get; set; }
	[AutoConfigDefinition("", Visible = false)]
	private static Color CurrentLineEdge { get; set; }
	// We can't have arrays as auto-property, so this wrapper is used for convenience.
	// It's not performance optimized, because there's no need to as it's intended to be invoked only when
	// pallete is set by menu item.
	private static Color[] UserPaletteWrapper
	{
		get => [
			Default,
			Keyword,
			Number,
			String,
			CharLiteral,
			Punctuation,
			Preprocessor,
			Identifier,
			KnownIdentifier,
			PreprocIdentifier,
			Comment,
			MultiLineComment,
			Background,
			Cursor,
			Selection,
			ErrorMarker,
			Breakpoint,
			LineNumber,
			CurrentLineFill,
			CurrentLineFillInactive,
			CurrentLineEdge
		];
	}
	private bool _showCustomPalette;
	public ProgrammableChipMotherboard Motherboard
	{
		get;
		private set
		{
			field = value;
			this._motherboardTraverse = new Traverse(field);
			// The field is readonly, so we can read it's value to always have access to the list.
			this._circuitHolders = this._motherboardTraverse.Field<List<ICircuitHolder>>("_circuitHolders").Value;
		}
	}

	public bool UnlockCursor => true;
	public override ImGuiNET.ImGuiWindowFlags Flags => ImGuiNET.ImGuiWindowFlags.MenuBar;

	public SourceCodeEditor(ProgrammableChipMotherboard motherboard) : base("Entropy.Editor", WindowSize)
	{
		Motherboard = motherboard;
		this._textEditor = new TextEditor(IC10LanguageDefinition.Definition);
	}

	private static SourceCodeEditor? _instance;
	public static void Open(ProgrammableChipMotherboard motherboard)
	{
		PanelToolTip.Instance.ClearToolTip();
		_instance ??= new SourceCodeEditor(motherboard);
		_instance.Motherboard = motherboard;
		ImGuiWindowManager.Open(_instance);
	}
	public override void OnOpen()
	{
		PanelToolTip.Instance.ClearToolTip();
		var traverse = new Traverse(this);
		traverse.Field<Vector2>("_initialSize").Value = WindowSize;
		traverse.Field<Vector2>("_initialPosition").Value = WindowPosition;
		_textEditor.Text = Motherboard.GetSourceCode();
		this._textEditor.UnsavedChanges = Motherboard.UnsavedChanges;
		MouseModeController.AddModal(this);
		SetColorScheme();
		if (AutoPause)
			WorldManager.SetGamePause(true);
		// restore previously opened windows
		foreach(var openedWindow in _openedWindows)
			ImGuiWindowManager.Open(openedWindow);
	}
	public override void OnClose()
	{
		if (WorldManager.IsGamePaused)
			WorldManager.SetGamePause(false);
		MouseModeController.RemoveModal(this);
		Motherboard.UnsavedChanges = this._textEditor.UnsavedChanges;
		Motherboard.SetSourceCode(this._textEditor.Text);
		Motherboard.SendUpdate();
		foreach(var openedWindow in _openedWindows)
			if(openedWindow.IsShowing)
				openedWindow.CloseWindow();
	}
	public override void DrawContent()
	{
		for(var i = _openedWindows.Count-1; i >= 0; i--)
		{
			if(!_openedWindows[i].IsShowing)
				_openedWindows.RemoveAt(i);
		}
		ImGuiHelper.KeyPressed(ImGuiKey.Escape, this.CloseWindow);
		var cpos = this._textEditor.CursorPosition;

		if (this._selectedCircuitHolder is not null && !this._circuitHolders.Contains(this._selectedCircuitHolder))
		{
			this._selectedCircuitHolder = null;
		}
		ImGuiHelper.MenuBar(() =>
		{
			ImGuiHelper.Menu("File", () =>
			{
				ImGuiHelper.MenuItem("Import", () => { });
				ImGuiHelper.Menu("Import from", () =>
				{
					foreach (var holder in this._circuitHolders)
					{
						var thing = (Thing) holder;
						ImGuiHelper.MenuItem(thing.DisplayName, () =>
						{
							this._textEditor.Text = holder.GetSourceCode();
							this._textEditor.ResetUnsavedChanges();
							this._selectedCircuitHolder = holder;
						});
					}
				});
				ImGuiHelper.MenuItem("Export", false, this._selectedCircuitHolder != null, () =>
				{
					if (this._selectedCircuitHolder != null)
					{
						this._selectedCircuitHolder.SetSourceCode(this._textEditor.Text);
						this._textEditor.ResetUnsavedChanges();
					}
				});
				ImGuiHelper.Menu("Export to", () =>
				{
					foreach (var holder in this._circuitHolders)
					{
						var thing = (Thing) holder;
						ImGuiHelper.MenuItem(thing.DisplayName, () =>
						{
							holder.SetSourceCode(this._textEditor.Text);
							this._selectedCircuitHolder = holder;
							this._textEditor.ResetUnsavedChanges();
						});
					}
				});
				ImGuiHelper.MenuItem("Close", "Alt-F4", () =>
				{
					this.CloseWindow();
				});
			});
			ImGuiHelper.Menu("Edit", () =>
			{
				ImGuiHelper.MenuItem("Undo", "Ctrl-Z", false, this._textEditor.CanUndo(), this._textEditor.Undo);
				ImGuiHelper.MenuItem("Redo", "Ctrl-Y", false, this._textEditor.CanRedo(), this._textEditor.Redo);
				ImGui.Separator();
				ImGuiHelper.MenuItem("Copy", "Ctrl-C", false, this._textEditor.HasSelection(), this._textEditor.Copy);
				ImGuiHelper.MenuItem("Cut", "Ctrl-X", false, this._textEditor.HasSelection(), this._textEditor.Cut);
				ImGuiHelper.MenuItem("Delete", "Del", false, this._textEditor.HasSelection(), () => this._textEditor.Delete(false, false));
				ImGuiHelper.MenuItem("Paste", "Ctrl-V", false, ImGui.GetClipboardText() != null, this._textEditor.Paste);
				ImGui.Separator();
				ImGuiHelper.MenuItem("Select all", "Ctrl-A", () =>
					this._textEditor.SetSelection(new Coordinates(0, 0), new Coordinates(this._textEditor.TotalLines, 0)));
			});

			ImGuiHelper.Menu("View", () =>
			{
				ImGuiHelper.MenuItem("Original Editor", () =>
				{
					this.CloseWindow();
					InputSourceCode.Instance.PCM = Motherboard;
					if (InputSourceCode.ShowInputPanel("Edit Script", Motherboard.GetSourceCode()))
					{
						InputSourceCode.OnSubmit += Motherboard.InputFinished;
					}
				});
				ImGui.Separator();
				ImGuiHelper.MenuItem("Dark palette", () =>
				{
					ColorScheme = ColorSchemes.Dark;
					SetColorScheme();
				});
				ImGuiHelper.MenuItem("Light palette", () =>
				{
					ColorScheme = ColorSchemes.Light;
					SetColorScheme();
				});
				ImGuiHelper.MenuItem("Retro blue palette", () =>
				{
					ColorScheme = ColorSchemes.Retro;
					SetColorScheme();
				});
				ImGuiHelper.MenuItem("Custom...", () =>
				{
					ColorScheme = ColorSchemes.User;
					_showCustomPalette = true;
				});
			});
			ImGuiHelper.Menu("Game", () =>
			{
				ImGuiHelper.MenuItem("Autopause", AutoPause, () => AutoPause = !AutoPause);
				ImGuiHelper.MenuItem("Play", false, WorldManager.IsGamePaused, () => WorldManager.SetGamePause(false));
				ImGuiHelper.MenuItem("Pause", false, !WorldManager.IsGamePaused, () => WorldManager.SetGamePause(true));
			});
			ImGuiHelper.Menu("Help", () =>
			{
				ImGuiHelper.MenuItem("Stationpedia...", Stationpedia.Open);
				ImGuiHelper.MenuItem("Functions...", () => { _openedWindows.Add(FunctionsHelp.Open()); });
				ImGuiHelper.MenuItem("Variables...", () => { _openedWindows.Add(VariablesHelp.Open()); });
				ImGuiHelper.MenuItem("Slot variables...", () => { _openedWindows.Add(SlotVariablesHelp.Open()); });
			});
		});

		ImGui.Text(string.Format(CultureInfo.InvariantCulture,
			"{0,6}/{1,-6} {2,6} lines  | {3} | {4} | {5} |",
			cpos.Line + 1,
			cpos.Column + 1,
			this._textEditor.TotalLines,
			this._textEditor.IsOverwrite() ? "Ovr" : "Ins",
			this._textEditor.UnsavedChanges ? "*" : " ",
			this._textEditor.LanguageDefinition.Name));
		ImGui.SameLine();
		var programName = Motherboard.ProgramName ?? (_selectedCircuitHolder is Thing t? t.DisplayName : "New program");
		if(ImGui.InputText("", ref programName, 64))
			Motherboard.ProgramName = programName;


		this._textEditor.Render("TextEditor");
		if(_showCustomPalette)
		{
			ImGuiHelper.Window("Custom palette", ref _showCustomPalette, CustomPaletteWindow);
		}
		WindowPosition = ImGui.GetWindowPos();
		WindowSize = ImGui.GetWindowSize();
	}

	private void CustomPaletteWindow()
	{
		Color[]? importPalette = null;
		ImGuiHelper.Button("From Dark", () => importPalette = TextEditor.DarkPalette);
		ImGui.SameLine();
		ImGuiHelper.Button("From Light", () => importPalette = TextEditor.LightPalette);
		ImGui.SameLine();
		ImGuiHelper.Button("From Retro", () => importPalette = TextEditor.RetroBluePalette);

		if(importPalette != null)
		{
			for(var i = 0; i < (int)PaletteIndex.Max; i++)
			{
				switch((PaletteIndex)i)
				{
				case PaletteIndex.Default:
					Default = importPalette[i];
					break;
				case PaletteIndex.Keyword:
					Keyword = importPalette[i];
					break;
				case PaletteIndex.Number:
					Number = importPalette[i];
					break;
				case PaletteIndex.String:
					String = importPalette[i];
					break;
				case PaletteIndex.CharLiteral:
					CharLiteral = importPalette[i];
					break;
				case PaletteIndex.Punctuation:
					Punctuation = importPalette[i];
					break;
				case PaletteIndex.Preprocessor:
					Preprocessor = importPalette[i];
					break;
				case PaletteIndex.Identifier:
					Identifier = importPalette[i];
					break;
				case PaletteIndex.KnownIdentifier:
					KnownIdentifier = importPalette[i];
					break;
				case PaletteIndex.PreprocIdentifier:
					PreprocIdentifier = importPalette[i];
					break;
				case PaletteIndex.Comment:
					Comment = importPalette[i];
					break;
				case PaletteIndex.MultiLineComment:
					MultiLineComment = importPalette[i];
					break;
				case PaletteIndex.Background:
					Background = importPalette[i];
					break;
				case PaletteIndex.Cursor:
					Cursor = importPalette[i];
					break;
				case PaletteIndex.Selection:
					Selection = importPalette[i];
					break;
				case PaletteIndex.ErrorMarker:
					ErrorMarker = importPalette[i];
					break;
				case PaletteIndex.Breakpoint:
					Breakpoint = importPalette[i];
					break;
				case PaletteIndex.LineNumber:
					LineNumber = importPalette[i];
					break;
				case PaletteIndex.CurrentLineFill:
					CurrentLineFill = importPalette[i];
					break;
				case PaletteIndex.CurrentLineFillInactive:
					CurrentLineFillInactive = importPalette[i];
					break;
				case PaletteIndex.CurrentLineEdge:
					CurrentLineEdge = importPalette[i];
					break;
				}
			}
		}
		ImGuiHelper.ColorEdit3("Default", Default, (color) => Default = color);
		ImGuiHelper.ColorEdit3("Keyword", Keyword, (color) => Keyword = color);
		ImGuiHelper.ColorEdit3("Number", Number, (color) => Number = color);
		ImGuiHelper.ColorEdit3("String", String, (color) => String = color);
		ImGuiHelper.ColorEdit3("Char literal", CharLiteral, (color) => CharLiteral = color);
		ImGuiHelper.ColorEdit3("Punctuation", Punctuation, (color) => Punctuation = color);
		ImGuiHelper.ColorEdit3("Preprocessor", Preprocessor, (color) => Preprocessor = color);
		ImGuiHelper.ColorEdit3("Identifier", Identifier, (color) => Identifier = color);
		ImGuiHelper.ColorEdit3("Known identifier", KnownIdentifier, (color) => KnownIdentifier = color);
		ImGuiHelper.ColorEdit3("Preproc identifier", PreprocIdentifier, (color) => PreprocIdentifier = color);
		ImGuiHelper.ColorEdit3("Comment (single line)", Comment, (color) => Comment = color);
		ImGuiHelper.ColorEdit3("Comment (multi line)", MultiLineComment, (color) => MultiLineComment = color);
		ImGuiHelper.ColorEdit3("Background", Background, (color) => Background = color);
		ImGuiHelper.ColorEdit3("Cursor", Cursor, (color) => Cursor = color);
		ImGuiHelper.ColorEdit3("Selection", Selection, (color) => Selection = color);
		ImGuiHelper.ColorEdit3("ErrorMarker", ErrorMarker, (color) => ErrorMarker = color);
		ImGuiHelper.ColorEdit3("Breakpoint", Breakpoint, (color) => Breakpoint = color);
		ImGuiHelper.ColorEdit3("Line number", LineNumber, (color) => LineNumber = color);
		ImGuiHelper.ColorEdit3("Current line fill", CurrentLineFill, (color) => CurrentLineFill = color);
		ImGuiHelper.ColorEdit3("Current line fill (inactive)", CurrentLineFillInactive, (color) => CurrentLineFillInactive = color);
		ImGuiHelper.ColorEdit3("Current line edge", CurrentLineEdge, (color) => CurrentLineEdge = color);
		ImGuiHelper.Button("Close", () => _showCustomPalette = false);
		SetColorScheme();
	}

	private void SetColorScheme()
	{
		switch(ColorScheme)
		{
			case ColorSchemes.Light:
				this._textEditor.Palette = TextEditor.LightPalette;
				break;
			case ColorSchemes.Dark:
				this._textEditor.Palette = TextEditor.DarkPalette;
				break;
			case ColorSchemes.Retro:
				this._textEditor.Palette = TextEditor.RetroBluePalette;
				break;
			case ColorSchemes.User:
				this._textEditor.Palette = UserPaletteWrapper;
				break;
		}
	}
}