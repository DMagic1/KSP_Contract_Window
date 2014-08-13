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
	//[KSPAddonImproved(KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TimeElapses, false)]
	class contractsWindow: MonoBehaviourWindow
	{

		#region Initialization

		private List<contractContainer> cList = new List<contractContainer>();
		private List<contractContainer> showList = new List<contractContainer>();
		private List<contractContainer> hiddenList = new List<contractContainer>();
		private List<contractContainer> nextRemoveList = new List<contractContainer>();
		private string version;
		private Assembly assembly;
		private Vector2 scroll;
		private bool resizing, showSort, editorLocked;
		private float dragStart, windowHeight;
		private sortClass sort;
		private int order; //0 is descending, 1 is ascending
		private int windowMode; //0 is compact, 1 is expiration display, 2 is full display
		private int showHideList; //0 is standard, 1 shows hidden contracts
		private Rect dropDownSort;

		private contractScenario contract = contractScenario.Instance;

		internal override void Awake()
		{
			assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
			version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

			WindowCaption = "Contracts +";
			WindowRect = new Rect(40, 80, 250, 300);
			WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
			WindowStyle = contractSkins.newWindowStyle;
			Visible = true;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);

			SetRepeatRate(5);
			RepeatingWorkerInitialWait = 10;
			StartRepeatingWorker();

			InputLockManager.RemoveControlLock("ContractsWindow".GetHashCode().ToString());

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
			EditorLogic.fetch.Unlock("ContractsWindow".GetHashCode().ToString());
			GameEvents.Contract.onAccepted.Remove(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Remove(contractLoaded);
		}

		internal override void Update()
		{
		}

		#endregion

		#region GUI Draw

		internal override void DrawWindowPre(int id)
		{
			//Update the drag rectangle
			DragRect.height = WindowRect.height - 24;

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

			//Lock editor click through
			if (HighLogic.LoadedSceneIsEditor)
			{
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !editorLocked)
				{
					EditorLogic.fetch.Lock(true, true, true, "ContractsWindow".GetHashCode().ToString());
					editorLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && editorLocked)
				{
					EditorLogic.fetch.Unlock("ContractsWindow".GetHashCode().ToString());
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
			foreach (contractContainer c in cList)
			{
				//Contracts
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
			{
				buildSortMenu(id);
			}
			//Bottom bar
			buildBottomBar(id);
			//Resize window when the resizer is grabbed by the mouse
			buildResizer(id);
		}

		#region Top Menu

		private void buildMenuBar(int ID)
		{
			//Sort icons
			if (GUI.Button(new Rect(4, 1, 30, 17), new GUIContent(contractSkins.sortIcon, "Sort Contracts")))
				showSort = !showSort;

			if (order == 0)
			{
				if (GUI.Button(new Rect(33, 1, 24, 17), new GUIContent(contractSkins.orderAsc, "Ascending Order")))
				{
					order = 1;
					contractScenario.Instance.setOrderMode(order);
					refreshContracts(cList);
				}
			}
			else if (order == 1)
			{
				if (GUI.Button(new Rect(33, 1, 24, 17), new GUIContent(contractSkins.orderDesc, "Descending Order")))
				{
					order = 0;
					contractScenario.Instance.setOrderMode(order);
					refreshContracts(cList);
				}
			}

			//Show and hide icons
			if (showHideList == 0)
			{
				if (GUI.Button(new Rect(60, 1, 26, 19), new GUIContent(contractSkins.revealShowIcon, "Show Hidden Contracts")))
				{
					showHideList = 1;
					contractScenario.Instance.setShowHideMode(showHideList);
					cList = hiddenList;
					refreshContracts(cList);
				}
			}
			else if (showHideList == 1)
			{
				if (GUI.Button(new Rect(60, 1, 26, 19), new GUIContent(contractSkins.revealHideIcon, "Show Standard Contracts")))
				{
					showHideList = 0;
					contractScenario.Instance.setShowHideMode(showHideList);
					cList = showList;
					refreshContracts(cList);
				}
			}

			//Expand and contract icons
			if (windowMode == 0)
			{
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.expandRight))
				{
					windowMode = 1;
					contractScenario.Instance.setWindowMode(windowMode);
					WindowRect.width = 290;
					contractScenario.Instance.setWindowPosition((int)WindowRect.x, (int)WindowRect.y, (int)WindowRect.width, (int)WindowRect.height);
					DragRect.width = WindowRect.width - 20;
				}
			}
			else if (windowMode == 1)
			{
				if (GUI.Button(new Rect(WindowRect.width - 48, 1, 24, 18), contractSkins.collapseLeftStop))
				{
					windowMode = 0;
					contractScenario.Instance.setWindowMode(windowMode);
					WindowRect.width = 250;
					contractScenario.Instance.setWindowPosition((int)WindowRect.x, (int)WindowRect.y, (int)WindowRect.width, (int)WindowRect.height);
					DragRect.width = WindowRect.width - 20;
				}
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.expandRightStop))
				{
					windowMode = 2;
					contractScenario.Instance.setWindowMode(windowMode);
					WindowRect.width = 480;
					contractScenario.Instance.setWindowPosition((int)WindowRect.x, (int)WindowRect.y, (int)WindowRect.width, (int)WindowRect.height);
					DragRect.width = WindowRect.width - 20;
				}
			}
			else if (windowMode == 2)
			{
				if (GUI.Button(new Rect(WindowRect.width - 24, 1, 24, 18), contractSkins.collapseLeft))
				{
					windowMode = 1;
					contractScenario.Instance.setWindowMode(windowMode);
					WindowRect.width = 290;
					contractScenario.Instance.setWindowPosition((int)WindowRect.x, (int)WindowRect.y, (int)WindowRect.width, (int)WindowRect.height);
					DragRect.width = WindowRect.width - 20;
				}
			}

			GUI.DrawTexture(new Rect(2, 16, WindowRect.width - 4, 4), contractSkins.headerBar);
		}


		#endregion

		#region Contract Titles

		private void buildContractTitle(contractContainer c, int id)
		{
			//Contract title buttons
			GUILayout.BeginHorizontal();
			if (!showSort)
			{
				if (GUILayout.Button(c.contract.Title, titleState(c.contract.ContractState), GUILayout.MaxWidth(210)))
					c.showParams = !c.showParams;
			}
			else
				GUILayout.Box(c.contract.Title, hoverTitleState(c.contract.ContractState), GUILayout.MaxWidth(210));

			//Show and hide icons
			GUILayout.Space(-5);
			GUILayout.BeginVertical();
			if (showHideList == 0)
			{
				if (GUILayout.Button(new GUIContent(contractSkins.hideIcon, "Hide Contract"), GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
					nextRemoveList.Add(c);
			}
			else if (showHideList == 1)
			{
				if (GUILayout.Button(new GUIContent(contractSkins.showIcon, "Un-Hide Contract"), GUILayout.MaxWidth(15), GUILayout.MaxHeight(15)))
					nextRemoveList.Add(c);
			}
			GUILayout.EndVertical();

			//Expiration date
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

			else if (windowMode == 2)
			{
				if (c.duration >= 2160000)
					GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));
				else if (c.duration > 0)
					GUILayout.Label(c.daysToExpire, contractSkins.timerBad, GUILayout.Width(45));
				else if (c.contract.ContractState != Contract.State.Active)
					GUILayout.Label(c.daysToExpire, contractSkins.timerFinished, GUILayout.Width(45));
				else
					GUILayout.Label(c.daysToExpire, contractSkins.timerGood, GUILayout.Width(45));

				//Reward and penalty amounts
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
				
				//Rep rewards and penalty amounts
				//GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				if (c.contract.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
					GUILayout.Space(-5);
					GUILayout.Label("+ " + c.contract.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38));
					GUILayout.EndHorizontal();
				}
				if (c.contract.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Cancelled || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Failed))
				{
					if (c.contract.ContractState == Contract.State.Active)
						GUILayout.Space(-9);
					GUILayout.BeginHorizontal();
					GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
					GUILayout.Space(-5);
					GUILayout.Label("- " + c.contract.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38));
					GUILayout.EndHorizontal();
				}
				GUILayout.EndVertical();

				//Science reward
				//GUILayout.FlexibleSpace();
				if (c.contract.ScienceCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
				{
					GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(14), GUILayout.MaxWidth(14));
					GUILayout.Space(-5);
					GUILayout.Label("+ " + c.contract.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37));
				}
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Contract Parameters

		//private void buildContractParameters(contractContainer c, int id)
		//{

		//    //Contract Parameter list for each contract
		//    foreach (parameterContainer cP in c.paramList)
		//    {
		//        if (!string.IsNullOrEmpty(cP.cParam.Title))
		//        {
		//            //Check if each parameter has notes associated with it
		//            if (cP.cParam.State != ParameterState.Complete && !string.IsNullOrEmpty(cP.cParam.Notes))
		//            {
		//                GUILayout.Space(-2);
		//                GUILayout.BeginHorizontal();
		//                //Note icon
		//                if (GUILayout.Button("[+]", contractSkins.noteButton, GUILayout.MaxWidth(24)))
		//                    cP.showNote = !cP.showNote;
		//                GUILayout.Space(3);
		//                //Contract parameter title
		//                GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(228));

		//                //Contract reward info
		//                if (windowMode == 2)
		//                {
		//                    GUILayout.BeginVertical();
		//                    if (cP.cParam.FundsCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.FundsCompletion.ToString("N0"), contractSkins.reward, GUILayout.Width(65), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    if (cP.cParam.FundsFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
		//                    {
		//                        if (cP.cParam.FundsCompletion > 0)
		//                            GUILayout.Space(-6);
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("- " + cP.cParam.FundsFailure.ToString("N0"), contractSkins.penalty, GUILayout.Width(65), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    GUILayout.EndVertical();

		//                    GUILayout.FlexibleSpace();
		//                    GUILayout.BeginVertical();
		//                    if (cP.cParam.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    if (cP.cParam.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
		//                    {
		//                        if (cP.cParam.ReputationCompletion > 0)
		//                            GUILayout.Space(-9);
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("- " + cP.cParam.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    GUILayout.EndVertical();

		//                    GUILayout.FlexibleSpace();
		//                    if (cP.cParam.ScienceCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37), GUILayout.MaxHeight(14));
		//                    }
		//                }
		//                GUILayout.EndHorizontal();
		//                //Display note
		//                if (cP.showNote)
		//                {
		//                    GUILayout.Space(-3);
		//                    GUILayout.Box(cP.cParam.Notes, GUILayout.MaxWidth(261));
		//                }

		//            }

		//            //If no notes are present just display the title
		//            else
		//            {
		//                GUILayout.Space(-2);
		//                GUILayout.BeginHorizontal();
		//                GUILayout.Space(15);
		//                GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(250));

		//                //Reward info
		//                if (windowMode == 2)
		//                {
		//                    GUILayout.BeginVertical();
		//                    if (cP.cParam.FundsCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.fundsGreen, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.FundsCompletion.ToString("N0"), contractSkins.reward, GUILayout.Width(65), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    if (cP.cParam.FundsFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
		//                    {
		//                        if (cP.cParam.FundsCompletion > 0)
		//                            GUILayout.Space(-6);
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.fundsRed, GUILayout.MaxHeight(10), GUILayout.MaxWidth(10));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("- " + cP.cParam.FundsFailure.ToString("N0"), contractSkins.penalty, GUILayout.Width(65), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    GUILayout.EndVertical();
		//                    GUILayout.FlexibleSpace();
		//                    GUILayout.BeginVertical();
		//                    if (cP.cParam.ReputationCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.repGreen, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.ReputationCompletion.ToString("F0"), contractSkins.reward, GUILayout.Width(38), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    if (cP.cParam.ReputationFailure > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Failed || c.contract.ContractState == Contract.State.DeadlineExpired || c.contract.ContractState == Contract.State.Cancelled))
		//                    {
		//                        if (cP.cParam.ReputationCompletion > 0)
		//                            GUILayout.Space(-9);
		//                        GUILayout.BeginHorizontal();
		//                        GUILayout.Label(contractSkins.repRed, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("- " + cP.cParam.ReputationFailure.ToString("F0"), contractSkins.penalty, GUILayout.Width(38), GUILayout.MaxHeight(13));
		//                        GUILayout.EndHorizontal();
		//                    }
		//                    GUILayout.EndVertical();
		//                    GUILayout.FlexibleSpace();
		//                    if (cP.cParam.ScienceCompletion > 0 && (c.contract.ContractState == Contract.State.Active || c.contract.ContractState == Contract.State.Completed))
		//                    {
		//                        GUILayout.Label(contractSkins.science, GUILayout.MaxHeight(13), GUILayout.MaxWidth(14));
		//                        GUILayout.Space(-5);
		//                        GUILayout.Label("+ " + cP.cParam.ScienceCompletion.ToString("F0"), contractSkins.scienceReward, GUILayout.MaxWidth(37), GUILayout.MaxHeight(14));
		//                    }
		//                }
		//                GUILayout.EndHorizontal();

		//            }
		//        }
		//    }
		//}

		#endregion

		#region Parameters

		private void buildParameterLabel(parameterContainer cP, contractContainer c, int level, int id)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(4 + (level * 5));

			if (cP.cParam.State == ParameterState.Incomplete && !string.IsNullOrEmpty(cP.cParam.Notes))
			{
				//Note icon
				if (GUILayout.Button("[+]", contractSkins.noteButton, GUILayout.MaxWidth(24)))
					cP.showNote = !cP.showNote;
				GUILayout.Space(-6);
			}

			//Contract parameter title
			GUILayout.Box(cP.cParam.Title, paramState(cP.cParam.State), GUILayout.MaxWidth(240));

			//Contract reward info
			if (windowMode == 2)
			{
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
					if (c.contract.ContractState == Contract.State.Active)
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
					if (c.contract.ContractState == Contract.State.Active)
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

			//Display note
			if (!string.IsNullOrEmpty(cP.cParam.Notes) && cP.showNote && cP.cParam.State == ParameterState.Incomplete)
			{
				GUILayout.Space(-3);
				GUILayout.Box(cP.cParam.Notes, GUILayout.MaxWidth(261));
			}

			if (level < 4)
			{
				foreach (parameterContainer sP in c.paramList)
				{
					if (sP.level == level + 1)
						buildParameterLabel(sP, c, level + 1, id);
				}
			}

		}

		#endregion

		#region Sort Menu

		private void buildSortMenu(int id)
		{
			GUILayout.BeginVertical();
			dropDownSort = new Rect(10, 20, 80, 110);
			GUI.Box(dropDownSort, "", contractSkins.dropDown);
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 2, dropDownSort.width - 4, 20), "Expiration", contractSkins.sortMenu))
			{
				showSort = false;
				sort = sortClass.Expiration;
				contractScenario.Instance.setSortMode(sort);
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 23, dropDownSort.width - 4, 20), "Acceptance", contractSkins.sortMenu))
			{
				showSort = false;
				sort = sortClass.Acceptance;
				contractScenario.Instance.setSortMode(sort);
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 44, dropDownSort.width - 4, 20), "Difficulty", contractSkins.sortMenu))
			{
				showSort = false;
				sort = sortClass.Difficulty;
				contractScenario.Instance.setSortMode(sort);
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 65, dropDownSort.width - 4, 20), "Reward", contractSkins.sortMenu))
			{
				showSort = false;
				sort = sortClass.Reward;
				contractScenario.Instance.setSortMode(sort);
				refreshContracts(cList);
			}
			if (GUI.Button(new Rect(dropDownSort.x + 2, dropDownSort.y + 86, dropDownSort.width - 4, 20), "Type", contractSkins.sortMenu))
			{
				showSort = false;
				sort = sortClass.Type;
				contractScenario.Instance.setSortMode(sort);
				refreshContracts(cList);
			}
			GUILayout.EndVertical();
		}

		#endregion

		#region Bottom Bar

		private void buildBottomBar(int id)
		{
			GUI.DrawTexture(new Rect(2, WindowRect.height - 30, WindowRect.width - 4, 4), contractSkins.footerBar);

			//Version label
			GUI.Label(new Rect(10, WindowRect.height - 20, 30, 20), version, contractSkins.paramText);

			//Tooltip toggle icon
			if (GUI.Button(new Rect(40, WindowRect.height - 22, 35, 20), new GUIContent(contractSkins.tooltipIcon, "Toggle Tooltips")))
			{
				TooltipsEnabled = !TooltipsEnabled;
				contractScenario.Instance.setToolTips(TooltipsEnabled);
			}
		}

		#endregion

		#region Resizer

		private void buildResizer(int id)
		{
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
			//Remove any hidden contracts after the window is drawn
			if (nextRemoveList.Count > 0)
			{
				foreach (contractContainer c in nextRemoveList)
					showHideContract(c);
				nextRemoveList.Clear();
			}

			//Close the sort menu if clicked outside of its rectangle
			if (showSort && Event.current.type == EventType.mouseDown && !dropDownSort.Contains(Event.current.mousePosition))
				showSort = false;

			//Set the persistent window location
			contractScenario.Instance.setWindowPosition((int)WindowRect.x, (int)WindowRect.y, (int)WindowRect.width, (int)WindowRect.height);
		}

		#endregion

		#region Methods

		//Function to sort the list based on several criteria
		private List<contractContainer> sortList(List<contractContainer> cL, sortClass s, int i)
		{
			List<contractContainer> sortedList = new List<contractContainer>();
			bool Order = i < 1;
			if (s == sortClass.Default)
				return cL;
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
			return cL;
		}

		private List<contractContainer> typeSort(List<contractContainer> cL, bool B)
		{
			LogFormatted_DebugOnly("Checking For Altitude Parameters");
			List<int> position = new List<int>();
			List<contractContainer> altList = new List<contractContainer>();
			for (int i = 0; i < cL.Count; i++)
			{
				foreach (parameterContainer cP in cL[i].paramList)
				{
					if (cP.cParam.ID == "testAltitudeEnvelope")
					{
						altList.Add(cL[i]);
						position.Add(i);
					}
				}
			}
			if (altList.Count > 1)
			{
				LogFormatted_DebugOnly("Sorting Based On Altitude Envelope");
				altList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachAltitudeEnvelope)a.contract.AllParameters.First(s => s.ID == "testAltitudeEnvelope")).minAltitude.CompareTo(((ReachAltitudeEnvelope)b.contract.AllParameters.First(s => s.ID == "testAltitudeEnvelope")).minAltitude), a.contract.Title.CompareTo(b.contract.Title)));
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
			if (showHideList == 0)
				cList = showList;
		}

		//Rebuild contract list when the scene changes
		private void contractLoaded()
		{
			//Only update this list if the scenario module list is empty
			if (cList.Count > 0)
				return;
			else
			{
				foreach (Contract c in ContractSystem.Instance.Contracts)
				{
					if (c.ContractState == Contract.State.Active)
					{
						cList.Add(new contractContainer(c));
					}
				}
				cList = sortList(cList, sort, order);
				showHideList = 0;
				showList = cList;
			}
		}

		private void refreshContracts(List<contractContainer> cL)
		{
			foreach (contractContainer cC in cL)
			{
				if (cC.contract.ContractState != Contract.State.Active)
				{
					cC.duration = 0;
					cC.daysToExpire = "---";
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
			if (showHideList == 0)
				contractScenario.Instance.showList = cL;
			else if (showHideList == 1)
				contractScenario.Instance.hiddenList = cL;
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
			if (Visible)
			{
				LogFormatted_DebugOnly("Refreshing Contract List; Duration of repeat: {0}", RepeatingWorkerDuration);
				if (cList.Count > 0)
					refreshContracts(cList);
			}
		}

		#endregion

		#region Persistence

		//Load window position and size settings
		private void PersistenceLoad()
		{
			LogFormatted_DebugOnly("Loading Parameters Now");
			order = contractScenario.Instance.loadOrderMode();
			windowMode = contractScenario.Instance.loadWindowMode();
			sort = contractScenario.Instance.loadSortMode();
			showHideList = contractScenario.Instance.loadShowHideMode();
			TooltipsEnabled = contractScenario.Instance.loadToolTips();
			Visible = contractScenario.Instance.loadWindowVisible();
			int[] i = contractScenario.Instance.loadWindowPosition();
			WindowRect.x = i[0];
			WindowRect.y = i[1];
			WindowRect.width = i[2];
			WindowRect.height = i[3];
			DragRect = new Rect(0, 0, WindowRect.width - 19, WindowRect.height - 25);

			showList = contractScenario.Instance.showList;
			hiddenList = contractScenario.Instance.hiddenList;

			if (showHideList == 0)
				cList = showList;
			else
				cList = hiddenList;
			LogFormatted_DebugOnly("Contract Window Loaded");
		}

		#endregion

	}
}
