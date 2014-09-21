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
using Contracts.Parameters;
using UnityEngine;

namespace ContractsWindow
{

	class contractsWindow: MonoBehaviourWindow
	{

		#region Initialization

		private List<Guid> cList = new List<Guid>();
		private List<Guid> pinnedList = new List<Guid>();
		private List<contractContainer> nextRemoveList = new List<contractContainer>();
		private List<contractContainer> nextPinnedList = new List<contractContainer>();
		private string version;
		private Vector2 scroll;
		private bool resizing, showSort, rebuild, editorLocked, spacecenterLocked, trackingLocked, contractsLoading, loaded;
		private float dragStart, windowHeight;
		private int timer;
		private Rect dropDownSort, resetRect;
		private int sceneInt;
		private const string lockID = "ContractsWindow_LockID";
		private const string centerLockID = "ContractsWindow_SC_LockID";
		private const string trackingLockID = "ContractsWindow_TS_LockID";

		private contractScenario contract = contractScenario.Instance;

		internal override void Awake()
		{
			Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
			version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
			sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);

			//Set up the various GUI options to their default values here
			WindowCaption = "Contracts +";
			WindowRect = new Rect(40, 80, 250, 300);
			WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
			WindowStyle = contractSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			RepeatingWorkerInitialWait = 10;

			//Make sure our click-through control locks are disabled
			InputLockManager.RemoveControlLock(lockID);

			SkinsLibrary.SetCurrent("ContractUnitySkin");
		}

		internal override void Start()
		{
			GameEvents.Contract.onAccepted.Add(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Add(contractLoaded);
			PersistenceLoad();
		}

		internal override void OnDestroy()
		{
			GameEvents.Contract.onAccepted.Remove(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Remove(contractLoaded);
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
			if (InputLockManager.lockStack.ContainsKey(centerLockID))
				InputLockManager.RemoveControlLock(centerLockID);
			if (InputLockManager.lockStack.ContainsKey(trackingLockID))
				InputLockManager.RemoveControlLock(trackingLockID);
		}

		internal override void Update()
		{
			if (HighLogic.LoadedScene == GameScenes.FLIGHT && !loaded && !contractsLoading)
			{
				contractsLoading = true;
				timer = 0;
			}
			//Start a timer after contracts begin loading to ensure that all contracts are loaded before we populate the lists
			else if (contractsLoading && !loaded)
			{
				if (timer < 15)
					timer++;
				else
				{
					contractsLoading = false;
					loaded = true;

					generateList();

					//Load ordering lists and contract settings after primary contract dictionary has been loaded
					contractScenario.Instance.loadContractLists(contractScenario.Instance.showString, 0);
					contractScenario.Instance.loadContractLists(contractScenario.Instance.hiddenString, 1);

					if (contractScenario.Instance.showHideMode[sceneInt] == 0)
					{
						cList = contractScenario.Instance.showList;
						pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					}
					else
					{
						cList = contractScenario.Instance.hiddenList;
						pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					}

					if (cList.Count == 0)
						rebuildList();

					if (cList.Count > 0)
						refreshContracts(cList);
				}
			}
		}

		#endregion

		#region GUI Draw

		internal override void DrawWindowPre(int id)
		{
			//Update the drag rectangle
			DragRect.height = WindowRect.height - 24;

			//Prevent click through from activating part options
			if (HighLogic.LoadedSceneIsFlight)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && GUIUtility.hotControl == 0 && Input.GetMouseButton(0))
				{
					foreach (var window in GameObject.FindObjectsOfType(typeof(UIPartActionWindow)).OfType<UIPartActionWindow>().Where(p => p.Display == UIPartActionWindow.DisplayType.Selected))
					{
						window.enabled = false;
						window.displayDirty = true;
					}
				}
			}

			//Lock space center click through
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !spacecenterLocked)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_FACILITIES | ControlTypes.KSC_UI, centerLockID);
					spacecenterLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && spacecenterLocked)
				{
					InputLockManager.RemoveControlLock(centerLockID);
					spacecenterLocked = false;
				}
			}

			//Lock tracking scene click through
			if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !trackingLocked)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.TRACKINGSTATION_ALL, trackingLockID);
					trackingLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && trackingLocked)
				{
					InputLockManager.RemoveControlLock(trackingLockID);
					trackingLocked = false;
				}
			}

			//Lock editor click through
			if (HighLogic.LoadedSceneIsEditor)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !editorLocked)
				{
					EditorLogic.fetch.Lock(true, true, true, lockID);
					editorLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && editorLocked)
				{
					EditorLogic.fetch.Unlock(lockID);
					editorLocked = false;
				}
			}
		}

		internal override void DrawWindow(int id)
		{
			//Menu Bar
			buildMenuBar(id);

			GUILayout.BeginVertical();
			GUILayout.Space(8);
			scroll = GUILayout.BeginScrollView(scroll);

			//Contract List Begins
			foreach (Guid gID in cList)
			{
				contractContainer c = contractScenario.Instance.getContract(gID);
				//Contracts

				GUILayout.Space(-1);

				buildContractTitleBar(c, id);

				GUILayout.Space(-5);

				buildContractTitle(c, id);

				//Parameters
				if (c.showParams)
				{
					foreach (parameterContainer cP in c.paramList)
					{
						if (cP.level == 0)
							buildParameterLabel(cP, c, 0, id);
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.Space(18);
			GUILayout.EndVertical();

			//If the sort menu is open
			if (showSort)
				buildSortMenu(id);

			//Bottom bar
			buildBottomBar(id);

			//Reset warning menu
			if (rebuild)
				resetMenu(id);

			//Resize window when the resizer is grabbed by the mouse
			buildResizer(id);
		}

		#region Top Menu

		private void buildMenuBar(int ID)
		{
			//Sort icons
			if (GUI.Button(new Rect(4, 2, 26, 18), new GUIContent(contractSkins.sortIcon, "Sort Contracts")))
				showSort = !showSort;

			if (contractScenario.Instance.orderMode[sceneInt] == 0)
			{
				if (GUI.Button(new Rect(30, 2, 26, 18), new GUIContent(contractSkins.orderAsc, "Ascending Order")))
				{
					contractScenario.Instance.orderMode[sceneInt] = 1;
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(new Rect(30, 2, 26, 18), new GUIContent(contractSkins.orderDesc, "Descending Order")))
				{
					contractScenario.Instance.orderMode[sceneInt] = 0;
					refreshContracts(cList);
				}
			}

			//Show and hide icons
			if (contractScenario.Instance.showHideMode[sceneInt] == 0)
			{
				if (GUI.Button(new Rect(56, 1, 26, 18), new GUIContent(contractSkins.revealShowIcon, "Show Hidden Contracts")))
				{
					contractScenario.Instance.showHideMode[sceneInt] = 1;
					cList = contractScenario.Instance.hiddenList;
					pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(new Rect(56, 1, 26, 18), new GUIContent(contractSkins.revealHideIcon, "Show Standard Contracts")))
				{
					contractScenario.Instance.showHideMode[sceneInt] = 0;
					cList = contractScenario.Instance.showList;
					pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}

			//Expand and contract icons
			if (contractScenario.Instance.windowMode[sceneInt] == 0)
			{
				if (GUI.Button(new Rect(WindowRect.width - 20, 1, 16, 18), contractSkins.expandRight))
				{
					contractScenario.Instance.windowMode[sceneInt] = 1;
					WindowRect.width = 480;
					DragRect.width = WindowRect.width - 19;
				}
			}
			else
			{
				if (GUI.Button(new Rect(WindowRect.width - 22, 1, 16, 18), contractSkins.collapseLeft))
				{
					contractScenario.Instance.windowMode[sceneInt] = 0;
					WindowRect.width = 250;
					DragRect.width = WindowRect.width - 19;
				}
			}

			GUI.DrawTexture(new Rect(2, 17, WindowRect.width - 4, 4), contractSkins.headerBar);
		}


		#endregion

		#region Contract Title Bar

		private void buildContractTitleBar(contractContainer c, int id)
		{
			GUILayout.BeginHorizontal();
			//Difficulty icons
			if (c.contract.Prestige == Contract.ContractPrestige.Trivial)
			{
				GUILayout.Space(16);
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
				GUILayout.Space(16);
			}
			else if (c.contract.Prestige == Contract.ContractPrestige.Significant)
			{
				GUILayout.Space(8);
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
				GUILayout.Space(-4);
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
				GUILayout.Space(8);
			}
			else
			{
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
				GUILayout.Space(-6);
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
				GUILayout.Space(-6);
				GUILayout.Label(contractSkins.goldStar, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
			}

			//Expiration date
			if (c.duration >= 2160000)
				GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(55));
			else if (c.duration > 0)
				GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.Width(55));
			else if (c.contract.ContractState == Contract.State.Completed)
				GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(55));
			else
				GUILayout.Label(c.daysToExpire, contractSkins.timerFinished, GUILayout.Width(55));

			GUILayout.Space(4);

			//Show and hide icons
			if (c.contract.ContractState == Contract.State.Active)
			{
				if (contractScenario.Instance.showHideMode[sceneInt] == 0)
				{
					if (GUILayout.Button(new GUIContent(contractSkins.hideIcon, "Hide Contract"), contractSkins.texButtonSmall, GUILayout.MaxWidth(16), GUILayout.MaxHeight(14)))
					{
						nextRemoveList.Add(c);
					}
				}
				else
				{
					if (GUILayout.Button(new GUIContent(contractSkins.showIcon, "Un-Hide Contract"), contractSkins.texButtonSmall, GUILayout.MaxWidth(16), GUILayout.MaxHeight(12)))
					{
						nextRemoveList.Add(c);
					}
				}
			}
			else
				if (GUILayout.Button(new GUIContent(contractSkins.closeIcon, "Remove Contract"), contractSkins.texButtonSmall, GUILayout.MaxWidth(16), GUILayout.MaxHeight(16)))
				{
					nextRemoveList.Add(c);
				}

			GUILayout.Space(4);

			//Pin icon button
			if (c.listOrder == null)
			{
				if (GUILayout.Button(new GUIContent(contractSkins.pinIcon, "Pin Contract"), contractSkins.texButtonSmall, GUILayout.MaxWidth(16), GUILayout.MaxHeight(14)))
				{
					nextPinnedList.Add(c);
				}
			}
			else
			{
				if (GUILayout.Button(new GUIContent(contractSkins.pinDownIcon, "Un-Pin Contract"), contractSkins.texButtonSmall, GUILayout.MaxWidth(14), GUILayout.MaxHeight(16)))
				{
					nextPinnedList.Add(c);
				}
				GUILayout.Space(2);
			}

			GUILayout.Space(6);

			//Note icon button
			if (c.contract.ContractState == Contract.State.Active && !string.IsNullOrEmpty(c.contract.Notes))
			{
				if (!c.showNote)
				{
					if (GUILayout.Button(new GUIContent(contractSkins.noteIcon, "Show Note"), contractSkins.texButtonSmall, GUILayout.MaxWidth(14), GUILayout.MaxHeight(14)))
						c.showNote = !c.showNote;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(contractSkins.noteIconOff, "Hide Note"), contractSkins.texButtonSmall, GUILayout.MaxWidth(14), GUILayout.MaxHeight(14)))
						c.showNote = !c.showNote;
				}
			}

			GUILayout.EndHorizontal();
		}

		#endregion

		#region Contract Titles

		private void buildContractTitle(contractContainer c, int id)
		{
			//Contract title buttons
			GUILayout.BeginHorizontal();
			if (!showSort && !rebuild)
			{
				if (GUILayout.Button(c.contract.Title, titleState(c.contract.ContractState), GUILayout.MaxWidth(225)))
					c.showParams = !c.showParams;
			}
			else
				GUILayout.Box(c.contract.Title, hoverTitleState(c.contract.ContractState), GUILayout.MaxWidth(225));

			if (contractScenario.Instance.windowMode[sceneInt] == 1)
			{
				//Reward and penalty amounts
				if (c.contract.FundsCompletion > 0 || c.contract.FundsFailure > 0)
				{
					GUILayout.BeginVertical();
					if (c.contract.FundsCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(11), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + c.contract.FundsCompletion.ToString("N0"), contractSkins.reward, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					if (c.contract.FundsFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Cancelled || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Failed))
					{
						if (c.contract.ContractState == Contract.State.Active)
							GUILayout.Space(-6);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(11), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("- " + c.contract.FundsFailure.ToString("N0"), contractSkins.penalty, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				else
					GUILayout.Space(75);
				
				//Rep rewards and penalty amounts
				if (c.contract.ReputationCompletion > 0 || c.contract.ReputationFailure > 0)
				{
					GUILayout.BeginVertical();
					if (c.contract.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + c.contract.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					if (c.contract.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Cancelled || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Failed))
					{
						if (c.contract.ContractState == Contract.State.Active)
							GUILayout.Space(-8);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
						GUILayout.Space(-5);
						GUILayout.Label("- " + c.contract.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				else
					GUILayout.Space(55);

				//Science reward
				if (c.contract.ScienceCompletion > 0)
				{
					if (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed)
					{
						GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(11), GUILayout.MaxWidth(11));
						GUILayout.Label("+ " + c.contract.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37));
					}
				}
				else
					GUILayout.Space(58);

			}
			GUILayout.EndHorizontal();

			//Display note
			if (!string.IsNullOrEmpty(c.contract.Notes) && c.showNote && c.contract.ContractState == Contract.State.Active)
			{
				GUILayout.Space(-3);
				GUILayout.Box(c.contract.Notes, GUILayout.MaxWidth(300));
			}
		}

		#endregion

		#region Parameters

		private void buildParameterLabel(parameterContainer cP, contractContainer c, int level, int id)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(5 + (level * 5));

			//Note icon button
			if (cP.cParam.State == ParameterState.Incomplete && !string.IsNullOrEmpty(cP.cParam.Notes))
			{
				GUILayout.Space(-2);
				if (!cP.showNote)
				{
					if (GUILayout.Button(new GUIContent(contractSkins.noteIcon, "Show Note"), GUILayout.MaxWidth(14), GUILayout.MaxHeight(14)))
						cP.showNote = !cP.showNote;
				}
				else
				{
					if (GUILayout.Button(new GUIContent(contractSkins.noteIconOff, "Hide Note"), GUILayout.MaxWidth(14), GUILayout.MaxHeight(14)))
						cP.showNote = !cP.showNote;
				}
				GUILayout.Space(-2);
			}

			//Editor part icon button
			if (cP.part != null && HighLogic.LoadedSceneIsEditor)
			{
				GUILayout.Space(-4);
				if (GUILayout.Button(new GUIContent(contractSkins.partIcon, "Show Part"), GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
				{
					EditorLogic.fetch.Unlock(lockID);
					editorLocked = false;
					EditorPartList.Instance.RevealPart(cP.part, true);
				}
				GUILayout.Space(-2);
			}

			//Contract parameter title
			GUILayout.Box(cP.cParam.Title, paramState(cP), GUILayout.MaxWidth(220 - (level * 5)));

			//Parameter reward info
			if (contractScenario.Instance.windowMode[sceneInt] == 1 && cP.cParam.State == ParameterState.Incomplete)
			{
				if (cP.fundsReward> 0 || cP.fundsPenalty > 0)
				{
					GUILayout.BeginVertical();
					if (cP.fundsReward > 0 && (cP.cParam.State == ParameterState.Complete || cP.cParam.State == ParameterState.Incomplete))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + cP.fundsReward.ToString("N0"), contractSkins.reward, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					if (cP.fundsPenalty > 0 && (cP.cParam.State == ParameterState.Incomplete || cP.cParam.State == ParameterState.Failed))
					{
						if (cP.cParam.State == ParameterState.Incomplete)
							GUILayout.Space(-6);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
						GUILayout.Space(-5);
						GUILayout.Label("- " + cP.fundsPenalty.ToString("N0"), contractSkins.penalty, GUILayout.Width(65));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				else
					GUILayout.Space(75);

				if (cP.repReward > 0 || cP.repPenalty > 0)
				{
					GUILayout.BeginVertical();
					if (cP.repReward > 0 && (cP.cParam.State == ParameterState.Complete || cP.cParam.State == ParameterState.Incomplete))
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
						GUILayout.Space(-5);
						GUILayout.Label("+ " + cP.repReward.ToString("F0"), contractSkins.reward, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					if (cP.repPenalty > 0 && (cP.cParam.State == ParameterState.Incomplete || cP.cParam.State == ParameterState.Failed))
					{
						if (cP.cParam.State == ParameterState.Incomplete)
							GUILayout.Space(-8);
						GUILayout.BeginHorizontal();
						GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(12), GUILayout.MaxWidth(12));
						GUILayout.Space(-5);
						GUILayout.Label("- " + cP.repPenalty.ToString("F0"), contractSkins.penalty, GUILayout.Width(38));
						GUILayout.EndHorizontal();
					}
					GUILayout.EndVertical();
				}
				else
					GUILayout.Space(55);

				if (cP.scienceReward > 0 && (cP.cParam.State == ParameterState.Complete || cP.cParam.State == ParameterState.Incomplete))
				{
					GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(11), GUILayout.MaxWidth(11));
					GUILayout.Label("+ " + cP.scienceReward.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37));
				}
				else
					GUILayout.Space(58);

			}
			GUILayout.EndHorizontal();

			//Display note
			if (!string.IsNullOrEmpty(cP.cParam.Notes) && cP.showNote && cP.cParam.State == ParameterState.Incomplete)
			{
				GUILayout.Space(-3);
				GUILayout.Box(cP.cParam.Notes, GUILayout.MaxWidth(320));
			}

			if (level < 4)
			{
				foreach (parameterContainer sP in cP.paramList)
				{
					if (sP.level == level + 1)
						buildParameterLabel(sP, c, level + 1, id);
				}
			}

		}

		#endregion

		#region Sort Menu

		//Sort option buttons to display when the sort button is activated
		private void buildSortMenu(int id)
		{
			dropDownSort = new Rect(10, 20, 80, 110);
			GUI.Box(dropDownSort, "", contractSkins.dropDown);
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 2, dropDownSort.width - 4, 20), "Expiration", contractSkins.sortMenu))
			{
				showSort = false;
				contractScenario.Instance.sortMode[sceneInt] = sortClass.Expiration;
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 23, dropDownSort.width - 4, 20), "Acceptance", contractSkins.sortMenu))
			{
				showSort = false;
				contractScenario.Instance.sortMode[sceneInt] = sortClass.Acceptance;
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 44, dropDownSort.width - 4, 20), "Difficulty", contractSkins.sortMenu))
			{
				showSort = false;
				contractScenario.Instance.sortMode[sceneInt] = sortClass.Difficulty;
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 65, dropDownSort.width - 4, 20), "Reward", contractSkins.sortMenu))
			{
				showSort = false;
				contractScenario.Instance.sortMode[sceneInt] = sortClass.Reward;
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 86, dropDownSort.width - 4, 20), "Type", contractSkins.sortMenu))
			{
				showSort = false;
				contractScenario.Instance.sortMode[sceneInt] = sortClass.Type;
				refreshContracts(cList);
			}
		}

		#endregion

		#region Bottom Bar

		private void buildBottomBar(int id)
		{
			GUI.DrawTexture(new Rect(2, WindowRect.height - 30, WindowRect.width - 4, 4), contractSkins.footerBar);

			//Version label
			GUI.Label(new Rect(8, WindowRect.height - 23, 30, 20), version, contractSkins.paramText);

			//Tooltip toggle icon
			if (GUI.Button(new Rect(36, WindowRect.height - 25, 30, 22), new GUIContent(contractSkins.tooltipIcon, "Toggle Tooltips")))
			{
				TooltipsEnabled = !TooltipsEnabled;
				contractScenario.Instance.toolTips = TooltipsEnabled;
			}

			//Clear list button
			if (GUI.Button(new Rect(74, WindowRect.height - 25, 30, 22), new GUIContent(contractSkins.resetIcon, "Reset Contracts Window Display")))
				rebuild = !rebuild;

			//Font size button
			if (GUI.Button(new Rect(112, WindowRect.height - 25, 30, 22), new GUIContent(contractSkins.fontSize, "Toggle Font Size")))
			{
				if (contractSkins.normalFontSize == 0)
					contractSkins.normalFontSize = 1;
				else
					contractSkins.normalFontSize = 0;
				contractSkins.initializeSkins();
				SkinsLibrary.SetCurrent("ContractUnitySkin");
				contractScenario.Instance.fontSmall = !contractScenario.Instance.fontSmall;
			}

			//Window size button
			if (GUI.Button(new Rect(150, WindowRect.height - 25, 30, 22), new GUIContent(contractSkins.windowSize, "Change Window Size")))
			{
				if (contractSkins.windowFontSize == 0)
					contractSkins.windowFontSize = 1;
				else
					contractSkins.windowFontSize = 0;
				contractSkins.initializeSkins();
				SkinsLibrary.SetCurrent("ContractUnitySkin");
				if (contractScenario.Instance.windowSize == 0)
					contractScenario.Instance.windowSize = 1;
				else
					contractScenario.Instance.windowSize = 0;
			}
		}

		#endregion

		#region ResetMenu

		private void resetMenu(int id)
		{
			resetRect = new Rect(10, WindowRect.height - 180, 230, 150);
			GUI.Box(resetRect, "", contractSkins.dropDown);
			GUI.Label(new Rect(resetRect.x + 7, resetRect.y + 10, resetRect.width - 14, 100), "Rebuild\nContracts Window + Display:\n\n<b>Will Not</b> Affect Contract Status", contractSkins.resetBox);
			if (GUI.Button(new Rect(resetRect.x + 20, resetRect.y + 110, resetRect.width - 40, 25), "Reset Display", contractSkins.resetButton))
			{
				LogFormatted("Rebuilding Contract Window List");
				generateList();
				rebuildList();
				rebuild = false;
			}
		}

		#endregion


		#region Resizer

		private void buildResizer(int id)
		{
			Rect resizer = new Rect(WindowRect.width - 16, WindowRect.height - 25, 14, 22);
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

		#endregion


		internal override void DrawWindowPost(int id)
		{
			//Pin contracts after the window is drawn
			if (nextPinnedList.Count > 0)
			{
				foreach(contractContainer c in nextPinnedList)
				{
					if (pinnedList.Contains(c.contract.ContractGuid))
					{
						pinnedList.Remove(c.contract.ContractGuid);
						c.listOrder = null;
					}
					else
					{
						c.listOrder = pinnedList.Count;
						pinnedList.Add(c.contract.ContractGuid);
					}
				}

				nextPinnedList.Clear();
				refreshContracts(cList);
			}

			//Remove any hidden contracts after the window is drawn
			if (nextRemoveList.Count > 0)
			{
				foreach (contractContainer c in nextRemoveList)
					showHideContract(c);

				nextRemoveList.Clear();
				refreshContracts(cList);
			}

			

			//Close the sort menu if clicked outside of its rectangle
			if (showSort && Event.current.type == EventType.mouseDown && !dropDownSort.Contains(Event.current.mousePosition))
				showSort = false;

			//Close the reset warning if clicked outside of its rectangle
			if (rebuild && Event.current.type == EventType.mouseDown && !resetRect.Contains(Event.current.mousePosition))
				rebuild = false;

			//Set the persistent window location
			contractScenario.Instance.windowRects[sceneInt] = WindowRect;
		}

		#endregion

		#region Methods

		//Reset contract list from the "refresh" button
		private void rebuildList()
		{
			contractScenario.Instance.showHideMode[sceneInt] = 0;
			contractScenario.Instance.showList.Clear();
			contractScenario.Instance.hiddenList.Clear();
			cList.Clear();
			pinnedList.Clear();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (contractScenario.Instance.getContract(c.ContractGuid) != null)
					contractScenario.Instance.showList.Add(c.ContractGuid);
			}
			cList = contractScenario.Instance.showList;
			refreshContracts(cList);
		}

		//Initial contract load
		private void generateList()
		{
			contractScenario.Instance.resetList();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
					contractScenario.Instance.addContract(c.ContractGuid, new contractContainer(c));
			}
		}

		//Update contract values
		private void refreshContracts(List<Guid> gID)
		{
			List<Guid> removeList = new List<Guid>();
			foreach (Guid id in gID)
			{
				contractContainer cC = contractScenario.Instance.getContract(id);
				if (cC == null)
				{
					removeList.Add(id);
					continue;
				}
				else
				{
					if (cC.contract.ContractState != Contract.State.Active)
					{
						cC.duration = 0;
						cC.daysToExpire = "----";
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
						cC.duration = cC.contract.DateDeadline - Planetarium.GetUniversalTime();
						//Calculate time in day values using Kerbin or Earth days
						cC.daysToExpire = contractScenario.timeInDays(cC.duration);
					}
				}
			}
			foreach (Guid removeID in removeList)
				gID.Remove(removeID);
			gID = sortList(gID, contractScenario.Instance.sortMode[sceneInt], contractScenario.Instance.orderMode[sceneInt]);
			if (contractScenario.Instance.showHideMode[sceneInt] == 0)
				contractScenario.Instance.showList = gID;
			else
				contractScenario.Instance.hiddenList = gID;
		}

		//Remove contract from current list and update
		private void showHideContract(contractContainer c)
		{
			if (contractScenario.Instance.showHideMode[sceneInt] == 0)
			{
				if (!contractScenario.Instance.hiddenList.Contains(c.contract.ContractGuid) && c.contract.ContractState == Contract.State.Active)
				{
					contractScenario.Instance.hiddenList.Add(c.contract.ContractGuid);
					c.showParams = false;
				}
				contractScenario.Instance.showList.Remove(c.contract.ContractGuid);
				if (pinnedList.Contains(c.contract.ContractGuid))
				{
					pinnedList.Remove(c.contract.ContractGuid);
					c.listOrder = null;
				}
				cList = contractScenario.Instance.showList;
			}
			else
			{
				if (!contractScenario.Instance.showList.Contains(c.contract.ContractGuid) && c.contract.ContractState == Contract.State.Active)
				{
					contractScenario.Instance.showList.Add(c.contract.ContractGuid);
					c.showParams = true;
				}
				contractScenario.Instance.hiddenList.Remove(c.contract.ContractGuid);
				if (pinnedList.Contains(c.contract.ContractGuid))
				{
					pinnedList.Remove(c.contract.ContractGuid);
					c.listOrder = null;
				}
				cList = contractScenario.Instance.hiddenList;
			}
		}

		//Function to sort the list based on several criteria
		private List<Guid> sortList(List<Guid> gID, sortClass s, int i)
		{
			List<contractContainer> cL = new List<contractContainer>();
			//Only add non-pinned contracts to the sort list
			foreach (Guid id in gID)
			{
				contractContainer cC = contractScenario.Instance.getContract(id);
				if (cC != null)
				{
					if (cC.listOrder == null)
						cL.Add(cC);
				}
			}
			bool Order = i < 1;
			if (s == sortClass.Default)
				return gID;
			else if (s == sortClass.Expiration)
				cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Order, a.duration.CompareTo(b.duration), a.contract.Title.CompareTo(b.contract.Title)));
			else if (s == sortClass.Acceptance)
				cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Order, a.contract.DateAccepted.CompareTo(b.contract.DateAccepted), a.contract.Title.CompareTo(b.contract.Title)));
			else if (s == sortClass.Reward)
				cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Order, a.totalReward.CompareTo(b.totalReward), a.contract.Title.CompareTo(b.contract.Title)));
			else if (s == sortClass.Difficulty)
				cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Order, a.contract.Prestige.CompareTo(b.contract.Prestige), a.contract.Title.CompareTo(b.contract.Title)));
			else if (s == sortClass.Type)
			{
				cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Order, a.contract.GetType().Name.CompareTo(b.contract.GetType().Name), a.contract.Title.CompareTo(b.contract.Title)));
				cL = typeSort(cL, Order);
			}
			gID.Clear();
			
			//Add pinned contracts to the beginning of the list
			if (pinnedList.Count > 0)
				gID.AddRange(pinnedList);

			//Next add the sorted contracts
			foreach (contractContainer cC in cL)
				gID.Add(cC.contract.ContractGuid);

			return gID;
		}

		//Special method for handling altitude based parameters; only ReachAltitudeEnvelope seems to be relevant
		private List<contractContainer> typeSort(List<contractContainer> cL, bool B)
		{
			List<int> position = new List<int>();
			List<contractContainer> altList = new List<contractContainer>();
			for (int i = 0; i < cL.Count; i++)
			{
				foreach (ContractParameter cP in cL[i].contract.AllParameters)
				{
					if (cP.GetType() == typeof(ReachAltitudeEnvelope))
					{
						altList.Add(cL[i]);
						position.Add(i);
						break;
					}
				}
			}
			if (altList.Count > 1)
			{
				altList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachAltitudeEnvelope)a.contract.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude.CompareTo(((ReachAltitudeEnvelope)b.contract.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude), a.contract.Title.CompareTo(b.contract.Title)));
				for (int j = 0; j < position.Count; j++)
				{
					cL[position[j]] = altList[j];
				}
			}

			//ReachFlightEnvelop doesn't actually seem to be used by anything

			//position.Clear();
			//List<contractContainer> flightList = new List<contractContainer>();
			//for (int i = 0; i < cL.Count; i++)
			//{
			//    foreach (parameterContainer cP in cL[i].paramList)
			//    {
			//        if (cP.cParam.ID == "testFlightEnvelope")
			//        {
			//            flightList.Add(cL[i]);
			//            position.Add(i);
			//        }
			//    }
			//}
			//if (flightList.Count > 1)
			//{
			//    flightList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachFlightEnvelope)a.contract.AllParameters.First(s => s.ID == "testFlightEnvelope")).minAltitude.CompareTo(((ReachFlightEnvelope)b.contract.AllParameters.First(s => s.ID == "testFlightEnvelope")).minAltitude), a.contract.Title.CompareTo(b.contract.Title)));
			//    for (int j = 0; j < position.Count; j++)
			//    {
			//        cL[position[j]] = flightList[j];
			//    }
			//}

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

		//Label style for contract titles when the sort menu is open
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
		private GUIStyle paramState(parameterContainer cP)
		{
			switch (cP.cParam.State)
			{
				case ParameterState.Complete:
					return contractSkins.paramCompleted;
				case ParameterState.Failed:
					return contractSkins.paramFailed;
				default:
					if (cP.level == 0)
						return contractSkins.paramText;
					else
						return contractSkins.paramSub;
			}
		}

		//Adds new contracts when they are accepted in Mission Control
		private void contractAccepted(Contract c)
		{
			contractScenario.Instance.addContract(c.ContractGuid, new contractContainer(c));
			contractScenario.Instance.showList.Add(c.ContractGuid);
			if (contractScenario.Instance.showHideMode[sceneInt] == 0)
			{
				contractScenario.Instance.showList = sortList(contractScenario.Instance.showList, contractScenario.Instance.sortMode[sceneInt], contractScenario.Instance.orderMode[sceneInt]);
				cList = contractScenario.Instance.showList;
			}
		}

		//Starts the rebuild timer when the contract list is loaded
		private void contractLoaded()
		{
			if (!contractsLoading && !loaded)
			{
				timer = 0;
				contractsLoading = true;
			}
		}

		#endregion

		#region Repeating Worker

		internal override void RepeatingWorker()
		{
			if (cList.Count > 0)
				refreshContracts(cList);
		}

		#endregion

		#region Persistence

		//Load window position and size settings
		private void PersistenceLoad()
		{
			if (contractScenario.Instance != null)
			{
				cList.Clear();
				WindowRect = contractScenario.Instance.windowRects[sceneInt];
				DragRect = new Rect(0, 0, WindowRect.width - 19, WindowRect.height - 24);
				Visible = contractScenario.Instance.windowVisible[sceneInt];
				TooltipsEnabled = contractScenario.Instance.toolTips;
				if (Visible)
					StartRepeatingWorker(5);
			}
		}

		#endregion

	}

	#region SortClass

	internal enum sortClass
	{
		Default = 1,
		Expiration = 2,
		Acceptance = 3,
		Difficulty = 4,
		Reward = 5,
		Type = 6,
	}

	#endregion
}
