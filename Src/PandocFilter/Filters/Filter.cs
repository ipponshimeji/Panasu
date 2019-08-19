using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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


			private void InitializeThisClassLevel(object value) {
				// argument checks
				// value can be null

				// initialize this class level
				SetValue(value);

				return;
			}

			protected void Initialize(object value) {
				// argument checks
				// value can be null

				// initialize this instance
				base.Initialize();
				InitializeThisClassLevel(value);

				return;
			}

			protected new void Initialize(ActualContext parent) {
				// argument checks

				// initialize this instance
				base.Initialize(parent);
				InitializeThisClassLevel(null);

				return;
			}

			protected void Initialize(ActualContext parent, string name, object value) {
				// argument checks
				// value can be null

				// initialize this instance
				base.Initialize(parent, name);
				InitializeThisClassLevel(value);

				return;
			}

			protected void Initialize(ActualContext parent, int index, object value) {
				// argument checks
				// value can be null

				// initialize this instance
				base.Initialize(parent, index);
				InitializeThisClassLevel(value);

				return;
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

			private class InstanceCache: InstanceCache<ModifyingContext> {
				#region creation and disposal

				public InstanceCache(): base(nameof(ModifyingContext)) {
				}

				#endregion


				#region overrides

				protected override ModifyingContext CreateInstance() {
					return new ModifyingContext();
				}

				#endregion
			}

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

						return GetExtendedSlot(index);
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

					yield break;
				}

				#endregion


				#region methods

				public List<object> ToList() {
					return new List<object>(this);
				}

				public object Get(int index) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// replace the value of the slot
					// Note that the value may be the 'Removed' object.
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

					return;
				}

				public void Remove(int index) {
					// mark as Removed
					// The items which come from the original array are treated as "landmark",
					// so it should not be removed but keep the location and are marked as 'Removed'.
					Set(index, Removed);
				}

				public void InsertBefore(int index, object value) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// extend the slot for the previous index
					LinkedList<object> extendedSlot = GetExtendedSlot(index - 1);
					// append the object at the last of the extended slot
					extendedSlot.AddLast(value);

					return;
				}

				public void InsertAfter(int index, object value) {
					// argument checks
					if (index < 0 || this.array.Length <= index) {
						throw new ArgumentOutOfRangeException(nameof(index));
					}

					// extend the slot for the index
					LinkedList<object> extendedSlot = GetExtendedSlot(index);
					// insert the object at the next of the item
					extendedSlot.AddAfter(extendedSlot.First, value);

					return;
				}

				private LinkedList<object> GetExtendedSlot(int index) {
					// argument checks
					Debug.Assert(-1 <= index && index < this.array.Length);

					LinkedList<object> extendedSlot;
					if (index == -1) {
						// precedence of the array
						extendedSlot = this.precedentSlot;
						if (extendedSlot == null) {
							extendedSlot = new LinkedList<object>();
							this.precedentSlot = extendedSlot;
						}
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

			private static readonly InstanceCache instanceCache = new InstanceCache();


			public ModifyingContext Root { get; private set; } = null;

			public AST AST { get; private set; } = null;

			public ArrayEditor arrayEditor = null;

			#endregion


			#region properties

			public new IDictionary<string, object> ObjectValue {
				get {
					return base.ObjectValue;
				}
			}

			// This property returns a read only interface to the array.
			// An editable interface to the array is provided by ArrayEditor.
			public new IReadOnlyList<object> ArrayValue {
				get {
					return base.ArrayValue;
				}
			}

			#endregion


			#region creation and destruction

			private ModifyingContext(): base() {
			}


			private void InitializeThisClassLevel(ModifyingContext root, AST ast) {
				// argument checks
				Debug.Assert(root != null);
				Debug.Assert(ast != null);

				// initialize this class level
				this.Root = root;
				this.AST = ast;
				Debug.Assert(this.arrayEditor == null);
			}

			private void Initialize(AST ast) {
				// argument checks
				if (ast == null) {
					throw new ArgumentNullException(nameof(ast));
				}

				// initialize this instance
				base.Initialize(ast.JsonValue);
				InitializeThisClassLevel(this, ast);

				return;
			}

			private new void Initialize(ModifyingContext parent) {
				// argument checks

				// initialize this instance
				base.Initialize(parent);
				Debug.Assert(parent != null);	// checked by the base class
				InitializeThisClassLevel(parent.Root, parent.AST);

				return;
			}

			private new void Initialize(ModifyingContext parent, string name, object value) {
				// argument checks

				// initialize this instance
				base.Initialize(parent, name, value);
				Debug.Assert(parent != null);   // checked by the base class
				InitializeThisClassLevel(parent.Root, parent.AST);

				return;
			}

			private new void Initialize(ModifyingContext parent, int index, object value) {
				// argument checks

				// initialize this instance
				base.Initialize(parent, index, value);
				Debug.Assert(parent != null);   // checked by the base class
				InitializeThisClassLevel(parent.Root, parent.AST);

				return;
			}

			private new void Clear() {
				// clear this instance
				this.arrayEditor = null;
				this.AST = null;
				this.Root = null;
				base.Clear();
			}


			private static ModifyingContext CreateContext(AST ast) {
				// create and setup an instance
				ModifyingContext instance = instanceCache.AllocInstance();
				instance.Initialize(ast);

				return instance;
			}

			private ModifyingContext CreateChildContext() {
				// create and setup a child context
				ModifyingContext childContext = instanceCache.AllocInstance();
				childContext.Initialize(this);

				return childContext;
			}

			private ModifyingContext CreateChildContext(string childName, object childValue) {
				// create and setup a child context
				ModifyingContext childContext = instanceCache.AllocInstance();
				childContext.Initialize(this, childName, childValue);

				return childContext;
			}

			private ModifyingContext CreateChildContext(int childIndex, object childValue) {
				// create and setup a child context
				ModifyingContext childContext = instanceCache.AllocInstance();
				childContext.Initialize(this, childIndex, childValue);

				return childContext;
			}

			private static void ReleaseContext(ModifyingContext instance) {
				// argument checks
				if (instance == null) {
					throw new ArgumentNullException(nameof(instance));
				}

				// if array is edited, replace the array value
				if (instance.arrayEditor != null) {
					instance.ReplaceValue(instance.arrayEditor.ToList());
				}

				// clear the instance
				instance.Clear();

				// cache the instance
				instanceCache.ReleaseInstance(instance);

				return;
			}

			#endregion


			#region methods - context operations

			public static void RunInContext(AST ast, Action<ModifyingContext> action) {
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

				return;
			}

			public void RunInChildContext(string childName, object childValue, Action<ModifyingContext> action) {
				// argument checks
				if (childName == null) {
					throw new ArgumentNullException(nameof(childName));
				}
				// childName can be empty
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

				return;
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

			public void RunInEachChildContext(IDictionary<string, object> obj, Action<ModifyingContext> action) {
				// argument checks
				if (obj == null) {
					return;
				}
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ModifyingContext context = CreateChildContext();
				try {
					string[] keys = obj.Keys.ToArray();	// fix the current keys
					foreach (string key in keys) {
						context.Name = key;
						context.Value = obj[key];
						action(context);
					}
				} finally {
					ReleaseContext(context);
				}
			}

			#endregion


			#region methods - modifying

			public ArrayEditor GetArrayEditor() {
				// state checks
				if (this.IsArray == false) {
					throw new InvalidOperationException("The value is not JSON array.");
				}

				// get the ArrayEditor
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

				// replace the value of the child
				this.ObjectValue[name] = value;
			}

			public void ReplaceChild(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// replace the value of the child
				GetArrayEditor().Set(index, value);
			}

			public void RemoveChild(string name) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty

				// remove the child
				this.ObjectValue.Remove(name);
			}

			public void RemoveChild(int index) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}

				// remove the child
				GetArrayEditor().Remove(index);
			}

			public void AddChild(string name, object value) {
				// argument checks
				if (name == null) {
					throw new ArgumentNullException(nameof(name));
				}
				// name can be empty
				// value can be null;

				// add the child
				this.ObjectValue[name] = value;
			}

			public void InsertChildBefore(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// insert the value before the child
				GetArrayEditor().InsertBefore(index, value);
			}

			public void InsertChildAfter(int index, object value) {
				// argument checks
				if (index < 0 || this.ArrayValue.Count <= index) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				// value can be null;

				// insert the value after the value
				GetArrayEditor().InsertAfter(index, value);
			}


			public void ReplaceValue(object value) {
				if (this.IsParentObject) {
					Debug.Assert(this.Parent != null);
					this.Parent.ReplaceChild(this.Name, value);
				} else if (this.IsParentArray) {
					Debug.Assert(this.Parent != null);
					this.Parent.ReplaceChild(this.Index, value);
				} else {
					throw new InvalidOperationException("The value is not contained by a JSON object or an array.");
				}
			}

			public void RemoveValue() {
				if (this.IsParentObject) {
					Debug.Assert(this.Parent != null);
					this.Parent.RemoveChild(this.Name);
				} else if (this.IsParentArray) {
					Debug.Assert(this.Parent != null);
					this.Parent.RemoveChild(this.Index);
				} else {
					throw new InvalidOperationException("The value is not contained by a JSON object or an array.");
				}
			}

			public void AddSibling(string name, object value) {
				// argument checks

				// state checks
				if (this.IsParentObject == false) {
					throw new InvalidOperationException("The value of the parent must be a JSON object.");
				}

				this.Parent.AddChild(name, value);
			}

			public void InsertSiblingBefore(object value) {
				// argument checks

				// state checks
				if (this.IsParentArray == false) {
					throw new InvalidOperationException("The parent must be an array.");
				}

				this.Parent.InsertChildBefore(this.Index, value);
			}

			public void InsertSiblingAfter(object value) {
				// argument checks

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

		public void Modify(Dictionary<string, object> astJsonValue) {
			// argument checks
			if (astJsonValue == null) {
				throw new ArgumentNullException(nameof(astJsonValue));
			}

			// modify the ast
			AST ast = new AST(astJsonValue);

			// modify the ast
			ModifyingContext.RunInContext(ast, (rootContext) => {
				object value;

				// Metadata
				if (astJsonValue.TryGetValue(Schema.Names.Meta, out value)) {
					rootContext.RunInChildContext(Schema.Names.Meta, value, ModifyMetadata);
				}

				// Blocks
				if (astJsonValue.TryGetValue(Schema.Names.Blocks, out value)) {
					rootContext.RunInChildContext(Schema.Names.Blocks, value, ModifyValue);
				}
			});

			return;
		}

		public IDictionary<string, object> Generate(IDictionary<string, object> ast) {
			// argument checks
			if (ast == null) {
				throw new ArgumentNullException(nameof(ast));
			}

			// TODO: implement
			throw new NotImplementedException();
		}

		#endregion


		#region overridables - modify

		protected virtual void ModifyMetadata(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			context.RunInEachChildContext(context.ObjectValue, ModifyValue);
		}

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
			context.RunInEachChildContext(context.ArrayValue, ModifyValue);
		}

		protected virtual void ModifyObject(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			Debug.Assert(context.IsObject);

			// modify only elements, by default
			(string type, object contents) = Schema.IsElement(context.ObjectValue);
			if (type != null) {
				// value is an element
				// Note that content may be null.
				ModifyElement(context, type, contents);
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

			// process the element
			switch (type) {
				case Schema.TypeNames.MetaMap:
					(string name, IReadOnlyDictionary<string, object> macro) = Schema.IsMacro(contents);
					if (name != null) {
						ModifyMacro(context, name, macro);
						return;
					}
					break;
			}

			// modify contents
			if (contents != null) {
				context.RunInChildContext(Schema.Names.C, contents, ModifyValue);
			}
		}

		protected virtual void ModifyMacro(ModifyingContext context, string name, IReadOnlyDictionary<string, object> macro) {
			throw new ApplicationException($"Undefined macro: {name}");
		}

		#endregion


		#region overridables - generate
		#endregion
	}
}
