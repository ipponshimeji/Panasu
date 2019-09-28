using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Panasu.Filters {
	public class WorkingTreeNode {
		#region constants

		public const int UndefinedIndex = -1;

		#endregion


		#region data

		protected WorkingTreeNode Parent { get; private set; } = null;

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

		protected WorkingTreeNode() {
		}


		// defined formally. actually do nothing.
		protected void Initialize() {
			// argument checks

			// initialize this instance
			Debug.Assert(this.Parent == null);
			Debug.Assert(this.Name == null);
			Debug.Assert(this.Index == UndefinedIndex);
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Initialize(WorkingTreeNode parent) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}

			// initialize this instance
			this.Parent = parent;
			Debug.Assert(this.Name == null);
			Debug.Assert(this.Index == UndefinedIndex);
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Initialize(WorkingTreeNode parent, string name) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}
			if (name == null) {
				throw new ArgumentOutOfRangeException(nameof(name));
			}
			// name can be empty

			// initialize this instance
			this.Parent = parent;
			this.Name = name;
			Debug.Assert(this.Index == UndefinedIndex);
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);

			return;
		}

		protected void Initialize(WorkingTreeNode parent, int index) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			// initialize this instance
			this.Parent = parent;
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
			this.Parent = null;

			return;
		}

		#endregion


		#region methods

		public string GetLocation() {
			StringBuilder buf = new StringBuilder();
			AppendLocation(buf);
			return buf.ToString();
		}

		#endregion


		#region overrides

		public virtual void AppendLocation(StringBuilder buf) {
			// argument checks
			if (buf == null) {
				throw new ArgumentNullException(nameof(buf));
			}

			// append parent location
			if (this.Parent != null) {
				this.Parent.AppendLocation(buf);
			}

			// append this class level location
			if (this.IsParentObject) {
				// "parent.name" format
				Debug.Assert(this.Name != null);
				if (0 < buf.Length) {
					buf.Append('.');
				}
				buf.Append(this.Name);
			} else if (this.IsParentArray) {
				// "parent[index]" format
				Debug.Assert(0 <= this.Index);
				buf.Append($"[{this.Index}]");
			}
		}

		#endregion
	}
}
