using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Commands {
	public class FilterCommand: Command {
		#region data

		private Filter.Configuration config = null;

		protected Stream InputStream { get; private set; } = null;

		protected Stream OutputStream { get; private set; } = null;

		#endregion


		#region properties

		public Filter.Configuration Config {
			get {
				return this.config;
			}
		}

		#endregion


		#region constructors

		protected FilterCommand(Filter.Configuration config, string commandName, string invocation = null) : base(commandName, invocation) {
			// argument checks
			if (config == null) {
				throw new ArgumentNullException(nameof(config));
			}

			// initialize members 
			this.config = config;
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

		protected TConfiguration GetConfiguration<TConfiguration>() where TConfiguration: Filter.Configuration {
			return (TConfiguration)this.config;
		}

		#endregion


		#region overridables

		protected virtual void Filter(string taskKind) {
			if (this.InputStream != null) {
				Debug.Assert(this.OutputStream != null);
				Filter(taskKind, this.InputStream, this.OutputStream);
			} else {
				Debug.Assert(this.OutputStream == null);
				using (Stream inputStream = Console.OpenStandardInput()) {
					using (Stream outputStream = Console.OpenStandardOutput()) {
						Filter(taskKind, inputStream, outputStream);
					}
				}
			}
		}

		protected virtual void Filter(string taskKind, Stream inputStream, Stream outputStream) {
		}

		#endregion
	}
}
