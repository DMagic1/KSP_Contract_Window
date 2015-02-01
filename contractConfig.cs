#region license
/*The MIT License (MIT)
Contract Config - Addon to control contract config options

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

using Contracts;
using Contracts.Parameters;
using ContractsWindow.Toolbar;
using UnityEngine;

namespace ContractsWindow
{
	class contractConfig : DMC_MBW
	{
		private const string lockID = "ContractConfigLockID";
		private bool dropDown, cDropDown, pDropDown, rCPopup, rPPopup, wPopup, zPopup, stockPopup, activePopup;
		private bool spacecenterLocked, trackingLocked, editorLocked;
		private bool stockToolbar = true;
		private bool replaceStockAppLauncher = false;
		private bool alterActive = false;
		private Rect ddRect;
		private Vector2 cScroll, pScroll;
		private List<contractTypeContainer> cList;
		private List<paramTypeContainer> pList;
		private paramTypeContainer paramType;
		private contractTypeContainer contractType;
		private float cFRew, cFAdv, cFPen, cRRew, cRPen, cSRew, cOffer, cActive, cDur, pFRew, pFPen, pRRew, pRPen, pSRew;
		private float[] oldCValues;
		private float[] oldPValues;

		protected override void Awake()
		{
			WindowCaption = "Contract Configuration";
			WindowRect = new Rect(40, 80, 780, 380);
			WindowStyle = contractSkins.newWindowStyle;
			Visible = false;
			DragEnabled = true;
			TooltipMouseOffset = new Vector2d(-10, -25);

			//Make sure our click-through control locks are disabled
			InputLockManager.RemoveControlLock(lockID);

			DMC_SkinsLibrary.SetCurrent("ContractUnitySkin");
		}

		protected override void Start()
		{
			cList = contractScenario.Instance.setContractTypes(cList);
			pList = contractScenario.Instance.setParamTypes(pList);
			if (cList.Count > 0 && pList.Count > 0)
			{
				setContractType(cList[0]);
				setParameterType(pList[0]);
			}
			stockToolbar = contractScenario.Instance.stockToolbar;
			alterActive = contractScenario.Instance.alterActive;
			replaceStockAppLauncher = contractScenario.Instance.replaceStockToolbar;
		}

		protected override void OnDestroy()
		{
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
		}

		protected override void DrawWindowPre(int id)
		{
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
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_FACILITIES | ControlTypes.KSC_UI, lockID);
					spacecenterLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && spacecenterLocked)
				{
					InputLockManager.RemoveControlLock(lockID);
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
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.TRACKINGSTATION_ALL, lockID);
					trackingLocked = true;
				}
				else if (!WindowRect.Contains(mousePos) && trackingLocked)
				{
					InputLockManager.RemoveControlLock(lockID);
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

			if (!dropDown)
			{
				cDropDown = false;
				pDropDown = false;
				rCPopup = false;
				rPPopup = false;
				zPopup = false;
				wPopup = false;
				stockPopup = false;
				activePopup = false;
			}
		}

		protected override void DrawWindow(int id)
		{
			closeButton(id);						/* Draw the close button */

			GUILayout.BeginVertical();
				GUILayout.Space(10);
				GUILayout.BeginHorizontal();
					GUILayout.Space(8);
					GUILayout.BeginVertical();
						contractSelectionMenu(id);	/* Drop down menu and label for the current contract type */
						contractOptions(id);		/* Contract reward/penalty sliders */
					GUILayout.EndVertical();
					GUILayout.Space(50);
					GUILayout.BeginVertical();
						parameterSelectionMenu(id);	/* Drop down menu and label for the current parameter */
						parameterOptions(id);		/* Parameter reward/penalty sliders */
						windowConfig(id);			/* Options and settings */
					GUILayout.EndVertical();
					GUILayout.Space(8);
				GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			windowFrame(id);						/* Draw simple textures to divide the window space */
			dropDownMenu(id);						/* Draw the drop down menus when open */
		}

		protected override void DrawWindowPost(int id)
		{
			if (contractScenario.Instance.allowZero && !contractScenario.Instance.warnedZero)
			{
				dropDown = true;
				zPopup = true;
				if (!contractScenario.Instance.warnedZero)
					contractScenario.Instance.allowZero = !contractScenario.Instance.allowZero;
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

			if (alterActive != contractScenario.Instance.alterActive)
			{
				alterActive = contractScenario.Instance.alterActive;
				if (alterActive)
				{
					dropDown = true;
					activePopup = true;
				}
			}

			if (replaceStockAppLauncher != contractScenario.Instance.replaceStockToolbar)
			{
				replaceStockAppLauncher = contractScenario.Instance.replaceStockToolbar;
				if (replaceStockAppLauncher)
				{
					dropDown = !dropDown;
					stockPopup = !stockPopup;
					replaceStockAppLauncher = false;
					contractScenario.Instance.replaceStockToolbar = false;
				}
			}

			if (dropDown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
				dropDown = false;
		}

		//Draw the close button in the upper right corner
		private void closeButton(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, "✖", contractSkins.configClose))
			{
				InputLockManager.RemoveControlLock(lockID);
				spacecenterLocked = false;
				trackingLocked = false;
				editorLocked = false;
				Visible = false;
			}
		}

		//Contract type selector
		private void contractSelectionMenu(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Contract Type:", contractSkins.configDropDown, GUILayout.MaxWidth(130)))
				{
					dropDown = !dropDown;
					cDropDown = !cDropDown;
				}

				if (contractType != null)
					GUILayout.Label(contractType.Name, contractSkins.configHeader, GUILayout.MaxWidth(160));
				else
					GUILayout.Label("Unknown", contractSkins.configHeader, GUILayout.MaxWidth(160));
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		//Contract options
		private void contractOptions(int id)
		{
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
				GUILayout.Label("Funds Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				string percent = "";
				if (!contractScenario.Instance.allowZero && contractType.RewardFund <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.RewardFund.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.RewardFund = logSlider(ref cFRew, -1, 1, r, 2);

				eventCheck(contractType.RewardFund, 0, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Funds Advance: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && contractType.AdvanceFund <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.AdvanceFund.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.AdvanceFund = logSlider(ref cFAdv, -1, 1, r, 2);

				eventCheck(contractType.AdvanceFund, 1, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Funds Penalty: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && contractType.PenaltyFund <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.PenaltyFund.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.PenaltyFund = logSlider(ref cFPen, -1, 1, r, 2);

				eventCheck(contractType.PenaltyFund, 2, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Rep Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && contractType.RewardRep <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.RewardRep.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.RewardRep = logSlider(ref cRRew, -1, 1, r, 2);

				eventCheck(contractType.RewardRep, 3, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Rep Penalty: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && contractType.PenaltyRep <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.PenaltyRep.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.PenaltyRep = logSlider(ref cRPen, -1, 1, r, 2);

				eventCheck(contractType.PenaltyRep, 4, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Science Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && contractType.RewardScience <= 0.009)
					percent = "0.1%";
				else
					percent = contractType.RewardScience.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.RewardScience = logSlider(ref cSRew, -1, 1, r, 2);

				eventCheck(contractType.RewardScience, 5, oldCValues, 0);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Duration: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				GUILayout.Label(contractType.DurationTime.ToString("P0"), contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.DurationTime = logSlider(ref cDur, -0.9f, 1, r, 2);

				eventCheck(contractType.DurationTime, 6, oldCValues, 0);

				drawSliderLabel(r, "10%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				string offers = "";
				if (contractType.MaxOffer < 10)
					offers = (contractType.MaxOffer * 10).ToString("N0");
				else
					offers = ("∞");
				GUILayout.Label("Max Offered: ", contractSkins.configCenterLabel, GUILayout.Width(48));
				GUILayout.Space(-15);
				GUILayout.Label(offers, contractSkins.configCenterLabel, GUILayout.Width(30));

				r = GUILayoutUtility.GetLastRect();
				r.x += 35;
				r.width = 110;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.MaxOffer = logSlider(ref cOffer, -1, 1, r, 2);

				eventCheck(contractType.MaxOffer, 7, oldCValues, 0);

				drawSliderLabel(r, "0", "   ∞", "10");

				GUILayout.Space(115);

				string actives = "";
				if (contractType.MaxActive < 10)
					actives = (contractType.MaxActive * 10).ToString("N0");
				else
					actives = "∞";
				GUILayout.Label("Max Active: ", contractSkins.configCenterLabel, GUILayout.Width(47));
				GUILayout.Space(-15);
				GUILayout.Label(actives, contractSkins.configCenterLabel, GUILayout.Width(30));

				r = GUILayoutUtility.GetLastRect();
				r.x += 35;
				r.width = 110;

				oldCValues = (float[])contractType.ContractValues.Clone();

				contractType.MaxActive = logSlider(ref cActive, -1, 1, r, 2);

				eventCheck(contractType.MaxActive, 8, oldCValues, 0);

				drawSliderLabel(r, "0", "   ∞", "10");
			GUILayout.EndHorizontal();
		}

		//Parameter type selector
		private void parameterSelectionMenu(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Parameter Type:", contractSkins.configDropDown, GUILayout.MaxWidth(140)))
				{
					dropDown = !dropDown;
					pDropDown = !pDropDown;
				}

				if (paramType != null)
					GUILayout.Label(paramType.Name, contractSkins.configHeader, GUILayout.MaxWidth(190));
				else
					GUILayout.Label("Unknown", contractSkins.configHeader, GUILayout.MaxWidth(190));
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		//Parameter options
		private void parameterOptions(int id)
		{
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
				GUILayout.Label("Funds Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				string percent = "";
				if (!contractScenario.Instance.allowZero && paramType.RewardFund <= 0.009)
					percent = "0.1%";
				else
					percent = paramType.RewardFund.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldPValues = (float[])paramType.ParamValues.Clone();

				paramType.RewardFund = logSlider(ref pFRew, -1, 1, r, 2);

				eventCheck(paramType.RewardFund, 0, oldPValues, 1);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Funds Penalty: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && paramType.PenaltyFund <= 0.009)
					percent = "0.1%";
				else
					percent = paramType.PenaltyFund.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldPValues = (float[])paramType.ParamValues.Clone();

				paramType.PenaltyFund = logSlider(ref pFPen, -1, 1, r, 2);

				eventCheck(paramType.PenaltyFund, 1, oldPValues, 1);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Rep Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && paramType.RewardRep <= 0.009)
					percent = "0.1%";
				else
					percent = paramType.RewardRep.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldPValues = (float[])paramType.ParamValues.Clone();

				paramType.RewardRep = logSlider(ref pRRew, -1, 1, r, 2);

				eventCheck(paramType.RewardRep, 2, oldPValues, 1);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Rep Penalty: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && paramType.PenaltyRep <= 0.009)
					percent = "0.1%";
				else
					percent = paramType.PenaltyRep.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldPValues = (float[])paramType.ParamValues.Clone();

				paramType.PenaltyRep = logSlider(ref pRPen, -1, 1, r, 2);

				eventCheck(paramType.PenaltyRep, 3, oldPValues, 1);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(14);

			GUILayout.BeginHorizontal();
				GUILayout.Label("Science Reward: ", contractSkins.configLabel, GUILayout.Width(100));
				GUILayout.Space(-4);
				if (!contractScenario.Instance.allowZero && paramType.RewardScience <= 0.009)
					percent = "0.1%";
				else
					percent = paramType.RewardScience.ToString("P0");
				GUILayout.Label(percent, contractSkins.configCenterLabel, GUILayout.Width(48));

				r = GUILayoutUtility.GetLastRect();
				r.x += 50;
				r.width = 200;

				oldPValues = (float[])paramType.ParamValues.Clone();

				paramType.RewardScience = logSlider(ref pSRew, -1, 1, r, 2);

				eventCheck(paramType.RewardScience, 4, oldPValues, 1);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		//Draw all of the config option toggles and buttons
		private void windowConfig(int id)
		{
			GUILayout.Space(30);
			GUILayout.BeginHorizontal();
				GUILayout.Space(20);
				GUILayout.BeginVertical();
					contractScenario.Instance.allowZero = GUILayout.Toggle(contractScenario.Instance.allowZero, "Allow 0% Values", contractSkins.configToggle);

					contractScenario.Instance.alterActive = GUILayout.Toggle(contractScenario.Instance.alterActive, "Alter Active Contracts", contractSkins.configToggle);

					if (ToolbarManager.ToolbarAvailable)
						contractScenario.Instance.stockToolbar = GUILayout.Toggle(contractScenario.Instance.stockToolbar, "Use Stock Toolbar", contractSkins.configToggle);

					if (stockToolbar)
						contractScenario.Instance.replaceStockToolbar = GUILayout.Toggle(contractScenario.Instance.replaceStockToolbar, "Replace Stock Toolbar", contractSkins.configToggle);
				GUILayout.EndVertical();

				GUILayout.Space(20);

				GUILayout.BeginVertical();
					if (GUILayout.Button("Reset Contract Values", contractSkins.configButton))
					{
						dropDown = !dropDown;
						rCPopup = !rCPopup;
					}

					if (GUILayout.Button("Reset Parameter Values", contractSkins.configButton))
					{
						dropDown = !dropDown;
						rPPopup = !rPPopup;
					}

					//if (GUILayout.Button("Save To Config", contractSkins.configButton))
					//{
					//	dropDown = !dropDown;
					//	wPopup = !wPopup;
					//}
				GUILayout.EndVertical();
				GUILayout.Space(20);
			GUILayout.EndHorizontal();
		}

		//Draw some line textures to break up the window into sections
		private void windowFrame(int id)
		{
			Rect r = new Rect(WindowRect.width - (WindowRect.width / 2) - 4, 265, (WindowRect.width / 2) + 2 , 4);
			GUI.DrawTexture(r, contractSkins.footerBar);

			r.x -= 2;
			r.y = WindowRect.height - (WindowRect.height - 64);
			r.width = 4;
			r.height = WindowRect.height - 60;
			GUI.DrawTexture(r, contractSkins.verticalBar);
		}

		//Handle all of the drop down menus and pop up windows here
		//Only 1 can be active at a time
		private void dropDownMenu(int id)
		{
			if (dropDown)
			{
				if (cDropDown)
				{
					ddRect = new Rect(40, 55, 280, 160);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					for (int i = 0; i < cList.Count; i++)
					{
						cScroll = GUI.BeginScrollView(ddRect, cScroll, new Rect(0, 0, 260, 25 * cList.Count));
						Rect r = new Rect(2, (25 * i) + 2, 250, 25);
						if (GUI.Button(r, cList[i].Name, contractSkins.configDropMenu))
						{
							setContractType(cList[i]);
							cDropDown = false;
							dropDown = false;
						}
					GUI.EndScrollView();
					}
				}

				else if (pDropDown)
				{
					ddRect = new Rect(WindowRect.width - 365, 55, 280, 160);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					for (int i = 0; i < pList.Count; i++)
					{
						pScroll = GUI.BeginScrollView(ddRect, pScroll, new Rect(0, 0, 260, 25 * pList.Count));
						Rect r = new Rect(2, (25 * i) + 2, 250, 25);
						if (GUI.Button(r, pList[i].Name, contractSkins.configDropMenu))
						{
							setParameterType(pList[i]);
							pDropDown = false;
							dropDown = false;
						}
						GUI.EndScrollView();
					}
				}

				else if (zPopup)
				{
					ddRect = new Rect(WindowRect.width - 260, WindowRect.height - 100, 240, 100);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 220, 80);
					GUI.Label(r, "Warning:\nContract values set to 0.0% may no longer be adjustable", contractSkins.resetBox);
					r.x += 85;
					r.y += 60;
					r.width = 70;
					r.height = 30;
					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						dropDown = false;
						zPopup = false;
						contractScenario.Instance.warnedZero = true;
						contractScenario.Instance.allowZero = true;
					}
				}

				else if (rCPopup)
				{
					ddRect = new Rect(WindowRect.width - 300, WindowRect.height - 100, 280, 100);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 270, 70);
					GUI.Label(r, "Contract Type:\n<b>" + contractType.Name + "</b>\nWill Be Reset To Default Values", contractSkins.resetBox);
					r.x += 110;
					r.y += 60;
					r.width = 70;
					r.height = 30;
					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						dropDown = false;
						rCPopup = false;
						resetContractToDefault();
					}
				}

				else if (rPPopup)
				{
					ddRect = new Rect(WindowRect.width - 300, WindowRect.height - 100, 280, 100);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 270, 80);
					GUI.Label(r, "Parameter Type:\n<b>" + paramType.Name + "</b>\nWill Be Reset To Default Values", contractSkins.resetBox);
					r.x += 110;
					r.y += 60;
					r.width = 70;
					r.height = 30;
					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						dropDown = false;
						rCPopup = false;
						resetParameToDefault();
					}
				}

				else if (wPopup)
				{
					ddRect = new Rect(WindowRect.width - 260, WindowRect.height - 80, 240, 80);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 10, ddRect.y + 5, 220, 45);
					GUI.Label(r, "Overwrite Default Config File With Current Values?", contractSkins.resetBox);
					r.x += 85;
					r.y += 40;
					r.width = 70;
					r.height = 30;
					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						dropDown = false;
						wPopup = false;
						resetParameToDefault();
					}
				}

				else if (stockPopup)
				{
					ddRect = new Rect(WindowRect.width - 320, WindowRect.height - 120, 300, 120);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 290, 80);
					GUI.Label(r, "Warning:\nStock Contracts Toolbar Can Only Be Reloaded After A\nScene Change/Reload", contractSkins.resetBox);
					r.x += 120;
					r.y += 80;
					r.width = 70;
					r.height = 30;
					if (GUI.Button(r, "Confirm", contractSkins.resetButton))
					{
						dropDown = false;
						stockPopup = false;
						if (contractScenario.Instance.appLauncherButton != null)
							contractScenario.Instance.appLauncherButton.replaceStockApp();
						replaceStockAppLauncher = true;
						contractScenario.Instance.replaceStockToolbar = true;
					}
				}

				else if (activePopup)
				{
					ddRect = new Rect(WindowRect.width - 300, WindowRect.height - 100, 280, 100);
					GUI.Box(ddRect, "", contractSkins.dropDown);
					Rect r = new Rect(ddRect.x + 5, ddRect.y + 5, 270, 80);
					GUI.Label(r, "Contract Duration Can Only Be Adjusted For Newly Offered Contracts", contractSkins.resetBox);
					r.x += 120;
					r.y += 60;
					r.width = 50;
					r.height = 30;
					if (GUI.Button(r, "OK", contractSkins.resetButton))
					{
						dropDown = false;
						activePopup = false;
					}
				}

				else
					dropDown = false;
			}
		}

		//Label for horizontal sliders
		private void drawSliderLabel(Rect r, string min, string max, string mid = null)
		{
			Rect sr = new Rect(r.x, r.y + 9, 10, 20);
			drawLabel(sr, "|", true, true);
			if (!string.IsNullOrEmpty(mid))
			{
				sr.x += (r.width / 2);
				drawLabel(sr, "|", true, false);
				sr.x += ((r.width / 2) - 1);
			}
			else
				sr.x += (r.width - 7);
			drawLabel(sr, "|", true, false);
			sr.width = 40;
			sr.x -= r.width;
			sr.y += 11;
			drawLabel(sr, min, true, false);
			if (!string.IsNullOrEmpty(mid))
			{
				sr.x += (r.width / 2);
				drawLabel(sr, mid, true, false);
				sr.x += ((r.width / 2) - 10f);
			}
			else
				sr.x += (r.width + 62);
			drawLabel(sr, max, true, true);
		}

		//Label method for small font size
		private void drawLabel(Rect r, string txt, bool aligned, bool left)
		{
			if (txt.Length < 1)
				return;
			if (aligned)
			{
				Vector2 sz = contractSkins.smallLabel.CalcSize(new GUIContent(txt.Substring(0, 1)));
				r.x -= sz.x / 2;
				r.y -= sz.y / 2;
			}
			if (left)
				GUI.Label(r, txt, contractSkins.smallLabel);
			else
				GUI.Label(r, txt, contractSkins.smallLabel);
		}

		//Semi log scale slider for percentage adjustments
		private float logSlider (ref float f, float min, float max, Rect r, int round)
		{
			float newVal = f;
			if (!dropDown)
				f = GUI.HorizontalSlider(r, f, min, max).Mathf_Round(round);
			else
				GUI.HorizontalSlider(r, f, min, max);

			if (f >= -1 && f < -0.05)
				newVal = f + 1;
			else if (f >= -0.05 && f < 0.05)
			{
				f = 0;
				newVal = 1;
			}
			else if (f >= 0.05 && f < 1)
				newVal = (float)Math.Pow(10, f);
			else
				newVal = 10f;

			return newVal;
		}

		//Check here to see if any values have changed and update contracts accordingly
		//Only active when updating active contracts is allowed
		//the float[] is a Clone of the original because arrays are reference objects
		private void eventCheck(float newF, int pos, float[] originals, int type)
		{
			if (contractScenario.Instance.alterActive)
			{
				if (Mathf.RoundToInt(originals[pos] * 100) != Mathf.RoundToInt(newF * 100))
				{
					if (type == 0)
					{
						contractScenario.onContractChange.Fire(originals, contractType);
					}
					else if (type == 1)
					{
						contractScenario.onParamChange.Fire(originals, paramType);
					}
				}
			}
		}

		//Reset all of the slider values for the newly selected contract type
		private void setContractType(contractTypeContainer c)
		{
			contractType = c;
			cFRew = c.RewardFund.reverseLog();
			cFAdv = c.AdvanceFund.reverseLog();
			cFPen = c.PenaltyFund.reverseLog();
			cRRew = c.RewardRep.reverseLog();
			cRPen = c.PenaltyRep.reverseLog();
			cSRew = c.RewardScience.reverseLog();
			cDur = c.DurationTime.reverseLog();
			cOffer = c.MaxOffer.reverseLog();
			cActive = c.MaxActive.reverseLog();
		}

		//Reset all of the slider values for the newly selected parameter type
		private void setParameterType(paramTypeContainer p)
		{
			paramType = p;
			pFRew = p.RewardFund.reverseLog();
			pFPen = p.PenaltyFund.reverseLog();
			pRRew = p.RewardRep.reverseLog();
			pRPen = p.PenaltyRep.reverseLog();
			pSRew = p.RewardScience.reverseLog();
		}

		//Reset the current contract type to its default values
		//Values always default to 100% for now; use config file later
		//Active contracts updated only if allowed
		private void resetContractToDefault()
		{
			float[] originals = (float[])contractType.ContractValues.Clone();
			contractType.RewardFund = 1f;
			contractType.AdvanceFund = 1f;
			contractType.PenaltyFund = 1f;
			contractType.RewardRep = 1f;
			contractType.PenaltyRep = 1f;
			contractType.RewardScience = 1f;
			contractType.DurationTime = 1f;
			contractType.MaxOffer = 10f;
			contractType.MaxActive = 10f;
			setContractType(contractType);
			if (contractScenario.Instance.alterActive)
				contractScenario.onContractChange.Fire(originals, contractType);
		}

		//Reset the current parameter type to its default values
		private void resetParameToDefault()
		{
			float[] originals = (float[])paramType.ParamValues.Clone();
			paramType.RewardFund = 1f;
			paramType.PenaltyFund = 1f;
			paramType.RewardRep = 1f;
			paramType.PenaltyRep = 1f;
			paramType.RewardScience = 1f;
			setParameterType(paramType);
			if (contractScenario.Instance.alterActive)
				contractScenario.onParamChange.Fire(originals, paramType);
		}
	}
}
