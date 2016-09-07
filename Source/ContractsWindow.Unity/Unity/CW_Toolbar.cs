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

		private CW_Window parent;

		public void setToolbar(CW_Window p)
		{
			if (p == null)
				return;

			if (p.Interface == null)
				return;

			parent = p;

			if (StockToggle == null)
				return;

			if (!p.Interface.BlizzyAvailable)
				StockToggle.gameObject.SetActive(false);

			if (StockReplace == null)
				return;

			if (!p.Interface.StockToolbar)
				StockReplace.gameObject.SetActive(false);
		}

		public void UseStockToolbar(bool isOn)
		{
			if (StockToggle == null || StockReplace == null)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			if (!parent.Interface.BlizzyAvailable)
			{
				parent.Interface.StockToolbar = true;
				return;
			}

			parent.Interface.StockToolbar = isOn;

			StockToggle.gameObject.SetActive(isOn);
		}

		public void ReplaceStockToolbar(bool isOn)
		{
			if (StockReplace == null)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			parent.Interface.ReplaceToolbar = isOn;
		}

		public void Close()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
