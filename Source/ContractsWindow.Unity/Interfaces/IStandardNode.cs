using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IStandardNode
	{
		bool IsVisible { get; }

		bool HasNote { get; }

		string NodeText { get; }

		string RewardText { get; }

		string GetNote { get; }

		void ProcessStyle(GameObject obj);
	}
}
