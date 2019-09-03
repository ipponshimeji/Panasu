using System;
using System.Collections.Generic;

namespace PandocUtil.PandocFilter.Filters {
	public class ASTEditor {
		#region types

		protected class Node: WorkingTreeNode {
			#region creation and destruction

			protected Node() {
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
