using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IProgressPanel
	{
		bool IsVisible { get; set; }

		IList<IIntervalType> GetIntervalTypes { get; }

		IList<IStandardNode> GetPOINodes { get; }

		IList<IStandardNode> GetStandardNodes { get; }

		IList<IBodyNodes> GetBodies { get; }
	}
}
