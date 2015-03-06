using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContractsWindow
{
	public class contractUIObject
	{
		private contractContainer container;
		private bool showParams;
		private int? order;

		internal contractUIObject(contractContainer c)
		{
			container = c;
			showParams = true;
			order = null;
		}

		public contractContainer Container
		{
			get { return container; }
		}

		public bool ShowParams
		{
			get { return showParams; }
			internal set { showParams = value; }
		}

		public int? Order
		{
			get { return order; }
			internal set { order = value; }
		}
	}
}
