#region license
/*The MIT License (MIT)
Contract Scenario - Scenario Module To Store Save Specific Info

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
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using ContractsWindow.Toolbar;

namespace ContractsWindow
{

	#region Scenario Setup

	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
	public class contractScenario : ScenarioModule
	{
		public static contractScenario Instance
		{
			get { return instance; }
		}

		//Use this to reset settings on updates
		[KSPField(isPersistant = true)]
		public string version = "1.0.5.2";

		[KSPField(isPersistant = true)]
		public bool stockToolbar = true;
		[KSPField(isPersistant = true)]
		public bool replaceStockToolbar = false;
		[KSPField(isPersistant = true)]
		public bool replaceStockWarned = false;
		[KSPField(isPersistant = true)]
		public bool toolTips = true;
		[KSPField(isPersistant = true)]
		public bool fontSmall = true;
		[KSPField(isPersistant = true)]
		public int windowSize = 0;

		private static contractScenario instance;

		//Primary contract storage
		private Dictionary<Guid, contractContainer> masterList = new Dictionary<Guid, contractContainer>();

		//Primary mission storage
		private Dictionary<string, contractMission> missionList = new Dictionary<string,contractMission>();

		//A specific contractMission is assigned to hold all contracts; contracts can't be removed from this
		private contractMission masterMission = new contractMission("MasterMission");

		//The currently active mission
		private contractMission currentMission;

		public contractMission MasterMission
		{
			get { return masterMission; }
		}

		//initialize data for each gamescene
		internal bool[] windowVisible = new bool[4];
		internal Rect[] windowRects = new Rect[4] { new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300) };
		private int[] windowPos = new int[16] { 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300 };

		internal contractStockToolbar appLauncherButton = null;
		internal contractToolbar blizzyToolbarButton = null;

		internal contractsWindow cWin;

		//A count of all active contracts as determined by manually checking the Game config node
		private int contractCount;

		public int ContractCount
		{
			get { return contractCount; }
		}

		//Convert all of our saved strings into the appropriate arrays for each game scene
		public override void OnLoad(ConfigNode node)
		{
			instance = this;

			try
			{
				//The first step is manually checking for active contracts from the Game ConfigNode (ie persistent.sfs file); the count of active contracts will be used later when the window is loading
				ConfigNode gameNode = HighLogic.CurrentGame.config;
				if (gameNode != null)
				{
					ConfigNode contractSystemNode = gameNode.GetNodes("SCENARIO").FirstOrDefault(c => c.GetValue("name") == "ContractSystem");
					if (contractSystemNode != null)
					{
						ConfigNode cNode = contractSystemNode.GetNode("CONTRACTS");
						if (cNode != null)
						{
							foreach (ConfigNode C in cNode.GetNodes("CONTRACT"))
							{
								if (C == null)
									continue;

								if (C.HasValue("autoAccept"))
								{
									if (C.GetValue("autoAccept") == "True")
										continue;
								}

								if (C.HasValue("state"))
								{
									if (C.GetValue("state") == "Active")
										contractCount++;
								}
							}
						}
						else
							DMC_MBE.LogFormatted("Contract System Can't Be Checked... Node Invalid");
					}
				}

				//The next step is checking the current version number and comparing it to the saved value;
				//if a newer version number is detected the rest of the load process is skipped;
				//this resets all values for version updates
				//if (version == contractAssembly.Version)
				//{
					ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
					if (scenes != null)
					{
						windowPos = stringSplit(scenes.GetValue("WindowPosition"));
						windowVisible = stringSplitBool(scenes.GetValue("WindowVisible"));
						int[] winPos = new int[4] { windowPos[4 * currentScene(HighLogic.LoadedScene)], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 1], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 2], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 3] };

						//All saved contract missions are loaded here
						//Each mission has a separate contract list
						foreach (ConfigNode m in scenes.GetNodes("Contracts_Window_Mission"))
						{
							if (m == null)
								continue;

							string name;
							string activeString = "";
							string hiddenString = "";
							string vesselString = "";
							bool ascending, showActive;
							int sortMode;
							bool master = false;

							if (m.HasValue("MissionName"))
								name = m.GetValue("MissionName");
							else
								continue;

							if (name == "MasterMission")
								master = true;

							if (m.HasValue("ActiveListID"))
								activeString = m.GetValue("ActiveListID");
							if (m.HasValue("HiddenListID"))
								hiddenString = m.GetValue("HiddenListID");
							if (m.HasValue("VesselIDs"))
								vesselString = m.GetValue("VesselIDs");

							if (!bool.TryParse(m.GetValue("AscendingSort"), out ascending))
								ascending = true;
							if (!bool.TryParse(m.GetValue("ShowActiveList"), out showActive))
								showActive = true;
							if (!int.TryParse(m.GetValue("SortMode"), out sortMode))
								sortMode = 0;

							contractMission mission = new contractMission(name, activeString, hiddenString, vesselString, ascending, showActive, sortMode, master);

							if (master)
							{
								masterMission = mission;
								DMC_MBE.LogFormatted_DebugOnly("Setting Master Mission During Load");
							}

							if (!missionList.ContainsKey(name))
								missionList.Add(name, mission);
						}
						loadWindow(winPos);
					}
				//}

				version = contractAssembly.Version;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Contracts Window Settings Cannot Be Loaded: {0}", e);
			}
		}

		public override void OnSave(ConfigNode node)
		{
			try
			{
				saveWindow(windowRects[currentScene(HighLogic.LoadedScene)]);

				ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");

				//Scene settings
				scenes.AddValue("WindowPosition", stringConcat(windowPos, windowPos.Length));
				scenes.AddValue("WindowVisible", stringConcat(windowVisible, windowVisible.Length));

				foreach (contractMission m in missionList.Values)
				{
					ConfigNode missionNode = new ConfigNode("Contracts_Window_Mission");

					missionNode.AddValue("MissionName", m.Name);
					missionNode.AddValue("ActiveListID", m.stringConcat(m.ActiveMissionList));
					missionNode.AddValue("HiddenListID", m.stringConcat(m.HiddenMissionList));
					missionNode.AddValue("VesselIDs", m.vesselConcat(currentMission));
					missionNode.AddValue("AscendingSort", m.AscendingOrder);
					missionNode.AddValue("ShowActiveList", m.ShowActiveMissions);
					missionNode.AddValue("SortMode", (int)m.OrderMode);

					scenes.AddNode(missionNode);
				}

				node.AddNode(scenes);
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Contracts Window Settings Cannot Be Saved: {0}", e);
			}
		}

		private void Start()
		{
			//Start the window object
			try
			{
				cWin = gameObject.AddComponent<contractsWindow>();
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Contracts Windows Cannot Be Started: {0}", e);
			}

			if (stockToolbar || !ToolbarManager.ToolbarAvailable)
			{
				appLauncherButton = gameObject.AddComponent<contractStockToolbar>();
				if (blizzyToolbarButton != null)
					Destroy(blizzyToolbarButton);
			}
			else if (ToolbarManager.ToolbarAvailable && !stockToolbar)
			{
				blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();
				if (appLauncherButton != null)
					Destroy(appLauncherButton);
			}
		}

		//Remove our contract window object
		private void OnDestroy()
		{
			if (cWin != null)
				Destroy(cWin);
			if (appLauncherButton != null)
				Destroy(appLauncherButton);
			if (blizzyToolbarButton != null)
				Destroy(blizzyToolbarButton);
		}

	#endregion

		#region utilities

		internal static int currentScene(GameScenes s)
		{
			switch (s)
			{
				case GameScenes.FLIGHT:
					return 0;
				case GameScenes.EDITOR:
					return 1;
				case GameScenes.SPACECENTER:
					return 2;
				case GameScenes.TRACKSTATION:
					return 3;
				default:
					return 0;
			}
		}

		//Convert array types into strings for storage
		private string stringConcat(int[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString();
			}
			return string.Join(",", s);
		}

		private string stringConcat(bool[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString();
			}
			return string.Join(",", s);
		}

		//Convert strings into the appropriate arrays
		private int[] stringSplit(string source)
		{
			string[] s = source.Split(',');
			int[] i = new int[s.Length];
			for (int j = 0; j < s.Length; j++)
			{
				i[j] = int.Parse(s[j]);
			}
			return i;
		}

		private bool[] stringSplitBool(string source)
		{
			string[] s = source.Split(',');
			bool[] b = new bool[s.Length];
			for (int j = 0; j < s.Length; j++)
			{
				b[j] = bool.Parse(s[j]);
			}
			return b;
		}

		#endregion

		#region contract Events

		//Used by external assemblies to update parameter values for the UI
		internal void paramChanged(Type t)
		{
			foreach (contractContainer cC in masterList.Values)
			{
				cC.updateParemeterInfo(t);
			}
		}

		//Used by external assemblies to update contract values for the UI
		internal void contractChanged(Type t)
		{
			foreach (contractContainer cC in masterList.Values)
			{
				if (cC.Contract.GetType() == t)
					cC.updateContractInfo();
			}
		}

		#endregion

		#region internal methods

		internal bool addMissionList(string name)
		{
			if (!missionList.ContainsKey(name))
			{
				contractMission mission = new contractMission(name);
				missionList.Add(name, mission);
				return true;
			}
			else
				DMC_MBE.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

			return false;
		}

		internal bool addMissionList(contractMission mission)
		{
			if (!missionList.ContainsKey(mission.Name))
			{
				missionList.Add(mission.Name, mission);
				return true;
			}
			else
				DMC_MBE.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

			return false;
		}

		//Used to add the master contract mission list; usually when something has gone wrong
		internal void addFullMissionList()
		{
			string s = "MasterMission";

			if (missionList.ContainsKey(s))
				removeMissionList(s);

			if (addMissionList(s))
			{
				missionList[s].MasterMission = true;
				addAllContractsToMaster();
				masterMission = missionList[s];
			}
		}

		//Adds all contracts to the master mission
		private void addAllContractsToMaster()
		{
			contractMission Master = missionList.FirstOrDefault(a => a.Value.MasterMission).Value;
			if (Master != null)
			{
				foreach (contractContainer c in masterList.Values)
					Master.addContract(c, true, true);
			}
		}

		internal void removeMissionList(string name, bool delete = true)
		{
			if (missionList.ContainsKey(name))
			{
				if (delete)
				{
					contractMission c = missionList[name];
					c = null;
				}
				missionList.Remove(name);
			}
			else
				DMC_MBE.LogFormatted("No Mission List Of Name: [{0}] Found", name);
		}

		internal void resetMissionsList()
		{
			missionList.Clear();
		}

		internal contractMission getMissionList(string name, bool warn = false)
		{
			if (missionList.ContainsKey(name))
				return missionList[name];
			else if (warn)
				DMC_MBE.LogFormatted("No Mission Of Name [{0}] Found In Primary Mission List", name);

			return null;
		}

		internal contractMission setCurrentMission(string s)
		{
			contractMission m = getMissionList(s, true);

			if (m != null)
			{
				currentMission = m;
				return currentMission;
			}

			currentMission = masterMission;
			return currentMission;
		}

		internal contractMission setLoadedMission(Vessel v)
		{
			if (v == null)
				return masterMission;

			foreach (contractMission m in missionList.Values)
			{
				if (m.ContainsVessel(v))
					return m;
			}

			return masterMission;
		}

		//Returns an ordered list of missions for the main window; the master mission is always first
		internal List<contractMission> getAllMissions()
		{
			List<contractMission> mList = new List<contractMission>();
			List<contractMission> tempList = new List<contractMission>();

			foreach (contractMission m in missionList.Values)
			{
				if (m.MasterMission)
					mList.Add(m);
				else
					tempList.Add(m);
			}

			if (mList.Count == 0)
			{
				if (addMissionList("MasterMission"))
					mList.Add(getMissionList("MasterMission"));
			}

			tempList.Sort((a,b) => RUIutils.SortAscDescPrimarySecondary(false, a.ActiveContracts.CompareTo(b.ActiveContracts), a.Name.CompareTo(b.Name)));

			if (tempList.Count > 0)
				mList.AddRange(tempList);

			return mList;
		}

		internal contractContainer getContract(Guid id)
		{
			if (masterList.ContainsKey(id))
				return masterList[id];
			else
				return null;
		}

		internal void addContract(Guid id, contractContainer c)
		{
			if (!masterList.ContainsKey(id))
				masterList.Add(id, c);
			else
				DMC_MBE.LogFormatted("Error Adding Contract; Already Present In Master List");
		}

		internal void removeContract(Guid id)
		{
			if (masterList.ContainsKey(id))
			{
				contractContainer c = masterList[id];
				masterList.Remove(id);
				c = null;
			}
			else
				DMC_MBE.LogFormatted("Error Removing Contract; Does Not Exist In Master List");
		}

		private void resetList()
		{
			masterList.Clear();
		}

		//Loads all active contracts into the master dictionary
		internal void loadAllContracts()
		{
			resetList();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
					addContract(c.ContractGuid, new contractContainer(c));
			}
		}

		//Initializes all missions that were added during the loading process
		internal void loadAllMissionLists()
		{
			foreach (contractMission m in missionList.Values)
			{
				if (m != null)
				{
					if (m.MasterMission)
					{
						m.buildMissionList();
						foreach (contractContainer c in masterList.Values)
							m.addContract(c, true, false);
						masterMission = m;
					}
					else
						m.buildMissionList();
				}
			}
		}

		internal static string paramTypeCheck(ContractParameter param)
		{
			if (param.GetType() == typeof(PartTest))
				return "partTest";

			if (contractAssembly.FPLoaded)
			{
				if (param.GetType() == contractAssembly._FPType)
					return "FinePrint";
			}

			if (contractAssembly.MCELoaded)
			{
				if (param.GetType() == contractAssembly._MCEType)
					return "MCEScience";
			}

			if (contractAssembly.DMLoaded)
			{
				if (param.GetType() == contractAssembly._DMCType)
					return "DMcollectScience";
			}

			if (contractAssembly.DMALoaded)
			{
				if (param.GetType() == contractAssembly._DMAType)
					return "DManomalyScience";
				else
					return "";
			}

			if (contractAssembly.DMAstLoaded)
			{
				if (param.GetType() == contractAssembly._DMAstType)
					return "DMasteroidScience";
				else
					return "";
			}

			return "";
		}

		internal static string timeInDays(double D)
		{
			if (D <= 0)
				return "----";

			int[] time = KSPUtil.GetDateFromUT((int)D);
			string s = "";

			if (time[4] > 0)
				s = string.Format("{0}y", time[4]);
			if (time[3] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}d", time[3]);
			}
			if (time[4] <= 0 && time[2] > 0)
			{
				if (!string.IsNullOrEmpty(s))
					s += " ";
				s += string.Format("{0}h", time[2]);
			}
			if (time[4] <= 0 && time[3] <= 0 && time[2] <= 0 && time[1] > 0)
				s = string.Format("{0}m", time[1]);

			return s;
		}

		internal static bool ListRemove(List<Guid> list, Guid id)
		{
			if (list.Contains(id))
			{
				list.Remove(id);
				return true;
			}

			return false;
		}

		#endregion

		#region save/load methods

		//Save and load the window rectangle position
		private void saveWindow(Rect source)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowPos[i * 4] = (int)source.x;
			windowPos[(i * 4) + 1] = (int)source.y;
			windowPos[(i * 4) + 2] = (int)source.width - (windowSize * 30);
			windowPos[(i * 4) + 3] = (int)source.height;
		}

		private void loadWindow(int[] window)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowRects[i] = new Rect(window[0], window[1], window[2], window[3]);
			windowRects[i].width += (windowSize * 30);
		}

		#endregion

	}
}
