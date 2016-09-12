using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IStandardNode
	{
		bool IsComplete { get; }

		string NodeText { get; }

		string RewardText { get; }

		string GetNote { get; }
	}
}
