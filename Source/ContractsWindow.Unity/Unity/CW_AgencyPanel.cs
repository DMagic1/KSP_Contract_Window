using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
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
		}

	}
}
