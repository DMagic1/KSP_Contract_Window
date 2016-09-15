using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_AgencyPanel : CW_Popup
	{
		[SerializeField]
		private RawImage AgencyLogo = null;
		[SerializeField]
		private Text AgencyTitle = null;
		
		public void setAgent(string title, Texture logo)
		{
			if (logo == null)
				return;

			AgencyLogo.texture = logo;
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
