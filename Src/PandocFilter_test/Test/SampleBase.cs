using System;

namespace PandocUtil.PandocFilter.Test {
	public class SampleBase {
		#region data

		public readonly string Description;

		#endregion


		#region constructors

		public SampleBase(string description) {
			// argument checks
			if (description == null) {
				description = "a sample";
			}

			// initialize members
			this.Description = description;
		}

		#endregion


		#region overrides

		public override string ToString() {
			return this.Description;
		}

		#endregion
	}
}
