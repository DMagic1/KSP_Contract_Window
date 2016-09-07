using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionCreate : CW_Popup
	{
		[SerializeField]
		private Text NewMission = null;

		private CW_Window parent;
		private IContractSection contract;

		public void setPanel(CW_Window p, IContractSection c)
		{
			if (p == null)
				return;

			if (c == null)
				return;

			parent = p;

			contract = c;
		}

		public void CreateMission()
		{
			if (parent == null)
				return;

			if (parent.Interface == null)
				return;

			if (NewMission == null)
				return;

			parent.Interface.NewMission(NewMission.text, contract.ID);

			DestroyPanel();
		}

		public void DestroyPanel()
		{
			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
