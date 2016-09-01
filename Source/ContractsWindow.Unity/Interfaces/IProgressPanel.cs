using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IProgressPanel
	{
		bool IsVisible { get; }

		IList<IIntervalType> GetIntervalTypes();

		IList<IStandardNode> GetPOINodes();

		IList<IStandardNode> GetStandardNodes();

		IList<IBodyNodes> GetBodies();

		void ProcessStyle(GameObject obj);
	}
}
