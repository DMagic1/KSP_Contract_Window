using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionEdit : MonoBehaviour
	{
		[SerializeField]
		private Text NewMissionName = null;

		private IMissionEditPanel missionInterface;
		private CW_Window parent;

		public void setMission(IMissionEditPanel mission, CW_Window p)
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

			missionInterface.ChangeName(NewMissionName.text);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.DeleteMission();

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
