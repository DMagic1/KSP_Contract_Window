using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
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

		private CW_Window parent;
		bool loaded;

		public void setScalar(CW_Window p)
		{
			if (SliderScale == null || FontToggle == null || ScaleToggle == null || SliderValue == null)
				return;

			if (p == null)
				return;

			if (parent.Interface == null)
				return;

			parent = p;

			FontToggle.isOn = parent.Interface.LargeFont;

			ScaleToggle.isOn = parent.Interface.IgnoreScale;

			SliderValue.text = parent.Interface.Scale.ToString("P0");

			SliderScale.value = parent.Interface.Scale * 10;

			loaded = true;
		}

		public void SetLargeFont(bool isOn)
		{
			if (!loaded)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			parent.Interface.LargeFont = isOn;
		}

		public void IgnoreScale(bool isOn)
		{
			if (!loaded)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			parent.Interface.IgnoreScale = isOn;

			if (isOn)
				parent.transform.localScale /= parent.Interface.MasterScale;
			else
				parent.transform.localScale *= parent.Interface.MasterScale;
		}

		public void SliderValueChange(float value)
		{
			if (!loaded)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			float f = value / 10;

			if (SliderValue != null)
				SliderValue.text = f.ToString("P0");

			parent.Interface.Scale = f;

			parent.transform.localScale *= f;
		}

		public void Close()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
