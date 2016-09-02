using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionSection
	{
		bool IsVisible { get; set; }

		string MissionTitle { get; }

		bool MasterMission { get; }

		IList<IContractSection> GetContracts();

		void ProcessStyle(GameObject obj);

		void Update();
	}
}
