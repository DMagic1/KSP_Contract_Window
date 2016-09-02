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

		private IToolbarPanel toolbarInterface;
		private CW_Window parent;

		public void setToolbar(IToolbarPanel toolbar, CW_Window p)
		{
			if (toolbar == null)
				return;

			if (p == null)
				return;

			toolbarInterface = toolbar;

			parent = p;

			if (StockToggle == null)
				return;

			if (!toolbar.BlizzyAvailable)
				StockToggle.gameObject.SetActive(false);

			if (StockReplace == null)
				return;

			if (!toolbar.UsingStock)
				StockReplace.gameObject.SetActive(false);
		}

		public void UseStockToolbar(bool isOn)
		{
			if (StockToggle == null || StockReplace == null)
				return;

			if (toolbarInterface == null)
				return;

			if (!toolbarInterface.BlizzyAvailable)
			{
				toolbarInterface.UsingStock = true;
				return;
			}

			toolbarInterface.UsingStock = isOn;

			if (!isOn)
				StockToggle.gameObject.SetActive(false);
		}

		public void ReplaceStockToolbar(bool isOn)
		{
			if (StockReplace == null)
				return;

			if (toolbarInterface == null)
				return;

			toolbarInterface.ReplacingStock = isOn;
		}

		public void Close()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
