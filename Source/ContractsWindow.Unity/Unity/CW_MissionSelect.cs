using System;
using System.Collections.Generic;
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
			print("[CW_UI] Closing Mission Popup");

			gameObject.SetActive(false);

			Destroy(gameObject);
		}

	}
}
