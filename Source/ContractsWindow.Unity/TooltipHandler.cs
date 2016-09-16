using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity
{

	public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IScrollHandler
	{
		[SerializeField, TextArea(2, 10)]
		private string Text = "";

		private bool isActive = true;
		private ToolTip tooltip;

		private void Start()
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Tooltip == null)
				return;

			if (CW_Window.Window.Interface == null)
				return;

			if (CW_Window.Window.Interface.MainCanvas == null)
				return;

			if (string.IsNullOrEmpty(Text))
				return;

			GameObject obj = Instantiate(CW_Window.Window.Tooltip);

			if (obj == null)
				return;

			obj.transform.SetParent(CW_Window.Window.Interface.MainCanvas.transform, false);

			tooltip = obj.GetComponent<ToolTip>();
		}

		public void SetNewText(string s)
		{
			Text = s;
		}

		public bool IsActive
		{
			set { isActive = value; }
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			if (!isActive)
				return;

			tooltip.SetTooltip(Text);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			tooltip.HideTooltip();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (tooltip == null)
				return;

			tooltip.HideTooltip();
		}

		public void OnScroll(PointerEventData eventData)
		{
			if (CW_Window.Window == null)
				return;

			if (CW_Window.Window.Scroll == null)
				return;

			CW_Window.Window.Scroll.OnScroll(eventData);
		}
	}
}
