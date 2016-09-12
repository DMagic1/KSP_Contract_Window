using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
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

			if (StockToggle == null)
				return;

			if (!CW_Window.Window.Interface.BlizzyAvailable)
				StockToggle.gameObject.SetActive(false);

			if (StockReplace == null)
				return;

			if (!CW_Window.Window.Interface.StockToolbar)
				StockReplace.gameObject.SetActive(false);
		}

		public void UseStockToolbar(bool isOn)
		{
			if (StockToggle == null || StockReplace == null)
				return;

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

			StockToggle.gameObject.SetActive(isOn);
		}

		public void ReplaceStockToolbar(bool isOn)
		{
			if (StockReplace == null)
				return;

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

			CW_Window.Window.DestroyChild(gameObject);
		}
	}
}
