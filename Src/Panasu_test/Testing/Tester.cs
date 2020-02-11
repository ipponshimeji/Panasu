using System;

namespace Panasu.Testing {
	public abstract class Tester<T> {
		#region overridables

		public abstract void Test(T target);

		#endregion
	}
}
