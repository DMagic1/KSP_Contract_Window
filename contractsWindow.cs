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
		private List<contractContainer> showList = new List<contractContainer>();
		private List<contractContainer> hiddenList = new List<contractContainer>();
		private List<contractContainer> nextRemoveList = new List<contractContainer>();
		private string version, assemblyLocation, assemblyFolder, fileLocation;
		private Assembly assembly;
		private Vector2 scroll;
		private bool resizing, visible, fileFound, showSort;
		private float dragStart, windowHeight, windowX, windowY, windowW, windowH, windowMaxY;
		private sortClass sort;
		private int order; //0 is descending, 1 is ascending
		private int windowMode; //0 is compact, 1 is expiration display, 2 is full display
		private int showHideList; //0 is standard, 1 shows hidden contracts
		
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
				WindowCaption = "Contracts +";
				WindowRect = new Rect(40, 80, 250, 300);
				WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
				WindowStyle = contractSkins.newWindowStyle;
				Visible = true;
				DragEnabled = true;
				TooltipsEnabled = true;
				TooltipMouseOffset = new Vector2d(-10, -20);

				PersistenceLoad();
				DragRect = new Rect(0, 0, WindowRect.width - 19, WindowRect.height - 25);

				PersistenceSave();

				SetRepeatRate(10);
				RepeatingWorkerInitialWait = 15;
				StartRepeatingWorker();

				InputLockManager.RemoveControlLock(_AssemblyName.GetHashCode().ToString());

				SkinsLibrary.SetCurrent("ContractUnitySkin");
			}
			else
			{
				//Shutdown everything if we aren't in career mode
				Visible = false;
				Toggle = false;
				WindowRect = new Rect(0, 0, 0, 0);
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

			//Menu Bar
			//GUILayout.BeginHorizontal();
			if (GUI.Button(new Rect(4, 1, 30, 17), new GUIContent (contractSkins.sortIcon, "Sort Contracts")))
				showSort = !showSort;

			if (order == 0)
			{
				if (GUI.Button(new Rect(33, 1, 24, 17), new GUIContent (contractSkins.orderAsc, "Ascending Order")))
				{
					order = 1;
					refreshContracts(cList);
				}
			}
			if (order == 1)
			{
				if (GUI.Button(new Rect(33, 1, 24, 17), new GUIContent (contractSkins.orderDesc, "Descending Order")))
				{
					order = 0;
					refreshContracts(cList);
				}
			}

			//Show and hide icons
			if (showHideList == 0)
			{
				if (GUI.Button(new Rect(60, 1, 26, 17), new GUIContent (contractSkins.revealShowIcon, "Show Hidden Contracts")))
				{
					showHideList = 1;
					cList = hiddenList;
					refreshContracts(cList);
				}
			}
			if (showHideList == 1)
			{
				if (GUI.Button(new Rect(60, 1, 26, 17), new GUIContent (contractSkins.revealHideIcon, "Show Standard Contracts")))
				{
					showHideList = 0;
					cList = showList;
					refreshContracts(cList);
				}
			}

			if (windowMode == 0)
			{
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.expandRight))
				{
					windowMode = 1;
					WindowRect.width = 290;
					DragRect.width = WindowRect.width - 20;
					PersistenceSave();
				}
			}
			if (windowMode == 1)
			{
				if (GUI.Button(new Rect(WindowRect.width - 48, 1, 24, 18), contractSkins.collapseLeftStop))
				{
					windowMode = 0;
					WindowRect.width = 250;
					DragRect.width = WindowRect.width - 20;
					PersistenceSave();
				}
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.expandRightStop))
				{
					windowMode = 2;
					WindowRect.width = 480;
					DragRect.width = WindowRect.width - 20;
					PersistenceSave();
				}
			}
			if (windowMode == 2)
			{
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.collapseLeft))
				{
					windowMode = 1;
					WindowRect.width = 290;
					DragRect.width = WindowRect.width - 20;
					PersistenceSave();
				}
			}
			//GUILayout.EndHorizontal();
			GUILayout.Space(8);

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
					GUILayout.Box(c.contract.Title, hoverTitleState(c.contract.ContractState), GUILayout.MaxWidth(210));

				GUILayout.Space(-5);
				GUILayout.BeginVertical();
				if (showHideList == 0)
				{
					if (GUILayout.Button(new GUIContent (contractSkins.hideIcon, "Hide Contract"), GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
						nextRemoveList.Add(c);
				}
				if (showHideList == 1)
				{
					if (GUILayout.Button(new GUIContent (contractSkins.showIcon, "Un-Hide Contract"), GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
						nextRemoveList.Add(c);
				}
				GUILayout.EndVertical();

				if (windowMode == 1)
				{
					if (c.duration >= 2160000)
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));
					else if (c.duration > 0)
						GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.Width(45));
					else if (c.contract.ContractState != Contract.State.Active)
						GUILayout.Label(c.daysToExpire, contractSkins.timerFinished, GUILayout.Width(45));
					else
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));
				}
				if (windowMode == 2)
				{
					if (c.duration >= 2160000)
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));
					else if (c.duration > 0)
						GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.Width(45));
					else if (c.contract.ContractState != Contract.State.Active)
						GUILayout.Label(c.daysToExpire, contractSkins.timerFinished, GUILayout.Width(45));
					else
						GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));

					//GUILayout.FlexibleSpace();
					GUILayout.BeginVertical();
					if (c.fundsReward > 0)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(11), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + c.fundsReward.ToString("N0"), contractSkins.reward, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					if (c.fundsPenalty > 0)
					{
						if (c.fundsReward > 0)
							GUILayout.Space(-6);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(11), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("- " + c.fundsPenalty.ToString("N0"), contractSkins.penalty, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical();
					if (c.repReward > 0)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + c.repReward.ToString("F0"), contractSkins.reward, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					if (c.repPenalty > 0)
					{
						if (c.repReward > 0)
							GUILayout.Space(-9);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
						GUILayout.Space(-5);
						GUILayout.Label("- " + c.repPenalty.ToString("F0"), contractSkins.penalty, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();

					GUILayout.FlexibleSpace();
					if (c.science > 0)
					{
						GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + c.science.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37));
					}
				}
				GUILayout.EndHorizontal();

				//Parameters
				if (c.showParams)
				{
					//Contract Parameter list for each contract
					foreach (parameterContainer cP in c.paramList)
					{
						if (!string.IsNullOrEmpty(cP.cParam.Title))
						{
							//Check if each parameter has notes associated with it
							if (cP.cParam.State != ParameterState.Complete && !string.IsNullOrEmpty(cP.cParam.Notes))
							{
								GUILayout.Space(-2);
								GUILayout.BeginHorizontal();
								if (GUILayout.Button("[+]", contractSkins.noteButton, GUILayout.MaxWidth(24)))
									cP.showNote = !cP.showNote;
								GUILayout.Space(3);
								GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(228));

								if (windowMode == 2)
								{
									//GUILayout.FlexibleSpace();
									GUILayout.BeginVertical();
									if (cP.cParam.FundsCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.FundsCompletion.ToString("N0"), contractSkins.reward, GUILayout.Width(65), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									if (cP.cParam.FundsFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
									{
										if (cP.cParam.FundsCompletion > 0)
											GUILayout.Space(-6);
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
										GUILayout.Space(-5);
										GUILayout.Label("- " + cP.cParam.FundsFailure.ToString("N0"), contractSkins.penalty, GUILayout.Width(65), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									GUILayout.EndVertical();

									GUILayout.FlexibleSpace();
									GUILayout.BeginVertical();
									if (cP.cParam.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									if (cP.cParam.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
									{
										if (cP.cParam.ReputationCompletion > 0)
											GUILayout.Space(-9);
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("- " + cP.cParam.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									GUILayout.EndVertical();

									GUILayout.FlexibleSpace();
									if (cP.cParam.ScienceCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37), GUILayout.MaxHeight(14));
									}
								}

								GUILayout.EndHorizontal();
								if (cP.showNote)
								{
									GUILayout.Space(-3);
									GUILayout.Box(cP.cParam.Notes, GUILayout.MaxWidth(261));
								}
							}

							//If no notes are present just display the title
							else
							{
								GUILayout.Space(-2);
								GUILayout.BeginHorizontal();
								GUILayout.Space(15);
								GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(250));
								if (windowMode == 2)
								{
									//GUILayout.FlexibleSpace();
									GUILayout.BeginVertical();
									if (cP.cParam.FundsCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.FundsCompletion.ToString("N0"), contractSkins.reward, GUILayout.Width(65), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									if (cP.cParam.FundsFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
									{
										if (cP.cParam.FundsCompletion > 0)
											GUILayout.Space(-6);
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
										GUILayout.Space(-5);
										GUILayout.Label("- " + cP.cParam.FundsFailure.ToString("N0"), contractSkins.penalty, GUILayout.Width(65), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									GUILayout.EndVertical();

									GUILayout.FlexibleSpace();
									GUILayout.BeginVertical();
									if (cP.cParam.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									if (cP.cParam.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
									{
										if (cP.cParam.ReputationCompletion > 0)
											GUILayout.Space(-9);
										GUILayout.BeginHorizontal();
										GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("- " + cP.cParam.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38), GUILayout.MaxHeight(13));
										GUILayout.EndHorizontal();
									}
									GUILayout.EndVertical();

									GUILayout.FlexibleSpace();
									if (cP.cParam.ScienceCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
									{
										GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
										GUILayout.Space(-5);
										GUILayout.Label("+ " + cP.cParam.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37), GUILayout.MaxHeight(14));
									}
								}
								GUILayout.EndHorizontal();
							}
						}
					}
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(18);
			GUILayout.EndVertical();

			if (showSort)
			{
				GUILayout.BeginVertical();
				Rect dropDownSort = new Rect(10, 20, 80, 88);
				GUI.Box(dropDownSort, "", contractSkins.dropDown);
				if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 2, dropDownSort.width - 4, 20), "Expiration", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Expiration;
					refreshContracts(cList);
				}
				if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 23, dropDownSort.width - 4, 20), "Acceptance", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Acceptance;
					refreshContracts(cList);
				}
				if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 44, dropDownSort.width - 4, 20), "Difficulty", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Difficulty;
					refreshContracts(cList);
				}
				if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 65, dropDownSort.width - 4, 20), "Reward", contractSkins.sortMenu))
				{
					showSort = false;
					sort = sortClass.Reward;
					refreshContracts(cList);
				}

				GUILayout.EndVertical();
			}

			//Version label
			GUI.Label(new Rect(10, WindowRect.height - 20, 30, 20), version, contractSkins.paramText);

			//Tooltip toggle icon
			if (GUI.Button(new Rect(40, WindowRect.height - 22, 35, 20), new GUIContent(contractSkins.tooltipIcon, "Toggle Tooltips")))
				TooltipsEnabled = !TooltipsEnabled;

			//Resize window when the resizer is grabbed by the mouse
			Rect resizer = new Rect(WindowRect.width - 19, WindowRect.height - 28, 20, 26);
			GUI.Label(resizer, contractSkins.expandIcon, contractSkins.dragButton);

			if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
			{
				if (resizer.Contains(Event.current.mousePosition))
				{
					resizing = true;
					dragStart = Input.mousePosition.y;
					windowHeight = WindowRect.height;
					Event.current.Use();
				}
			}
			if (resizing)
			{
				if (Input.GetMouseButtonUp(0))
				{
					resizing = false;
					WindowRect.yMax = WindowRect.y + windowHeight;
					PersistenceSave();
				}
				else
				{
					//Only consider y direction of mouse input
					float height = Input.mousePosition.y;
					if (Input.mousePosition.y < 0)
						height = 0;
					windowHeight += dragStart - height;
					dragStart = height;
					WindowRect.yMax = WindowRect.y + windowHeight;
					if (WindowRect.yMax > Screen.height)
					{
						WindowRect.yMax = Screen.height;
						windowHeight = WindowRect.yMax - WindowRect.y;
					}
					if (windowHeight < 200)
						windowHeight = 200;
				}
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

			if (nextRemoveList.Count > 0)
			{
				foreach (contractContainer c in nextRemoveList)
					showHideContract(c);
				nextRemoveList.Clear();
			}

			//Draw the resizer in rectangle
			if (Toggle)
			{
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
					cL.Sort((a, b) => a.duration.CompareTo(b.duration));
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
					cL.Sort((a, b) => -a.duration.CompareTo(b.duration));
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

		private GUIStyle hoverTitleState(Contract.State s)
		{
			switch (s)
			{
				case Contract.State.Completed:
					return contractSkins.contractCompletedBehind;
				case Contract.State.Cancelled:
				case Contract.State.DeadlineExpired:
				case Contract.State.Failed:
				case Contract.State.Withdrawn:
					return contractSkins.contractFailedBehind;
				default:
					return contractSkins.contractActiveBehind;
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
			showList.Add(new contractContainer(c));
			showList = sortList(cList, sort, order);
			cList = showList;
		}

		//Rebuild contract list when the scene changes
		private void contractLoaded()
		{
			showList.Clear();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
				{
					cList.Add(new contractContainer(c));
				}
			}
			showList = sortList(cList, sort, order);
			cList = showList;
		}

		private void refreshContracts(List<contractContainer> cL)
		{
			foreach (contractContainer cC in cL)
			{
				if (cC.contract.ContractState != Contract.State.Active)
				{
					cC.duration = 0;
					cC.daysToExpire = "---";
					if (cC.contract.ContractState == Contract.State.Completed)
					{
						cC.fundsPenalty = 0;
						cC.repPenalty = 0;
					}
					else if (cC.contract.ContractState == Contract.State.Failed || cC.contract.ContractState == Contract.State.DeadlineExpired || cC.contract.ContractState == Contract.State.Cancelled)
					{
						cC.fundsReward = 0;
						cC.repReward = 0;
						cC.science = 0;
					}
					continue;
				}
				//Update contract timers
				if (cC.contract.DateDeadline <= 0)
				{
					cC.duration = double.MaxValue;
					cC.daysToExpire = "----";
				}
				else
				{
					cC.duration = cC.deadline - Planetarium.GetUniversalTime();
					//Calculate time in day values using Kerbin or Earth days
					cC.daysToExpire = contractContainer.timeInDays(cC.duration);
				}
			}
			cL = sortList(cL, sort, order);
		}

		//Remove contract from primary list and update
		private void showHideContract(contractContainer c)
		{
			if (showHideList == 0)
			{
				hiddenList.Add(c);
				showList.Remove(c);
				cList= showList;
				refreshContracts(cList);
			}
			else
			{
				showList.Add(c);
				hiddenList.Remove(c);
				cList = hiddenList;
				refreshContracts(cList);
			}
		}

		#endregion

		#region Repeating Worker

		internal override void RepeatingWorker()
		{
			LogFormatted_DebugOnly("Refreshing Contract List; Duration of repeat: {0}", RepeatingWorkerDuration);
			if (cList.Count > 0)
				refreshContracts(cList);
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
