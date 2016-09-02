using System;
using System.Collections.Generic;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IAgencyPanel
	{
		Sprite AgencyLogo { get; }

		string AgencyTitle { get; }
	}
}
