using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IParameterSection
	{
		bool IsVisible { get; set; }

		bool HasNote { get; }

		ContractState ParameterState { get; }

		string TitleText { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		INote GetNote { get; }

		void ProcessStyle(GameObject obj);

		void Update();
	}
}
