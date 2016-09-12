using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces
{
	public class IntervalNodeUI : IIntervalNode
	{
		private progressInterval node;

		public IntervalNodeUI(progressInterval n)
		{
			if (n == null)
				return;

			node = n;
		}

		public bool IsReached
		{
			get
			{
				if (node == null)
					return false;

				return node.IsReached;
			}
		}

		public int Intervals
		{
			get
			{
				if (node == null)
					return 0;

				return node.TotalIntervals;
			}
		}

		public int NodeInterval
		{
			get
			{
				if (node == null)
					return 0;

				return node.Interval;
			}
		}

		public string NodeTitle
		{
			get
			{
				if (node == null)
					return "";

				return node.Descriptor;
			}
		}

		public string NodeValue(int i)
		{
			if (node == null)
				return "";

			return node.getRecord(i).ToString();
		}

		public string RewardText(int i)
		{
			if (node == null)
				return "";

			return string.Format("<color=#69D84FFF>£ {0}</color>  <color=#02D8E9FF>© {1}</color>  <color=#C9B003FF>¡ {2}</color>", node.getFundsString(i), node.getScienceString(i), node.getRepString(i));
		}

	}
}
