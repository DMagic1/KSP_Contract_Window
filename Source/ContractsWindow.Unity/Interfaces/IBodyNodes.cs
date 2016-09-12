using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IBodyNodes
	{
		string BodyName { get; }

		IList<IStandardNode> GetNodes { get; }
	}
}
