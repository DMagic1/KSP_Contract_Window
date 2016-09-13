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

		private string coloredText(string s, char c, string color)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return string.Format("<color={0}>{1}{2}</color>  ", color, c, s);
		}

		public string RewardText(int i)
		{
			if (node == null)
				return "";

			return string.Format("{0}{1}{2}", coloredText(node.getFundsString(i), '£', "#69D84FFF"), coloredText(node.getScienceString(i), '©', "#02D8E9FF"), coloredText(node.getRepString(i), '¡', "#C9B003FF"));
		}

	}
}
