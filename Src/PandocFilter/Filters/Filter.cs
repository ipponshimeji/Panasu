using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;

namespace PandocUtil.PandocFilter.Filters {
	public class Filter {
		#region types

		protected class Parameters: PandocUtil.PandocFilter.Filters.Parameters {
			#region creation

			public Parameters(Dictionary<string, object> dictionary, bool ast): base(dictionary, ast) {
			}

			#endregion
		}

		protected class ContextBase: WorkingTreeNode {
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

			protected ContextBase() {
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

			protected void Initialize(ContextBase parent) {
				// argument checks

				// initialize this instance
				base.Initialize(parent);
				InitializeThisClassLevel(null);

				return;
			}

			protected void Initialize(ContextBase parent, string name, object value) {
				// argument checks
				// value can be null

				// initialize this instance
				base.Initialize(parent, name);
				InitializeThisClassLevel(value);

				return;
			}

			protected void Initialize(ContextBase parent, int index, object value) {
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


			#region methods

			public FormatException CreateFormatException(string message) {
				return new FormatException($"{message}{Environment.NewLine}Location: {GetLocation()}");
			}

			public (bool, T) GetOptionalValue<T>(string key) {
				return ((IReadOnlyDictionary<string, object>)this.ObjectValue).GetOptionalValue<T>(key);
			}

			public T GetOptionalValue<T>(string key, T defaultValue) {
				return ((IReadOnlyDictionary<string, object>)this.ObjectValue).GetOptionalValue<T>(key, defaultValue);
			}

			public T GetIndispensableValue<T>(string key) {
				try {
					return ((IReadOnlyDictionary<string, object>)this.ObjectValue).GetIndispensableValue<T>(key);
				} catch (KeyNotFoundException) {
					string message = $"The indispensable key '{key}' is missing in the JSON object.";
					throw CreateFormatException(message);
				}
			}

			public (string type, object contents) IsElement() {
				return Schema.IsElement((IReadOnlyDictionary<string, object>)this.ObjectValue);
			}

			public (string macro, object contents) IsMacro() {
				return Schema.IsValueMacro(this.Value);
			}

			public virtual Dictionary<string, object> GetEditingBaseObject() {
				return new Dictionary<string, object>();
			}

			public virtual List<object> GetEditingBaseArray() {
				return new List<object>();
			}

			#endregion


			#region methods - metadata

			public string GetMetadataStringValue(string key, string formatName = null) {
				object value;
				if (this.ObjectValue.TryGetValue(key, out value)) {
					return Schema.GetMetadataStringValue(value, formatName);
				} else {
					return null;
				}
			}

			#endregion


			#region methods - adapter for subclasses

			protected static void SetName(ContextBase context, string name) {
				context.Name = name;
			}

			protected static void SetIndex(ContextBase context, int index) {
				context.Index = index;
			}

			protected static void SetValue(ContextBase context, object value) {
				context.Value = value;
			}

			#endregion
		}

		protected abstract class Context<ActualContext>: ContextBase where ActualContext: ContextBase {
			#region properties

			public new ActualContext Parent {
				get {
					return (ActualContext)base.Parent;
				}
			}

			#endregion


			#region creation and destruction

			protected Context() {
			}


			protected abstract ActualContext CreateChildContext();

			protected abstract ActualContext CreateChildContext(string childName, object childValue);

			protected abstract ActualContext CreateChildContext(int childIndex, object childValue);

			protected abstract void ReleaseChildContext(ActualContext childContext);

			#endregion


			#region methods - context operations

			public void RunInChildContext(string childName, object childValue, Action<ActualContext> action) {
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
				ActualContext childContext = CreateChildContext(childName, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseChildContext(childContext);
				}
			}

			public object RunInChildContext(string childName, object childValue, Func<ActualContext, object> func) {
				// argument checks
				if (childName == null) {
					throw new ArgumentNullException(nameof(childName));
				}
				// childName can be empty
				// childValue can be null
				if (func == null) {
					throw new ArgumentNullException(nameof(func));
				}

				// run the action in the context
				ActualContext childContext = CreateChildContext(childName, childValue);
				try {
					return func(childContext);
				} finally {
					ReleaseChildContext(childContext);
				}
			}

			public void RunInChildContext(int childIndex, object childValue, Action<ActualContext> action) {
				// argument checks
				if (childIndex < 0) {
					throw new ArgumentOutOfRangeException(nameof(childIndex));
				}
				// childValue can be null
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				ActualContext childContext = CreateChildContext(childIndex, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseChildContext(childContext);
				}
			}

			public object RunInChildContext(int childIndex, object childValue, Func<ActualContext, object> func) {
				// argument checks
				if (childIndex < 0) {
					throw new ArgumentOutOfRangeException(nameof(childIndex));
				}
				// childValue can be null
				if (func == null) {
					throw new ArgumentNullException(nameof(func));
				}

				// run the action in the context
				ActualContext childContext = CreateChildContext(childIndex, childValue);
				try {
					return func(childContext);
				} finally {
					ReleaseChildContext(childContext);
				}
			}

			public void RunInEachChildContext(Action<ActualContext> action) {
				// argument checks
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				if (this.IsArray) {
					List<object> array = this.ArrayValue;

					// run the action in the context
					ActualContext childContext = CreateChildContext();
					try {
						for (int i = 0; i < array.Count; ++i) {
							SetIndex(childContext, i);
							SetValue(childContext, array[i]);
							action(childContext);
						}
					} finally {
						ReleaseChildContext(childContext);
					}
				} else if (this.IsObject) {
					Dictionary<string, object> obj = this.ObjectValue;

					// run the action in the context
					ActualContext childContext = CreateChildContext();
					try {
						// take snapshot of the current key
						// Note that the contents of the obj may modified during the iteration.
						string[] keys = obj.Keys.ToArray();
						foreach (string key in keys) {
							SetName(childContext, key);
							SetValue(childContext, obj[key]);
							action(childContext);
						}
					} finally {
						ReleaseChildContext(childContext);
					}
				} else {
					throw new InvalidOperationException("The value is neither a JSON array nor a JSON object.");
				}
			}

			#endregion


			#region methods

			public abstract TParameters GetParameters<TParameters>() where TParameters : Parameters;

			#endregion
		}

		protected class ModifyingContext: Context<ModifyingContext> {
			#region types

			private class InstanceCache: InstanceCache<ModifyingContext> {
				#region creation and disposal

				public InstanceCache() : base(nameof(ModifyingContext)) {
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

			public new Dictionary<string, object> ObjectValue {
				get {
					return base.ObjectValue;
				}
			}

			// This property returns a read only interface to the original array.
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

			private void Initialize(ModifyingContext parent) {
				// argument checks

				// initialize this instance
				base.Initialize(parent);
				Debug.Assert(parent != null);	// checked by the base class
				InitializeThisClassLevel(parent.Root, parent.AST);

				return;
			}

			private void Initialize(ModifyingContext parent, string name, object value) {
				// argument checks

				// initialize this instance
				base.Initialize(parent, name, value);
				Debug.Assert(parent != null);   // checked by the base class
				InitializeThisClassLevel(parent.Root, parent.AST);

				return;
			}

			private void Initialize(ModifyingContext parent, int index, object value) {
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

			protected override ModifyingContext CreateChildContext() {
				// create and setup a child context
				ModifyingContext childContext = instanceCache.AllocInstance();
				childContext.Initialize(this);

				return childContext;
			}

			protected override ModifyingContext CreateChildContext(string childName, object childValue) {
				// create and setup a child context
				ModifyingContext childContext = instanceCache.AllocInstance();
				childContext.Initialize(this, childName, childValue);

				return childContext;
			}

			protected override ModifyingContext CreateChildContext(int childIndex, object childValue) {
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

			protected override void ReleaseChildContext(ModifyingContext childContext) {
				ReleaseContext(childContext);
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


			#region overrides

			public override Dictionary<string, object> GetEditingBaseObject() {
				// use the current object as a base in modifying scenario
				return this.ObjectValue;
			}

			public override List<object> GetEditingBaseArray() {
				// use the current array as a base in modifying scenario
				return base.ArrayValue;
			}

			public override TParameters GetParameters<TParameters>() {
				return (TParameters)this.AST.Parameters;
			}

			#endregion
		}

		protected static class StandardMacros {
			#region types

			public static class Names {
				public const string Rebase = "rebase";
				public const string Condition = "condition";
			}

			#endregion


			#region constants

			public const string MacroFormatName = "macro";

			#endregion


			#region methods

			public static object Rebase<ActualContext>(ActualContext context, Func<ActualContext, string, object> expander, Uri oldBaseUri, Uri newBaseUri) where ActualContext: Context<ActualContext> {
				// argument checks
				if (context == null) {
					throw new ArgumentNullException(nameof(context));
				}
				if (expander == null) {
					throw new ArgumentNullException(nameof(expander));
				}

				// expand 'target' value
				object target = expander(context, "target");
				if (target == null) {
					return null;
				}
				string oldLink = Schema.GetMetadataStringValue(target, MacroFormatName);

				// create a MetaInlines element to be replaced
				return Schema.CreateSimpleMetaInlinesElement(Util.RebaseRelativeUri(oldBaseUri, oldLink, newBaseUri));
			}

			public static object Condition<ActualContext>(ActualContext context, Func<ActualContext, string, object> expander, string fromFileRelPath) where ActualContext: Context<ActualContext> {
				// argument checks
				if (context == null) {
					throw new ArgumentNullException(nameof(context));
				}
				if (expander == null) {
					throw new ArgumentNullException(nameof(expander));
				}

				// local methods
				object getTrueCaseValue() {
					return expander(context, "true-case");
				}

				object getFalseCaseValue() {
					return expander(context, "false-case");
				}

				bool doesValueMeetCondition(IReadOnlyDictionary<string, object> value, Predicate<string> predicate) {
					// value can be null
					Debug.Assert(predicate != null);

					(string type, object contents) = Schema.IsElement(value);
					switch (type) {
						case Schema.TypeNames.MetaInlines:
						case Schema.TypeNames.MetaBlocks:
							string strValue = Schema.GetMetadataStringValue((IReadOnlyList<object>)contents);
							if (predicate(strValue)) {
								return true;
							}
							break;
						case Schema.TypeNames.MetaList:
							foreach (IReadOnlyDictionary<string, object> item in (IReadOnlyList<object>)contents) {
								if (doesValueMeetCondition(item, predicate)) {
									return true;
								}
							}
							break;
						default:
							throw new FormatException();	// TODO: message
					}

					return false;
				}

				bool doesParamMeetCondition(string paramName, Predicate<string> predicate) {
					// paramName can be null
					Debug.Assert(predicate != null);

					IReadOnlyDictionary<string, object> paramValue = context.GetOptionalValue<IReadOnlyDictionary<string, object>>(paramName, null);
					return doesValueMeetCondition(paramValue, predicate);
				}

				// "from-file" parameter
				// TODO: platform consideration for path comparison (case sensitivity)
				if (doesParamMeetCondition("from-file", str => string.Compare(str, fromFileRelPath, StringComparison.OrdinalIgnoreCase) == 0)) {
					return getTrueCaseValue();
				}

				return getFalseCaseValue();
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
			AST ast = new AST(astJsonValue, CreateParameters(astJsonValue));

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


		protected Parameters CreateParameters(Dictionary<string, object> ast) {
			// new a Parameters instance
			Parameters parameters = NewParameters(ast);

			// setup the instance and freeze it 
			SetupParameters(parameters);
			parameters.Freeze();

			return parameters;
		}

		#endregion


		#region overridables - parameters

		protected virtual Parameters NewParameters(Dictionary<string, object> ast) {
			return new Parameters(ast, ast: true);
		}

		protected virtual void SetupParameters(Parameters parameters) {
		}

		#endregion


		#region overridables - modify

		protected virtual void ModifyMetadata(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			context.RunInEachChildContext(ModifyValue);
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
			context.RunInEachChildContext(ModifyValue);
		}

		protected virtual void ModifyObject(ModifyingContext context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			Debug.Assert(context.IsObject);

			// modify only elements, by default
			(string type, object contents) = context.IsElement();
			if (type != null) {
				// value is an element
				// Note that content may be null.
				ModifyElement(context, type, contents);
			} else {
				context.RunInEachChildContext(ModifyValue);
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
					string macroName = Schema.IsContentsMacro(contents);
					if (macroName != null) {
						// macro
						object expandedValue = ExpandMacro(context, macroName, contents);
						if (expandedValue == null) {
							context.RemoveValue();
						} else {
							context.ReplaceValue(expandedValue);
						}
						return;	// processed
					}
					break;
			}

			// modify contents
			if (contents != null) {
				context.RunInChildContext(Schema.Names.C, contents, ModifyValue);
			}
		}

		#endregion


		#region overridables - generate
		#endregion


		#region overridables - macro expansion

		protected virtual object ExpandMacro<ActualContext>(ActualContext context) where ActualContext: Context<ActualContext> {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			object value;
			if (context.IsObject) {
				// the current value is an object
				(string macroName, object macroContents) = context.IsMacro();
				if (macroName == null) {
					// normal JSON object
					Dictionary<string, object> obj = context.GetEditingBaseObject();
					// Take a snapshot of the keys. The contents of obj may be modified.
					string[] keys = obj.Keys.ToArray();
					foreach (string key in keys) {
						obj[key] = context.RunInChildContext(key, obj[key], ExpandMacro<ActualContext>);
					}
					value = obj;
				} else {
					// macro
					// replace the current value with evaluated value
					value = ExpandMacro(context, macroName, macroContents);
				}
			} else if (context.IsArray) {
				// the current value is an array
				List<object> array = context.GetEditingBaseArray();
				for (int i = 0; i < array.Count; ++i) {
					array[i] = context.RunInChildContext(i, array[i], ExpandMacro<ActualContext>);
				}
				value = array;
			} else {
				// simple value
				value = context.Value;
			}

			return value;
		}

		protected object ExpandMacro<ActualContext>(ActualContext context, string macroName, object macroContents) where ActualContext: Context<ActualContext> {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (macroName == null) {
				throw new ArgumentNullException(nameof(macroName));
			}
			Debug.Assert(context.IsObject);

			return context.RunInChildContext(Schema.Names.C, macroContents, (childContext) => ExpandMacro<ActualContext>(childContext, macroName));
		}

		protected virtual object ExpandMacro<ActualContext>(ActualContext context, string macroName) where ActualContext: Context<ActualContext> {
			// argument checks
			if (macroName == null) {
				throw new ArgumentNullException(nameof(macroName));
			}

			throw new ApplicationException($"Unrecognized macro: {macroName}");
		}

		protected virtual object ExpandMacroParameter<ActualContext>(ActualContext context, string key) where ActualContext : Context<ActualContext> {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (key == null) {
				throw new ArgumentNullException(nameof(key));
			}

			object value = context.GetOptionalValue<object>(key, null);
			return (value == null) ? null : context.RunInChildContext(key, value, ExpandMacro<ActualContext>);
		}

		#endregion
	}
}
