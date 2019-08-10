using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace PandocUtil.PandocFilter.Filters {
	public class ASTEditor {
		#region types

		protected class Node: WorkingTreeNode<Node> {
			#region data

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

			#endregion


			#region creation and destruction

			protected Node() {
			}


			protected new void Clear() {
				// clear this instance
				if (this.annotation != null) {
					this.annotation.Clear();
				}
				base.Clear();
			}

			#endregion
		}

		protected class ObjectNode: Node {
			#region data

			private Dictionary<string, object> contents = new Dictionary<string, object>();

			#endregion


			#region creation and destruction

			private ObjectNode(): base() {
			}

			#endregion
		}

		protected class ArrayNode: Node {
			#region data

			private LinkedList<object> contents = new LinkedList<object>();

			#endregion


			#region creation and destruction

			private ArrayNode() : base() {
			}

			#endregion
		}

		#endregion


		#region creation

		public ASTEditor() {
		}

		#endregion
	}
}
