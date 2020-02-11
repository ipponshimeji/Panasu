using System;
using System.Diagnostics;
using Xunit;

namespace Panasu.Testing {
	public abstract class ReferenceTester<T>: Tester<T> where T: class {
		#region overridables

		public virtual void TestNullable(T? target) {
			Assert.NotNull(target);
			Debug.Assert(target != null);
			Test(target);
		}

		#endregion
	}
}
