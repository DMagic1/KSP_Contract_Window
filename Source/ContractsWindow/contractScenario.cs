#region license
/*The MIT License (MIT)
Contract Scenario - Scenario Module To Store Save Specific Info

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Contracts;
using ContractsWindow.Toolbar;
using ContractsWindow.PanelInterfaces;
using ContractParser;

namespace ContractsWindow
{
    [KSPScenario(ScenarioCreationOptions.AddToExistingCareerGames | ScenarioCreationOptions.AddToNewCareerGames
        , GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.TRACKSTATION, GameScenes.SPACECENTER)]
    public class contractScenario : ScenarioModule
    {
        #region Scenario Setup

        public static contractScenario Instance
        {
            get { return instance; }
        }

        private static contractScenario instance;

        //Primary mission storage
        private DictionaryValueList<string, contractMission> missionList = new DictionaryValueList<string, contractMission>();

        //A specific contractMission is assigned to hold all contracts; contracts can't be removed from this
        private contractMission masterMission = new contractMission("MasterMission");

        //The currently active mission
        private contractMission currentMission;

        public contractMission MasterMission
        {
            get { return masterMission; }
        }

        //initialize data for each gamescene
        internal bool[] windowVisible = new bool[4];
        internal Rect[] windowRects = new Rect[4] { new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300), new Rect(50, -80, 250, 300) };
        private int[] windowPos = new int[16] { 50, -80, 250, 300, 50, -80, 250, 300, 50, -80, 250, 300, 50, -80, 250, 300 };

        internal contractStockToolbar appLauncherButton = null;
        internal contractToolbar blizzyToolbarButton = null;

        private bool _loaded;
        
        private string infoVersion;

        public string InfoVersion
        {
            get { return infoVersion; }
        }

        public bool Loaded
        {
            get { return _loaded; }
        }

        //Convert all of our saved strings into the appropriate arrays for each game scene
        public override void OnLoad(ConfigNode node)
        {
            instance = this;

            try
            {
                ConfigNode scenes = node.GetNode("Contracts_Window_Parameters");
                if (scenes != null)
                {
                    contractUtils.LogFormatted("Loading Contracts Window + Data");
                    windowPos = contractUtils.stringSplit(scenes.GetValue("WindowPosition"));
                    windowVisible = contractUtils.stringSplitBool(scenes.GetValue("WindowVisible"));
                    int[] winPos = new int[4]
                    {
                        windowPos[4 * contractUtils.currentScene(HighLogic.LoadedScene)]
                        , windowPos[(4 * contractUtils.currentScene(HighLogic.LoadedScene)) + 1]
                        , windowPos[(4 * contractUtils.currentScene(HighLogic.LoadedScene)) + 2]
                        , windowPos[(4 * contractUtils.currentScene(HighLogic.LoadedScene)) + 3]
                    };

                    //All saved contract missions are loaded here
                    //Each mission has a separate contract list
                    foreach (ConfigNode m in scenes.GetNodes("Contracts_Window_Mission"))
                    {
                        if (m == null)
                            continue;

                        string name;
                        string activeString = "";
                        string hiddenString = "";
                        string vesselString = "";
                        bool ascending, showActive;
                        int sortMode;
                        bool master = false;

                        if (!m.HasValue("MissionName"))
                            continue;
                        
                        name = m.GetValue("MissionName");
                        
                        contractUtils.LogFormatted("Loading Contract Mission: {0}", name);

                        if (name == "MasterMission")
                            master = true;

                        if (m.HasValue("ActiveListID"))
                            activeString = m.GetValue("ActiveListID");
                        if (m.HasValue("HiddenListID"))
                            hiddenString = m.GetValue("HiddenListID");
                        if (m.HasValue("VesselIDs"))
                            vesselString = m.GetValue("VesselIDs");

                        if (!bool.TryParse(m.GetValue("AscendingSort"), out ascending))
                            ascending = true;
                        if (!bool.TryParse(m.GetValue("ShowActiveList"), out showActive))
                            showActive = true;
                        if (!int.TryParse(m.GetValue("SortMode"), out sortMode))
                            sortMode = 0;

                        contractMission mission = new contractMission(name, activeString, hiddenString, vesselString, ascending, showActive, sortMode, master);

                        if (master)
                        {
                            masterMission = mission;
                        }

                        if (!missionList.Contains(name))
                            missionList.Add(name, mission);
                    }

                    loadWindow(winPos);
                }
            }
            catch (Exception e)
            {
                contractUtils.LogFormatted("Contracts Window Settings Cannot Be Loaded: {0}", e);
            }

            _loaded = true;
        }

        public override void OnSave(ConfigNode node)
        {
            try
            {
                if (contractLoader.Settings != null)
                    contractLoader.Settings.Save();

                saveWindow(windowRects[contractUtils.currentScene(HighLogic.LoadedScene)]);

                ConfigNode scenes = new ConfigNode("Contracts_Window_Parameters");

                //Scene settings
                scenes.AddValue("WindowPosition", contractUtils.stringConcat(windowPos, windowPos.Length));
                scenes.AddValue("WindowVisible", contractUtils.stringConcat(windowVisible, windowVisible.Length));

                for (int i = missionList.Count - 1; i >= 0; i--)
                {
                    contractMission m = missionList.At(i);

                    if (m == null)
                        continue;

                    ConfigNode missionNode = new ConfigNode("Contracts_Window_Mission");

                    missionNode.AddValue("MissionName", m.InternalName);
                    missionNode.AddValue("ActiveListID", m.stringConcat(m.ActiveMissionList));
                    missionNode.AddValue("HiddenListID", m.stringConcat(m.HiddenMissionList));
                    missionNode.AddValue("VesselIDs", m.vesselConcat(currentMission));
                    missionNode.AddValue("AscendingSort", m.AscendingOrder);
                    missionNode.AddValue("ShowActiveList", m.ShowActiveMissions);
                    missionNode.AddValue("SortMode", (int)m.OrderMode);

                    scenes.AddNode(missionNode);
                }

                node.AddNode(scenes);
            }
            catch (Exception e)
            {
                contractUtils.LogFormatted("Contracts Window Settings Cannot Be Saved: {0}", e);
            }
        }

        private void Start()
        {
            Assembly assembly = AssemblyLoader.loadedAssemblies.GetByAssembly(Assembly.GetExecutingAssembly()).assembly;
            var ainfoV = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            switch (ainfoV == null)
            {
                case true: infoVersion = ""; break;
                default: infoVersion = ainfoV.InformationalVersion; break;
            }

            bool stockToolbar = true;

            if (contractLoader.Settings != null)
                stockToolbar = contractLoader.Settings.useStockToolbar;

            if (stockToolbar || !ToolbarManager.ToolbarAvailable || contractLoader.Settings.replaceStockApp)
            {
                appLauncherButton = gameObject.AddComponent<contractStockToolbar>();

                if (blizzyToolbarButton != null)
                {
                    Destroy(blizzyToolbarButton);
                    blizzyToolbarButton = null;
                }
            }
            else if (ToolbarManager.ToolbarAvailable && !stockToolbar)
            {
                blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();

                if (appLauncherButton != null)
                {
                    Destroy(appLauncherButton);
                    appLauncherButton = null;
                }
            }

            contractParser.onParameterAdded.Add(onParameterAdded);
        }

        //Remove our contract window object
        private void OnDestroy()
        {
            contractParser.onParameterAdded.Remove(onParameterAdded);
            
            if (appLauncherButton != null)
                Destroy(appLauncherButton);

            if (blizzyToolbarButton != null)
                Destroy(blizzyToolbarButton);
        }

        #endregion

        #region contract Events

        private void onParameterAdded(Contract c, ContractParameter cP)
        {
            contractContainer cc = contractParser.getActiveContract(c.ContractGuid);

            if (cc == null)
                return;

            var missions = getMissionsContaining(cc.ID);

            for (int i = missions.Count - 1; i >= 0; i--)
            {
                contractMission m = missions[i];

                if (m == null)
                    continue;

                contractUIObject cUI = m.getContract(cc.ID);

                if (cUI == null)
                    continue;

                cUI.AddParameter();
            }
        }

        //Used by external assemblies to update parameter values for the UI
        internal void paramChanged(Type t)
        {
            foreach (contractContainer cC in contractParser.getActiveContracts)
            {
                cC.updateParameterInfo(t);
            }
        }

        //Used by external assemblies to update contract values for the UI
        internal void contractChanged(Type t)
        {
            foreach (contractContainer cC in contractParser.getActiveContracts)
            {
                if (cC.Root.GetType() == t)
                    cC.updateContractInfo();
            }
        }

        #endregion

        #region internal methods

        internal void toggleToolbars()
        {
            bool stockToolbar = true;

            if (contractLoader.Settings != null)
                stockToolbar = contractLoader.Settings.useStockToolbar;

            if (stockToolbar || !ToolbarManager.ToolbarAvailable)
            {
                if (blizzyToolbarButton != null)
                {
                    Destroy(blizzyToolbarButton);
                    blizzyToolbarButton = null;
                }

                if (appLauncherButton == null)
                    appLauncherButton = gameObject.AddComponent<contractStockToolbar>();
            }
            else if (ToolbarManager.ToolbarAvailable && !stockToolbar)
            {
                if (appLauncherButton != null)
                {
                    Destroy(appLauncherButton);
                    appLauncherButton = null;
                }

                if (blizzyToolbarButton == null)
                    blizzyToolbarButton = gameObject.AddComponent<contractToolbar>();
            }
        }

        internal bool addMissionList(string name)
        {
            if (!missionList.Contains(name))
            {
                contractMission mission = new contractMission(name);
                missionList.Add(name, mission);
                return true;
            }
            else
                contractUtils.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

            return false;
        }

        internal bool addMissionList(contractMission mission)
        {
            if (!missionList.Contains(mission.MissionTitle))
            {
                missionList.Add(mission.MissionTitle, mission);
                return true;
            }
            else
                contractUtils.LogFormatted("Missions List Already Contains Mission Of Name: [{0}]; Please Rename", name);

            return false;
        }

        //Used to add the master contract mission list; usually when something has gone wrong
        internal void addFullMissionList()
        {
            string s = "MasterMission";

            if (missionList.Contains(s))
                removeMissionList(s);

            if (addMissionList(s))
            {
                missionList[s].MasterMission = true;
                addAllContractsToMaster();
                masterMission = missionList[s];
            }
        }

        //Adds all contracts to the master mission
        private void addAllContractsToMaster()
        {
            contractMission Master = null;

            for (int i = missionList.Count - 1; i >= 0; i--)
            {
                contractMission m = missionList.At(i);

                if (m == null)
                    continue;

                if (!m.MasterMission)
                    continue;

                Master = m;
                break;
            }

            if (Master != null)
            {
                List<contractContainer> active = contractParser.getActiveContracts;

                int l = active.Count;

                for (int j = 0; j < l; j++)
                {
                    contractContainer c = active[j];

                    if (c == null)
                        continue;

                    Master.addContract(c, true, true);
                }
            }
        }

        internal void removeMissionList(string name, bool delete = true)
        {
            if (missionList.Contains(name))
            {
                if (delete)
                {
                    contractMission c = missionList[name];
                    c = null;
                }
                missionList.Remove(name);
            }
            else
                contractUtils.LogFormatted("No Mission List Of Name: [{0}] Found", name);
        }

        internal void resetMissionsList()
        {
            missionList.Clear();
        }

        internal contractMission getMissionList(string name, bool warn = false)
        {
            if (missionList.Contains(name))
                return missionList[name];
            else if (warn)
                contractUtils.LogFormatted("No Mission Of Name [{0}] Found In Primary Mission List", name);

            return null;
        }

        internal void setCurrentMission(string s)
        {
            contractMission m = getMissionList(s, true);

            if (m != null)
                currentMission = m;
            else
                currentMission = masterMission;


            contractWindow.Instance.setMission(currentMission);
        }

        internal contractMission setLoadedMission(Vessel v)
        {
            if (v == null)
                return masterMission;

            for (int i = 0; i < missionList.Count; i++)
            {
                contractMission m = missionList.At(i);

                if (m == null)
                    continue;

                if (m.ContainsVessel(v))
                    return m;
            }

            return masterMission;
        }

        internal List<contractMission> getMissionsContaining(Guid id)
        {
            return missionList.Values.Where(m => m.ContractContained(id)).ToList();
        }

        //Returns an ordered list of missions for the main window; the master mission is always first
        internal List<contractMission> getAllMissions()
        {
            List<contractMission> mList = new List<contractMission>();
            List<contractMission> tempList = new List<contractMission>();

            for (int i = 0; i < missionList.Count; i++)
            {
                contractMission m = missionList.At(i);

                if (m == null)
                    continue;

                if (m.MasterMission)
                    mList.Add(m);
                else
                    tempList.Add(m);
            }

            if (mList.Count == 0)
            {
                if (addMissionList("MasterMission"))
                    mList.Add(getMissionList("MasterMission"));
            }

            tempList.Sort((a, b) => RUIutils.SortAscDescPrimarySecondary(false, a.ActiveContracts.CompareTo(b.ActiveContracts), a.MissionTitle.CompareTo(b.MissionTitle)));

            if (tempList.Count > 0)
                mList.AddRange(tempList);

            return mList;
        }

        //Initializes all missions that were added during the loading process
        internal void loadAllMissionLists()
        {
            if (missionList.Count <= 0)
            {
                contractUtils.LogFormatted("No Mission Lists Detected; Regenerating Master List");
                addFullMissionList();
            }
            else
            {
                for (int i = 0; i < missionList.Count; i++)
                {
                    contractMission m = missionList.At(i);

                    if (m == null)
                        continue;

                    if (m.MasterMission)
                    {
                        m.buildMissionList();

                        List<contractContainer> active = contractParser.getActiveContracts;
                        
                        for (int j = 0; j < active.Count; j++)
                        {
                            contractContainer c = active[j];

                            if (c == null)
                                continue;

                            m.addContract(c, true, false);
                        }

                        masterMission = m;
                    }
                    else
                        m.buildMissionList();
                }
            }
        }

        internal static bool ListRemove(List<Guid> list, Guid id)
        {
            if (list.Contains(id))
            {
                list.Remove(id);
                return true;
            }

            return false;
        }

        #endregion

        #region save/load methods

        //Save and load the window rectangle position
        private void saveWindow(Rect source)
        {
            int i = contractUtils.currentScene(HighLogic.LoadedScene);
            windowPos[i * 4] = (int)source.x;
            windowPos[(i * 4) + 1] = (int)source.y;
            windowPos[(i * 4) + 2] = (int)source.width;
            windowPos[(i * 4) + 3] = (int)source.height;
        }

        private void loadWindow(int[] window)
        {
            int i = contractUtils.currentScene(HighLogic.LoadedScene);
            windowRects[i] = new Rect(window[0], window[1], window[2], window[3]);
        }

        #endregion

    }
}
