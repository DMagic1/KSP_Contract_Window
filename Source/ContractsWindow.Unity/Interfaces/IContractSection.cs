using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IContractSection
	{
		bool IsVisible { get; set; }

		bool ShowActive { get; set; }

		bool IsPinned { get; set; }

		ContractState ParameterState { get; }

		int Order { get; set; }

		int Difficulty { get; }

		Sprite AgencyLogo { get; }

		string TimeRemaining { get; }

		string AgencyName { get; }

		string NoteText { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		void ProcessStyle(GameObject obj);

		void Update();
	}
}
