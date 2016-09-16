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
		private Toggle FontToggle = null;
		[SerializeField]
		private Toggle ScaleToggle = null;
		[SerializeField]
		private Text SliderValue = null;

		bool loaded;

		public void setScalar()
		{
			if (SliderScale == null || FontToggle == null || ScaleToggle == null || SliderValue == null)
				return;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			FontToggle.isOn = CW_Window.Window.Interface.LargeFont;

			ScaleToggle.isOn = CW_Window.Window.Interface.IgnoreScale;

			SliderValue.text = CW_Window.Window.Interface.Scale.ToString("P0");

			SliderScale.value = CW_Window.Window.Interface.Scale * 10;

			FadeIn();

			loaded = true;
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

			CW_Window.Window.UpdateFontSize(CW_Window.Window.gameObject, isOn ? 1 : -1);
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

			if (isOn)
				CW_Window.Window.transform.localScale /= CW_Window.Window.Interface.MasterScale;
			else
				CW_Window.Window.transform.localScale *= CW_Window.Window.Interface.MasterScale;
		}

		public void SliderValueChange(float value)
		{
			if (!loaded)
				return;

			float f = value / 10;

			if (SliderValue != null)
				SliderValue.text = f.ToString("P0");
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

			Vector3 scale = new Vector3(1, 1, 1);

			if (CW_Window.Window.Interface.IgnoreScale)
				scale /= CW_Window.Window.Interface.MasterScale;

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
