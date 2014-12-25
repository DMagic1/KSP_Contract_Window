using System;
using System.Collections;

using UnityEngine;

namespace ContractsWindow.Toolbar
{
	public class contractStockToolbar : DMC_MBE
	{
		private ApplicationLauncherButton stockButton = null;

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
			stockButton = ApplicationLauncher.Instance.AddModApplication(toggleOn, toggleOff, null, null, null, null, (ApplicationLauncher.AppScenes)63, contractSkins.toolbarIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);

			LogFormatted_DebugOnly("App Launcher Button Added");
		}

		private void removeButton(GameScenes scene)
		{
			LogFormatted_DebugOnly("Removing App Launcher Button");
			ApplicationLauncher.Instance.RemoveModApplication(stockButton);
			LogFormatted_DebugOnly("App Launcher Button Removed");
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
