using System;
using System.Collections.Generic;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IParameterSection
	{
		bool IsVisible { get; set; }

		ContractState ParameterState { get; }

		string NoteText { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		void Update();
	}
}
