using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ContractParser;
using ProgressParser;
using Contracts;
using Contracts.Parameters;
using ContractsWindow.Toolbar;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity.Unity;
using ContractsWindow.Unity;

namespace ContractsWindow.PanelInterfaces
{
	public class contractWindow : DMC_MBE, ICW_Window
	{
		private bool _isVisible;
		private bool progressLoaded, contractsLoaded;
		private int timer;
		private int sceneInt;
		private contractMission currentMission;
		private progressUIPanel progressPanel;
		private CW_Window UIWindow;
		private Rect windowPos;
		private Canvas _canvas;

		private List<Guid> cList = new List<Guid>();
		private List<Guid> pinnedList = new List<Guid>();

		private List<contractUIObject> sortList = new List<contractUIObject>();

		private static GameObject windowPrefab;
		private static contractWindow instance;

		public static contractWindow Instance
		{
			get { return instance; }
		}

		public bool HideTooltips
		{
			get { return contractScenario.Instance.toolTips; }
			set { contractScenario.Instance.toolTips = value; }
		}

		public bool IgnoreScale
		{
			get { return contractScenario.Instance.ignoreScale; }
			set { contractScenario.Instance.ignoreScale = value; }
		}

		public bool IsVisible
		{
			get { return _isVisible; }
		}

		public bool LargeFont
		{
			get { return !contractScenario.Instance.fontSmall; }
			set { contractScenario.Instance.fontSmall = !value; }
		}

		public float MasterScale
		{
			get { return GameSettings.UI_SCALE; }
		}

		public float Scale
		{
			get { return contractScenario.Instance.windowScale; }
			set { contractScenario.Instance.windowScale = value; }
		}

		public bool BlizzyAvailable
		{
			get { return ToolbarManager.ToolbarAvailable; }
		}

		public bool ReplaceToolbar
		{
			get { return contractScenario.Instance.replaceStockToolbar; }
			set
			{
				contractScenario.Instance.replaceStockToolbar = value;

				if (value && contractStockToolbar.Instance != null)
					contractStockToolbar.Instance.replaceStockApp();
			}
		}

		public bool StockToolbar
		{
			get { return contractScenario.Instance.stockToolbar; }
			set
			{
				contractScenario.Instance.stockToolbar = value;

				contractScenario.Instance.toggleToolbars();
			}
		}

		public string Version
		{
			get { return contractScenario.Instance.InfoVersion; }
		}

		public Canvas MainCanvas
		{
			get { return _canvas; }
		}

		public IList<IMissionSection> GetMissions
		{
			get { return new List<IMissionSection>(contractScenario.Instance.getAllMissions().ToArray()); }
		}

		public IMissionSection GetCurrentMission
		{
			get { return currentMission; }
		}

		public void NewMission(string title, Guid id)
		{
			if (string.IsNullOrEmpty(title))
				return;

			if (!contractScenario.Instance.addMissionList(title))
				return;

			contractMission cM = contractScenario.Instance.getMissionList(title);

			if (cM == null)
				return;

			contractContainer c = contractParser.getActiveContract(id);

			if (c == null)
				return;

			cM.addContract(c, true, true);
		}

		public void Rebuild()
		{
			contractScenario.Instance.addFullMissionList();

			currentMission = contractScenario.Instance.MasterMission;

			int l = ContractSystem.Instance.Contracts.Count;

			for (int i = 0; i < l; i++)
			{
				Contract c = ContractSystem.Instance.Contracts[i];

				if (c == null || c.ContractState != Contract.State.Active)
					continue;

				contractContainer cC = contractParser.getActiveContract(c.ContractGuid);

				if (cC != null)
					currentMission.addContract(cC, true, true);
			}

			UIWindow.SelectMission(currentMission);
		}

		public void SetAppState(bool on)
		{
			if (!StockToolbar)
				return;

			if (contractStockToolbar.Instance == null)
				return;

			if (contractStockToolbar.Instance.Button == null)
				return;

			if (on)
				contractStockToolbar.Instance.Button.SetTrue(false);
			else
				contractStockToolbar.Instance.Button.SetFalse(false);
		}

		public void SetWindowPosition(Rect r)
		{
			windowPos = r;

			contractScenario.Instance.windowRects[sceneInt] = windowPos;
		}

		public void setMission(contractMission mission)
		{
			currentMission = mission;

			setMission();
		}

		public void RefreshContracts()
		{
			if (cList.Count > 0)
				refreshContracts(cList, true);
		}

		private void refreshContracts(List<Guid> list, bool sort = true)
		{
			List<Guid> removeList = new List<Guid>();
			List<Guid> pinnedRemoveList = new List<Guid>();

			int l = list.Count;

			for (int i = 0; i < l; i++)
			{
				Guid id = list[i];

				contractContainer cC = contractParser.getActiveContract(id);

				if (cC == null)
					cC = contractParser.getCompletedContract(id);

				if (cC == null)
				{
					removeList.Add(id);
					continue;
				}
				else
				{
					if (cC.Root.ContractState != Contract.State.Active)
					{
						cC.Duration = 0;
						cC.DaysToExpire = "----";

						cC.Title = cC.Root.Title;
						cC.Notes = cC.Root.Notes;

						foreach (parameterContainer pC in cC.AllParamList)
						{
							pC.Title = pC.CParam.Title;
							pC.setNotes(pC.CParam.Notes);
						}

						continue;
					}

					//Update contract timers
					if (cC.Root.DateDeadline <= 0)
					{
						cC.Duration = double.MaxValue;
						cC.DaysToExpire = "----";
					}
					else
					{
						cC.Duration = cC.Root.DateDeadline - Planetarium.GetUniversalTime();
						//Calculate time in day values using Kerbin or Earth days
						//cC.DaysToExpire = cC.timeInDays(cC.Duration);
					}

					cC.Title = cC.Root.Title;
					cC.Notes = cC.Root.Notes;

					foreach (parameterContainer pC in cC.AllParamList)
					{
						pC.Title = pC.CParam.Title;
						pC.setNotes(pC.CParam.Notes);
					}
				}
			}

			foreach (Guid id in pinnedList)
			{
				contractContainer cC = contractParser.getActiveContract(id);
				if (cC == null)
					pinnedRemoveList.Add(id);
			}

			foreach (Guid id in removeList)
				contractScenario.ListRemove(list, id);

			foreach (Guid id in pinnedRemoveList)
				contractScenario.ListRemove(pinnedList, id);

			if (sort)
			{
				list = sortContracts(list, currentMission.OrderMode, currentMission.DescendingOrder);

				if (UIWindow == null)
					return;

				UIWindow.SortMissionChildren(list);
			}

			if (UIWindow == null)
				return;

			UIWindow.UpdateMissionChildren();
		}

		private List<Guid> sortContracts(List<Guid> list, contractSortClass sortClass, bool dsc)
		{
			sortList.Clear();

			int l = list.Count;

			for (int i = 0; i < l; i++)
			{
				Guid id = list[i];

				contractUIObject cC = currentMission.getContract(id);

				if (cC == null)
					continue;

				if (cC.Order != null)
					continue;

				sortList.Add(cC);
			}

			switch(sortClass)
			{
				case contractSortClass.Expiration:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Duration.CompareTo(b.Container.Duration), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case contractSortClass.Acceptance:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.DateAccepted.CompareTo(b.Container.Root.DateAccepted), a.Container.Title.CompareTo(b.Container.Title)));
					break;
				case contractSortClass.Reward:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.TotalReward.CompareTo(b.Container.TotalReward), a.Container.Title.CompareTo(b.Container.Title)));
	
					break;
				case contractSortClass.Difficulty:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.Prestige.CompareTo(b.Container.Root.Prestige), a.Container.Title.CompareTo(b.Container.Title)));
		
					break;
				case contractSortClass.Planet:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.TargetPlanet.CompareTo(b.Container.TargetPlanet), a.Container.Title.CompareTo(b.Container.Title)));
		
					break;
				case contractSortClass.Type:
					sortList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(!dsc, a.Container.Root.GetType().Name.CompareTo(b.Container.Root.GetType().Name), a.Container.Title.CompareTo(b.Container.Title)));
					sortList = typeSort(sortList, !dsc);
					break;
			}

			list.Clear();

			if (pinnedList.Count > 0)
				list.AddRange(pinnedList);

			int k = sortList.Count;

			for (int i = 0; i < k; i++)
			{
				contractUIObject c = sortList[i];

				if (c == null)
					continue;

				list.Add(c.ID);
			}

			return list;
		}

		private List<contractUIObject> typeSort(List<contractUIObject> cL, bool B)
		{
			List<int> position = new List<int>();
			List<contractUIObject> altList = new List<contractUIObject>();
			for (int i = 0; i < cL.Count; i++)
			{
				foreach (ContractParameter cP in cL[i].Container.Root.AllParameters)
				{
					if (cP.GetType() == typeof(ReachAltitudeEnvelope))
					{
						altList.Add(cL[i]);
						position.Add(i);
						break;
					}
				}
			}
			if (altList.Count > 1)
			{
				altList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachAltitudeEnvelope)a.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude.CompareTo(((ReachAltitudeEnvelope)b.Container.Root.AllParameters.First(s => s.GetType() == typeof(ReachAltitudeEnvelope))).minAltitude), a.Container.Title.CompareTo(b.Container.Title)));
				for (int j = 0; j < position.Count; j++)
				{
					cL[position[j]] = altList[j];
				}
			}

			//ReachFlightEnvelop doesn't actually seem to be used by anything

			//position.Clear();
			//List<contractContainer> flightList = new List<contractContainer>();
			//for (int i = 0; i < cL.Count; i++)
			//{
			//    foreach (parameterContainer cP in cL[i].paramList)
			//    {
			//        if (cP.cParam.ID == "testFlightEnvelope")
			//        {
			//            flightList.Add(cL[i]);
			//            position.Add(i);
			//        }
			//    }
			//}
			//if (flightList.Count > 1)
			//{
			//    flightList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(B, ((ReachFlightEnvelope)a.contract.AllParameters.First(s => s.ID == "testFlightEnvelope")).minAltitude.CompareTo(((ReachFlightEnvelope)b.contract.AllParameters.First(s => s.ID == "testFlightEnvelope")).minAltitude), a.contract.Title.CompareTo(b.contract.Title)));
			//    for (int j = 0; j < position.Count; j++)
			//    {
			//        cL[position[j]] = flightList[j];
			//    }
			//}

			return cL;
		}

		public void SetPinState(Guid id)
		{
			pinnedList.Add(id);
		}

		public void UnPin(Guid id)
		{
			contractScenario.ListRemove(pinnedList, id);
		}

		public int GetNextPin()
		{
			return pinnedList.Count;
		}

		protected override void Awake()
		{
			base.Awake();

			instance = this;

			if (windowPrefab == null)
				windowPrefab = contractLoader.Prefabs.LoadAsset<GameObject>("cw_plus");

			LogFormatted("Window Prefab is {0}", windowPrefab == null ? "null" : "loaded");

			RepeatingWorkerInitialWait = 10;
		}

		protected override void Start()
		{
			base.Start();

			sceneInt = contractScenario.currentScene(HighLogic.LoadedScene);

			contractParser.onContractStateChange.Add(contractAccepted);
			contractParser.onContractsParsed.Add(onContractsLoaded);
			progressParser.onProgressParsed.Add(onProgressLoaded);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (UIWindow != null)
			{
				UIWindow.gameObject.SetActive(false);

				Destroy(UIWindow);
			}

			contractParser.onContractStateChange.Remove(contractAccepted);
			contractParser.onContractsParsed.Remove(onContractsLoaded);
			progressParser.onProgressParsed.Remove(onProgressLoaded);
		}

		protected override void Update()
		{
			if (progressLoaded && contractsLoaded)
				return;

			//This is a backup loading system in case something blows up while the ContractSystem is loading
			if (timer < 500 && (!progressLoaded || !contractsLoaded))
				timer++;
			else if (!progressLoaded)
			{
				loadProgressLists();
				progressLoaded = true;
			}
			else if (!contractsLoaded)
			{
				loadLists();
				contractsLoaded = true;
			}
		}

		protected override void RepeatingWorker()
		{
			if (cList.Count > 0)
				refreshContracts(cList, false);
		}

		public void Open()
		{
			if (UIWindow == null)
				return;

			StartRepeatingWorker(5);

			_isVisible = true;

			UIWindow.gameObject.SetActive(true);

			UIWindow.FadeIn();
		}

		public void Close()
		{
			if (UIWindow == null)
				return;

			StopRepeatingWorker();

			_isVisible = false;

			UIWindow.Close();
		}

		private void GenerateWindow()
		{
			if (windowPrefab == null || UIWindow != null)
				return;

			GameObject obj = Instantiate(windowPrefab, new Vector3(50, -80, 0), Quaternion.identity) as GameObject;

			_canvas = MainCanvasUtil.MainCanvas;

			obj.transform.SetParent(_canvas.transform, false);

			UIWindow = obj.GetComponent<CW_Window>();

			UIWindow.setWindow(this);

			windowPos = contractScenario.Instance.windowRects[sceneInt];

			UIWindow.SetPosition(windowPos);

			UIWindow.gameObject.SetActive(false);
		}

		private void onContractsLoaded()
		{
			StartCoroutine(loadContracts());
		}

		private void onProgressLoaded()
		{
			StartCoroutine(loadProgressNodes());
		}

		private IEnumerator loadContracts()
		{
			int i = 0;

			contractsLoaded = true;

			while (!contractParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			if (i >= 200)
			{
				contractsLoaded = false;
				yield break;
			}

			loadLists();
		}

		private void loadLists()
		{
			generateList();

			//Load ordering lists and contract settings after primary contract dictionary has been loaded

			GenerateWindow();

			setMission();

			if (contractScenario.Instance.windowVisible[sceneInt])
			{
				Open();

				if (StockToolbar)
					SetAppState(true);
			}

			//if (cList.Count > 0)
			//	refreshContracts(cList);
			//else
			//	rebuildList();
		}

		private void setMission()
		{
			if (currentMission == null)
				return;

			if (currentMission.ShowActiveMissions)
				cList = currentMission.ActiveMissionList;
			else
				cList = currentMission.HiddenMissionList;

			pinnedList = currentMission.loadPinnedContracts(cList);

			if (UIWindow != null)
				UIWindow.SelectMission(currentMission);

			refreshContracts(cList);
		}

		private void generateList()
		{
			contractScenario.Instance.loadAllMissionLists();
			if (HighLogic.LoadedSceneIsFlight)
				currentMission = contractScenario.Instance.setLoadedMission(FlightGlobals.ActiveVessel);
			else
				currentMission = contractScenario.Instance.MasterMission;
		}

		private IEnumerator loadProgressNodes()
		{
			int i = 0;

			progressLoaded = true;

			while (!progressParser.Loaded && i < 200)
			{
				i++;
				yield return null;
			}

			if (i >= 200)
			{
				progressLoaded = false;
				yield break;
			}

			loadProgressLists();
		}

		private void loadProgressLists()
		{
			progressPanel = new progressUIPanel();

			if (UIWindow == null)
				return;

			UIWindow.setupProgressPanel(progressPanel);
		}

		private void contractAccepted(Contract c)
		{
			if (c == null)
				return;

			if (c.ContractState != Contract.State.Active)
				return;

			contractContainer cC = contractParser.getActiveContract(c.ContractGuid);
			if (cC != null)
			{
				currentMission.addContract(cC, true, true, true);
				if (currentMission.ShowActiveMissions)
					refreshContracts(cList);

				if (!currentMission.MasterMission)
					contractScenario.Instance.MasterMission.addContract(cC, true, true, true);
			}
		}
	}
}
