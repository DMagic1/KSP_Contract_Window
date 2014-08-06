using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Toolbar;

namespace ContractsWindow
{
	[KSPAddonImproved(KSPAddonImproved.Startup.EditorAny | KSPAddonImproved.Startup.TimeElapses, false)]
	class contractToolbar : MonoBehaviour
	{
		private IButton contractButton;

		internal contractToolbar()
		{
			contractButton = ToolbarManager.Instance.add("ContractsWindow", "ContractManager");

			contractButton.TexturePath = "000_Toolbar/resize-cursor";
			contractButton.ToolTip = "Contract Manager";
			contractButton.OnClick += (e) => contractsWindow.IsVisible = !contractsWindow.IsVisible;
		}

		internal void OnDestroy()
		{
			contractButton.Destroy();
		}
	}
}
