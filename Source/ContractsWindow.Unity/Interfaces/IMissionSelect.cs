using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionSelect
	{
		bool IsVisible { get; set; }

		IList<IMissionSelectObject> GetMissions();

		void ProcessStyle(GameObject obj);

		void SetParent(CW_MissionSelect section);
	}
}
