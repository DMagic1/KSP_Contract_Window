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

using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionSelect : CW_Popup
	{
		[SerializeField]
		private CW_MissionSelectObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

        public delegate void TitleToggle(bool isOn);

        private TitleToggle OnTitleToggle;

		public void setMission(IList<IMissionSection> missions, TitleToggle titleToggle, PopupFade popupFade)
		{
			if (missions == null)
				return;

            OnTitleToggle = titleToggle;
            OnPopupFade = popupFade;

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
            CW_MissionSelectObject missionObject = Instantiate(MissionObjectPrefab, MissionObjectTransform, false);
            
            missionObject.setMission(mission, OnTitleToggle, DestroyPanel);
        }
        
		public void DestroyPanel()
        {
            OnPopupFade.Invoke(this);
        }

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}

	}
}
