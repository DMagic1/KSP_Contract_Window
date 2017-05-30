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
		[SerializeField]
		private string[] m_TooltipNames = new string[1] { "" };
		[SerializeField]
		private string[] _tooltipText = new string[1] { "" };
		[SerializeField]
		private GameObject _prefab = null;
		[SerializeField]
		private bool m_IsActive = true;

		private int _tooltipIndex;

		private Canvas _canvas;
		private ToolTip _tooltip;
		private float _scale;

		public int TooltipCount
		{
			get
			{
				if (m_TooltipNames == null)
					return 0;

				return m_TooltipNames.Length;
			}
		}

		public string TooltipNames(int index)
		{
			if (m_TooltipNames == null)
				return "";

			if (index >= m_TooltipNames.Length)
				return "";

			if (index < 0)
				return "";

			return m_TooltipNames[index];
		}

		public string[] TooltipText
		{
			set { _tooltipText = value; }
		}

		public int TooltipIndex
		{
			set
			{
				if (value < 0 || value >= TooltipCount)
					value = 0;

				_tooltipIndex = value;
			}
		}

		public Canvas _Canvas
		{
			set { _canvas = value; }
		}

		public GameObject Prefab
		{
			set { _prefab = value; }
		}

		public float Scale
		{
			set { _scale = value; }
		}

		public bool IsActive
		{
			set { m_IsActive = value; }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!m_IsActive)
				return;

			OpenTooltip();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!m_IsActive)
				return;

			CloseTooltip();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			CloseTooltip();
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Scroll == null)
				return;

			CW_Window.Window.Scroll.OnScroll(eventData);
		}

		private void OpenTooltip()
		{
			if (_prefab == null || _canvas == null || _tooltipText == null || _tooltipText.Length <= 0)
				return;

			_tooltip = Instantiate(_prefab).GetComponent<ToolTip>();

			if (_tooltip == null)
				return;

			_tooltip.transform.SetParent(_canvas.transform, false);
			_tooltip.transform.SetAsLastSibling();

			_tooltip.Setup(_canvas, _tooltipText[_tooltipIndex], _scale);
		}

		private void CloseTooltip()
		{
			if (_tooltip == null)
				return;

			_tooltip.gameObject.SetActive(false);
			Destroy(_tooltip.gameObject);
			_tooltip = null;
		}
	}
}
