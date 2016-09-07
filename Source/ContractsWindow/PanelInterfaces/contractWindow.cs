using System;
using System.Collections;
using System.Collections.Generic;
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
		private bool progressLoaded, contractsLoaded;
		private int timer;
		private int sceneInt;
		private contractMission currentMission;
		private CW_Window UIWindow;
		private Rect windowPos;

		private static Texture icon;
		private static GameObject windowPrefab;

		public bool BlizzyAvailable
		{
			get { return ToolbarManager.ToolbarAvailable; }
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

		public bool IsVisible { get; set; }

		public bool LargeFont
		{
			get { return !contractScenario.Instance.fontSmall; }
			set { contractScenario.Instance.fontSmall = !value; }
		}

		public float MasterScale
		{
			get { return contractScenario.Instance.windowScale; }
			set { contractScenario.Instance.windowScale = value; }
		}

		public bool ReplaceToolbar
		{
			get { return contractScenario.Instance.replaceStockToolbar; }
			set { contractScenario.Instance.replaceStockToolbar = value; }
		}

		public float Scale
		{
			get { return contractScenario.Instance.windowScale; }
			set { contractScenario.Instance.windowScale = value; }
		}

		public bool StockToolbar
		{
			get { return contractScenario.Instance.stockToolbar; }
			set { contractScenario.Instance.stockToolbar = value; }
		}

		public string Version
		{
			get { return contractScenario.Instance.InfoVersion; }
		}

		public IList<IMissionSection> GetMissions
		{
			get { return new List<IMissionSection>(contractScenario.Instance.getAllMissions().ToArray()); }
		}

		public IMissionSection GetCurrentMission
		{
			get { return currentMission; }
		}

		public IProgressPanel GetProgress
		{
			get { return null; }
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


		}

		public void SetSort(int i)
		{

		}

		public void SetWindowPosition(Rect r)
		{
			windowPos = r;
		}

		protected override void Awake()
		{
			base.Awake();

			//if (icon == null)
			//	icon = SEP_UI_Loader.Images.LoadAsset<Texture2D>("toolbar_icon");

			//if (windowPrefab == null)
			//	windowPrefab = SEP_UI_Loader.Prefabs.LoadAsset<GameObject>("sep_window");
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

		public void Open()
		{
			if (UIWindow == null)
				return;

			UIWindow.SetPosition(windowPos);

			UIWindow.FadeIn();
		}

		public void Close()
		{
			if (UIWindow == null)
				return;

			UIWindow.Close();
		}

		private Vector3 GetAnchor()
		{
			windowPos = contractScenario.Instance.windowRects[sceneInt];

			if (windowPos == null)
			{

			}

			return new Vector3();
		}

		private void GenerateWindow()
		{
			if (windowPrefab == null || UIWindow != null)
				return;

			GameObject obj = Instantiate(windowPrefab, GetAnchor(), Quaternion.identity) as GameObject;

			obj.transform.SetParent(MainCanvasUtil.MainCanvas.transform);

			UIWindow = obj.GetComponent<CW_Window>();

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
			if (currentMission != null)
			{
				//if (currentMission.ShowActiveMissions)
				//{
				//	cList = currentMission.ActiveMissionList;
				//	pinnedList = currentMission.loadPinnedContracts(cList);
				//}
				//else
				//{
				//	cList = currentMission.HiddenMissionList;
				//	pinnedList = currentMission.loadPinnedContracts(cList);
				//}

				if (UIWindow != null)
					UIWindow.SelectMission(currentMission);
			}

			GenerateWindow();

			if (contractScenario.Instance.windowVisible[sceneInt])
				Open();

			//if (cList.Count > 0)
			//	refreshContracts(cList);
			//else
			//	rebuildList();
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
			//intervalNodes = progressParser.getAllIntervalNodes;
			//standardNodes = progressParser.getAllStandardNodes;
			//POInodes = progressParser.getAllPOINodes;
			//bodyNodes = progressParser.getAllBodyNodes;

			//bodySubNodes = new List<List<progressStandard>>(bodyNodes.Count);

			//for (int i = 0; i < bodyNodes.Count; i++)
			//{
			//	bodySubNodes.Add(bodyNodes[i].getAllNodes);
			//}
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
				currentMission.addContract(cC, true, true);
				//if (currentMission.ShowActiveMissions)
				//	refreshContracts(cList);

				if (!currentMission.MasterMission)
					contractScenario.Instance.MasterMission.addContract(cC, true, true);
			}
		}
	}
}
