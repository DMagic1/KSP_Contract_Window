using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContractsWindow
{
	public class contractMission
	{
		private string name;
		private string activeString;
		private string hiddenString;
		private Dictionary<Guid, contractUIObject> missionList;
		private List<Guid> activeMissionList;
		private List<Guid> hiddenMissionList;
		private bool ascendingOrder;
		private bool showActiveMissions;
		private sortClass orderMode = sortClass.Difficulty;
		private bool masterMission;

		public string Name
		{
			get { return name; }
		}

		public List<Guid> ActiveMissionList
		{
			get { return activeMissionList; }
			internal set { activeMissionList = value; }
		}

		public List<Guid> HiddenMissionList
		{
			get { return hiddenMissionList; }
			internal set { hiddenMissionList = value; }
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

		public sortClass OrderMode
		{
			get { return orderMode; }
			internal set { orderMode = value; }
		}

		public bool MasterMission
		{
			get { return masterMission; }
			internal set { masterMission = value; }
		}

		internal contractMission(string n, string active, string hidden, bool asc, bool showActive, int sMode, bool Master)
		{
			name = n;
			activeString = active;
			hiddenString = hidden;
			ascendingOrder = asc;
			showActiveMissions = showActive;
			masterMission = Master;
			orderMode = (sortClass)sMode;
			missionList = new Dictionary<Guid, contractUIObject>();
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
		}

		internal contractMission(string n)
		{
			name = n;
			missionList = new Dictionary<Guid, contractUIObject>();
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
		}

		internal contractUIObject getContract(Guid id)
		{
			if (missionList.ContainsKey(id))
				return missionList[id];
			else
				return null;
		}

		internal void buildMissionList()
		{
			resetMasterList();
			buildMissionList(activeString, activeMissionList);
			buildMissionList(hiddenString, hiddenMissionList);
		}

		private void buildMissionList(string s, List<Guid> IDList)
		{
			if (string.IsNullOrEmpty(s))
			{
				IDList = new List<Guid>();
			}
			else
			{
				string[] sA = s.Split(',');
				List<Guid> gID = new List<Guid>();
				for (int i = 0; i < sA.Length; i++)
				{
					contractContainer c = null;
					contractUIObject cUI = null;
					string[] sB = sA[i].Split('|');
					try
					{
						Guid g = new Guid(sB[0]);
						c = contractScenario.Instance.getContract(g);
						if (c != null)
						{
							addToMasterList(c);
							cUI = getContract(g);
							if (cUI == null)
								continue;
							gID.Add(g);
						}
						else
							continue;
					}
					catch (Exception e)
					{
						DMC_MBW.LogFormatted("Guid invalid: {0}", e);
						continue;
					}

					cUI.Order = stringIntParse(sB[1]);
					cUI.ShowParams = stringBoolParse(sB[2]);
				}

				IDList = gID;
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
					idTemp.Add(c.Container.Contract.ContractGuid);
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

		internal void addMission(contractContainer c)
		{
			if (!activeMissionList.Contains(c.Contract.ContractGuid) || !hiddenMissionList.Contains(c.Contract.ContractGuid))
			{
				activeMissionList.Add(c.Contract.ContractGuid);
				addToMasterList(c);
			}
			else
				DMC_MBE.LogFormatted("Mission List Already Contains Contract: {0}", c.Title);
		}

		private void addToMasterList(contractContainer c)
		{
			if (!missionList.ContainsKey(c.Contract.ContractGuid))
				missionList.Add(c.Contract.ContractGuid, new contractUIObject(c));
			else
				DMC_MBE.LogFormatted("Master Mission List For: [{0}] Already Contains Contract: [{1}]", name, c.Title);
		}

		internal void removeMission(contractContainer c)
		{
			foreach(Guid g in activeMissionList)
			{
				if (g == c.Contract.ContractGuid)
					activeMissionList.Remove(g);
			}

			foreach(Guid g in hiddenMissionList)
			{
				if (g == c.Contract.ContractGuid)
					hiddenMissionList.Remove(g);
			}

			removeFromMasterList(c);
		}

		private void removeFromMasterList(contractContainer c)
		{
			if (missionList.ContainsKey(c.Contract.ContractGuid))
				missionList.Remove(c.Contract.ContractGuid);
		}

		private void resetMasterList()
		{
			missionList.Clear();
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
				bool show = c.ShowParams;
				string id = string.Format("{0}|{1}|{2}", source[j], i, show);
				s.Add(id);
			}

			return string.Join(",", s.ToArray());
		}
	}
}
