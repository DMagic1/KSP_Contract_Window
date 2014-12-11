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
		private Rect ddRect;
		private Vector2 cScroll, pScroll;
		private paramTypeContainer paramType;
		private contractTypeContainer contractType;
		private float cFRew, cFAdv, cFPen, cRRew, cRPen, cSRew, cOffer, cActive, cDur, pFRew, pFPen, pRRew, pRPen, pSRew;

		internal override void Awake()
		{
			WindowCaption = "Contract Configuration";
			WindowRect = new Rect(40, 80, 250, 300);
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
			
		}

		private void OnDestroy()
		{
			if (InputLockManager.lockStack.ContainsKey(lockID))
				EditorLogic.fetch.Unlock(lockID);
		}

		internal override void DrawWindowPre(int id)
		{
			
		}

		internal override void DrawWindow(int id)
		{
			closeButton(id);						/* Draw the close button */

			GUILayout.BeginVertical();
				headerRegion(id);					/* Window header label */
				GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();
						contractSelectionMenu(id);	/* Drop down menu and label for the current contract type */
						contractOptions(id);		/* Contract reward/penalty sliders */
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
						parameterSelectionMenu(id);	/* Drop down menu and label for the current parameter */
						parameterOptions(id);		/* Parameter reward/penalty sliders */
					GUILayout.EndVertical();
				GUILayout.EndHorizontal();
				secondaryContractOptions(id);		/* Sliders for contract amounts and duration */
			GUILayout.EndVertical();

			dropDownMenu(id);						/* Draw the drop down menus when open */
		}

		internal override void DrawWindowPost(int id)
		{
			
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
			if(GUILayout.Button("Contract Type"))
			{
				dropDown = true;
				cDropDown = true;
			}

			GUILayout.Label(contractType.Name + ":");
		}

		//Contract options
		private void contractOptions(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Reward: {0:P0}", contractType.RewardFund));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.RewardFund = logSlider(cFRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Advance: {0:P0}", contractType.AdvanceFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.AdvanceFund = logSlider(cFAdv, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Penalty: {0:P0}", contractType.PenaltyFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.PenaltyFund = logSlider(cFPen, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Reward: {0:P0}", contractType.RewardRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.RewardRep = logSlider(cRRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Penalty: {0:P0}", contractType.PenaltyRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.PenaltyRep = logSlider(cRPen, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Science Reward: {0:P0}", contractType.RewardScience));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.RewardScience = logSlider(cSRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		//Parameter type selector
		private void parameterSelectionMenu(int id)
		{
			if (GUILayout.Button("Parameter Type"))
			{
				dropDown = true;
				pDropDown = true;
			}

			GUILayout.Label(paramType.Name + ":");
		}

		//Parameter options
		private void parameterOptions(int id)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Reward: {0:P0}", paramType.RewardFund));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				paramType.RewardFund = logSlider(pFRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Funds Penalty: {0:P0}", paramType.PenaltyFund));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				paramType.PenaltyFund = logSlider(pFPen, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Reward: {0:P0}", paramType.RewardRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				paramType.RewardRep = logSlider(pRRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Rep Penalty: {0:P0}", paramType.PenaltyRep));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				paramType.PenaltyRep = logSlider(pRPen, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();

			GUILayout.Space(8);

			GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Science Reward: {0:P0}", paramType.RewardScience));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				paramType.RewardScience = logSlider(pSRew, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
			GUILayout.EndHorizontal();
		}

		//Options for contract max amount and duration
		private void secondaryContractOptions(int id)
		{
			GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label(string.Format("Max Offered: {0:N0}", contractType.MaxOffer));

				Rect r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.MaxOffer = logSlider(cOffer, 0, 20, r, 0);

				drawSliderLabel(r, "0", "∞", "10");

				GUILayout.Label(string.Format("Max Active: {0:N0}", contractType.MaxActive));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.MaxActive = logSlider(cActive, 0, 20, r, 0);

				drawSliderLabel(r, "0", "∞", "10");

				GUILayout.Label(string.Format("Duration: {0:P0}", contractType.DurationTime));

				r = GUILayoutUtility.GetLastRect();
				r.x += 100;
				r.width = 200;

				contractType.DurationTime = logSlider(cDur, -1, 1, r, -2);

				drawSliderLabel(r, "0%", "1000%", "100%");
		}

		private void dropDownMenu(int id)
		{
			if (dropDown)
			{
				if (cDropDown)
				{
					ddRect = new Rect(10, 10, 120, 200);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					cScroll = GUI.BeginScrollView(ddRect, cScroll, new Rect (0,0,110,180));

					for (int i = 0; i < contractScenario.cTypeList.Count; i++)
					{
						Rect r = new Rect(2, 20 * i, 80, 20);
						if (GUI.Button(r, contractScenario.cTypeList.ElementAt(i).Value.Name, contractSkins.sortMenu))
						{
							contractType = contractScenario.cTypeList.ElementAt(i).Value;
						}
					}

					GUI.EndScrollView();
				}

				else if (pDropDown)
				{
					ddRect = new Rect(10, 100, 120, 200);
					GUI.Box(ddRect, "", contractSkins.dropDown);

					pScroll = GUI.BeginScrollView(ddRect, pScroll, new Rect(0, 0, 110, 180));

					for (int i = 0; i < contractScenario.pTypeList.Count; i++)
					{
						Rect r = new Rect(2, 20 * i, 80, 20);
						if (GUI.Button(r, contractScenario.pTypeList.ElementAt(i).Value.Name, contractSkins.sortMenu))
						{
							paramType = contractScenario.pTypeList.ElementAt(i).Value;
						}
					}

					GUI.EndScrollView();
				}

				else
					dropDown = false;
			}
		}

		//Label for horizontal sliders
		private void drawSliderLabel(Rect r, string min, string max, string mid = null)
		{
			Rect sr = new Rect(r.x, r.y + 7, 10, 20);
			drawLabel(sr, "|", true, true);
			if (!string.IsNullOrEmpty(mid))
			{
				sr.x += ((r.width / 2) - 5);
				drawLabel(sr, "|", true, false);
				sr.x += ((r.width / 2) - 3);
			}
			else
				sr.x += (r.width - 8);
			drawLabel(sr, "|", true, false);
			sr.width = 80;
			sr.x -= (r.width + 60);
			sr.y += 12;
			drawLabel(sr, min, true, false);
			if (!string.IsNullOrEmpty(mid))
			{
				sr.x += ((r.width / 2) + 20);
				drawLabel(sr, mid, true, false);
				sr.x += ((r.width / 2) + 42);
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

		private float logSlider (float f, float min, float max, Rect r, int round)
		{
			f = GUI.HorizontalSlider(r, f, min, max);

			return (float)Math.Pow(10, f);
		}

	}
}
