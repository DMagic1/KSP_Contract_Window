using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSelectObject : MonoBehaviour
	{
		[SerializeField]
		private Text MissionTitle = null;
		[SerializeField]
		private Text MissionNumber = null;

		private IMissionSection missionInterface;
		private CW_MissionSelect parent;

		public void setMission(IMissionSection mission, CW_MissionSelect p)
		{
			if (mission == null)
				return;

			if (MissionTitle == null || MissionNumber == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;

			MissionTitle.text = mission.MissionTitle;

			MissionNumber.text = mission.ContractNumber;
		}

		public void SetMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetMission();

			if (parent == null)
				return;

			parent.ToggleToContracts();

			parent.DestroyPanel();
		}

	}
}
