#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS1570 // XML comment has badly formed XML
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
#pragma warning disable CA1815 // Override equals and operator equals on value types

namespace Entropy.Common.UI.ImGUI;

public unsafe struct ImGuiErrorRecoveryState
{
	private short _sizeOfWindowStack;
	private short _sizeOfIDStack;
	private short _sizeOfTreeStack;
	private short _sizeOfColorStack;
	private short _sizeOfStyleVarStack;
	private short _sizeOfFontStack;
	private short _sizeOfFocusScopeStack;
	private short _sizeOfGroupStack;
	private short _sizeOfItemFlagsStack;
	private short _sizeOfBeginPopupStack;
	private short _sizeOfDisabledStack;

	public ref short SizeOfWindowStack => ref this._sizeOfWindowStack;
	public ref short SizeOfIDStack => ref this._sizeOfIDStack;
	public ref short SizeOfTreeStack => ref this._sizeOfTreeStack;
	public ref short SizeOfColorStack => ref this._sizeOfColorStack;
	public ref short SizeOfStyleVarStack => ref this._sizeOfStyleVarStack;
	public ref short SizeOfFontStack => ref this._sizeOfFontStack;
	public ref short SizeOfFocusScopeStack => ref this._sizeOfFocusScopeStack;
	public ref short SizeOfGroupStack => ref this._sizeOfGroupStack;
	public ref short SizeOfItemFlagsStack => ref this._sizeOfItemFlagsStack;
	public ref short SizeOfBeginPopupStack => ref this._sizeOfBeginPopupStack;
	public ref short SizeOfDisabledStack => ref this._sizeOfDisabledStack;
}