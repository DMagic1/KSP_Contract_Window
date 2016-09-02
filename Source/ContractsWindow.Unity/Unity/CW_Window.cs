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
	public class CW_Window : CanvasFader, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Image SortOrder = null;
		[SerializeField]
		private Image ShowHide = null;
		[SerializeField]
		private Image WindowType = null;
		[SerializeField]
		private Text VersionText = null;
		[SerializeField]
		private Text MissionTitle = null;
		[SerializeField]
		private Button MissionEdit = null;
		[SerializeField]
		private Sprite OrderUp = null;
		[SerializeField]
		private Sprite OrderDown = null;
		[SerializeField]
		private Sprite ContractIcon = null;
		[SerializeField]
		private Sprite ProgressIcon = null;
		[SerializeField]
		private Sprite EyeOpen = null;
		[SerializeField]
		private Sprite EyeClosed = null;
		[SerializeField]
		private float fastFadeDuration = 0.2f;
		[SerializeField]
		private float slowFadeDuration = 0.5f;
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

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private bool dragging;
		private bool resizing;

		private Dictionary<string, CW_MissionSection> missions = new Dictionary<string, CW_MissionSection>();
		private CW_MissionSection currentMission;
		private CW_ProgressPanel progressPanel;

		private ICW_Window windowInterface;

		protected override void Awake()
		{
			base.Awake();

			rect = GetComponent<RectTransform>();
		}

		private void Start()
		{
			Alpha(1);
		}

		public void setWindow(ICW_Window window)
		{
			if (window == null)
				return;

			windowInterface = window;

			prepareTopBar();

			if (VersionText != null)
				VersionText.text = window.Version;

			CreateMissionSections(window.GetMissions());

			CreateProgressSection(window.GetProgress());
		}

		private void prepareTopBar()
		{
			sortSprite(windowInterface.SortUp);

			showHideSprite(windowInterface.ShowActive);
		}

		private void sortSprite(bool on)
		{
			if (SortOrder != null && OrderUp != null && OrderDown != null)
			{
				if (on)
					SortOrder.sprite = OrderUp;
				else
					SortOrder.sprite = OrderDown;
			}
		}

		private void showHideSprite(bool on)
		{
			if (ShowHide != null && EyeOpen != null && EyeClosed != null)
			{
				if (on)
					ShowHide.sprite = EyeOpen;
				else
					ShowHide.sprite = EyeClosed;
			}
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

			windowInterface.ProcessStyle(obj);

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

			if (MissionSectionPrefab == null || MissionSectionTransform == null)
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
			GameObject obj = Instantiate(MissionSectionPrefab);

			if (obj == null)
				return;

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(MissionSectionTransform, false);

			CW_MissionSection missionObject = obj.GetComponent<CW_MissionSection>();

			if (missionObject == null)
				return;

			missionObject.setMission(mission, this);

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

		public void SelectMission(string mission)
		{
			if (!missions.ContainsKey(mission))
				return;

			CW_MissionSection section = missions[mission];

			if (currentMission != null && currentMission.MissionTitle != section.MissionTitle)
				currentMission.SetMissionVisible(false);

			currentMission = section;

			currentMission.SetMissionVisible(true);

			if (MissionTitle != null)
				MissionTitle.text = currentMission.MissionTitle;

			if (MissionEdit != null)
			{
				if (currentMission.MasterMission)
					MissionEdit.gameObject.SetActive(false);
				else
					MissionEdit.gameObject.SetActive(true);
			}
		}

		public void ToggleMainWindow(bool showContracts)
		{
			if (showContracts)
			{
				if (progressPanel != null)
					progressPanel.SetProgressVisible(false);

				if (currentMission != null)
					currentMission.SetMissionVisible(true);
			}
			else
			{
				if (currentMission != null)
					currentMission.SetMissionVisible(false);

				if (progressPanel != null)
					progressPanel.SetProgressVisible(true);
			}

			if (WindowType != null && ContractIcon != null && ProgressIcon != null)
			{
				if (showContracts)
					WindowType.sprite = ContractIcon;
				else
					WindowType.sprite = ProgressIcon;
			}
		}

		public void ShowSort()
		{
			if (SortPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(SortPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_SortMenu sortObject = obj.GetComponent<CW_SortMenu>();

			if (sortObject == null)
				return;

			sortObject.setSort(windowInterface.GetSort(), this);
		}

		public void ToggleSortOrder(bool sortUp)
		{
			sortSprite(sortUp);

			if (windowInterface == null)
				return;

			windowInterface.SortUp = sortUp;
		}

		public void ToggleShowHide(bool showActive)
		{
			showHideSprite(showActive);

			if (windowInterface == null)
				return;

			windowInterface.ShowActive = showActive;
		}

		public void showSelector()
		{
			if (MissionSelectPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(MissionSelectPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_MissionSelect selectorObject = obj.GetComponent<CW_MissionSelect>();

			if (selectorObject == null)
				return;

			selectorObject.setMission(windowInterface.GetMissionSelect(), this);
		}

		public void showCreator()
		{
			if (MissionCreatePrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(MissionCreatePrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_MissionCreate creatorObject = obj.GetComponent<CW_MissionCreate>();

			if (creatorObject == null)
				return;

			creatorObject.setPanel(windowInterface.GetMissionCreate(), this);
		}

		public void showEditor()
		{
			if (MissionEditPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(MissionEditPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_MissionEdit editorObject = obj.GetComponent<CW_MissionEdit>();

			if (editorObject == null)
				return;

			editorObject.setMission(windowInterface.GetMissionEdit(), this);
		}

		public void ToggleTooltips(bool isOn)
		{
			if (windowInterface == null)
				return;

			windowInterface.ShowTooltips = isOn;
		}

		public void showRefresh()
		{
			if (RebuildPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(RebuildPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_Rebuild rebuildObject = obj.GetComponent<CW_Rebuild>();

			if (rebuildObject == null)
				return;

			rebuildObject.setInterface(windowInterface.GetRefresh(), this);
		}

		public void showScale()
		{
			if (ScalarPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(ScalarPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_Scale scalarObject = obj.GetComponent<CW_Scale>();

			if (scalarObject == null)
				return;

			scalarObject.setScalar(windowInterface.GetScalar(), this);
		}

		public void showToolbar()
		{
			if (ToolbarPrefab == null)
				return;

			if (windowInterface == null)
				return;

			GameObject obj = Instantiate(ToolbarPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_Toolbar toolbarObject = obj.GetComponent<CW_Toolbar>();

			if (toolbarObject == null)
				return;

			toolbarObject.setToolbar(windowInterface.GetToolbar(), this);
		}

		public void ShowAgentWindow(IContractSection contract)
		{
			if (AgencyPrefab == null)
				return;

			if (contract == null)
				return;

			GameObject obj = Instantiate(AgencyPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_AgencyPanel agencyObject = obj.GetComponent<CW_AgencyPanel>();

			if (agencyObject == null)
				return;

			agencyObject.setAgent(contract.GetAgent());
		}

		public void ShowMissionAddWindow(IContractSection contract)
		{
			if (MissionAddPrefab == null)
				return;

			if (contract == null)
				return;

			GameObject obj = Instantiate(MissionAddPrefab);

			windowInterface.ProcessStyle(obj);

			obj.transform.SetParent(transform, false);

			CW_MissionAdd adderObject = obj.GetComponent<CW_MissionAdd>();

			if (adderObject == null)
				return;

			adderObject.setMission(contract.GetMissionAdd(), this);
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
			if (rect.sizeDelta.y < 200)
				numY = 200;
			else if (rect.sizeDelta.y > Screen.height)
				numY = Screen.height;

			if (rect.sizeDelta.x < 250)
				numX = 250;
			else if (rect.sizeDelta.x > 540)
				numX = 540;

			rect.sizeDelta = new Vector2(numX, numY);
		}

		public void onEndResize(BaseEventData eventData)
		{
			resizing = false;

			if (rect == null)
				return;

			checkMaxResize((int)rect.sizeDelta.y, (int)rect.sizeDelta.x);
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

		public void FadeIn()
		{
			Fade(1, fastFadeDuration);
		}

		public void FadeOut()
		{
			Fade(0.6f, slowFadeDuration);
		}

		public void DestroyChild(GameObject obj)
		{
			if (obj == null)
				return;

			obj.SetActive(false);

			Destroy(obj);
		}

		public void DestroyPopup()
		{
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
