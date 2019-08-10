using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace PandocUtil.PandocFilter.Filters {
	public class Filter {
		#region types

		protected class Context<ActualContext>: WorkingTreeNode<ActualContext> where ActualContext: WorkingTreeNodeBase {
			#region data

			private object value = null;

			private Dictionary<string, object> objValue = null;

			private List<object> arrayValue = null;

			#endregion


			#region properties

			public object Value {
				get {
					return this.value;
				}
				protected set {
					SetValue(value);
				}
			}

			protected Dictionary<string, object> ObjectValue {
				get {
					Dictionary<string, object> value = this.objValue;
					if (value == null) {
						throw new InvalidOperationException("The value is not a JSON object.");
					}
					return value;
				}
			}

			protected List<object> ArrayValue {
				get {
					List<object> value = this.arrayValue;
					if (value == null) {
						throw new InvalidOperationException("The value is not a JSON array.");
					}
					return value;
				}
			}

			public bool IsObject {
				get {
					return this.objValue != null;
				}
			}

			public bool IsArray {
				get {
					return this.arrayValue != null;
				}
			}

			#endregion


			#region creation and destruction

			protected Context() {
			}


			protected void Initialize(object value) {
				// argument checks
				// value can be null

				// initialize this instance
				base.Initialize();
				SetValue(value);
			}

			protected new void Initialize(ActualContext parent) {
				// argument checks
				if (parent == null) {
					throw new ArgumentOutOfRangeException(nameof(parent));
				}

				// initialize this instance
				base.Initialize(parent);
				SetValue(null);
			}

			protected void Initialize(ActualContext parent, string name, object value) {
				// argument checks
				if (parent == null) {
					throw new ArgumentOutOfRangeException(nameof(parent));
				}
				if (name == null) {
					throw new ArgumentOutOfRangeException(nameof(name));
				}
				// value can be null

				// initialize this instance
				base.Initialize(parent, name);
				SetValue(value);
			}

			protected void Initialize(ActualContext parent, int index, object value) {
				// argument checks
				if (parent == null) {
					throw new ArgumentOutOfRangeException(nameof(parent));
				}
				if (index < 0) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null

				// initialize this instance
				base.Initialize(parent, index);
				SetValue(value);
			}

			protected new void Clear() {
				// clear this instance
				SetValue(null);
				base.Clear();
			}

			private void SetValue(object value) {
				this.value = value;
				switch (value) {
					case Dictionary<string, object> obj:
						// value is an object
						this.objValue = obj;
						this.arrayValue = null;
						break;
					case List<object> array:
						// value is an array
						this.objValue = null;
						this.arrayValue = array;
						break;
					default:
						this.objValue = null;
						this.arrayValue = null;
						break;
				}
			}

			#endregion
		}

		protected class ModifyingContext: Context<ModifyingContext> {
			#region types

			public class ArrayEditor: IEnumerable<object> {
				#region data

				private static readonly object Removed = new object();


				private LinkedList<object> precedentSlot = null;

				private readonly object[] array = null;

				#endregion


				#region properties

				public LinkedList<object> this[int index] {
					get {
						// argument checks
						if (index < -1 || this.array.Length <= index) {
							throw new ArgumentOutOfRangeException(nameof(index));
						}

						return (index == -1) ? this.precedentSlot : GetExtendedSlot(index);
					}
				}

				#endregion


				#region constructors

				public ArrayEditor(IReadOnlyList<object> original) {
					// argument checks
					if (original == null) {
						throw new ArgumentNullException(nameof(original));
					}

					// initialize members
					this.array = original.ToArray();
				}

				#endregion


				#region IEnumerator

				IEnumerator IEnumerable.GetEnumerator() {
					return GetEnumerator();
				}

				#endregion


				#region IEnumerator<object>

				public IEnumerator<object> GetEnumerator() {
					// precedent slot
					if (this.precedentSlot != null) {
						foreach (object item in this.precedentSlot) {
							yield return item;
						}
					}

					// slots in the array
					foreach (object slot in array) {
						switch (slot) {
							case LinkedList<object> extendedSlot:
								// the slot is extended
								foreach (object item in extendedSlot) {
									if (item != Removed) {
										yield return item;
									}
								}
								break;
							default:
								// the slot is not extended
								if (slot != Removed) {
									yield return slot;
								}
								break;
						}
					}
				}

				#endregion


				#region methods

				public IList<object> ToList() {
					return new List<object>(this);
				}

				public object Get(int index) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// replace the value of the slot
					object value = this.array[index];
					switch (value) {
						case LinkedList<object> extendedSlot:
							// this slot has been extended
							return extendedSlot.First.Value;
						default:
							return value;
					}
				}

				public void Set(int index, object value) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// replace the value of the slot
					switch (this.array[index]) {
						case LinkedList<object> extendedSlot:
							// this slot has been extended
							extendedSlot.First.Value = value;
							break;
						default:
							// this slot is not extended yet
							this.array[index] = value;
							break;
					}
				}

				public void Remove(int index) {
					// mark as Removed
					Set(index, Removed);
				}

				public void InsertBefore(int index, object value) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// extend the slot for the previous index
					LinkedList<object> extendedSlot = GetExtendedSlot(index - 1);
					// append the object into the extended slot
					extendedSlot.AddLast(value);
				}

				public void InsertAfter(int index, object value) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// extend the slot for the index
					LinkedList<object> extendedRoom = GetExtendedSlot(index);
					// insert the object into the extended slot
					extendedRoom.AddAfter(extendedRoom.First, value);
				}

				private LinkedList<object> GetExtendedSlot(int index) {
					// argument checks
					Debug.Assert(-1 <= index && index < this.array.Length);

					LinkedList<object> extendedSlot;
					if (index == -1) {
						// precedence of the array
						extendedSlot = this.precedentSlot;
					} else {
						// inside the array
						object value = this.array[index];
						extendedSlot = value as LinkedList<object>;
						if (extendedSlot == null) {
							// this slot is not extended yet
							// extend the slot
							extendedSlot = new LinkedList<object>();
							extendedSlot.AddFirst(value);
							this.array[index] = extendedSlot;
						}
					}

					return extendedSlot;
				}

				#endregion
			}

			#endregion


			#region data

			private static object classLocker = new object();

			private static Stack<ModifyingContext> instanceCache = new Stack<ModifyingContext>();


			public ModifyingContext Root { get; private set; } = null;

			public ArrayEditor arrayEditor = null;

			private Dictionary<string, object> annotation = null;

			#endregion


			#region properties

			public new IDictionary<string, object> ObjectValue {
				get {
					return base.ObjectValue;
				}
			}

			// This property returns a read only interface to the array.
			// To get an editable interface to the array,
			// reference the ArrayEditor.
			public new IReadOnlyList<object> ArrayValue {
				get {
					return base.ArrayValue;
				}
			}

			public IDictionary<string, object> AST {
				get {
					return this.Root.ObjectValue;
				}
			}

			public IDictionary<string, object> Meta {
				get {
					IDictionary<string, object> ast = this.AST;
					Debug.Assert(ast != null);
					return ast.GetOptionalValue<IDictionary<string, object>>(Schema.Names.Meta, null);
				}
			}

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

			private ModifyingContext(): base() {
			}


			private void InitializeThisClassLevel(ModifyingContext root) {
				// argument checks
				Debug.Assert(root != null);

				// initialize this class level
				this.Root = root;
				Debug.Assert(this.arrayEditor == null);
				Debug.Assert(this.annotation == null || this.annotation.Count == 0);
			}

			private void Initialize(Dictionary<string, object> ast) {
				// argument checks
				if (ast == null) {
					throw new ArgumentNullException(nameof(ast));
				}

				// initialize this instance
				base.Initialize(ast);
				InitializeThisClassLevel(this);
			}

			private new void Initialize(ModifyingContext parent) {
				// argument checks
				if (parent == null) {
					throw new ArgumentNullException(nameof(parent));
				}

				// initialize this instance
				base.Initialize(parent);
				InitializeThisClassLevel(parent.Root);
			}

			private new void Initialize(ModifyingContext parent, string name, object value) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// value can be null

				// initialize this instance
				base.Initialize(parent, name, value);
				InitializeThisClassLevel(parent.Root);
			}

			private new void Initialize(ModifyingContext parent, int index, object value) {
				// argument checks
				if (index < 0) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null

				// initialize this instance
				base.Initialize(parent, index, value);
				InitializeThisClassLevel(parent.Root);
			}

			private new void Clear() {
				// clear members
				if (this.annotation != null) {
					this.annotation.Clear();
				}
				this.arrayEditor = null;
				this.Root = null;
				base.Clear();
			}


			private static ModifyingContext CreateContext() {
				ModifyingContext instance;

				// try to get an instance from the cache
				lock (classLocker) {
					instanceCache.TryPop(out instance);
				}

				// create an instance if cache is empty
				if (instance == null) {
					instance = new ModifyingContext();
				}

				return instance;
			}

			private static ModifyingContext CreateContext(Dictionary<string, object> ast) {
				// create and setup an instance
				ModifyingContext instance = CreateContext();
				instance.Initialize(ast);
				return instance;
			}

			private ModifyingContext CreateChildContext() {
				// create and setup a child context
				ModifyingContext childContext = CreateContext();
				childContext.Initialize(this);
				return childContext;
			}

			private ModifyingContext CreateChildContext(string childName, object childValue) {
				// create and setup a child context
				ModifyingContext childContext = CreateContext();
				childContext.Initialize(this, childName, childValue);
				return childContext;
			}

			private ModifyingContext CreateChildContext(int childIndex, object childValue) {
				// create and setup a child context
				ModifyingContext childContext = CreateContext();
				childContext.Initialize(this, childIndex, childValue);
				return childContext;
			}

			private static void ReleaseContext(ModifyingContext instance) {
				// argument checks
				if (instance == null) {
					return;
				}

				// if array is edited, replace the array value
				if (instance.arrayEditor != null) {
					instance.ReplaceValue(instance.arrayEditor.ToList());
				}

				// clear the instance
				instance.Clear();

				// cache the instance
				lock (classLocker) {
					instanceCache.Push(instance);
				}

				return;
			}

			#endregion


			#region methods - context operations

			public static void RunInContext(Dictionary<string, object> ast, Action<ModifyingContext> action) {
				// argument checks
				if (ast == null) {
					throw new ArgumentNullException(nameof(ast));
				}
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ModifyingContext context = CreateContext(ast);
				try {
					action(context);
				} finally {
					ReleaseContext(context);
				}
			}

			public void RunInChildContext(string childName, object childValue, Action<ModifyingContext> action) {
				// argument checks
				if (childName == null) {
					throw new ArgumentNullException(nameof(childName));
				}
				// childValue can be null
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ModifyingContext childContext = CreateChildContext(childName, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseContext(childContext);
				}
			}

			public void RunInChildContext(int childIndex, object childValue, Action<ModifyingContext> action) {
				// argument checks
				if (childIndex < 0) {
					throw new ArgumentOutOfRangeException(nameof(childIndex));
				}
				// childValue can be null
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ModifyingContext childContext = CreateChildContext(childIndex, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseContext(childContext);
				}
			}

			public void RunInEachChildContext(IReadOnlyList<object> array, Action<ModifyingContext> action) {
				// argument checks
				if (array == null) {
					return;
				}
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ModifyingContext context = CreateChildContext();
				try {
					for (int i = 0; i < array.Count; ++i) {
						context.Index = i;
						context.Value = array[i];
						action(context);
					}
				} finally {
					ReleaseContext(context);
				}
			}

			#endregion


			#region methods - modifying

			public ArrayEditor GetArrayEditor() {
				ArrayEditor value = this.arrayEditor;
				if (value == null) {
					value = new ArrayEditor(this.ArrayValue);
					this.arrayEditor = value;
				}

				return value;
			}

			public void ReplaceChild(string name, object value) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty
				// value can be null;

				// state checks
				Debug.Assert(this.IsObject);

				// replace the value of the child
				this.ObjectValue[name] = value;
			}

			public void ReplaceChild(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// state checks
				Debug.Assert(this.IsArray);

				// replace the value of the child
				ArrayEditor arrayEditor = GetArrayEditor();
				arrayEditor.Set(index, value);
			}

			public void RemoveChild(string name) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty

				// state checks
				Debug.Assert(this.IsObject);

				// remove the child
				this.ObjectValue.Remove(name);
			}

			public void RemoveChild(int index) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				// state checks
				Debug.Assert(this.IsArray);

				// remove the child
				ArrayEditor arrayEditor = GetArrayEditor();
				arrayEditor.Remove(index);
			}

			public void AddChild(string name, object value) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty
				// value can be null;

				// state checks
				Debug.Assert(this.IsObject);

				// add the child
				this.ObjectValue[name] = value;
			}

			public void InsertChildBefore(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// state checks
				Debug.Assert(this.IsArray);

				// insert the value before the child
				ArrayEditor arrayEditor = GetArrayEditor();
				arrayEditor.InsertBefore(index, value);
			}

			public void InsertChildAfter(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// state checks
				Debug.Assert(this.IsArray);

				// insert the value after the value
				ArrayEditor arrayEditor = GetArrayEditor();
				arrayEditor.InsertAfter(index, value);
			}


			public void ReplaceValue(object value) {
				ModifyingContext parent = this.Parent;
				if (this.IsParentObject) {
					Debug.Assert(parent != null);
					parent.ReplaceChild(this.Name, value);
				} else if (this.IsParentArray) {
					Debug.Assert(parent != null);
					parent.ReplaceChild(this.Index, value);
				} else {
					throw new InvalidOperationException("The value is not contained by a JSON object or an array.");
				}
			}

			public void RemoveValue() {
				ModifyingContext parent = this.Parent;
				if (this.IsParentObject) {
					Debug.Assert(parent != null);
					parent.RemoveChild(this.Name);
				} else if (this.IsParentArray) {
					Debug.Assert(parent != null);
					parent.RemoveChild(this.Index);
				} else {
					throw new InvalidOperationException("The value is not contained by a JSON object or an array.");
				}
			}

			public void AddSibling(string name, object value) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty
				// value can be null

				// state checks
				if (this.IsParentObject == false) {
					throw new InvalidOperationException("The value of the parent must be a JSON object.");
				}

				this.Parent.AddChild(name, value);
			}

			public void InsertSiblingBefore(object value) {
				// argument checks
				// value can be null

				// state checks
				if (this.IsParentArray == false) {
					throw new InvalidOperationException("The parent must be an array.");
				}

				this.Parent.InsertChildBefore(this.Index, value);
			}

			public void InsertSiblingAfter(object value) {
				// argument checks
				// value can be null

				// state checks
				if (this.IsParentArray == false) {
					throw new InvalidOperationException("The value of the parent must be an array.");
				}

				this.Parent.InsertChildAfter(this.Index, value);
			}

			#endregion
		}

		#endregion


		#region creation

		protected Filter() {
		}

		#endregion


		#region methods

		public void Modify(Dictionary<string, object> ast) {
			// argument checks
			if (ast == null) {
				throw new ArgumentNullException(nameof(ast));
			}

			// single thread mode
			ModifyingContext.RunInContext(ast, (rootContext) => {
				foreach (KeyValuePair<string, object> item in ast) {
					switch (item.Key) {
						case Schema.Names.Blocks:
							rootContext.RunInChildContext(item.Key, item.Value, ModifyValue);
							break;
						// TODO: meta
					}
				}
			});
		}

		public IDictionary<string, object> Generate(IDictionary<string, object> ast, bool concurrent = true) {
			// argument checks
			if (ast == null) {
				throw new ArgumentNullException(nameof(ast));
			}

			// TODO: implement
			throw new NotImplementedException();
		}

		#endregion


		#region overridables - modify

		protected virtual void ModifyValue(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			// modify depending on type of the value
			if (context.IsObject) {
				// value is an object
				ModifyObject(context);
			} else if (context.IsArray) {
				ModifyArray(context);
			}

			return;
		}

		protected virtual void ModifyArray(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			Debug.Assert(context.IsArray);

			// modify each element
			IReadOnlyList<object> array = context.ArrayValue;
			context.RunInEachChildContext(array, ModifyValue);
		}

		protected virtual void ModifyObject(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			Debug.Assert(context.IsObject);

			// modify only elements, by default
			IDictionary<string, object> obj = context.ObjectValue;
			(bool _, string type) = obj.GetOptionalValue<string>(Schema.Names.T);
			(bool _, object content) = obj.GetOptionalValue<object>(Schema.Names.C);
			if (!string.IsNullOrEmpty(type)) {
				// value is an element
				// Note that content may be null.
				ModifyElement(context, type, content);
			}
		}

		protected virtual void ModifyElement(ModifyingContext context, string type, object contents) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (string.IsNullOrEmpty(type)) {
				throw new ArgumentNullException(nameof(type));
			}
			// contents can be null

			// modify contents
			if (contents != null) {
				context.RunInChildContext(Schema.Names.C, contents, ModifyValue);
			}
		}

		#endregion


		#region overridables - generate
		#endregion
	}
}
