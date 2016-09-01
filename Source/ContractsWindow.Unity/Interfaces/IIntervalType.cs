using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IIntervalType
	{
		string IntervalName { get; }

		IList<IIntervalNode> GetNodes();

		void ProcessStyle(GameObject obj);
	}
}
