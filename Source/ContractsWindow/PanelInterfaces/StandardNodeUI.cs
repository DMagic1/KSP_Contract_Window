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

				string body = node.Body == null ? "" : node.Body.theName;

				return string.Format(node.Descriptor, body);
			}
		}

		private string coloredText(string s, char c, string color)
		{
			if (string.IsNullOrEmpty(s))
				return "";

			return string.Format("<color={0}>{1}{2}</color>  ", color, c, s);
		}

		public string RewardText
		{
			get
			{
				if (node == null)
					return "";

				return string.Format("{0}{1}{2}", coloredText(node.FundsRewardString, '£', "#69D84FFF"), coloredText(node.SciRewardString, '©', "#02D8E9FF"), coloredText(node.RepRewardString, '¡', "#C9B003FF"));
			}
		}
	}
}
