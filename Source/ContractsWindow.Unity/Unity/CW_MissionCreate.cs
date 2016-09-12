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

		private IContractSection contract;

		public void setPanel(IContractSection c)
		{
			if (c == null)
				return;

			contract = c;
		}

		public void CreateMission()
		{
			if (CW_Window.Window == null)
				return;


			if (CW_Window.Window.Interface == null)
				return;

			if (NewMission == null)
				return;

			CW_Window.Window.Interface.NewMission(NewMission.text, contract.ID);

			DestroyPanel();
		}

		public void DestroyPanel()
		{
			if (CW_Window.Window == null)
				return;

			CW_Window.Window.DestroyChild(gameObject);
		}
	}
}
