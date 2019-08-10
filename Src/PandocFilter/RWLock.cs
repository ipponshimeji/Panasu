using System;
using System.Diagnostics;
using System.Threading;

namespace PandocUtil.PandocFilter {
	public class RWLock: IDisposable {
		#region data

		/// <summary>
		/// The dummy locker.
		/// This locker does not lock actually.
		/// </summary>
		public static readonly RWLock Dummy = new RWLock(true);

		private ReaderWriterLockSlim readerWriterLock = null;

		#endregion


		#region properties

		public bool IsDummy {
			get {
				return this.readerWriterLock == null;
			}
		}

		#endregion


		#region creation and disposal

		private RWLock(bool dummy) {
			if (dummy == false) {
				readerWriterLock = new ReaderWriterLockSlim();
			}
		}

		public RWLock() : this(false) {
		}


		public virtual void Dispose() {
			Util.ClearDisposable(ref this.readerWriterLock);
		}

		#endregion


		#region methods - locking operation

		public T RunInReadLock<T>(Func<T> func) {
			// argument checks
			if (func == null) {
				throw new ArgumentNullException(nameof(func));
			}

			// run the func inside the read lock
			ReaderWriterLockSlim readerWriterLock = this.readerWriterLock;
			if (readerWriterLock == null) {
				return func();
			} else {
				readerWriterLock.EnterReadLock();
				try {
					return func();
				} finally {
					readerWriterLock.ExitReadLock();
				}
			}
		}

		public void RunInWriteLock(Action action) {
			// argument checks
			if (action == null) {
				throw new ArgumentNullException(nameof(action));
			}

			// run the action inside the write lock
			ReaderWriterLockSlim readerWriterLock = this.readerWriterLock;
			if (readerWriterLock == null) {
				action();
			} else {
				readerWriterLock.EnterWriteLock();
				try {
					action();
				} finally {
					readerWriterLock.ExitWriteLock();
				}
			}
		}

		public T RunInWriteLock<T>(Func<T> func) {
			// argument checks
			if (func == null) {
				throw new ArgumentNullException(nameof(func));
			}

			// run the func inside the write lock
			ReaderWriterLockSlim readerWriterLock = this.readerWriterLock;
			if (readerWriterLock == null) {
				return func();
			} else {
				readerWriterLock.EnterWriteLock();
				try {
					return func();
				} finally {
					readerWriterLock.ExitWriteLock();
				}
			}
		}

		#endregion
	}
}
