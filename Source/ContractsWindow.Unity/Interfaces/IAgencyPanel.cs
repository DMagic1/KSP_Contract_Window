using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IAgencyPanel
	{
		Sprite AgencyLogo { get; }

		string AgencyTitle { get; }
	}
}
