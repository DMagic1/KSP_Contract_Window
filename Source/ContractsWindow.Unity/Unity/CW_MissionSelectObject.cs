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

		private IMissionSelectObject missionInterface;
		private CW_MissionSelect parent;

		public void setMission(IMissionSelectObject mission, CW_MissionSelect p)
		{
			if (mission == null)
				return;

			if (MissionTitle == null || MissionNumber == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;

			MissionTitle.text = mission.MissionName;

			MissionNumber.text = mission.ContractNumber;
		}

		private void OnDestroy()
		{
			if (parent == null)
				return;

			parent.RemoveMission(this);

			gameObject.SetActive(false);
		}

		public void OnUpdate()
		{
			if (missionInterface == null)
				return;

			missionInterface.Update();

			MissionTitle.text = missionInterface.MissionName;

			MissionNumber.text = missionInterface.ContractNumber;
		}

		public void SetMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.SetMission(this);

			if (parent == null)
				return;

			parent.DestroyPanel();
		}

	}
}
