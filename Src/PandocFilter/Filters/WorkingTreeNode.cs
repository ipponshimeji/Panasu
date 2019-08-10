using System;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class WorkingTreeNode<ActualNode>: WorkingTreeNodeBase where ActualNode : WorkingTreeNodeBase {
		#region data

		public ActualNode Parent { get; private set; } = null;

		#endregion


		#region creation and destruction

		protected WorkingTreeNode() {
		}


		// defined formally. actually do nothing.
		protected new void Initialize() {
			// argument checks

			// initialize this instance
			base.Initialize();
			Debug.Assert(this.Parent == null);
		}

		protected void Initialize(ActualNode parent) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}

			// initialize this instance
			base.Initialize();
			this.Parent = parent;
		}

		protected void Initialize(ActualNode parent, string name) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}

			// initialize this instance
			base.Initialize(name);
			this.Parent = parent;
		}

		protected void Initialize(ActualNode parent, int index) {
			// argument checks
			if (parent == null) {
				throw new ArgumentOutOfRangeException(nameof(parent));
			}

			// initialize this instance
			base.Initialize(index);
			this.Parent = parent;
		}

		protected new void Clear() {
			// clear members
			this.Parent = null;
			base.Clear();
		}

		#endregion
	}
}
