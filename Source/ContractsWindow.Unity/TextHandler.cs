#region license
/*The MIT License (MIT)
TextHandler - Script for handling Text object replacement with Text Mesh Pro

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
using UnityEngine.Events;

namespace ContractsWindow.Unity
{
	public class TextHandler : MonoBehaviour
	{
		[SerializeField]
		private bool m_LocalizedText = false;
		[SerializeField]
		private string m_LocalizedName = "";
        [SerializeField]
        private bool m_CutoffText = false;

		public class OnTextEvent : UnityEvent<string> { }

		public class OnColorEvent : UnityEvent<Color> { }

		public class OnFontEvent : UnityEvent<int> { }

		private OnTextEvent _onTextUpdate = new OnTextEvent();
		private OnColorEvent _onColorUpdate = new OnColorEvent();
		private OnFontEvent _onFontChange = new OnFontEvent();

		private Vector2 _preferredSize = new Vector2();

		public bool LocalizedText
		{
			get { return m_LocalizedText; }
		}

        public bool CutoffText
        {
            get { return m_CutoffText; }
        }

        public string LocalizedName
		{
			get { return m_LocalizedName; }
		}

		public void SetLocalText(string text)
		{
			Text t = GetComponent<Text>();

			if (t != null)
				t.text = text;
		}

		public Vector2 PreferredSize
		{
			get { return _preferredSize; }
			set { _preferredSize = value; }
		}

		public UnityEvent<string> OnTextUpdate
		{
			get { return _onTextUpdate; }
		}

		public UnityEvent<Color> OnColorUpdate
		{
			get { return _onColorUpdate; }
		}

		public UnityEvent<int> OnFontChange
		{
			get { return _onFontChange; }
		}
	}
}
