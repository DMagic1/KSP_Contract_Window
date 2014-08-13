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
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using Contracts;

namespace ContractsWindow
{

	[KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)] 
	class contractScenario: ScenarioModule
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

		internal List<contractContainer> showList = new List<contractContainer>();
		internal List<contractContainer> hiddenList = new List<contractContainer>();

		//initialize data for each gamescene
		private int[] orderMode = new int[4];
		private int[] windowMode = new int[4];
		private int[] showHideMode = new int[4];
		private List<long> showIDList = new List<long>();
		private List<long> hiddenIDList = new List<long>();
		private bool[] windowVisible = new bool[4];
		private bool[] toolTips = new bool[4];
		private sortClass[] sortMode = new sortClass[4] { sortClass.Difficulty, sortClass.Difficulty, sortClass.Difficulty, sortClass.Difficulty };
		private int[] windowPos = new int[16] { 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300, 50, 80, 250, 300 };

		internal contractsWindow cWin;

		public override void OnLoad(ConfigNode node)
		{
			ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
			if (scenes != null)
			{
				showIDList = stringSplitLong(scenes.GetValue("DefaultListID"));
				hiddenIDList = stringSplitLong(scenes.GetValue("HiddenListID"));
				showHideMode = stringSplit(scenes.GetValue("ShowListMode"));
				windowMode = stringSplit(scenes.GetValue("WindowMode"));
				orderMode = stringSplit(scenes.GetValue("SortOrder"));
				int[] sort = stringSplit(scenes.GetValue("SortMode"));
				sortMode = new sortClass[4] { (sortClass)sort[0], (sortClass)sort[1], (sortClass)sort[2], (sortClass)sort[3] };
				windowPos = stringSplit(scenes.GetValue("WindowPosition"));
				windowVisible = stringSplitBool(scenes.GetValue("WindowVisible"));
				toolTips = stringSplitBool(scenes.GetValue("ToolTips"));

				if (showIDList.Count > 0)
					foreach (long l in showIDList)
						addToList(l, showList);
				if (hiddenIDList.Count > 0)
					foreach (long l in hiddenIDList)
						addToList(l, hiddenList);

				cWin = gameObject.AddComponent<contractsWindow>();
			}
		}

		public override void OnSave(ConfigNode node)
		{
			long[] showListID = contractID(showList);
			long[] hiddenListID = contractID(hiddenList);

			ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");
			scenes.AddValue("DefaultListID", stringConcat(showListID, showListID.Length));
			scenes.AddValue("HiddenListID", stringConcat(hiddenListID, hiddenListID.Length));
			scenes.AddValue("ShowListMode", stringConcat(showHideMode, showHideMode.Length));
			scenes.AddValue("WindowMode", stringConcat(windowMode, windowMode.Length));
			scenes.AddValue("SortOrder", stringConcat(orderMode, orderMode.Length));
			int[] sort = new int[4] { (int)sortMode[0], (int)sortMode[1], (int)sortMode[2], (int)sortMode[3] };
			scenes.AddValue("SortMode", stringConcat(sort, sort.Length));
			scenes.AddValue("WindowPosition", stringConcat(windowPos, windowPos.Length));
			scenes.AddValue("WindowVisible", stringConcat(windowVisible, windowVisible.Length));
			scenes.AddValue("ToolTips", stringConcat(toolTips, toolTips.Length));
			node.AddNode(scenes);
		}

		#region utilities

		private int currentScene(GameScenes s)
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
					return 3;
			}
		}

		private string stringConcat(int[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString() + ",";
			}
			return string.Concat(s).TrimEnd(',');
		}

		private string stringConcat(long[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString() + ",";
			}
			return string.Concat(s).TrimEnd(',');
		}

		private string stringConcat(bool[] source, int i)
		{
			if (i == 0)
				return "";
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString() + ",";
			}
			return string.Concat(s).TrimEnd(',');
		}

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

		private List<long> stringSplitLong(string source)
		{
			if (source == "")
				return new List<long>();
			string[] s = source.Split(',');
			List<long> i = new List<long>();
			for (int j = 0; j < s.Length; j++)
			{
				i.Add(long.Parse(s[j]));
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

		private long[] contractID(List<contractContainer> c)
		{
			long[] l = new long[c.Count];
			for (int j = 0; j < c.Count; j++)
				l[j] = c[j].contract.ContractID;
			return l;
		}

		private void addToList(long ID, List<contractContainer> cL)
		{
			Contract c = ContractSystem.Instance.Contracts.FirstOrDefault(n => n.ContractID == ID);
			if (c != null)
				cL.Add(new contractContainer(c));
		}

		#endregion

		#region save/load methods

		//Saving and loading info from the window
		internal void setOrderMode(int i)
		{
			orderMode[currentScene(HighLogic.LoadedScene)] = i;
		}

		internal void setWindowMode(int i)
		{
			windowMode[currentScene(HighLogic.LoadedScene)] = i;
		}

		internal void setShowHideMode(int i)
		{
			showHideMode[currentScene(HighLogic.LoadedScene)] = i;
		}

		internal void setSortMode(sortClass sC)
		{
			sortMode[currentScene(HighLogic.LoadedScene)] = sC;
		}

		internal void setWindowPosition(int x, int y, int w, int h)
		{
			int s = currentScene(HighLogic.LoadedScene) * 4;
			windowPos[s] = x;
			windowPos[s + 1] = y;
			windowPos[s + 2] = w;
			windowPos[s + 3] = h;
		}

		internal void setWindowVisible(bool b)
		{
			windowVisible[currentScene(HighLogic.LoadedScene)] = b;
		}

		internal void setToolTips(bool b)
		{
			toolTips[currentScene(HighLogic.LoadedScene)] = b;
		}

		internal int loadOrderMode()
		{
			return orderMode[currentScene(HighLogic.LoadedScene)];
		}

		internal int loadWindowMode()
		{
			return windowMode[currentScene(HighLogic.LoadedScene)];
		}

		internal int loadShowHideMode()
		{
			return showHideMode[currentScene(HighLogic.LoadedScene)];
		}

		internal sortClass loadSortMode()
		{
			return sortMode[currentScene(HighLogic.LoadedScene)];
		}

		internal int[] loadWindowPosition()
		{
			int i = currentScene(HighLogic.LoadedScene) * 4;
			return new int[4] { windowPos[i], windowPos[i + 1], windowPos[i + 2], windowPos[i + 3] };
		}

		internal bool loadWindowVisible()
		{
			return windowVisible[currentScene(HighLogic.LoadedScene)];
		}

		internal bool loadToolTips()
		{
			return toolTips[currentScene(HighLogic.LoadedScene)];
		}

		#endregion

	}
}
