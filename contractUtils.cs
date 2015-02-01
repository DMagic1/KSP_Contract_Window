#region license
/*The MIT License (MIT)
Contract Utilities - Public utilities for accessing and altering internal information

Copyright (c) 2014 DMagic

KSP Plugin Framework by TriggerAu, 2014: http://forum.kerbalspaceprogram.com/threads/66503-KSP-Plugin-Framework

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
using System.Linq;
using Contracts;
using Contracts.Parameters;
using UnityEngine;

namespace ContractsWindow
{
	public static class contractUtils
	{

		/// <summary>
		/// A method for manually resetting a locally cached contract title.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <param name="name">The new contract title</param>
		public static void setContractTitle (Contract contract, string name)
		{
			try
			{
				if (string.IsNullOrEmpty(name))
					return;
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				if (c != null)
				{
					c.Title = name;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Contract Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract notes.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <param name="notes">The new contract notes</param>
		public static void setContractNotes (Contract contract, string notes)
		{
			try
			{
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				if (c != null)
				{
					c.Notes = notes;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign new Contract Notes: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting a locally cached contract parameter title.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="name">The new contract parameter title</param>
		public static void setParameterTitle (Contract contract, ContractParameter parameter, string name)
		{
			try
			{
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				if (c != null)
				{
					parameterContainer pC = c.ParamList.First(a => a.CParam == parameter);
					if (pC != null)
					{
						pC.Title = name;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Title: " + e);
			}
		}

		/// <summary>
		/// A method for manually resetting locally cached contract parameter notes.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <param name="notes">The new contract parameter notes</param>
		public static void setParameterNotes(Contract contract, ContractParameter parameter, string notes)
		{
			try
			{
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				if (c != null)
				{
					parameterContainer pC = c.ParamList.First(a => a.CParam == parameter);
					if (pC != null)
					{
						pC.Notes = notes;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to assign a new Parameter Notes: " + e);
			}
		}
	}
}
