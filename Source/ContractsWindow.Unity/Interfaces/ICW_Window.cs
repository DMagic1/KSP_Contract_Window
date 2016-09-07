using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool IsVisible { get; set; }

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

		IProgressPanel GetProgress { get; }

		void Rebuild();

		void SetSort(int i);

		void NewMission(string title, Guid id);

		void SetAppState(bool on);

		void SetWindowPosition(Rect r);
	}
}
