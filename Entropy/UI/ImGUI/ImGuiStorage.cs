#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Runtime.InteropServices;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;

public unsafe struct ImGuiStorage
{
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_BuildSortByKey(ImGuiStorage* self);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_Clear(ImGuiStorage* self);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool ImGuiStorage_GetBool(ImGuiStorage* self, ImGuiID key, bool default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern bool* ImGuiStorage_GetBoolRef(ImGuiStorage* self, ImGuiID key, bool default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern float ImGuiStorage_GetFloat(ImGuiStorage* self, ImGuiID key, float default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern float* ImGuiStorage_GetFloatRef(ImGuiStorage* self, ImGuiID key, float default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern int ImGuiStorage_GetInt(ImGuiStorage* self, ImGuiID key, int default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern int* ImGuiStorage_GetIntRef(ImGuiStorage* self, ImGuiID key, int default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void* ImGuiStorage_GetVoidPtr(ImGuiStorage* self, ImGuiID key);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void** ImGuiStorage_GetVoidPtrRef(ImGuiStorage* self, ImGuiID key, void* default_val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_SetAllInt(ImGuiStorage* self, int val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_SetBool(ImGuiStorage* self, ImGuiID key, bool val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_SetFloat(ImGuiStorage* self, ImGuiID key, float val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_SetInt(ImGuiStorage* self, ImGuiID key, int val);
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	private static extern void ImGuiStorage_SetVoidPtr(ImGuiStorage* self, ImGuiID key, void* val);

	// [Internal]
	public ImVector<ImGuiStoragePair> Data;

	// - Get***() functions find pair, never add/allocate. Pairs are sorted so a query is O(log N)
	// - Set***() functions find pair, insertion on demand if missing.
	// - Sorted insertion is costly, paid once. A typical frame shouldn't need to insert any new pair.
	public void Clear() => ImGuiStorage_Clear(ThisPtr);
	public int GetInt(ImGuiID key, int default_val = 0) => ImGuiStorage_GetInt(ThisPtr, key, default_val);
	public void SetInt(ImGuiID key, int val) => ImGuiStorage_SetInt(ThisPtr, key, val);
	public bool GetBool(ImGuiID key, bool default_val = false) => ImGuiStorage_GetBool(ThisPtr, key, default_val);
	public void SetBool(ImGuiID key, bool val) => ImGuiStorage_SetBool(ThisPtr, key, val);
	public float GetFloat(ImGuiID key, float default_val = 0.0f) => ImGuiStorage_GetFloat(ThisPtr, key, default_val);
	public void SetFloat(ImGuiID key, float val) => ImGuiStorage_SetFloat(ThisPtr, key, val);
	public void* GetVoidPtr(ImGuiID key) => ImGuiStorage_GetVoidPtr(ThisPtr, key);
	public void SetVoidPtr(ImGuiID key, void* val) => ImGuiStorage_SetVoidPtr(ThisPtr, key, val);

	// - Get***Ref() functions finds pair, insert on demand if missing, return pointer. Useful if you intend to do Get+Set.
	// - References are only valid until a new value is added to the storage. Calling a Set***() function or a Get***Ref() function invalidates the pointer.
	// - A typical use case where this is convenient for quick hacking (e.g. add storage during a live Edit&Continue session if you can't modify existing struct)
	//      float* pvar = ImGui::GetFloatRef(key); ImGui::SliderFloat("var", pvar, 0, 100.0f); some_var += *pvar;
	public int* GetIntRef(ImGuiID key, int default_val = 0) => ImGuiStorage_GetIntRef(ThisPtr, key, default_val);
	public bool* GetBoolRef(ImGuiID key, bool default_val = false) => ImGuiStorage_GetBoolRef(ThisPtr, key, default_val);
	public float* GetFloatRef(ImGuiID key, float default_val = 0.0f) => ImGuiStorage_GetFloatRef(ThisPtr, key, default_val);
	public void** GetVoidPtrRef(ImGuiID key, void* default_val = null) => ImGuiStorage_GetVoidPtrRef(ThisPtr, key, default_val);

	// Advanced: for quicker full rebuild of a storage (instead of an incremental one), you may add all your contents and then sort once.
	public void BuildSortByKey() => ImGuiStorage_BuildSortByKey(ThisPtr);
	// Obsolete: use on your own storage if you know only integer are being stored (open/close all tree nodes)
	public void SetAllInt(int val) => ImGuiStorage_SetAllInt(ThisPtr, val);

	private ImGuiStorage* ThisPtr
	{
		get
		{
			fixed(ImGuiStorage* ptr = &this)
				return ptr;
		}
	}
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ImGuiStoragePair
{
	[FieldOffset(0)]
	public ImGuiID key;

	[FieldOffset(4)]
	public int val_i;
	[FieldOffset(4)]
	public float val_f;
	[FieldOffset(4)]
	public void* val_p;

	ImGuiStoragePair(ImGuiID _key, int _val)
	{
		this.key = _key;
		this.val_i = _val;
	}

	ImGuiStoragePair(ImGuiID _key, float _val)
	{
		this.key = _key;
		this.val_f = _val;
	}

	ImGuiStoragePair(ImGuiID _key, void* _val)
	{
		this.key = _key;
		this.val_p = _val;
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member