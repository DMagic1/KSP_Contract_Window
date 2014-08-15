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
		internal static GUISkin contractUnitySkin;
		internal static GUISkin contractKSPSkin;

		internal static GUIStyle newWindowStyle;

		internal static GUIStyle contractActive;
		internal static GUIStyle contractCompleted;
		internal static GUIStyle contractFailed;

		internal static GUIStyle contractActiveBehind;
		internal static GUIStyle contractCompletedBehind;
		internal static GUIStyle contractFailedBehind;

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
		internal static GUIStyle timerFinished;

		internal static GUIStyle reward;
		internal static GUIStyle advance;
		internal static GUIStyle penalty;
		internal static GUIStyle scienceReward;
		internal static GUIStyle texLabel;

		internal static GUIStyle texButton;
		internal static GUIStyle dragButton;

		internal static GUIStyle resetBox;
		internal static GUIStyle resetButton;

		internal static Texture2D fundsGreen;
		internal static Texture2D fundsRed;
		internal static Texture2D repGreen;
		internal static Texture2D repRed;
		internal static Texture2D science;
		internal static Texture2D expandIcon;
		internal static Texture2D dropDownTex;
		internal static Texture2D expandRight;
		internal static Texture2D expandRightStop;
		internal static Texture2D collapseLeft;
		internal static Texture2D collapseLeftStop;
		internal static Texture2D orderAsc;
		internal static Texture2D orderDesc;
		internal static Texture2D windowTex;
		internal static Texture2D sortIcon;
		//internal static Texture2D pinIcon;
		internal static Texture2D hideIcon;
		internal static Texture2D showIcon;
		internal static Texture2D revealHideIcon;
		internal static Texture2D revealShowIcon;
		internal static Texture2D tooltipIcon;
		internal static Texture2D buttonHover;
		internal static Texture2D headerBar;
		internal static Texture2D footerBar;
		internal static Texture2D resetIcon;

		internal override void OnGUIOnceOnly()
		{

			//Fetch icon textures
			fundsGreen = GameDatabase.Instance.GetTexture("Contracts Window/Textures/FundsGreenIcon", false);
			fundsRed = GameDatabase.Instance.GetTexture("Contracts Window/Textures/FundsRedIcon", false);
			repGreen = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RepGreenIcon", false);
			repRed = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RepRedIcon", false);
			science = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ScienceIcon", false);
			expandIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ResizeIcon", false);
			expandRight = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ExpandRight", false);
			expandRightStop = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ExpandRightStop", false);
			collapseLeft = GameDatabase.Instance.GetTexture("Contracts Window/Textures/CollapseLeft", false);
			collapseLeftStop = GameDatabase.Instance.GetTexture("Contracts Window/Textures/CollapseLeftStop", false);
			orderAsc = GameDatabase.Instance.GetTexture("Contracts Window/Textures/OrderAsc", false);
			orderDesc = GameDatabase.Instance.GetTexture("Contracts Window/Textures/OrderDesc", false);
			windowTex = GameDatabase.Instance.GetTexture("Contracts Window/Textures/WindowTex", false);
			dropDownTex = GameDatabase.Instance.GetTexture("Contracts Window/Textures/DropDownTex", false);
			sortIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/SortIcon", false);
			//pinIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/PinIcon", false);
			hideIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/HideIcon", false);
			showIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ShowIcon", false);
			revealHideIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RevealHideIcon", false);
			revealShowIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/RevealShowIcon", false);
			tooltipIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ToolTipIcon", false);
			buttonHover = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ButtonHover", false);
			headerBar = GameDatabase.Instance.GetTexture("Contracts Window/Textures/HeaderBar", false);
			footerBar = GameDatabase.Instance.GetTexture("Contracts Window/Textures/FooterBar", false);
			resetIcon = GameDatabase.Instance.GetTexture("Contracts Window/Textures/ResetIcon", false);

			//Initialize Skins
			contractKSPSkin = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.KSP);
			contractKSPSkin.button = SkinsLibrary.DefKSPSkin.button;
			SkinsLibrary.AddSkin("ContractKSPSkin", contractKSPSkin);

			contractUnitySkin = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.Unity);
			SkinsLibrary.AddSkin("ContractUnitySkin", contractUnitySkin);

			//Main Window Style
			newWindowStyle = new GUIStyle(SkinsLibrary.DefUnitySkin.window);
			newWindowStyle.name = "WindowStyle";
			newWindowStyle.padding = new RectOffset(0, 1, 20, 12);
			newWindowStyle.normal.background = windowTex;
			newWindowStyle.focused.background = newWindowStyle.normal.background;
			newWindowStyle.onNormal.background = newWindowStyle.normal.background;

			//Contract Title Style
			contractActive = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			contractActive.name = "ContractActiveText";
			contractActive.fontSize = 12;
			contractActive.alignment = TextAnchor.UpperLeft;
			contractActive.normal.textColor = XKCDColors.DustyOrange;
			contractActive.wordWrap = true;
			contractActive.margin = new RectOffset(1, 3, 2, 2);

			contractActiveBehind = new GUIStyle(contractActive);
			contractActiveBehind.name = "ContractActiveTextBehind";
			contractActiveBehind.hover.background = contractActiveBehind.normal.background;
			contractActiveBehind.hover.textColor = contractActiveBehind.normal.textColor;

			contractCompleted = new GUIStyle(contractActive);
			contractCompleted.name = "ContractCompleteText";
			contractCompleted.normal.textColor = XKCDColors.FreshGreen;

			contractCompletedBehind = new GUIStyle(contractCompleted);
			contractCompletedBehind.name = "ContractCompletedTextBehind";
			contractCompletedBehind.hover.background = contractCompletedBehind.normal.background;
			contractCompletedBehind.hover.textColor = contractCompletedBehind.normal.textColor;

			contractFailed = new GUIStyle(contractActive);
			contractFailed.name = "ContractFailedText";
			contractFailed.normal.textColor = XKCDColors.Red;

			contractFailedBehind = new GUIStyle(contractFailed);
			contractFailedBehind.name = "ContractFailedTextBehind";
			contractFailedBehind.hover.background = contractFailedBehind.normal.background;
			contractFailedBehind.hover.textColor = contractFailedBehind.normal.textColor;

			//Parameter Style
			paramText = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
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
			noteText = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			noteText.name = "ContractNoteText";
			noteText.fontSize = 11;
			noteText.wordWrap = true;
			noteText.alignment = TextAnchor.UpperLeft;
			noteText.normal.textColor = XKCDColors.AquaBlue;

			noteButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			noteButton.name = "ContractNoteButton";
			noteButton.fontSize = 10;
			noteButton.normal.background = SkinsLibrary.DefUnitySkin.label.normal.background;
			noteButton.alignment = TextAnchor.MiddleLeft;
			noteButton.normal.textColor = XKCDColors.AquaBlue;

			//Expanded Style
			timerGood = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			timerGood.name = "TimerGood";
			timerGood.fontSize = 11;
			timerGood.normal.textColor = XKCDColors.FreshGreen;
			timerGood.wordWrap = false;
			timerGood.alignment = TextAnchor.LowerCenter;

			timerBad = new GUIStyle(timerGood);
			timerBad.name = "TimerBad";
			timerBad.normal.textColor = XKCDColors.VomitYellow;

			timerFinished = new GUIStyle(timerGood);
			timerFinished.name = "TimerFinished";
			timerFinished.normal.textColor = XKCDColors.Red;

			reward = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			reward.name = "FundsReward";
			reward.fontSize = 10;
			reward.normal.textColor = XKCDColors.FreshGreen;
			//reward.normal.background = SkinsLibrary.DefUnitySkin.box.normal.background;
			reward.wordWrap = false;
			reward.alignment = TextAnchor.MiddleLeft;

			advance = new GUIStyle(reward);
			advance.name = "FundsAdvance";
			advance.normal.textColor = XKCDColors.SunYellow;

			penalty = new GUIStyle(reward);
			penalty.name = "FundsPenalty";
			penalty.normal.textColor = XKCDColors.OrangeyRed;

			texLabel = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			texLabel.name = "TexLabel";
			texLabel.padding = new RectOffset(0, 0, 0, 0);

			scienceReward = new GUIStyle(reward);
			scienceReward.name = "ScienceReward";
			scienceReward.normal.textColor = XKCDColors.AquaBlue;

			//Other Styles
			dropDown = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			dropDown.name = "DropDown";
			dropDown.normal.background = dropDownTex;

			sortMenu = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			sortMenu.name = "SortMenu";
			sortMenu.fontSize = 12;
			sortMenu.padding = new RectOffset(2, 2, 2, 2);
			sortMenu.normal.textColor = XKCDColors.White;
			sortMenu.hover.textColor = XKCDColors.AlmostBlack;
			Texture2D sortBackground = new Texture2D(1, 1);
			sortBackground.SetPixel(1, 1, XKCDColors.OffWhite);
			sortBackground.Apply();
			sortMenu.hover.background = sortBackground;
			sortMenu.alignment = TextAnchor.MiddleLeft;

			sortOrder = new GUIStyle(sortMenu);
			sortOrder.name = "SortOrder";

			texButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			texButton.name = "TexButton";
			texButton.normal.background = SkinsLibrary.DefUnitySkin.label.normal.background;
			texButton.hover.background = buttonHover;
			texButton.padding = new RectOffset(1, 1, 2, 2);

			dragButton = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			dragButton.name = "DragButton";
			dragButton.padding = new RectOffset(1, 2, 0, 0);

			resetBox = new GUIStyle(SkinsLibrary.DefUnitySkin.label);
			resetBox.name = "ResetBox";
			resetBox.fontSize = 16;
			resetBox.normal.textColor = XKCDColors.VomitYellow;
			resetBox.wordWrap = true;
			resetBox.alignment = TextAnchor.UpperCenter;

			resetButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			resetButton.name = "ResetButton";
			resetButton.fontSize = 15;
			resetButton.alignment = TextAnchor.MiddleCenter;

			//Add skins and styles to the library
			SkinsLibrary.List["ContractUnitySkin"].window = new GUIStyle(newWindowStyle);
			SkinsLibrary.List["ContractUnitySkin"].button = new GUIStyle(texButton);
			SkinsLibrary.List["ContractUnitySkin"].box = new GUIStyle(noteText);
			SkinsLibrary.List["ContractUnitySkin"].label = new GUIStyle(texLabel);

			SkinsLibrary.AddStyle("ContractUnitySkin", newWindowStyle);
			SkinsLibrary.AddStyle("ContractUnitySkin", texButton);
			SkinsLibrary.AddStyle("ContractUnitySkin", noteText);
			SkinsLibrary.AddStyle("ContractUnitySkin", texLabel);

			

		}
	}
}
