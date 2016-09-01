using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IMissionAddPanel
	{
		IList<IMissionAddObject> GetMissions();

		void ProcessStyle(GameObject obj);
	}
}
