using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IContractSection
	{
		bool IsHidden { get; set; }

		bool ShowParams { get; set; }

		bool IsPinned { get; set; }

		ContractState ContractState { get; }

		Guid ID { get; }

		int? Order { get; set; }

		int Difficulty { get; }

		int TimeState { get; }

		Texture AgencyLogo { get; }

		string TimeRemaining { get; }

		string AgencyName { get; }

		string ContractTitle { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		string GetNote { get; }

		IList<IParameterSection> GetParameters { get; }

		void RemoveContractFromAll();
	}
}
