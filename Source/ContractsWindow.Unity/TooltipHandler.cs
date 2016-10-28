#region license
/*The MIT License (MIT)
TooltipHandler - Script to control tooltip activation

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
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity
{

	public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IScrollHandler
	{
		[SerializeField, TextArea(2, 10)]
		private string Text = "";
		[SerializeField]
		private bool isActive = true;

		private ToolTip tooltip;

		private void Start()
		{
			if (string.IsNullOrEmpty(Text))
				return;

			GameObject obj = Instantiate(CW_Window.Window.Tooltip);

			if (obj == null)
				return;

			tooltip = obj.GetComponent<ToolTip>();

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Tooltip == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			if (CW_Window.Window.Interface.MainCanvas == null)
				return;

			obj.transform.SetParent(CW_Window.Window.Interface.MainCanvas.transform, false);
			obj.transform.SetAsLastSibling();
		}

		public void SetNewText(string s)
		{
			Text = s;
		}

		public bool IsActive
		{
			set { isActive = value; }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			if (!isActive)
				return;

			tooltip.SetTooltip(Text);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			tooltip.HideTooltip();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			tooltip.HideTooltip();
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Scroll == null)
				return;

			CW_Window.Window.Scroll.OnScroll(eventData);
		}
	}
}
