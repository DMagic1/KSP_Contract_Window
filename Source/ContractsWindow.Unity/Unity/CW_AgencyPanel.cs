using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
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

		private IAgencyPanel agentInterface;

		public void setAgent(IAgencyPanel agency)
		{
			if (agency == null)
				return;

			agentInterface = agency;

			AgencyLogo.sprite = agency.AgencyLogo;
			AgencyTitle.text = agency.AgencyTitle;
		}

	}
}
