using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool IsVisible { get; set; }

		void GetMissions();

		void SetAppState(bool on);

		void ProcessStyle(GameObject obj);
	}
}
