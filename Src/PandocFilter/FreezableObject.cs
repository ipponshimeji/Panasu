using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace Panasu {
	public class FreezableObject {
		#region data

		public bool IsFreezed { get; private set; } = false;

		#endregion


		#region methods

		public void Freeze() {
			if (this.IsFreezed == false) {
				this.IsFreezed = true;
				OnFreezed();
			}
		}

		public void EnsureNotFreezed() {
			if (this.IsFreezed) {
				throw new InvalidOperationException("It cannot be modified because it is freezed.");
			}
		}

		#endregion


		#region overridables

		protected virtual void OnFreezed() {
		}

		#endregion
	}
}
