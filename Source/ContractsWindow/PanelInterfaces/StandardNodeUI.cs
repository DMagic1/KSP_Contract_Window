using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces
{
	public class StandardNodeUI : IStandardNode
	{
		private progressStandard node;

		public bool IsVisible { get; }

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
			}
		}
	}
}
