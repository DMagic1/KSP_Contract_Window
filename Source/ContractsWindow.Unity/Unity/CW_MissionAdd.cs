using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionAdd : MonoBehaviour
	{
		[SerializeField]
		private GameObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

		private CW_Window parent;
		private IMissionAddPanel missionInterface;
		private List<CW_MissionAddObject> missions = new List<CW_MissionAddObject>();

		public void setMission(IMissionAddPanel mission, CW_Window window)
		{
			if (mission == null)
				return;

			if (window == null)
				return;

			parent = window;

			missionInterface = mission;

			CreateMissionSections(missionInterface.GetMissions());
		}

		private void CreateMissionSections(IList<IMissionAddObject> missions)
		{
			if (missions == null)
				return;

			if (MissionObjectPrefab == null)
				return;

			if (MissionObjectTransform == null)
				return;

			for (int i = missions.Count - 1; i >= 0; i--)
			{
				IMissionAddObject mission = missions[i];

				if (mission == null)
					continue;

				CreateMissionSection(mission);
			}
		}

		private void CreateMissionSection(IMissionAddObject mission)
		{
			GameObject obj = Instantiate(MissionObjectPrefab);

			if (obj == null)
				return;

			missionInterface.ProcessStyle(obj);

			obj.transform.SetParent(MissionObjectTransform, false);

			CW_MissionAddObject missionObject = obj.GetComponent<CW_MissionAddObject>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission, this);

			missions.Add(missionObject);
		}

		public void CreateNewMission()
		{


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
