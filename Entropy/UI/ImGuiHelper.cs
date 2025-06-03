using Assets.Scripts.UI.ImGuiUi;
using Entropy.UI.ImGUI;
using ImGuiNET;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using ImGuiTabBarFlags = Entropy.UI.ImGUI.ImGuiTabBarFlags;
using ImGuiTabBarPtr = Entropy.UI.ImGUI.ImGuiTabBarPtr;
using ImS16 = System.Int16;
using ImGuiID = System.UInt32;
using ImGuiTabItemFlags = Entropy.UI.ImGUI.ImGuiTabItemFlags;
using ImGuiTabItem = Entropy.UI.ImGUI.ImGuiTabItem;
using ImGuiTabItemPtr = Entropy.UI.ImGUI.ImGuiTabItemPtr;
using Assets.Scripts.UI;
using ImGuiButtonFlags = Entropy.UI.ImGUI.ImGuiButtonFlags;
using ImGuiWindow = Entropy.UI.ImGUI.ImGuiWindow;
using ImGuiWindowPtr = Entropy.UI.ImGUI.ImGuiWindowPtr;
using ImGuiWindowFlags = Entropy.UI.ImGUI.ImGuiWindowFlags;

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
		var result = ImGui.Begin(name, (ImGuiNET.ImGuiWindowFlags)flags);
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
		if(ImGui.Begin(name, ref open, (ImGuiNET.ImGuiWindowFlags)flags))
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
		var result = ImGui.BeginChild(str_id, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(str_id, size, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(str_id, border, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(str_id, size, border, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(id, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(id, size, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(id, border, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChild(id, size, border, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginChildFrame(id, size, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginPopup(str_id, (ImGuiNET.ImGuiWindowFlags)flags);
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
		var result = ImGui.BeginPopupModal(str_id, (ImGuiNET.ImGuiWindowFlags)flags);
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
		if(ImGui.BeginPopupModal(str_id, ref open, (ImGuiNET.ImGuiWindowFlags)flags))
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
		var result = ImGui.BeginTabBar(str_id, (ImGuiNET.ImGuiTabBarFlags)flags);
		if(result)
		{
			body();
			ImGui.EndTabBar();
		}
		return result;
	}

	public static unsafe bool VerticalTabBar(string str_id, ImGuiTabBarFlags flags, Action body)
	{
		var result = BeginVerticalTabBar(str_id, flags);
		if(result)
		{
			body();
			EndVerticalTabBar();
		}
		return result;

		bool BeginVerticalTabBar(string str_id, ImGuiTabBarFlags flags)
		{
			//throw new NotImplementedException();
			var g = (ImGuiContextPtr)ImGui.GetCurrentContext();
			ImGuiWindow* window = g.CurrentWindow;
			if(window->SkipItems)
				return false;
			var id = window->GetID(str_id);
			ImGuiTabBarPtr tabBar = g.TabBars.GetOrAddByKey(id);
			// Original:
			var tabBarBoundingBox = new ImRect(window->DC.CursorPos.x, window->DC.CursorPos.y, window->WorkRect.Max.X, window->DC.CursorPos.y + g.FontSize + g.Style.FramePadding.Y * 2);
			// By the context it seems that it's bounding box for where tab bar is drawn, i.e. tab headers without tab content.
			// It case of horizontal tab bar, the height is easily determinable. In our case, neither width nor height is determinable until layout.
			// So reserve the whole remaining window space as bounding box.
			//var tabBarBoundingBox = new ImRect(window->DC.CursorPos.x, window->DC.CursorPos.y, window->WorkRect.xMax, window->WorkRect.yMax);
			tabBar.ID = id;
			flags |= ImGuiTabBarFlags.IsFocused;

			if((flags & ImGuiTabBarFlags.DockNode) == 0)
				ImGui.PushOverrideID(tabBar.ID);

			// Add to stack
			var tabBarRef = GetTabBarRefFromTabBar(tabBar);
			g.CurrentTabBarStack.Add(&tabBarRef);
			g.CurrentTabBar = tabBar;

			// Append with multiple BeginTabBar()/EndTabBar() pairs.
			tabBar.BackupCursorPos = window->DC.CursorPos;
			if(tabBar.CurrFrameVisible == g.FrameCount)
			{
				window->DC.CursorPos = new Vector2(tabBar.BarRect.Min.X, tabBar.BarRect.Max.Y + tabBar.ItemSpacingY);
				tabBar.BeginCount++;
				return true;
			}

			// Ensure correct ordering when toggling ImGuiTabBarFlags_Reorderable flag, or when a new tab was added while being not reorderable
			if((flags & ImGuiTabBarFlags.Reorderable) != (tabBar.Flags & ImGuiTabBarFlags.Reorderable) || (tabBar.TabsAddedNew && (flags & ImGuiTabBarFlags.Reorderable) == 0))
				igImQsort(tabBar.Tabs.GetPtr(0), (nuint)tabBar.Tabs.Size, (nuint)Marshal.SizeOf<ImGuiTabItem>(), TabItemComparerByBeginOrder);
			tabBar.TabsAddedNew = false;

			// Flags
			if((flags & ImGuiTabBarFlags.FittingPolicyMask) == 0)
				flags |= ImGuiTabBarFlags.FittingPolicyDefault;

			tabBar.Flags = flags;
			tabBar.BarRect = tabBarBoundingBox;
			tabBar.WantLayout = true; // Layout will be done on the first call to ItemTab()
			tabBar.PrevFrameVisible = tabBar.CurrFrameVisible;
			tabBar.CurrFrameVisible = g.FrameCount;
			tabBar.PrevTabsContentsHeight = tabBar.CurrTabsContentsHeight;
			tabBar.CurrTabsContentsHeight = 0.0f;
			tabBar.ItemSpacingY = g.Style.ItemSpacing.Y;
			tabBar.FramePadding = g.Style.FramePadding;
			tabBar.TabsActiveCount = 0;
			tabBar.BeginCount = 1;

			// Set cursor pos in a way which only be used in the off-chance the user erroneously submits item before BeginTabItem(): items will overlap
			window->DC.CursorPos = new Vector2(tabBar.BarRect.Min.X, tabBar.BarRect.Max.Y + tabBar.ItemSpacingY);

			// Draw separator
			var col = ImGui.GetColorU32((flags & ImGuiTabBarFlags.IsFocused) != 0 ? ImGuiCol.TabActive : ImGuiCol.TabUnfocusedActive);
			float y = tabBar.BarRect.Max.Y - 1.0f;
			{
				var separator_min_x = tabBar.BarRect.Min.X - (float)Math.Floor(window->WindowPadding.X * 0.5f);
				var separator_max_x = tabBar.BarRect.Max.X + (float)Math.Floor(window->WindowPadding.X * 0.5f);
				window->DrawListPtr.AddLine(new ImVec2(separator_min_x, y), new ImVec2(separator_max_x, y), col, 1.0f);
			}
			return true;
		}
		void EndVerticalTabBar()
		{
			var g = (ImGuiContextPtr)ImGui.GetCurrentContext();
			ImGuiWindowPtr window = g.CurrentWindow;
			if(window.SkipItems)
				return;

			ImGuiTabBarPtr tab_bar = g.CurrentTabBar;
			if(tab_bar.NativePtr == null)
			{
				throw new InvalidOperationException("Mismatched BeginTabBar()/EndTabBar()!");
			}

			// Fallback in case no TabItem have been submitted
			if(tab_bar.WantLayout)
				TabBarLayout(tab_bar);

			// Restore the last visible height if no tab is visible, this reduce vertical flicker/movement when a tabs gets removed without calling SetTabItemClosed().
			bool tab_bar_appearing = (tab_bar.PrevFrameVisible + 1 < g.FrameCount);
			if(tab_bar.VisibleTabWasSubmitted || tab_bar.VisibleTabId == 0 || tab_bar_appearing)
			{
				tab_bar.CurrTabsContentsHeight = Math.Max(window.DC.CursorPos.y - tab_bar.BarRect.Max.Y, tab_bar.CurrTabsContentsHeight);
				window.NativePtr->DC.CursorPos.y = tab_bar.BarRect.Max.Y + tab_bar.CurrTabsContentsHeight;
			}
			else
			{
				window.NativePtr->DC.CursorPos.y = tab_bar.BarRect.Max.Y + tab_bar.PrevTabsContentsHeight;
			}
			if(tab_bar.BeginCount > 1)
				window.NativePtr->DC.CursorPos = tab_bar.BackupCursorPos;

			if((tab_bar.Flags & ImGuiTabBarFlags.DockNode) == 0)
				ImGui.PopID();

			g.CurrentTabBarStack.Pop();
			g.CurrentTabBar = g.CurrentTabBarStack.IsEmpty ? null : GetTabBarFromTabBarRef(g.CurrentTabBarStack.GetLast());
		}

		void TabBarLayout(ImGuiTabBarPtr tab_bar)
		{
			ImGuiContextPtr g = ImGui.GetCurrentContext();
			tab_bar.WantLayout = false;

			// Garbage collect by compacting list
			// Detect if we need to sort out tab list (e.g. in rare case where a tab changed section)
			int tab_dst_n = 0;
			bool need_sort_by_section = false;
			var sections = stackalloc ImGuiTabBarSection[3]; // Layout sections: Leading, Central, Trailing
			for(int tab_src_n = 0; tab_src_n < tab_bar.Tabs.Size; tab_src_n++)
			{
				ImGuiTabItemPtr tab = tab_bar.Tabs.GetPtr(tab_src_n);
				if(tab.LastFrameVisible < tab_bar.PrevFrameVisible || tab.WantClose)
				{
					// Remove tab
					if(tab_bar.VisibleTabId == tab.ID)
						tab_bar.VisibleTabId = 0;
					if(tab_bar.SelectedTabId == tab.ID)
						tab_bar.SelectedTabId = 0;
					if(tab_bar.NextSelectedTabId == tab.ID)
						tab_bar.NextSelectedTabId = 0;
					continue;
				}
				if(tab_dst_n != tab_src_n)
					tab_bar.Tabs.Set(tab_dst_n, tab_bar.Tabs.Get(tab_src_n));

				tab = tab_bar.Tabs.GetPtr(tab_dst_n);
				tab.IndexDuringLayout = (ImS16)tab_dst_n;

				// We will need sorting if tabs have changed section (e.g. moved from one of Leading/Central/Trailing to another)
				int curr_tab_section_n = TabItemGetSectionIdx(tab);
				if(tab_dst_n > 0)
				{
					ImGuiTabItemPtr prev_tab = tab_bar.Tabs.GetPtr(tab_dst_n - 1);
					int prev_tab_section_n = TabItemGetSectionIdx(prev_tab);
					if(curr_tab_section_n == 0 && prev_tab_section_n != 0)
						need_sort_by_section = true;
					if(prev_tab_section_n == 2 && curr_tab_section_n != 2)
						need_sort_by_section = true;
				}

				sections[curr_tab_section_n].TabCount++;
				tab_dst_n++;
			}
			if(tab_bar.Tabs.Size != tab_dst_n)
				tab_bar.Tabs.Resize(tab_dst_n);

			if(need_sort_by_section)
				igImQsort(tab_bar.Tabs.GetPtr(0), (nuint)tab_bar.Tabs.Size, (nuint)Marshal.SizeOf<ImGuiTabItem>(), TabItemComparerBySection);

			// Calculate spacing between sections
			sections[0].Spacing = sections[0].TabCount > 0 && (sections[1].TabCount + sections[2].TabCount) > 0 ? g.Style.ItemInnerSpacing.X : 0.0f;
			sections[1].Spacing = sections[1].TabCount > 0 && sections[2].TabCount > 0 ? g.Style.ItemInnerSpacing.X : 0.0f;

			// Setup next selected tab
			ImGuiID scroll_to_tab_id = 0;
			if(tab_bar.NextSelectedTabId != 0)
			{
				tab_bar.SelectedTabId = tab_bar.NextSelectedTabId;
				tab_bar.NextSelectedTabId = 0;
				scroll_to_tab_id = tab_bar.SelectedTabId;
			}

			// Process order change request (we could probably process it when requested but it's just saner to do it in a single spot).
			if(tab_bar.ReorderRequestTabId != 0)
			{
				if(ImGui.TabBarProcessReorder((IntPtr)tab_bar.NativePtr))
					if(tab_bar.ReorderRequestTabId == tab_bar.SelectedTabId)
						scroll_to_tab_id = tab_bar.ReorderRequestTabId;
				tab_bar.ReorderRequestTabId = 0;
			}

			// Tab List Popup (will alter tab_bar.BarRect and therefore the available width!)
			bool tab_list_popup_button = (tab_bar.Flags & ImGuiTabBarFlags.TabListPopupButton) != 0;
			if(tab_list_popup_button)
			{
				ImGuiTabItemPtr tab_to_select = TabBarTabListPopupButton(tab_bar);
				if(tab_to_select.NativePtr != null) // NB: Will alter BarRect.Min.x!
					scroll_to_tab_id = tab_bar.SelectedTabId = tab_to_select.ID;
			}

			// Leading/Trailing tabs will be shrink only if central one aren't visible anymore, so layout the shrink data as: leading, trailing, central
			// (whereas our tabs are stored as: leading, central, trailing)
			var shrink_buffer_indexes = stackalloc int[3];
			shrink_buffer_indexes[0] = 0;
			shrink_buffer_indexes[1] = sections[0].TabCount + sections[2].TabCount;
			shrink_buffer_indexes[2] = sections[0].TabCount;
			g.ShrinkWidthBuffer.Resize(tab_bar.Tabs.Size);

			// Compute ideal tabs widths + store them into shrink buffer
			ImGuiTabItemPtr most_recently_selected_tab = null;
			int curr_section_n = -1;
			bool found_selected_tab_id = false;
			for(int tab_n = 0; tab_n < tab_bar.Tabs.Size; tab_n++)
			{
				ImGuiTabItemPtr tab = tab_bar.Tabs.GetPtr(tab_n);
				if(tab.LastFrameVisible < tab_bar.PrevFrameVisible)
					throw new InvalidOperationException("TabBarLayout() called with a tab that was not visible in the previous frame! This is likely a bug in ImGui.NET or your code.");

				if((most_recently_selected_tab.NativePtr == null || most_recently_selected_tab.LastFrameSelected < tab.LastFrameSelected) && (tab.Flags & ImGuiTabItemFlags.Button) == 0)
					most_recently_selected_tab = tab;
				if(tab.ID == tab_bar.SelectedTabId)
					found_selected_tab_id = true;
				if(scroll_to_tab_id == 0 && g.NavJustMovedToId == tab.ID)
					scroll_to_tab_id = tab.ID;

				// Refresh tab width immediately, otherwise changes of style e.g. style.FramePadding.x would noticeably lag in the tab bar.
				// Additionally, when using TabBarAddTab() to manipulate tab bar order we occasionally insert new tabs that don't have a width yet,
				// and we cannot wait for the next BeginTabItem() call. We cannot compute this width within TabBarAddTab() because font size depends on the active window.
				var tab_name = tab_bar.GetTabName(tab);
				bool has_close_button = (tab.Flags & ImGuiTabItemFlags.NoCloseButton) != 0 ? false : true;
				tab.ContentWidth = (tab.RequestedWidth > 0.0f) ? tab.RequestedWidth : TabItemCalcSize(tab_name, has_close_button).X;

				int section_n = TabItemGetSectionIdx(tab);
				ImGuiTabBarSectionPtr section = &sections[section_n];
				section.Width += tab.ContentWidth + (section_n == curr_section_n ? g.Style.ItemInnerSpacing.X : 0.0f);
				curr_section_n = section_n;

				// Store data so we can build an array sorted by width if we need to shrink tabs down
				ImGuiShrinkWidthItemPtr shrink_width_item = g.ShrinkWidthBuffer.GetPtr(shrink_buffer_indexes[section_n]++);
				shrink_width_item.Index = tab_n;
				shrink_width_item.Width = shrink_width_item.InitialWidth = tab.ContentWidth;

				if(tab.ContentWidth <= 0)
					throw new InvalidOperationException($"Tab '{tab_name}' has a non-positive width ({tab.ContentWidth})! This is likely a bug in ImGui.NET or your code.");
				tab.Width = tab.ContentWidth;
			}

			// Compute total ideal width (used for e.g. auto-resizing a window)
			tab_bar.WidthAllTabsIdeal = 0.0f;
			for(int section_n = 0; section_n < 3; section_n++)
				tab_bar.WidthAllTabsIdeal += sections[section_n].Width + sections[section_n].Spacing;

			// Horizontal scrolling buttons
			// (note that TabBarScrollButtons() will alter BarRect.Max.x)
			if((tab_bar.WidthAllTabsIdeal > tab_bar.BarRect.GetWidth() && tab_bar.Tabs.Size > 1) && (tab_bar.Flags & ImGuiTabBarFlags.NoTabListScrollingButtons) == 0 && (tab_bar.Flags & ImGuiTabBarFlags.FittingPolicyScroll) != 0)
			{
				ImGuiTabItemPtr scroll_and_select_tab = TabBarScrollingButtons(tab_bar);
				if(scroll_and_select_tab.NativePtr != null)
				{
					scroll_to_tab_id = scroll_and_select_tab.ID;
					if((scroll_and_select_tab.Flags & ImGuiTabItemFlags.Button) == 0)
						tab_bar.SelectedTabId = scroll_to_tab_id;
				}
			}

			// Shrink widths if full tabs don't fit in their allocated space
			float section_0_w = sections[0].Width + sections[0].Spacing;
			float section_1_w = sections[1].Width + sections[1].Spacing;
			float section_2_w = sections[2].Width + sections[2].Spacing;
			bool central_section_is_visible = (section_0_w + section_2_w) < tab_bar.BarRect.GetWidth();
			float width_excess;
			if(central_section_is_visible)
				width_excess = Math.Max(section_1_w - (tab_bar.BarRect.GetWidth() - section_0_w - section_2_w), 0.0f); // Excess used to shrink central section
			else
				width_excess = (section_0_w + section_2_w) - tab_bar.BarRect.GetWidth(); // Excess used to shrink leading/trailing section

			// With ImGuiTabBarFlags_FittingPolicyScroll policy, we will only shrink leading/trailing if the central section is not visible anymore
			if(width_excess > 0.0f && ((tab_bar.Flags & ImGuiTabBarFlags.FittingPolicyResizeDown) != 0 || !central_section_is_visible))
			{
				int shrink_data_count = (central_section_is_visible ? sections[1].TabCount : sections[0].TabCount + sections[2].TabCount);
				int shrink_data_offset = (central_section_is_visible ? sections[0].TabCount + sections[2].TabCount : 0);
				ImGui.ShrinkWidths(g.ShrinkWidthBuffer.GetPtr(0) + shrink_data_offset, shrink_data_count, width_excess);

				// Apply shrunk values into tabs and sections
				for(int tab_n = shrink_data_offset; tab_n < shrink_data_offset + shrink_data_count; tab_n++)
				{
					ImGuiTabItemPtr tab = tab_bar.Tabs.GetPtr(g.ShrinkWidthBuffer.GetPtr(tab_n)->Index);
					float shrinked_width = (float)Math.Floor(g.ShrinkWidthBuffer.GetPtr(tab_n)->Width);
					if(shrinked_width < 0.0f)
						continue;

					int section_n = TabItemGetSectionIdx(tab);
					sections[section_n].Width -= (tab.Width - shrinked_width);
					tab.Width = shrinked_width;
				}
			}

			// Layout all active tabs
			int section_tab_index = 0;
			float tab_offset = 0.0f;
			tab_bar.WidthAllTabs = 0.0f;
			for(int section_n = 0; section_n < 3; section_n++)
			{
				ImGuiTabBarSectionPtr section = &sections[section_n];
				if(section_n == 2)
					tab_offset = Math.Clamp(tab_bar.BarRect.GetWidth(), 0.0f, tab_offset);

				for(int tab_n = 0; tab_n < section.TabCount; tab_n++)
				{
					ImGuiTabItemPtr tab = tab_bar.Tabs.GetPtr(section_tab_index + tab_n);
					tab.Offset = tab_offset;
					tab_offset += tab.Width + (tab_n < section.TabCount - 1 ? g.Style.ItemInnerSpacing.X : 0.0f);
				}
				tab_bar.WidthAllTabs += Math.Max(section.Width + section.Spacing, 0.0f);
				tab_offset += section.Spacing;
				section_tab_index += section.TabCount;
			}

			// If we have lost the selected tab, select the next most recently active one
			if(found_selected_tab_id == false)
				tab_bar.SelectedTabId = 0;
			if(tab_bar.SelectedTabId == 0 && tab_bar.NextSelectedTabId == 0 && most_recently_selected_tab.NativePtr != null)
				scroll_to_tab_id = tab_bar.SelectedTabId = most_recently_selected_tab.ID;

			// Lock in visible tab
			tab_bar.VisibleTabId = tab_bar.SelectedTabId;
			tab_bar.VisibleTabWasSubmitted = false;

			// Update scrolling
			if(scroll_to_tab_id != 0)
				TabBarScrollToTab(tab_bar, scroll_to_tab_id, sections);
			tab_bar.ScrollingAnim = TabBarScrollClamp(tab_bar, tab_bar.ScrollingAnim);
			tab_bar.ScrollingTarget = TabBarScrollClamp(tab_bar, tab_bar.ScrollingTarget);
			if(tab_bar.ScrollingAnim != tab_bar.ScrollingTarget)
			{
				// Scrolling speed adjust itself so we can always reach our target in 1/3 seconds.
				// Teleport if we are aiming far off the visible line
				tab_bar.ScrollingSpeed = Math.Max(tab_bar.ScrollingSpeed, 70.0f * g.FontSize);
				tab_bar.ScrollingSpeed = Math.Max(tab_bar.ScrollingSpeed, Math.Abs(tab_bar.ScrollingTarget - tab_bar.ScrollingAnim) / 0.3f);
				bool teleport = (tab_bar.PrevFrameVisible + 1 < g.FrameCount) || (tab_bar.ScrollingTargetDistToVisibility > 10.0f * g.FontSize);
				tab_bar.ScrollingAnim = teleport ? tab_bar.ScrollingTarget : ImGui.ImLinearSweep(tab_bar.ScrollingAnim, tab_bar.ScrollingTarget, g.IO.DeltaTime * tab_bar.ScrollingSpeed);
			}
			else
			{
				tab_bar.ScrollingSpeed = 0.0f;
			}
			tab_bar.ScrollingRectMinX = tab_bar.BarRect.Min.X + sections[0].Width + sections[0].Spacing;
			tab_bar.ScrollingRectMaxX = tab_bar.BarRect.Max.X - sections[2].Width - sections[1].Spacing;

			// Clear name buffers
			if((tab_bar.Flags & ImGuiTabBarFlags.DockNode) == 0)
				tab_bar.TabsNames.Buf.Resize(0);

			// Actual layout in host window (we don't do it in BeginTabBar() so as not to waste an extra frame)
			ImGuiWindowPtr window = g.CurrentWindow;
			window.NativePtr->DC.CursorPos = tab_bar.BarRect.Min;
			ImGui.ItemSize(new ImVec2(tab_bar.WidthAllTabs, tab_bar.BarRect.GetHeight()), tab_bar.FramePadding.Y);
			window.NativePtr->DC.IdealMaxPos.x = Math.Max(window.DC.IdealMaxPos.x, tab_bar.BarRect.Min.X + tab_bar.WidthAllTabsIdeal);
		}

		static ImGuiTabItem* TabBarTabListPopupButton(ImGuiTabBarPtr tab_bar)
		{
			ImGuiContextPtr g = ImGui.GetCurrentContext();
			ImGuiWindowPtr window = g.CurrentWindow;

			// We use g.Style.FramePadding.y to match the square ArrowButton size
			float tab_list_popup_button_width = g.FontSize + g.Style.FramePadding.Y;
			ImVec2 backup_cursor_pos = window.DC.CursorPos;
			window.NativePtr->DC.CursorPos = new ImVec2(tab_bar.BarRect.Min.X - g.Style.FramePadding.Y, tab_bar.BarRect.Min.Y);
			var rect = tab_bar.BarRect;
			rect.Min.X += tab_list_popup_button_width;
			tab_bar.BarRect = rect;

			ImVec4 arrow_col = g.Style.Colors[(int)ImGuiCol.Text];
			arrow_col.w *= 0.5f;
			ImGui.PushStyleColor(ImGuiCol.Text, arrow_col);
			ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(0, 0, 0, 0));
			bool open = ImGui.BeginCombo("##v", null, ImGuiComboFlags.NoPreview | ImGuiComboFlags.HeightLargest);
			ImGui.PopStyleColor(2);

			ImGuiTabItemPtr tab_to_select = null;
			if(open)
			{
				for(int tab_n = 0; tab_n < tab_bar.Tabs.Size; tab_n++)
				{
					ImGuiTabItemPtr tab = tab_bar.Tabs.GetPtr(tab_n);
					if((tab.Flags & ImGuiTabItemFlags.Button) != 0)
						continue;

					var tab_name = tab_bar.GetTabName(tab);
					if(ImGui.Selectable(tab_name, tab_bar.SelectedTabId == tab.ID))
						tab_to_select = tab;
				}
				ImGui.EndCombo();
			}

			window.NativePtr->DC.CursorPos = backup_cursor_pos;
			return tab_to_select;
		}
		static ImVec2 TabItemCalcSize(string label, bool has_close_button)
		{
			ImGuiContextPtr g = ImGui.GetCurrentContext();
			ImVec2 label_size = ImGui.CalcTextSize(label, null, true);
			ImVec2 size = new ImVec2(label_size.X + g.Style.FramePadding.X, label_size.Y + g.Style.FramePadding.Y * 2.0f);
			if(has_close_button)
				size.X += g.Style.FramePadding.X + (g.Style.ItemInnerSpacing.X + g.FontSize); // We use Y intentionally to fit the close button circle.
			else
				size.X += g.Style.FramePadding.X + 1.0f;
			return new ImVec2(Math.Min(size.X, TabBarCalcMaxTabWidth()), size.Y);
		}
		static float TabBarCalcMaxTabWidth()
		{
			ImGuiContextPtr g = ImGui.GetCurrentContext();
			return g.FontSize * 20.0f;
		}
		static ImGuiTabItemPtr TabBarScrollingButtons(ImGuiTabBarPtr tab_bar)
		{
			ImGuiContextPtr g = ImGui.GetCurrentContext();
			ImGuiWindowPtr window = g.CurrentWindow;

			ImVec2 arrow_button_size = new(g.FontSize -2.0f, g.FontSize + g.Style.FramePadding.Y * 2.0f);
			float scrolling_buttons_width = arrow_button_size.X * 2.0f;

			ImVec2 backup_cursor_pos = window.DC.CursorPos;
			//window.DrawList.AddRect(ImVec2(tab_bar.BarRect.Max.x - scrolling_buttons_width, tab_bar.BarRect.Min.y), ImVec2(tab_bar.BarRect.Max.x, tab_bar.BarRect.Max.y), IM_COL32(255,0,0,255));

			int select_dir = 0;
			ImVec4 arrow_col = g.Style.Colors[(int)ImGuiCol.Text];
			arrow_col.w *= 0.5f;

			ImGui.PushStyleColor(ImGuiCol.Text, arrow_col);
			ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(0, 0, 0, 0));
			float backup_repeat_delay = g.IO.KeyRepeatDelay;
			float backup_repeat_rate = g.IO.KeyRepeatRate;
			g.NativePtr->IO.KeyRepeatDelay = 0.250f;
			g.NativePtr->IO.KeyRepeatRate = 0.200f;
			float x = Math.Max(tab_bar.BarRect.Min.X, tab_bar.BarRect.Max.X - scrolling_buttons_width);
			window.NativePtr->DC.CursorPos = new ImVec2(x, tab_bar.BarRect.Min.Y);
			if(ImGui.ArrowButtonEx("##<", ImGuiDir.Left, arrow_button_size, (ImGuiNET.ImGuiButtonFlags)(ImGuiButtonFlags.PressedOnClick | ImGuiButtonFlags.Repeat)))
				select_dir = -1;
			window.NativePtr->DC.CursorPos = new ImVec2(x + arrow_button_size.X, tab_bar.BarRect.Min.Y);
			if(ImGui.ArrowButtonEx("##>", ImGuiDir.Right, arrow_button_size, (ImGuiNET.ImGuiButtonFlags)(ImGuiButtonFlags.PressedOnClick | ImGuiButtonFlags.Repeat)))
				select_dir = +1;
			ImGui.PopStyleColor(2);
			g.NativePtr->IO.KeyRepeatRate = backup_repeat_rate;
			g.NativePtr->IO.KeyRepeatDelay = backup_repeat_delay;

			ImGuiTabItemPtr tab_to_scroll_to = null;
			if(select_dir != 0)
			{
				ImGuiTabItemPtr tab_item = (IntPtr)ImGui.TabBarFindTabByID((IntPtr)tab_bar.NativePtr, tab_bar.SelectedTabId).NativePtr;
				if(tab_item.NativePtr != null)
				{
					int selected_order = tab_bar.GetTabOrder(tab_item);
					int target_order = selected_order + select_dir;

					// Skip tab item buttons until another tab item is found or end is reached
					while(tab_to_scroll_to.NativePtr == null)
					{
						// If we are at the end of the list, still scroll to make our tab visible
						tab_to_scroll_to = tab_bar.Tabs.GetPtr((target_order >= 0 && target_order < tab_bar.Tabs.Size) ? target_order : selected_order);

						// Cross through buttons
						// (even if first/last item is a button, return it so we can update the scroll)
						if((tab_to_scroll_to.Flags & ImGuiTabItemFlags.Button) != 0)
						{
							target_order += select_dir;
							selected_order += select_dir;
							tab_to_scroll_to = (target_order < 0 || target_order >= tab_bar.Tabs.Size) ? tab_to_scroll_to : null;
						}
					}
				}
			}
			window.NativePtr->DC.CursorPos = backup_cursor_pos;
			ImRect rect = tab_bar.BarRect;
			rect.Max.X -= scrolling_buttons_width + 1.0f;
			tab_bar.BarRect = rect;

			return tab_to_scroll_to;
		}
		static void TabBarScrollToTab(ImGuiTabBarPtr tab_bar, ImGuiID tab_id, ImGuiTabBarSection* sections)
		{
			ImGuiTabItemPtr tab = (IntPtr)ImGui.TabBarFindTabByID((IntPtr)tab_bar.NativePtr, tab_id).NativePtr;
			if (tab.NativePtr == null)
				return;
			if ((tab.Flags & ImGuiTabItemFlags.SectionMask) != 0)
				return;

			ImGuiContextPtr g = ImGui.GetCurrentContext();
			float margin = g.FontSize * 1.0f; // When to scroll to make Tab N+1 visible always make a bit of N visible to suggest more scrolling area (since we don't have a scrollbar)
			int order = tab_bar.GetTabOrder(tab);

			// Scrolling happens only in the central section (leading/trailing sections are not scrolling)
			// FIXME: This is all confusing.
			float scrollable_width = tab_bar.BarRect.GetWidth() - sections[0].Width - sections[2].Width - sections[1].Spacing;

			// We make all tabs positions all relative Sections[0].Width to make code simpler
			float tab_x1 = tab.Offset - sections[0].Width + (order > sections[0].TabCount - 1 ? -margin : 0.0f);
			float tab_x2 = tab.Offset - sections[0].Width + tab.Width + (order + 1 < tab_bar.Tabs.Size - sections[2].TabCount ? margin : 1.0f);
			tab_bar.ScrollingTargetDistToVisibility = 0.0f;
			if (tab_bar.ScrollingTarget > tab_x1 || (tab_x2 - tab_x1 >= scrollable_width))
			{
				// Scroll to the left
				tab_bar.ScrollingTargetDistToVisibility = Math.Max(tab_bar.ScrollingAnim - tab_x2, 0.0f);
				tab_bar.ScrollingTarget = tab_x1;
			}
			else if (tab_bar.ScrollingTarget < tab_x2 - scrollable_width)
			{
				// Scroll to the right
				tab_bar.ScrollingTargetDistToVisibility = Math.Max((tab_x1 - scrollable_width) - tab_bar.ScrollingAnim, 0.0f);
				tab_bar.ScrollingTarget = tab_x2 - scrollable_width;
			}
		}

		static float TabBarScrollClamp(ImGuiTabBarPtr tab_bar, float scrolling)
		{
			scrolling = Math.Min(scrolling, tab_bar.WidthAllTabs - tab_bar.BarRect.GetWidth());
			return Math.Max(scrolling, 0.0f);
		}
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
		var result = ImGui.BeginTabItem(label, (ImGuiNET.ImGuiTabItemFlags)flags);
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
		var result = ImGui.BeginTabItem(label, ref open, (ImGuiNET.ImGuiTabItemFlags)flags);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}

	private unsafe static ImGuiPtrOrIndex GetTabBarRefFromTabBar(ImGuiTabBarPtr tab_bar)
	{
		var g = (ImGuiContext*)ImGui.GetCurrentContext();
		if(g->TabBars.Contains(tab_bar))
			return new ImGuiPtrOrIndex { Index = g->TabBars.GetIndex(tab_bar) };
		return new ImGuiPtrOrIndex { Ptr = tab_bar };
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate int ImQsortCompareFunc(void* a, void* b);

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static unsafe extern void igImQsort(void* basePtr, nuint count, nuint sizeOfElement, ImQsortCompareFunc compareFunc);
	private static unsafe int TabItemComparerByBeginOrder(void* lhs, void* rhs)
	{

		ImGuiTabItemPtr a = (ImGuiTabItem*)lhs;
		ImGuiTabItemPtr b = (ImGuiTabItem*)rhs;
		return (int) (a.BeginOrder - b.BeginOrder);
	}
	private static unsafe int TabItemComparerBySection(void* lhs, void* rhs)
	{
		ImGuiTabItemPtr a = (ImGuiTabItem*)lhs;
		ImGuiTabItemPtr b = (ImGuiTabItem*)rhs;
		int a_section = TabItemGetSectionIdx(a);
		int b_section = TabItemGetSectionIdx(b);
		if(a_section != b_section)
			return a_section - b_section;
		return (a.IndexDuringLayout - b.IndexDuringLayout);
	}
	private static int TabItemGetSectionIdx(ImGuiTabItemPtr tab) =>
		(tab.Flags & ImGuiTabItemFlags.Leading) != 0 ? 0 : (tab.Flags & ImGuiTabItemFlags.Trailing) != 0 ? 2 : 1;
	private static unsafe ImGuiTabBarPtr GetTabBarFromTabBarRef(ImGuiPtrOrIndex tabBarRef)
	{
		return tabBarRef.Ptr != IntPtr.Zero ? (ImGuiTabBarPtr)tabBarRef.Ptr : ((ImGuiContextPtr)ImGui.GetCurrentContext()).TabBars.GetByIndex(tabBarRef.Index);
	}
}