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
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionEdit : CW_Popup
	{
		[SerializeField]
		private InputHandler m_MissionInput = null;

		private IMissionSection missionInterface;

		private void Update()
		{
			if (CW_Window.Window == null || CW_Window.Window.Interface == null)
				return;

			if (CW_Window.Window.Interface.LockInput)
			{
				if (m_MissionInput != null && !m_MissionInput.IsFocused)
					CW_Window.Window.Interface.LockInput = false;
			}
		}
	
		public void setMission(IMissionSection mission)
		{
			if (mission == null)
				return;

			missionInterface = mission;

			FadeIn();
		}

		public void OnInputClick(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData) || CW_Window.Window == null || CW_Window.Window.Interface == null)
				return;

			if (((PointerEventData)eventData).button != PointerEventData.InputButton.Left)
				return;

			CW_Window.Window.Interface.LockInput = true;
		}

		public void ChangeName()
		{
			if (missionInterface == null)
				return;

			if (m_MissionInput == null)
				return;

			missionInterface.MissionTitle = m_MissionInput.Text;

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface != null)
				CW_Window.Window.Interface.LockInput = false;

			CW_Window.Window.FadePopup(this);
		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission();

			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Interface != null)
				CW_Window.Window.Interface.LockInput = false;

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
