#region license
/*The MIT License (MIT)
Contract Skins - Addon to initialize GUISkins and GUIStyles

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

using System.Collections.Generic;
using System.Linq;

using KSP;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class contractSkins: MonoBehaviourExtended
	{
		internal static GUIStyle contractActive;
		internal static GUIStyle contractCompleted;
		internal static GUIStyle contractFailed;

		internal static GUIStyle paramText;
		internal static GUIStyle paramCompleted;
		internal static GUIStyle paramFailed;

		internal static GUIStyle noteText;
		internal static GUIStyle noteButton;

		internal static GUIStyle dropDown;
		internal static GUIStyle sortMenu;
		internal static GUIStyle sortOrder;

		internal static GUIStyle timerGood;
		internal static GUIStyle timerBad;

		internal static GUIStyle fundsReward;
		internal static GUIStyle fundsAdvance;
		internal static GUIStyle fundsPenalty;
		internal static GUIStyle repReward;
		internal static GUIStyle repPenalty;
		internal static GUIStyle scienceReward;

		internal static Texture2D fundsGreen;
		//internal static Texture2D fundsRed;
		//internal static Texture2D repGreen;
		//internal static Texture2D repRed;
		//internal static Texture2D science;
		internal static Texture2D expandIcon;
		internal static Texture2D dropDownTex;

		internal override void OnGUIOnceOnly()
		{

			//Fetch icon textures
			fundsGreen = GameDatabase.Instance.GetTexture("Contracts Window/Textures/FundsGreenIcon", false);
			//fundsRed = GameDatabase.Instance.GetTexture("Contracts Window/Textures/FundsRedIcon", false);
			//repGreen = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RepGreenIcon", false);
			//repRed = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RepRedIcon", false);
			//science = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ScienceIcon", false);
			expandIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ResizeIcon", false);
			dropDownTex = new Texture2D(1, 1);
			dropDownTex.SetPixel(0, 0, XKCDColors.AlmostBlack);
			dropDownTex.Apply();

			//Initialize Skins
			GUISkin contractKSP = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.KSP);
			contractKSP.button = SkinsLibrary.DefKSPSkin.button;
			SkinsLibrary.AddSkin("DefaultSkin", contractKSP);

			GUISkin contractUnity = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.Unity);
			contractUnity.button = SkinsLibrary.DefUnitySkin.button;
			contractUnity.box = SkinsLibrary.DefUnitySkin.box;
			SkinsLibrary.AddSkin("UnitySkin", contractUnity);

			//Contract Title Style
			contractActive = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			contractActive.name = "ContractActiveText";
			contractActive.fontSize = 12;
			contractActive.alignment = TextAnchor.UpperLeft;
			contractActive.normal.textColor = XKCDColors.DustyOrange;
			contractActive.wordWrap = true;

			contractCompleted = new GUIStyle(contractActive);
			contractCompleted.name = "ContractCompleteText";
			contractCompleted.normal.textColor = XKCDColors.FreshGreen;

			contractFailed = new GUIStyle(contractActive);
			contractFailed.name = "ContractFailedText";
			contractFailed.normal.textColor = XKCDColors.Red;

			//Parameter Style
			paramText = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			paramText.name = "ParameterText";
			paramText.fontSize = 11;
			paramText.normal.textColor = new Color(220, 220, 220);
			paramText.wordWrap = true;
			paramText.alignment = TextAnchor.UpperLeft;

			paramCompleted = new GUIStyle(paramText);
			paramCompleted.name = "ParameterCompleteText";
			paramCompleted.normal.textColor = XKCDColors.FreshGreen;

			paramFailed = new GUIStyle(paramText);
			paramFailed.name = "ParameterFailedText";
			paramFailed.normal.textColor = XKCDColors.Red;

			//Note Style
			noteText = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			noteText.name = "ContractNoteText";
			noteText.fontSize = 11;
			noteText.wordWrap = true;
			noteText.alignment = TextAnchor.UpperLeft;
			noteText.normal.textColor = XKCDColors.AquaBlue;

			noteButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			noteButton.name = "ContractNoteButton";
			noteButton.fontSize = 12;
			noteButton.alignment = TextAnchor.MiddleLeft;
			noteButton.normal.textColor = XKCDColors.AquaBlue;

			//Expanded Style
			timerGood = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			timerGood.name = "TimerGood";
			timerGood.fontSize = 11;
			timerGood.normal.textColor = XKCDColors.FreshGreen;
			timerGood.wordWrap = false;
			timerGood.alignment = TextAnchor.MiddleCenter;

			timerBad = new GUIStyle(timerGood);
			timerBad.name = "TimerBad";
			timerBad.normal.textColor = XKCDColors.VomitYellow;

			fundsReward = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			fundsReward.name = "FundsReward";
			fundsReward.fontSize = 10;
			fundsReward.normal.textColor = XKCDColors.FreshGreen;
			fundsReward.wordWrap = false;
			fundsReward.alignment = TextAnchor.MiddleLeft;

			fundsAdvance = new GUIStyle(fundsReward);
			fundsAdvance.name = "FundsAdvance";
			fundsAdvance.normal.textColor = XKCDColors.SunYellow;

			fundsPenalty = new GUIStyle(fundsReward);
			fundsPenalty.name = "FundsPenalty";
			fundsPenalty.normal.textColor = XKCDColors.Red;

			repReward = new GUIStyle(fundsReward);
			repReward.name = "RepReward";

			repPenalty = new GUIStyle(fundsPenalty);
			repPenalty.name = "RepPenalty";

			scienceReward = new GUIStyle(fundsReward);
			scienceReward.name = "ScienceReward";
			scienceReward.normal.textColor = XKCDColors.AquaBlue;

			//Other Styles
			dropDown = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			dropDown.name = "DropDown";
			dropDown.normal.background = dropDownTex;

			sortMenu = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			sortMenu.name = "SortMenu";
			sortMenu.fontSize = 12;
			sortMenu.normal.textColor = XKCDColors.White;
			sortMenu.alignment = TextAnchor.MiddleLeft;

			sortOrder = new GUIStyle(sortMenu);
			sortOrder.name = "SortOrder";
			sortOrder.alignment = TextAnchor.MiddleCenter;

			//Add skins and styles to the library
			SkinsLibrary.List["UnitySkin"].button = new GUIStyle(contractActive);
			SkinsLibrary.List["UnitySkin"].box = new GUIStyle(paramText);

			SkinsLibrary.AddStyle("UnitySkin", "ParameterText", paramText);
			SkinsLibrary.AddStyle("UnitySkin", "ParameterCompleteText", paramCompleted);
			SkinsLibrary.AddStyle("UnitySkin", "ParameterFailedText", paramFailed);
			SkinsLibrary.AddStyle("UnitySkin", "ContractActiveText", contractActive);
			SkinsLibrary.AddStyle("UnitySkin", "ContractCompleteText", contractCompleted);
			SkinsLibrary.AddStyle("UnitySkin", "ContractFailedText", contractFailed);
			SkinsLibrary.AddStyle("UnitySkin", "ContractNoteText", noteText);
			SkinsLibrary.AddStyle("UnitySkin", "ContractNoteButton", noteButton);
			SkinsLibrary.AddStyle("UnitySkin", "TimerGood", timerGood);
			SkinsLibrary.AddStyle("UnitySkin", "TimerBad", timerBad);
			SkinsLibrary.AddStyle("UnitySkin", "FundsReward", fundsReward);
			SkinsLibrary.AddStyle("UnitySkin", "FundsAdvance", fundsAdvance);
			SkinsLibrary.AddStyle("UnitySkin", "FundsPenalty", fundsPenalty);
			SkinsLibrary.AddStyle("UnitySkin", "RepReward", repReward);
			SkinsLibrary.AddStyle("UnitySkin", "RepPenalty", repPenalty);
			SkinsLibrary.AddStyle("UnitySkin", "ScienceReward", scienceReward);
			SkinsLibrary.AddStyle("UnitySkin", "DropDown", dropDown);
			SkinsLibrary.AddStyle("UnitySkin", "SortMenu", sortMenu);
			SkinsLibrary.AddStyle("UnitySkin", "SortOrder", sortOrder);

		}
	}
}
