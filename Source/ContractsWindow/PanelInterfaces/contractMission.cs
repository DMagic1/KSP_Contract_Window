#region license
/*The MIT License (MIT)
Contract Mission - Object to hold info about a mission list

Copyright (c) 2014 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

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
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ContractParser;
using ContractsWindow.Unity.Unity;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.PanelInterfaces
{
	/// <summary>
	/// A list of contracts; each with its own sort order and separate lists for active and hidden contracts
	/// A special master mission list is used to store all contracts and is used as the source for all other missions
	/// </summary>
	public class contractMission : IMissionSection
	{
		private bool _isVisible;
		private string _missionTitle;
		private string activeString;
		private string hiddenString;
		private string vesselIDString = "";
		private Dictionary<Guid, Vessel> currentVessels;
		private Dictionary<Guid, contractUIObject> contractList;
		private List<Guid> activeMissionList;
		private List<Guid> hiddenMissionList;
		private bool ascendingOrder = true;
		private bool showActiveMissions = true;
		private contractSortClass orderMode = contractSortClass.Difficulty;
		private bool _masterMission;
		private CW_MissionSection UIParent;

		public string ContractNumber
		{
			get { return contractList.Count.ToString(); }			
		}

		public bool IsVisible
		{
			get { return _isVisible; }
			set { _isVisible = value; }
		}

		public bool MasterMission
		{
			get { return _masterMission; }
			set { _masterMission = value; }
		}

		public bool ShowHidden
		{
			get { return !showActiveMissions; }
			set
			{
				showActiveMissions = !value;

				if (contractWindow.Instance == null)
					return;

				contractWindow.Instance.switchLists(value);

				contractWindow.Instance.RefreshContracts();
			}
		}

		public bool DescendingOrder
		{
			get { return !ascendingOrder; }
			set
			{
				ascendingOrder = !value;

				if (contractWindow.Instance == null)
					return;

				contractWindow.Instance.RefreshContracts();
			}
		}
		
		public string MissionTitle
		{
			get
			{
				if (_masterMission)
					return "All Contracts";

				return _missionTitle;
			}
			set
			{
				if (_masterMission)
					return;

				string old = _missionTitle;

				contractScenario.Instance.removeMissionList(old);

				_missionTitle = value;

				if (!contractScenario.Instance.addMissionList(_missionTitle))
					_missionTitle = old;
			}
		}

		public void AddContract(IContractSection contract)
		{
			if (UIParent != null)
				UIParent.AddContract(contract);

			addContract(((contractUIObject)contract).Container, !contract.IsHidden, false);
		}
		
		public bool ContractContained(IContractSection contract)
		{
			return contractList.ContainsKey(contract.ID);
		}
		
		public IList<IContractSection> GetContracts
		{
			get { return new List<IContractSection>(contractList.Values.ToArray()); }
		}

		public void SetSort(int i)
		{
			orderMode = (contractSortClass)i;

			if (contractWindow.Instance == null)
				return;

			contractWindow.Instance.RefreshContracts();
		}
		
		public void RemoveContract(IContractSection contract)
		{
			if (UIParent != null)
				UIParent.RemoveContract(contract.ID);

			removeContract(((contractUIObject)contract).Container);
		}
		
		public void RemoveMission()
		{
			contractScenario.Instance.removeMissionList(_missionTitle);

			contractScenario.Instance.setCurrentMission(contractScenario.Instance.MasterMission.MissionTitle);
		}
		
		public void SetMission()
		{
			contractScenario.Instance.setCurrentMission(_missionTitle);

			contractWindow.Instance.setMission(this);
		}

		public void SetParent(CW_MissionSection m)
		{
			UIParent = m;
		}

		public string Name
		{
			get
			{
				if (_masterMission)
					return "All Contracts";

				return _missionTitle;
			}
			internal set { _missionTitle = value; }
		}

		public string InternalName
		{
			get { return _missionTitle; }
		}

		public string VesselIDs
		{
			get { return vesselIDString; }
		}

		public int ActiveContracts
		{
			get { return contractList.Count; }
		}

		public List<Guid> ActiveMissionList
		{
			get { return activeMissionList; }
		}

		public List<Guid> HiddenMissionList
		{
			get { return hiddenMissionList; }
		}

		public bool AscendingOrder
		{
			get { return ascendingOrder; }
			internal set { ascendingOrder = value; }
		}

		public bool ShowActiveMissions
		{
			get { return showActiveMissions; }
			internal set { showActiveMissions = value; }
		}

		public contractSortClass OrderMode
		{
			get { return orderMode; }
			internal set { orderMode = value; }
		}

		public bool ContainsVessel(Vessel v)
		{
			if (v == null)
				return false;

			return currentVessels.ContainsKey(v.id);
		}

		public void RefreshContract(IContractSection contract)
		{
			if (contract == null)
				return;

			if (UIParent == null)
				return;

			UIParent.RefreshContract(contract);
		}

		internal contractMission(string n, string active, string hidden, string vessels, bool asc, bool showActive, int sMode, bool Master)
		{
			_missionTitle = n;
			activeString = active;
			hiddenString = hidden;
			vesselIDString = vessels;
			ascendingOrder = asc;
			showActiveMissions = showActive;
			_masterMission = Master;
			orderMode = (contractSortClass)sMode;
			contractList = new Dictionary<Guid, contractUIObject>();
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
			currentVessels = new Dictionary<Guid, Vessel>();
		}

		internal contractMission(string n)
		{
			_missionTitle = n;
			contractList = new Dictionary<Guid, contractUIObject>();
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
			currentVessels = new Dictionary<Guid, Vessel>();
		}

		internal contractUIObject getContract(Guid id)
		{
			if (contractList.ContainsKey(id))
			{
				contractUIObject c = contractList[id];
				if (c.Container == null)
					return null;
				else
					return c;
			}
			else
				return null;
		}

		internal bool containsContract(Guid id)
		{
			return contractList.ContainsKey(id);
		}

		internal void buildMissionList()
		{
			resetMasterList();
			clearLists();
			buildMissionList(activeString, true);
			buildMissionList(hiddenString, false);
			buildVesselList(vesselIDString);
		}

		private void buildMissionList(string s, bool Active)
		{
			if (string.IsNullOrEmpty(s))
				return;
			else
			{
				string[] sA = s.Split(',');
				for (int i = 0; i < sA.Length; i++)
				{
					contractContainer c = null;
					contractUIObject cUI = null;
					string[] sB = sA[i].Split('|');
					try
					{
						Guid g = new Guid(sB[0]);
						if (g == null)
							continue;

						c = contractParser.getActiveContract(g);
						if (c == null)
							continue;

						addContract(c, Active, true);
						cUI = getContract(g);
						if (cUI == null)
							continue;

						cUI.SetHidden(!Active);
						cUI.Order = stringIntParse(sB[1]);
						cUI.ShowParams = stringBoolParse(sB[2]);
					}
					catch (Exception e)
					{
						DMC_MBE.LogFormatted("Guid invalid: {0}", e);
						continue;
					}
				}
			}
		}

		internal List<Guid> loadPinnedContracts(List<Guid> gID)
		{
			List<contractUIObject> temp = new List<contractUIObject>();
			List<Guid> idTemp = new List<Guid>();
			foreach (Guid id in gID)
			{
				contractUIObject c = getContract(id);
				if (c != null)
				{
					if (c.Order != null)
						temp.Add(c);
				}
			}
			if (temp.Count > 0)
			{
				temp.Sort((a, b) =>
				{
					return Comparer<int?>.Default.Compare(a.Order, b.Order);
				});
				foreach (contractUIObject c in temp)
				{
					idTemp.Add(c.Container.Root.ContractGuid);
				}
			}
			return idTemp;
		}

		private bool stringBoolParse(string source)
		{
			bool b;
			if (bool.TryParse(source, out b))
				return b;
			return true;
		}

		private int? stringIntParse(string s)
		{
			int i;
			if (int.TryParse(s, out i)) return i;
			return null;
		}

		internal void addContract(contractContainer c, bool active, bool warn, bool addToUI = false)
		{
			if (!activeMissionList.Contains(c.Root.ContractGuid) && !hiddenMissionList.Contains(c.Root.ContractGuid))
			{
				if (addToMasterList(c, addToUI))
				{
					if (active)
						activeMissionList.Add(c.Root.ContractGuid);
					else
						hiddenMissionList.Add(c.Root.ContractGuid);
				}
			}
			else if (warn)
				DMC_MBE.LogFormatted("Mission List Already Contains Contract: {0}", c.Title);
		}

		private bool addToMasterList(contractContainer c, bool add = false)
		{
			if (!contractList.ContainsKey(c.Root.ContractGuid))
			{
				contractUIObject cUI = new contractUIObject(c, this);

				contractList.Add(c.Root.ContractGuid, cUI);

				if (add && UIParent != null)
					UIParent.AddContract(cUI);

				return true;
			}
			else
				DMC_MBE.LogFormatted("Master Mission List For: [{0}] Already Contains Contract: [{1}]", _missionTitle, c.Title);

			return false;
		}

		internal void removeContract(contractContainer c)
		{
			if (contractScenario.ListRemove(activeMissionList, c.Root.ContractGuid) || contractScenario.ListRemove(hiddenMissionList, c.Root.ContractGuid))
				removeFromMasterList(c);
		}

		private void removeFromMasterList(contractContainer c)
		{
			if (contractList.ContainsKey(c.Root.ContractGuid))
				contractList.Remove(c.Root.ContractGuid);
		}

		private void resetMasterList()
		{
			contractList.Clear();
		}

		private void clearLists()
		{
			activeMissionList.Clear();
			hiddenMissionList.Clear();
		}

		private void addToVessels(Vessel v, bool warn = true)
		{
			if (!currentVessels.ContainsKey(v.id))
				currentVessels.Add(v.id, v);
			else if (warn)
				DMC_MBE.LogFormatted("Mission [{0}] Vessel List Already Contains A Vessel With ID: [{1}] And Title [{2}]", _missionTitle, v.id, v.vesselName);
		}

		private void removeFromVessels(Vessel v, bool warn = true)
		{
			if (currentVessels.ContainsKey(v.id))
				currentVessels.Remove(v.id);
			else if (warn)
				DMC_MBE.LogFormatted("Mission [{0}] Vessel List Does Not Contain A Vessel With ID: [{1}] And Title [{2}]", _missionTitle, v.id, v.vesselName);
		}

		internal string stringConcat(List<Guid> source)
		{
			if (source.Count == 0)
				return "";
			List<string> s = new List<string>();
			for (int j = 0; j < source.Count; j++)
			{
				contractUIObject c = getContract(source[j]);
				if (c == null)
					continue;
				string i;
				if (c.Order == null)
					i = "N";
				else
					i = c.Order.ToString();
				string id = string.Format("{0}|{1}|{2}", source[j], i, c.ShowParams);
				s.Add(id);
			}

			return string.Join(",", s.ToArray());
		}

		internal string vesselConcat(contractMission m)
		{
			if (m == null || !HighLogic.LoadedSceneIsFlight)
				return vesselIDString;

			Vessel v = FlightGlobals.ActiveVessel;

			if (v == null)
				return vesselIDString;

			bool withVessel = ContainsVessel(v);
			bool currentMission = m == this;

			if (withVessel && !currentMission)
				removeFromVessels(v, false);
			else if (!withVessel && currentMission)
				addToVessels(v, false);

			List<Vessel> source = currentVessels.Values.ToList();

			return vesselConcat(source);
		}

		private string vesselConcat(List<Vessel> v)
		{
			int i = v.Count;
			if (i <= 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				if (v[j] != null)
					s[j] = v[j].id.ToString();
			}

			return string.Join(",", s);
		}

		private void buildVesselList(string s)
		{
			if (!HighLogic.LoadedSceneIsFlight)
				return;

			if (string.IsNullOrEmpty(s))
				return;

			string[] a = s.Split(',');

			for (int i = 0; i < a.Length; i++)
			{
				try
				{
					Guid g = new Guid(a[i]);
					Vessel v = FlightGlobals.Vessels.FirstOrDefault(V => V.id == g);

					if (v == null)
						continue;

					addToVessels(v);
				}
				catch (Exception e)
				{
					DMC_MBE.LogFormatted("Guid invalid: {0} for mission [{1}]", e, _missionTitle);
				}
			}

			List<Vessel> source = currentVessels.Values.ToList();

			vesselIDString = vesselConcat(source);
		}
	}
}
