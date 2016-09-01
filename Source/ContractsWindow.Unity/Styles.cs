#region license
/*The MIT License (MIT)
Styles - Storage class for holding some UI style elements

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

namespace ContractsWindow.Unity
{
	public class Styles
	{
		private Color color;
		private Font font;
		private int size;
		private FontStyle style;

		public Color Color
		{
			get { return color; }
			set { color = value; }
		}

		public Font Font
		{
			get { return font; }
			set { font = value; }
		}

		public int Size
		{
			get { return size; }
			set { size = value; }
		}

		public FontStyle Style
		{
			get { return style; }
			set { style = value; }
		}
	}
}
