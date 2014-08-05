using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using Contracts;
using UnityEngine;

namespace Contracts_Window
{
	[KSPAddonImproved(KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TimeElapses, false)]
	class contractsWindow: MonoBehaviourWindow
	{
		private List<contractContainer> cList = new List<contractContainer>();
		private string version;
		private Assembly assembly;
		
		internal override void Awake()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
				version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
				WindowCaption = string.Format("Contracts {0}", version);
				WindowRect = new Rect(0, 0, 150, 100);
				Visible = true;
				DragEnabled = true;
				SkinsLibrary.SetCurrent("DefaultSkin");
			}
		}

		internal override void Start()
		{
			GameEvents.Contract.onAccepted.Add(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Add(contractLoaded);
		}

		internal override void OnDestroy()
		{
			GameEvents.Contract.onAccepted.Remove(contractAccepted);
			GameEvents.Contract.onContractsLoaded.Remove(contractLoaded);
		}

		internal override void Update()
		{
			
		}

		internal override void DrawWindow(int id)
		{

			GUILayout.Label(new GUIContent("Contracts", "Get Contracts"));
			GUILayout.Label(string.Format("Active Contracts: {0}", ContractSystem.Instance.Contracts.Count));
			GUILayout.BeginVertical();
			foreach (contractContainer c in cList)
				GUILayout.Box(c.contract.Title);
			GUILayout.EndVertical();

		}

		private void contractAccepted(Contract c)
		{
			cList.Add(new contractContainer(c));
		}

		private void contractLoaded()
		{
			cList.Clear();
			foreach (Contract c in ContractSystem.Instance.Contracts)
			{
				if (c.ContractState == Contract.State.Active)
				{
					cList.Add(new contractContainer(c));
				}
			}
		}

	}
}
