using System.Text.RegularExpressions;
using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.UI;
using Entropy.Scripts.Processor;
using Entropy.Scripts.UI.TextEditor;
using HarmonyLib;
using ImGuiNET;
using JetBrains.Annotations;
using UnityEngine;

namespace Entropy.Scripts.UI;

public class SourceCodeEditor : MonoBehaviour
{
	private static class IC10LanguageDefinition
	{
		private static readonly Regex IC10TokenizerRegex = new(@"^(?:[ \t]*)(?<label>[a-z_][a-z0-9_.]*(?:[ \t]*)(?::))?(?:[ \t]*)((?<instruction>[a-z_][a-z0-9_.]*)((?:[ \t]*)((?<hash>HASH\(""""[^""""\r\n]+""""\))|(?<register>(dr*|r+)\d{1,2})|(?<id>[a-z_][a-z0-9_.]*)|(?<number>(-?[0-9]+(.[0-9]+)?([e][+\-]?[0-9]+)?)|(\$[0-9a-f]+))))*)?(?:[ \t]*)(?<comment>\#[^\r\n]*)?",
			RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
		public static LanguageDefinition Definition => new()
		{
			mName = "IC10",
			mKeywords = [..ChipProcessor.GetInstructions()],
			Identifiers = ChipProcessor.GetEnumerationValues().ToDictionary(x => x, _ => new Identifier()),
			mAutoIndentation = true,
			mSingleLineComment = "#",
			mCaseSensitive = false,
			mTokenize = Tokenizer,
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
				foreach (Capture capture in captures)
					yield return (new ArraySegment<char>(buffer, capture.Index, capture.Length), color);
		}
	}
	private TextEditor.TextEditor _textEditor = null!;
	private ProgrammableChipMotherboard _motherboard = null!;
	private Traverse _motherboardTraverse = null!;
	private Traverse<List<ICircuitHolder>> _circuitHoldersTraverse = null!;
	private ICircuitHolder? _selectedCircuitHolder;
	private bool _editorOpen;

	public void ShowTextEditor()
	{
		this._editorOpen = true;
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		ImGuiUn.Layout += OnLayout;
		this._textEditor = new TextEditor.TextEditor(IC10LanguageDefinition.Definition);
		this._motherboard = this.gameObject.GetComponent<ProgrammableChipMotherboard>();
		this._motherboardTraverse = new Traverse(this._motherboard);
		this._circuitHoldersTraverse = this._motherboardTraverse.Field<List<ICircuitHolder>>("_circuitHolders");
	}

	[UsedImplicitly]
	void OnDisable() => ImGuiUn.Layout -= OnLayout;

    void OnLayout() => TextEditor();

    private bool TextEditor()
	{
		if (!this._editorOpen)
			return false;
		var cpos = this._textEditor.CursorPosition;
		var fileToEdit = this._selectedCircuitHolder is Thing thing ? thing.CustomName : "New program";
		if(this._textEditor.UnsavedChanges)
			fileToEdit += " *";
		ImGui.Begin("Text Editor test", ref this._editorOpen, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.MenuBar);
		CursorManager.SetCursor(!this._editorOpen);
		InputSourceCode.InputState = this._editorOpen ?  InputPanelState.Waiting : InputPanelState.None;
		if(!this._editorOpen && WorldManager.IsGamePaused)
		{
			WorldManager.SetGamePause(false);
		}
		ImGui.SetWindowSize(new Vector2(800, 600), ImGuiCond.FirstUseEver);
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.BeginMenu("File"))
			{
				if (ImGui.BeginMenu("Import"))
				{
					foreach(var holder in this._circuitHoldersTraverse.Value)
						if(ImGui.MenuItem(((Thing)holder).CustomName))
						{
							this._textEditor.Text = holder.GetSourceCode();
							this._textEditor.ResetUnsavedChanges();
							this._selectedCircuitHolder = holder;
						}
					ImGui.EndMenu();
				}
				if (ImGui.MenuItem("Export", false, this._selectedCircuitHolder != null))
				{
					if (this._selectedCircuitHolder != null)
					{
						this._selectedCircuitHolder.SetSourceCode(this._textEditor.Text);
						this._textEditor.ResetUnsavedChanges();
					}
				}
				if(ImGui.BeginMenu("Export to"))
				{
					foreach (var holder in this._circuitHoldersTraverse.Value)
						if (ImGui.MenuItem(((Thing)holder).CustomName))
						{
							holder.SetSourceCode(this._textEditor.Text);
							this._selectedCircuitHolder = holder;
							this._textEditor.ResetUnsavedChanges();
						}
					ImGui.EndMenu();
				}
				if (ImGui.MenuItem("Close", "Alt-F4"))
					Destroy(this);
				ImGui.EndMenu();
			}
			if (ImGui.BeginMenu("Edit"))
			{
				ImGui.Separator();

				if (ImGui.MenuItem("Undo", "Ctrl-Z", false, this._textEditor.CanUndo()))
					this._textEditor.Undo();
				if (ImGui.MenuItem("Redo", "Ctrl-Y", false, this._textEditor.CanRedo()))
					this._textEditor.Redo();

				ImGui.Separator();

				if (ImGui.MenuItem("Copy", "Ctrl-C", false, this._textEditor.HasSelection()))
					this._textEditor.Copy();
				if (ImGui.MenuItem("Cut", "Ctrl-X", false, this._textEditor.HasSelection()))
					this._textEditor.Cut();
				if (ImGui.MenuItem("Delete", "Del", false, this._textEditor.HasSelection()))
					this._textEditor.Delete();
				if (ImGui.MenuItem("Paste", "Ctrl-V", false, ImGui.GetClipboardText() != null))
					this._textEditor.Paste();

				ImGui.Separator();

				if (ImGui.MenuItem("Select all", "Ctrl-A"))
					this._textEditor.SetSelection(new Coordinates(0, 0), new Coordinates(this._textEditor.GetTotalLines(), 0));
				ImGui.EndMenu();
			}

			if (ImGui.BeginMenu("View"))
			{
				if (ImGui.MenuItem("Dark palette"))
					this._textEditor.Palette = UI.TextEditor.TextEditor.DarkPalette;
				if (ImGui.MenuItem("Light palette"))
					this._textEditor.Palette = UI.TextEditor.TextEditor.LightPalette;
				if (ImGui.MenuItem("Retro blue palette"))
					this._textEditor.Palette = UI.TextEditor.TextEditor.RetroBluePalette;
				ImGui.EndMenu();
			}
			if(ImGui.BeginMenu("Game"))
			{
				if (ImGui.MenuItem("Play", false, WorldManager.IsGamePaused))
				{
					WorldManager.SetGamePause(false);
				}
				if(ImGui.MenuItem("Pause", false, !WorldManager.IsGamePaused))
				{
					WorldManager.SetGamePause(true);
				}
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}

		ImGui.Text(string.Format(
			"{0,6}/{1,-6} {2,6} lines  | {3} | {4} | {5} | {6}",
			cpos.mLine + 1,
			cpos.mColumn + 1,
			this._textEditor.GetTotalLines(),
			this._textEditor.IsOverwrite() ? "Ovr" : "Ins",
			this._textEditor.CanUndo() ? "*" : " ",
			this._textEditor.LanguageDefinition.mName,
			fileToEdit));

		this._textEditor.Render("TextEditor");
		ImGui.End();
		return true;
	}
}