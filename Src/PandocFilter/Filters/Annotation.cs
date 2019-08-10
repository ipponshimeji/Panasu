using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class Annotation {
		#region data

		public RWLock Lock { get; private set; } = RWLock.Dummy;

		private Dictionary<string, object> annotation = null;

		#endregion


		#region creation and destruction

		public Annotation() {
		}

		public Annotation(RWLock rwLock) {
			Initialize(rwLock);
		}


		protected void Initialize(RWLock rwLock) {
			// argument checks
			if (rwLock == null) {
				rwLock = RWLock.Dummy;
			}

			// initialize an instance
			this.Lock = rwLock;
			Debug.Assert(this.annotation == null || this.annotation.Count == 0);
		}

		protected void Clear() {
			// clear members
			if (this.annotation != null) {
				this.annotation.Clear();
			}
			this.Lock = RWLock.Dummy;
		}

		#endregion


		#region methods

		private Dictionary<string, object> GetAnnotationTUS() {
			Dictionary<string, object> value = this.annotation;
			if (value == null) {
				value = new Dictionary<string, object>();
				this.annotation = value;
			}
			return value;
		}

		public (bool, T) Get<T>(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}

			// get the annotation data
			return this.Lock.RunInReadLock<(bool, T)>(() => {
				IReadOnlyDictionary<string, object> annotation = this.annotation;
				if (annotation == null) {
					return (false, default(T));
				} else {
					return annotation.GetOptionalValue<T>(name);
				}
			});
		}

		public void Set<T>(string name, T value) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}
			// value can be null

			// set the annotation data
			this.Lock.RunInWriteLock(() => {
				GetAnnotationTUS()[name] = value;
			});
		}

		public void Remove(string name) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}

			// remove the annotation data
			this.Lock.RunInWriteLock(() => {
				Dictionary<string, object> annotation = this.annotation;
				if (annotation != null) {
					annotation.Remove(name);
				}
			});
		}

		public T RunWithAnnotation<T>(Func<IReadOnlyDictionary<string, object>, T> func) {
			return this.Lock.RunInReadLock<T>(() => func(this.annotation));
		}

		public void RunWithAnnotation(Action<IDictionary<string, object>> action) {
			this.Lock.RunInWriteLock(() => action(this.annotation));
		}

		#endregion
	}
}
