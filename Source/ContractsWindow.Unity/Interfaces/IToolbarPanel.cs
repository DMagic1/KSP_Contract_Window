using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IToolbarPanel
	{
		bool BlizzyAvailable { get; }

		bool UsingStock { get; set; }

		bool ReplacingStock { get; set; }
	}
}
