using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionEditPanel
	{
		void ChangeName(string name);

		void DeleteMission();
	}
}
