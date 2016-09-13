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
	public class CW_Window : CanvasFader, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
	{
		[SerializeField]
		private Text VersionText = null;
		[SerializeField]
		private Text MissionTitle = null;
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
		 
		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private bool dragging;
		private bool resizing;

		private Dictionary<string, CW_MissionSection> missions = new Dictionary<string, CW_MissionSection>();
		private CW_MissionSection currentMission;
		private CW_MissionSection masterMission;
		private CW_ProgressPanel progressPanel;

		private List<TooltipHandler> tooltips = new List<TooltipHandler>();

		private ICW_Window windowInterface;
		private bool loaded;

		private bool popupOpen;

		private static CW_Window window;

		public static CW_Window Window
		{
			get { return window; }
		}

		public ICW_Window Interface
		{
			get { return windowInterface; }
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

			Fade(1, true, true, true);
		}

		public void setWindow(ICW_Window window)
		{
			if (window == null)
				return;

			windowInterface = window;

			if (VersionText != null)
				VersionText.text = window.Version;

			CreateMissionSections(window.GetMissions);

			tooltips = GetComponentsInChildren<TooltipHandler>().ToList();

			UpdateFontSize(gameObject, window.LargeFont ? 1 : 0);
		}

		public void setupProgressPanel(IProgressPanel panel)
		{
			if (windowInterface == null)
				return;

			if (panel == null)
				return;

			CreateProgressSection(panel);

			UpdateFontSize(progressPanel.gameObject, windowInterface.LargeFont ? 1 : 0);
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

			progressPanel.gameObject.SetActive(false);
		}

		private void CreateMissionSections(IList<IMissionSection> sections)
		{
			if (sections == null)
				return;

			for (int i = sections.Count - 1; i >= 0; i--)
			{
				IMissionSection mission = sections[i];

				if (mission == null)
					continue;

				if (missions.ContainsKey(mission.MissionTitle))
					continue;

				CreateMissionSection(mission);
			}
		}

		private void CreateMissionSection(IMissionSection mission)
		{
			if (MissionSectionPrefab == null || MissionSectionTransform == null)
				return;

			GameObject obj = Instantiate(MissionSectionPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(MissionSectionTransform, false);

			CW_MissionSection missionObject = obj.GetComponent<CW_MissionSection>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission);

			missions.Add(mission.MissionTitle, missionObject);

			missionObject.gameObject.SetActive(false);
		}

		public void AddMissionSection(IMissionSection mission)
		{
			if (mission == null)
				return;

			if (missions.ContainsKey(mission.MissionTitle))
				return;

			CreateMissionSection(mission);
		}

		public void RemoveMissionSection(IMissionSection mission)
		{
			if (mission == null)
				return;

			if (!missions.ContainsKey(mission.MissionTitle))
				return;

			CW_MissionSection section = missions[mission.MissionTitle];

			section.gameObject.SetActive(false);

			Destroy(section);

			missions.Remove(mission.MissionTitle);
		}

		public void SelectMission(IMissionSection mission)
		{
			SelectMission(mission.MissionTitle);
		}

		public void SelectMission(string mission)
		{
			if (!missions.ContainsKey(mission))
				return;

			CW_MissionSection section = missions[mission];

			if (section == null)
				return;

			if (currentMission != null && currentMission.MissionTitle != section.MissionTitle)
				currentMission.SetMissionVisible(false);

			currentMission = section;

			loaded = false;

			prepareTopBar();

			currentMission.SetMissionVisible(true);

			if (MissionTitle != null)
				MissionTitle.text = currentMission.MissionTitle + ":";

			if (MissionEdit != null)
			{
				if (currentMission.MasterMission)
					MissionEdit.gameObject.SetActive(false);
				else
					MissionEdit.gameObject.SetActive(true);
			}
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
			if (showProgress)
			{
				if (currentMission != null)
					currentMission.SetMissionVisible(false);

				if (progressPanel != null)
					progressPanel.SetProgressVisible(true);

				if (MissionTitle == null)
					return;

				MissionTitle.text = "Progress Nodes:";
			}
			else
			{
				if (progressPanel != null)
					progressPanel.SetProgressVisible(false);

				if (currentMission != null)
					currentMission.SetMissionVisible(true);

				if (MissionTitle == null || currentMission == null)
					return;

				MissionTitle.text = currentMission.MissionTitle + ":";
			}
		}

		public void ShowSort()
		{
			if (popupOpen)
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
			if (popupOpen)
				return;

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

		public void ToggleTooltips(bool isOn)
		{
			if (windowInterface == null)
				return;

			windowInterface.HideTooltips = isOn;

			for (int i = tooltips.Count - 1; i >= 0; i--)
			{
				TooltipHandler t = tooltips[i];

				if (t == null)
					continue;

				t.IsActive = isOn;
			}
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

		public void onResize(BaseEventData eventData)
		{
			if (rect == null)
				return;

			if (!(eventData is PointerEventData))
				return;

			resizing = true;

			rect.sizeDelta = new Vector2(rect.sizeDelta.x + ((PointerEventData)eventData).delta.x , rect.sizeDelta.y - ((PointerEventData)eventData).delta.y);

			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);
		}

		private void checkMaxResize(int numY, int numX)
		{
			if (windowInterface == null)
				return;

			if (rect.sizeDelta.y < 200 * windowInterface.Scale)
				numY = (int)(200 * windowInterface.Scale);
			else if (rect.sizeDelta.y > Screen.height)
				numY = Screen.height;

			if (rect.sizeDelta.x < 250 * windowInterface.Scale)
				numX = (int)(250 * windowInterface.Scale);
			else if (rect.sizeDelta.x > 540 * windowInterface.Scale)
				numX = (int)(540 * windowInterface.Scale);

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

			windowInterface.SetWindowPosition(new Rect(rect.position.x, rect.position.y, rect.sizeDelta.x, rect.sizeDelta.y));
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
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			dragging = false;

			if (rect == null)
				return;

			windowInterface.SetWindowPosition(new Rect(rect.position.x, rect.position.y, rect.sizeDelta.x, rect.sizeDelta.y));
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			FadeIn();
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

			rect.position = new Vector3(r.x, r.y, 0);

			rect.sizeDelta = new Vector2(r.width, r.height);

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

		public void UpdateTooltips()
		{
			tooltips = GetComponentsInChildren<TooltipHandler>().ToList();
		}

		public void UpdateFontSize(GameObject obj, int s)
		{
			var texts = obj.GetComponentsInChildren<Text>();

			for (int i = texts.Length - 1; i >= 0; i--)
			{
				Text t = texts[i];

				if (t == null)
					continue;

				t.fontSize += s;
			}
		}

		public void FadeIn()
		{
			Fade(1, true, false);
		}

		public void FadeOut()
		{
			Fade(0.6f, false, false);
		}

		public void Close()
		{
			Fade(0, true, true, false, Hide);
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}

		public void DestroyChild(GameObject obj)
		{
			if (obj == null)
				return;

			obj.SetActive(false);

			Destroy(obj);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!popupOpen)
				return;

			var popups = GetComponentsInChildren<CW_Popup>();

			print("[CW_UI] Found " + popups.Length + " Popups");

			for (int i = popups.Length - 1; i >= 0; i--)
			{
				CW_Popup popup = popups[i];

				if (popup == null)
					continue;

				if (!popup.gameObject.activeSelf)
				{
					DestroyPopup(popup);
					continue;
				}

				RectTransform r = popup.GetComponent<RectTransform>();

				if (r == null)
					continue;

				if (RectTransformUtility.RectangleContainsScreenPoint(r, eventData.position, eventData.pressEventCamera))
					continue;

				DestroyPopup(popup);

				popupOpen = false;
			}
		}

		public void DestroyPopup(CW_Popup p)
		{
			p.gameObject.SetActive(false);

			Destroy(p);

			var popups = GetComponentsInChildren<CW_Popup>();

			for (int i = popups.Length - 1; i >= 0; i--)
			{
				CW_Popup popup = popups[i];

				if (popup == null)
					continue;

				popup.gameObject.SetActive(false);

				Destroy(popup);
			}
		}

	}
}
