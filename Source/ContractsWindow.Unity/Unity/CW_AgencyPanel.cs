using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_AgencyPanel : MonoBehaviour
	{
		[SerializeField]
		private Image AgencyLogo = null;
		[SerializeField]
		private Text AgencyTitle = null;

		private IAgencyPanel agentInterface;
		private CW_Window parent;

		public void setAgent(IAgencyPanel agency, CW_Window p)
		{
			if (agency == null)
				return;

			if (p == null)
				return;

			parent = p;

			agentInterface = agency;

			AgencyLogo.sprite = agency.AgencyLogo;
			AgencyTitle.text = agency.AgencyTitle;
		}

	}
}
