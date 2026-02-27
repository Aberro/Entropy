using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Entropy.Common.UI.ImGUI;
using Entropy.Common.Utils;
using HarmonyLib;
using ImGuiNET;
using ImGuiNET.Unity;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using ImGuiButtonFlags = Entropy.Common.UI.ImGUI.ImGuiButtonFlags;
using ImGuiID = System.UInt32;
using ImGuiTabBar = Entropy.Common.UI.ImGUI.ImGuiTabBar;
using ImGuiTabBarFlags = Entropy.Common.UI.ImGUI.ImGuiTabBarFlags;
using ImGuiTabItem = Entropy.Common.UI.ImGUI.ImGuiTabItem;
using ImGuiTabItemFlags = Entropy.Common.UI.ImGUI.ImGuiTabItemFlags;
using ImGuiWindow = Entropy.Common.UI.ImGUI.ImGuiWindow;
using ImGuiWindowFlags = Entropy.Common.UI.ImGUI.ImGuiWindowFlags;
using ImS16 = System.Int16;

namespace Entropy.Common.UI;

public static class ImGuiHelper
{
	[HarmonyPatch]
	private static class Patches
	{
		[HarmonyPatch(typeof(ImGuiManager), "OnEnable")]
		[HarmonyPrefix]
		public static void DearImGuiAwakePrefix(ImGuiManager __instance, ref TextureManager ___igTextureManager)
		{
			_textureManager = ___igTextureManager;
		}
	}
	private static unsafe class ImGuiHelperInternals
	{
		/// <summary>
		/// A helper class in a helper class in a helper class... that just holds a pointer to byte buffer and frees it when the garbage collector collects the instance.
		/// This is used to map strings to UTF8 byte buffers that can be passed to ImGUI native functions without allocating and freeing memory every time a string is used.
		/// </summary>
		private sealed class BufferBox
		{
			public nint Value { get; private set; }
			public BufferBox(nint value)
			{
				Value = value;
				entries++;
			}
			~BufferBox()
			{
				if(Value != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(Value);
					Value = IntPtr.Zero;
					entries--;
				}
			}
		}
		/// <summary>
		/// A table that maps strings to their corresponding UTF8 byte buffer pointers to pass into ImGUI native functions.
		/// </summary>
		private static uint entries;
		private static ConditionalWeakTable<string, BufferBox> stringsTable = new();

		/// <summary>
		/// Gets a pointer to a UTF8 byte buffer for the given string.
		/// </summary>
		/// <param name="s"> The string to get the pointer for.</param>
		/// <returns> A pointer to a UTF8 byte buffer that can be passed to ImGUI native functions.</returns>
		/// <remarks>
		/// This method will cache used strings in a <see cref="ConditionalWeakTable{TKey, TValue}"/> to avoid memory allocations and deallocations every time a string is used.
		/// Garbage collection of a cached string will automatically free the memory allocated for the UTF8 byte buffer.
		/// </remarks>
		public static byte* GetStringPointer(string s)
		{
			if(s == null)
				throw new ArgumentNullException(nameof(s), "String cannot be null.");
			if(stringsTable.TryGetValue(s, out var ptr))
				return (byte*)ptr.Value;
			var size = 0;
			byte* buffer;
			size = MarshalUtils.GetByteCount(s);
			buffer = (byte*)(void*)Marshal.AllocHGlobal(size + 1);

			fixed(char* chars = s)
			{
				var utf = Encoding.UTF8.GetBytes(chars, s.Length, buffer, size);
				buffer[utf] = 0;
			}
			stringsTable.Add(s, new BufferBox((nint)buffer));
			return buffer;
		}

		public static string GetString(byte* strPtr)
		{
			if(strPtr == null)
				throw new ArgumentNullException(nameof(strPtr), "Pointer cannot be null.");
			// this would be unsafe either way, so why not use highly optimized IndexOf.
			var len = new ReadOnlySpan<byte>(strPtr, int.MaxValue).IndexOf<byte>(0);
			// There's a chance to avoid creating a string and doing many enumerations for UTF8 -> UTF16 conversion by looking up the table
			if(len < entries || entries < 256)
			{
				foreach(var entry in stringsTable)
				{
					if(entry.Value.Value == (nint)strPtr)
						return entry.Key;
				}
			}
			// couldn't find the string in the table, so we have to allocate a new string.
			Marshal.PtrToStringUTF8((nint)strPtr, len);
			while(strPtr[len] != 0)
				len++;
			return Encoding.UTF8.GetString(strPtr, len);
		}
	}

	private static TextureManager _textureManager = null!;

	/// <summary>
	/// Gets a pointer to a UTF8 byte buffer for the given string.
	/// </summary>
	/// <param name="str_id"> The string to get the pointer for.</param>
	/// <returns> A pointer to a UTF8 byte buffer that can be passed to ImGUI native functions.</returns>
	/// <remarks>
	/// This method will cache used strings in a <see cref="ConditionalWeakTable{TKey, TValue}"/> to avoid memory allocations and deallocations every time a string is used.
	/// If a stored string gets garbage collected, this will cause the <see cref="BufferBox"/> to be finalized as well, which will free the memory allocated for the UTF8 byte buffer,
	/// thus preventing memory leaks.
	/// Using of <see cref="ConditionalWeakTable{TKey, TValue}"/> facilitates both memory management and performance,
	/// as it automatically removes garbage collected keys and performs lookup in O(1) time complexity.
	/// </remarks>
	public static unsafe byte* GetStringPointer(string str_id) => ImGuiHelperInternals.GetStringPointer(str_id);

	/// <summary>
	/// Gets a string from a UTF8 byte pointer.
	/// </summary>
	/// <param name="strPtr"> The pointer to the UTF8 byte buffer.</param>
	/// <returns> A string representation of the UTF8 byte buffer.</returns>
	/// <remarks>
	/// This tries to find the string in the internal cache of strings, and if it fails, it creates a new string from the UTF8 byte buffer.
	/// </remarks>
	public static unsafe string GetString(byte* strPtr) => ImGuiHelperInternals.GetString(strPtr);

	/// <summary>
	/// Gets a pointer to the given value.
	/// </summary>
	/// <typeparam name="T"> The type of the value to get the pointer for.</typeparam>
	/// <param name="value"> The value to get the pointer for.</param>
	/// <returns>A pointer to the given value.</returns>
	public static unsafe T* GetPointer<T>(ref T value)
		where T : unmanaged => (T*)UnsafeUtility.AddressOf<T>(ref value);
	/// <summary>
	/// Gets a reference to the value at the given pointer.
	/// </summary>
	/// <typeparam name="T"> The type of the value to get the reference for.</typeparam>
	/// <param name="ptr"> The pointer to the value.</param>
	/// <returns> A reference to the value at the given pointer.</returns>
	/// <exception cref="NullReferenceException"></exception>
	public unsafe static ref T GetRef<T>(T* ptr) where T : unmanaged
	{
		if(ptr == null)
			throw new NullReferenceException("Attempt to read null pointer of type " + typeof(T).Name + ".");
		return ref UnsafeUtility.AsRef<T>(ptr);
	}
	/// <summary>
	/// Validates that the given pointer is not null.
	/// </summary>
	/// <typeparam name="T"> The type of the pointer to validate.</typeparam>
	/// <param name="ptr"> The pointer to validate.</param>
	/// <exception cref="NullReferenceException"> Thrown when the pointer is null.</exception>
	public static unsafe ref T ValidatePointer<T>(T* ptr) where T : unmanaged
	{
		if(ptr == null)
			throw new NullReferenceException("Attempt to read null pointer of type " + typeof(T).Name + ".");
		return ref *ptr;
	}
	/// <summary>
	/// Gets the current ImGui context.
	/// </summary>
	/// <returns> A reference to the current ImGui context.</returns>
	public static unsafe ref ImGuiContext GetCurrentContext() => ref *igGetCurrentContext();

	/// <summary>
	/// Gets a unique ID for the given string.
	/// </summary>
	/// <param name="str_id"> The string to get the ID for.</param>
	/// <returns> A unique ID for the given string.</returns>
	public static unsafe uint GetID(string str_id)
	{
		return ImGuiNative.igGetID_Str(GetStringPointer(str_id));
	}

	/// <summary>
	/// Finds a tab item by its ID in the given tab bar.
	/// </summary>
	/// <param name="tab_bar"> The tab bar to search in.</param>
	/// <param name="tab_id"> The ID of the tab item to find.</param>
	/// <returns> A reference to the found tab item, or a default value if not found.</returns>
	public static unsafe bool TryGetTabByID(ref ImGuiTabBar tab_bar, uint tab_id, ref ImGuiTabItem tab)
	{
		var nativePtr = igTabBarFindTabByID(GetPointer(ref tab_bar), tab_id);
		if(nativePtr == null)
			return false;
		tab = GetRef(nativePtr);
		return true;
	}
	public static unsafe bool TabBarProcessReorder(ref ImGuiTabBar tab_bar)
	{
		return igTabBarProcessReorder(GetPointer(ref tab_bar)) != 0;
	}
	/// <summary>
	/// Displays a regular window with a title.
	/// </summary>
	/// <param name="name">The name of the window.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the window is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Window(string name, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Window(string name, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Window(string name, ref bool open, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Window(string name, ref bool open, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.Begin(name, ref open, (ImGuiNET.ImGuiWindowFlags)flags))
		{
			body();
			ImGui.End();
		}
		return open;
	}

	public static bool Child(string str_id, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		var result = ImGui.BeginChild(str_id);
		if(result)
			body();
		ImGui.EndChild();
		return result;
	}

	public static bool Child(string str_id, bool border, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		var result = ImGui.BeginChild(str_id, border);
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
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the child window is visible, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, Vector2 size, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, Vector2 size, bool border, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, bool border, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(string str_id, Vector2 size, bool border, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, Vector2 size, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, bool border, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, Vector2 size, bool border, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, bool border, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Child(uint id, Vector2 size, bool border, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static void Group(Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		ImGui.BeginGroup();
		body();
		ImGui.EndGroup();
	}

	/// <summary>
	/// A helper class for <see cref="ImGui.BeginCombo"/> to automatically call <see cref="ImGui.EndCombo()"/>.
	/// </summary>
	/// <param name="label">The label of the combobox.</param>
	/// <param name="current_item">The currently selected item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the combobox is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Combo(string label, string current_item, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool Combo(string label, string current_item, ImGuiComboFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool ChildFrame(uint id, Vector2 size, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool ChildFrame(uint id, Vector2 size, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static void Disabled(bool disabled, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool ListBox(string label, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool MenuBar(Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="body"/> is <see langword="null"/>.</exception>
	public static bool MainMenuBar(Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool Menu(string label, Action body)
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
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, bool selected, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, selected))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="shortcut">The shortcut of the menu item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, string shortcut, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, shortcut))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="enabled">Flag indicating if the menu item is enabled.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, bool selected, bool enabled, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, selected, enabled))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="shortcut">The shortcut of the menu item.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, string shortcut, bool selected, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, shortcut, selected))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="shortcut">The shortcut of the menu item.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, string shortcut, ref bool selected, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, shortcut, ref selected))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="shortcut">The shortcut of the menu item.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="enabled">Flag indicating if the menu item is enabled.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, string shortcut, bool selected, bool enabled, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, shortcut, selected, enabled))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a menu with the specified label and optionally populates it with submenu items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">Menu item label.</param>
	/// <param name="shortcut">The shortcut of the menu item.</param>
	/// <param name="selected">Flag indicating if the menu item is selected.</param>
	/// <param name="enabled">Flag indicating if the menu item is enabled.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the menu is open, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool MenuItem(string label, string shortcut, ref bool selected, bool enabled, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.MenuItem(label, shortcut, ref selected, enabled))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a tooltip window, that follows the mouse. Tooltip windows do not take focus away.
	/// </summary>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static void Tooltip(Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool Popup(string str_id, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool Popup(string str_id, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool PopupModal(string str_id, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool PopupModal(string str_id, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool PopupModal(string str_id, ref bool open, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool PopupModal(string str_id, ref bool open, ImGuiWindowFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabBar(string str_id, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabBar(string str_id, ImGuiTabBarFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		var result = ImGui.BeginTabBar(str_id, (ImGuiNET.ImGuiTabBarFlags)flags);
		if(result)
		{
			body();
			ImGui.EndTabBar();
		}
		return result;
	}

	/// <summary>
	/// Displays a tab item with the specified label and populates it with items using the provided <paramref name="body"/> action.
	/// </summary>
	/// <param name="label">The label of the tab item.</param>
	/// <param name="body">The action to be executed to populate child elements.</param>
	/// <returns><see langword="true"/> if the tab item is visible, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabItem(string label, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabItem(string label, ref bool open, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabItem(string label, ImGuiTabItemFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
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
	/// <exception cref="ArgumentNullException">Thrown when the <paramref name="body"/> action is null.</exception>
	public static bool TabItem(string label, ref bool open, ImGuiTabItemFlags flags, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		var result = ImGui.BeginTabItem(label, ref open, (ImGuiNET.ImGuiTabItemFlags)flags);
		if(result)
		{
			body();
			ImGui.EndTabItem();
		}
		return result;
	}
	public static unsafe bool BeginVerticalTabBar(string str_id, ImGuiTabBarFlags flags)
	{
		//var id = GetID(str_id);
		//if(!ImGuiHelperInternals.verticalTabBars.TryGetValue(id, out var tabBar))
		//{
		//	tabBar = new ImGuiVerticalTabBar();
		//	ImGuiHelperInternals.verticalTabBars.Add(id, tabBar);
		//}
		//tabBar.ID = id;
		//ImGuiHelperInternals.CurrentTabBarStack.Push(tabBar);
		//ImGuiHelperInternals.CurrentVerticalTabBar = tabBar;
		//return true;

		ref var g = ref GetCurrentContext();
		ref var window = ref g.CurrentWindow;
		if(window.SkipItems)
			return false;
		var id = window.GetID(str_id);
		ref var tabBar = ref g.TabBars.GetOrAddByKey(id);
		// Original:
		var tabBarBoundingBox = new ImRect(window.DC.CursorPos.x, window.DC.CursorPos.y, window.WorkRect.Max.X, window.DC.CursorPos.y + g.FontSize + (g.Style.FramePadding.Y * 2));
		// By the context it seems that it's bounding box for where tab bar is drawn, i.e. tab headers without tab content.
		// It case of horizontal tab bar, the height is easily determinable. In our case, neither width nor height is determinable until layout.
		// So reserve the whole remaining window space as bounding box.
		//var tabBarBoundingBox = new ImRect(window->DC.CursorPos.x, window->DC.CursorPos.y, window->WorkRect.xMax, window->WorkRect.yMax);
		tabBar.ID = id;
		flags |= ImGuiTabBarFlags.IsFocused;

		if((flags & ImGuiTabBarFlags.DockNode) == 0)
			ImGui.PushOverrideID(tabBar.ID);

		// Add to stack
		var tabBarRef = GetTabBarRefFromTabBar(ref tabBar);
		g.CurrentTabBarStack.Add(ref tabBarRef);
		g.SetCurrentTabBar(ref tabBar);

		// Append with multiple BeginTabBar()/EndTabBar() pairs.
		tabBar.BackupCursorPos = window.DC.CursorPos;
		if(tabBar.CurrFrameVisible == g.FrameCount)
		{
			window.DC.CursorPos = new Vector2(tabBar.BarRect.Min.X, tabBar.BarRect.Max.Y + tabBar.ItemSpacingY);
			tabBar.BeginCount++;
			return true;
		}

		// Ensure correct ordering when toggling ImGuiTabBarFlags_Reorderable flag, or when a new tab was added while being not reorderable
		if((flags & ImGuiTabBarFlags.Reorderable) != (tabBar.Flags & ImGuiTabBarFlags.Reorderable) || (tabBar.TabsAddedNew && (flags & ImGuiTabBarFlags.Reorderable) == 0))
			igImQsort(GetPointer(ref tabBar.Tabs.Get(0)), (nuint)tabBar.Tabs.Size, (nuint)Marshal.SizeOf<ImGuiTabItem>(), TabItemComparerByBeginOrder);
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
		window.DC.CursorPos = new Vector2(tabBar.BarRect.Min.X, tabBar.BarRect.Max.Y + tabBar.ItemSpacingY);

		// Draw separator
		var col = ImGui.GetColorU32((flags & ImGuiTabBarFlags.IsFocused) != 0 ? ImGuiCol.TabActive : ImGuiCol.TabUnfocusedActive);
		var y = tabBar.BarRect.Max.Y - 1.0f;
		{
			var separator_min_x = tabBar.BarRect.Min.X - (float)Math.Floor(window.WindowPadding.X * 0.5f);
			var separator_max_x = tabBar.BarRect.Max.X + (float)Math.Floor(window.WindowPadding.X * 0.5f);
			var p1 = new ImVec2(separator_min_x, y);
			var p2 = new ImVec2(separator_max_x, y);
			window.DrawList.AddLine(ref p1, ref p2, col, 1.0f);
		}
		return true;
	}
	public static unsafe void EndVerticalTabBar()
	{
		//var tab_bar = ImGuiHelperInternals.CurrentVerticalTabBar;
		//if(tab_bar == null)
		//	throw new InvalidOperationException("Mismatched BeginVerticalTabBar()/EndVerticalTabBar()!");
		//if(tab_bar.WantLayout)
		//	TabBarLayout(tab_bar);

		ref var g = ref GetCurrentContext();
		ref var window = ref g.CurrentWindow;
		if(window.SkipItems)
			return;

		if(!g.HasCurrentTabBar)
		{
			throw new InvalidOperationException("Mismatched BeginTabBar()/EndTabBar()!");
		}
		ref var tab_bar = ref g.CurrentTabBar;

		// Fallback in case no TabItem have been submitted
		if(tab_bar.WantLayout)
			TabBarLayout(ref tab_bar);

		// Restore the last visible height if no tab is visible, this reduce vertical flicker/movement when a tabs gets removed without calling SetTabItemClosed().
		var tab_bar_appearing = tab_bar.PrevFrameVisible + 1 < g.FrameCount;
		if(tab_bar.VisibleTabWasSubmitted || tab_bar.VisibleTabId == 0 || tab_bar_appearing)
		{
			tab_bar.CurrTabsContentsHeight = Math.Max(window.DC.CursorPos.y - tab_bar.BarRect.Max.Y, tab_bar.CurrTabsContentsHeight);
			window.DC.CursorPos.y = tab_bar.BarRect.Max.Y + tab_bar.CurrTabsContentsHeight;
		} else
		{
			window.DC.CursorPos.y = tab_bar.BarRect.Max.Y + tab_bar.PrevTabsContentsHeight;
		}
		if(tab_bar.BeginCount > 1)
			window.DC.CursorPos = tab_bar.BackupCursorPos;

		if((tab_bar.Flags & ImGuiTabBarFlags.DockNode) == 0)
			ImGui.PopID();

		g.CurrentTabBarStack.Pop();
		if(g.CurrentTabBarStack.IsEmpty)
			g.ClearCurrentTabBar();
		else
			g.SetCurrentTabBar(ref GetTabBarFromTabBarRef(ref g.CurrentTabBarStack.GetLast()));
	}

	private static unsafe bool TabBarTabListPopupButton(ref ImGuiTabBar tab_bar, ref ImGuiTabItem result)
	{
		ref var g = ref GetCurrentContext();
		ref var window = ref g.CurrentWindow;

		// We use g.Style.FramePadding.y to match the square ArrowButton size
		var tab_list_popup_button_width = g.FontSize + g.Style.FramePadding.Y;
		ImVec2 backup_cursor_pos = window.DC.CursorPos;
		window.DC.CursorPos = new ImVec2(tab_bar.BarRect.Min.X - g.Style.FramePadding.Y, tab_bar.BarRect.Min.Y);
		var rect = tab_bar.BarRect;
		rect.Min.X += tab_list_popup_button_width;
		tab_bar.BarRect = rect;

		var arrow_col = g.Style.Colors[(int)ImGuiCol.Text];
		arrow_col.W *= 0.5f;
		ImGui.PushStyleColor(ImGuiCol.Text, arrow_col);
		ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(0, 0, 0, 0));
		var open = ImGui.BeginCombo("##v", null, ImGuiComboFlags.NoPreview | ImGuiComboFlags.HeightLargest);
		ImGui.PopStyleColor(2);

		var tab_to_select_set = false;
		ref var tab_to_select = ref UnsafeUtility.AsRef<ImGuiTabItem>(null);
		if(open)
		{
			for(var tab_n = 0; tab_n < tab_bar.Tabs.Size; tab_n++)
			{
				ref var tab = ref tab_bar.Tabs.Get(tab_n);
				if((tab.Flags & ImGuiTabItemFlags.Button) != 0)
					continue;

				var tab_name = tab_bar.GetTabName(ref tab);
				if(ImGui.Selectable(tab_name, tab_bar.SelectedTabId == tab.ID))
					tab_to_select = ref tab;
			}
			ImGui.EndCombo();
		}

		window.DC.CursorPos = backup_cursor_pos;
		if(tab_to_select_set)
			result = ref tab_to_select;
		return tab_to_select_set;
	}
	private static ImVec2 TabItemCalcSize(string label, bool has_close_button)
	{
		ref var g = ref GetCurrentContext();
		ImVec2 label_size = ImGui.CalcTextSize(label, null, true);
		var size = new ImVec2(label_size.X + g.Style.FramePadding.X, label_size.Y + (g.Style.FramePadding.Y * 2.0f));
		if(has_close_button)
			size.X += g.Style.FramePadding.X + (g.Style.ItemInnerSpacing.X + g.FontSize); // We use Y intentionally to fit the close button circle.
		else
			size.X += g.Style.FramePadding.X + 1.0f;
		return new ImVec2(Math.Min(size.X, TabBarCalcMaxTabWidth()), size.Y);
	}
	private static float TabBarCalcMaxTabWidth()
	{
		ref var g = ref GetCurrentContext();
		return g.FontSize * 20.0f;
	}
	private static unsafe bool TabBarScrollingButtons(ref ImGuiTabBar tab_bar, ref ImGuiTabItem result)
	{
		ref var g = ref GetCurrentContext();
		ref var window = ref g.CurrentWindow;

		ImVec2 arrow_button_size = new(g.FontSize - 2.0f, g.FontSize + (g.Style.FramePadding.Y * 2.0f));
		var scrolling_buttons_width = arrow_button_size.X * 2.0f;

		ImVec2 backup_cursor_pos = window.DC.CursorPos;
		//window.DrawList.AddRect(ImVec2(tab_bar.BarRect.Max.x - scrolling_buttons_width, tab_bar.BarRect.Min.y), ImVec2(tab_bar.BarRect.Max.x, tab_bar.BarRect.Max.y), IM_COL32(255,0,0,255));

		var select_dir = 0;
		var arrow_col = g.Style.Colors[(int)ImGuiCol.Text];
		arrow_col.W *= 0.5f;

		ImGui.PushStyleColor(ImGuiCol.Text, arrow_col);
		ImGui.PushStyleColor(ImGuiCol.Button, new ImVec4(0, 0, 0, 0));
		var backup_repeat_delay = g.IO.KeyRepeatDelay;
		var backup_repeat_rate = g.IO.KeyRepeatRate;
		g.IO.KeyRepeatDelay = 400f;
		g.IO.KeyRepeatRate = 0.200f;
		var x = Math.Max(tab_bar.BarRect.Min.X, tab_bar.BarRect.Max.X - scrolling_buttons_width);
		window.DC.CursorPos = new ImVec2(x, tab_bar.BarRect.Min.Y);
		if(ImGui.ArrowButtonEx("##<", ImGuiDir.Left, arrow_button_size, (ImGuiNET.ImGuiButtonFlags)(ImGuiButtonFlags.PressedOnClick | ImGuiButtonFlags.Repeat)))
			select_dir = -1;
		window.DC.CursorPos = new ImVec2(x + arrow_button_size.X, tab_bar.BarRect.Min.Y);
		if(ImGui.ArrowButtonEx("##>", ImGuiDir.Right, arrow_button_size, (ImGuiNET.ImGuiButtonFlags)(ImGuiButtonFlags.PressedOnClick | ImGuiButtonFlags.Repeat)))
			select_dir = +1;
		ImGui.PopStyleColor(2);
		g.IO.KeyRepeatRate = backup_repeat_rate;
		g.IO.KeyRepeatDelay = backup_repeat_delay;

		var tab_to_scroll_to_set = false;
		ref var tab_to_scroll_to = ref GetRef<ImGuiTabItem>(null);
		if(select_dir != 0)
		{
			ref var tab_item = ref GetRef<ImGuiTabItem>(null);
			if(TryGetTabByID(ref tab_bar, tab_bar.SelectedTabId, ref tab_item))
			{
				var selected_order = tab_bar.GetTabOrder(ref tab_item);
				var target_order = selected_order + select_dir;

				// Skip tab item buttons until another tab item is found or end is reached
				while(!tab_to_scroll_to_set)
				{
					// If we are at the end of the list, still scroll to make our tab visible
					tab_to_scroll_to = ref tab_bar.Tabs.Get((target_order >= 0 && target_order < tab_bar.Tabs.Size) ? target_order : selected_order);
					tab_to_scroll_to_set = true;

					// Cross through buttons
					// (even if first/last item is a button, return it so we can update the scroll)
					if((tab_to_scroll_to.Flags & ImGuiTabItemFlags.Button) != 0)
					{
						target_order += select_dir;
						selected_order += select_dir;
						if(target_order >= 0 && target_order < tab_bar.Tabs.Size)
							tab_to_scroll_to_set = false;
					}
				}
			}
		}
		window.DC.CursorPos = backup_cursor_pos;
		var rect = tab_bar.BarRect;
		rect.Max.X -= scrolling_buttons_width + 1.0f;
		tab_bar.BarRect = rect;
		if(tab_to_scroll_to_set)
			result = ref tab_to_scroll_to;
		return tab_to_scroll_to_set;
	}
	private static unsafe void TabBarScrollToTab(ref ImGuiTabBar tab_bar, ImGuiID tab_id, Span<ImGuiTabBarSection> sections)
	{
		ref var tab = ref GetRef<ImGuiTabItem>(null);
		if(TryGetTabByID(ref tab_bar, tab_id, ref tab))
			return;
		if((tab.Flags & ImGuiTabItemFlags.SectionMask) != 0)
			return;

		ref var g = ref GetCurrentContext();
		var margin = g.FontSize * 1.0f; // When to scroll to make Tab N+1 visible always make a bit of N visible to suggest more scrolling area (since we don't have a scrollbar)
		var order = tab_bar.GetTabOrder(ref tab);

		// Scrolling happens only in the central section (leading/trailing sections are not scrolling)
		// FIXME: This is all confusing.
		var scrollable_width = tab_bar.BarRect.Width - sections[0].Width - sections[2].Width - sections[1].Spacing;

		// We make all tabs positions all relative Sections[0].Width to make code simpler
		var tab_x1 = tab.Offset - sections[0].Width + (order > sections[0].TabCount - 1 ? -margin : 0.0f);
		var tab_x2 = tab.Offset - sections[0].Width + tab.Width + (order + 1 < tab_bar.Tabs.Size - sections[2].TabCount ? margin : 1.0f);
		tab_bar.ScrollingTargetDistToVisibility = 0.0f;
		if(tab_bar.ScrollingTarget > tab_x1 || (tab_x2 - tab_x1 >= scrollable_width))
		{
			// Scroll to the left
			tab_bar.ScrollingTargetDistToVisibility = Math.Max(tab_bar.ScrollingAnim - tab_x2, 0.0f);
			tab_bar.ScrollingTarget = tab_x1;
		} else if(tab_bar.ScrollingTarget < tab_x2 - scrollable_width)
		{
			// Scroll to the right
			tab_bar.ScrollingTargetDistToVisibility = Math.Max(tab_x1 - scrollable_width - tab_bar.ScrollingAnim, 0.0f);
			tab_bar.ScrollingTarget = tab_x2 - scrollable_width;
		}
	}

	private static float TabBarScrollClamp(in ImGuiTabBar tab_bar, float scrolling)
	{
		scrolling = Math.Min(scrolling, tab_bar.WidthAllTabs - tab_bar.BarRect.Width);
		return Math.Max(scrolling, 0.0f);
	}
	private static unsafe void TabBarLayout(ref ImGuiTabBar tab_bar)
	{
		ref var g = ref ImGuiHelper.GetCurrentContext();
		tab_bar.WantLayout = false;

		// Garbage collect by compacting list
		// Detect if we need to sort out tab list (e.g. in rare case where a tab changed section)
		var tab_dst_n = 0;
		var need_sort_by_section = false;
		var sections = stackalloc ImGuiTabBarSection[3]; // Layout sections: Leading, Central, Trailing
		for(var tab_src_n = 0; tab_src_n < tab_bar.Tabs.Size; tab_src_n++)
		{
			ref var tab = ref tab_bar.Tabs.Get(tab_src_n);
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
				tab_bar.Tabs.Set(tab_dst_n, ref tab_bar.Tabs.Get(tab_src_n));

			tab = ref tab_bar.Tabs.Get(tab_dst_n);
			tab.IndexDuringLayout = (ImS16)tab_dst_n;

			// We will need sorting if tabs have changed section (e.g. moved from one of Leading/Central/Trailing to another)
			var curr_tab_section_n = TabItemGetSectionIdx(in tab);
			if(tab_dst_n > 0)
			{
				ref var prev_tab = ref tab_bar.Tabs.Get(tab_dst_n - 1);
				var prev_tab_section_n = TabItemGetSectionIdx(prev_tab);
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
			igImQsort(GetPointer(ref tab_bar.Tabs.Get(0)), (nuint)tab_bar.Tabs.Size, (nuint)Marshal.SizeOf<ImGuiTabItem>(), TabItemComparerBySection);

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
			if(TabBarProcessReorder(ref tab_bar))
			{
				if(tab_bar.ReorderRequestTabId == tab_bar.SelectedTabId)
					scroll_to_tab_id = tab_bar.ReorderRequestTabId;
			}

			tab_bar.ReorderRequestTabId = 0;
		}

		// Tab List Popup (will alter tab_bar.BarRect and therefore the available width!)
		var tab_list_popup_button = (tab_bar.Flags & ImGuiTabBarFlags.TabListPopupButton) != 0;
		if(tab_list_popup_button)
		{
			ref var tab_to_select = ref GetRef<ImGuiTabItem>(null);
			if(TabBarTabListPopupButton(ref tab_bar, ref tab_to_select)) // NB: Will alter BarRect.Min.x!
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
		ref var most_recently_selected_tab = ref GetRef<ImGuiTabItem>(null);
		var most_recently_selected_tab_set = false;
		var curr_section_n = -1;
		var found_selected_tab_id = false;
		for(var tab_n = 0; tab_n < tab_bar.Tabs.Size; tab_n++)
		{
			ref var tab = ref tab_bar.Tabs.Get(tab_n);
			if(tab.LastFrameVisible < tab_bar.PrevFrameVisible)
				throw new InvalidOperationException("TabBarLayout() called with a tab that was not visible in the previous frame! This is likely a bug in ImGui.NET or your code.");

			if((!most_recently_selected_tab_set || most_recently_selected_tab.LastFrameSelected < tab.LastFrameSelected) && (tab.Flags & ImGuiTabItemFlags.Button) == 0)
			{
				most_recently_selected_tab = ref tab;
				most_recently_selected_tab_set = true;
			}
			if(tab.ID == tab_bar.SelectedTabId)
				found_selected_tab_id = true;
			if(scroll_to_tab_id == 0 && g.NavJustMovedToId == tab.ID)
				scroll_to_tab_id = tab.ID;

			// Refresh tab width immediately, otherwise changes of style e.g. style.FramePadding.x would noticeably lag in the tab bar.
			// Additionally, when using TabBarAddTab() to manipulate tab bar order we occasionally insert new tabs that don't have a width yet,
			// and we cannot wait for the next BeginTabItem() call. We cannot compute this width within TabBarAddTab() because font size depends on the active window.
			var tab_name = tab_bar.GetTabName(ref tab);
			var has_close_button = (tab.Flags & ImGuiTabItemFlags.NoCloseButton) != 0 ? false : true;
			tab.ContentWidth = (tab.RequestedWidth > 0.0f) ? tab.RequestedWidth : TabItemCalcSize(tab_name, has_close_button).X;

			var section_n = TabItemGetSectionIdx(tab);
			ref var section = ref sections[section_n];
			section.Width += tab.ContentWidth + (section_n == curr_section_n ? g.Style.ItemInnerSpacing.X : 0.0f);
			curr_section_n = section_n;

			// Store data so we can build an array sorted by width if we need to shrink tabs down
			ref var shrink_width_item = ref g.ShrinkWidthBuffer.Get(shrink_buffer_indexes[section_n]++);
			shrink_width_item.Index = tab_n;
			shrink_width_item.Width = shrink_width_item.InitialWidth = tab.ContentWidth;

			if(tab.ContentWidth <= 0)
				throw new InvalidOperationException($"Tab '{tab_name}' has a non-positive width ({tab.ContentWidth})! This is likely a bug in ImGui.NET or your code.");
			tab.Width = tab.ContentWidth;
		}

		// Compute total ideal width (used for e.g. auto-resizing a window)
		tab_bar.WidthAllTabsIdeal = 0.0f;
		for(var section_n = 0; section_n < 3; section_n++)
			tab_bar.WidthAllTabsIdeal += sections[section_n].Width + sections[section_n].Spacing;

		// Horizontal scrolling buttons
		// (note that TabBarScrollButtons() will alter BarRect.Max.x)
		if(tab_bar.WidthAllTabsIdeal > tab_bar.BarRect.Width && tab_bar.Tabs.Size > 1 && (tab_bar.Flags & ImGuiTabBarFlags.NoTabListScrollingButtons) == 0 && (tab_bar.Flags & ImGuiTabBarFlags.FittingPolicyScroll) != 0)
		{
			ref var scroll_and_select_tab = ref GetRef<ImGuiTabItem>(null);
			if(TabBarScrollingButtons(ref tab_bar, ref scroll_and_select_tab))
			{
				scroll_to_tab_id = scroll_and_select_tab.ID;
				if((scroll_and_select_tab.Flags & ImGuiTabItemFlags.Button) == 0)
					tab_bar.SelectedTabId = scroll_to_tab_id;
			}
		}

		// Shrink widths if full tabs don't fit in their allocated space
		var section_0_w = sections[0].Width + sections[0].Spacing;
		var section_1_w = sections[1].Width + sections[1].Spacing;
		var section_2_w = sections[2].Width + sections[2].Spacing;
		var central_section_is_visible = (section_0_w + section_2_w) < tab_bar.BarRect.Width;
		float width_excess;
		if(central_section_is_visible)
			width_excess = Math.Max(section_1_w - (tab_bar.BarRect.Width - section_0_w - section_2_w), 0.0f); // Excess used to shrink central section
		else
			width_excess = section_0_w + section_2_w - tab_bar.BarRect.Width; // Excess used to shrink leading/trailing section

		// With ImGuiTabBarFlags_FittingPolicyScroll policy, we will only shrink leading/trailing if the central section is not visible anymore
		if(width_excess > 0.0f && ((tab_bar.Flags & ImGuiTabBarFlags.FittingPolicyResizeDown) != 0 || !central_section_is_visible))
		{
			var shrink_data_count = central_section_is_visible ? sections[1].TabCount : sections[0].TabCount + sections[2].TabCount;
			var shrink_data_offset = central_section_is_visible ? sections[0].TabCount + sections[2].TabCount : 0;
			ShrinkWidths(ref g.ShrinkWidthBuffer, shrink_data_offset, shrink_data_count, width_excess);

			// Apply shrunk values into tabs and sections
			for(var tab_n = shrink_data_offset; tab_n < shrink_data_offset + shrink_data_count; tab_n++)
			{
				ref var tab = ref tab_bar.Tabs.Get(g.ShrinkWidthBuffer.Get(tab_n).Index);
				var shrinked_width = (float)Math.Floor(g.ShrinkWidthBuffer.Get(tab_n).Width);
				if(shrinked_width < 0.0f)
					continue;

				var section_n = TabItemGetSectionIdx(tab);
				sections[section_n].Width -= tab.Width - shrinked_width;
				tab.Width = shrinked_width;
			}
		}

		// Layout all active tabs
		var section_tab_index = 0;
		var tab_offset = 0.0f;
		tab_bar.WidthAllTabs = 0.0f;
		for(var section_n = 0; section_n < 3; section_n++)
		{
			ref var section = ref sections[section_n];
			if(section_n == 2)
				tab_offset = Math.Clamp(tab_bar.BarRect.Width, 0.0f, tab_offset);

			for(var tab_n = 0; tab_n < section.TabCount; tab_n++)
			{
				ref var tab = ref tab_bar.Tabs.Get(section_tab_index + tab_n);
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
		if(tab_bar.SelectedTabId == 0 && tab_bar.NextSelectedTabId == 0 && most_recently_selected_tab_set)
			scroll_to_tab_id = tab_bar.SelectedTabId = most_recently_selected_tab.ID;

		// Lock in visible tab
		tab_bar.VisibleTabId = tab_bar.SelectedTabId;
		tab_bar.VisibleTabWasSubmitted = false;

		// Update scrolling
		if(scroll_to_tab_id != 0)
			TabBarScrollToTab(ref tab_bar, scroll_to_tab_id, new Span<ImGuiTabBarSection>(sections, 3));
		tab_bar.ScrollingAnim = TabBarScrollClamp(tab_bar, tab_bar.ScrollingAnim);
		tab_bar.ScrollingTarget = TabBarScrollClamp(tab_bar, tab_bar.ScrollingTarget);
		if(tab_bar.ScrollingAnim != tab_bar.ScrollingTarget)
		{
			// Scrolling speed adjust itself so we can always reach our target in 1/3 seconds.
			// Teleport if we are aiming far off the visible line
			tab_bar.ScrollingSpeed = Math.Max(tab_bar.ScrollingSpeed, 70.0f * g.FontSize);
			tab_bar.ScrollingSpeed = Math.Max(tab_bar.ScrollingSpeed, Math.Abs(tab_bar.ScrollingTarget - tab_bar.ScrollingAnim) / 0.3f);
			var teleport = (tab_bar.PrevFrameVisible + 1 < g.FrameCount) || (tab_bar.ScrollingTargetDistToVisibility > 10.0f * g.FontSize);
			tab_bar.ScrollingAnim = teleport ? tab_bar.ScrollingTarget : ImGui.ImLinearSweep(tab_bar.ScrollingAnim, tab_bar.ScrollingTarget, g.IO.DeltaTime * tab_bar.ScrollingSpeed);
		} else
		{
			tab_bar.ScrollingSpeed = 0.0f;
		}
		tab_bar.ScrollingRectMinX = tab_bar.BarRect.Min.X + sections[0].Width + sections[0].Spacing;
		tab_bar.ScrollingRectMaxX = tab_bar.BarRect.Max.X - sections[2].Width - sections[1].Spacing;

		// Clear name buffers
		if((tab_bar.Flags & ImGuiTabBarFlags.DockNode) == 0)
			tab_bar.TabsNames.Buf.Resize(0);

		// Actual layout in host window (we don't do it in BeginTabBar() so as not to waste an extra frame)
		ref var window = ref g.CurrentWindow;
		window.DC.CursorPos = tab_bar.BarRect.Min;
		ImGui.ItemSize(new ImVec2(tab_bar.WidthAllTabs, tab_bar.BarRect.Height), tab_bar.FramePadding.Y);
		window.DC.IdealMaxPos.x = Math.Max(window.DC.IdealMaxPos.x, tab_bar.BarRect.Min.X + tab_bar.WidthAllTabsIdeal);
	}

	private unsafe static ImGuiPtrOrIndex GetTabBarRefFromTabBar(ref ImGuiTabBar tab_bar)
	{
		ref var g = ref ImGuiHelper.GetCurrentContext();
		if(g.TabBars.Contains(ref tab_bar))
			return new ImGuiPtrOrIndex { Index = g.TabBars.GetIndex(ref tab_bar) };
		return new ImGuiPtrOrIndex { Ptr = (nint)GetPointer(ref tab_bar) };
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate int ImQsortCompareFunc(void* a, void* b);

	private static unsafe int TabItemComparerByBeginOrder(void* lhs, void* rhs)
	{
		var a = (ImGuiTabItem*)lhs;
		var b = (ImGuiTabItem*)rhs;
		return a->BeginOrder - b->BeginOrder;
	}
	private static unsafe int TabItemComparerBySection(void* lhs, void* rhs)
	{
		ref var a = ref GetRef((ImGuiTabItem*)lhs);
		ref var b = ref GetRef((ImGuiTabItem*)rhs);
		var a_section = TabItemGetSectionIdx(a);
		var b_section = TabItemGetSectionIdx(b);
		if(a_section != b_section)
			return a_section - b_section;
		return a.IndexDuringLayout - b.IndexDuringLayout;
	}
	private static int TabItemGetSectionIdx(in ImGuiTabItem tab) =>
		(tab.Flags & ImGuiTabItemFlags.Leading) != 0 ? 0 : (tab.Flags & ImGuiTabItemFlags.Trailing) != 0 ? 2 : 1;
	private static unsafe ref ImGuiTabBar GetTabBarFromTabBarRef(ref ImGuiPtrOrIndex tabBarRef)
	{
		if(tabBarRef.Ptr != IntPtr.Zero)
			return ref UnsafeUtility.AsRef<ImGuiTabBar>((void*)tabBarRef.Ptr);
		return ref GetCurrentContext().TabBars.GetByIndex(tabBarRef.Index);
	}
	public static unsafe void ShrinkWidths(ref ImGUI.ImVector<ImGuiShrinkWidthItem> vector, int from, int count, float width_excess)
	{
		var pData = GetPointer(ref vector.Get(0)) + from;
		igShrinkWidths(pData, count, width_excess);
	}
	public static void KeyPressed(ImGuiKey key, Action action)
	{
		ArgumentNullException.ThrowIfNull(action);
		if(ImGui.IsKeyPressed(key))
			action();
	}
	/// <summary>
	/// Displays a button and executes the provided action if the button is clicked.
	/// </summary>
	/// <param name="label">The text to display on the button.</param>
	/// <param name="body">The action to execute when the button is clicked.</param>
	/// <returns>True if the button was clicked and the action was executed; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provided action is null.</exception>
	public static bool Button(string label, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.Button(label))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a button and executes the provided action if the button is clicked.
	/// </summary>
	/// <param name="label">The text to display on the button.</param>
	/// <param name="size">The size of the button.</param>
	/// <param name="body">The action to execute when the button is clicked.</param>
	/// <returns>True if the button was clicked and the action was executed; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown if the provided action is null.</exception>
	public static bool Button(string label, Vector2 size, Action body)
	{
		ArgumentNullException.ThrowIfNull(body);
		if(ImGui.Button(label, size))
		{
			body();
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a color editor for a 3-component color and executes the optionally provided action if the color is changed.
	/// </summary>
	/// <param name="label">The label for the color editor.</param>
	/// <param name="color">The color to edit.</param>
	/// <param name="body">An optional action to execute when the color is changed.</param>
	/// <returns>True if the color was changed and the action was executed; otherwise, false.</returns>
	public static bool ColorEdit3(string label, ref Color color, Action<Color>? body = null)
	{
		var colorVec = color.ToVector3();
		if(ImGui.ColorEdit3(label, ref colorVec))
		{
			color = colorVec.ToColor();
			if(body != null)
				body(color);
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a color editor for a 3-component color and executes the optionally provided action if the color is changed.
	/// </summary>
	/// <param name="label">The label for the color editor.</param>
	/// <param name="color">The color to edit.</param>
	/// <param name="flags">Color edit flags to modify the behavior of the color editor.</param>
	/// <param name="body">An optional action to execute when the color is changed.</param>
	/// <returns>True if the color was changed and the action was executed; otherwise, false.</returns>
	public static bool ColorEdit3(string label, ref Color color, ImGuiColorEditFlags flags, Action<Color>? body = null)
	{
		var colorVec = color.ToVector3();
		if(ImGui.ColorEdit3(label, ref colorVec, flags))
		{
			color = colorVec.ToColor();
			if(body != null)
				body(color);
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a color editor for a 3-component color and executes the optionally provided action if the color is changed.
	/// </summary>
	/// <param name="label">The label for the color editor.</param>
	/// <param name="color">The color to edit.</param>
	/// <param name="body">An optional action to execute when the color is changed.</param>
	/// <returns>True if the color was changed and the action was executed; otherwise, false.</returns>
	public static bool ColorEdit3(string label, Color color, Action<Color>? body = null)
	{
		var colorVec = color.ToVector3();
		if(ImGui.ColorEdit3(label, ref colorVec))
		{
			color = colorVec.ToColor();
			if(body != null)
				body(color);
			return true;
		}
		return false;
	}
	/// <summary>
	/// Displays a color editor for a 3-component color and executes the optionally provided action if the color is changed.
	/// </summary>
	/// <param name="label">The label for the color editor.</param>
	/// <param name="color">The color to edit.</param>
	/// <param name="flags">Color edit flags to modify the behavior of the color editor.</param>
	/// <param name="body">An optional action to execute when the color is changed.</param>
	/// <returns>True if the color was changed and the action was executed; otherwise, false.</returns>
	public static bool ColorEdit3(string label, Color color, ImGuiColorEditFlags flags, Action<Color>? body = null)
	{
		var colorVec = color.ToVector3();
		if(ImGui.ColorEdit3(label, ref colorVec, flags))
		{
			color = colorVec.ToColor();
			if(body != null)
				body(color);
			return true;
		}
		return false;
	}
	public static void Image(Sprite sprite)
	{
		ArgumentNullException.ThrowIfNull(sprite);
		var textureId = (IntPtr)_textureManager.GetTextureId(sprite.texture);
		if(sprite.uv.Length != 2)
			throw new ApplicationException("Only rectangular sprites are supported");
		ImGui.Image(textureId, sprite.textureRect.size, sprite.uv[0], sprite.uv[1]);
	}
	public static void Image(Texture texture)
	{
		ArgumentNullException.ThrowIfNull(texture);
		var textureId = (IntPtr)_textureManager.GetTextureId(texture);
		ImGui.Image(textureId, new Vector2(texture.width, texture.height));
	}
	public static void Image(Texture texture, Vector2 size)
	{
		ArgumentNullException.ThrowIfNull(texture);
		var textureId = (IntPtr)_textureManager.GetTextureId(texture);
		ImGui.Image(textureId, size);
	}
	private static readonly Dictionary<ImGuiCol, Color> _stationeersColorScheme = new()
	{
		{ ImGuiCol.Text, Color.FromString("#ffffff") },
		{ ImGuiCol.TextDisabled, Color.FromString("#808080") },
		{ ImGuiCol.WindowBg, Color.FromString("#2F2F37") },
		{ ImGuiCol.ChildBg, Color.FromString("#2f2f37") },
		{ ImGuiCol.PopupBg, Color.FromString("#202020") },
		{ ImGuiCol.Border, Color.FromString("#6E6E80") },
		{ ImGuiCol.BorderShadow, Color.FromString("#000000") },
		{ ImGuiCol.FrameBg, Color.FromString("#212129") },
		{ ImGuiCol.FrameBgHovered, Color.FromString("#57575a") },
		{ ImGuiCol.FrameBgActive, Color.FromString("#71403d") },
		{ ImGuiCol.TitleBg, Color.FromString("#292933") },
		{ ImGuiCol.TitleBgActive, Color.FromString("#71403d") },
		{ ImGuiCol.TitleBgCollapsed, Color.FromString("#000000") },
		{ ImGuiCol.MenuBarBg, Color.FromString("#242424") },
		{ ImGuiCol.ScrollbarBg, Color.FromString("#050505") },
		{ ImGuiCol.ScrollbarGrab, Color.FromString("#ffffff") },
		{ ImGuiCol.ScrollbarGrabHovered, Color.FromString("#ffffff") },
		{ ImGuiCol.ScrollbarGrabActive, Color.FromString("#c8c8c8") },
		{ ImGuiCol.CheckMark, Color.FromString("#ff6617") },
		{ ImGuiCol.SliderGrab, Color.FromString("#ff6617") },
		{ ImGuiCol.SliderGrabActive, Color.FromString("#71403d") },
		{ ImGuiCol.Button, Color.FromString("#1c1c21") },
		{ ImGuiCol.ButtonHovered, Color.FromString("#57575a") },
		{ ImGuiCol.ButtonActive, Color.FromString("#71403d") },
		{ ImGuiCol.Header, Color.FromString("#1c1c21") },
		{ ImGuiCol.HeaderHovered, Color.FromString("#57575a") },
		{ ImGuiCol.HeaderActive, Color.FromString("#71403d") },
		{ ImGuiCol.Separator, Color.FromString("#6E6E80") },
		{ ImGuiCol.SeparatorHovered, Color.FromString("#1a66c0") },
		{ ImGuiCol.SeparatorActive, Color.FromString("#1a66c0") },
		{ ImGuiCol.ResizeGrip, Color.FromString("#ffffff") },
		{ ImGuiCol.ResizeGripHovered, Color.FromString("#ffffff") },
		{ ImGuiCol.ResizeGripActive, Color.FromString("#c8c8c8") },
		{ ImGuiCol.Tab, Color.FromString("#1c1c21") },
		{ ImGuiCol.TabHovered, Color.FromString("#57575a") },
		{ ImGuiCol.TabActive, Color.FromString("#ff6617") },
		{ ImGuiCol.TabUnfocused, Color.FromString("#121a26") },
		{ ImGuiCol.TabUnfocusedActive, Color.FromString("#71403d") },
		{ ImGuiCol.PlotLines, Color.FromString("#9c9c9c") },
		{ ImGuiCol.PlotLinesHovered, Color.FromString("#ff6E5a") },
		{ ImGuiCol.PlotHistogram, Color.FromString("#e6b300") },
		{ ImGuiCol.PlotHistogramHovered, Color.FromString("#ff9a00") },
		{ ImGuiCol.TableHeaderBg, Color.FromString("#313133") },
		{ ImGuiCol.TableBorderStrong, Color.FromString("#4f4f5a") },
		{ ImGuiCol.TableBorderLight, Color.FromString("#3b3b40") },
		{ ImGuiCol.TableRowBg, Color.FromString("#000000") },
		{ ImGuiCol.TableRowBgAlt, Color.FromString("#ffffff") },
		{ ImGuiCol.TextSelectedBg, Color.FromString("#4397fb") },
		{ ImGuiCol.DragDropTarget, Color.FromString("#ffff00") },
		{ ImGuiCol.NavHighlight, Color.FromString("#4397fb") },
		{ ImGuiCol.NavWindowingHighlight, Color.FromString("#ffffff") },
		{ ImGuiCol.NavWindowingDimBg, Color.FromString("#cdcdcd") },
		{ ImGuiCol.ModalWindowDimBg, Color.FromString("#cdcdcd") },
	};
	public static void ApplyStationeersStyle()
	{
		ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 1.0f);
		ImGui.PushStyleVar(ImGuiStyleVar.DisabledAlpha, 0.8f);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
		ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 11);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1);
		ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, new Vector2(32, 32));
		ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign, new Vector2(0.5f, 0.5f));
		ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1);
		ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1);
		ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 4));
		ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
		ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 4));
		ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(4, 4));
		ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(4, 4));
		ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20);
		ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarSize, 11);
		ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 20);
		ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 4);
		ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
		ImGui.PushStyleVar(ImGuiStyleVar.SelectableTextAlign, new Vector2(0, 0));
		foreach(var entry in _stationeersColorScheme)
			ImGui.PushStyleColor(entry.Key, entry.Value);
	}
	public static void RestoreDefaultStyle()
	{
		ImGui.PopStyleVar(25);
		ImGui.PopStyleColor(53);
	}
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static unsafe extern void igImQsort(void* basePtr, nuint count, nuint sizeOfElement, ImQsortCompareFunc compareFunc);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static unsafe extern ImGuiContext* igGetCurrentContext();
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private unsafe static extern ImGuiTabItem* igTabBarFindTabByID(ImGuiTabBar* tab_bar, uint tab_id);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private unsafe static extern byte igTabBarProcessReorder(ImGuiTabBar* tab_bar);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private unsafe static extern void igShrinkWidths(ImGuiShrinkWidthItem* items, int count, float width_excess);
}