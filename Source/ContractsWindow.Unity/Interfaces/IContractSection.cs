using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IContractSection
	{
		bool IsVisible { get; set; }

		bool IsHidden { get; set; }

		bool IsPinned { get; set; }

		bool HasNote { get; }

		ContractState ContractState { get; }

		int Order { get; set; }

		int Difficulty { get; }

		int TimeState { get; }

		IMissionAddPanel GetMissionAdd();

		IAgencyPanel GetAgent();

		Sprite AgencyLogo { get; }

		string TimeRemaining { get; }

		string AgencyName { get; }

		string ContractTitle { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		INote GetNote { get; }

		IList<IParameterSection> GetParameters();

		void RemoveContract();

		void ProcessStyle(GameObject obj);

		void Update();
	}
}
