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
using System.Collections.Generic;
using System.Linq;
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
		private Button MissionEdit = null;
		[SerializeField]
		private GameObject MissionSectionPrefab = null;
		[SerializeField]
		private Transform MissionSectionTransform = null;
		[SerializeField]
		private GameObject ProgressPanelPrefab = null;
		[SerializeField]
		private GameObject AgencyPrefab = null;
		[SerializeField]
		private GameObject SortPrefab = null;
		[SerializeField]
		private GameObject MissionSelectPrefab = null;
		[SerializeField]
		private GameObject MissionAddPrefab = null;
		[SerializeField]
		private GameObject MissionEditPrefab = null;
		[SerializeField]
		private GameObject MissionCreatePrefab = null;
		[SerializeField]
		private GameObject RebuildPrefab = null;
		[SerializeField]
		private GameObject ToolbarPrefab = null;
		[SerializeField]
		private GameObject ScalarPrefab = null;
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
		private GameObject TooltipPrefab = null;
		 
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private bool dragging;
		private bool resizing;

		private CW_MissionSection currentMission;
		private CW_ProgressPanel progressPanel;

		private List<TooltipHandler> tooltips = new List<TooltipHandler>();

		private ICW_Window windowInterface;
		private bool loaded;
		private bool showingContracts = true;

		private bool popupOpen;

		private static CW_Window window;

		public static CW_Window Window
		{
			get { return window; }
		}

		public bool ShowingContracts
		{
			get { return showingContracts; }
		}

		public ICW_Window Interface
		{
			get { return windowInterface; }
		}

		public GameObject Tooltip
		{
			get { return TooltipPrefab; }
		}

		public ScrollRect Scroll
		{
			get { return Scroller; }
		}

		public Toggle TitleToggle
		{
			get { return m_TitleToggle; }
		}

		protected override void Awake()
		{
			base.Awake();

			window = this;

			rect = GetComponent<RectTransform>();
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

			SelectMission(window.GetCurrentMission);

			if (window.IgnoreScale)
				transform.localScale /= window.MasterScale;

			transform.localScale *= window.Scale;

			ProcessTooltips();
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

		private void CreateProgressSection(IProgressPanel progress)
		{
			if (progress == null)
				return;

			if (ProgressPanelPrefab == null || MissionSectionTransform == null)
				return;

			GameObject obj = Instantiate(ProgressPanelPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(MissionSectionTransform, false);

			progressPanel = obj.GetComponent<CW_ProgressPanel>();

			if (progressPanel == null)
				return;

			progressPanel.setPanel(progress);
		}

		private CW_MissionSection CreateMissionSection(IMissionSection mission)
		{
			if (MissionSectionPrefab == null || MissionSectionTransform == null)
				return null;

			GameObject obj = Instantiate(MissionSectionPrefab);

			if (obj == null)
				return null;

			obj.transform.SetParent(MissionSectionTransform, false);

			CW_MissionSection missionObject = obj.GetComponent<CW_MissionSection>();

			if (missionObject == null)
				return null;

			missionObject.setMission(mission);

			return missionObject;
		}

		public void SelectMission(IMissionSection mission)
		{
			if (currentMission != null)
			{
				if (currentMission.MissionTitle != mission.MissionTitle)
					DestroyImmediate(currentMission.gameObject);
				else
					return;
			}

			currentMission = CreateMissionSection(mission);

			if (currentMission == null)
				return;

			loaded = false;

			prepareTopBar();
			
			if (MissionTitle != null)
				MissionTitle.OnTextUpdate.Invoke(currentMission.MasterMission ? windowInterface.AllMissionTitle : currentMission.MissionTitle + ":");

			if (MissionEdit != null)
			{
				if (currentMission.MasterMission)
					MissionEdit.gameObject.SetActive(false);
				else
					MissionEdit.gameObject.SetActive(true);
			}

			ProcessTooltips();
		}

		public void ProcessTooltips()
		{
			if (windowInterface == null)
				return;

			TooltipHandler[] handlers = gameObject.GetComponentsInChildren<TooltipHandler>(true);

			if (handlers == null)
				return;

			for (int j = 0; j < handlers.Length; j++)
				ProcessTooltip(handlers[j], windowInterface.TooltipsOn, windowInterface.TooltipCanvas, windowInterface.Scale);
		}

		private void ProcessTooltip(TooltipHandler handler, bool isOn, Canvas c, float scale)
		{
			if (handler == null)
				return;

			handler.IsActive = isOn;
			handler._Canvas = c;
			handler.Scale = scale;
		}

		private void prepareTopBar()
		{
			if (currentMission == null)
				return;

			if (currentMission.MissionInterface == null)
				return;

			if (SortOrderToggle != null)
				SortOrderToggle.isOn = currentMission.MissionInterface.DescendingOrder;

			if (ShowHideToggle != null)
				ShowHideToggle.isOn = currentMission.MissionInterface.ShowHidden;

			loaded = true;
		}

		public void ToggleMainWindow(bool showProgress)
		{
			if (windowInterface == null)
				return;

			if (showProgress)
			{
				if (currentMission != null)
					DestroyImmediate(currentMission.gameObject);

				CreateProgressSection(windowInterface.GetProgressPanel);

				if (MissionTitle != null)
					MissionTitle.OnTextUpdate.Invoke(windowInterface.ProgressTitle);

				if (MainPanelTooltip != null)
					MainPanelTooltip.TooltipIndex = 0;
			}
			else
			{
				if (progressPanel != null)
					DestroyImmediate(progressPanel.gameObject);

				SelectMission(windowInterface.GetCurrentMission);

				if (MissionTitle != null && currentMission != null)
					MissionTitle.OnTextUpdate.Invoke(currentMission.MasterMission ? windowInterface.AllMissionTitle : currentMission.MissionTitle + ":");

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

			if (currentMission == null)
				return;

			GameObject obj = Instantiate(SortPrefab);

			obj.transform.SetParent(transform, false);

			CW_SortMenu sortObject = obj.GetComponent<CW_SortMenu>();

			if (sortObject == null)
				return;

			sortObject.setSort(currentMission.MissionInterface);

			popupOpen = true;
		}

		public void ToggleSortOrder(bool isOn)
		{
			if (!loaded)
				return;

			if (currentMission == null)
				return;

			if (currentMission.MissionInterface == null)
				return;

			currentMission.MissionInterface.DescendingOrder = isOn;
		}

		public void ToggleShowHide(bool isOn)
		{
			if (EyesTooltip != null)
				EyesTooltip.TooltipIndex = isOn ? 1 : 0;

			if (!loaded)
				return;

			if (currentMission == null)
				return;

			if (currentMission.MissionInterface == null)
				return;

			currentMission.MissionInterface.ShowHidden = isOn;

			currentMission.ToggleContracts(isOn);
		}

		public void showSelector()
		{
			if (popupOpen)
				return;

			if (MissionSelectPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(MissionSelectPrefab);

			obj.transform.SetParent(transform, false);

			CW_MissionSelect selectorObject = obj.GetComponent<CW_MissionSelect>();

			if (selectorObject == null)
				return;

			selectorObject.setMission(windowInterface.GetMissions);

			popupOpen = true;
		}

		public void showCreator(IContractSection contract)
		{
			if (MissionCreatePrefab == null)
				return;

			if (windowInterface == null)
				return;

			if (contract == null)
				return;

			GameObject obj = Instantiate(MissionCreatePrefab);

			obj.transform.SetParent(transform, false);

			CW_MissionCreate creatorObject = obj.GetComponent<CW_MissionCreate>();

			if (creatorObject == null)
				return;

			creatorObject.setPanel(contract);

			popupOpen = true;
		}

		public void showEditor()
		{
			if (popupOpen)
				return;

			if (MissionEditPrefab == null)
				return;

			if (windowInterface == null)
				return;

			if (currentMission == null)
				return;

			GameObject obj = Instantiate(MissionEditPrefab);

			obj.transform.SetParent(transform, false);

			CW_MissionEdit editorObject = obj.GetComponent<CW_MissionEdit>();

			if (editorObject == null)
				return;

			editorObject.setMission(currentMission.MissionInterface);

			popupOpen = true;
		}

		public void showRefresh()
		{
			if (popupOpen)
				return;

			if (RebuildPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(RebuildPrefab);

			obj.transform.SetParent(transform, false);

			popupOpen = true;
		}

		public void showScale()
		{
			if (popupOpen)
				return;

			if (ScalarPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(ScalarPrefab);

			obj.transform.SetParent(transform, false);

			CW_Scale scalarObject = obj.GetComponent<CW_Scale>();

			if (scalarObject == null)
				return;

			scalarObject.setScalar();

			popupOpen = true;
		}

		public void showToolbar()
		{
			if (popupOpen)
				return;

			if (ToolbarPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(ToolbarPrefab);

			obj.transform.SetParent(transform, false);

			CW_Toolbar toolbarObject = obj.GetComponent<CW_Toolbar>();

			if (toolbarObject == null)
				return;

			toolbarObject.setToolbar();

			popupOpen = true;
		}

		public void ShowAgentWindow(IContractSection contract)
		{
			if (popupOpen)
				return;

			if (AgencyPrefab == null)
				return;

			if (contract == null)
				return;

			GameObject obj = Instantiate(AgencyPrefab);

			obj.transform.SetParent(transform, false);

			CW_AgencyPanel agencyObject = obj.GetComponent<CW_AgencyPanel>();

			if (agencyObject == null)
				return;

			agencyObject.setAgent(contract.AgencyName, contract.AgencyLogo);

			popupOpen = true;
		}

		public void ShowMissionAddWindow(IContractSection contract)
		{
			if (popupOpen)
				return;

			if (MissionAddPrefab == null)
				return;

			if (contract == null)
				return;

			GameObject obj = Instantiate(MissionAddPrefab);

			obj.transform.SetParent(transform, false);

			CW_MissionAdd adderObject = obj.GetComponent<CW_MissionAdd>();

			if (adderObject == null)
				return;

			adderObject.setMission(windowInterface.GetMissions, contract);

			popupOpen = true;
		}

		public void onStartResize(BaseEventData eventData)
		{
			if (!(eventData is PointerEventData))
				return;

			resizing = true;

			if (windowInterface == null || windowInterface.MainCanvas == null)
				return;

			windowInterface.MainCanvas.pixelPerfect = false;
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

			float diff = (Screen.height / windowInterface.MasterScale) - Screen.height;

			float derp = ((rect.anchoredPosition.y * Screen.height) - (diff * Screen.height)) / (Screen.height + diff);

			windowInterface.SetWindowPosition(new Rect(rect.anchoredPosition.x * windowInterface.MasterScale, derp, rect.sizeDelta.x, rect.sizeDelta.y));

			if (windowInterface.MainCanvas == null)
				return;

			if (windowInterface.PixelPerfect)
				windowInterface.MainCanvas.pixelPerfect = true;
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

			float diff = (Screen.height / windowInterface.MasterScale) - Screen.height;

			float derp = ((rect.anchoredPosition.y * Screen.height) - (diff * Screen.height)) / (Screen.height + diff);

			windowInterface.SetWindowPosition(new Rect(rect.anchoredPosition.x * windowInterface.MasterScale, derp, rect.sizeDelta.x, rect.sizeDelta.y));
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
						
			r.x /= windowInterface.MasterScale;

			float diff = (Screen.height / windowInterface.MasterScale) - Screen.height;

			float derp = diff - (((-1f * r.y) / Screen.height) * diff);

			r.y += derp;

			rect.anchoredPosition = new Vector3(r.x, r.y, 0);

			rect.sizeDelta = new Vector2(r.width, r.height);

			rect.position = new Vector3(rect.position.x, rect.position.y, 1);

			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);
		}

		public void SortMissionChildren(List<Guid> sorted)
		{
			if (currentMission == null)
				return;

			currentMission.SortChildren(sorted);
		}

		public void UpdateMissionChildren()
		{
			if (currentMission == null)
				return;

			currentMission.UpdateChildren();
		}

		public void RefreshProgress()
		{
			if (progressPanel == null)
				return;

			progressPanel.Refresh();
		}

		public void FadeIn(bool overrule)
		{
			Fade(1, true, null, true, overrule);
		}

		public void ScaleAndFadeIn()
		{
			Fade(1, true);
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
			gameObject.SetActive(false);
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (Scroller == null)
				return;

			Scroller.OnScroll(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (windowInterface != null)
				windowInterface.SetAsLastSibling();

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

		public void FadePopup(CW_Popup p)
		{
			popupOpen = false;

			p.FadeOut(p.ClosePopup);
		}
	}
}
