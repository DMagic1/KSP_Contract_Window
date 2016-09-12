using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IIntervalNode
	{
		bool IsReached { get; }

		int Intervals { get; }
		
		int NodeInterval { get; }

		string NodeTitle { get; }

		string NodeValue(int i);

		string RewardText(int i);
	}
}
