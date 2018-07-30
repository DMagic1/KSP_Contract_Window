#region license
/*The MIT License (MIT)
CW_MissionAdd - Controls the contract mission contract add popup

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
	public class CW_MissionAdd : CW_Popup
	{
		[SerializeField]
		private CW_MissionAddObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

        public delegate void SpawnMissionCreator(IContractSection contract);

        private SpawnMissionCreator OnSpawnMissionCreator; 

		private IContractSection contractInterface;
        
		public void setMission(IList<IMissionSection> missions, IContractSection contract, SpawnMissionCreator spawnMissionCreator, PopupFade popupFade)
		{
			if (missions == null || contract == null)
				return;

            OnSpawnMissionCreator = spawnMissionCreator;
            OnPopupFade = popupFade;

            contractInterface = contract;

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
            CW_MissionAddObject missionObject = Instantiate(MissionObjectPrefab, MissionObjectTransform, false);
            
			missionObject.setMission(mission, contractInterface, DestroyPanel);
		}

		public void CreateNewMission()
		{
            OnSpawnMissionCreator.Invoke(contractInterface);

            OnPopupFade.Invoke(this);
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
