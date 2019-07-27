using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class AST {
		#region data

		private readonly IDictionary<string, object> root;

		#endregion


		#region properties

		public IDictionary<string, object> Root {
			get {
				return this.root;
			}
		}

		#endregion


		#region creation

		public AST(IDictionary<string, object> root) {
			// argument checks
			if (root == null) {
				throw new ArgumentNullException(nameof(root));
			}

			this.root = root;
		}

		#endregion
	}
}
