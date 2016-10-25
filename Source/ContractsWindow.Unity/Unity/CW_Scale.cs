#region license
/*The MIT License (MIT)
CW_Scale - Controls the UI scale popup

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
	public class CW_Scale : CW_Popup
	{
		[SerializeField]
		private Slider SliderScale = null;
		[SerializeField]
		private Toggle PixelToggle = null;
		[SerializeField]
		private Toggle FontToggle = null;
		[SerializeField]
		private Toggle ScaleToggle = null;
		[SerializeField]
		private TextHandler SliderValue = null;

		bool loaded;

		public void setScalar()
		{
			if (SliderScale == null || FontToggle == null || ScaleToggle == null || SliderValue == null)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			if (PixelToggle != null)
				PixelToggle.isOn = CW_Window.Window.Interface.PixelPerfect;

			if (FontToggle != null)
				FontToggle.isOn = CW_Window.Window.Interface.LargeFont;

			if (ScaleToggle != null)
				ScaleToggle.isOn = CW_Window.Window.Interface.IgnoreScale;

			if (SliderValue != null)
				SliderValue.OnTextUpdate.Invoke(CW_Window.Window.Interface.Scale.ToString("P0"));

			if (SliderScale != null)
				SliderScale.value = CW_Window.Window.Interface.Scale * 10;

			FadeIn();

			loaded = true;
		}

		public void SetPixelPerfect(bool isOn)
		{
			if (!loaded)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_Window.Window.Interface.PixelPerfect = isOn;
		}

		public void SetLargeFont(bool isOn)
		{
			if (!loaded)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_Window.Window.Interface.LargeFont = isOn;

			ApplyFontSize(isOn ? 1 : -1);
		}

		private void ApplyFontSize(int s)
		{
			var texts = CW_Window.Window.gameObject.GetComponentsInChildren<TextHandler>(true);

			for (int i = texts.Length - 1; i >= 0; i--)
			{
				TextHandler t = texts[i];

				if (t == null)
					continue;

				t.OnFontChange.Invoke(s);
			}
		}

		public void IgnoreScale(bool isOn)
		{
			if (!loaded)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			CW_Window.Window.Interface.IgnoreScale = isOn;

			float f = CW_Window.Window.Interface.Scale;

			Vector3 scale = Vector3.one;

			if (isOn)
				scale /= CW_Window.Window.Interface.MasterScale;
			else
				scale *= CW_Window.Window.Interface.MasterScale;

			CW_Window.Window.transform.localScale = scale * f;
		}

		public void SliderValueChange(float value)
		{
			if (!loaded)
				return;

			float f = value / 10;

			if (SliderValue != null)
				SliderValue.OnTextUpdate.Invoke(f.ToString("P0"));
		}

		public void ApplyScale()
		{
			if (SliderScale == null)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			float f = SliderScale.value / 10;

			CW_Window.Window.Interface.Scale = f;

			Vector3 scale = Vector3.one;

			if (CW_Window.Window.Interface.IgnoreScale)
				scale /= CW_Window.Window.Interface.MasterScale;
			else
				scale *= CW_Window.Window.Interface.MasterScale;

			CW_Window.Window.transform.localScale = scale * f;
		}

		public void Close()
		{
			if (CW_Window.Window == null)
				return;

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
