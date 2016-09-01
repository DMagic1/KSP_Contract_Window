using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionSelectObject
	{
		string MissionName { get; }

		string ContractNumber { get; }

		void Update();

		void SetMission(CW_MissionSelectObject mission);
	}
}
