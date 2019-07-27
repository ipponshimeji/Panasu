using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PandocUtil.PandocFilter.Filters {
	public class Filter {
		#region types

		protected class Context {
			#region constants

			public const int UndefinedIndex = -1;

			#endregion


			#region data

			private static object classLocker = new object();

			private static Context instanceCacheChainTop = null;


			private ReaderWriterLockSlim contextLock;

			public Context Root { get; private set; }

			public Context Parent { get; private set; }

			public string Name { get; private set; }

			public int Index { get; private set; }

			public object Value { get; private set; }

			public IDictionary<string, object> data;

			#endregion


			#region properties

			public bool Concurrent {
				get {
					return this.contextLock != null;
				}
			}

			public IDictionary<string, object> Data {
				get {
					IDictionary<string, object> value = this.data;
					if (value == null) {
						RunInWriteLock(() => {
							value = this.data;  // may be set while we got lock
							if (value == null) {
								value = new Dictionary<string, object>();
								this.data = value;
							}
						});
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

			private Context() {
				// initialize members
				this.contextLock = null;
				this.Root = null;
				this.Parent = null;
				this.Name = null;
				this.Index = UndefinedIndex;
				this.Value = null;
				this.data = null;
			}

			private void Clear() {
				// clear members
				Util.ClearDisposable(ref this.contextLock);
				this.Root = null;
				this.Parent = null;
				this.Name = null;
				this.Index = UndefinedIndex;
				this.Value = null;
				if (this.data != null) {
					this.data.Clear();
				}
			}


			private static Context CreateContext() {
				Context instance;

				// try to get an instance from the cache
				lock (classLocker) {
					instance = instanceCacheChainTop;
					if (instance != null) {
						instanceCacheChainTop = instance.Parent;
						instance.Parent = null;
					}
				}
				
				// create an instance if cache is empty
				if (instance == null) {
					instance = new Context();
				}

				return instance;
			}

			private static Context CreateContext(bool concurrent, object value) {
				// argument checks
				// value can be null

				// create and setup an instance
				Context instance = CreateContext();
				Debug.Assert(instance.contextLock == null);
				Debug.Assert(instance.Root == null);
				Debug.Assert(instance.Parent == null);
				Debug.Assert(instance.Name == null);
				Debug.Assert(instance.Index == UndefinedIndex);
				instance.Value = value;
				Debug.Assert(instance.data == null || instance.data.Count == 0);

				if (concurrent) {
					instance.contextLock = new ReaderWriterLockSlim();
				}

				return instance;
			}

			private static Context CreateContext(bool concurrent, IDictionary<string, object> ast) {
				// argument checks
				Debug.Assert(ast != null);

				// create and setup an instance
				Context instance = CreateContext(concurrent, ast);
				instance.Root = instance;
				return instance;
			}

			private Context CreateChildContext(object childValue) {
				// create and setup a child context
				Context childContext = CreateContext(this.Concurrent, childValue);
				childContext.Root = this.Root;
				childContext.Parent = this;
				return childContext;
			}

			private Context CreateChildContext(string childName, object childValue) {
				// argument checks
				Debug.Assert(childName != null);

				// create and setup a child context
				Context childContext = CreateChildContext(childValue);
				childContext.Name = childName;
				return childContext;
			}

			private Context CreateChildContext(int childIndex, object childValue) {
				// argument checks
				Debug.Assert(0 <= childIndex);

				// create and setup a child context
				Context childContext = CreateChildContext(childValue);
				childContext.Index = childIndex;
				return childContext;
			}

			private static void ReleaseContext(Context instance) {
				// argument checks
				if (instance == null) {
					return;
				}

				// clear the instance
				instance.Clear();

				// cache the instance
				lock (classLocker) {
					instance.Parent = instanceCacheChainTop;
					instanceCacheChainTop = instance;
				}

				return;
			}

			#endregion


			#region methods

			public static void RunInContext(bool concurrent, IDictionary<string, object> ast, Action<Context> action) {
				// argument checks
				if (ast == null) {
					throw new ArgumentNullException(nameof(ast));
				}
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				Context context = CreateContext(concurrent, ast);
				try {
					action(context);
				} finally {
					ReleaseContext(context);
				}
			}

			public void RunInChildContext(string childName, object childValue, Action<Context> action) {
				// argument checks
				if (childName == null) {
					throw new ArgumentNullException(nameof(childName));
				}
				// childValue can be null
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				Context childContext = CreateChildContext(childName, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseContext(childContext);
				}
			}

			public void RunInChildContext(int childIndex, object childValue, Action<Context> action) {
				// argument checks
				if (childIndex < 0) {
					throw new ArgumentOutOfRangeException(nameof(childIndex));
				}
				// childValue can be null
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the action in the context
				Context childContext = CreateChildContext(childIndex, childValue);
				try {
					action(childContext);
				} finally {
					ReleaseContext(childContext);
				}
			}

			public void RunInEachChildContext(IList<object> array, Action<Context> action) {
				// argument checks
				if (this.Concurrent) {
					throw new NotSupportedException("This method is optimized version only for single thread mode.");
				}
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}
				if (array == null) {
					return;
				}

				// run the action in the context
				Context context = CreateChildContext(null);
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

			public T RunInReadLock<T>(Func<T> func) {
				// argument checks
				if (func == null) {
					throw new ArgumentNullException(nameof(func));
				}

				// run the func inside the read lock
				ReaderWriterLockSlim contextLock = this.contextLock;
				if (contextLock == null) {
					// running in single thread mode
					return func();
				} else {
					// running in concurrent mode
					contextLock.EnterReadLock();
					try {
						return func();
					} finally {
						contextLock.ExitReadLock();
					}
				}
			}

			public void RunInWriteLock(Action action) {
				// argument checks
				if (action == null) {
					throw new ArgumentNullException(nameof(action));
				}

				// run the func inside the write lock
				ReaderWriterLockSlim contextLock = this.contextLock;
				if (contextLock == null) {
					// running in single thread mode
					action();
				} else {
					// running in concurrent mode
					contextLock.EnterWriteLock();
					try {
						action();
					} finally {
						contextLock.ExitWriteLock();
					}
				}
			}

			#endregion
		}

		#endregion


		#region creation

		protected Filter() {
		}

		#endregion


		#region methods

		public void Modify(bool concurrent, IDictionary<string, object> ast) {
			// argument checks
			if (ast == null) {
				throw new ArgumentNullException(nameof(ast));
			}

			// single thread mode
			Context.RunInContext(concurrent, ast, (rootContext) => {
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

		protected virtual void ModifyValue(Context context) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}

			// modify depending on type of the value
			switch (context.Value) {
				// Check IDictionary<string, object> prior to IList<object> to detect object,
				// because IDictionary<string, object> implements IList<object>.
				case IDictionary<string, object> obj:
					// value is an object
					ModifyObject(context, obj);
					break;
				case IList<object> array:
					// value is an array
					ModifyArray(context, array);
					break;
			}

			return;
		}

		protected virtual void ModifyArray(Context context, IList<object> array) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (array == null) {
				throw new ArgumentNullException(nameof(array));
			}

			// modify each element
			if (context.Concurrent) {
				Parallel.For(0, array.Count, (i) => {
					context.RunInChildContext(i, array[i], ModifyValue);
				});
			} else {
				context.RunInEachChildContext(array, ModifyValue);
			}
		}

		protected virtual void ModifyObject(Context context, IDictionary<string, object> obj) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (obj == null) {
				throw new ArgumentNullException(nameof(obj));
			}

			// modify only elements, by default
			(bool _, string type) = obj.GetOptionalValue<string>(Schema.Names.T);
			(bool _, object content) = obj.GetOptionalValue<object>(Schema.Names.C);
			if (!string.IsNullOrEmpty(type)) {
				// value is an element
				// Note that content may be null.
				ModifyElement(context, obj, type, content);
			}
		}

		protected virtual void ModifyElement(Context context, IDictionary<string, object> element, string type, object contents) {
			// argument checks
			if (context == null) {
				throw new ArgumentNullException(nameof(context));
			}
			if (element == null) {
				throw new ArgumentNullException(nameof(element));
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
