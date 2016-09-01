using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionAddObject
	{
		string MissionName { get; }

		string ContractNumber { get; }

		bool ContractContained { get; }

		void Update();

		void RemoveMission(CW_MissionAddObject mission);

		void SetMission(CW_MissionAddObject mission);
	}
}
