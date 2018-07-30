#region license
/*The MIT License (MIT)
CW_Toolbar - Controls the toolbar options popup

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

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
		[SerializeField]
		private Toggle StockUIStyle = null;

        public delegate void StockToolbarToggle(bool isOn);
        public delegate void ReplaceToolbarToggle(bool isOn);
        public delegate void StockUIToggle(bool isOn);

        private StockToolbarToggle OnStockToolbarToggle;
        private ReplaceToolbarToggle OnReplaceToolbarToggle;
        private StockUIToggle OnStockUIToggle;

        private bool loaded;
        private bool _blizzyAvailable;

		public void setToolbar(bool stockToolbar, bool blizzyAvailable, bool replaceStock, bool stockUI
            , StockToolbarToggle stockToolbarToggle, ReplaceToolbarToggle replaceToolbarToggle, StockUIToggle stockUIToggle, PopupFade popupFade)
		{
            OnStockToolbarToggle = stockToolbarToggle;
            OnReplaceToolbarToggle = replaceToolbarToggle;
            OnStockUIToggle = stockUIToggle;
            OnPopupFade = popupFade;

            _blizzyAvailable = blizzyAvailable;


            if (StockToggle != null)
			{
				StockToggle.isOn = stockToolbar;

				if (!blizzyAvailable)
					StockToggle.gameObject.SetActive(false);
			}

			if (StockReplace != null)
			{
				StockReplace.isOn = replaceStock;

				if (!stockToolbar)
					StockReplace.gameObject.SetActive(false);
			}

			if (StockUIStyle != null)
				StockUIStyle.isOn = stockUI;

			loaded = true;

			FadeIn();
		}

		public void UseStockToolbar(bool isOn)
		{
			if (!loaded)
				return;
            
			if (!_blizzyAvailable)
			{
                OnStockToolbarToggle.Invoke(true);
                return;
			}

            OnStockToolbarToggle.Invoke(isOn);
            
			if (StockReplace != null)
				StockReplace.gameObject.SetActive(isOn);
		}

		public void ReplaceStockToolbar(bool isOn)
		{
			if (!loaded)
				return;

            OnReplaceToolbarToggle.Invoke(isOn);
		}

		public void ToggleUIStyle(bool isOn)
		{
			if (!loaded)
				return;

            OnStockUIToggle.Invoke(isOn);
		}

		public void Close()
		{
            OnPopupFade.Invoke(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
