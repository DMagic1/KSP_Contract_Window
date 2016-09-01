using System;
using System.Collections.Generic;
using System.Linq;
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
		private ScrollRect scrollRect = null;
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

		private Vector2 mouseStart;
		private Vector3 windowStart;
		private RectTransform rect;

		private bool dragging;
		private bool resizing;

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

			Destroy(obj);
		}

	}
}
