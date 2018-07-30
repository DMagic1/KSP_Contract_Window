#region license
/*The MIT License (MIT)
Contract Toolbar- Addon for toolbar interface

Copyright (c) 2014 DMagic

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
using ContractsWindow.PanelInterfaces;
using UnityEngine;

namespace ContractsWindow.Toolbar
{
	class contractToolbar : MonoBehaviour
	{
		private IButton contractButton;

		private void Start()
		{
			setupToolbar();
		}

		private void setupToolbar()
		{
			if (!ToolbarManager.ToolbarAvailable)
				return;

			int sceneInt = contractUtils.currentScene(HighLogic.LoadedScene);

			contractButton = ToolbarManager.Instance.add("ContractsWindow", "ContractWindowPlus");

			if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/DMagicUtilities/ContractsWindow/Resources/ContractsIcon.png").Replace("\\", "/")))
				contractButton.TexturePath = "DMagicUtilities/ContractsWindow/Resources/ContractsIcon";
			else
				contractButton.TexturePath = "000_Toolbar/resize-cursor";

			contractButton.ToolTip = "Contract Window";
			contractButton.OnClick += (e) =>
				{
					if (contractScenario.Instance == null)
                        contractUtils.LogFormatted("Contract Scenario Not Loaded...");
					else if (contractWindow.Instance == null)
                        contractUtils.LogFormatted("Contract Window Not Loaded...");
					else
					{
						if (contractWindow.Instance.IsVisible)
						{
							contractWindow.Instance.Close();
							contractScenario.Instance.windowVisible[sceneInt] = false;
						}
						else
						{
							contractWindow.Instance.Open();
							contractScenario.Instance.windowVisible[sceneInt] = true;
						}
					}
				};
		}

		private void OnDestroy()
		{
			if (contractButton != null)
				contractButton.Destroy();
		}
	}
}
