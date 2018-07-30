#region license
/*The MIT License (MIT)
CW_Window - Controls the main UI window

Copyright (c) 2016 DMagic

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
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(RectTransform))]
	public class CW_Window : CanvasFader, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IScrollHandler
	{
		[SerializeField]
		private TextHandler VersionText = null;
		[SerializeField]
		private TextHandler MissionTitle = null;
        [SerializeField]
        private CW_MissionSection MissionSection = null;
        [SerializeField]
        private CW_ProgressPanel ProgressPanel = null;
        [SerializeField]
        private Transform ContentTransform = null;
        [SerializeField]
		private Button MissionEdit = null;
		[SerializeField]
		private CW_AgencyPanel AgencyPrefab = null;
		[SerializeField]
		private CW_SortMenu SortPrefab = null;
		[SerializeField]
		private CW_MissionSelect MissionSelectPrefab = null;
		[SerializeField]
		private CW_MissionAdd MissionAddPrefab = null;
		[SerializeField]
		private CW_MissionEdit MissionEditPrefab = null;
		[SerializeField]
		private CW_MissionCreate MissionCreatePrefab = null;
		[SerializeField]
		private CW_Rebuild RebuildPrefab = null;
		[SerializeField]
		private CW_Toolbar ToolbarPrefab = null;
		[SerializeField]
		private CW_Scale ScalarPrefab = null;
		[SerializeField]
		private Toggle SortOrderToggle = null;
		[SerializeField]
		private Toggle ShowHideToggle = null;
		[SerializeField]
		private Toggle TooltipToggle = null;
		[SerializeField]
		private Toggle m_TitleToggle = null;
		[SerializeField]
		private ScrollRect Scroller = null;
		[SerializeField]
		private TooltipHandler EyesTooltip = null;
		[SerializeField]
		private TooltipHandler MainPanelTooltip = null;
        [SerializeField]
        private Canvas ContractCanvas = null;
        [SerializeField]
        private Canvas ProgressCanvas = null;
        [SerializeField]
        private CW_ContractSection ContractSectionPrefab = null;

        private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;
        private Canvas windowCanvas;

		private bool dragging;
		private bool resizing;
        
        private Dictionary<Guid, CW_ContractSection> masterContractList = new Dictionary<Guid, CW_ContractSection>();

		private List<TooltipHandler> tooltips = new List<TooltipHandler>();

		private ICW_Window windowInterface;
		private bool loaded;
		private bool showingContracts = true;

		private bool popupOpen;
        
		public bool ShowingContracts
		{
			get { return showingContracts; }
		}
        
		protected override void Awake()
		{
			base.Awake();
            
			rect = GetComponent<RectTransform>();
            windowCanvas = GetComponent<Canvas>();
        }

		private void Start()
		{
			Alpha(1);

			Fade(1, true);
		}

		public void setWindow(ICW_Window window)
		{
			if (window == null)
				return;
            
			windowInterface = window;
            
			if (VersionText != null)
				VersionText.OnTextUpdate.Invoke(window.Version);

			if (TooltipToggle != null)
				TooltipToggle.isOn = window.TooltipsOn;

            MissionSection.Init(ProcessTooltips, AddContract, windowInterface.ContractStorageContainer);

            StartCoroutine(GenerateContracts(window.GetAllContracts));

			if (window.IgnoreScale)
				transform.localScale /= window.MasterScale;

			transform.localScale *= window.Scale;

            if (ContractCanvas != null)
            {
                ContractCanvas.overridePixelPerfect = !window.PixelPerfect;
                ContractCanvas.pixelPerfect = window.PixelPerfect;
            }

            if (ProgressCanvas != null)
            {
                ProgressCanvas.overridePixelPerfect = !window.PixelPerfect;
                ProgressCanvas.pixelPerfect = window.PixelPerfect;
            }
        }

        public void setScale()
        {
            if (windowInterface == null)
                return;

            Vector3 scale = Vector3.one;

            if (windowInterface.IgnoreScale)
                scale /= windowInterface.MasterScale;

            transform.localScale = scale * windowInterface.Scale;
        }

        private IEnumerator GenerateContracts(IList<IContractSection> contracts)
        {
            if (contracts == null || ContractSectionPrefab == null)
                yield break;

            int count = contracts.Count;

            ProgressPanel.transform.SetParent(windowInterface.ContractStorageContainer, false);

            MissionSection.gameObject.SetActive(true);

            while (count > 0)
            {
                count--;

                GenerateContract(contracts[count]);

                yield return null;
            }

            SelectMission(windowInterface.GetCurrentMission);
            
            windowInterface.RefreshContracts();
            
            if (ProgressPanel != null)
                yield return StartCoroutine(ProgressPanel.GeneratePanel(windowInterface.GetProgressPanel, false));

            ProgressPanel.transform.SetParent(ContentTransform, false);
        }

        private CW_ContractSection GenerateContract(IContractSection contract)
        {
            CW_ContractSection con = Instantiate(ContractSectionPrefab, windowInterface.ContractStorageContainer, false);
            
            con.setContract(contract, ShowAgentWindow, ShowMissionAddWindow, Scroller);

            if (!masterContractList.ContainsKey(contract.ID))
                masterContractList.Add(contract.ID, con);

            return con;
        }

        public void AddContract(IContractSection contract)
        {
            if (contract == null)
                return;
            
            if (masterContractList.ContainsKey(contract.ID))
                return;
            
            MissionSection.AddContract(GenerateContract(contract), contract);

            ProcessTooltips();
        }
        
		public void SelectMission(IMissionSection mission)
		{
            MissionSection.ClearContracts();

            MissionSection.setMission(mission, masterContractList);

			loaded = false;

			prepareTopBar();
			
			if (MissionTitle != null)
				MissionTitle.OnTextUpdate.Invoke(MissionSection.MasterMission ? windowInterface.AllMissionTitle : MissionSection.MissionTitle + ":");

			if (MissionEdit != null)
			{
				if (MissionSection.MasterMission)
					MissionEdit.gameObject.SetActive(false);
				else
					MissionEdit.gameObject.SetActive(true);
			}

			ProcessTooltips();
		}

        private void ProcessTooltips()
		{
			if (windowInterface == null)
				return;

			TooltipHandler[] handlers = gameObject.GetComponentsInChildren<TooltipHandler>(true);

			if (handlers == null)
				return;

			for (int j = 0; j < handlers.Length; j++)
				ProcessTooltip(handlers[j], windowInterface.TooltipsOn, windowInterface.TooltipCanvas, windowInterface.Scale, Scroller);
		}

		private void ProcessTooltip(TooltipHandler handler, bool isOn, Canvas c, float scale, ScrollRect scroll)
		{
			if (handler == null)
				return;

			handler.IsActive = isOn;
			handler._Canvas = c;
			handler.Scale = scale;
            handler.WindowScroll = scroll;
		}

		private void prepareTopBar()
		{
			if (MissionSection == null)
				return;

			if (MissionSection.MissionInterface == null)
				return;

			if (SortOrderToggle != null)
				SortOrderToggle.isOn = MissionSection.MissionInterface.DescendingOrder;

			if (ShowHideToggle != null)
				ShowHideToggle.isOn = MissionSection.MissionInterface.ShowHidden;

			loaded = true;
		}

		public void ToggleMainWindow(bool showProgress)
		{
			if (windowInterface == null)
				return;

			if (showProgress)
			{
                if (MissionSection != null)
                    MissionSection.gameObject.SetActive(false);

                if (ProgressPanel != null)
                    ProgressPanel.gameObject.SetActive(true);
                
                RefreshProgress();

				if (MissionTitle != null)
					MissionTitle.OnTextUpdate.Invoke(windowInterface.ProgressTitle);

				if (MainPanelTooltip != null)
					MainPanelTooltip.TooltipIndex = 0;
			}
			else
            {
                if (ProgressPanel != null)
                    ProgressPanel.gameObject.SetActive(false);

                if (MissionSection != null)
                    MissionSection.gameObject.SetActive(true);

                if (MissionTitle != null && MissionSection != null)
					MissionTitle.OnTextUpdate.Invoke(MissionSection.MasterMission ? windowInterface.AllMissionTitle : MissionSection.MissionTitle + ":");

				if (MainPanelTooltip != null)
					MainPanelTooltip.TooltipIndex = 1;
			}

			showingContracts = !showProgress;

			ProcessTooltips();
		}

		public void ToggleTooltips(bool isOn)
		{
			if (!loaded || windowInterface == null)
				return;

			windowInterface.TooltipsOn = isOn;

			ProcessTooltips();
		}

		public void ShowSort()
		{
			if (popupOpen)
				return;

			if (windowInterface == null)
				return;

			if (SortPrefab == null)
				return;

			if (MissionSection == null)
				return;

            CW_SortMenu sortObject = Instantiate(SortPrefab, transform, false);
            
			sortObject.setSort(MissionSection.MissionInterface, FadePopup);

			popupOpen = true;
		}

		public void ToggleSortOrder(bool isOn)
		{
			if (!loaded)
				return;

			if (MissionSection == null)
				return;

			if (MissionSection.MissionInterface == null)
				return;

			MissionSection.MissionInterface.DescendingOrder = isOn;
		}

		public void ToggleShowHide(bool isOn)
		{
			if (EyesTooltip != null)
				EyesTooltip.TooltipIndex = isOn ? 1 : 0;

			if (!loaded)
				return;

			if (MissionSection == null)
				return;

			if (MissionSection.MissionInterface == null)
				return;

			MissionSection.MissionInterface.ShowHidden = isOn;

			MissionSection.ToggleContracts(isOn);
		}

		public void showSelector()
		{
			if (popupOpen)
				return;

			if (MissionSelectPrefab == null)
				return;

			if (windowInterface == null)
				return;

            CW_MissionSelect selectorObject = Instantiate(MissionSelectPrefab, transform, false);
            
			selectorObject.setMission(windowInterface.GetMissions, SetTitleToggle, FadePopup);

			popupOpen = true;
		}

        private void SetTitleToggle(bool isOn)
        {
            if (m_TitleToggle != null)
                m_TitleToggle.isOn = isOn;
        }

		public void showCreator(IContractSection contract)
		{
			if (MissionCreatePrefab == null)
				return;

			if (windowInterface == null)
				return;

			if (contract == null)
				return;

            CW_MissionCreate creatorObject = Instantiate(MissionCreatePrefab, transform, false);
            
			creatorObject.setPanel(contract.ID, OnCreateMission, FadePopup, OnInputLock);

			popupOpen = true;
		}

        private void OnInputLock(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.LockInput = isOn;
        }

        private void OnCreateMission(string title, Guid id)
        {
            if (windowInterface != null)
                windowInterface.NewMission(title, id);
        }

        public void showEditor()
		{
			if (popupOpen)
				return;

			if (MissionEditPrefab == null)
				return;

			if (windowInterface == null)
				return;

			if (MissionSection == null)
				return;

            CW_MissionEdit editorObject = Instantiate(MissionEditPrefab, transform, false);
            
			editorObject.setMission(OnMissionNameChange, OnMissionDelete, FadePopup, OnInputLock);

			popupOpen = true;
		}

        private void OnMissionNameChange(string title)
        {
            if (MissionSection == null || MissionSection.MissionInterface == null)
                return;

            MissionSection.MissionInterface.MissionTitle = title;
        }

        private void OnMissionDelete()
        {
            if (MissionSection == null || MissionSection.MissionInterface == null)
                return;

            MissionSection.MissionInterface.RemoveMission();
        }

		public void showRefresh()
		{
			if (popupOpen)
				return;

			if (RebuildPrefab == null)
				return;

			if (windowInterface == null)
				return;

            CW_Rebuild rebuilder = Instantiate(RebuildPrefab, transform, false);
            
            rebuilder.setRebuilder(OnRebuild, FadePopup);

			popupOpen = true;
		}

        private void OnRebuild()
        {
            if (windowInterface != null)
                windowInterface.Rebuild();
        }

        public void showScale()
		{
			if (popupOpen)
				return;

			if (ScalarPrefab == null)
				return;

			if (windowInterface == null)
				return;

            CW_Scale scalarObject = Instantiate(ScalarPrefab, transform, false);
            
			scalarObject.setScalar(windowInterface.PixelPerfect, windowInterface.LargeFont, windowInterface.IgnoreScale, windowInterface.Scale
                , OnPixelPerfectToggle, OnLargeFontToggle, OnIgnoreScaleToggle, OnScaleChange, FadePopup);

			popupOpen = true;
		}

        private void OnPixelPerfectToggle(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.PixelPerfect = isOn;

            if (ContractCanvas != null)
            {
                ContractCanvas.overridePixelPerfect = !isOn;
                ContractCanvas.pixelPerfect = isOn;
            }

            if (ProgressCanvas != null)
            {
                ProgressCanvas.overridePixelPerfect = !isOn;
                ProgressCanvas.pixelPerfect = isOn;
            }
        }

        private void OnLargeFontToggle(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.LargeFont = isOn;

            var texts = gameObject.GetComponentsInChildren<TextHandler>(true);

            for (int i = texts.Length - 1; i >= 0; i--)
            {
                TextHandler t = texts[i];

                if (t == null)
                    continue;

                t.OnFontChange.Invoke(isOn ? 1 : -1);
            }
        }

        private void OnIgnoreScaleToggle(bool isOn)
        {
            if (windowInterface == null)
                return;

            windowInterface.IgnoreScale = isOn;

            float f = windowInterface.Scale;

            Vector3 scale = Vector3.one;

            if (isOn)
                scale /= windowInterface.MasterScale;

            transform.localScale = scale * f;
        }

        private void OnScaleChange(float scale)
        {
            if (windowInterface == null)
                return;

            windowInterface.Scale = scale;

            Vector3 s = Vector3.one;

            if (windowInterface.IgnoreScale)
                s /= windowInterface.MasterScale;

            transform.localScale = scale * s;
        }

        public void showToolbar()
        {
            if (popupOpen)
                return;

            if (ToolbarPrefab == null)
                return;

            if (windowInterface == null)
                return;

            CW_Toolbar toolbarObject = Instantiate(ToolbarPrefab, transform, false);
            
            toolbarObject.setToolbar(windowInterface.StockToolbar, windowInterface.BlizzyAvailable, windowInterface.ReplaceToolbar, windowInterface.StockUIStyle,
               OnStockToolbarToggle, OnReplaceToolbarToggle, OnStockUIToggle, FadePopup);

            popupOpen = true;
        }

        private void OnStockToolbarToggle(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.StockToolbar = isOn;
        }

        private void OnReplaceToolbarToggle(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.ReplaceToolbar = isOn;
        }

        private void OnStockUIToggle(bool isOn)
        {
            if (windowInterface != null)
                windowInterface.StockUIStyle = isOn;
        }

        private void ShowAgentWindow(IContractSection contract)
		{
			if (popupOpen)
				return;

			if (AgencyPrefab == null)
				return;

			if (contract == null)
				return;

            CW_AgencyPanel agencyObject = Instantiate(AgencyPrefab, transform, false);
            
			agencyObject.setAgent(contract.AgencyName, contract.AgencyLogo);

			popupOpen = true;
		}

        private void ShowMissionAddWindow(IContractSection contract)
		{
			if (popupOpen)
				return;

			if (MissionAddPrefab == null)
				return;

			if (contract == null)
				return;

            CW_MissionAdd adderObject = Instantiate(MissionAddPrefab, transform, false);
            
			adderObject.setMission(windowInterface.GetMissions, contract, showCreator, FadePopup);

			popupOpen = true;
		}

		public void onStartResize(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			resizing = true;

            if (ContractCanvas != null)
            {
                ContractCanvas.overridePixelPerfect = true;
                ContractCanvas.pixelPerfect = false;
            }

            if (ProgressCanvas != null)
            {
                ProgressCanvas.overridePixelPerfect = true;
                ProgressCanvas.pixelPerfect = false;
            }
        }

		public void onResize(BaseEventData eventData)
		{
			if (rect == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			rect.sizeDelta = new Vector2(rect.sizeDelta.x + ((PointerEventData)eventData).delta.x , rect.sizeDelta.y - ((PointerEventData)eventData).delta.y);

			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);
		}

		private void checkMaxResize(int numY, int numX)
		{
			if (windowInterface == null)
				return;

			float f = windowInterface.IgnoreScale ? windowInterface.Scale : windowInterface.MasterScale * windowInterface.Scale;

			if (rect.sizeDelta.y < 280)
				numY = 280;
			else if (rect.sizeDelta.y > Screen.height / f)
				numY = (int)(Screen.height / f);

			if (rect.sizeDelta.x < 250)
				numX = 250;
			else if (rect.sizeDelta.x > 540)
				numX = 510;

			rect.sizeDelta = new Vector2(numX, numY);
		}

		public void onEndResize(BaseEventData eventData)
		{
			resizing = false;

			if (rect == null)
				return;

			if (windowInterface == null)
				return;

			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);

            windowInterface.SetWindowPosition(new Rect(rect.anchoredPosition.x, rect.anchoredPosition.y, rect.sizeDelta.x, rect.sizeDelta.y));

            if (windowInterface.PixelPerfect)
            {
                if (ContractCanvas != null)
                {
                    ContractCanvas.overridePixelPerfect = false;
                    ContractCanvas.pixelPerfect = true;
                }

                if (ProgressCanvas != null)
                {
                    ProgressCanvas.overridePixelPerfect = false;
                    ProgressCanvas.pixelPerfect = true;
                }
            }
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			dragging = true;

			mouseStart = eventData.position;
			windowStart = rect.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (rect == null)
				return;

			rect.position = windowStart + (Vector3)(eventData.position - mouseStart);

			rect.position = clamp(rect, new RectOffset(100, 100, 200, 200));
		}

		private Vector3 clamp(RectTransform r, RectOffset offset)
		{
			Vector3 pos = new Vector3();

			float f = 1;

			if (windowInterface != null)
				f = windowInterface.IgnoreScale ? 1 * windowInterface.Scale : windowInterface.MasterScale * windowInterface.Scale;

			pos.x = Mathf.Clamp(r.position.x, (-1 * (f * r.sizeDelta.x - offset.left)) - (Screen.width / 2), (Screen.width / 2) - offset.right);
			pos.y = Mathf.Clamp(r.position.y, offset.bottom - (Screen.height / 2), (Screen.height / 2) + (f * r.sizeDelta.y - offset.top));
			pos.z = 1;

			return pos;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			dragging = false;

			if (rect == null)
				return;

            windowInterface.SetWindowPosition(new Rect(rect.anchoredPosition.x, rect.anchoredPosition.y, rect.sizeDelta.x, rect.sizeDelta.y));
        }

		public void OnPointerEnter(PointerEventData eventData)
		{
			FadeIn(false);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (!dragging && !resizing)
				FadeOut();
		}

		public void SetPosition(Rect r)
		{
			if (rect == null)
				return;

			rect.anchoredPosition3D = new Vector3(r.x, r.y, 0);

			rect.sizeDelta = new Vector2(r.width, r.height);
            
			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);

            clamp(rect, new RectOffset(100, 100, 200, 200));
        }

		public void SortMissionChildren(List<Guid> sorted)
		{
			if (MissionSection == null)
				return;

			MissionSection.SortChildren(sorted);
		}

		public void UpdateMissionChildren()
		{
			if (MissionSection == null)
				return;

			MissionSection.UpdateChildren();
		}

		public void RefreshProgress()
		{
			if (ProgressPanel == null)
				return;

			ProgressPanel.Refresh();
		}

        public void Open()
        {
            if (windowCanvas != null)
                windowCanvas.enabled = true;

            if (MissionSection != null)
                MissionSection.gameObject.SetActive(showingContracts);

            if (ProgressPanel != null)
                ProgressPanel.gameObject.SetActive(!showingContracts);

            FadeIn(true);
        }

		public void FadeIn(bool overrule)
		{
			Fade(1, true, null, true, overrule);
		}
        
		public void FadeOut()
		{
			Fade(0.8f, false);
		}

		public void Close()
		{
			Fade(0, true, Hide, false);
		}

		private void Hide()
		{
            if (windowCanvas != null)
                windowCanvas.enabled = false;

            if (MissionSection != null)
                MissionSection.gameObject.SetActive(false);

            if (ProgressPanel != null)
                ProgressPanel.gameObject.SetActive(false);
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (Scroller == null)
				return;

			Scroller.OnScroll(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (rect != null)
				rect.SetAsLastSibling();

			if (!popupOpen)
				return;

			var popups = GetComponentsInChildren<CW_Popup>();
			
			for (int i = popups.Length - 1; i >= 0; i--)
			{
				CW_Popup popup = popups[i];

				if (popup == null)
					continue;

				if (!popup.gameObject.activeSelf)
				{
					FadePopup(popup);
					continue;
				}

				RectTransform r = popup.GetComponent<RectTransform>();

				if (r == null)
					continue;

				if (RectTransformUtility.RectangleContainsScreenPoint(r, eventData.position, eventData.pressEventCamera))
					continue;

				FadePopup(popup);

				popupOpen = false;
			}
		}

        private void FadePopup(CW_Popup p)
		{
			popupOpen = false;

			p.FadeOut(p.ClosePopup);
		}
	}
}
