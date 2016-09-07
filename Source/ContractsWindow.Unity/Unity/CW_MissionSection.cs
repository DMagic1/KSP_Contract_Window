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
		private List<Guid> activeContracts = new List<Guid>();
		private List<Guid> hiddenContracts = new List<Guid>();
		private Dictionary<Guid, CW_ContractSection> masterList = new Dictionary<Guid, CW_ContractSection>();

		public bool MasterMission
		{
			get
			{
				if (missionInterface == null)
					return false;

				return missionInterface.MasterMission;
			}
		}

		public IMissionSection MissionInterface
		{
			get { return missionInterface; }
		}

		public void setMission(IMissionSection section, CW_Window parent)
		{
			if (section == null)
				return;

			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			missionInterface = section;

			missionTitle = missionInterface.MissionTitle;

			window = parent;

			CreateContractSections(section.GetContracts);
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

				if (masterList.ContainsKey(contract.ID))
					continue;

				CreateContractSection(contract);
			}
		}

		private void CreateContractSection(IContractSection contract)
		{
			GameObject obj = Instantiate(ContractSectionPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(ContractSectionTransform, false);

			CW_ContractSection contractObject = obj.GetComponent<CW_ContractSection>();

			if (contractObject == null)
				return;

			contractObject.setContract(contract, window, this);

			if (contract.IsHidden)
				hiddenContracts.Add(contract.ID);
			else
				activeContracts.Add(contract.ID);

			masterList.Add(contract.ID, contractObject);

			contractObject.gameObject.SetActive(false);
		}

		public void AddContract(IContractSection contract)
		{
			if (contract == null)
				return;

			if (masterList.ContainsKey(contract.ID))
				return;

			CreateContractSection(contract);
		}

		public void SwitchContract(Guid id, bool hidden)
		{
			if (id == null)
				return;

			if (!masterList.ContainsKey(id))
				return;

			if (hidden)
			{
				ListRemove(activeContracts, id);
				hiddenContracts.Add(id);
			}
			else
			{
				ListRemove(hiddenContracts, id);
				activeContracts.Add(id);
			}
		}

		public void RemoveContract(Guid id)
		{
			if (id == null)
				return;

			if (ListRemove(activeContracts, id) || ListRemove(hiddenContracts, id))
				RemoveFromMasterList(id);
		}

		private void RemoveFromMasterList(Guid id)
		{
			if (masterList.ContainsKey(id))
			{
				masterList[id].gameObject.SetActive(false);
				Destroy(masterList[id]);
				masterList.Remove(id);
			}
		}

		private bool ListRemove(List<Guid> list, Guid id)
		{
			if (list.Contains(id))
			{
				list.Remove(id);
				return true;
			}

			return false;
		}

		public void SetMissionVisible(bool isOn)
		{
			if (missionInterface == null)
				return;
			
			gameObject.SetActive(isOn);
		}

		public void ToggleContracts(bool showHidden)
		{
			for (int i = activeContracts.Count - 1; i >= 0; i--)
			{
				Guid id = activeContracts[i];

				if (id == null)
					continue; ;

				if (!masterList.ContainsKey(id))
					continue;

				masterList[id].gameObject.SetActive(!showHidden);
			}

			for (int i = hiddenContracts.Count - 1; i >= 0; i--)
			{
				Guid id = hiddenContracts[i];

				if (id == null)
					continue; ;

				if (!masterList.ContainsKey(id))
					continue;

				masterList[id].gameObject.SetActive(showHidden);
			}
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
