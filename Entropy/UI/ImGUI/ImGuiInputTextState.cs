#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using ImGuiNET;
using UnityEngine;
using ImGuiID = uint;

namespace Entropy.UI.ImGUI;
public struct ImGuiInputTextState
{
	public ImGuiID ID;                     // widget id owning the text state
	public int CurLenW, CurLenA;       // we need to maintain our buffer length in both UTF-8 and wchar format. UTF-8 length is valid even if TextA is not.
	public ImVector<char> TextW;                  // edit buffer, we need to persist but can't guarantee the persistence of the user-provided buffer. so we copy into own buffer.
	public ImVector<byte> TextA;                  // temporary UTF8 buffer for callbacks and other operations. this is not updated in every code-path! size=capacity.
	public ImVector<byte> InitialTextA;           // backup of end-user buffer at the time of focus (in UTF-8, unaltered)
	public bool TextAIsValid;           // temporary UTF8 buffer is not initially valid before we make the widget active (until then we pull the data from user argument)
	public int BufCapacityA;           // end-user buffer capacity
	public float ScrollX;                // horizontal scrolling/offset
	public STB_TexteditState Stb;                   // state for stb_textedit.h
	public float CursorAnim;             // timer for cursor blink, reset on every user action so the cursor reappears immediately
	public bool CursorFollow;           // set when we want scrolling to follow the current cursor position (not always!)
	public bool SelectedAllMouseLock;   // after a double-click to select all, we ignore further mouse drags to update selection
	public bool Edited;                 // edited this frame
	public ImGuiInputTextFlags Flags;                  // copy of InputText() flags
}
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member