#region license
/*The MIT License (MIT)
CW_MissionSelect - Controls the mission selection popup window

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
using System.Linq;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionSelect : CW_Popup
	{
		[SerializeField]
		private GameObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

		public void setMission(IList<IMissionSection> missions)
		{
			if (missions == null)
				return;

			CreateMissionSections(missions);

			FadeIn();
		}

		private void CreateMissionSections(IList<IMissionSection> missions)
		{
			if (missions == null)
				return;

			if (MissionObjectPrefab == null)
				return;

			if (MissionObjectTransform == null)
				return;

			int l = missions.Count;

			for (int i = 0; i < l; i++)
			{
				IMissionSection mission = missions[i];

				if (mission == null)
					continue;

				CreateMissionSection(mission);
			}			
		}

		private void CreateMissionSection(IMissionSection mission)
		{
			GameObject obj = Instantiate(MissionObjectPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(MissionObjectTransform, false);

			CW_MissionSelectObject missionObject = obj.GetComponent<CW_MissionSelectObject>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission, this);
		}

		public void ToggleToContracts()
		{
			if (CW_Window.Window == null)
				return;

			CW_Window.Window.ToggleMainWindow(false);
		}

		public void DestroyPanel()
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
