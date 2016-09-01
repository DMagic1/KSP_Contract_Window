using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_MissionSection : MonoBehaviour
	{

		[SerializeField]
		private GameObject ContractSectionPrefab = null;
		[SerializeField]
		private Transform ContractSectionTransform = null;

		private IMissionSection missionInterface;
		private CW_Window window;
		private List<CW_ContractSection> contracts = new List<CW_ContractSection>();


	}
}
