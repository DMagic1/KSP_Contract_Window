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
using KSP.UI.Screens;
using ContractsWindow.PanelInterfaces;
using UnityEngine;

namespace ContractsWindow.Toolbar
{
	public class contractStockToolbar : ApplicationLauncherButton
	{
		private ApplicationLauncherButton toolbarButton = null;
		private bool replaceStock;
		private static contractStockToolbar instance;
		
		public static contractStockToolbar Instance
		{
			get { return instance; }
		}

		public ApplicationLauncherButton Button
		{
			get
			{
				if (replaceStock)
				{

				}

				return toolbarButton;
			}
		}

		private void Start()
		{
			setupToolbar();
		}

		private void setupToolbar()
		{
			if (!contractScenario.Instance.replaceStockToolbar)
				StartCoroutine(addButton());
			else
				StartCoroutine(replaceStockContractApp());
		}

		private void OnDestroy()
		{
			GameEvents.onGUIApplicationLauncherUnreadifying.Remove(removeButton);

			removeButton(HighLogic.LoadedScene);
		}

		IEnumerator addButton()
		{

			while (!ApplicationLauncher.Ready)
				yield return null;

			toolbarButton = ApplicationLauncher.Instance.AddModApplication(toggle, toggle, null, null, null, null, (ApplicationLauncher.AppScenes)63, contractSkins.toolbarIcon);

			GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
		}

		private void removeButton(GameScenes scene)
		{
			if (toolbarButton != null)
			{
				ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
				toolbarButton = null;
			}
		}

		internal void replaceStockApp()
		{
			StartCoroutine(replaceStockContractApp());
		}

		IEnumerator replaceStockContractApp()
		{
			while (ContractsApp.Instance.appLauncherButton == null || !ApplicationLauncher.Ready)
				yield return null;

			if (toolbarButton == null)
			{
				DMC_MBE.LogFormatted("Contracts Window + App Launcher Button Not Initialized; Starting It Now");
				toolbarButton = ApplicationLauncher.Instance.AddModApplication(open, close, null, null, null, null, (ApplicationLauncher.AppScenes)63, contractSkins.toolbarIcon);
			}

			ApplicationLauncherButton stockContracts = ContractsApp.Instance.appLauncherButton;

			if (stockContracts != null)
			{
				stockContracts.onDisable();

				stockContracts.onTrue = toolbarButton.onTrue;
				stockContracts.onFalse = toolbarButton.onFalse;
				stockContracts.onHover = toolbarButton.onHover;
				stockContracts.onHoverOut = toolbarButton.onHoverOut;
				stockContracts.onEnable = toolbarButton.onEnable;
				stockContracts.onDisable = toolbarButton.onDisable;

				ApplicationLauncher.Instance.DisableMutuallyExclusive(stockContracts);

				DMC_MBE.LogFormatted("Stock Contracts App Replaced With Contracts Window +");

				try
				{
					removeButton(HighLogic.LoadedScene);
				}
				catch (Exception e)
				{
					DMC_MBE.LogFormatted("Error In Removing Contracts Window + Toolbar App After Replacing Stock App: {0}", e);
				}
			}
			else
			{
				DMC_MBE.LogFormatted("Something went wrong while replacing the stock contract; attempting to add standard toolbar button");

				if (toolbarButton != null)
					GameEvents.onGUIApplicationLauncherUnreadifying.Add(removeButton);
				else
					StartCoroutine(addButton());
			}
		}

		private void open()
		{
			if (contractWindow.Instance == null)
				return;

			contractWindow.Instance.Open();
		}

		private void close()
		{
			if (contractWindow.Instance == null)
				return;

			contractWindow.Instance.Close();
		}

		private void toggle()
		{
			int sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);
			if (contractScenario.Instance == null)
				DMC_MBE.LogFormatted("Contract Scenario Not Loaded...");
			else if (contractScenario.Instance.cWin == null)
				DMC_MBE.LogFormatted("Contract Window Not Loaded...");
			else
			{
				if (!contractScenario.Instance.cWin.Visible)
					contractScenario.Instance.cWin.StartRepeatingWorker(5);
				else
					contractScenario.Instance.cWin.StopRepeatingWorker();
				contractScenario.Instance.cWin.Visible = !contractScenario.Instance.cWin.Visible;
				contractScenario.Instance.windowVisible[sceneInt] = !contractScenario.Instance.windowVisible[sceneInt];
			}
		}

	}
}
