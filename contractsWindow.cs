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
using Contracts.Agents;
using UnityEngine;

namespace ContractsWindow
{

	class contractsWindow: DMC_MBW
	{

		#region Initialization

		private List<Guid> cList = new List<Guid>();
		private List<Guid> pinnedList = new List<Guid>();
		private List<contractContainer> nextRemoveList = new List<contractContainer>();
		private List<contractContainer> nextPinnedList = new List<contractContainer>();
		private Agent currentAgent;
		private string version;
		private Vector2 scroll;
		private bool resizing, editorLocked, spacecenterLocked, trackingLocked, contractsLoading, loaded;
		private bool popup, showSort, rebuild, agencyPopup;
		private float dragStart, windowHeight;
		private int timer;
		private Rect dropDownSort, resetRect, agentPopupRect, scrollRect, scrollViewRect;
		private int sceneInt;
		private const string lockID = "ContractsWindow_LockID";
		private const string centerLockID = "ContractsWindow_SC_LockID";
		private const string trackingLockID = "ContractsWindow_TS_LockID";

		private contractScenario contract = contractScenario.Instance;

		internal override void Awake()
		{
			Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
			var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
			switch (ainfoV == null)
			{
				case true: version = ""; break;
				default: version = ainfoV.InformationalVersion; break;
			}

			sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);

			//Set up the various GUI options to their default values here
			WindowCaption = "    Contracts +";
			WindowRect = new Rect(40, 80, 250, 300);
			//WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
			WindowStyle = contractSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);
			RepeatingWorkerInitialWait = 10;

			//Make sure our click-through control locks are disabled
			InputLockManager.RemoveControlLock(lockID);

			DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
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
				if (timer < 30)
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
			DragRect.height = WindowRect.height - 24 - contractScenario.Instance.windowSize * 8;

			scrollRect = new Rect(0, 25, WindowRect.width - 2, WindowRect.height - 55);
			scrollViewRect = new Rect(0, 0, scrollRect.width - 20, 1500);

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
			}
		}

		internal override void DrawWindow(int id)
		{
			int windowSizeAdjust = contractScenario.Instance.windowSize;
			//Menu Bar
			buildMenuBar(id, windowSizeAdjust);

			GUILayout.BeginVertical();
			GUILayout.Space(8);

			scroll = GUILayout.BeginScrollView(scroll);
			//scroll = GUI.BeginScrollView(scrollRect, scroll, scrollViewRect);

			Rect lastRect = new Rect(0, -2, 10, 0);

			//Contract List Begins
			foreach (Guid gID in cList)
			{
				contractContainer c = contractScenario.Instance.getContract(gID);
				//Contracts

				GUILayout.Space(-1);

				buildContractTitleBar(c, id, windowSizeAdjust, ref lastRect);

				GUILayout.Space(-5);

				buildContractTitle(c, id, windowSizeAdjust, ref lastRect);

				//Parameters
				if (c.showParams)
				{
					foreach (parameterContainer cP in c.paramList)
					{
						if (cP.level == 0 && !string.IsNullOrEmpty(cP.title))
							buildParameterLabel(cP, c, 0, id, windowSizeAdjust, ref lastRect);
					}
				}
			}

			GUILayout.EndScrollView();
			GUILayout.Space(18 + contractScenario.Instance.windowSize * 4);
			GUILayout.EndVertical();

			//Bottom bar
			buildBottomBar(id);

			//Draw various popup and dropdown windows
			buildPopup(id);

			//Resize window when the resizer is grabbed by the mouse
			buildResizer(id);
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

			r.x += 26 + (sizeAdjust * 9);
			r.width = 26 + (sizeAdjust * 6);
			r.height = 18 + (sizeAdjust * 6);

			if (contractScenario.Instance.orderMode[sceneInt] == 0)
			{
				if (GUI.Button(r, new GUIContent(contractSkins.orderAsc, "Ascending Order")))
				{
					contractScenario.Instance.orderMode[sceneInt] = 1;
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(r, new GUIContent(contractSkins.orderDesc, "Descending Order")))
				{
					contractScenario.Instance.orderMode[sceneInt] = 0;
					refreshContracts(cList);
				}
			}

			r.x += 26 + (sizeAdjust * 16);

			//Show and hide icons
			if (contractScenario.Instance.showHideMode[sceneInt] == 0)
			{
				if (GUI.Button(r, new GUIContent(contractSkins.revealShowIcon, "Show Hidden Contracts")))
				{
					contractScenario.Instance.showHideMode[sceneInt] = 1;
					cList = contractScenario.Instance.hiddenList;
					pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}
			else
			{
				if (GUI.Button(r, new GUIContent(contractSkins.revealHideIcon, "Show Standard Contracts")))
				{
					contractScenario.Instance.showHideMode[sceneInt] = 0;
					cList = contractScenario.Instance.showList;
					pinnedList = contractScenario.Instance.loadPinnedContracts(cList);
					refreshContracts(cList);
				}
			}

			r.x = WindowRect.width - 20 - (sizeAdjust * 4);
			r.width = 16 + (sizeAdjust * 4);

			//Expand and contract icons
			if (contractScenario.Instance.windowMode[sceneInt] == 0)
			{
				if (GUI.Button(r, contractSkins.expandRight))
				{
					contractScenario.Instance.windowMode[sceneInt] = 1;
					WindowRect.width = 540 + contractScenario.Instance.windowSize * 150;
					DragRect.width = WindowRect.width - 19;
				}
			}
			else
			{
				r.x -= 2;
				if (GUI.Button(r, contractSkins.collapseLeft))
				{
					contractScenario.Instance.windowMode[sceneInt] = 0;
					WindowRect.width = 250 + contractScenario.Instance.windowSize * 30;
					DragRect.width = WindowRect.width - 19;
				}
			}

			GUI.DrawTexture(new Rect(2, 17 + (sizeAdjust * 6), WindowRect.width - 4, 4), contractSkins.headerBar);
		}


		#endregion

		#region Contract Title Bar

		private void buildContractTitleBar(contractContainer c, int id, int size, ref Rect r)
		{
			r.x = 6;
			r.y += (r.height + 2);
			r.width = 54 + (size * 12);
			r.height = 15 + (size * 4);

			//Difficulty icons
			if (c.contract.Prestige == Contract.ContractPrestige.Trivial)
				GUI.DrawTexture(r, contractSkins.goldStar);
			else if (c.contract.Prestige == Contract.ContractPrestige.Significant)
				GUI.DrawTexture(r, contractSkins.goldStarTwo);
			else
				GUI.DrawTexture(r, contractSkins.goldStarThree);

			r.x += r.width + 4;
			r.width = 58 + (size * 15);

			//Expiration date
			if (c.duration >= 2160000)
				GUI.Label(r, c.daysToExpire, contractSkins.timerGood);
			else if (c.duration > 0)
				GUI.Label(r, c.daysToExpire, contractSkins.timerBad);
			else if (c.contract.ContractState == Contract.State.Completed)
				GUI.Label(r, c.daysToExpire, contractSkins.timerGood);
			else
				GUI.Label(r, c.daysToExpire, contractSkins.timerFinished);

			r.x += 60 + (size * 15);
			r.width = 16 + (size * 4);
			r.height += 1;

			//Agency Icon
			if (GUI.Button(r, new GUIContent(contractSkins.agencyIcon, "Agency"), contractSkins.texButtonSmall))
			{
				currentAgent = c.contract.Agent;
				popup = !popup;
				agencyPopup = !agencyPopup;
			}

			r.x += 22 + (size * 4);

			//Show and hide icons
			if (c.contract.ContractState == Contract.State.Active)
			{
				if (contractScenario.Instance.showHideMode[sceneInt] == 0)
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
				{
					nextRemoveList.Add(c);
				}
			}

			r.x += 22 + (size * 4);

			//Pin icon button
			if (c.listOrder == null)
			{
				if (GUI.Button(r, new GUIContent(contractSkins.pinIcon, "Pin Contract"), contractSkins.texButtonSmall))
				{
					nextPinnedList.Add(c);
				}
			}
			else
			{
				r.width -= 2;
				if (GUI.Button(r, new GUIContent(contractSkins.pinDownIcon, "Un-Pin Contract"), contractSkins.texButtonSmall))
				{
					nextPinnedList.Add(c);
				}
			}

			r.x += 22 + (size * 4);
			r.width = 12 + (size * 4);
			r.height = 14 + (size * 4);

			//Note icon button
			if (c.contract.ContractState == Contract.State.Active && !string.IsNullOrEmpty(c.notes))
			{
				if (!c.showNote)
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Note"), contractSkins.texButtonSmall))
						c.showNote = !c.showNote;
				}
				else
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Note"), contractSkins.texButtonSmall))
						c.showNote = !c.showNote;
				}
			}
		}

		#endregion

		#region Contract Titles

		private void buildContractTitle(contractContainer c, int id, int size, ref Rect r)
		{
			string contractTitle = c.title;
			GUIStyle cStyle = titleState(c.contract.ContractState);
			bool active = c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed;
			bool failed = c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Cancelled || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Failed;

			//Contract title buttons

			GUILayout.Space(23 + size * 4);

			if (!popup)
			{
				if (GUILayout.Button(contractTitle, cStyle, GUILayout.MaxWidth(225 + size * 30)))
					c.showParams = !c.showParams;
			}
			else
				GUILayout.Box(contractTitle, hoverTitleState(c.contract.ContractState), GUILayout.MaxWidth(225 + size * 30));

			r = GUILayoutUtility.GetLastRect();

			if (contractScenario.Instance.windowMode[sceneInt] == 1)
			{
				Rect rewardsRect = r;
				rewardsRect.x += 230 + (size * 30);
				rewardsRect.y += 4;

				//Reward and penalty amounts
				if (c.fundsReward > 0 || c.fundsPenalty > 0)
				{
					rewardsRect.width = 8 + (size * 2);
					rewardsRect.height = 11 + (size * 5);

					if (c.fundsReward > 0 && active)
					{
						Rect fundsRect = rewardsRect;

						GUI.DrawTexture(fundsRect, contractSkins.fundsGreen);

						fundsRect.x += 9 + (size * 2);
						fundsRect.width = 110 + (size * 18);
						fundsRect.height += 4;

						GUI.Label(fundsRect, c.fundsRew, contractSkins.reward);
					}
					if (c.fundsPenalty > 0 && failed)
					{
						Rect fundsRect = rewardsRect;

						fundsRect.y += 14 + (size * 5);

						GUI.DrawTexture(fundsRect, contractSkins.fundsRed);

						fundsRect.x += 9 + (size * 2);
						fundsRect.width = 110 + (size * 18);
						fundsRect.height += 4;

						GUI.Label(fundsRect, c.fundsPen, contractSkins.penalty);
					}
				}

				rewardsRect.x += 126 + (size * 20);

				//Rep rewards and penalty amounts
				if (c.repReward > 0 || c.repPenalty > 0)
				{
					rewardsRect.width = 12 + (size * 4);
					rewardsRect.height = 12 + (size * 4);

					if (c.repReward > 0 && active)
					{
						Rect repRect = rewardsRect;

						GUI.DrawTexture(repRect, contractSkins.repGreen);

						repRect.x += 14 + (size * 4);
						repRect.width = 66 + (size * 12);
						repRect.height += 4;

						GUI.Label(repRect, c.repRew, contractSkins.reward);
					}
					if (c.repPenalty > 0 && failed)
					{
						Rect repRect = rewardsRect;

						repRect.y += 14 + (size * 5);

						GUI.DrawTexture(repRect, contractSkins.repRed);

						repRect.x += 14 + (size * 4);
						repRect.width = 66 + (size * 12);
						repRect.height += 4;

						GUI.Label(repRect, c.repPen, contractSkins.penalty);
					}
				}

				rewardsRect.x += 82 + (size * 16);

				//Science reward
				if (c.scienceReward > 0)
				{
					if (active)
					{
						rewardsRect.width = 12 + (size * 4);
						rewardsRect.height = 12 + (size * 4);

						GUI.DrawTexture(rewardsRect, contractSkins.science);

						rewardsRect.x += 14 + (size * 4);
						rewardsRect.width = 66 + (size * 12);
						rewardsRect.height += 4;

						GUI.Label(rewardsRect, c.sciRew, contractSkins.scienceReward);
					}
				}
			}

			//Display note
			if (!string.IsNullOrEmpty(c.notes) && c.showNote && c.contract.ContractState == Contract.State.Active)
			{
				GUILayout.Space(-3);
				GUILayout.Box(c.notes, GUILayout.MaxWidth(300 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}
		}

		#endregion

		#region Parameters

		private void buildParameterLabel(parameterContainer cP, contractContainer c, int level, int id, int size, ref Rect r)
		{
			string paramTitle = cP.title;
			bool active = cP.cParam.State == ParameterState.Incomplete;
			bool greenState = cP.cParam.State == ParameterState.Complete || cP.cParam.State == ParameterState.Incomplete;
			bool redState = cP.cParam.State == ParameterState.Incomplete || cP.cParam.State == ParameterState.Failed;
			GUIStyle pStyle = paramState(cP);

			GUILayout.BeginHorizontal();
			GUILayout.Space(5 + (level * 5));

			r.x = 5 + (level * 5);
			r.y += r.height;

			//Note icon button
			if (active && !string.IsNullOrEmpty(cP.notes))
			{
				r.x -= 2;
				r.y += 4;
				r.width = 12 + (size * 2);
				r.height = 14 + (size * 4);

				if (!cP.showNote)
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIcon, "Show Note"), contractSkins.texButtonSmall))
						cP.showNote = !cP.showNote;
				}
				else
				{
					if (GUI.Button(r, new GUIContent(contractSkins.noteIconOff, "Hide Note"), contractSkins.texButtonSmall))
						cP.showNote = !cP.showNote;
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
			////}

			//r.x += r.width + (size * 2);
			//r.y += 20 + (size * 4);
			//r.width = 220 + (size * 30) - (level * 5);
			//r.height = pStyle.CalcHeight(new GUIContent(paramTitle), r.width) + 4;

			//Contract parameter title
			GUILayout.Box(paramTitle, pStyle, GUILayout.MaxWidth(220 - (level * 5) + size * 30));

			r = GUILayoutUtility.GetLastRect();

			GUILayout.EndHorizontal();
			//Parameter reward info
			if (contractScenario.Instance.windowMode[sceneInt] == 1 && active)
			{
				Rect rewardsRect = r;
				rewardsRect.x += 230 + (size * 30);
				rewardsRect.y += 4;

				if (cP.fundsReward > 0 || cP.fundsPenalty > 0)
				{
					rewardsRect.width = 8 + (size * 2);
					rewardsRect.height = 11 + (size * 5);

					if (cP.fundsReward > 0 && greenState)
					{
						Rect fundsRect = rewardsRect;

						GUI.DrawTexture(fundsRect, contractSkins.fundsGreen);

						fundsRect.x += 9 + (size * 2);
						fundsRect.width = 110 + (size * 18);
						fundsRect.height += 4;

						GUI.Label(fundsRect, cP.fundsRew, contractSkins.reward);
					}
					if (cP.fundsPenalty > 0 && redState)
					{
						Rect fundsRect = rewardsRect;

						fundsRect.y += 12 + (size * 5);

						GUI.DrawTexture(fundsRect, contractSkins.fundsRed);

						fundsRect.x += 9 + (size * 2);
						fundsRect.width = 110 + (size * 18);
						fundsRect.height += 4;

						GUI.Label(fundsRect, cP.fundsPen, contractSkins.penalty);
					}
				}

				rewardsRect.x += 126 + (size * 20);

				if (cP.repReward > 0 || cP.repPenalty > 0)
				{
					rewardsRect.width = 12 + (size * 4);
					rewardsRect.height = 12 + (size * 4);

					if (cP.repReward > 0 && greenState)
					{
						Rect repRect = rewardsRect;

						GUI.DrawTexture(repRect, contractSkins.repGreen);

						repRect.x += 14 + (size * 4);
						repRect.width = 66 + (size * 12);
						repRect.height += 4;

						GUI.Label(repRect, cP.repRew, contractSkins.reward);
					}
					if (cP.repPenalty > 0 && redState)
					{
						Rect repRect = rewardsRect;

						repRect.y += 14 + (size * 4);

						GUI.DrawTexture(repRect, contractSkins.repRed);

						repRect.x += 14 + (size * 4);
						repRect.width = 66 + (size * 12);
						repRect.height += 4;

						GUI.Label(repRect, cP.repPen, contractSkins.penalty);
					}
				}

				rewardsRect.x += 82 + (size * 16);

				if (cP.scienceReward > 0 && greenState)
				{
					rewardsRect.width = 12 + (size * 4);
					rewardsRect.height = 12 + (size * 4);

					GUI.DrawTexture(rewardsRect, contractSkins.science);

					rewardsRect.x += 14 + (size * 4);
					rewardsRect.width = 66 + (size * 12);
					rewardsRect.height += 4;

					GUI.Label(rewardsRect, cP.sciRew, contractSkins.scienceReward);
				}

			}

			//Display note
			if (!string.IsNullOrEmpty(cP.notes) && cP.showNote && active)
			{
				GUILayout.Space(-3);
				GUILayout.Box(cP.notes, GUILayout.MaxWidth(320 + size * 60));

				r.height += GUILayoutUtility.GetLastRect().height;
			}

			if (level < 4)
			{
				foreach (parameterContainer sP in cP.paramList)
				{
					if (sP.level == level + 1 && !string.IsNullOrEmpty(sP.title))
					{
						if (active)
							buildParameterLabel(sP, c, level + 1, id, size, ref r);
					}
				}
			}

		}

		#endregion

		#region Popups

		private void buildPopup(int id)
		{
			if (popup)
			{
				if (showSort)
				{
					dropDownSort = new Rect(10, 20, 80 + contractScenario.Instance.windowSize * 15, 110 + contractScenario.Instance.windowSize * 23);
					GUI.Box(dropDownSort, "", contractSkins.dropDown);
					if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 2, dropDownSort.width - 4, 20 + contractScenario.Instance.windowSize * 5), "Expiration", contractSkins.sortMenu))
					{
						showSort = false;
						contractScenario.Instance.sortMode[sceneInt] = sortClass.Expiration;
						refreshContracts(cList);
					}
					if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 23 + contractScenario.Instance.windowSize * 5, dropDownSort.width - 4, 20 + contractScenario.Instance.windowSize * 5), "Acceptance", contractSkins.sortMenu))
					{
						showSort = false;
						contractScenario.Instance.sortMode[sceneInt] = sortClass.Acceptance;
						refreshContracts(cList);
					}
					if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 44 + contractScenario.Instance.windowSize * 10, dropDownSort.width - 4, 20 + contractScenario.Instance.windowSize * 5), "Difficulty", contractSkins.sortMenu))
					{
						showSort = false;
						contractScenario.Instance.sortMode[sceneInt] = sortClass.Difficulty;
						refreshContracts(cList);
					}
					if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 65 + contractScenario.Instance.windowSize * 15, dropDownSort.width - 4, 20 + contractScenario.Instance.windowSize * 5), "Reward", contractSkins.sortMenu))
					{
						showSort = false;
						contractScenario.Instance.sortMode[sceneInt] = sortClass.Reward;
						refreshContracts(cList);
					}
					if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 86 + contractScenario.Instance.windowSize * 20, dropDownSort.width - 4, 20 + contractScenario.Instance.windowSize * 5), "Type", contractSkins.sortMenu))
					{
						showSort = false;
						contractScenario.Instance.sortMode[sceneInt] = sortClass.Type;
						refreshContracts(cList);
					}
				}

				else if (rebuild)
				{
					resetRect = new Rect(10, WindowRect.height - 180, 230, 150);
					GUI.Box(resetRect, "", contractSkins.dropDown);
					GUI.Label(new Rect(resetRect.x + 7, resetRect.y + 10, resetRect.width - 14, 100), "Rebuild\nContracts Window + Display:\n\n<b>Will Not</b> Affect Contract Status", contractSkins.resetBox);
					if (GUI.Button(new Rect(resetRect.x + 20, resetRect.y + 110, resetRect.width - 40, 25), "Reset Display", contractSkins.resetButton))
					{
						LogFormatted("Rebuilding Contract Window List");
						generateList();
						rebuildList();
						resetWindow();
						rebuild = false;
					}
				}

				else if (agencyPopup)
				{
					agentPopupRect = new Rect(10, 40, 230 + contractScenario.Instance.windowSize * 20, 80);
					GUI.Box(agentPopupRect, "", contractSkins.dropDown);
					Rect r = new Rect(agentPopupRect.x + 5, agentPopupRect.y + 10, 84, 60);
					GUI.Box(r, "", contractSkins.agentBackground);
					r.x += 10;
					r.y += 10;
					r.width = 64;
					r.height = 40;
					GUI.Label(r, currentAgent.LogoScaled);
					r.x += 85;
					r.y -= 10;
					r.width = 120 + contractScenario.Instance.windowSize * 20;
					r.height = 60;
					GUI.Label(r, currentAgent.Name, contractSkins.agentName);
				}

				else
					popup = false;
			}
		}

		#endregion

		#region Bottom Bar

		private void buildBottomBar(int id)
		{
			Rect r = new Rect(2, WindowRect.height - 30 - contractScenario.Instance.windowSize * 4, WindowRect.width - 4, 4);
			GUI.DrawTexture(r, contractSkins.footerBar);

			//Version label
			r.x = 8;
			r.y = WindowRect.height - 23 + contractScenario.Instance.windowSize * -4;
			r.width = 30 + contractScenario.Instance.windowSize * 4;
			r.height = 20 + contractScenario.Instance.windowSize * 4;
			GUI.Label(r, version, contractSkins.paramText);

			//Tooltip toggle icon
			r.x = 36 + contractScenario.Instance.windowSize * 4;
			r.y -= 2;
			r.height += 2;
			if (GUI.Button(r, new GUIContent(contractSkins.tooltipIcon, "Toggle Tooltips")))
			{
				TooltipsEnabled = !TooltipsEnabled;
				contractScenario.Instance.toolTips = TooltipsEnabled;
			}

			//Clear list button
			r.x = 74 + contractScenario.Instance.windowSize * 10;
			if (GUI.Button(r, new GUIContent(contractSkins.resetIcon, "Reset Contracts Window Display")))
			{
				popup = !popup;
				rebuild = !rebuild;
			}

			//Font size button
			r.x = 112 + contractScenario.Instance.windowSize * 16;
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
			r.x = 150 + contractScenario.Instance.windowSize * 22;
			if (GUI.Button(r, new GUIContent(contractSkins.windowSize, "Change Window Size")))
			{
				if (contractScenario.Instance.windowSize == 0)
				{
					contractScenario.Instance.windowSize = 1;
					contractSkins.windowFontSize = 2;
					if (contractScenario.Instance.windowMode[sceneInt] == 0)
						WindowRect.width += 30;
					else
						WindowRect.width += 60;
					DragRect.width = WindowRect.width - 19;
				}
				else
				{
					contractScenario.Instance.windowSize = 0;
					contractSkins.windowFontSize = 0;
					if (contractScenario.Instance.windowMode[sceneInt] == 0)
						WindowRect.width -= 30;
					else
						WindowRect.width -= 60;
					DragRect.width = WindowRect.width - 19;
				}
				contractSkins.initializeSkins();
				WindowStyle = contractSkins.newWindowStyle;
				DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
			}

			//Contract config window button
			r.x = 188 + contractScenario.Instance.windowSize * 28;
			if (GUI.Button(r, new GUIContent(contractSkins.settingsIcon, "Contract Configuration")))
			{
				contractScenario.Instance.cConfig.Visible = !contractScenario.Instance.cConfig.Visible;
			}
		}

		#endregion

		#region Resizer

		private void buildResizer(int id)
		{
			Rect resizer = new Rect(WindowRect.width - 16 - contractScenario.Instance.windowSize * 3, WindowRect.height - 25 - contractScenario.Instance.windowSize * 3, 14 + contractScenario.Instance.windowSize * 4, 22 + contractScenario.Instance.windowSize * 4);
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
			{
				popup = false;
				showSort = false;
			}

			//Close the reset warning if clicked outside of its rectangle
			if (rebuild && Event.current.type == EventType.mouseDown && !resetRect.Contains(Event.current.mousePosition))
			{
				popup = false;
				rebuild = false;
			}

			//Close the agency popup if clicked outside of its rectangle
			if (agencyPopup && Event.current.type == EventType.mouseDown && !agentPopupRect.Contains(Event.current.mousePosition))
			{
				popup = false;
				agencyPopup = false;
			}

			//Set the persistent window location
			contractScenario.Instance.windowRects[sceneInt] = WindowRect;
		}

		#endregion

		#region Methods

		//Reset contract list from the "refresh" button
		private void rebuildList()
		{
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

		//Reset all parameters
		private void resetWindow()
		{
			//Reset window settings
			WindowRect = new Rect(40, 80, 250, 300);
			TooltipsEnabled = true;
			Visible = true;
			DragRect.width = WindowRect.width - 19;
			contractScenario.Instance.showHideMode[sceneInt] = 0;
			contractScenario.Instance.orderMode[sceneInt] = 0;
			contractScenario.Instance.windowMode[sceneInt] = 0;
			contractScenario.Instance.sortMode[sceneInt] = sortClass.Difficulty;
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
