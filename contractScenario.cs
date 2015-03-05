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
		internal static contractScenario Instance
		{
			get
			{
				Game g = HighLogic.CurrentGame;
				try
				{
					var mod = g.scenarios.FirstOrDefault(m => m.moduleName == typeof(contractScenario).Name);
					if (mod != null)
						return (contractScenario)mod.moduleRef;
					else
						return null;
				}
				catch(Exception e)
				{
					print(string.Format("[Contracts+] Could not find Contracts Window Scenario Module: {0}", e));
					return null;
				}
			}
			private set { }
		}

		//Use this to reset settings on updates
		[KSPField(isPersistant = true)]
		public string version = "1.0.3.3";

		[KSPField(isPersistant = true)]
		public bool stockToolbar = true;
		[KSPField(isPersistant = true)]
		public bool replaceStockToolbar = false;
		[KSPField(isPersistant = true)]
		public bool toolTips = true;
		[KSPField(isPersistant = true)]
		public bool fontSmall = true;
		[KSPField(isPersistant = true)]
		public int windowSize = 0;

		//Master contract storage
		private Dictionary<Guid, contractContainer> masterList = new Dictionary<Guid, contractContainer>();

		private Dictionary<string, contractMission> missionList = new Dictionary<string,contractMission>();

		private contractMission masterMission;

		//initialize data for each gamescene
		internal int[] windowMode = new int[4];
		internal bool[] windowVisible = new bool[4];
		internal Rect[] windowRects = new Rect[4] { new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300), new Rect(50, 80, 250, 300) };
		private int[] windowPos = new int[16] { 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300 };

		//Contract config event
		internal static EventData<float[]> onContractChange;
		internal static EventData<float[]> onParamChange;

		internal contractStockToolbar appLauncherButton = null;
		internal contractToolbar blizzyToolbarButton = null;

		internal contractsWindow cWin;

		//Convert all of our saved strings into the appropriate arrays for each game scene
		public override void OnLoad(ConfigNode node)
		{
			try
			{
				if (version == contractAssembly.Version)
				{
					ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
					if (scenes != null)
					{
						//Scene settings
						windowMode = stringSplit(scenes.GetValue("WindowMode"));
						windowPos = stringSplit(scenes.GetValue("WindowPosition"));
						windowVisible = stringSplitBool(scenes.GetValue("WindowVisible"));
						int[] winPos = new int[4] { windowPos[4 * currentScene(HighLogic.LoadedScene)], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 1], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 2], windowPos[(4 * currentScene(HighLogic.LoadedScene)) + 3] };

						foreach (ConfigNode m in scenes.GetNodes("Contracts_Window_Mission"))
						{
							if (m != null)
							{
								string name;
								string activeString = "";
								string hiddenString = "";
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

								if (!bool.TryParse(m.GetValue("AscendingSort"), out ascending))
									ascending = true;
								if (!bool.TryParse(m.GetValue("ShowActiveList"), out showActive))
									showActive = true;
								if (!int.TryParse(m.GetValue("SortMode"), out sortMode))
									sortMode = 0;

								contractMission mission = new contractMission(name, activeString, hiddenString, ascending, showActive, sortMode, master);
								masterMission = mission;

								if (!missionList.ContainsKey(name))
									missionList.Add(name, mission);
							}
						}
						loadWindow(winPos);
					}
				}

				version = contractAssembly.Version;

			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Contracts Window Settings Cannot Be Loaded: {0}", e);
			}

			//Start the window object
			try
			{
				cWin = gameObject.AddComponent<contractsWindow>();
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Contracts Windows Cannot Be Started: {0}", e);
			}
		}

		public override void OnSave(ConfigNode node)
		{
			try
			{
				saveWindow(windowRects[currentScene(HighLogic.LoadedScene)]);

				ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");

				//Scene settings
				scenes.AddValue("WindowMode", stringConcat(windowMode, windowMode.Length));
				scenes.AddValue("WindowPosition", stringConcat(windowPos, windowPos.Length));
				scenes.AddValue("WindowVisible", stringConcat(windowVisible, windowVisible.Length));

				foreach (contractMission m in missionList.Values)
				{
					ConfigNode missionNode = new ConfigNode("Contracts_Window_Mission");

					missionNode.AddValue("MissionName", m.Name);
					missionNode.AddValue("ActiveListID", m.stringConcat(m.ActiveMissionList));
					missionNode.AddValue("HiddenListID", m.stringConcat(m.HiddenMissionList));
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
			if (onContractChange == null)
				onContractChange = new EventData<float[]>("onContractChange");
			if (onParamChange == null)
				onParamChange = new EventData<float[]>("onParamChange");
			onContractChange.Add(contractChanged);
			onParamChange.Add(paramChanged);

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
			Destroy(cWin);
			onContractChange.Remove(contractChanged);
			onParamChange.Remove(paramChanged);

			if (appLauncherButton != null)
				Destroy(appLauncherButton);
			if (blizzyToolbarButton != null)
				Destroy(blizzyToolbarButton);
		}

		public Dictionary<string, contractMission> MissionList
		{
			get { return missionList; }
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

		private int stringintParse(string s)
		{
			int i;
			if (int.TryParse(s, out i)) return i;
			return 0;
		}

		private float stringFloatParse(string s, bool b)
		{
			float f;
			if (float.TryParse(s, out f)) return f;
			if (b)
				return 1;
			else
				return 10;
		}

		#endregion

		#region contract Events

		private void paramChanged(float[] originals)
		{
			for (int j = 0; j < masterList.Count; j++)
			{
				masterList.ElementAt(j).Value.updateParameterInfo();
			}
		}

		private void contractChanged(float[] originals)
		{
			for (int j = 0; j < masterList.Count; j++)
			{
				masterList.ElementAt(j).Value.updateContractInfo();
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

		internal void addFullMissionList()
		{
			string s = "MasterMission";
			if (missionList.ContainsKey(s))
				removeMissionList(s);
			contractMission mission = new contractMission(s);
			mission.MasterMission = true;
			missionList.Add(name, mission);
		}

		internal void removeMissionList(string name)
		{
			if (missionList.ContainsKey(name))
				missionList.Remove(name);
			else
				DMC_MBE.LogFormatted("No Mission List Of Name: [{0}] Found", name);
		}

		internal void resetMissionsList()
		{
			missionList.Clear();
		}

		internal contractMission getMissionList(string name)
		{
			if (missionList.ContainsKey(name))
				return missionList[name];
			else
				return null;
		}

		//Populate the contract lists based on contract Guid values
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

		internal void resetList()
		{
			masterList.Clear();
		}

		internal void loadAllContracts()
		{
			resetList();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
					addContract(c.ContractGuid, new contractContainer(c));
			}
		}

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
							m.addMission(c);
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

		#endregion

		#region save/load methods

		//Save and load the window rectangle position
		private void saveWindow(Rect source)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowPos[i * 4] = (int)source.x;
			windowPos[(i * 4) + 1] = (int)source.y;
			if (windowMode[i] == 0)
				windowPos[(i * 4) + 2] = (int)source.width - (windowSize * 30);
			else
				windowPos[(i * 4) + 2] = (int)source.width - (windowSize * 150);
			windowPos[(i * 4) + 3] = (int)source.height;
		}

		private void loadWindow(int[] window)
		{
			int i = currentScene(HighLogic.LoadedScene);
			windowRects[i] = new Rect(window[0], window[1], window[2], window[3]);
			if (windowMode[i] == 0)
				windowRects[i].width += (windowSize * 30);
			else
				windowRects[i].width += (windowSize * 150);
		}

		#endregion

	}
}
