using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Unity
{
	public class CW_Rebuild : CW_Popup
	{
		private IRebuildPanel rebuildInterface;
		private CW_Window parent;

		public void setInterface(IRebuildPanel rebuild, CW_Window p)
		{
			if (rebuild == null)
				return;

			if (p == null)
				return;

			parent = p;

			rebuildInterface = rebuild;
		}

		public void Rebuild()
		{
			if (rebuildInterface == null)
				return;

			rebuildInterface.Refresh();

			if (parent == null)
				return;

			parent.DestroyChild(gameObject);
		}
	}
}
