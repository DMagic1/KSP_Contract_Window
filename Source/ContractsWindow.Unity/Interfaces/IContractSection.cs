#region license
/*The MIT License (MIT)
IContractSection - Interface for transferring contract information

Copyright (c) 2016 DMagic

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using ContractsWindow.Unity.Unity;
using UnityEngine;

namespace ContractsWindow.Unity.Interfaces
{
	public interface IContractSection
	{
		bool IsHidden { get; set; }

		bool ShowParams { get; set; }

		bool IsPinned { get; set; }

		ContractState ContractState { get; }

		Guid ID { get; }

		int? Order { get; set; }

		int Difficulty { get; }

		int TimeState { get; }

		Texture AgencyLogo { get; }

		string TimeRemaining { get; }

		string AgencyName { get; }

		string ContractTitle { get; }

		string RewardText { get; }

		string PenaltyText { get; }

		string GetNote { get; }

		IList<IParameterSection> GetParameters { get; }

		void RemoveContractFromAll();
	}
}
