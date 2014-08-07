using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

using Contracts;
using UnityEngine;

namespace ContractsWindow
{
	[KSPAddonImproved(KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TimeElapses, false)]
	class contractsWindow: MonoBehaviourWindow
	{
		internal static bool IsVisible;
		private List<contractContainer> cList = new List<contractContainer>();
		private string version, assemblyLocation;
		private Assembly assembly;
		private Vector2 scroll;
		private bool resizing;
		private float dragStart, windowHeight;
		private Texture2D iconTex;
		
		internal override void Awake()
		{
			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
				assemblyLocation = assembly.Location.Replace("\\", "/");
				version = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;
				iconTex = GameDatabase.Instance.GetTexture("Contracts Window/ResizeIcon", false);

				WindowCaption = string.Format("Contracts {0}", version);
				WindowRect = new Rect(40, 80, 250, 300);
				WindowOptions = new GUILayoutOption[1] { GUILayout.MaxHeight(Screen.height) };
				Visible = true;
				DragEnabled = true;
				DragRect = new Rect(WindowRect.x - 35, WindowRect.y - 77, 230, 30);
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
				if (GUILayout.Button(c.contract.Title, titleState(c.contract.ContractState)))
					c.showParams = !c.showParams;
				if (c.showParams)
				{
					GUILayout.BeginVertical();
					foreach (ContractParameter cP in c.contract.AllParameters)
					{
						if (!string.IsNullOrEmpty(cP.Title))
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(10);
							GUILayout.Box(cP.Title, paramState(cP.State));
							GUILayout.EndHorizontal();
							if (cP.State != ParameterState.Complete && !string.IsNullOrEmpty(cP.Notes))
							{
								GUILayout.BeginHorizontal();
								if (GUILayout.Button("[+] Note:", contractSkins.noteText, GUILayout.MaxWidth(45)))
								{
									c.showNotes = !c.showNotes;
								}
								if (c.showNotes)
								{
									GUILayout.Space(3);
									GUILayout.Box(cP.Notes, contractSkins.noteText);
								}
								GUILayout.EndHorizontal();
							}
						}
					}
					GUILayout.EndVertical();
				}
			}
			GUILayout.EndScrollView();
			GUILayout.Space(15);
			GUILayout.EndVertical();
		}

		internal override void DrawGUI()
		{
			Toggle = IsVisible;
			DragRect.height = WindowRect.height - 50;
			base.DrawGUI();
			if (Toggle)
			{
				Rect resizer = new Rect(WindowRect.x + WindowRect.width - 27, WindowRect.y + WindowRect.height - 27, 24, 24);
				GUI.Box(resizer, iconTex);

				if (Event.current.type == EventType.mouseDown && Event.current.button == 0)
				{
					if (resizer.Contains(Event.current.mousePosition))
					{
						resizing = true;
						dragStart = Input.mousePosition.y;
						windowHeight = WindowRect.yMax;
						Event.current.Use();
					}
				}
				if (resizing)
				{
					if (Input.GetMouseButtonUp(0))
					{
						resizing = false;
						WindowRect.yMax = windowHeight;
					}
					else
					{
						float height = Input.mousePosition.y;
						if (Input.mousePosition.y < 0)
							height = 0;
						windowHeight -= height - dragStart;
						dragStart = height;
						WindowRect.yMax = windowHeight;
						if (WindowRect.yMax > Screen.height)
							WindowRect.yMax = Screen.height;
					}
				}
			}
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
