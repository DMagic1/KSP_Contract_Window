using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_AgencyPanel : CW_Popup
	{
		[SerializeField]
		private Image AgencyLogo = null;
		[SerializeField]
		private Text AgencyTitle = null;
		
		public void setAgent(string title, Sprite logo)
		{
			if (logo == null)
				return;

			AgencyLogo.sprite = logo;
			AgencyTitle.text = title;

			FadeIn();
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
