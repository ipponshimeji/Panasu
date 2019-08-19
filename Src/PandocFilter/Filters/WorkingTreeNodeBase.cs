using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class WorkingTreeNodeBase {
		#region constants

		public const int UndefinedIndex = -1;

		#endregion


		#region data

		public string Name { get; protected set; } = null;

		public int Index { get; protected set; } = UndefinedIndex;

		private Dictionary<string, object> annotation = null;

		#endregion


		#region properties

		public IDictionary<string, object> Annotation {
			get {
				Dictionary<string, object> value = this.annotation;
				if (value == null) {
					value = new Dictionary<string, object>();
					this.annotation = value;
				}

				return value;
			}
		}

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
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Initialize(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentOutOfRangeException(nameof(name));
			}
			// name can be empty

			// initialize this instance
			this.Name = name;
			Debug.Assert(this.Index == UndefinedIndex);
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Initialize(int index) {
			// argument checks
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			// initialize this instance
			Debug.Assert(this.Name == null);
			this.Index = index;
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Clear() {
			// clear this instance
			if (this.annotation != null) {
				this.annotation.Clear();
			}
			this.Index = UndefinedIndex;
			this.Name = null;

			return;
		}

		#endregion
	}
}
