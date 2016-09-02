using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface ICW_Window
	{
		bool IsVisible { get; set; }

		bool SortUp { get; set; }

		bool ShowActive { get; set; }

		bool ShowTooltips { get; set; }

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
