#region license
/*The MIT License (MIT)
Contract Stock Toolbar- Addon for stock app launcher interface

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
using System.Collections;

using UnityEngine;

namespace ContractsWindow.Toolbar
{
	public class contractStockToolbar : DMC_MBE
	{
		private ApplicationLauncherButton stockToolbarButton = null;

		internal override void Start()
		{
			setupToolbar();
		}

		private void setupToolbar()
		{
			LogFormatted_DebugOnly("Starting App Launcher Manager");
			StartCoroutine(addButton());
		}

		internal override void OnDestroy()
		{
			LogFormatted_DebugOnly("Destroying App Launcher Manager");
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);
			removeButton(HighLogic.LoadedScene);
		}

		IEnumerator addButton()
		{
			LogFormatted_DebugOnly("Waiting For Application Launcher...");

			while (!ApplicationLauncher.Ready)
				yield return null;

			LogFormatted_DebugOnly("Adding App Launcher Button");
			stockToolbarButton = ApplicationLauncher.Instance.AddModApplication(toggleOn, toggleOff, null, null, null, null, (ApplicationLauncher.AppScenes)63, contractSkins.toolbarIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);

			LogFormatted_DebugOnly("App Launcher Button Added");
		}

		private void removeButton(GameScenes scene)
		{
			LogFormatted_DebugOnly("Removing App Launcher Button");
			ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
			LogFormatted_DebugOnly("App Launcher Button Removed");
		}

		internal void replaceStockApp()
		{
			StartCoroutine(replaceStockContractApp());
		}

		IEnumerator replaceStockContractApp()
		{
			while (ContractsApp.Instance.appLauncherButton == null)
				yield return null;

			if (stockToolbarButton != null)
			{
				LogFormatted_DebugOnly("Replacing Stock Contracts App With Contracts +");

				ApplicationLauncherButton stockContracts = ContractsApp.Instance.appLauncherButton;

				stockContracts.toggleButton.onDisable();

				stockContracts.toggleButton.onTrue = stockToolbarButton.toggleButton.onTrue;
				stockContracts.toggleButton.onFalse = stockToolbarButton.toggleButton.onFalse;
				stockContracts.toggleButton.onHover = stockToolbarButton.toggleButton.onHover;
				stockContracts.toggleButton.onHoverOut = stockToolbarButton.toggleButton.onHoverOut;
				stockContracts.toggleButton.onEnable = stockToolbarButton.toggleButton.onEnable;
				stockContracts.toggleButton.onDisable = stockToolbarButton.toggleButton.onDisable;
				//stockContracts.SetSprite(contractSkins.toolbarSprite); //Needs to be replaced with sprite
				stockContracts.Setup(contractSkins.toolbarIcon);

				LogFormatted_DebugOnly("Stock Contracts App Replaced With Contracts Window +");

				try
				{
					ApplicationLauncher.Instance.RemoveModApplication(stockToolbarButton);
					LogFormatted_DebugOnly("Contracts Window + Toolbar Removed");
				}
				catch (Exception e)
				{
					LogFormatted("Error In Removing Contracts Window + Toolbar App After Replacing Stock App: {0}", e);
				}
			}
			else
				LogFormatted("Contracts Window + App Launcher Button Not Initialized; Retaining Stock Contracts App");
		}

		private void toggleOn()
		{
			int sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);
			if (contractScenario.Instance == null)
				LogFormatted("Contract Scenario Not Loaded...");
			else if (contractScenario.Instance.cWin == null)
				LogFormatted("Contract Window Not Loaded...");
			else
			{
				contractScenario.Instance.cWin.Visible = true;
				contractScenario.Instance.cWin.StartRepeatingWorker(5);
				contractScenario.Instance.windowVisible[sceneInt] = true;
			}
		}

		private void toggleOff()
		{
			int sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);
			if (contractScenario.Instance == null)
				LogFormatted("Contract Scenario Not Loaded...");
			else if (contractScenario.Instance.cWin == null)
				LogFormatted("Contract Window Not Loaded...");
			else
			{
				contractScenario.Instance.cWin.Visible = false;
				contractScenario.Instance.cWin.StopRepeatingWorker();
				contractScenario.Instance.windowVisible[sceneInt] = false;
			}
		}

	}
}
