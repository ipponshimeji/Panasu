using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Commands {
	public class FilteringCommand: Command {
		#region properties

		protected Stream InputStream { get; private set; } = null;

		protected Stream OutputStream { get; private set; } = null;

		#endregion


		#region constructors

		public FilteringCommand(): base() {
		}

		#endregion


		#region methods

		public int Run(string[] args, Stream inputStream, Stream outputStream) {
			// argument checks
			if (inputStream == null) {
				throw new ArgumentNullException(nameof(inputStream));
			}
			if (outputStream == null) {
				throw new ArgumentNullException(nameof(outputStream));
			}

			// state checks
			if (this.InputStream != null || this.OutputStream != null) {
				throw new InvalidOperationException();
			}

			this.InputStream = inputStream;
			this.OutputStream = outputStream;
			try {
				return Run(args);
			} finally {
				this.OutputStream = null;
				this.InputStream = null;
			}
		}

		#endregion


		#region overrides

		protected override void Execute() {
			if (this.InputStream != null) {
				Debug.Assert(this.OutputStream != null);
				Execute(this.InputStream, this.OutputStream);
			} else {
				Debug.Assert(this.OutputStream == null);
				using (Stream inputStream = Console.OpenStandardInput()) {
					using (Stream outputStream = Console.OpenStandardOutput()) {
						Execute(inputStream, outputStream);
					}
				}
			}
		}

		#endregion


		#region overridables

		protected virtual void Execute(Stream inputStream, Stream outputStream) {
		}

		#endregion
	}
}
