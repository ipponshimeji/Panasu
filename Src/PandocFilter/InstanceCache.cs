using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace PandocUtil.PandocFilter {
	/// <summary>
	/// The class to cache instances so that instances are be reused easily.
	/// It may prevent 'garbage' increasing and frequent GC if the target
	/// class is, for example, large, newed frequently and its life is short.
	/// </summary>
	/// <remarks>
	/// You must program carefully so that the cached instance, which is in deactivated state,
	/// is not accessed.
	/// </remarks>
	/// <typeparam name="T"></typeparam>
	public abstract class InstanceCache<T>: IDisposable where T: class {
		#region constants

		public const int DefaultMaxCachedInstanceCount = 16;

		#endregion


		#region data

		public readonly string CacheName;

		#endregion


		#region data - synchronized by locking instanceLocker

		private readonly object instanceLock = new object();

		private Queue<T> cache = new Queue<T>();

		private int maxCachedInstanceCount = DefaultMaxCachedInstanceCount;

		#endregion


		#region properties

		public int MaxCachedInstanceCount {
			get {
//				lock (this.instanceLock) {
					return this.maxCachedInstanceCount;
//				}
			}
			set {
				// argument checks
				if (value < 0) {
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				lock (this.instanceLock) {
					this.maxCachedInstanceCount = value;
				}
			}
		}

		#endregion


		#region creation and disposal

		public InstanceCache(string cacheName) {
			// initialize members
			this.CacheName = cacheName;

			return;
		}

		public virtual void Dispose() {
			Queue<T> temp;
			lock (this.instanceLock) {
				// clear the instance cache
				temp = this.cache;
				this.cache = null;
			}

			// discard instances
			if (temp != null) {
				T instance;
				while (temp.TryDequeue(out instance)) {
					DiscardInstanceIgnoringException(instance);
				}
			}

			return;
		}

		#endregion


		#region methods

		protected void DiscardInstanceIgnoringException(T instance) {
			// argument checks
			Debug.Assert(instance != null);

			// discard the instance ignoring exception
			try {
				DiscardInstance(instance);
			} catch {
				// continue
			}

			return;
		}

		public T AllocInstance() {
			T instance = null;
			lock (this.instanceLock) {
				// state checks
				Queue<T> cache = this.cache;
				if (cache == null) {
					throw new ObjectDisposedException(GetType().Name);
				}

				// try to reuse a cached instance
				if (cache.TryDequeue(out instance) == false) {
					// create a new instance if there is no cached one
					instance = CreateInstance();
				}
			}

			return instance;
		}

		public void ReleaseInstance(T instance, bool discardInstance = false) {
			// argument checks
			if (instance == null) {
				throw new ArgumentNullException(nameof(instance));
			}

			try {
				lock (this.instanceLock) {
					// try to cache the instance
					if (discardInstance == false) {
						Queue<T> cache = this.cache;
						if (cache != null && cache.Count < this.maxCachedInstanceCount) {
							cache.Enqueue(instance);
							instance = null;
						}
					}
				}
			} catch {
				// continue
			}

			// discard the instance if not cached
			if (instance != null) {
				DiscardInstanceIgnoringException(instance);
			}

			return;
		}

		#endregion


		#region overridables

		protected abstract T CreateInstance();

		protected virtual void DiscardInstance(T instance) {
			// do nothing by default
		}

		#endregion
	}
}
