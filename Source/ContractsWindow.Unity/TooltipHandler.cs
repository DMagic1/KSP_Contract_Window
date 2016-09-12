using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ContractsWindow.Unity
{

	public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private GameObject TooltipPrefab = null;
		[SerializeField, TextArea(2, 10)]
		private string Text = "";

		private bool isActive = true;
		private ToolTip tooltip;

		private void Start()
		{
			if (TooltipPrefab == null)
				return;

			if (string.IsNullOrEmpty(Text))
				return;

			GameObject obj = Instantiate(TooltipPrefab);

			if (obj == null)
				return;

			obj.transform.SetParent(transform);

			tooltip = obj.GetComponent<ToolTip>();

			if (tooltip == null)
				return;

			tooltip.gameObject.SetActive(false);
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
	}
}
