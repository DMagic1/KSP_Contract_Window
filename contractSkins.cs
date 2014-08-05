using System;
using System.Collections.Generic;
using System.Linq;

using KSP;
using UnityEngine;

namespace Contracts_Window
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	class contractSkins: MonoBehaviourExtended
	{
		internal override void OnGUIOnceOnly()
		{
			GUISkin contractKSP = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.KSP);
			contractKSP.button = SkinsLibrary.DefKSPSkin.button;
			SkinsLibrary.AddSkin("DefaultSkin", contractKSP);



			GUISkin contractUnity = SkinsLibrary.CopySkin(SkinsLibrary.DefSkinType.Unity);
			contractUnity.button = SkinsLibrary.DefUnitySkin.button;
			SkinsLibrary.AddSkin("UnitySkin", contractUnity);

			GUIStyle styleText = new GUIStyle(SkinsLibrary.DefUnitySkin.box);
			styleText.fontSize = 10;
			styleText.normal.textColor = new Color(200, 200, 200);
			styleText.wordWrap = true;

			SkinsLibrary.List["DefaultSkin"].box = new GUIStyle(styleText);
		}
	}
}
