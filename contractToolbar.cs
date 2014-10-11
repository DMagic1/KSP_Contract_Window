#region license
/*The MIT License (MIT)
Contract Toolbar- Addon for toolbar interface

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

using System.IO;
using System;
using UnityEngine;
using Toolbar;

namespace ContractsWindow
{
	[DMC_KSPAddonImproved(DMC_KSPAddonImproved.Startup.EditorAny | DMC_KSPAddonImproved.Startup.TimeElapses, false)]
	class contractToolbar : MonoBehaviour
	{
		private IButton contractButton;
		private Game.Modes mode;

		internal contractToolbar()
		{
			int sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);

			try
			{
				mode = HighLogic.CurrentGame.Mode;
			}
			catch (Exception e)
			{
				DMC_MBE.LogFormatted("Looks Like Something Went Wrong Here; Current Game Mode Cannot Be Detected: {0}", e);
				mode = Game.Modes.CAREER;
			}

			if (mode == Game.Modes.CAREER)
			{
				contractButton = ToolbarManager.Instance.add("ContractsWindow", "ContractWindowPlus");

				if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/Contracts Window/Textures/ContractsIcon.png").Replace("\\", "/")))
					contractButton.TexturePath = "Contracts Window/Textures/ContractsIcon";
				else
					contractButton.TexturePath = "000_Toolbar/resize-cursor";

				contractButton.ToolTip = "Contract Window";
				contractButton.OnClick += (e) =>
					{
						if (contractScenario.Instance == null)
							DMC_MBE.LogFormatted("Contract Scenario Not Loaded...");
						else if (contractScenario.Instance.cWin == null)
							DMC_MBE.LogFormatted("Contract Window Not Loaded...");
						else
						{
							if (contractScenario.Instance.cWin.Visible)
							{
								contractScenario.Instance.cWin.Visible = false;
								contractScenario.Instance.cWin.StopRepeatingWorker();
								contractScenario.Instance.windowVisible[sceneInt] = false;
							}
							else
							{
								contractScenario.Instance.cWin.Visible = true;
								contractScenario.Instance.cWin.StartRepeatingWorker(5);
								contractScenario.Instance.windowVisible[sceneInt] = true;
							}
						}
					};
			}
			else
				return;
		}

		internal void OnDestroy()
		{
			if (contractButton != null)
				contractButton.Destroy();
		}
	}
}
