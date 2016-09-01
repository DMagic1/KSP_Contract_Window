using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSelect : MonoBehaviour
	{
		[SerializeField]
		private GameObject MissionObjectPrefab = null;
		[SerializeField]
		private Transform MissionObjectTransform = null;

		private CW_Window parent;
		private IMissionSelect missionInterface;
		private List<CW_MissionSelectObject> missions = new List<CW_MissionSelectObject>();

		public bool IsVisible
		{
			get
			{
				if (missionInterface == null)
					return false;

				return missionInterface.IsVisible;
			}
		}

		public CW_Window Parent
		{
			get { return parent; }
		}

		private void Update()
		{
			if (missionInterface == null)
				return;

			if (!missionInterface.IsVisible)
				return;

			for (int i = missions.Count - 1; i >= 0; i--)
			{
				CW_MissionSelectObject mission = missions[i];

				if (mission == null)
					continue;

				mission.OnUpdate();
			}
		}

		public void setMission(IMissionSelect mission, CW_Window window)
		{
			if (mission == null)
				return;

			if (window == null)
				return;

			parent = window;

			missionInterface = mission;

			CreateMissionSections(missionInterface.GetMissions());

			missionInterface.SetParent(this);
		}

		private void CreateMissionSections(IList<IMissionSelectObject> missions)
		{
			if (missions == null)
				return;

			if (MissionObjectPrefab == null)
				return;

			if (MissionObjectTransform == null)
				return;

			for (int i = missions.Count - 1; i >= 0; i--)
			{
				IMissionSelectObject mission = missions[i];

				if (mission == null)
					continue;

				CreateMissionSection(mission);
			}			
		}

		private void CreateMissionSection(IMissionSelectObject mission)
		{
			GameObject obj = Instantiate(MissionObjectPrefab);

			if (obj == null)
				return;

			missionInterface.ProcessStyle(obj);

			obj.transform.SetParent(MissionObjectTransform, false);

			CW_MissionSelectObject missionObject = obj.GetComponent<CW_MissionSelectObject>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission, this);

			missions.Add(missionObject);
		}

		public void AddMission(IMissionSelectObject mission)
		{
			if (mission == null)
				return;

			CreateMissionSection(mission);
		}

		public void RemoveMission(CW_MissionSelectObject mission)
		{
			if (mission == null)
				return;

			if (missions.Contains(mission))
				missions.Remove(mission);
		}

		public void DestroyPanel()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

	}
}
