using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces
{
	public class StandardNodeUI : IStandardNode
	{
		private progressStandard node;

		public StandardNodeUI(progressStandard n)
		{
			if (n == null)
				return;

			node = n;
		}

		public bool IsComplete
		{
			get
			{
				if (node == null)
					return false;

				return node.IsComplete;
			}
		}

		public string GetNote
		{
			get
			{
				if (node == null)
					return "";

				return string.Format(node.Note, node.NoteReference, node.KSPDateString);
			}
		}
		
		public string NodeText
		{
			get
			{
				if (node == null)
					return "";

				return node.Descriptor;
			}
		}
		
		public string RewardText
		{
			get
			{
				if (node == null)
					return "";

				return string.Format("<color=#69D84FFF>£ {0}</color>  <color=#02D8E9FF>© {1}</color>  <color=#C9B003FF>¡ {2}</color>", node.FundsRewardString, node.SciRewardString, node.RepRewardString);
			}
		}
	}
}
