using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_IntervalNode : MonoBehaviour
	{
		[SerializeField]
		private Text Title = null;
		[SerializeField]
		private Text Reward = null;

		private IIntervalNode intervalInterface;

		public void setNode(IIntervalNode node)
		{
			if (node == null)
				return;

			intervalInterface = node;

			if (Title != null)
				Title.text = getTitle();

			if (Reward != null)
				Reward.text = node.RewardText;
		}

		private string getTitle()
		{
			if (intervalInterface == null)
				return "";

			return string.Format("{0} Record {1}: {2}", intervalInterface.NodeTitle, intervalInterface.NodeOrder, intervalInterface.NodeValue);
		}
	}
}
