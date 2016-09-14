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
		private int interval;

		public void setNode(IIntervalNode node, int i)
		{
			if (node == null)
				return;

			intervalInterface = node;

			interval = i;

			if (Title != null)
				Title.text = getTitle();

			if (Reward != null)
				Reward.text = node.RewardText(i);
		}

		public void UpdateText()
		{
			if (intervalInterface == null)
				return;

			if (Title != null)
				Title.text = getTitle();

			if (Reward != null)
				Reward.text = intervalInterface.RewardText(interval);
		}

		private string getTitle()
		{
			if (intervalInterface == null)
				return "";

			return string.Format("{0} Record {1}: {2}", intervalInterface.NodeTitle, interval, intervalInterface.NodeValue(interval));
		}
	}
}
