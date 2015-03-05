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

		internal contractMission(string n, string active, string hidden)
		{
			name = n;
			activeString = active;
			hiddenString = hidden;
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
		}

		internal contractMission(string n)
		{
			name = n;
			activeMissionList = new List<Guid>();
			hiddenMissionList = new List<Guid>();
		}

		internal void addMission(contractContainer c)
		{
			if (!activeMissionList.Contains(c.Contract.ContractGuid) || !hiddenMissionList.Contains(c.Contract.ContractGuid))
			{
				activeMissionList.Add(c.Contract.ContractGuid);
			}
			else
				DMC_MBE.LogFormatted("Mission List Already Contains Contract: {0}", c.Title);
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
		}
	}
}
