using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool HideTooltips { get; set; }

		bool BlizzyAvailable { get; }

		bool StockToolbar { get; set; }

		bool ReplaceToolbar { get; set; }

		bool LargeFont { get; set; }

		bool IgnoreScale { get; set; }

		float Scale { get; set; }

		float MasterScale { get; set; }

		string Version { get; }

		IList<IMissionSection> GetMissions { get; }

		IMissionSection GetCurrentMission { get; }

		void Rebuild();

		void NewMission(string title, Guid id);

		void SetWindowPosition(Rect r);
	}
}
