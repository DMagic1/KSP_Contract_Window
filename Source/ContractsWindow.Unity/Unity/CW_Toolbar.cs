using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_Toolbar : CW_Popup
	{
		[SerializeField]
		private Toggle StockToggle = null;
		[SerializeField]
		private Toggle StockReplace = null;

		public void setToolbar()
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			if (StockToggle == null && !CW_Window.Window.Interface.BlizzyAvailable)
				StockToggle.gameObject.SetActive(false);

			if (StockReplace == null && !CW_Window.Window.Interface.StockToolbar)
				StockReplace.gameObject.SetActive(false);

			FadeIn();
		}

		public void UseStockToolbar(bool isOn)
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			if (!CW_Window.Window.Interface.BlizzyAvailable)
			{
				CW_Window.Window.Interface.StockToolbar = true;
				return;
			}

			CW_Window.Window.Interface.StockToolbar = isOn;

			if (StockReplace != null)
				StockReplace.gameObject.SetActive(isOn);
		}

		public void ReplaceStockToolbar(bool isOn)
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_Window.Window.Interface.ReplaceToolbar = isOn;
		}

		public void Close()
		{
			if (CW_Window.Window == null)
				return;

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
