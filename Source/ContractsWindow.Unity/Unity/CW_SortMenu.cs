using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_SortMenu : CW_Popup
	{
		private ISortMenu sortInterface;
		private CW_Window parent;

		public void setSort(ISortMenu sort, CW_Window p)
		{
			if (sort == null)
				return;

			if (p == null)
				return;

			sortInterface = sort;

			parent = p;
		}

		public void SortDifficulty()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(0);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void SortExpiration()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(1);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void SortAccept()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(2);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void SortReward()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(3);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void SortType()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(4);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}

		public void SortPlanet()
		{
			if (sortInterface == null)
				return;

			sortInterface.SetSort(5);

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
