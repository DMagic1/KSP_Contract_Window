using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IIntervalNode
	{
		string NodeTitle { get; }

		string NodeValue { get; }

		string NodeOrder { get; }

		string RewardText { get; }
	}
}
