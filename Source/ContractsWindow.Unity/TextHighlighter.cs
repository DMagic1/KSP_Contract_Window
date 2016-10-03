#region license
/*The MIT License (MIT)
TextHighlighter - Script for handling text color changes on mouse-over

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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace ContractsWindow.Unity
{
	public class TextHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IScrollHandler
	{
		[SerializeField]
		private Color NormalColor = Color.white;
		[SerializeField]
		private Color HighlightColor = Color.yellow;

		private ScrollRect scroller;
		private bool _hover;
		private TextHandler _attachedText;

		public bool Hover
		{
			get { return _hover; }
		}

		private void Awake()
		{
			_attachedText = GetComponent<TextHandler>();
		}

		private void SetColor(Color c)
		{

		}

		public void setScroller(ScrollRect s)
		{
			scroller = s;
		}

		public void setNormalColor(Color c)
		{
			NormalColor = c;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_attachedText == null)
				return;

			_hover = true;

			_attachedText.OnColorUpdate.Invoke(HighlightColor);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_hover = false;

			if (_attachedText == null)
				return;

			_attachedText.OnColorUpdate.Invoke(NormalColor);
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (scroller == null)
				return;

			scroller.OnScroll(eventData);
		}

	}
}
