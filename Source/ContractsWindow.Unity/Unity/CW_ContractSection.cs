using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_ContractSection : MonoBehaviour
	{
		[SerializeField]
		private Text ContractTitle = null;
		[SerializeField]
		private Image Stars = null;
		[SerializeField]
		private Text TimeRemaining = null;
		[SerializeField]
		private Image ShowHide = null;
		[SerializeField]
		private Image Pin = null;
		[SerializeField]
		private Image ContractNoteToggle = null;
		[SerializeField]
		private Text ContractRewardText = null;
		[SerializeField]
		private Text ContractPenaltyText = null;
		[SerializeField]
		private GameObject ContractNotePrefab = null;
		[SerializeField]
		private Transform ContractNoteTransform = null;
		[SerializeField]
		private GameObject ParamaterSectionPrefab = null;
		[SerializeField]
		private Transform ParamaterSectionTransform = null;
		[SerializeField]
		private Sprite Stars_One = null;
		[SerializeField]
		private Sprite Stars_Two = null;
		[SerializeField]
		private Sprite Stars_Three = null;
		[SerializeField]
		private Sprite Show = null;
		[SerializeField]
		private Sprite Hide = null;
		[SerializeField]
		private Sprite Close = null;
		[SerializeField]
		private Sprite Pin_Sprite = null;
		[SerializeField]
		private Sprite UnPin = null;
		[SerializeField]
		private Sprite NoteOn = null;
		[SerializeField]
		private Sprite NoteOff = null;

		private Color textColor = new Color();
		private Color successColor = new Color();
		private Color failColor = new Color();
		private Color timerWarningColor = new Color();

		private IContractSection contractInterface;
		private CW_MissionSection mission;
		private List<CW_ParameterSection> parameters = new List<CW_ParameterSection>();
	}
}
