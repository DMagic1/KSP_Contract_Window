using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IParameterSection
	{
		ContractState ParameterState { get; }

		int ParamLayer { get; }

		string TitleText { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		string GetNote { get; }

		IList<IParameterSection> GetSubParams { get; }
	}
}
