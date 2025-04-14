#nullable enable
using ImGuiNET.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Assets.Scripts
{
	public class ImGuiHost : MonoBehaviour
	{
		[SerializeField]
		private RenderTexture _ImGuiTargetRenderTexture;

		// Update is called once per frame
		void Update()
		{
			if (this._ImGuiTargetRenderTexture.width != Screen.width || this._ImGuiTargetRenderTexture.height != Screen.height)
			{
				this._ImGuiTargetRenderTexture = new RenderTexture(Screen.width, Screen.height, this._ImGuiTargetRenderTexture.depth);
				GetComponent<DearImGui>().Camera.targetTexture = this._ImGuiTargetRenderTexture;
			}

		}
		private void OnGUI()
		{
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this._ImGuiTargetRenderTexture);
		}
	}
}
