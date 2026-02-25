#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiStorage
{
	private ImVector<ImGuiStoragePair> _data;

	// [Internal]
	public ref ImVector<ImGuiStoragePair> Data => ref this._data;

	// - Get***() functions find pair, never add/allocate. Pairs are sorted so a query is O(log N)
	// - Set***() functions find pair, insertion on demand if missing.
	// - Sorted insertion is costly, paid once. A typical frame shouldn't need to insert any new pair.
	public void Clear() => ImGuiStorage_Clear(ref this);
	public int GetInt(ImGuiID key, int default_val = 0) => ImGuiStorage_GetInt(ref this, key, default_val);
	public void SetInt(ImGuiID key, int val) => ImGuiStorage_SetInt(ref this, key, val);
	public bool GetBool(ImGuiID key, bool default_val = false) => ImGuiStorage_GetBool(ref this, key, default_val);
	public void SetBool(ImGuiID key, bool val) => ImGuiStorage_SetBool(ref this, key, val);
	public float GetFloat(ImGuiID key, float default_val = 0.0f) => ImGuiStorage_GetFloat(ref this, key, default_val);
	public void SetFloat(ImGuiID key, float val) => ImGuiStorage_SetFloat(ref this, key, val);
	public void* GetVoidPtr(ImGuiID key) => ImGuiStorage_GetVoidPtr(ref this, key);
	public void SetVoidPtr(ImGuiID key, void* val) => ImGuiStorage_SetVoidPtr(ref this, key, val);

	// - Get***Ref() functions finds pair, insert on demand if missing, return pointer. Useful if you intend to do Get+Set.
	// - References are only valid until a new value is added to the storage. Calling a Set***() function or a Get***Ref() function invalidates the pointer.
	// - A typical use case where this is convenient for quick hacking (e.g. add storage during a live Edit&Continue session if you can't modify existing struct)
	//      float* pvar = ImGui::GetFloatRef(key); ImGui::SliderFloat("var", pvar, 0, 100.0f); some_var += *pvar;
	public int* GetIntRef(ImGuiID key, int default_val = 0) => ImGuiStorage_GetIntRef(ref this, key, default_val);
	public bool* GetBoolRef(ImGuiID key, bool default_val = false) => ImGuiStorage_GetBoolRef(ref this, key, default_val);
	public float* GetFloatRef(ImGuiID key, float default_val = 0.0f) => ImGuiStorage_GetFloatRef(ref this, key, default_val);
	public void** GetVoidPtrRef(ImGuiID key, void* default_val = null) => ImGuiStorage_GetVoidPtrRef(ref this, key, default_val);

	// Advanced: for quicker full rebuild of a storage (instead of an incremental one), you may add all your contents and then sort once.
	public void BuildSortByKey() => ImGuiStorage_BuildSortByKey(this);
	// Obsolete: use on your own storage if you know only integer are being stored (open/close all tree nodes)
	public void SetAllInt(int val) => ImGuiStorage_SetAllInt(ref this, val);

	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_BuildSortByKey(in ImGuiStorage self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_Clear(ref ImGuiStorage self);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern bool ImGuiStorage_GetBool(ref ImGuiStorage self, ImGuiID key, [MarshalAs(UnmanagedType.U1)] bool default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern bool* ImGuiStorage_GetBoolRef(ref ImGuiStorage self, ImGuiID key, [MarshalAs(UnmanagedType.U1)] bool default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern float ImGuiStorage_GetFloat(ref ImGuiStorage self, ImGuiID key, float default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern float* ImGuiStorage_GetFloatRef(ref ImGuiStorage self, ImGuiID key, float default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern int ImGuiStorage_GetInt(ref ImGuiStorage self, ImGuiID key, int default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern int* ImGuiStorage_GetIntRef(ref ImGuiStorage self, ImGuiID key, int default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void* ImGuiStorage_GetVoidPtr(ref ImGuiStorage self, ImGuiID key);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void** ImGuiStorage_GetVoidPtrRef(ref ImGuiStorage self, ImGuiID key, void* default_val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_SetAllInt(ref ImGuiStorage self, int val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_SetBool(ref ImGuiStorage self, ImGuiID key, [MarshalAs(UnmanagedType.U1)] bool val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_SetFloat(ref ImGuiStorage self, ImGuiID key, float val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_SetInt(ref ImGuiStorage self, ImGuiID key, int val);
	[DllImport("cimgui")]
	[SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes")]
	private static extern void ImGuiStorage_SetVoidPtr(ref ImGuiStorage self, ImGuiID key, void* val);
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiStoragePair
{
	[FieldOffset(0)]
	ImGuiID _key;

	[FieldOffset(4)]
	int _i;
	[FieldOffset(4)]
	float _f;
	[FieldOffset(4)]
	void* _p;

	public ref ImGuiID Key => ref this._key;

	public ref int Int => ref this._i;
	public ref float Float => ref this._f;
	public ref void* Pointer => ref this._p;

	ImGuiStoragePair(ImGuiID key, int val)
	{
		this._key = key;
		this._i = val;
	}

	ImGuiStoragePair(ImGuiID key, float val)
	{
		this._key = key;
		this._f = val;
	}

	ImGuiStoragePair(ImGuiID key, void* val)
	{
		this._key = key;
		this._p = val;
	}
}