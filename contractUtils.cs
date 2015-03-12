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
					parameterContainer pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
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
					parameterContainer pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
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

		/// <summary>
		/// A method for returning a contractContainer object. The contract in question must be loaded by Contracts
		/// Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the contract in question</param>
		/// <returns>contractContainer object</returns>
		public static contractContainer getContractContainer(Contract contract)
		{
			try
			{
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				return c;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Contract Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for returning a parameterContainer object. The contract and parameter in question must be loaded by 
		/// Contracts Window + and may return null. All fields within the object are publicly accessible through properties.
		/// </summary>
		/// <param name="contract">Instance of the root contract (contractParameter.Root)</param>
		/// <param name="parameter">Instance of the contract parameter in question</param>
		/// <returns>parameterContainer object</returns>
		public static parameterContainer getParameterContainer(Contract contract, ContractParameter parameter)
		{
			try
			{
				contractContainer c = contractScenario.Instance.getContract(contract.ContractGuid);
				parameterContainer pC = null;
				if (c != null)
					pC = c.AllParamList.SingleOrDefault(a => a.CParam == parameter);
				return pC;
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Something went wrong when attempting to get a Parameter Container object: " + e);
				return null;
			}
		}

		/// <summary>
		/// A method for updating all reward values for active contracts
		/// </summary>
		/// <param name="contractType">Type of contract that needs to be updated; must be a subclass of Contracts.Contract</param>
		public static void UpdateContractType(Type contractType)
		{
			if (contractType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update contract method is null");
				return;
			}
			if (contractType.IsSubclassOf(typeof(Contracts.Contract)))
			{
				Debug.LogWarning("[Contracts +] Type provided is not derived from the base Contract class");
				return;
			}
			if (contractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				contractScenario.Instance.contractChanged(contractType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating contract values: " + e);
			}
		}

		/// <summary>
		/// A method for updating contract parameter reward values for active contracts
		/// </summary>
		/// <param name="parameterType">Type of parameter that needs to be updated; must be a subclass of Contracts.ContractParameter</param>
		public static void UpdateParameterType(Type parameterType)
		{
			if (parameterType == null)
			{
				Debug.LogWarning("[Contracts +] Type provided for update parameter method is null");
				return;
			}
			if (parameterType.IsSubclassOf(typeof(Contracts.ContractParameter)))
			{
				Debug.LogWarning("[Contracts +] Type provided is not derived from the base Contract Parameter class");
				return;
			}
			if (contractScenario.Instance == null)
			{
				Debug.LogWarning("[Contracts +] Contracts Window + scenario module is not loaded");
				return;
			}

			try
			{
				contractScenario.Instance.paramChanged(parameterType);
			}
			catch (Exception e)
			{
				Debug.LogWarning("[Contracts +] Error while updating parameter values: " + e);
			}
		}
	}
}
