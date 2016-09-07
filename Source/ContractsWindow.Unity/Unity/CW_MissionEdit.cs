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
		private CW_Window parent;

		public void setMission(IMissionSection mission, CW_Window p)
		{
			if (mission == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;
		}

		public void ChangeName()
		{
			if (missionInterface == null)
				return;

			if (NewMissionName == null)
				return;

			missionInterface.MissionTitle = NewMissionName.text;

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission();

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
