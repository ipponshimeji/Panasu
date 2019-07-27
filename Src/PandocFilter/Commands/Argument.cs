using System;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Commands {
	public struct Argument {
		#region data

		public readonly int Index;

		public readonly string Name;

		public readonly string Value;

		#endregion


		#region properties

		public bool IsOption {
			get {
				return Index < 0;
			}
		}

		#endregion


		#region constructors

		public Argument(int index, string normalArg) {
			// argument checks
			if (index < 0) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			if (string.IsNullOrEmpty(normalArg)) {
				throw new ArgumentNullException(nameof(normalArg));
			}

			// initialize members
			this.Index = index;
			this.Name = null;
			this.Value = normalArg;
		}

		public Argument(string optionName, string optionValue) {
			// argument checks
			if (string.IsNullOrEmpty(optionName)) {
				throw new ArgumentNullException(nameof(optionName));
			}
			// optionValue can be null

			// initialize members
			this.Index = -1;
			this.Name = optionName;
			this.Value = optionValue;
		}

		#endregion
	}
}
