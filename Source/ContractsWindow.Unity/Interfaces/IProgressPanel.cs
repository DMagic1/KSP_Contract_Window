using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IProgressPanel
	{
		bool IsVisible { get; set; }

		bool AnyInterval { get; }

		bool AnyPOI { get; }

		bool AnyStandard { get; }

		bool AnyBody { get; }

		bool AnyBodyNode(string s);

		IList<IIntervalNode> GetIntervalNodes { get; }

		IList<IStandardNode> GetPOINodes { get; }

		IList<IStandardNode> GetStandardNodes { get; }

		Dictionary<string, List<IStandardNode>> GetBodies { get; }
	}
}
