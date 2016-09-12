using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using ContractsWindow.Unity.Unity;
using ProgressParser;

namespace ContractsWindow.PanelInterfaces
{
	public class progressUIPanel : IProgressPanel
	{

		private List<IBodyNodes> bodies = new List<IBodyNodes>();
		private List<IIntervalType> intervals = new List<IIntervalType>();
		private List<IStandardNode> pois = new List<IStandardNode>();
		private List<IStandardNode> standards = new List<IStandardNode>();

		public bool IsVisible { get; set; }

		public IList<IBodyNodes> GetBodies
		{
			get { return new List<IBodyNodes>(bodies.ToArray()); }
		}

		public IList<IIntervalType> GetIntervalTypes
		{
			get { return new List<IIntervalType>(intervals.ToArray()); }
		}

		public IList<IStandardNode> GetPOINodes
		{
			get { return new List<IStandardNode>(pois.ToArray()); }
		}

		public IList<IStandardNode> GetStandardNodes
		{
			get { return new List<IStandardNode>(standards.ToArray()); }
		}

	}
}
