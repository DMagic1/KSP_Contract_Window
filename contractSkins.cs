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
	class contractSkins: DMC_MBE
	{
		internal static GUISkin contractUnitySkin;
		internal static GUISkin contractKSPSkin;

		internal static GUIStyle newWindowStyle;

		internal static GUIStyle missionLabel;

		internal static GUIStyle contractActive;
		internal static GUIStyle contractCompleted;
		internal static GUIStyle contractFailed;

		internal static GUIStyle contractActiveBehind;
		internal static GUIStyle contractCompletedBehind;
		internal static GUIStyle contractFailedBehind;

		internal static GUIStyle paramText;
		internal static GUIStyle paramSub;
		internal static GUIStyle paramCompleted;
		internal static GUIStyle paramFailed;

		internal static GUIStyle noteText;

		internal static GUIStyle dropDown;
		internal static GUIStyle sortMenu;
		internal static GUIStyle missionMenu;
		internal static GUIStyle missionMenuCenter;
		internal static GUIStyle missionTextBox;

		internal static GUIStyle timerGood;
		internal static GUIStyle timerBad;
		internal static GUIStyle timerFinished;

		internal static GUIStyle reward;
		internal static GUIStyle penalty;
		internal static GUIStyle scienceReward;
		internal static GUIStyle texLabel;

		internal static GUIStyle texButton;
		internal static GUIStyle texButtonSmall;
		internal static GUIStyle dragButton;

		internal static GUIStyle resetBox;
		internal static GUIStyle resetButton;

		internal static GUIStyle agentName;
		internal static GUIStyle agentBackground;

		internal static GUIStyle toolbarToggle;

		internal static Texture2D fundsGreen;
		internal static Texture2D fundsRed;
		internal static Texture2D repGreen;
		internal static Texture2D repRed;
		internal static Texture2D science;
		internal static Texture2D expandIcon;
		internal static Texture2D dropDownTex;
		internal static Texture2D orderAsc;
		internal static Texture2D orderDesc;
		internal static Texture2D windowTex;
		internal static Texture2D sortIcon;
		internal static Texture2D hideIcon;
		internal static Texture2D showIcon;
		internal static Texture2D closeIcon;
		internal static Texture2D revealHideIcon;
		internal static Texture2D revealShowIcon;
		internal static Texture2D tooltipIcon;
		internal static Texture2D buttonHover;
		internal static Texture2D headerBar;
		internal static Texture2D footerBar;
		internal static Texture2D verticalBar;
		internal static Texture2D resetIcon;
		internal static Texture2D partIcon;
		internal static Texture2D noteIcon;
		internal static Texture2D noteIconOff;
		internal static Texture2D goldStar;
		internal static Texture2D goldStarTwo;
		internal static Texture2D goldStarThree;
		internal static Texture2D pinIcon;
		internal static Texture2D pinDownIcon;
		internal static Texture2D fontSize;
		internal static Texture2D windowSize;
		internal static Texture2D settingsIcon;
		internal static Texture2D agencyIcon;
		internal static Texture2D toolbarIcon;
		internal static Texture2D missionIcon;
		internal static Texture2D cancelMissionIcon;
		internal static Texture2D missionSelectionIcon;

		internal static int normalFontSize = 0;
		internal static int windowFontSize = 0;

		protected override void OnGUIOnceOnly()
		{
			//Fetch icon textures
			fundsGreen = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/FundsGreenIcon", false);
			fundsRed = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/FundsRedIcon", false);
			repGreen = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/RepGreenIcon", false);
			repRed = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/RepRedIcon", false);
			science = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ScienceIcon", false);
			expandIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ResizeIcon", false);
			orderAsc = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/OrderAsc", false);
			orderDesc = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/OrderDesc", false);
			windowTex = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/WindowTex", false);
			dropDownTex = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/DropDownTex", false);
			sortIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/SortIcon", false);
			hideIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/HideIcon", false);
			showIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ShowIcon", false);
			revealHideIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/RevealHideIcon", false);
			revealShowIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/RevealShowIcon", false);
			tooltipIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ToolTipIcon", false);
			buttonHover = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ButtonHover", false);
			headerBar = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/HeaderBar", false);
			footerBar = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/FooterBar", false);
			verticalBar = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/VerticalBar", false);
			resetIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ResetIcon", false);
			partIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/PartIcon", false);
			noteIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/NoteIcon", false);
			noteIconOff = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/NoteIconOff", false);
			goldStar = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/GoldStar", false);
			goldStarTwo = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/GoldStarTwo", false);
			goldStarThree = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/GoldStarThree", false);
			pinIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/PinIcon", false);
			pinDownIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/PinDownIcon", false);
			closeIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/CloseIcon", false);
			fontSize = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/FontSizeIcon", false);
			windowSize = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/WindowSizeIcon", false);
			settingsIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ToolbarSettingsIcon", false);
			agencyIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/AgentIcon", false);
			toolbarIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/ContractsIconApp", false);
			missionIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/CheckBoxIcon", false);
			cancelMissionIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/CheckCloseIcon", false);
			missionSelectionIcon = GameDatabase.Instance.GetTexture("ContractsWindow/Textures/MissionListIcon", false);

			initializeSkins();
		}

		internal static void initializeSkins()
		{
			//Initialize Skins
			contractKSPSkin = DMC_SkinsLibrary.CopySkin(DMC_SkinsLibrary.DefSkinType.KSP);
			contractKSPSkin.button = DMC_SkinsLibrary.DefKSPSkin.button;
			DMC_SkinsLibrary.AddSkin("ContractKSPSkin", contractKSPSkin);

			contractUnitySkin = DMC_SkinsLibrary.CopySkin(DMC_SkinsLibrary.DefSkinType.Unity);
			DMC_SkinsLibrary.AddSkin("ContractUnitySkin", contractUnitySkin);

			//Main Window Style
			newWindowStyle = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.window);
			newWindowStyle.name = "WindowStyle";
			newWindowStyle.fontSize = 14 + normalFontSize + windowFontSize;
			newWindowStyle.fontStyle = FontStyle.Bold;
			newWindowStyle.padding = new RectOffset(0, 1, 20, 12);
			newWindowStyle.normal.background = windowTex;
			newWindowStyle.focused.background = newWindowStyle.normal.background;
			newWindowStyle.onNormal.background = newWindowStyle.normal.background;

			missionLabel = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			missionLabel.name = "MissionLabel";
			missionLabel.fontSize = 13 + normalFontSize + windowFontSize;
			missionLabel.alignment = TextAnchor.MiddleCenter;
			missionLabel.wordWrap = false;
			missionLabel.fontStyle = FontStyle.Bold;

			//Contract Title Style
			contractActive = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.button);
			contractActive.name = "ContractActiveText";
			contractActive.fontSize = 12 + normalFontSize + windowFontSize;
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
			paramText = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			paramText.name = "ParameterText";
			paramText.fontSize = 11 + normalFontSize + windowFontSize;
			paramText.normal.textColor = XKCDColors.PaleGrey;
			paramText.wordWrap = true;
			paramText.alignment = TextAnchor.UpperLeft;

			paramSub = new GUIStyle(paramText);
			paramSub.name = "ParamSubText";
			paramSub.normal.textColor = XKCDColors.LightGrey;

			paramCompleted = new GUIStyle(paramText);
			paramCompleted.name = "ParameterCompleteText";
			paramCompleted.normal.textColor = XKCDColors.FreshGreen;

			paramFailed = new GUIStyle(paramText);
			paramFailed.name = "ParameterFailedText";
			paramFailed.normal.textColor = XKCDColors.Red;

			//Note Style
			noteText = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			noteText.name = "ContractNoteText";
			noteText.fontSize = 11 + normalFontSize + windowFontSize;
			noteText.wordWrap = true;
			noteText.alignment = TextAnchor.UpperLeft;
			noteText.normal.textColor = XKCDColors.AquaBlue;

			//Expanded Style
			timerGood = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			timerGood.name = "TimerGood";
			timerGood.fontSize = 11 + normalFontSize + windowFontSize;
			timerGood.normal.textColor = XKCDColors.FreshGreen;
			timerGood.wordWrap = false;
			timerGood.alignment = TextAnchor.UpperCenter;
			timerGood.padding = new RectOffset(1, 1, 1, 2);

			timerBad = new GUIStyle(timerGood);
			timerBad.name = "TimerBad";
			timerBad.normal.textColor = XKCDColors.VomitYellow;

			timerFinished = new GUIStyle(timerGood);
			timerFinished.name = "TimerFinished";
			timerFinished.normal.textColor = XKCDColors.Red;

			reward = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			reward.name = "FundsReward";
			reward.fontSize = 10 + normalFontSize + windowFontSize;
			reward.normal.textColor = XKCDColors.FreshGreen;
			reward.wordWrap = false;
			reward.alignment = TextAnchor.MiddleLeft;

			penalty = new GUIStyle(reward);
			penalty.name = "FundsPenalty";
			penalty.normal.textColor = XKCDColors.OrangeyRed;

			texLabel = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			texLabel.name = "TexLabel";
			texLabel.padding = new RectOffset(0, 0, 0, 0);

			scienceReward = new GUIStyle(reward);
			scienceReward.name = "ScienceReward";
			scienceReward.normal.textColor = XKCDColors.AquaBlue;
			scienceReward.padding = new RectOffset(1, 1, 1, 3);

			//Other Styles
			dropDown = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.box);
			dropDown.name = "DropDown";
			dropDown.normal.background = dropDownTex;

			sortMenu = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			sortMenu.name = "SortMenu";
			sortMenu.fontSize = 12 + normalFontSize + windowFontSize;
			sortMenu.padding = new RectOffset(2, 2, 2, 2);
			sortMenu.normal.textColor = XKCDColors.White;
			sortMenu.hover.textColor = XKCDColors.AlmostBlack;
			Texture2D sortBackground = new Texture2D(1, 1);
			sortBackground.SetPixel(1, 1, XKCDColors.OffWhite);
			sortBackground.Apply();
			sortMenu.hover.background = sortBackground;
			sortMenu.alignment = TextAnchor.MiddleLeft;

			missionMenu = new GUIStyle(sortMenu);
			missionMenu.name = "MissionMenu";
			missionMenu.fontSize = 13 + normalFontSize + windowFontSize;

			missionMenuCenter = new GUIStyle(missionMenu);
			missionMenuCenter.alignment = TextAnchor.MiddleCenter;

			missionTextBox = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.textField);
			missionTextBox.name = "MissionTextBox";
			missionTextBox.fontSize = 12 + normalFontSize + windowFontSize;
			missionTextBox.alignment = TextAnchor.MiddleCenter;

			texButton = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.button);
			texButton.name = "TexButton";
			texButton.normal.background = DMC_SkinsLibrary.DefUnitySkin.label.normal.background;
			texButton.hover.background = buttonHover;
			texButton.padding = new RectOffset(1, 1, 2, 2);

			texButtonSmall = new GUIStyle(texButton);
			texButtonSmall.name = "TexButtonSmall";
			texButtonSmall.padding = new RectOffset(1, 1, 1, 1);

			dragButton = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			dragButton.name = "DragButton";
			dragButton.padding = new RectOffset(1, 2, 0, 0);

			resetBox = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			resetBox.name = "ResetBox";
			resetBox.fontSize = 14;
			resetBox.normal.textColor = XKCDColors.VomitYellow;
			resetBox.fontStyle = FontStyle.Bold;
			resetBox.wordWrap = true;
			resetBox.alignment = TextAnchor.UpperCenter;

			resetButton = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.button);
			resetButton.name = "ResetButton";
			resetButton.fontSize = 15;
			resetButton.fontStyle = FontStyle.Bold;
			resetButton.alignment = TextAnchor.MiddleCenter;

			agentName = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.label);
			agentName.name = "AgentName";
			agentName.normal.textColor = XKCDColors.White;
			agentName.fontSize = 13 + normalFontSize + windowFontSize;
			agentName.fontStyle = FontStyle.Bold;
			agentName.wordWrap = true;
			agentName.alignment = TextAnchor.MiddleCenter;

			agentBackground = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.box);
			agentBackground.name = "AgentBackground";
			Texture2D agentBackDrop = new Texture2D(1, 1);
			agentBackDrop.SetPixel(1, 1, XKCDColors.White);
			agentBackDrop.Apply();
			agentBackground.normal.background = agentBackDrop;

			toolbarToggle = new GUIStyle(DMC_SkinsLibrary.DefUnitySkin.toggle);
			toolbarToggle.name = "ToolbarToggle";
			toolbarToggle.fontStyle = FontStyle.Bold;
			toolbarToggle.fontSize = 13 + normalFontSize + windowFontSize;

			//Add skins and styles to the library
			DMC_SkinsLibrary.List["ContractUnitySkin"].window = new GUIStyle(newWindowStyle);
			DMC_SkinsLibrary.List["ContractUnitySkin"].button = new GUIStyle(texButton);
			DMC_SkinsLibrary.List["ContractUnitySkin"].box = new GUIStyle(noteText);
			DMC_SkinsLibrary.List["ContractUnitySkin"].label = new GUIStyle(texLabel);
			DMC_SkinsLibrary.List["ContractUnitySkin"].textField = new GUIStyle(missionTextBox);
			DMC_SkinsLibrary.List["ContractUnitySkin"].toggle = new GUIStyle(toolbarToggle);

			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", newWindowStyle);
			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", texButton);
			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", noteText);
			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", texLabel);
			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", missionTextBox);
			DMC_SkinsLibrary.AddStyle("ContractUnitySkin", toolbarToggle);
		}
	}
}
