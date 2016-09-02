using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool IsVisible { get; set; }

		bool SortDown { get; set; }

		bool ShowHidden { get; set; }

		bool HideTooltips { get; set; }

		string Version { get; }

		IList<IMissionSection> GetMissions();

		IMissionEditPanel GetMissionEdit();

		IMissionSelect GetMissionSelect();

		INewMissionPanel GetMissionCreate();

		ISortMenu GetSort();

		IToolbarPanel GetToolbar();

		IRebuildPanel GetRefresh();

		IScalarPanel GetScalar();

		IProgressPanel GetProgress();

		void SetAppState(bool on);

		void ProcessStyle(GameObject obj);
	}
}
