#region license
/*The MIT License (MIT)
Contract Assembly - Monobehaviour To Check For Other Addons And Their Methods

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
using KSP.UI;
using KSP.Localization;

namespace ContractsWindow.PanelInterfaces
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class contractWindow : MonoBehaviour, ICW_Window
    {
        private const string controlLock = "CWInputLock";

        private static bool _firstLoad = true;

        private bool _isVisible;
        private bool _inputLock;
        private bool windowGenerated;
        private bool progressLoaded, contractsLoaded;
        private int sceneInt;
        private contractMission currentMission;
        private progressUIPanel progressPanel;
        private CW_Window UIWindow;
        private Rect windowPos;

        private List<Guid> cList = new List<Guid>();
        private List<Guid> pinnedList = new List<Guid>();

        private List<contractUIObject> sortList = new List<contractUIObject>();

        private Coroutine _repeatingWorker;

        private static contractWindow instance;

        public static contractWindow Instance
        {
            get { return instance; }
        }

        public bool TooltipsOn
        {
            get
            {
                if (contractLoader.Settings == null)
                    return true;

                return contractLoader.Settings.tooltips;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.tooltips = value;

                contractLoader.ToggleTooltips(value);
            }
        }

        public bool IgnoreScale
        {
            get
            {
                if (contractLoader.Settings == null)
                    return false;

                return contractLoader.Settings.ignoreKSPScale;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.ignoreKSPScale = value;
            }
        }

        public bool IsVisible
        {
            get { return _isVisible; }
        }

        public bool PixelPerfect
        {
            get
            {
                if (contractLoader.Settings == null)
                    return false;

                return contractLoader.Settings.pixelPerfect;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.pixelPerfect = value;
            }
        }

        public bool LargeFont
        {
            get
            {
                if (contractLoader.Settings == null)
                    return false;

                return contractLoader.Settings.largeFont;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.largeFont = value;

                contractLoader.UpdateFontSize(value ? 1 : -1);
            }
        }

        public float MasterScale
        {
            get { return GameSettings.UI_SCALE; }
        }

        public float Scale
        {
            get
            {
                if (contractLoader.Settings == null)
                    return 1;

                return contractLoader.Settings.windowScale;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.windowScale = value;
            }
        }

        public bool BlizzyAvailable
        {
            get { return ToolbarManager.ToolbarAvailable; }
        }

        public bool ReplaceToolbar
        {
            get
            {
                if (contractLoader.Settings == null)
                    return false;

                return contractLoader.Settings.replaceStockApp;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.replaceStockApp = value;

                if (value && contractStockToolbar.Instance != null)
                    contractStockToolbar.Instance.replaceStockApp();
            }
        }

        public bool StockToolbar
        {
            get
            {
                if (contractLoader.Settings == null)
                    return true;

                return contractLoader.Settings.useStockToolbar;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.useStockToolbar = value;

                contractScenario.Instance.toggleToolbars();
            }
        }

        public bool StockUIStyle
        {
            get
            {
                if (contractLoader.Settings == null)
                    return false;

                return contractLoader.Settings.stockUIStyle;
            }
            set
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.stockUIStyle = value;

                contractLoader.ResetUIStyle();

                if (_isVisible)
                {
                    if (UIWindow != null)
                    {
                        UIWindow.gameObject.SetActive(false);

                        DestroyImmediate(UIWindow.gameObject);
                    }

                    windowGenerated = false;

                    Open();
                }
            }
        }

        public bool LockInput
        {
            get { return _inputLock; }
            set
            {
                _inputLock = value;

                if (_inputLock)
                    InputLockManager.SetControlLock(controlLock);
                else
                    InputLockManager.RemoveControlLock(controlLock);
            }
        }

        public string AllMissionTitle
        {
            get { return Localizer.Format("#autoLOC_textAllMissionTitle"); }
        }

        public string ProgressTitle
        {
            get { return Localizer.Format("#autoLOC_textProgressTitle"); }
        }

        public string Version
        {
            get { return contractScenario.Instance.InfoVersion; }
        }

        public Canvas TooltipCanvas
        {
            get { return UIMasterController.Instance.tooltipCanvas; }
        }

        public IList<IMissionSection> GetMissions
        {
            get { return new List<IMissionSection>(contractScenario.Instance.getAllMissions().ToArray()); }
        }

        public IMissionSection GetCurrentMission
        {
            get { return currentMission; }
        }

        public IProgressPanel GetProgressPanel
        {
            get { return progressPanel; }
        }

        public IList<IContractSection> GetAllContracts
        {
            get { return contractScenario.Instance.MasterMission.GetContracts; }
        }

        public Transform ContractStorageContainer
        {
            get { return transform; }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            GameEvents.OnGameSettingsApplied.Add(onSettingsApplied);
            contractParser.onContractStateChange.Add(contractStateChange);
            contractParser.onContractsParsed.Add(onContractsLoaded);
            progressParser.onProgressParsed.Add(onProgressLoaded);
        }

        private void Start()
        {
            sceneInt = contractUtils.currentScene(HighLogic.LoadedScene);

            contractLoader.UpdateFontSize(LargeFont ? 1 : 0);

            StartCoroutine(waitForContentLoad());
        }

        private void OnDestroy()
        {
            if (instance != this)
                return;

            instance = null;

            if (UIWindow != null)
            {
                UIWindow.gameObject.SetActive(false);

                Destroy(UIWindow.gameObject);
            }

            if (_repeatingWorker != null)
            {
                StopCoroutine(_repeatingWorker);
                _repeatingWorker = null;
            }

            GameEvents.OnGameSettingsApplied.Remove(onSettingsApplied);
            contractParser.onContractStateChange.Remove(contractStateChange);
            contractParser.onContractsParsed.Remove(onContractsLoaded);
            progressParser.onProgressParsed.Remove(onProgressLoaded);
        }
        
        private void onSettingsApplied()
        {
            if (UIWindow != null)
            {
                UIWindow.setScale();
                UIWindow.SetPosition(windowPos);
            }
        }

        private IEnumerator RepeatingWorker(float seconds)
        {
            WaitForSeconds wait = new WaitForSeconds(seconds);

            yield return wait;

            while (true)
            {
                if (UIWindow != null)
                {
                    if (UIWindow.ShowingContracts)
                    {
                        if (cList.Count > 0)
                            refreshContracts(cList, false);
                    }
                    else
                        UIWindow.RefreshProgress();
                }

                yield return wait;
            }
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
            if (!StockToolbar && !ReplaceToolbar)
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

            for (int i = 0; i < list.Count; i++)
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
                        cC.DaysToExpire = cC.timeInDays(cC.Duration);
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

                if (UIWindow != null)
                    UIWindow.SortMissionChildren(list);
            }

            if (UIWindow != null)
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

            switch (sortClass)
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

        public void Open()
        {
            if (!windowGenerated)
                GenerateWindow();

            if (UIWindow == null)
                return;

            if (_repeatingWorker != null)
                StopCoroutine(_repeatingWorker);

            _repeatingWorker = StartCoroutine(RepeatingWorker(5));

            _isVisible = true;

            UIWindow.Open();
        }

        public void Close()
        {
            if (UIWindow == null)
                return;

            StopCoroutine(_repeatingWorker);
            _repeatingWorker = null;

            _isVisible = false;

            UIWindow.Close();
        }

        private void GenerateWindow()
        {
            if (contractLoader.WindowPrefab == null || UIWindow != null)
                return;

            UIWindow = Instantiate(contractLoader.WindowPrefab, DialogCanvasUtil.DialogCanvasRect, false).GetComponent<CW_Window>();

            UIWindow.setWindow(this);

            windowPos = contractScenario.Instance.windowRects[sceneInt];

            UIWindow.SetPosition(windowPos);
            windowGenerated = true;
        }

        private void onContractsLoaded()
        {
            StartCoroutine(loadContracts());
        }

        private IEnumerator loadContracts()
        {
            while (!contractParser.Loaded)
                yield return null;

            while (contractScenario.Instance == null || !contractScenario.Instance.Loaded)
                yield return null;

            loadLists();

            contractsLoaded = true;
        }

        private void loadLists()
        {
            contractUtils.LogFormatted("Loading All Contract Lists...");

            generateList();

            //Load ordering lists and contract settings after primary contract dictionary has been loaded

            if (currentMission != null)
            {
                if (currentMission.ShowActiveMissions)
                    cList = currentMission.ActiveMissionList;
                else
                    cList = currentMission.HiddenMissionList;

                pinnedList = currentMission.loadPinnedContracts(cList);
            }
        }

        private void generateList()
        {
            contractScenario.Instance.loadAllMissionLists();

            if (HighLogic.LoadedSceneIsFlight)
                currentMission = contractScenario.Instance.setLoadedMission(FlightGlobals.ActiveVessel);
            else
                currentMission = contractScenario.Instance.MasterMission;
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

        public void switchLists(bool showHidden)
        {
            if (showHidden)
                cList = currentMission.HiddenMissionList;
            else
                cList = currentMission.ActiveMissionList;
        }

        private void onProgressLoaded()
        {
            StartCoroutine(loadProgressNodes());
        }

        private IEnumerator loadProgressNodes()
        {
            while (!progressParser.Loaded)
                yield return null;

            loadProgressLists();

            progressLoaded = true;
        }

        private void loadProgressLists()
        {
            progressPanel = new progressUIPanel();
        }

        private IEnumerator waitForContentLoad()
        {
            while (!progressLoaded || !contractsLoaded)
                yield return null;

            //if (_firstLoad)
            //{
            //    yield return new WaitForSeconds(3);
            //    _firstLoad = false;
            //}

            if (!windowGenerated)
                GenerateWindow();

            if (contractScenario.Instance.windowVisible[sceneInt])
            {
                Open();
                
                if (StockToolbar || ReplaceToolbar)
                    SetAppState(true);
            }
        }

        private void contractStateChange(Contract c)
        {
            if (c == null)
                return;

            if (c.ContractState == Contract.State.Active)
            {
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
            else if (c.ContractState == Contract.State.Completed)
            {
                contractContainer cC = contractParser.getCompletedContract(c.ContractGuid);

                if (cC != null)
                {
                    if (currentMission.ContractContained(cC.ID))
                        currentMission.RefreshContract(cC.ID);
                }
            }
            else if (c.ContractState == Contract.State.Declined)
            {
                if (currentMission.ContractContained(c.ContractGuid))
                    currentMission.RefreshContract(c.ContractGuid);
            }
            else if (c.ContractState == Contract.State.Cancelled)
            {
                if (currentMission.ContractContained(c.ContractGuid))
                    currentMission.RefreshContract(c.ContractGuid);
            }
        }
    }
}
