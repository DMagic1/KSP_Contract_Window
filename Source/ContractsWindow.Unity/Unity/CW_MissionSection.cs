using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSection : MonoBehaviour
	{
		[SerializeField]
		private GameObject ContractSectionPrefab = null;
		[SerializeField]
		private Transform ContractSectionTransform = null;

		private string missionTitle;
		private IMissionSection missionInterface;
		private CW_Window window;
		private List<CW_ContractSection> activeContracts = new List<CW_ContractSection>();
		private List<CW_ContractSection> hiddenContracts = new List<CW_ContractSection>();

		private GameObject activeSection;
		private GameObject hiddenSection;

		public bool MasterMission
		{
			get
			{
				if (missionInterface == null)
					return false;

				return missionInterface.MasterMission;
			}
		}

		public void setMission(IMissionSection section, CW_Window parent)
		{
			if (section == null)
				return;

			if (parent == null)
				return;

			missionInterface = section;

			missionTitle = missionInterface.MissionTitle;

			window = parent;

			CreateContractSections(section.GetContracts());
		}

		private void CreateContractSections(IList<IContractSection> contracts)
		{
			if (contracts == null)
				return;

			if (ContractSectionPrefab == null || ContractSectionTransform == null)
				return;

			for (int i = contracts.Count - 1; i >= 0; i--)
			{
				IContractSection contract = contracts[i];

				if (contract == null)
					continue;

				CreateContractSection(contract);
			}
		}

		private void CreateContractSection(IContractSection contract)
		{
			GameObject obj = Instantiate(ContractSectionPrefab);

			if (obj == null)
				return;

			missionInterface.ProcessStyle(obj);

			obj.transform.SetParent(ContractSectionTransform, false);

			CW_ContractSection contractObject = obj.GetComponent<CW_ContractSection>();

			if (contractObject == null)
				return;

			contractObject.setContract(contract, window, this);

			activeContracts.Add(contractObject);

			contractObject.gameObject.SetActive(false);
		}

		public void AddContract(IContractSection contract)
		{
			if (contract == null)
				return;

			CreateContractSection(contract);
		}

		public void RemoveContract(CW_ContractSection contract)
		{
			if (contract == null)
				return;

			if (activeContracts.Contains(contract))
				activeContracts.Remove(contract);
		}

		public void SetMissionVisible(bool isOn)
		{
			if (missionInterface == null)
				return;

			missionInterface.IsVisible = isOn;

			gameObject.SetActive(isOn);
		}

		public string MissionTitle
		{
			get { return missionTitle; }
		}

		public void DestroyChild(GameObject obj)
		{
			if (obj == null)
				return;

			Destroy(obj);
		}
	}
}
