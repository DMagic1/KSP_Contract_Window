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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProgressParser;
using ContractParser;
using Contracts;
using Contracts.Parameters;
using Contracts.Agents;
using ContractsWindow.Toolbar;
using UnityEngine;

namespace ContractsWindow
{

	class contractsWindow: DMC_MBW
	{

		#region Initialization

		private List<progressInterval> intervalNodes = new List<progressInterval>();
		private List<progressStandard> standardNodes = new List<progressStandard>();
		private List<progressStandard> POInodes = new List<progressStandard>();
		private List<progressBodyCollection> bodyNodes = new List<progressBodyCollection>();
		private List<List<progressStandard>> bodySubNodes = new List<List<progressStandard>>();
		private int progressMode = 0;
		private int selectedBody = 1;

		private List<Guid> cList = new List<Guid>();
		private List<Guid> pinnedList = new List<Guid>();
		private List<contractMission> missionList = new List<contractMission>();
		private List<contractUIObject> nextRemoveList = new List<contractUIObject>();
		private List<contractUIObject> nextRemoveMissionList = new List<contractUIObject>();
		private List<contractUIObject> nextPinnedList = new List<contractUIObject>();
		private contractMission currentMission;
		private contractUIObject tempContainer;
		private Agent currentAgent;
		private string version, inputField, editField;
		private Vector2 scroll, missionScroll;
		private bool resizing, editorLocked, spacecenterLocked, trackingLocked, progressLoaded, contractsLoaded, stockToolbar, replaceStock;
		private bool popup, showSort, rebuild, agencyPopup, missionCreator, missionTextBox, missionSelector, toolbar, missionEdit, replaceStockPopup;
		private bool showProgress, toggleProgress, oldToggleProgress;
		private bool useCustomNotes;
		private Vector2 dragStart;
		private float windowHeight, windowWidth;
		//private int timer;
		private Rect popupRect;
		private int sceneInt;
		private int timer;
		private const string lockID = "ContractsWindow_LockID";
		private const string centerLockID = "ContractsWindow_SC_LockID";
		private const string trackingLockID = "ContractsWindow_TS_LockID";

		private contractScenario contract = contractScenario.Instance;

		protected override void Awake()
		{
			Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
			var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			switch (ainfoV == null)
			{
				case true: version = ""; break;
				default: version = ainfoV.InformationalVersion; break;
			}

			sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);
			timer = 0;

			//Set up the various GUI options to their default values here
			WindowCaption = "Contracts +";
			WindowRect = new Rect(40, 80, 250, 300);
			WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
			WindowStyle = contractSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			ClampToScreen = true;
			ClampToScreenOffset = new RectOffset(-200, -200, -200, -200);
			TooltipMouseOffset = new Vector2d(-10, -25);
			RepeatingWorkerInitialWait = 10;

			//Make sure our click-through control locks are disabled
			InputLockManager.RemoveControlLock(lockID);

			DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
		}

		protected override void Start()
		{
			base.Start();

			contractParser.onContractStateChange.Add(contractAccepted);
			contractParser.onContractsParsed.Add(onContractsLoaded);
			progressParser.onProgressParsed.Add(onProgressLoaded);
			PersistenceLoad();
			useCustomNotes = HighLogic.LoadedSceneIsEditor || HighLogic.LoadedScene == GameScenes.SPACECENTER;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			contractParser.onContractStateChange.Remove(contractAccepted);
			contractParser.onContractsParsed.Remove(onContractsLoaded);
			progressParser.onProgressParsed.Remove(onProgressLoaded);
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
			if (InputLockManager.lockStack.ContainsKey(centerLockID))
				InputLockManager.RemoveControlLock(centerLockID);
			if (InputLockManager.lockStack.ContainsKey(trackingLockID))
				InputLockManager.RemoveControlLock(trackingLockID);
		}

		protected override void Update()
		{
			if (progressLoaded && contractsLoaded)
				return;

			//This is a backup loading system in case something blows up while the ContractSystem is loading
			if (timer < 500 && (!progressLoaded || !contractsLoaded))
				timer++;
			else if (!progressLoaded)
			{
				loadProgressLists();
				progressLoaded = true;
			}
			else if (!contractsLoaded)
			{
				loadLists();
				contractsLoaded = true;
			}
		}

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
		}

		private void onProgressLoaded()
		{
			StartCoroutine(loadProgressNodes());
		}

		private void loadLists()
		{
			generateList();

			//Load ordering lists and contract settings after primary contract dictionary has been loaded
			if (currentMission != null)
			{
				if (currentMission.ShowActiveMissions)
				{
					cList = currentMission.ActiveMissionList;
					pinnedList = currentMission.loadPinnedContracts(cList);
				}
				else
				{
					cList = currentMission.HiddenMissionList;
					pinnedList = currentMission.loadPinnedContracts(cList);
				}
			}			

			if (cList.Count > 0)
				refreshContracts(cList);
			else
				rebuildList();
		}

		private void loadProgressLists()
		{
			intervalNodes = progressParser.getAllIntervalNodes;
			standardNodes = progressParser.getAllStandardNodes;
			POInodes = progressParser.getAllPOINodes;
			bodyNodes = progressParser.getAllBodyNodes;

			bodySubNodes = new List<List<progressStandard>>(bodyNodes.Count);

			for (int i = 0; i < bodyNodes.Count; i++)
			{
				bodySubNodes.Add(bodyNodes[i].getAllNodes);	
			}
		}

		private IEnumerator loadContracts()
		{
			int i = 0;

			contractsLoaded = true;

			while (!contractParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			if (i >= 200)
			{
				contractsLoaded = false;
				yield break;
			}

			loadLists();
		}

		private IEnumerator loadProgressNodes()
		{
			int i = 0;

			progressLoaded = true;

			while (!progressParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			if (i >= 200)
			{
				progressLoaded = false;
				yield break;
			}

			loadProgressLists();
		}

		#endregion

		#region GUI Draw

		protected override void DrawWindowPre(int id)
		{
			//Update the drag rectangle
			DragRect.height = WindowRect.height - 24 - contractScenario.Instance.windowSize * 8;
			DragRect.width = WindowRect.width - 19;

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

			if (!popup)
			{
				showSort = false;
				rebuild = false;
				agencyPopup = false;
				missionCreator = false;
				missionSelector = false;
				missionTextBox = false;
				missionEdit = false;
				replaceStockPopup = false;
				toolbar = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			int windowSizeAdjust = contractScenario.Instance.windowSize;

			//Menu Bar
			buildMenuBar(id, windowSizeAdjust);

			GUILayout.BeginVertical();
			GUILayout.Space(20 + (windowSizeAdjust * 10));

			Rect lastRect = new Rect(26, 22 + (windowSizeAdjust * 6), 22 + (windowSizeAdjust * 4), 22 + (windowSizeAdjust * 4));

			if (!showProgress)
			{
				GUI.DrawTexture(lastRect, contractSkins.contractIcon);

				lastRect = new Rect(20, 20 + (windowSizeAdjust * 6), 180 + (windowSizeAdjust * 30), 26 + (windowSizeAdjust * 4));

				if (popup)
					GUI.Label(lastRect, "   " + currentMission.Name + ":", contractSkins.missionLabel);
				else
				{
					if (GUI.Button(lastRect, new GUIContent("   " + currentMission.Name + ":", "Go To Progress Records"), contractSkins.missionLabel))
					{
						toggleProgress = true;
					}
				}

				if (!currentMission.MasterMission)
				{
					lastRect.x += 180 + (windowSizeAdjust * 24);
					lastRect.y += 2 + (windowSizeAdjust * 2);
					lastRect.width = 20 + (windowSizeAdjust * 4);
					lastRect.height = 20 + (windowSizeAdjust * 4);

					if (GUI.Button(lastRect, new GUIContent(contractSkins.missionEditIcon, "Edit Mission")))
					{
						editField = currentMission.Name;
						popup = true;
						missionEdit = true;
					}
				}

				GUILayout.Space(7);

				scroll = GUILayout.BeginScrollView(scroll);

				lastRect = new Rect(0, -2, 10, 0);

				//Contract List Begins
				foreach (Guid gID in cList)
				{
					contractUIObject c = currentMission.getContract(gID);

					if (c == null)
						continue;

					if (c.Container == null)
						continue;

					GUILayout.Space(-1);

					buildContractTitleBar(c, id, windowSizeAdjust, ref lastRect);

					GUILayout.Space(-5);

					buildContractTitle(c, id, windowSizeAdjust, ref lastRect);

					//Parameters
					if (c.ShowParams)
					{
						foreach (parameterContainer cP in c.Container.ParamList)
						{
							if (cP.Level == 0 && !string.IsNullOrEmpty(cP.Title))
								buildParameterLabel(cP, c.Container, 0, id, windowSizeAdjust, ref lastRect);
						}
					}
				}

				GUILayout.EndScrollView();
				GUILayout.Space(18 + windowSizeAdjust * 4);
			}
			else
			{
				GUI.DrawTexture(lastRect, contractSkins.progressIcon);

				lastRect = new Rect(20, 20 + (windowSizeAdjust * 6), 180 + (windowSizeAdjust * 30), 26 + (windowSizeAdjust * 4));

				if (popup)
					GUI.Label(lastRect, "Progress Nodes:", contractSkins.missionLabel);
				else
				{
					if (GUI.Button(lastRect, new GUIContent("Progress Nodes:", "Go To Contracts"), contractSkins.missionLabel))
					{
						toggleProgress = false;
					}
				}

				GUILayout.Space(8);

				scroll = GUILayout.BeginScrollView(scroll);

				lastRect = new Rect(0, -2, 10, 0);

				buildIntervals(id, windowSizeAdjust, ref lastRect);

				buildStandards(id, windowSizeAdjust, ref lastRect);

				buildPOIs(id, windowSizeAdjust, ref lastRect);

				buildBodies(id, windowSizeAdjust, ref lastRect);

				GUILayout.EndScrollView();
				GUILayout.Space(18 + windowSizeAdjust * 4);
			}

			GUILayout.EndVertical();

			//Bottom bar
			buildBottomBar(id, windowSizeAdjust);

			//Draw various popup and dropdown windows
			buildPopup(id, windowSizeAdjust);

			//Resize window when the resizer is grabbed by the mouse
			buildResizer(id, windowSizeAdjust);
		}

		#region Top Menu

		private void buildMenuBar(int ID, int sizeAdjust)
		{
			Rect r = new Rect(4, 2, 26 + (sizeAdjust * 6), 18 + (sizeAdjust * 6));

			//Sort icons
			if (GUI.Button(r, new GUIContent(contractSkins.sortIcon, "Sort Contracts")))
			{
				popup = !popup;
				showSort = !showSort;
			}

			r.x += 36 + (sizeAdjust * 9);

			if (currentMission.AscendingOrder)
			{
				if (GUI.Button(r, new GUIContent(contractSkins.orderAsc, "Ascending Order")))
				{
					currentMission.AscendingOrder = false;
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(r, new GUIContent(contractSkins.orderDesc, "Descending Order")))
				{
					currentMission.AscendingOrder = true;
					refreshContracts(cList);
				}
			}

			r.x = WindowRect.width - 68 - (sizeAdjust * 9);

			//Show and hide icons
			if (currentMission.ShowActiveMissions)
			{
				if (GUI.Button(r, new GUIContent(contractSkins.revealShowIcon, "Show Hidden Contracts")))
				{
					currentMission.ShowActiveMissions = false;
					cList = currentMission.HiddenMissionList;
					pinnedList = currentMission.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(r, new GUIContent(contractSkins.revealHideIcon, "Show Standard Contracts")))
				{
					currentMission.ShowActiveMissions = true;
					cList = currentMission.ActiveMissionList;
					pinnedList = currentMission.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}

			r.x += 36 + (sizeAdjust * 6);

			//Mission selection icon
			if (GUI.Button(r, new GUIContent(contractSkins.missionSelectionIcon, "Select Mission")))
			{
				popup = true;
				missionSelector = true;
			}

			GUI.DrawTexture(new Rect(2, 17 + (sizeAdjust * 6), WindowRect.width - 4, 4), contractSkins.headerBar);
		}


		#endregion

		#region Contract Title Bar

		private void buildContractTitleBar(contractUIObject c, int id, int size, ref Rect r)
		{
			r.x = 6;
			r.y += (r.height + 2);
			r.width = 54 + (size * 12);
			r.height = 15 + (size * 4);

			if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (30 + size * 6)))
			{
				//Difficulty icons
				if (c.Container.Root.Prestige == Contract.ContractPrestige.Trivial)
					GUI.DrawTexture(r, contractSkins.goldStar);
				else if (c.Container.Root.Prestige == Contract.ContractPrestige.Significant)
					GUI.DrawTexture(r, contractSkins.goldStarTwo);
				else
					GUI.DrawTexture(r, contractSkins.goldStarThree);

				r.x += r.width;
				r.width = 58 + (size * 15);

				//Expiration date
				if (c.Container.Duration >= 2160000)
					GUI.Label(r, c.Container.DaysToExpire, contractSkins.timerGood);
				else if (c.Container.Duration > 0)
					GUI.Label(r, c.Container.DaysToExpire, contractSkins.timerBad);
				else if (c.Container.Root.ContractState == Contract.State.Completed)
					GUI.Label(r, c.Container.DaysToExpire, contractSkins.timerGood);
				else
					GUI.Label(r, c.Container.DaysToExpire, contractSkins.timerFinished);

				r.x += 60 + (size * 10);
				r.width = 16 + (size * 4);
				r.height += 1;

				//Draw icon buttons when no popup menus are open
				if (!popup)
				{
					//Agency Icon
					if (GUI.Button(r, new GUIContent(contractSkins.agencyIcon, "Agency"), contractSkins.texButtonSmall))
					{
						currentAgent = c.Container.Root.Agent;
						popup = !popup;
						agencyPopup = !agencyPopup;
					}

					r.x += 22 + (size * 4);

					//Show and hide icons
					if (c.Container.Root.ContractState == Contract.State.Active)
					{
						if (currentMission.ShowActiveMissions)
						{
							if (GUI.Button(r, new GUIContent(contractSkins.hideIcon, "Hide Contract"), contractSkins.texButtonSmall))
								nextRemoveList.Add(c);
						}
						else
						{
							if (GUI.Button(r, new GUIContent(contractSkins.showIcon, "Un-Hide Contract"), contractSkins.texButtonSmall))
								nextRemoveList.Add(c);
						}
					}
					else
					{
						if (GUI.Button(r, new GUIContent(contractSkins.closeIcon, "Remove Contract"), contractSkins.texButtonSmall))
							nextRemoveMissionList.Add(c);
					}

					r.x += 22 + (size * 4);

					//Pin icon button
					if (c.Order == null)
					{
						if (GUI.Button(r, new GUIContent(contractSkins.pinIcon, "Pin Contract"), contractSkins.texButtonSmall))
							nextPinnedList.Add(c);
					}
					else
					{
						r.width -= 2;
						if (GUI.Button(r, new GUIContent(contractSkins.pinDownIcon, "Un-Pin Contract"), contractSkins.texButtonSmall))
							nextPinnedList.Add(c);
					}

					r.x += 22 + (size * 4);
					r.width = 16 + (size * 4);

					//Mission list button
					if (GUI.Button(r, new GUIContent(contractSkins.missionIcon, "Add To Mission List"),contractSkins.texButtonSmall))
					{
						tempContainer = c;
						popup = true;
						missionCreator = true;
					}

					r.width = 12 + (size * 4);
					r.x += 18 + (size * 4);

					//Note icon button
					if (c.Container.Root.ContractState == Contract.State.Active && !string.IsNullOrEmpty(c.Container.Notes))
					{
						if (!c.Container.ShowNote)
						{
							if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Note"), contractSkins.texButtonSmall))
								c.Container.ShowNote = !c.Container.ShowNote;
						}
						else
						{
							if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Note"), contractSkins.texButtonSmall))
								c.Container.ShowNote = !c.Container.ShowNote;
						}
					}
				}
				//Draw inactive icons while any popup menu is active
				else
				{
					//Agency Icon
					GUI.Label(r, contractSkins.agencyIcon, contractSkins.texButtonSmall);

					r.x += 22 + (size * 4);

					//Show and hide icons
					if (c.Container.Root.ContractState == Contract.State.Active)
					{
						if (currentMission.ShowActiveMissions)
							GUI.Label(r, contractSkins.hideIcon, contractSkins.texButtonSmall);
						else
							GUI.Label(r, contractSkins.showIcon, contractSkins.texButtonSmall);
					}
					else
						GUI.Label(r, contractSkins.closeIcon, contractSkins.texButtonSmall);

					r.x += 22 + (size * 4);

					//Pin icon button
					if (c.Order == null)
						GUI.Label(r, contractSkins.pinIcon, contractSkins.texButtonSmall);
					else
					{
						r.width -= 2;
						GUI.Label(r, contractSkins.pinDownIcon, contractSkins.texButtonSmall);
					}

					r.x += 22 + (size * 4);
					r.width = 16 + (size * 4);

					//Mission list button
					GUI.Label(r, contractSkins.missionIcon, contractSkins.texButtonSmall);

					r.x += 18 + (size * 4);
					r.width = 12 + (size * 4);

					//Note icon button
					if (c.Container.Root.ContractState == Contract.State.Active && !string.IsNullOrEmpty(c.Container.Notes))
					{
						if (!c.Container.ShowNote)
							GUI.Label(r, contractSkins.noteIcon, contractSkins.texButtonSmall);
						else
							GUI.Label(r, contractSkins.noteIconOff, contractSkins.texButtonSmall);
					}
				}
			}
		}

		#endregion

		#region Contract Titles

		private void buildContractTitle(contractUIObject c, int id, int size, ref Rect r)
		{
			string contractTitle = c.Container.Title;
			GUIStyle cStyle = titleState(c.Container.Root.ContractState);
			bool active = c.Container.Root.ContractState == Contract.State.Active || c.Container.Root.ContractState == Contract.State.Completed;
			bool failed = c.Container.Root.ContractState == Contract.State.Active || c
				.Container.Root.ContractState == Contract.State.Cancelled || c.Container.Root.ContractState == Contract.State.DeadlineExpired || c.Container.Root.ContractState == Contract.State.Failed;

			//Add in space for the contract title buttons
			GUILayout.Space(23 + size * 4);

			//Draw inactive buttons if any popup window is open
			if (!popup)
			{
				if (GUILayout.Button(contractTitle, cStyle, GUILayout.MaxWidth(225 + size * 30)))
					c.ShowParams = !c.ShowParams;
			}
			else
				GUILayout.Box(contractTitle, hoverTitleState(c.Container.Root.ContractState), GUILayout.MaxWidth(225 + size * 30));

			r = GUILayoutUtility.GetLastRect();

			//Only draw the rewards if they are visible in the window
			if (WindowRect.width >= 270 + (size* 30))
			{
				if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (20 + size * 6)))
				{
					Rect rewardsRect = r;
					rewardsRect.x = 230 + (size * 30);
					rewardsRect.y -= (2 + (size *2));

					scaledContent(ref rewardsRect, c.Container.FundsRewString, c.Container.FundsPenString, Currency.Funds, size, active, failed);

					scaledContent(ref rewardsRect, c.Container.SciRewString, "", Currency.Science, size, active, failed);

					scaledContent(ref rewardsRect, c.Container.RepRewString, c.Container.RepPenString, Currency.Reputation, size, active, failed);
				}
			}

			//Display note
			if (!string.IsNullOrEmpty(c.Container.Notes) && c.Container.ShowNote && c.Container.Root.ContractState == Contract.State.Active)
			{
				GUILayout.Space(-3);
				GUILayout.Box(c.Container.Notes, GUILayout.MaxWidth(300 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}
		}

		#endregion

		#region Parameters

		private void buildParameterLabel(parameterContainer cP, contractContainer c, int level, int id, int size, ref Rect r)
		{
			string paramTitle = cP.Title;
			bool active = cP.CParam.State == ParameterState.Incomplete;
			bool greenState = cP.CParam.State == ParameterState.Complete || cP.CParam.State == ParameterState.Incomplete;
			bool redState = cP.CParam.State == ParameterState.Incomplete || cP.CParam.State == ParameterState.Failed;
			GUIStyle pStyle = paramState(cP);

			GUILayout.BeginHorizontal();
			GUILayout.Space(5 + (level * 5));

			r.x = 5 + (level * 5);
			r.y += r.height;

			//Note icon button
			if (active && !string.IsNullOrEmpty(cP.Notes(useCustomNotes)))
			{
				r.x -= 2;
				r.y += 4;
				r.width = 12 + (size * 2);
				r.height = 14 + (size * 4);

				if (!cP.ShowNote)
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Note"), contractSkins.texButtonSmall))
						cP.ShowNote = !cP.ShowNote;
				}
				else
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Note"), contractSkins.texButtonSmall))
						cP.ShowNote = !cP.ShowNote;
				}
				GUILayout.Space(12 + size * 2);
			}

			/* FIXME - Disabled For Now; Need to Figure Out Changes Made In 0.90 */
			//Editor part icon button
			//if (cP.part != null && HighLogic.LoadedSceneIsEditor)
			//{
			//	if (GUILayout.Button(new GUIContent(contractSkins.partIcon, "Preview Part"), contractSkins.texButtonSmall, GUILayout.MaxWidth(18 + contractScenario.Instance.windowSize * 4), GUILayout.MaxHeight(18 + contractScenario.Instance.windowSize * 4)))
			//	{
			//		EditorLogic.fetch.Unlock(lockID);
			//		editorLocked = false;
			//		EditorPartList.Instance.RevealPart(cP.part, true);
			//	}
			//	GUILayout.Space(-3);
			//}

			//Contract parameter title
			if (!string.IsNullOrEmpty(cP.Notes(useCustomNotes)))
				GUILayout.Box(paramTitle, pStyle, GUILayout.MaxWidth(208 - (level * 5) + size * 28));
			else
				GUILayout.Box(paramTitle, pStyle, GUILayout.MaxWidth(220 - (level * 5) + size * 30));

			r = GUILayoutUtility.GetLastRect();

			GUILayout.EndHorizontal();

			//Parameter reward info
			if (WindowRect.width >= 270 + (size * 30))
			{
				if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (30 + size * 6)))
				{
					Rect rewardsRect = r;
					rewardsRect.x = 230 + (size * 30);
					rewardsRect.y += 4;

					scaledContent(ref rewardsRect, cP.FundsRewString, cP.FundsPenString, Currency.Funds, size, greenState, redState);

					scaledContent(ref rewardsRect, cP.SciRewString, "", Currency.Science, size, greenState, redState);

					scaledContent(ref rewardsRect, cP.RepRewString, cP.RepPenString, Currency.Reputation, size, greenState, redState);
				}
			}

			//Display note
			if (!string.IsNullOrEmpty(cP.Notes(useCustomNotes)) && cP.ShowNote && active)
			{
				GUILayout.Space(-6);
				GUILayout.Box(cP.Notes(useCustomNotes), GUILayout.MaxWidth(320 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}

			if (level < 4)
			{
				foreach (parameterContainer sP in cP.ParamList)
				{
					if (sP.Level == level + 1 && !string.IsNullOrEmpty(sP.Title))
					{
						if (active)
							buildParameterLabel(sP, c, level + 1, id, size, ref r);
					}
				}
			}

		}

		#endregion

		#region Rewards

		private void scaledContent(ref Rect r, string top, string bottom, Currency type, int i, bool active, bool failed)
		{
			bool t = string.IsNullOrEmpty(top);
			bool b = string.IsNullOrEmpty(bottom);
			if (t && b)
				return;

			int width = 0;
			int height = 0;
			if (type == Currency.Funds)
			{
				width = 8 + (i * 2);
				height = 11 + (i * 5);
			}
			else
			{
				width = 12 + (i * 4);
				height = 12 + (i * 4);
			}

			r.width = width;
			r.height = height;

			GUIStyle sTop = currencyStyle(type, true);
			Vector2 szTop = sTop.CalcSize(new GUIContent(top));
			if (!t && active)
			{
				GUI.DrawTexture(r, currencyIcon(type, true));

				r.x += r.width + 2 + (i * 2);

				r.width = szTop.x;
				r.height = szTop.y;

				GUI.Label(r, top, sTop);
			}

			r.width = width;
			r.height = height;

			GUIStyle sBot = currencyStyle(type, false);
			Vector2 szBot = sBot.CalcSize(new GUIContent(bottom));
			if (!b && failed)
			{
				r.x -= (width + 2 + (i * 2));
				r.y += 14 + (i * 5);
				GUI.DrawTexture(r, currencyIcon(type, false));

				r.x += r.width + 2 + (i * 2);

				r.width = szBot.x;
				r.height = szBot.y;

				GUI.Label(r, bottom, sBot);
				r.y -= (14 + (i * 5));
			}

			r.x += Math.Max(szTop.x, szBot.x) + 4 + (i * 4);
		}

		private GUIStyle currencyStyle(Currency t, bool reward)
		{
			switch (t)
			{
				case Currency.Funds:
					return reward ? contractSkins.reward : contractSkins.penalty;
				case Currency.Reputation:
					return reward ? contractSkins.repReward : contractSkins.repPenalty;
				default:
					return contractSkins.scienceReward;
			}
		}

		private Texture2D currencyIcon(Currency t, bool reward)
		{
			switch (t)
			{
				case Currency.Funds:
					return reward ? contractSkins.fundsGreen : contractSkins.fundsRed;
				case Currency.Reputation:
					return reward ? contractSkins.repGreen : contractSkins.repRed;
				default:
					return contractSkins.science;
			}
		}

		#endregion

		#region Popups

		private void buildPopup(int id, int size)
		{
			if (popup)
			{
				if (showSort)
				{
					popupRect = new Rect(10, 20, 80 + size * 15, 132 + size * 23);
					GUI.Box(popupRect, "", contractSkins.dropDown);

					var sortTypes = Enum.GetNames(typeof(sortClass));

					for (int i = 0; i < sortTypes.Length; i++)
					{
						if (GUI.Button(new Rect(popupRect.x + 2, popupRect.y + 2 + ((21 + size * 4) * i), popupRect.width - 4, 20 + size * 5), sortTypes[i], contractSkins.sortMenu))
						{
							showSort = false;
							currentMission.OrderMode = (sortClass)Enum.Parse(typeof(sortClass), sortTypes[i]);
							refreshContracts(cList);
							toggleProgress = false;
						}
					}
				}

				else if (rebuild)
				{
					popupRect = new Rect(10, WindowRect.height - 180, 230, 150);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					GUI.Label(new Rect(popupRect.x + 7, popupRect.y + 10, popupRect.width - 14, 100), "Rebuild\nContracts Window + Display:\n\n<b>Will Not</b> Affect Contract Status", contractSkins.resetBox);
					if (GUI.Button(new Rect(popupRect.x + 20, popupRect.y + 110, popupRect.width - 40, 25), "Reset Display", contractSkins.resetButton))
					{
						LogFormatted("Rebuilding Contract Window List");
						rebuildList();
						resetWindow();
						popup = false;
						rebuild = false;
						toggleProgress = false;
					}
				}

				else if (agencyPopup)
				{
					popupRect = new Rect(10, 40, 230 + size * 20, 80);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					Rect r = new Rect(popupRect.x + 5, popupRect.y + 10, 84, 60);
					GUI.Box(r, "", contractSkins.agentBackground);
					r.x += 10;
					r.y += 10;
					r.width = 64;
					r.height = 40;
					GUI.Label(r, currentAgent.LogoScaled);
					r.x += 85;
					r.y -= 10;
					r.width = 120 + size * 20;
					r.height = 60;
					GUI.Label(r, currentAgent.Name, contractSkins.agentName);
				}

				else if (missionCreator)
				{
					popupRect = new Rect(20, 30, 210 + size * 20, 200);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					if (!missionTextBox)
					{
						for (int i = 0; i < missionList.Count; i++)
						{
							missionScroll = GUI.BeginScrollView(popupRect, missionScroll, new Rect(0, 0, 190 + size * 20, 25 * missionList.Count));
							Rect r = new Rect(2, (25 * i) + 2, 140 + size * 20, 25);
							if (i == 0)
							{
								if (GUI.Button(r, "Create New Mission", contractSkins.missionMenuCenter))
								{
									inputField = "";
									missionTextBox = true;
									popup = true;
								}
							}
							else
							{
								contractMission m = missionList[i];
								bool containsContract = m.containsContract(tempContainer.Container.Root.ContractGuid);

								r.x += 15;

								if (containsContract)
								{
									GUI.DrawTexture(new Rect(r.x - 15, r.y + 6, 12 + size * 2, 10 + size * 2), contractSkins.checkIcon);

									GUI.Label(r, m.Name, contractSkins.missionMenu);
								}
								else
								{
									if (GUI.Button(r, m.Name, contractSkins.missionMenu))
									{
										m.addContract(tempContainer.Container, true, true);
										popup = false;
										missionCreator = false;
										toggleProgress = false;
									}
								}

								r.x += 145 + size * 18;
								r.y += 4;
								r.width = 15 + size * 5;

								GUI.Label(r, m.ActiveContracts.ToString(), contractSkins.timerGood);

								if (!m.MasterMission && containsContract)
								{
									r.x += 15 + size * 2;
									r.width = 14 + size * 4;
									r.height = 14 + size * 4;

									if (GUI.Button(r, new GUIContent(contractSkins.cancelMissionIcon, "Remove From Mission List"), contractSkins.texButtonSmall))
									{
										if (m == currentMission)
											nextRemoveMissionList.Add(tempContainer);
										else
											m.removeContract(tempContainer.Container);
										toggleProgress = false;
									}
								}
							}
							GUI.EndScrollView();
						}
					}
					else
					{
						Rect r = new Rect(popupRect.x + 2, popupRect.y + 2, 150 + size * 20, 25);
						GUI.Label(r, "Create New Mission", contractSkins.missionMenuCenter);

						r.y += 30;

						inputField = GUI.TextField(r, inputField, 20);

						r.y += 30;

						if (GUI.Button(r, "Create Mission", contractSkins.resetButton))
						{
							if (!string.IsNullOrEmpty(inputField))
							{
								if (contractScenario.Instance.addMissionList(inputField))
								{
									contractMission cM = contractScenario.Instance.getMissionList(inputField);
									if (cM != null)
										cM.addContract(tempContainer.Container, true, true);
									missionList = contractScenario.Instance.getAllMissions();
									popup = false;
									missionTextBox = false;
									missionCreator = false;
									toggleProgress = false;
								}
							}
							else
								ScreenMessages.PostScreenMessage("Contract mission list must have a name", 5f, ScreenMessageStyle.UPPER_CENTER);
						}
					}
				}

				else if (missionSelector)
				{
					popupRect = new Rect(20, 30, 180 + size * 20, 200);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					for (int i = 0; i < missionList.Count; i++)
					{
						missionScroll = GUI.BeginScrollView(popupRect, missionScroll, new Rect(0, 0, 160 + size * 20, 25 * missionList.Count));
						Rect r = new Rect(2, (25 * i) + 2, 140 + size * 20, 25);
						contractMission m = missionList[i];
						if (GUI.Button(r, m.Name, contractSkins.missionMenu))
						{
							currentMission = contractScenario.Instance.setCurrentMission(m.Name);

							if (currentMission.ShowActiveMissions)
								cList = currentMission.ActiveMissionList;
							else
								cList = currentMission.HiddenMissionList;

							pinnedList = currentMission.loadPinnedContracts(cList);

							refreshContracts(cList);

							popup = false;
							missionSelector = false;
							toggleProgress = false;
						}
						r.x += 145 + size * 18;
						r.y += 4;
						r.width = 15 + size * 5;
						GUI.Label(r, m.ActiveContracts.ToString(), contractSkins.timerGood);
						GUI.EndScrollView();
					}
				}

				else if (toolbar)
				{
					popupRect = new Rect(10, WindowRect.height - 170, 230 + (size * 20), 140);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					Rect r = new Rect(popupRect.x + 10, popupRect.y + 10, popupRect.width - 20, 30);
					GUI.Label(r, "Toolbar Options:", contractSkins.resetBox);

					r.y += 30;

					if (ToolbarManager.ToolbarAvailable)
					{
						contractScenario.Instance.stockToolbar = GUI.Toggle(r, contractScenario.Instance.stockToolbar, " Use Stock Toolbar");
						r.y += 30;
					}

					//if (stockToolbar || !ToolbarManager.ToolbarAvailable)
					//{
					//	contractScenario.Instance.replaceStockToolbar = GUI.Toggle(r, contractScenario.Instance.replaceStockToolbar, " Replace Stock Toolbar");
					//	r.y += 30;
					//}

					r.x += 70;
					r.width = 70;

					if (GUI.Button(r, "Close", contractSkins.resetButton))
					{
						toolbar = false;
						popup = false;
					}
				}

				else if (replaceStockPopup)
				{
					popupRect = new Rect(10, WindowRect.height - 195, 230 + (size * 20), 165);
					GUI.Box(popupRect, "", contractSkins.dropDown);
					Rect r = new Rect(popupRect.x + 5, popupRect.y + 5, popupRect.width - 10, 90);
					GUI.Label(r, "Warning:\nReplacing Stock Contracts App May Produce Errors\nUse This Option\nAt Your Own Risk", contractSkins.resetBox);

					r.y += 95;
					r.height = 30;

					contractScenario.Instance.replaceStockWarned = GUI.Toggle(r, contractScenario.Instance.replaceStockWarned, "Do Not Display This Warning");

					r.x += 70;
					r.y += 30;
					r.width = 75;

					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						popup = false;
						replaceStockPopup = false;
						if (contractScenario.Instance.appLauncherButton != null)
						{
							contractScenario.Instance.appLauncherButton.replaceStockApp();
							replaceStock = true;
							contractScenario.Instance.replaceStockToolbar = true;
						}
						else
							LogFormatted("Error In Setting Stock App Launcher Button...");
					}
				}

				else if (missionEdit)
				{
					popupRect = new Rect(20, 40, 180 + size * 20, 200);
					GUI.Box(popupRect, "", contractSkins.dropDown);

					Rect r = new Rect(popupRect.x + 2, popupRect.y + 2, popupRect.width - 14, 30);
					GUI.Label(r, "Edit Mission Title", contractSkins.resetBox);

					r.x += 10;
					r.y += 30;
					r.width = 150 + size * 20;
					r.height = 25;

					editField = GUI.TextField(r, editField, 20);

					r.y += 30;

					if (GUI.Button(r, "Change Name", contractSkins.resetButton))
					{
						if (!string.IsNullOrEmpty(editField))
						{
							string oldName = currentMission.Name;
							contractScenario.Instance.removeMissionList(oldName, false);

							currentMission.Name = editField;

							if (contractScenario.Instance.addMissionList(currentMission))
							{
								missionList = contractScenario.Instance.getAllMissions();
								currentMission = contractScenario.Instance.setCurrentMission(editField);

								if (currentMission.ShowActiveMissions)
									cList = currentMission.ActiveMissionList;
								else
									cList = currentMission.HiddenMissionList;

								pinnedList = currentMission.loadPinnedContracts(cList);

								refreshContracts(cList);

								popup = false;
								missionEdit = false;
								toggleProgress = false;
							}
							else
								currentMission.Name = oldName;
						}
						else
							ScreenMessages.PostScreenMessage("Contract mission list must have a name", 5f, ScreenMessageStyle.UPPER_CENTER);
					}

					r.x -= 10;
					r.y += 30;
					r.width = popupRect.width - 14;
					r.height = 60;

					GUI.Label(r, "Delete Current Mission:\n<b>Will Not</b> Affect Contract Status", contractSkins.resetBox);

					r.x += 10;
					r.y += 60;
					r.width -= 25;
					r.height = 25;

					if (GUI.Button(r, "Delete Mission", contractSkins.resetButton))
					{
						contractScenario.Instance.removeMissionList(currentMission.Name);
						missionList = contractScenario.Instance.getAllMissions();
						currentMission = contractScenario.Instance.MasterMission;

						if (currentMission.ShowActiveMissions)
							cList = currentMission.ActiveMissionList;
						else
							cList = currentMission.HiddenMissionList;

						pinnedList = currentMission.loadPinnedContracts(cList);

						refreshContracts(cList);

						popup = false;
						missionEdit = false;
					}
				}

				else
					popup = false;
			}
		}

		#endregion

		#region Bottom Bar

		private void buildBottomBar(int id, int size)
		{
			Rect r = new Rect(2, WindowRect.height - 30 - (size * 4), WindowRect.width - 4, 4);
			GUI.DrawTexture(r, contractSkins.footerBar);

			//Version label
			r.x = 8;
			r.y = WindowRect.height - 23 - (size * 4);
			r.width = 30 + size * 4;
			r.height = 20 + size * 4;
			GUI.Label(r, version, contractSkins.paramText);

			//Tooltip toggle icon
			r.x = 36 + size * 4;
			r.y -= 2;
			r.height += 2;
			if (GUI.Button(r, new GUIContent(contractSkins.tooltipIcon, "Toggle Tooltips")))
			{
				TooltipsEnabled = !TooltipsEnabled;
				contractScenario.Instance.toolTips = TooltipsEnabled;
			}

			//Clear list button
			r.x = 74 + size * 10;
			if (GUI.Button(r, new GUIContent(contractSkins.resetIcon, "Reset Contracts Window Display")))
			{
				popup = !popup;
				rebuild = !rebuild;
			}

			//Font size button
			r.x = 112 + size * 16;
			if (GUI.Button(r, new GUIContent(contractSkins.fontSize, "Toggle Font Size")))
			{
				if (contractSkins.normalFontSize == 0)
					contractSkins.normalFontSize = 1;
				else
					contractSkins.normalFontSize = 0;
				contractSkins.initializeSkins();
				WindowStyle = contractSkins.newWindowStyle;
				DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
				contractScenario.Instance.fontSmall = !contractScenario.Instance.fontSmall;
			}

			//Window size button
			r.x = 150 + size * 22;
			if (GUI.Button(r, new GUIContent(contractSkins.windowSize, "Change Window Size")))
			{
				contractScenario.Instance.windowSize += 1;

				if (contractScenario.Instance.windowSize > 2)
				{
					contractScenario.Instance.windowSize = 0;
					WindowRect.width -= 60;
				}
				else
					WindowRect.width += 30;

				contractSkins.windowFontSize = 2 * contractScenario.Instance.windowSize;

				//if (contractScenario.Instance.windowSize == 0)
				//{
				//	contractScenario.Instance.windowSize = 1;
				//	contractSkins.windowFontSize = 2;
				//	WindowRect.width += 30;
				//}
				//else
				//{
				//	contractScenario.Instance.windowSize = 0;
				//	contractSkins.windowFontSize = 0;
				//	WindowRect.width -= 30;
				//}

				contractSkins.initializeSkins();
				WindowStyle = contractSkins.newWindowStyle;
				DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
			}

			//Toolbar options button
			r.x = 188 + size * 28;
			if (GUI.Button(r, new GUIContent(contractSkins.settingsIcon, "Toolbar Options")))
			{
				popup = true;
				toolbar = true;
			}
		}

		#endregion

		#region Resizer

		private void buildResizer(int id, int size)
		{
			Rect resizer = new Rect(WindowRect.width - 25 - size * 3, WindowRect.height - 25 - size * 3, 22 + size * 4, 22 + size * 4);
			GUI.Label(resizer, contractSkins.expandIcon, contractSkins.dragButton);
			if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
			{
				if (resizer.Contains(Event.current.mousePosition))
				{
					resizing = true;
					dragStart.x = Input.mousePosition.x;
					dragStart.y = Input.mousePosition.y;
					windowHeight = WindowRect.height;
					windowWidth = WindowRect.width;
					Event.current.Use();
				}
			}
			if (resizing)
			{
				if (Input.GetMouseButtonUp(0))
				{
					resizing = false;
					WindowRect.yMax = WindowRect.y + windowHeight;
					WindowRect.xMax = WindowRect.x + windowWidth;
				}
				else
				{
					float height = Input.mousePosition.y;
					float width = Input.mousePosition.x;
					if (Input.mousePosition.y < 0)
						height = 0;
					if (Input.mousePosition.x < 0)
						width = 0;
					windowHeight += dragStart.y - height;
					dragStart.y = height;
					windowWidth += width - dragStart.x;
					dragStart.x = width;
					WindowRect.yMax = WindowRect.y + windowHeight;
					WindowRect.xMax = WindowRect.x + windowWidth;
					if (WindowRect.yMax > Screen.height)
					{
						WindowRect.yMax = Screen.height;
						windowHeight = WindowRect.yMax - WindowRect.y;
					}
					if (WindowRect.xMax > Screen.width)
					{
						WindowRect.xMax = Screen.width;
						windowWidth = WindowRect.xMax - WindowRect.x;
					}
					if (windowHeight < 200)
						windowHeight = 200;
					if (windowWidth < 250 + (size * 30))
						windowWidth = 250 + (size * 30);
					if (windowWidth > 540 + (size * 100))
						windowWidth = 540 + (size * 100);
				}
			}
		}

		#endregion

		#region Progress Nodes

		private void buildIntervals(int id, int size, ref Rect r)
		{
			if (progressParser.AnyInterval)
			{
				if (popup)
					GUILayout.Label("Interval Nodes:", contractSkins.progressTitleBehind, GUILayout.MaxWidth(200 + size * 30));
				else
				{
					if (GUILayout.Button("Interval Nodes:", contractSkins.progressTitle, GUILayout.MaxWidth(200 + size * 30)))
					{
						progressMode = 0;
					}
				}

				if (progressMode == 0)
				{
					for (int i = 0; i < intervalNodes.Count; i++)
					{
						progressInterval p = intervalNodes[i];

						if (p == null)
							continue;

						if (p.Interval <= 1)
							continue;

						buildIntervalNode(id, p, size, ref r);
					}
				}
			}
		}

		private void buildStandards(int id, int size, ref Rect r)
		{
			if (progressParser.AnyStandard)
			{
				if (popup)
					GUILayout.Label("Standard Nodes:", contractSkins.progressTitleBehind, GUILayout.MaxWidth(200 + size * 30));
				else
				{
					if (GUILayout.Button("Standard Nodes:", contractSkins.progressTitle, GUILayout.MaxWidth(200 + size * 30)))
					{
						progressMode = 1;
					}
				}

				r = GUILayoutUtility.GetLastRect();

				if (progressMode == 1)
				{
					for (int i = 0; i < standardNodes.Count; i++)
					{
						progressStandard p = standardNodes[i];

						if (p == null)
							continue;

						if (!p.IsComplete)
							continue;

						buildStandardNode(id, p, size, ref r);
					}
				}
			}
		}

		private void buildPOIs(int id, int size, ref Rect r)
		{
			if (progressParser.AnyPOI)
			{
				if (popup)
					GUILayout.Label("Point Of Interest Nodes:", contractSkins.progressTitleBehind, GUILayout.MaxWidth(200 + size * 30));
				else
				{
					if (GUILayout.Button("Point Of Interest Nodes:", contractSkins.progressTitle, GUILayout.MaxWidth(200 + size * 30)))
					{
						progressMode = 2;
					}
				}

				r = GUILayoutUtility.GetLastRect();

				if (progressMode == 2)
				{
					for (int i = 0; i < POInodes.Count; i++)
					{
						progressStandard p = POInodes[i];

						if (p == null)
							continue;

						if (!p.IsComplete)
							continue;

						buildPOINode(id, p, size, ref r);
					}
				}
			}
		}

		private void buildBodies(int id, int size, ref Rect r)
		{
			if (progressParser.AnyBody)
			{
				if (popup)
					GUILayout.Label("Celestial Body Nodes:", contractSkins.progressTitleBehind, GUILayout.MaxWidth(200 + size * 30));
				else
				{
					if (GUILayout.Button("Celestial Body Nodes:", contractSkins.progressTitle, GUILayout.MaxWidth(200 + size * 30)))
					{
						progressMode = 3;
					}
				}

				if (progressMode == 3)
				{
					for (int i = 0; i < bodyNodes.Count; i++)
					{
						progressBodyCollection p = bodyNodes[i];

						if (p == null)
							continue;

						if (!p.IsReached)
							continue;

						buildBodyNode(id, i, p, size, ref r);
					}
				}
			}
		}

		private void buildIntervalNode(int id, progressInterval p, int size, ref Rect r)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);

			if (popup)
				GUILayout.Label(p.Descriptor, contractSkins.progressBodyTitleBehind, GUILayout.MaxWidth(160 + size * 30));
			else
			{
				if (GUILayout.Button(p.Descriptor, contractSkins.progressBodyTitle, GUILayout.MaxWidth(160 + size * 30)))
				{
					p.ShowRecords = !p.ShowRecords;
				}
			}
			GUILayout.EndHorizontal();

			if (p.ShowRecords)
			{
				for (int i = 1; i < p.Interval; i++)
				{
					GUILayout.Label(p.Descriptor + " Record " + i.ToString() + ": " + p.getRecord(i).ToString(), contractSkins.progressNodeTitle, GUILayout.MaxWidth(165 + size * 30));

					r = GUILayoutUtility.GetLastRect();

					//Only draw the rewards if they are visible in the window
					if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (20 + size * 6)))
					{
						Rect rewardsRect = r;
						rewardsRect.x = 180 + (size * 30);
						rewardsRect.y += (2 + (size * 2));

						scaledContent(ref rewardsRect, p.getFundsString(i), "", Currency.Funds, size, true, false);

						scaledContent(ref rewardsRect, p.getScienceString(i), "", Currency.Science, size, true, false);

						scaledContent(ref rewardsRect, p.getRepString(i), "", Currency.Reputation, size, true, false);
					}
				}
			}
		}

		private void buildStandardNode(int id, progressStandard p, int size, ref Rect r, string s = "")
		{
			GUILayout.BeginHorizontal();

			r.x = 3;

			if (!string.IsNullOrEmpty(p.Note))
			{
				r.y += r.height + 4;
				r.width = 12 + (size * 2);
				r.height = 14 + (size * 4);

				if (!p.ShowNotes)
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Completion Note"), contractSkins.texButtonSmall))
						p.ShowNotes = !p.ShowNotes;
				}
				else
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Completion Note"), contractSkins.texButtonSmall))
						p.ShowNotes = !p.ShowNotes;
				}
				GUILayout.Space(16 + size * 2);
			}

			GUILayout.Label(string.Format(p.Descriptor, s), contractSkins.progressNodeTitle, GUILayout.MaxWidth(165 + size * 30));

			r = GUILayoutUtility.GetLastRect();

			GUILayout.EndHorizontal();

			//Only draw the rewards if they are visible in the window
			if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (20 + size * 6)))
			{
				Rect rewardsRect = r;
				rewardsRect.x = 180 + (size * 30);
				rewardsRect.y += (2 + (size * 2));

				scaledContent(ref rewardsRect, p.FundsRewardString, "", Currency.Funds, size, true, false);

				scaledContent(ref rewardsRect, p.SciRewardString, "", Currency.Science, size, true, false);

				scaledContent(ref rewardsRect, p.RepRewardString, "", Currency.Reputation, size, true, false);
			}

			//Display note
			if (!string.IsNullOrEmpty(p.Note) && p.ShowNotes)
			{
				GUILayout.Space(-7);
				GUILayout.Box(string.Format(p.Note, p.NoteReference, p.KSPDateString), GUILayout.MaxWidth(210 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}
		}

		private void buildPOINode(int id, progressStandard p, int size, ref Rect r)
		{
			GUILayout.BeginHorizontal();

			r.x = 3;

			if (!string.IsNullOrEmpty(p.Note))
			{
				r.y += r.height + 4;
				r.width = 12 + (size * 2);
				r.height = 14 + (size * 4);

				if (!p.ShowNotes)
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Completion Note"), contractSkins.texButtonSmall))
						p.ShowNotes = !p.ShowNotes;
				}
				else
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Completion Note"), contractSkins.texButtonSmall))
						p.ShowNotes = !p.ShowNotes;
				}
				GUILayout.Space(12 + size * 2);
			}

			GUILayout.Label(p.Descriptor, contractSkins.progressNodeTitle, GUILayout.MaxWidth(165 + size * 30));

			r = GUILayoutUtility.GetLastRect();

			GUILayout.EndHorizontal();

			//Only draw the rewards if they are visible in the window
			if (r.yMin >= (scroll.y - 20) && r.yMax <= (scroll.y + WindowRect.height - (20 + size * 6)))
			{
				Rect rewardsRect = r;
				rewardsRect.x = 180 + (size * 30);
				rewardsRect.y += (2 + (size * 2));

				scaledContent(ref rewardsRect, p.FundsRewardString, "", Currency.Funds, size, true, false);

				scaledContent(ref rewardsRect, p.SciRewardString, "", Currency.Science, size, true, false);

				scaledContent(ref rewardsRect, p.RepRewardString, "", Currency.Reputation, size, true, false);
			}

			//Display note
			if (!string.IsNullOrEmpty(p.Note) && p.ShowNotes)
			{
				GUILayout.Space(-7);
				GUILayout.Box(string.Format(p.Note, p.NoteReference, p.KSPDateString), GUILayout.MaxWidth(210 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}
		}

		private void buildBodyNode(int id, int index, progressBodyCollection p, int size, ref Rect r)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(20);

			if (popup)
				GUILayout.Label(p.Body.bodyName, contractSkins.progressBodyTitleBehind, GUILayout.MaxWidth(160 + size * 30));
			{
				if (GUILayout.Button(p.Body.bodyName, contractSkins.progressBodyTitle, GUILayout.MaxWidth(160 + size * 30)))
				{
					selectedBody = index;
				}
			}

			r = GUILayoutUtility.GetLastRect();

			GUILayout.EndHorizontal();

			if (selectedBody != index)
				return;

			for (int i = 0; i < bodySubNodes[index].Count; i++)
			{
				progressStandard s = bodySubNodes[index][i];

				if (s == null)
					continue;

				if (!s.IsComplete)
					continue;

				buildStandardNode(id, s, size, ref r, p.Body.theName);
			}
		}

		#endregion

		protected override void DrawWindowPost(int id)
		{
			if (oldToggleProgress != toggleProgress)
			{
				oldToggleProgress = toggleProgress;

				showProgress = toggleProgress;
			}

			//Pin contracts after the window is drawn
			if (nextPinnedList.Count > 0)
			{
				foreach(contractUIObject c in nextPinnedList)
				{
					if (contractScenario.ListRemove(pinnedList, c.Container.Root.ContractGuid))
						c.Order = null;
					else
					{
						c.Order = pinnedList.Count;
						pinnedList.Add(c.Container.Root.ContractGuid);
					}
				}

				nextPinnedList.Clear();
				refreshContracts(cList);
			}

			//Remove any hidden contracts after the window is drawn
			if (nextRemoveList.Count > 0)
			{
				foreach (contractUIObject c in nextRemoveList)
					showHideContract(c);

				nextRemoveList.Clear();
				refreshContracts(cList);
			}

			if (nextRemoveMissionList.Count > 0)
			{
				foreach (contractUIObject c in nextRemoveMissionList)
				{
					if (c.Container.Root.ContractState != Contract.State.Active)
					{
						foreach (contractMission m in missionList)
							m.removeContract(c.Container);
					}
					else
						currentMission.removeContract(c.Container);
				}

				nextRemoveMissionList.Clear();
				refreshContracts(cList);
			}

			if (stockToolbar != contractScenario.Instance.stockToolbar)
			{
				stockToolbar = contractScenario.Instance.stockToolbar;
				if (stockToolbar)
				{
					contractScenario.Instance.appLauncherButton = gameObject.AddComponent<contractStockToolbar>();
					if (contractScenario.Instance.blizzyToolbarButton != null)
					{
						Destroy(contractScenario.Instance.blizzyToolbarButton);
					}
				}
				else
				{
					contractScenario.Instance.blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();
					if (contractScenario.Instance.appLauncherButton != null)
					{
						Destroy(contractScenario.Instance.appLauncherButton);
					}
				}
			}

			if (!contractScenario.Instance.replaceStockWarned)
			{
				if (replaceStock != contractScenario.Instance.replaceStockToolbar)
				{
					replaceStock = contractScenario.Instance.replaceStockToolbar;
					if (replaceStock)
					{
						contractScenario.Instance.replaceStockToolbar = false;
						replaceStock = false;
						popup = true;
						toolbar = false;
						replaceStockPopup = true;
					}
				}
			}

			//Close the sort menu if clicked outside of its rectangle
			if (popup && Event.current.type == EventType.mouseDown && !popupRect.Contains(Event.current.mousePosition))
			{
				popup = false;
			}

			//Set the persistent window location
			contractScenario.Instance.windowRects[sceneInt] = WindowRect;
		}

		#endregion

		#region Methods

		//Reset contract list from the "refresh" button
		private void rebuildList()
		{
			contractScenario.Instance.addFullMissionList();

			currentMission = contractScenario.Instance.MasterMission;

			cList.Clear();
			pinnedList.Clear();

			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				contractContainer cC = contractParser.getActiveContract(c.ContractGuid);
				if (cC != null)
					currentMission.addContract(cC, true, false);
			}

			cList = currentMission.ActiveMissionList;
			refreshContracts(cList);
		}

		//Reset all parameters
		private void resetWindow()
		{
			//Reset window settings
			WindowRect = new Rect(40, 80, 250, 300);
			TooltipsEnabled = true;
			Visible = true;
			contractScenario.Instance.windowRects[sceneInt] = WindowRect;
			contractScenario.Instance.fontSmall = true;
			contractScenario.Instance.windowSize = 0;
			contractScenario.Instance.windowVisible[sceneInt] = Visible;
			contractScenario.Instance.toolTips = TooltipsEnabled;

			//Reset GUI settings
			contractSkins.normalFontSize = 0;
			contractSkins.windowFontSize = 0;
			contractSkins.initializeSkins();
			WindowStyle = contractSkins.newWindowStyle;
			DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
		}

		//Initial contract load
		private void generateList()
		{
			contractScenario.Instance.loadAllMissionLists();
			if (HighLogic.LoadedSceneIsFlight)
				currentMission = contractScenario.Instance.setLoadedMission(FlightGlobals.ActiveVessel);
			else
				currentMission = contractScenario.Instance.MasterMission;
		}

		//Update contract values
		private void refreshContracts(List<Guid> gID, bool sort = true)
		{
			List<Guid> removeList = new List<Guid>();
			List<Guid> pinnedRemoveList = new List<Guid>();
			foreach (Guid id in gID)
			{
				contractContainer cC = contractParser.getActiveContract(id);
				if (cC == null)
					cC = contractParser.getCompletedContract(id);

				if (cC == null)
				{
					removeList.Add(id);
					continue;
				}
				else
				{
					if (cC.Root.ContractState != Contract.State.Active)
					{
						cC.Duration = 0;
						cC.DaysToExpire = "----";

						cC.Title = cC.Root.Title;
						cC.Notes = cC.Root.Notes;

						foreach (parameterContainer pC in cC.AllParamList)
						{
							pC.Title = pC.CParam.Title;
							pC.setNotes(pC.CParam.Notes);
						}

						continue;
					}

					//Update contract timers
					if (cC.Root.DateDeadline <= 0)
					{
						cC.Duration = double.MaxValue;
						cC.DaysToExpire = "----";
					}
					else
					{
						cC.Duration = cC.Root.DateDeadline - Planetarium.GetUniversalTime();
						//Calculate time in day values using Kerbin or Earth days
						cC.DaysToExpire = contractScenario.timeInDays(cC.Duration);
					}

					cC.Title = cC.Root.Title;
					cC.Notes = cC.Root.Notes;

					foreach (parameterContainer pC in cC.AllParamList)
					{
						pC.Title = pC.CParam.Title;
						pC.setNotes(pC.CParam.Notes);
					}
				}
			}

			foreach(Guid id in pinnedList)
			{
				contractContainer cC = contractParser.getActiveContract(id);
				if (cC == null)
					pinnedRemoveList.Add(id);
			}

			foreach (Guid id in removeList)
				contractScenario.ListRemove(gID, id);

			foreach (Guid id in pinnedRemoveList)
				contractScenario.ListRemove(pinnedList, id);

			if (sort)
				gID = sortList(gID, currentMission.OrderMode, currentMission.AscendingOrder);
		}

		//Remove contract from current list and update
		private void showHideContract(contractUIObject c)
		{
			if (currentMission.ShowActiveMissions)
			{
				if (!currentMission.HiddenMissionList.Contains(c.Container.Root.ContractGuid) && c.Container.Root.ContractState == Contract.State.Active)
				{
					currentMission.HiddenMissionList.Add(c.Container.Root.ContractGuid);
					c.ShowParams = false;
				}

				contractScenario.ListRemove(currentMission.ActiveMissionList, c.Container.Root.ContractGuid);

				if (contractScenario.ListRemove(pinnedList, c.Container.Root.ContractGuid))
					c.Order = null;

				cList = currentMission.ActiveMissionList;
			}
			else
			{
				if (!currentMission.ActiveMissionList.Contains(c.Container.Root.ContractGuid) && c.Container.Root.ContractState == Contract.State.Active)
				{
					currentMission.ActiveMissionList.Add(c.Container.Root.ContractGuid);
					c.ShowParams = true;
				}

				contractScenario.ListRemove(currentMission.HiddenMissionList, c.Container.Root.ContractGuid);

				if (contractScenario.ListRemove(pinnedList, c.Container.Root.ContractGuid))
					c.Order = null;

				cList = currentMission.HiddenMissionList;
			}

			if (c.Container.Root.ContractState != Contract.State.Active)
			{
				currentMission.removeContract(c.Container);
				foreach (contractMission m in missionList)
					m.removeContract(c.Container);
			}
		}

		//Function to sort the list based on several criteria
		private List<Guid> sortList(List<Guid> gID, sortClass s, bool Asc)
		{
			List<contractUIObject> cL = new List<contractUIObject>();
			//Only add non-pinned contracts to the sort list
			foreach (Guid id in gID)
			{
				contractUIObject cC = currentMission.getContract(id);
				if (cC != null)
				{
					if (cC.Order == null)
						cL.Add(cC);
				}
			}
			switch (s)
			{
				case sortClass.Expiration:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.Duration.CompareTo(b.Container.Duration), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case sortClass.Acceptance:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.Root.DateAccepted.CompareTo(b.Container.Root.DateAccepted), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case sortClass.Reward:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.TotalReward.CompareTo(b.Container.TotalReward), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case sortClass.Difficulty:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.Root.Prestige.CompareTo(b.Container.Root.Prestige), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case sortClass.Planet:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.TargetPlanet.CompareTo(b.Container.TargetPlanet), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case sortClass.Type:
					cL.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(Asc, a.Container.Root.GetType().Name.CompareTo(b.Container.Root.GetType().Name), a.Container.Title.CompareTo(b.Container.Title)));
					cL = typeSort(cL, Asc);
					break;
			}
			gID.Clear();
			
			//Add pinned contracts to the beginning of the list
			if (pinnedList.Count > 0)
				gID.AddRange(pinnedList);

			//Next add the sorted contracts
			foreach (contractUIObject cC in cL)
				gID.Add(cC.Container.Root.ContractGuid);

			return gID;
		}

		//Special method for handling altitude based parameters; only ReachAltitudeEnvelope seems to be relevant
		private List<contractUIObject> typeSort(List<contractUIObject> cL, bool B)
		{
			List<int> position = new List<int>();
			List<contractUIObject> altList = new List<contractUIObject>();
			for (int i = 0; i < cL.Count; i++)
			{
				foreach (ContractParameter cP in cL[i].Container.Root.AllParameters)
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
				altList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachAltitudeEnvelope)a.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude.CompareTo(((ReachAltitudeEnvelope)b.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude), a.Container.Title.CompareTo(b.Container.Title)));
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
			switch (cP.CParam.State)
			{
				case ParameterState.Complete:
					return contractSkins.paramCompleted;
				case ParameterState.Failed:
					return contractSkins.paramFailed;
				default:
					if (cP.Level == 0)
						return contractSkins.paramText;
					else
						return contractSkins.paramSub;
			}
		}

		//Adds new contracts when they are accepted in Mission Control
		private void contractAccepted(Contract c)
		{
			if (c == null)
				return;

			if (c.ContractState != Contract.State.Active)
				return;

			contractContainer cC = contractParser.getActiveContract(c.ContractGuid);
			if (cC != null)
			{
				currentMission.addContract(cC, true, true);
				if (currentMission.ShowActiveMissions)
					refreshContracts(cList);

				if (!currentMission.MasterMission)
					contractScenario.Instance.MasterMission.addContract(cC, true, true);
			}
		}

		#endregion

		#region Repeating Worker

		protected override void RepeatingWorker()
		{
			if (cList.Count > 0)
				refreshContracts(cList, false);
		}

		#endregion

		#region Persistence

		//Load window position and size settings
		private void PersistenceLoad()
		{
			if (contractScenario.Instance != null)
			{
				stockToolbar = contractScenario.Instance.stockToolbar;
				replaceStock = contractScenario.Instance.replaceStockToolbar;
				cList.Clear();
				missionList = contractScenario.Instance.getAllMissions();
				currentMission = missionList[0];
				if (currentMission == null)
					currentMission = new contractMission("");
				WindowRect = contractScenario.Instance.windowRects[sceneInt];
				if (contractScenario.Instance.fontSmall)
					contractSkins.normalFontSize = 0;
				else
					contractSkins.normalFontSize = 1;
				if (contractScenario.Instance.windowSize == 0)
					contractSkins.windowFontSize = 0;
				else
					contractSkins.windowFontSize = 2;
				contractSkins.initializeSkins();
				WindowStyle = contractSkins.newWindowStyle;
				DragRect = new Rect(0, 0, WindowRect.width - 19, WindowRect.height - 24 - contractScenario.Instance.windowSize * 8);
				Visible = contractScenario.Instance.windowVisible[sceneInt];
				TooltipsEnabled = contractScenario.Instance.toolTips;
				if (Visible)
					StartRepeatingWorker(5);
				if (WindowRect.width < 100)
					resetWindow();
			}
		}

		#endregion

	}

	#region SortClass

	public enum sortClass
	{
		Difficulty = 1,
		Expiration = 2,
		Acceptance = 3,
		Reward = 4,
		Type = 5,
		Planet = 6,
	}

	#endregion
}
