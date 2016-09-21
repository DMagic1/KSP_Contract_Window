using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity
{
	public abstract class CW_Popup : CanvasFader
	{
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
