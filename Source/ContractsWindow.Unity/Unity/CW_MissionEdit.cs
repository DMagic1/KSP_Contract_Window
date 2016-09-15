using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_MissionEdit : CW_Popup
	{
		[SerializeField]
		private Text NewMissionName = null;

		private IMissionSection missionInterface;

		public void setMission(IMissionSection mission)
		{
			if (mission == null)
				return;

			missionInterface = mission;

			FadeIn();
		}

		public void ChangeName()
		{
			if (missionInterface == null)
				return;

			if (NewMissionName == null)
				return;

			missionInterface.MissionTitle = NewMissionName.text;

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.FadePopup(this);
		}

		public void DeleteMission()
		{
			if (missionInterface == null)
				return;

			missionInterface.RemoveMission();

			if (CW_Window.Window == null)
				return;

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
