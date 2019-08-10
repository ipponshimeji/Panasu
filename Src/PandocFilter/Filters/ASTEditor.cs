using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using PandocUtil.PandocFilter.Utils;

namespace PandocUtil.PandocFilter.Filters {
	public class ASTEditor {
		#region types

		protected class Node: WorkingTreeNode<Node> {
			#region data

			private RWLock nodeLock = null;

			private readonly NodeAnnotation annotation = new NodeAnnotation();

			#endregion


			#region properties

			public Annotation Annotation {
				get {
					return this.annotation;
				}
			}

			public bool Concurrent {
				get {
					return !this.nodeLock.IsDummy;
				}
			}

			#endregion


			#region creation and destruction

			protected Node() {
			}


			private void InitializeThisClassLevel(bool concurrent) {
				// initialize an instance
				Debug.Assert(this.nodeLock == null);

				this.nodeLock = concurrent? new RWLock(): RWLock.Dummy;
			}

			protected void Initialize(bool concurrent) {
				// initialize this instance
				base.Initialize();
				InitializeThisClassLevel(concurrent);
			}

			protected new void Initialize(Node parent, string name) {
				// initialize this instance
				base.Initialize(parent, name);
				Debug.Assert(parent != null);
				InitializeThisClassLevel(parent.Concurrent);
			}

			protected new void Initialize(Node parent, int index) {
				// initialize this instance
				Initialize(parent, index);
				Debug.Assert(parent != null);
				InitializeThisClassLevel(parent.Concurrent);
			}

			protected new void Clear() {
				// clear members
				this.annotation.Clear();
				Util.ClearDisposable(ref this.nodeLock);
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
