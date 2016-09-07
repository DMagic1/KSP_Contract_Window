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

		private IMissionSection missionInterface;
		private IContractSection contractInterface;
		private CW_MissionAdd parent;

		public void setMission(IMissionSection mission, IContractSection contract, CW_MissionAdd p)
		{
			if (mission == null || contract == null)
				return;

			if (MissionTitle == null || MissionNumber == null || Checkmark == null || XMark == null)
				return;

			if (p == null)
				return;

			parent = p;

			missionInterface = mission;

			contractInterface = contract;

			MissionTitle.text = mission.MissionTitle;

			MissionNumber.text = mission.ContractNumber;

			if (!mission.ContractContained(contract))
			{
				Checkmark.gameObject.SetActive(false);

				XMark.gameObject.SetActive(false);
			}
		}

		public void AddContract()
		{
			if (missionInterface == null)
				return;

			missionInterface.AddContract(contractInterface);

			if (parent == null)
				return;

			parent.DestroyPanel();
		}

		public void RemoveContract()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveContract(contractInterface);

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
