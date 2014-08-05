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
		}
	}
}
