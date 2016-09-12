using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
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

			CW_Window.Window.DestroyChild(gameObject);
		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission();

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.DestroyChild(gameObject);
		}
	}
}
