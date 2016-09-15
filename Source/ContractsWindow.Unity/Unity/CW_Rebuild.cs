using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_Rebuild : CW_Popup
	{
		private void Start()
		{
			FadeIn();
		}

		public void Rebuild()
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_Window.Window.Interface.Rebuild();

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
