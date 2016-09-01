using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ContractsWindow.Unity.Interfaces;

namespace ContractsWindow.Unity.Unity
{
	public class CW_ParameterSection : MonoBehaviour
	{

		[SerializeField]
		private Text ParameterText = null;
		[SerializeField]
		private Text ParameterRewardText = null;
		[SerializeField]
		private Text ParameterPenaltyText = null;
		[SerializeField]
		private Image ParameterNoteImage = null;
		[SerializeField]
		private Sprite NoteOn = null;
		[SerializeField]
		private Sprite NoteOff = null;
		[SerializeField]
		private GameObject NotePrefab = null;
		[SerializeField]
		private Transform NoteTransform = null;

		private Color textColor = new Color();
		private Color successColor = new Color();
		private Color failColor = new Color();
		private IParameterSection parameterInterface;
		private CW_ContractSection root;


	}
}
