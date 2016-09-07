using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_SortMenu : CW_Popup
	{
		private IMissionSection missionInterface;
		private CW_Window parent;

		public void setSort(CW_Window p, IMissionSection m)
		{
			if (p == null)
				return;

			if (m == null)
				return;

			parent = p;

			missionInterface = m;
		}

		public void SortDifficulty()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(0);

			parent.DestroyChild(gameObject);
		}

		public void SortExpiration()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(1);

			parent.DestroyChild(gameObject);
		}

		public void SortAccept()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(2);

			parent.DestroyChild(gameObject);
		}

		public void SortReward()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(3);

			parent.DestroyChild(gameObject);
		}

		public void SortType()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(4);

			parent.DestroyChild(gameObject);
		}

		public void SortPlanet()
		{
			if (parent == null)
				return;

			if (missionInterface == null)
				return;

			missionInterface.SetSort(5);

			parent.DestroyChild(gameObject);
		}
	}
}
