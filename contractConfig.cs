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
using UnityEngine;

namespace ContractsWindow
{
	class contractConfig : DMC_MBW
	{
		private const string lockID = "ContractConfigLockID";
		private bool dropDown, cDropDown, pDropDown;
		private bool spacecenterLocked, trackingLocked, editorLocked;
		private Rect ddRect;
		private Vector2 cScroll, pScroll;
		private paramTypeContainer paramType;
		private contractTypeContainer contractType;
		private float cFRew, cFAdv, cFPen, cRRew, cRPen, cSRew, cOffer, cActive, cDur, pFRew, pFPen, pRRew, pRPen, pSRew = 0f;

		internal override void Awake()
		{
			WindowCaption = "Contract Configuration";
			WindowRect = new Rect(40, 80, 780, 360);
			WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
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
			if (contractScenario.cTypeList.Count > 0 && contractScenario.pTypeList.Count > 0)
			{
				contractType = contractScenario.cTypeList.ElementAt(0).Value;
				paramType = contractScenario.pTypeList.ElementAt(0).Value;
			}
		}

		private void OnDestroy()
		{
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
		}

		internal override void DrawWindowPre(int id)
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
			}
		}

		internal override void DrawWindow(int id)
		{
			closeButton(id);						/* Draw the close button */

			GUILayout.BeginVertical();
				//headerRegion(id);					/* Window header label */
				GUILayout.BeginHorizontal();
					GUILayout.Space(8);
					GUILayout.BeginVertical();
						contractSelectionMenu(id);	/* Drop down menu and label for the current contract type */
						contractOptions(id);		/* Contract reward/penalty sliders */
					GUILayout.EndVertical();
					GUILayout.FlexibleSpace();
					GUILayout.BeginVertical();
						parameterSelectionMenu(id);	/* Drop down menu and label for the current parameter */
						parameterOptions(id);		/* Parameter reward/penalty sliders */
					GUILayout.EndVertical();
					GUILayout.Space(8);
				GUILayout.EndHorizontal();
				secondaryContractOptions(id);		/* Sliders for contract amounts and duration */
			GUILayout.EndVertical();

			dropDownMenu(id);						/* Draw the drop down menus when open */
		}

		internal override void DrawWindowPost(int id)
		{
			if (dropDown && Event.current.type == EventType.mouseDown && !ddRect.Contains(Event.current.mousePosition))
				dropDown = false;
		}

		private void closeButton(int id)
		{

		}

		private void headerRegion(int id)
		{
			GUILayout.Label("Contract Configuration");
		}

		//Contract type selector
		private void contractSelectionMenu(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(GUILayout.Button("Contract Type:"))
				{
					dropDown = true;
					cDropDown = true;
				}

				GUILayout.Space(8);

				if (contractType != null)
					GUILayout.Label(contractType.Name);
				else
					GUILayout.Label("Unknown:");
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		//Contract options
		private void contractOptions(int id)
		{
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Reward: {0:P0}", contractType.RewardFund));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.RewardFund = logSlider(ref cFRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Advance: {0:P0}", contractType.AdvanceFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.AdvanceFund = logSlider(ref cFAdv, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Penalty: {0:P0}", contractType.PenaltyFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.PenaltyFund = logSlider(ref cFPen, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Reward: {0:P0}", contractType.RewardRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.RewardRep = logSlider(ref cRRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Penalty: {0:P0}", contractType.PenaltyRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.PenaltyRep = logSlider(ref cRPen, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Science Reward: {0:P0}", contractType.RewardScience));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				contractType.RewardScience = logSlider(ref cSRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		//Parameter type selector
		private void parameterSelectionMenu(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Parameter Type:"))
				{
					dropDown = true;
					pDropDown = true;
				}

				GUILayout.Space(8);

				if (paramType != null)
					GUILayout.Label(paramType.Name);
				else
					GUILayout.Label("Unknown:");
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}

		//Parameter options
		private void parameterOptions(int id)
		{
			GUILayout.Space(12);
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Reward: {0:P0}", paramType.RewardFund));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				paramType.RewardFund = logSlider(ref pFRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Penalty: {0:P0}", paramType.PenaltyFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				paramType.PenaltyFund = logSlider(ref pFPen, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Reward: {0:P0}", paramType.RewardRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				paramType.RewardRep = logSlider(ref pRRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Penalty: {0:P0}", paramType.PenaltyRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				paramType.PenaltyRep = logSlider(ref pRPen, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(16);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Science Reward: {0:P0}", paramType.RewardScience));

				r = GUILayoutUtility.GetLastRect();
				r.x += 150;
				r.width = 200;

				paramType.RewardScience = logSlider(ref pSRew, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		//Options for contract max amount and duration
		private void secondaryContractOptions(int id)
		{
			GUILayout.Space(30);
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Max Offered: {0:N0}", contractType.MaxOffer));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 105;
				r.width = 120;

				contractType.MaxOffer = logSlider(ref cOffer, 0, 20, r, 0);

				drawSliderLabel(r, "0", "∞", "10");

				GUILayout.Space(145);

				GUILayout.Label(string.Format("Max Active: {0:N0}", contractType.MaxActive));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 120;

				contractType.MaxActive = logSlider(ref cActive, 0, 20, r, 0);

				drawSliderLabel(r, "0", "∞", "10");

				GUILayout.Space(145);

				GUILayout.Label(string.Format("Duration: {0:P0}", contractType.DurationTime));

				r = GUILayoutUtility.GetLastRect();
				r.x += 110;
				r.width = 160;

				contractType.DurationTime = logSlider(ref cDur, -1, 1, r, 2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		private void dropDownMenu(int id)
		{
			if (dropDown)
			{
				if (cDropDown)
				{
					ddRect = new Rect(75, 40, 190, 160);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					for (int i = 0; i < contractScenario.cTypeList.Count; i++)
					{
						cScroll = GUI.BeginScrollView(ddRect, cScroll, new Rect(0, 0, 170, 20 * contractScenario.cTypeList.Count));
						Rect r = new Rect(2, 20 * i, 160, 20);
						if (GUI.Button(r, contractScenario.cTypeList.ElementAt(i).Value.Name, contractSkins.sortMenu))
						{
							contractType = contractScenario.cTypeList.ElementAt(i).Value;
							cDropDown = false;
							dropDown = false;
						}
					GUI.EndScrollView();
					}

				}

				else if (pDropDown)
				{
					ddRect = new Rect(WindowRect.width - 250, 40, 200, 160);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					for (int i = 0; i < contractScenario.pTypeList.Count; i++)
					{
						pScroll = GUI.BeginScrollView(ddRect, pScroll, new Rect(0, 0, 180, 20 * contractScenario.pTypeList.Count));
						Rect r = new Rect(2, 20 * i, 170, 20);
						if (GUI.Button(r, contractScenario.pTypeList.ElementAt(i).Value.Name, contractSkins.sortMenu))
						{
							paramType = contractScenario.pTypeList.ElementAt(i).Value;
							pDropDown = false;
							dropDown = false;
						}
						GUI.EndScrollView();
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
				sr.x += ((r.width / 2) - 2);
			}
			else
				sr.x += (r.width - 8);
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

		private float logSlider (ref float f, float min, float max, Rect r, int round)
		{
			f = GUI.HorizontalSlider(r, f, min, max).Mathf_Round(round);

			if (f >= -1 && f < 0)
			{
				return f + 1;
			}
			else
				return (float)Math.Pow(10, f);
		}

	}
}
