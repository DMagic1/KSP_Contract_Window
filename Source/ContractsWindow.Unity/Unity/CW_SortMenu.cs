using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	[RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
	public class CW_SortMenu : CW_Popup
	{
		private IMissionSection missionInterface;

		public void setSort(IMissionSection m)
		{
			if (m == null)
				return;

			missionInterface = m;

			FadeIn();
		}

		public void SortDifficulty()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(0);

			CW_Window.Window.FadePopup(this);
		}

		public void SortExpiration()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(1);

			CW_Window.Window.FadePopup(this);
		}

		public void SortAccept()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(2);

			CW_Window.Window.FadePopup(this);
		}

		public void SortReward()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(3);

			CW_Window.Window.FadePopup(this);
		}

		public void SortType()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(4);

			CW_Window.Window.FadePopup(this);
		}

		public void SortPlanet()
		{
			if (CW_Window.Window == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(5);

			CW_Window.Window.FadePopup(this);
		}

		public override void ClosePopup()
		{
			gameObject.SetActive(false);

			Destroy(gameObject);
		}
	}
}
