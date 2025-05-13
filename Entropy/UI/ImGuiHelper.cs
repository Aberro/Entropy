using Assets.Scripts.UI.ImGuiUi;
using Entropy.UI.ImGUI;
using ImGuiNET;
using UnityEngine;

namespace Entropy.UI;

/// <summary>
/// A static class that provides helper methods for ImGui.
/// </summary>
public static class ImGuiHelper
{
	/// <summary>
	/// Displays a regular window with a title.
	/// </summary>
	/// <param name="name">The name of the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the window is open, otherwise <see langword="false"/>.</returns>
	public static bool Window(string name, Action body)
	{
		var result = ImGui.Begin(name);
		if(result)
		{
			body();
			ImGui.End();
		}
		return result;
	}

	/// <summary>
	/// Displays a regular window with a title.
	/// </summary>
	/// <param name="name">The name of the window.</param>
	/// <param name="flags">The flags for the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the window is open, otherwise <see langword="false"/>.</returns>
	public static bool Window(string name, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.Begin(name, flags);
		if(result)
		{
			body();
			ImGui.End();
		}
		return result;
	}

	/// <summary>
	/// Displays a regular window with a title.
	/// </summary>
	/// <param name="name">The name of the window.</param>
	/// <param name="open">A reference to a boolean value indicating if the window is open.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the window is open, otherwise <see langword="false"/>.</returns>
	public static bool Window(string name, ref bool open, Action body)
	{
		if(ImGui.Begin(name, ref open))
		{
			body();
			ImGui.End();
		}
		return open;
	}

	/// <summary>
	/// Displays a regular window with a title.
	/// </summary>
	/// <param name="name">The name of the window.</param>
	/// <param name="open">A reference to a boolean value indicating if the window is open.</param>
	/// <param name="flags">The flags for the window.</param>
/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the window is open, otherwise <see langword="false"/>.</returns>
	public static bool Window(string name, ref bool open, ImGuiWindowFlags flags, Action body)
	{
		if(ImGui.Begin(name, ref open, flags))
		{
			body();
			ImGui.End();
		}
		return open;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, Vector2 size, Action body)
	{
		var result = ImGui.BeginChild(str_id, size);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(str_id, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, Vector2 size, bool border, Action body)
	{
		var result = ImGui.BeginChild(str_id, size, border);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(str_id, size, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, bool border, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(str_id, border, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="str_id">The ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(string str_id, Vector2 size, bool border, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(str_id, size, border, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, Action body)
	{
		var result = ImGui.BeginChild(id);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, Vector2 size, Action body)
	{
		var result = ImGui.BeginChild(id, size);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, bool border, Action body)
	{
		var result = ImGui.BeginChild(id, border);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(id, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, Vector2 size, bool border, Action body)
	{
		var result = ImGui.BeginChild(id, size, border);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(id, size, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, bool border, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(id, border, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Displays a self-contained independent scrolling/clipping regions within a host window. Child windows can embed their own child.
	/// </summary>
	/// <remarks>
	/// When component of vector <paramref name="size"/> is equal to 0, the child window will use the remaining space of the host window in that axis.
	/// When component of vector <paramref name="size"/> is less than 0, the child window will use the remaining space of the host window in that axis minus the absolute value of the component.
	/// </remarks>
	/// <param name="id">The unique ID of the child window.</param>
	/// <param name="size">The size of the child window.</param>
	/// <param name="border">Flag indicating if the child window should have a border.</param>
	/// <param name="flags">Flags for the child window behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	public static bool Child(uint id, Vector2 size, bool border, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChild(id, size, border, flags);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	/// <summary>
	/// Locks horizontal starting position of elements displayed in <paramref name="body"/> action and captures the whole group bounding box into one "item" (so you can use IsItemHovered() or layout primitives such as SameLine() on whole group, etc.)
	/// </summary>
	/// <param name="body">The action to be executed to populate child elements.</param>
	public static void Group(Action body)
	{
		ImGui.BeginGroup();
		body();
		ImGui.EndGroup();
	}

	/// <summary>
	/// A helper class for <see cref="O:ImGuiNET.ImGui.BeginCombo"/> to automatically call <see cref="ImGui.EndCombo()"/>.
	/// </summary>
	/// <param name="label">The label of the combobox.</param>
	/// <param name="current_item">The currently selected item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the combobox is open, otherwise <see langword="false"/>.</returns>
	public static bool Combo(string label, string current_item, Action body)
	{
		var result = ImGui.BeginCombo(label, current_item);
		if(result)
		{
			body();
			ImGui.EndCombo();
		}
		return result;
	}

	/// <summary>
	/// Displays a combo box with the specified label and current item and populates it with selectable items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label"></param>
	/// <param name="current_item"></param>
	/// <param name="flags">The flags for the combobox.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the combo is open, otherwise <see langword="false"/>.</returns>
	public static bool Combo(string label, string current_item, ImGuiComboFlags flags, Action body)
	{
		var result = ImGui.BeginCombo(label, current_item, flags);
		if(result)
		{
			body();
			ImGui.EndCombo();
		}
		return result;
	}

	/// <summary>
	/// A helper method to create a child window / scrolling region that looks like a normal widget frame.
	/// </summary>
	/// <param name="id">The unique ID of the child frame.</param>
	/// <param name="size">The size of the child frame.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child frame is visible, otherwise <see langword="false"/>.</returns>
	public static bool ChildFrame(uint id, Vector2 size, Action body)
	{
		var result = ImGui.BeginChildFrame(id, size);
		if(result)
			body();
		ImGui.EndChildFrame();
		return result;
	}

	/// <summary>
	/// A helper method to create a child window / scrolling region that looks like a normal widget frame.
	/// </summary>
	/// <param name="id">The unique ID of the child frame.</param>
	/// <param name="size">The size of the child frame.</param>
	/// <param name="flags">Flags for the child frame behavior.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child frame is visible, otherwise <see langword="false"/>.</returns>
	public static bool ChildFrame(uint id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginChildFrame(id, size, flags);
		if(result)
			body();
		ImGui.EndChildFrame();
		return result;
	}

	/// <summary>
	/// Disables all user interactions and dim items visuals (applying <see cref="ImGuiStyle.DisabledAlpha"/> over current colors).
	/// </summary>
	/// <param name="disabled">Flag indicating if the items should be disabled.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	public static void Disabled(bool disabled, Action body)
	{
		if(disabled)
			ImGui.BeginDisabled();
		body();
		if(disabled)
			ImGui.EndDisabled();
	}

	/// <summary>
	/// Displays a list box with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the list box.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the list box is visible, otherwise <see langword="false"/>.</returns>
	public static bool ListBox(string label, Action body)
	{
		var result = ImGui.BeginListBox(label);
		if(result)
		{
			body();
			ImGui.EndListBox();
		}
		return result;
	}

	/// <summary>
	/// Displays a menu bar at the top of a window (should be called inside a window body) and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu bar is visible, otherwise <see langword="false"/>.</returns>
	public static bool MenuBar(Action body)
	{
		var result = ImGui.BeginMenuBar();
		if(result)
		{
			body();
			ImGui.EndMenuBar();
		}
		return result;
	}

	/// <summary>
	/// Displays a menu bar at the top of the screen and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu bar is visible, otherwise <see langword="false"/>.</returns>
	public static bool MainMenuBar(Action body)
	{
		var result = ImGui.BeginMainMenuBar();
		if(result)
		{
			body();
			ImGui.EndMainMenuBar();
		}
		return result;
	}

	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	public static bool Menu(string label, Action? body = null)
	{
		var result = ImGui.BeginMenu(label);
		if(result)
		{
			if(body is not null)
				body();
			ImGui.EndMenu();
		}
		return result;
	}

	/// <summary>
	/// Displays a tooltip window, that follows the mouse. Tooltip windows do not take focus away.
	/// </summary>
	/// <param name="body">The action to be executed to populate child elements.</param>
	public static void Tooltip(Action body)
	{
		ImGui.BeginTooltip();
		body();
		ImGui.EndTooltip();
	}

	/// <summary>
	/// Displas a popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it. The popup will be closed automatically if the user clicks outside of it or presses the escape key.
	/// </summary>
	/// <param name="str_id">The unique ID of the popup window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool Popup(string str_id, Action body)
	{
		var result = ImGui.BeginPopup(str_id);
		if(result)
		{
			body();
			ImGui.EndPopup();
		}
		return result;
	}

	/// <summary>
	/// Displas a popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it.
	/// The popup will be closed automatically if the user clicks outside of it or presses the escape key.
	/// </summary>
	/// <param name="str_id">The unique ID of the popup window.</param>
	/// <param name="flags">The flags for the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool Popup(string str_id, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginPopup(str_id, flags);
		if(result)
		{
			body();
			ImGui.EndPopup();
		}
		return result;
	}

	/// <summary>
	/// Displays a modal popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Modal popup windows are special types of popups that block user input to other windows until they are closed and adds a dimming background and have a title bar.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it.
	/// </summary>
	/// <param name="str_id">The unique ID of the popup window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool PopupModal(string str_id, Action body)
	{
		var result = ImGui.BeginPopupModal(str_id);
		if(result)
		{
			body();
			ImGui.EndPopup();
		}
		return result;
	}

	/// <summary>
	/// Displays a modal popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Modal popup windows are special types of popups that block user input to other windows until they are closed and adds a dimming background and have a title bar.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it.
	/// </summary>
	/// <param name="str_id">The unique ID of the popup window.</param>
	/// <param name="flags">The flags for the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool PopupModal(string str_id, ImGuiWindowFlags flags, Action body)
	{
		var result = ImGui.BeginPopupModal(str_id, flags);
		if(result)
		{
			body();
			ImGui.EndPopup();
		}
		return result;
	}

	/// <summary>
	/// Displays a modal popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Modal popup windows are special types of popups that block user input to other windows until they are closed and adds a dimming background and have a title bar.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it.
	/// </summary>
	/// <param name="str_id"></param>
	/// <param name="open">if set to <see langword="false"/>, will close the popup, otherwise will be set to the state of the popup window (i.e. <see langword="true"/> if the popup is open and <see langword="false"/> if it is closed).</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool PopupModal(string str_id, ref bool open, Action body)
	{
		if(ImGui.BeginPopupModal(str_id, ref open))
		{
			body();
			ImGui.EndPopup();
		}
		return open;
	}

	/// <summary>
	/// Displays a modal popup window with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// Modal popup windows are special types of popups that block user input to other windows until they are closed and adds a dimming background and have a title bar.
	/// Needs to be used in conjunction with <see cref="ImGui.OpenPopup(string)"/> or <see cref="ImGui.OpenPopupOnItemClick(string)"/> to open the popup,
	/// and <see cref="ImGui.CloseCurrentPopup()"/> to close it.
	/// </summary>
	/// <param name="str_id"></param>
	/// <param name="open">if set to <see langword="false"/>, will close the popup, otherwise will be set to the state of the popup window (i.e. <see langword="true"/> if the popup is open and <see langword="false"/> if it is closed).</param>
	/// <param name="flags">The flags for the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the popup is open, otherwise <see langword="false"/>.</returns>
	public static bool PopupModal(string str_id, ref bool open, ImGuiWindowFlags flags, Action body)
	{
		if(ImGui.BeginPopupModal(str_id, ref open, flags))
		{
			body();
			ImGui.EndPopup();
		}
		return open;
	}

	/// <summary>
	/// Displays a tab bar with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="str_id">The unique ID of the tab bar.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab bar is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabBar(string str_id, Action body)
	{
		var result = ImGui.BeginTabBar(str_id);
		if(result)
		{
			body();
			ImGui.EndTabBar();
		}
		return result;
	}

	/// <summary>
	/// Displays a tab bar with the specified ID and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="str_id">The unique ID of the tab bar.</param>
	/// <param name="flags">The flags for the tab bar.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab bar is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabBar(string str_id, ImGuiTabBarFlags flags, Action body)
	{
		var result = ImGui.BeginTabBar(str_id, flags);
		if(result)
		{
			body();
			ImGui.EndTabBar();
		}
		return result;
	}

	public static unsafe bool VerticalTabBar(string str_id, Action body)
	{

		var g = (ImGuiContext*)ImGui.GetCurrentContext();
		return false;
		var window = ImGui.GetCurrentWindow();
		if(window.SkipItems)
			return false;
		//var g = (ImGuiContext*)ImGui.GetCurrentContext();
		//var id = window.GetID(str_id);
		//ref var tab_bar = ref g->TabBars.GetOrAddByKey(id);
		//Rect tab_bar_bb = Rect.MinMaxRect(window.DC.CursorPos.x, window.DC.CursorPos.y, window.WorkRect.xMax,
		//	window.DC.CursorPos.y + g->FontSize + g->Style.FramePadding.y * 2);
		//tab_bar.ID = id;
		//tab_bar.SeparatorMinX = tab_bar->BarRect.Min.x - IM_TRUNC(window->WindowPadding.x * 0.5f);
		//tab_bar.SeparatorMaxX = tab_bar->BarRect.Max.x + IM_TRUNC(window->WindowPadding.x * 0.5f);
		////if (g.NavWindow && IsWindowChildOf(g.NavWindow, window, false, false))
		//flags |= ImGuiTabBarFlags_IsFocused;
		//return BeginTabBarEx(tab_bar, tab_bar_bb, flags);
	}

	/// <summary>
	/// Displays a tab item with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the tab item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab item is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabItem(string label, Action body)
	{
		var result = ImGui.BeginTabItem(label);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}

	/// <summary>
	/// Displays a tab item with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the tab item.</param>
	/// <param name="open">if set to <see langword="false"/>, will not display the tab item, otherwise will be set to the state of the tab item (i.e. <see langword="true"/> if the tab item is visible and <see langword="false"/> otherwise).</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab item is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabItem(string label, ref bool open, Action body)
	{
		var result = ImGui.BeginTabItem(label);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}

	/// <summary>
	/// Displays a tab item with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the tab item.</param>
	/// <param name="flags">The flags for the tab item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab item is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabItem(string label, ImGuiTabItemFlags flags, Action body)
	{
		var result = ImGui.BeginTabItem(label, flags);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}

	/// <summary>
	/// Displays a tab item with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the tab item.</param>
	/// <param name="open">if set to <see langword="false"/>, will not display the tab item, otherwise will be set to the state of the tab item (i.e. <see langword="true"/> if the tab item is visible and <see langword="false"/> otherwise).</param>
	/// <param name="flags">The flags for the tab item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab item is visible, otherwise <see langword="false"/>.</returns>
	public static bool TabItem(string label, ref bool open, ImGuiTabItemFlags flags, Action body)
	{
		var result = ImGui.BeginTabItem(label, ref open, flags);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}
}