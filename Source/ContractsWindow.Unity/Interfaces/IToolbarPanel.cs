using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IToolbarPanel
	{
		bool BlizzyAvailable { get; }

		bool UsingStock { get; }

		bool ReplacingStock { get; }

		void UseStock(bool yes);

		void ReplaceStock(bool yes);
	}
}
