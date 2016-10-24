#region license
/*The MIT License (MIT)
CW_MissionEdit - Controls the contract mission editor popup

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
	public class CW_MissionEdit : CW_Popup
	{
		[SerializeField]
		private Text NewMissionName = null;

		private IMissionSection missionInterface;

		public void setMission(IMissionSection mission)
		{
			if (mission == null)
				return;

			missionInterface = mission;

			FadeIn();
		}

		public void ChangeName()
		{
			if (missionInterface == null)
				return;

			if (NewMissionName == null)
				return;

			missionInterface.MissionTitle = NewMissionName.text;

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.FadePopup(this);
		}

		public void TextUpdate()
		{

		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission();

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
