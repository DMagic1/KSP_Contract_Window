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
		private Slider scaleSlider = null;

		private IScalarPanel scalarInterface;
		private CW_Window parent;

		public void setScalar(IScalarPanel scalar, CW_Window p)
		{
			if (scalar == null)
				return;

			if (p == null)
				return;

			scalarInterface = scalar;

			parent = p;
		}

		public void SetLargeFont(bool isOn)
		{
			if (scalarInterface == null)
				return;

			scalarInterface.LargeFont = isOn;
		}



		public void Close()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
