using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionAddObject : MonoBehaviour
	{
		[SerializeField]
		private Text MissionTitle = null;
		[SerializeField]
		private Text MissionNumber = null;
		[SerializeField]
		private Image Checkmark = null;
		[SerializeField]
		private Button XMark = null;

		private IMissionAddObject missionInterface;
		private CW_MissionAdd parent;

		public void setMission(IMissionAddObject mission, CW_MissionAdd p)
		{
			if (mission == null)
				return;

			if (MissionTitle == null || MissionNumber == null || Checkmark == null || XMark == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;

			MissionTitle.text = mission.MissionName;

			MissionNumber.text = mission.ContractNumber;

			if (!mission.ContractContained)
			{
				Checkmark.gameObject.SetActive(false);

				XMark.gameObject.SetActive(false);
			}
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

		public void RemoveMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission(this);

			if (Checkmark != null && XMark != null)
			{
				Checkmark.gameObject.SetActive(false);

				XMark.gameObject.SetActive(false);
			}

			if (MissionNumber != null)
				MissionNumber.text = missionInterface.ContractNumber;
		}


	}
}
