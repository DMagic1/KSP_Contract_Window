

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


		internal List<contractContainer> cList, hiddenList = new List<contractContainer>();
		internal int[] order; //0 is descending, 1 is ascending
		internal int[] windowMode; //0 is compact, 1 is expiration display, 2 is full display
		internal int[] showHideList;
		internal int[] contractIDList;
		internal int[] hiddenIDList;
		internal sortClass[] sort;
		internal int[] windowPos;
		internal GameScenes loadedScene = HighLogic.LoadedScene;

		public override void OnLoad(ConfigNode node)
		{
			
		}

		public override void OnSave(ConfigNode node)
		{
			ConfigNode scenes = new ConfigNode("GameScenes");
			foreach (GameScenes s in this.targetScenes)
			{
				ConfigNode scene = new ConfigNode(s.ToString());
				scene.AddValue("DefaultListID", stringConcat(contractIDList, contractIDList.Length));
				scene.AddValue("HiddenListID", stringConcat(hiddenIDList, hiddenIDList.Length));
				scene.AddValue("ShowList", stringConcat(showHideList, showHideList.Length));
				scene.AddValue("WindowMode", stringConcat(windowMode, windowMode.Length));
				scene.AddValue("SortOrder", stringConcat(order, order.Length));
				int[] sortMode = new int[4] { (int)sort[0], (int)sort[1], (int)sort[2], (int)sort[3] };
				scene.AddValue("SortMode", stringConcat(sortMode, sortMode.Length));
				scene.AddValue("WindowPosition", stringConcat(windowPos, windowMode.Length));
				scenes.AddNode(scene);
			}
			node.AddNode(scenes);
		}

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
			string[] s = new string[i];
			for (int j = 0; j < i; j++)
			{
				s[j] = source[j].ToString() + ",";
			}
			s[i - 1].TrimEnd(',');
			return string.Concat(s);
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

	}
}
