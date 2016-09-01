using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionCreate : MonoBehaviour
	{
		[SerializeField]
		private Text NewMission = null;

		private INewMissionPanel missionInterface;
		private CW_Window parent;

		public void setPanel(INewMissionPanel mission, CW_Window p)
		{
			if (mission == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;
		}

		public void CreateMission()
		{
			if (missionInterface == null)
				return;

			if (NewMission == null)
				return;

			missionInterface.NewMission(NewMission.text);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
