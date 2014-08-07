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

		internal override void OnGUIOnceOnly()
		{
			GUISkin contractKSP = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.KSP);
			contractKSP.button = SkinsLibrary.DefKSPSkin.button;
			SkinsLibrary.AddSkin("DefaultSkin", contractKSP);

			GUISkin contractUnity = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.Unity);
			contractUnity.button = SkinsLibrary.DefUnitySkin.button;
			contractUnity.box = SkinsLibrary.DefUnitySkin.box;
			SkinsLibrary.AddSkin("UnitySkin", contractUnity);

			//Contract Title Skin
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

			//Parameter Skin

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

			//Note Skin

			noteText = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			noteText.name = "ContractNoteText";
			noteText.fontSize = 11;
			noteText.wordWrap = true;
			noteText.alignment = TextAnchor.UpperLeft;
			noteText.normal.textColor = XKCDColors.AquaBlue;

			noteButton = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			noteButton.name = "ContractNoteButton";
			noteButton.fontSize = 12;
			noteButton.alignment = TextAnchor.MiddleCenter;
			noteButton.normal.textColor = XKCDColors.AquaBlue;

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

		}
	}
}
