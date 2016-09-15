using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionAdd : CW_Popup
	{
		[SerializeField]
		private GameObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

		private IContractSection contractInterface;

		public void setMission(IList<IMissionSection> missions, IContractSection contract)
		{
			if (missions == null || contract == null)
				return;
			
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

			for (int i = missions.Count - 1; i >= 0; i--)
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

			CW_MissionAddObject missionObject = obj.GetComponent<CW_MissionAddObject>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission, contractInterface, this);
		}

		public void CreateNewMission()
		{
			if (CW_Window.Window == null)
				return;
			
			CW_Window.Window.showCreator(contractInterface);

			DestroyPanel();
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
