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
		private Vector2 scroll;
		
		internal override void Awake()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
				version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;
				WindowCaption = string.Format("Contracts {0}", version);
				WindowRect = new Rect(0, 0, 250, 300);
				WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(300) };
				Visible = true;
				DragEnabled = true;
				DragRect = new Rect(WindowRect.x, WindowRect.y, WindowRect.width, 20);
				SkinsLibrary.SetCurrent("UnitySkin");
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
			GUILayout.Label(string.Format("Active Contracts: {0}", cList.Count));
			GUILayout.BeginVertical();
			scroll = GUILayout.BeginScrollView(scroll);
			foreach (contractContainer c in cList)
			{
				GUILayout.Button(c.contract.Title, titleState(c.contract.ContractState));
				
					GUILayout.BeginVertical();
					//parameterBox(c, 250);
					foreach (ContractParameter cP in c.contract.AllParameters)
					{

						GUILayout.BeginHorizontal();
						GUILayout.Space(10);
						GUILayout.Box(cP.Title, paramState(cP.State));
						GUILayout.EndHorizontal();
					}

					GUILayout.EndVertical();
				

				
				
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

		}

		private void parameterBox(contractContainer c, int i)
		{
			GUILayout.BeginVertical();
			GUILayout.Space(5);
			foreach (ContractParameter cP in c.contract.AllParameters)
			{

				GUILayout.BeginHorizontal();
				GUILayout.Space(10);
				GUILayout.Box(cP.Title, paramState(cP.State));
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
		}

		private GUIStyle titleState(Contract.State s)
		{
			switch (s)
			{
				case Contract.State.Completed:
					return contractSkins.contractCompleted;
				case Contract.State.Cancelled:
				case Contract.State.DeadlineExpired:
				case Contract.State.Failed:
				case Contract.State.Withdrawn:
					return contractSkins.contractFailed;
				default:
					return contractSkins.contractActive;
			}
		}

		private GUIStyle paramState(ParameterState s)
		{
			switch (s)
			{
				case ParameterState.Complete:
					return contractSkins.paramCompleted;
				case ParameterState.Failed:
					return contractSkins.paramFailed;
				default:
					return contractSkins.paramText;
			}
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
