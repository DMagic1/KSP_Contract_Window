using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionSection
	{
		string MissionTitle { get; set; }

		string ContractNumber { get; }

		bool IsVisible { get; set; }

		bool MasterMission { get; }

		bool ContractContained(IContractSection contract);

		bool DescendingOrder { get; set; }

		bool ShowHidden { get; set; }

		IList<IContractSection> GetContracts { get; }

		void RemoveMission();

		void SetMission();

		void SetSort(int i);

		void AddContract(IContractSection contract);

		void RemoveContract(IContractSection contract);

		void SetParent(CW_MissionSection mission);
	}
}
