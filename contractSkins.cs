using System;
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

			noteText = new GUIStyle(SkinsLibrary.DefUnitySkin.button);
			noteText.name = "ContractNoteText";
			noteText.fontSize = 11;
			noteText.wordWrap = true;
			noteText.alignment = TextAnchor.UpperLeft;
			noteText.normal.textColor = XKCDColors.AquaBlue;

			SkinsLibrary.List["UnitySkin"].button = new GUIStyle(contractActive);
			SkinsLibrary.List["UnitySkin"].box = new GUIStyle(paramText);
			SkinsLibrary.AddStyle("UnitySkin", "ParameterText", paramText);
			SkinsLibrary.AddStyle("UnitySkin", "ParameterCompleteText", paramCompleted);
			SkinsLibrary.AddStyle("UnitySkin", "ParameterFailedText", paramFailed);
			SkinsLibrary.AddStyle("UnitySkin", "ContractActiveText", contractActive);
			SkinsLibrary.AddStyle("UnitySkin", "ContractCompleteText", contractCompleted);
			SkinsLibrary.AddStyle("UnitySkin", "ContractFailedText", contractFailed);
			SkinsLibrary.AddStyle("UnitySkin", "ContractNoteText", noteText);

		}
	}
}
