using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces
{
	public class IntervalNodeUI : IIntervalNode
	{
		private progressInterval node;

		public string NodeOrder
		{
			get
			{
				if (node == null)
					return "";

				return node.Interval.ToString();
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

		public string NodeValue
		{
			get
			{
				if (node == null)
					return "";

				return node.getRecord(node.Interval).ToString();
			}
		}

		public string RewardText
		{
			get
			{
				if (node == null)
					return "";

				int i = node.Interval;

				return string.Format("<color=#69D84FFF>£ {0}</color>  <color=#02D8E9FF>© {1}</color>  <color=#C9B003FF>¡ {2}</color>", node.getFundsString(i), node.getScienceString(i), node.getRepString(i));
			}
		}
	}
}
