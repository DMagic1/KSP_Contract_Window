#region license
/*The MIT License (MIT)
CW_Popup - Monobehaviour to mark certain UI objects

Copyright (c) 2014 DMagic

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

namespace ContractsWindow.Unity
{
	public abstract class CW_Popup : CanvasFader
	{
        public delegate void PopupFade(CW_Popup popup);
        public delegate void PopupInputLock(bool isOn);

        protected PopupFade OnPopupFade;
        protected PopupInputLock OnPopupInputLock;

        protected bool inputLock;

		protected void FadeIn()
		{
			Alpha(0);

			Fade(1, true);
		}

		public void FadeOut(Action call)
		{
			Fade(0, false, call, false);
		}

		public abstract void ClosePopup();
	}
}
