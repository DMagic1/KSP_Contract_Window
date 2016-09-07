using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionAdd : CW_Popup
	{
		[SerializeField]
		private GameObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

		private CW_Window parent;
		private IContractSection contractInterface;

		public void setMission(IList<IMissionSection> missions, IContractSection contract, CW_Window window)
		{
			if (missions == null || contract == null)
				return;

			if (window == null)
				return;

			parent = window;

			contractInterface = contract;

			CreateMissionSections(missions);
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
			if (parent == null)
				return;

			parent.showCreator(contractInterface);

			DestroyPanel();
		}

		public void DestroyPanel()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
