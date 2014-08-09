#region license
/*The MIT License (MIT)
Contract Window - Addon to control window for contracts

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
using System.Reflection;

using Contracts;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddonImproved(KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TimeElapses, false)]
	class contractsWindow: MonoBehaviourWindow
	{

		#region Initialization

		internal static bool IsVisible;
		private List<contractContainer> cList = new List<contractContainer>();
		private string version, assemblyLocation, assemblyFolder, fileLocation;
		private Assembly assembly;
		private Vector2 scroll;
		private bool resizing, visible, fileFound, showSort;
		private float dragStart, windowHeight, windowX, windowY, windowW, windowH, windowMaxY;
		private sortClass sort;
		private int order; //0 is descending, 1 is ascending
		private int windowMode; //0 is compact, 1 is expiration display, 2 is full display
		private Rect dropDownSort;
		
		internal override void Awake()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
				assemblyLocation = assembly.Location.Replace("\\", "/");
				version = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
				assemblyFolder = System.IO.Path.GetDirectoryName(assemblyLocation).Replace("\\", "/");
				fileLocation = assemblyFolder + "/WindowSettings.cfg";
				if (System.IO.File.Exists(fileLocation))
					fileFound = true;

				sort = sortClass.Default;
				order = 0;
				WindowCaption = string.Format("Contracts {0}", version);
				WindowRect = new Rect(40, 80, 250, 300);
				WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
				Visible = true;
				DragEnabled = true;

				dropDownSort = new Rect(WindowRect.x - 26, WindowRect.y - 15, 100, 145);

				PersistenceLoad();

				DragRect = new Rect(0, 0, WindowRect.width - 20, WindowRect.height - 25);
				PersistenceSave();

				InputLockManager.RemoveControlLock(_AssemblyName.GetHashCode().ToString());

				SkinsLibrary.SetCurrent("UnitySkin");
			}
		}

		internal override void Start()
		{
			GameEvents.Contract.onAccepted.Add(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Add(contractLoaded);
		}

		internal override void OnDestroy()
		{
			EditorLogic.fetch.Unlock(_AssemblyName.GetHashCode().ToString());
			GameEvents.Contract.onAccepted.Remove(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Remove(contractLoaded);
			PersistenceSave();
		}

		internal override void Update()
		{
		}

		#endregion

		#region GUI Draw

		internal override void DrawWindow(int id)
		{
			GUILayout.BeginVertical();
			GUILayout.Label(string.Format("Active Contracts: {0}", cList.Count));

			//Menu Bar
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Sort", contractSkins.sortMenu, GUILayout.MaxWidth(50)))
				showSort = !showSort;
			GUILayout.Space(5);
			if (order == 0)
			{
				if (GUILayout.Button("Asc.", contractSkins.sortOrder, GUILayout.MaxWidth(45)))
				{
					order = 1;
					cList = sortList(cList, sort, order);
				}
			}
			if (order == 1)
			{
				if (GUILayout.Button("Desc.", contractSkins.sortOrder, GUILayout.MaxWidth(45)))
				{
					order = 0;
					cList = sortList(cList, sort, order);
				}
			}
			GUILayout.Space(90);
			if (windowMode == 0)
			{
				if (GUILayout.Button("+", contractSkins.contractActive, GUILayout.MaxWidth(20)))
				{
					windowMode = 1;
					WindowRect.width = 325;
					DragRect.width = WindowRect.width - 20;
				}
			}
			if (windowMode == 1)
			{
				GUILayout.Space(45);
				if (GUILayout.Button("-", contractSkins.contractActive, GUILayout.MaxWidth(20)))
				{
					windowMode = 0;
					WindowRect.width = 250;
					DragRect.width = WindowRect.width - 20;
				}
				GUILayout.Space(5);
				if (GUILayout.Button("+", contractSkins.contractActive, GUILayout.MaxWidth(20)))
				{
					windowMode = 2;
					WindowRect.width = 400;
					DragRect.width = WindowRect.width - 20;
				}
			}
			if (windowMode == 2)
			{
				GUILayout.Space(150);
				if (GUILayout.Button("-", contractSkins.contractActive, GUILayout.MaxWidth(20)))
				{
					windowMode = 1;
					WindowRect.width = 325;
					DragRect.width = WindowRect.width - 20;
				}
			}
			GUILayout.EndHorizontal();

			//Contract List Begins
			scroll = GUILayout.BeginScrollView(scroll);
			foreach (contractContainer c in cList)
			{
				GUILayout.BeginHorizontal();
				if (!showSort)
				{
					if (GUILayout.Button(c.contract.Title, titleState(c.contract.ContractState), GUILayout.MaxWidth(210)))
						c.showParams = !c.showParams;
				}
				else
					GUILayout.Box(c.contract.Title, titleState(c.contract.ContractState), GUILayout.MaxWidth(210));
				if (windowMode == 1)
				{
					GUILayout.Space(3);
					if (c.deadline >= 21600)
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.MaxWidth(50));
					else
						GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.MaxWidth(50));
				}
				if (windowMode == 2)
				{
					GUILayout.Space(3);
					if (c.deadline >= 21600)
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.MaxWidth(50));
					else
						GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.MaxWidth(50));
					GUILayout.Space(10);
					GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxWidth(13));
					GUILayout.Space(-5);
					GUILayout.Label("+ " + c.contract.FundsCompletion.ToString("N1"), contractSkins.fundsReward, GUILayout.MaxWidth(70));
				}

				GUILayout.EndHorizontal();

				if (c.showParams)
				{
					//Contract Parameter list for each contract
					GUILayout.BeginVertical();
					foreach (parameterContainer cP in c.paramList)
					{
						if (!string.IsNullOrEmpty(cP.cParam.Title))
						{
							//Check if each parameter has notes associated with it
							if (cP.cParam.State != ParameterState.Complete && !string.IsNullOrEmpty(cP.cParam.Notes))
							{
								GUILayout.BeginHorizontal();
								if (GUILayout.Button("[+]", contractSkins.noteButton, GUILayout.MaxWidth(26)))
									cP.showNote = !cP.showNote;
								GUILayout.Space(5);
								GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(226));
								if (windowMode == 2)
								{
									if (cP.cParam.FundsCompletion > 0)
									{
										GUILayout.Space(5);
										GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxWidth(13));
										GUILayout.Space(-5);
										GUILayout.Label("+" + cP.cParam.FundsCompletion.ToString("N1"), contractSkins.fundsReward, GUILayout.MaxWidth(60));
									}
								}
								GUILayout.EndHorizontal();
								if (cP.showNote)
								{
									GUILayout.Space(3);
									GUILayout.Box(cP.cParam.Notes, contractSkins.noteText, GUILayout.MaxWidth(261));
								}
							}
							//If no notes are present just display the title
							else
							{
							GUILayout.BeginHorizontal();
							GUILayout.Space(15);
							GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(250));
							if (windowMode == 2)
							{
								if (cP.cParam.FundsCompletion > 0)
								{
									GUILayout.Space(5);
									GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxWidth(13));
									GUILayout.Space(-5);
									GUILayout.Label("+" + cP.cParam.FundsCompletion.ToString("N1"), contractSkins.fundsReward, GUILayout.MaxWidth(60));
								}
							}
							GUILayout.EndHorizontal();
							}
						}
					}
					GUILayout.EndVertical();
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(15);
			GUILayout.EndVertical();

			if (showSort)
			{
				GUILayout.BeginVertical();
				GUI.Box(new Rect(dropDownSort.x, dropDownSort.y, dropDownSort.width, 113), "", contractSkins.dropDown);
				if (GUI.Button(new Rect(dropDownSort.x, dropDownSort.y+2, dropDownSort.width, 25), "Expiration", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Expiration;
					cList = sortList(cList, sort, order);
				}
				if (GUI.Button(new Rect(dropDownSort.x, dropDownSort.y + 30, dropDownSort.width, 25) , "Acceptance", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Acceptance;
					cList = sortList(cList, sort, order);
				}
				if (GUI.Button(new Rect(dropDownSort.x, dropDownSort.y + 58, dropDownSort.width, 25), "Difficulty", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Difficulty;
					cList = sortList(cList, sort, order);
				}
				if (GUI.Button(new Rect(dropDownSort.x, dropDownSort.y + 86, dropDownSort.width, 25), "Reward", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Reward;
					cList = sortList(cList, sort, order);
				}

				GUILayout.EndVertical();
			}
		}

		internal override void DrawGUI()
		{
			//Bool toggled by Toolbar icon
			Toggle = IsVisible;

			//Update the drag rectangle
			DragRect.height = WindowRect.height - 24;

			//Draw the window
			base.DrawGUI();

			//Draw the resizer in rectangle
			if (Toggle)
			{
				Rect resizer = new Rect(WindowRect.x + WindowRect.width - 25, WindowRect.y + WindowRect.height - 25, 22, 22);
				GUI.Box(resizer, contractSkins.expandIcon);

				//Resize window when the resizer is grabbed by the mouse
				if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
				{
					if (resizer.Contains(Event.current.mousePosition))
					{
						resizing = true;
						dragStart = Input.mousePosition.y;
						windowHeight = WindowRect.yMax;
						Event.current.Use();
					}
				}
				if (resizing)
				{
					if (Input.GetMouseButtonUp(0))
					{
						resizing = false;
						WindowRect.yMax = windowHeight;
						PersistenceSave();
					}
					else
					{
						//Only consider y direction of mouse input
						float height = Input.mousePosition.y;
						if (Input.mousePosition.y < 0)
							height = 0;
						windowHeight -= height - dragStart;
						dragStart = height;
						WindowRect.yMax = windowHeight;
						if (WindowRect.yMax > Screen.height)
							WindowRect.yMax = Screen.height;
					}
				}

				// Hide part rightclick menu.
				if (HighLogic.LoadedSceneIsFlight)
				{
					if (WindowRect.Contains(Input.mousePosition) && GUIUtility.hotControl == 0 && Input.GetMouseButton(0))
					{
						foreach (var window in GameObject.FindObjectsOfType(typeof(UIPartActionWindow)).OfType<UIPartActionWindow>().Where(p => p.Display == UIPartActionWindow.DisplayType.Selected))
						{
							window.enabled = false;
							window.displayDirty = true;
						}
					}
				}

				if (HighLogic.LoadedSceneIsEditor)
				{
					if (WindowRect.Contains(Input.mousePosition))
					{
						EditorTooltip.Instance.HideToolTip();
						EditorLogic.fetch.Lock(false, false, false, _AssemblyName.GetHashCode().ToString());
					}
					else if (!WindowRect.Contains(Input.mousePosition))
					{
						EditorLogic.fetch.Unlock(_AssemblyName.GetHashCode().ToString());
					}
				}
			}

		}

		#endregion

		#region Methods

		//Function to sort the list based on several criteria
		private List<contractContainer> sortList(List<contractContainer> cL, sortClass s, int i)
		{
			List<contractContainer> sortedList = new List<contractContainer>();
			if (i == 0)
			{
				if (s == sortClass.Default)
					return cL;
				else if (s == sortClass.Expiration)
					cL.Sort((a, b) => a.deadline.CompareTo(b.deadline));
				else if (s == sortClass.Acceptance)
					cL.Sort((a, b) => a.contract.DateAccepted.CompareTo(b.contract.DateAccepted));
				else if (s == sortClass.Reward)
					cL.Sort((a, b) => a.totalReward.CompareTo(b.totalReward));
				else if (s == sortClass.Difficulty)
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(true, a.contract.Prestige.CompareTo(b.contract.Prestige), a.contract.Title.CompareTo(b.contract.Title)));
			}
			else
			{
				if (s == sortClass.Default)
					return cL;
				else if (s == sortClass.Expiration)
					cL.Sort((a, b) => -a.deadline.CompareTo(b.deadline));
				else if (s == sortClass.Acceptance)
					cL.Sort((a, b) => -a.contract.DateAccepted.CompareTo(b.contract.DateAccepted));
				else if (s == sortClass.Reward)
					cL.Sort((a, b) => -a.totalReward.CompareTo(b.totalReward));
				else if (s == sortClass.Difficulty)
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(false, a.contract.Prestige.CompareTo(b.contract.Prestige), a.contract.Title.CompareTo(b.contract.Title)));
			}
			return cL;
		}

		//Change the contract title's GUIStyle based on its current state
		private GUIStyle titleState(Contract.State s)
		{
			switch (s)
			{
				case Contract.State.Completed:
					return contractSkins.contractCompleted;
				case Contract.State.Cancelled:
				case Contract.State.DeadlineExpired:
				case Contract.State.Failed:
				case Contract.State.Withdrawn:
					return contractSkins.contractFailed;
				default:
					return contractSkins.contractActive;
			}
		}

		//Change parameter title GUIStyle based on its current state
		private GUIStyle paramState(ParameterState s)
		{
			switch (s)
			{
				case ParameterState.Complete:
					return contractSkins.paramCompleted;
				case ParameterState.Failed:
					return contractSkins.paramFailed;
				default:
					return contractSkins.paramText;
			}
		}

		//Adds new contracts when they are accepted in Mission Control
		private void contractAccepted(Contract c)
		{
			cList.Add(new contractContainer(c));
			cList = sortList(cList, sort, order);
		}

		//Rebuild contract list when the scene changes
		private void contractLoaded()
		{
			cList.Clear();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
				{
					cList.Add(new contractContainer(c));
				}
			}
			cList = sortList(cList, sort, order);
		}

		#endregion

		#region Persistence

		//Load window position and size settings
		private void PersistenceLoad()
		{
			if (fileFound)
			{
				ConfigNode node = ConfigNode.Load(fileLocation);
				if (node != null)
				{
					ConfigNode windowNode = node.GetNode("CONTRACT_WINDOW_SETTINGS");
					if (windowNode != null)
					{
						if (float.TryParse(windowNode.GetValue("Window X"), out windowX))
							WindowRect.x = windowX;
						else
							WindowRect.x = 50;
						if (float.TryParse(windowNode.GetValue("Window Y"), out windowY))
							WindowRect.y = windowY;
						else
							WindowRect.y = 80;
						if (float.TryParse(windowNode.GetValue("Window Width"), out windowW))
							WindowRect.width = windowW;
						else
							WindowRect.width = 250;
						if (float.TryParse(windowNode.GetValue("Window Height"), out windowH))
							WindowRect.height = windowH;
						else
							WindowRect.height = 300;
						if (float.TryParse(windowNode.GetValue("Window MaxY"), out windowMaxY))
							WindowRect.yMax = windowMaxY;
						else
							WindowRect.yMax = 380;
						if (bool.TryParse(windowNode.GetValue("Window Visible"), out visible))
							IsVisible = visible;
						else
							IsVisible = false;
						if (!int.TryParse(windowNode.GetValue("Window Mode"), out windowMode))
							windowMode = 0;
						if (!int.TryParse(windowNode.GetValue("Sort Mode"), out order))
							order = 0;
						int i;
						if (int.TryParse(windowNode.GetValue("Sort Order"), out i))
							sort = (sortClass)i;
						else
							sort = sortClass.Default;
					}
				}
			}
		}

		//Save window size and position settings to config file
		private void PersistenceSave()
		{
			if (fileFound)
			{
				ConfigNode node = ConfigNode.Load(fileLocation);
				if (node != null)
				{
					node.ClearNodes();
					ConfigNode windowNode = new ConfigNode("CONTRACT_WINDOW_SETTINGS");
					if (windowNode != null)
					{
						windowNode.AddValue("Window X", WindowRect.x.ToString());
						windowNode.AddValue("Window Y", WindowRect.y.ToString());
						windowNode.AddValue("Window Width", WindowRect.width.ToString());
						windowNode.AddValue("Window Height", WindowRect.height.ToString());
						windowNode.AddValue("Window MaxY", WindowRect.yMax.ToString());
						windowNode.AddValue("Window Visible", IsVisible.ToString());
						windowNode.AddValue("Window Mode", windowMode.ToString());
						windowNode.AddValue("Sort Mode", order.ToString());
						windowNode.AddValue("Sort Order", ((int)sort).ToString());
						node.AddNode(windowNode);
						node.Save(fileLocation);
					}
				}
			}
		}

		#endregion

	}
}
