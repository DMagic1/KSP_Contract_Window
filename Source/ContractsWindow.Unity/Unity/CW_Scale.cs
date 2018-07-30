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

        public delegate void PixelPerfectToggle(bool isOn);
        public delegate void LargeFontToggle(bool isOn);
        public delegate void IgnoreScaleToggle(bool isOn);
        public delegate void ScaleChange(float scale);

        private PixelPerfectToggle OnPixelPerfectToggle;
        private LargeFontToggle OnLargeFontToggle;
        private IgnoreScaleToggle OnIgnoreScaleToggle;
        private ScaleChange OnScaleChange;

        bool loaded;

		public void setScalar(bool pixelPerfect, bool largeFont, bool ignoreScale, float scale
            , PixelPerfectToggle pixelPerfectToggle, LargeFontToggle largeFontToggle, IgnoreScaleToggle ignoreScaleToggle, ScaleChange scaleChange, PopupFade popupFade)
		{
			if (SliderScale == null || FontToggle == null || ScaleToggle == null || SliderValue == null || PixelToggle == null)
				return;

            OnPixelPerfectToggle = pixelPerfectToggle;
            OnLargeFontToggle = largeFontToggle;
            OnIgnoreScaleToggle = ignoreScaleToggle;
            OnScaleChange = scaleChange;
            OnPopupFade = popupFade;

			PixelToggle.isOn = pixelPerfect;

			FontToggle.isOn = largeFont;

			ScaleToggle.isOn = ignoreScale;

			SliderValue.OnTextUpdate.Invoke(scale.ToString("P0"));

			SliderScale.value = scale * 10;

			FadeIn();

			loaded = true;
		}

		public void SetPixelPerfect(bool isOn)
		{
			if (!loaded)
				return;

            OnPixelPerfectToggle.Invoke(isOn);
		}

		public void SetLargeFont(bool isOn)
		{
			if (!loaded)
				return;

            OnLargeFontToggle.Invoke(isOn);
		}
        
		public void IgnoreScale(bool isOn)
		{
			if (!loaded)
				return;

            OnIgnoreScaleToggle.Invoke(isOn);
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

			float f = SliderScale.value / 10;

            OnScaleChange.Invoke(f);
		}

		public void Close()
		{
            OnPopupFade(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
