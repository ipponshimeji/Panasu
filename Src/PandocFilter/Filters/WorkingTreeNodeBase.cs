using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class WorkingTreeNodeBase {
		#region constants

		public const int UndefinedIndex = -1;

		#endregion


		#region data

		public string Name { get; private set; } = null;

		public int Index { get; protected set; } = UndefinedIndex;

		#endregion


		#region properties

		public bool IsParentObject {
			get {
				return this.Name != null;
			}
		}

		public bool IsParentArray {
			get {
				return this.Index != UndefinedIndex;
			}
		}

		#endregion


		#region creation and destruction

		protected WorkingTreeNodeBase() {
		}


		// defined formally. actually do nothing.
		protected void Initialize() {
			// argument checks

			// initialize this instance
			Debug.Assert(this.Name == null);
			Debug.Assert(this.Index == UndefinedIndex);
		}

		protected void Initialize(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentOutOfRangeException(nameof(name));
			}

			// initialize this instance
			this.Name = name;
			Debug.Assert(this.Index == UndefinedIndex);
		}

		protected void Initialize(int index) {
			// argument checks
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			// initialize this instance
			Debug.Assert(this.Name == null);
			this.Index = index;
		}

		protected void Clear() {
			// clear members
			this.Index = UndefinedIndex;
			this.Name = null;
		}

		#endregion
	}
}
