#region license
/*The MIT License (MIT)
CW_MissionSection - Controls the contract mission UI element

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSection : MonoBehaviour
	{
		[SerializeField]
		private Transform ActiveTransform = null;
		[SerializeField]
		private Transform HiddenTransform = null;

        public delegate void ProcessTooltips();
        public delegate void AddContractUI(IContractSection contract);

        private ProcessTooltips OnProcessTooltips;
        private AddContractUI OnAddContract;

        private Transform ContractContainer;

		private string missionTitle;
		private IMissionSection missionInterface;
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

        public void Init(ProcessTooltips processTooltips, AddContractUI addContract, Transform container)
        {
            OnProcessTooltips = processTooltips;
            OnAddContract = addContract;
            ContractContainer = container;
        }

		public void setMission(IMissionSection section, Dictionary<Guid, CW_ContractSection> contracts)
		{
			if (section == null)
				return;

			missionInterface = section;

			missionTitle = missionInterface.MissionTitle;

            AssignContracts(contracts, section.GetContracts);
            
			section.SetParent(this);

			if (section.ShowHidden)
			{
				ActiveTransform.gameObject.SetActive(false);
				HiddenTransform.gameObject.SetActive(true);
			}
		}

        private void AssignContracts(Dictionary<Guid, CW_ContractSection> contracts, IList<IContractSection> missionContracts)
        {
            if (ActiveTransform == null || HiddenTransform == null || contracts == null || missionContracts == null)
                return;

            for (int i = missionContracts.Count - 1; i >= 0; i--)
            {
                IContractSection contract = missionContracts[i];

                if (masterList.ContainsKey(contract.ID))
                    continue;

                if (!contracts.ContainsKey(contract.ID))
                    continue;

                if (contract.IsHidden)
                {
                    contracts[contract.ID].transform.SetParent(HiddenTransform, false);
                    contracts[contract.ID].UpdateContractState(contract);
                    hiddenContracts.Add(contract.ID);
                }
                else
                {
                    contracts[contract.ID].transform.SetParent(ActiveTransform, false);
                    contracts[contract.ID].UpdateContractState(contract);
                    activeContracts.Add(contract.ID);
                }

                contracts[contract.ID].setMissionCallbacks(RemoveContract, SwitchContract);

                masterList.Add(contract.ID, contracts[contract.ID]);
            }
        }
        
        public void ClearContracts()
        {
            for (int i = masterList.Count - 1; i >= 0; i--)
                masterList.ElementAt(i).Value.transform.SetParent(ContractContainer, false);

            masterList.Clear();

            activeContracts.Clear();
            hiddenContracts.Clear();
        }
        
        public void AddContract(CW_ContractSection contract, IContractSection contractInterface)
        {
            if (contract == null)
                return;

            if (missionInterface == null)
                return;

            if (masterList.ContainsKey(contractInterface.ID))
                return;

            if (contractInterface.IsHidden)
            {
                contract.transform.SetParent(HiddenTransform, false);
                hiddenContracts.Add(contractInterface.ID);
            }
            else
            {
                contract.transform.SetParent(ActiveTransform, false);
                activeContracts.Add(contractInterface.ID);
            }

            contract.setMissionCallbacks(RemoveContract, SwitchContract);

            masterList.Add(contractInterface.ID, contract);
        }

        public void AddContract(IContractSection contract)
        {
            if (contract == null)
                return;

            if (missionInterface == null)
                return;

            if (masterList.ContainsKey(contract.ID))
                return;

            OnAddContract.Invoke(contract);
        }

        private CW_ContractSection GetContract(Guid id)
		{
			if (masterList.ContainsKey(id))
				return masterList[id];

			return null;
		}

        public void SwitchContract(Guid id, bool hidden)
		{
			if (id == null)
				return;

			if (!masterList.ContainsKey(id))
				return;

			if (ActiveTransform == null || HiddenTransform == null)
				return;

			if (hidden)
			{
				ListRemove(activeContracts, id);
				hiddenContracts.Add(id);
				masterList[id].transform.SetParent(HiddenTransform);
			}
			else
			{
				ListRemove(hiddenContracts, id);
				activeContracts.Add(id);
				masterList[id].transform.SetParent(ActiveTransform);
			}
		}

		public void SortChildren(List<Guid> sortedList)
		{
			if (missionInterface == null)
				return;

			if (ActiveTransform == null || HiddenTransform == null)
				return;

			int l = sortedList.Count;

			for (int i = l - 1; i >= 0; i--)
			{
				Guid id = sortedList[i];

				CW_ContractSection c = GetContract(id);

				if (c == null)
					continue;

				c.transform.SetParent(null);
			}			

			for (int i = 0; i < l; i++)
			{
				Guid id = sortedList[i];

				CW_ContractSection c = GetContract(id);

				if (c == null)
					continue;

				c.transform.SetParent(c.IsHidden ? HiddenTransform : ActiveTransform);
			}
		}

		public void UpdateChildren()
		{
			foreach (CW_ContractSection contract in masterList.Values)
			{
				if (contract == null)
					continue;

				contract.RefreshContract();
			}
		}

        private IEnumerator UpdateContracts()
        {
            if (missionInterface == null)
                yield break;

            WaitForSeconds wait = new WaitForSeconds(0.1f);

            while(true)
            {
                if (missionInterface.ShowHidden)
                {
                    for (int i = hiddenContracts.Count - 1; i >= 0; i--)
                    {
                        if (masterList.ContainsKey(hiddenContracts[i]))
                            masterList[hiddenContracts[i]].UpdateContract();
                    }
                }
                else
                {
                    for (int i = activeContracts.Count - 1; i >= 0; i--)
                    {
                        if (masterList.ContainsKey(activeContracts[i]))
                            masterList[activeContracts[i]].UpdateContract();
                    }
                }

                yield return wait;
            }
        }
        
        public void RefreshContract(IContractSection contract)
		{
			if (contract == null)
				return;

			CW_ContractSection c = GetContract(contract.ID);

			if (c == null)
				return;

            c.RefreshContract(true);

			c.RefreshParameters();

            OnProcessTooltips.Invoke();
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
				Destroy(masterList[id].gameObject);
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
        
		public void ToggleContracts(bool showHidden)
		{
			if (ActiveTransform == null || HiddenTransform == null)
				return;

			ActiveTransform.gameObject.SetActive(!showHidden);

			HiddenTransform.gameObject.SetActive(showHidden);
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
