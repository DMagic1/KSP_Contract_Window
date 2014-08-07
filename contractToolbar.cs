using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

			if (File.Exists(Path.Combine(new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName, "GameData/Contracts Window/ContractsIcon.png").Replace("\\", "/")))
				contractButton.TexturePath = "Contracts Window/ContractsIcon";
			else
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
