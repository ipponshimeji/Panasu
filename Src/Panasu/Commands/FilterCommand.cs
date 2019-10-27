using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Utf8Json;
using Panasu.Filters;

namespace Panasu.Commands {
	public abstract class FilterCommand: Command {
		#region data

		private Filter.Configurations config = null;

		protected string InputFile { get; private set; } = null;

		protected Stream InputStream { get; private set; } = null;

		protected Stream OutputStream { get; private set; } = null;

		#endregion


		#region properties

		public Filter.Configurations Config {
			get {
				return this.config;
			}
		}

		#endregion


		#region constructors

		protected FilterCommand(Filter.Configurations config, string commandName, string invoker): base(commandName, invoker) {
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
				// already running
				throw new InvalidOperationException();
			}

			// call Run(string[]) in the state that I/O stream is set
			this.InputStream = inputStream;
			this.OutputStream = outputStream;
			try {
				return Run(args);
			} finally {
				this.OutputStream = null;
				this.InputStream = null;
			}
		}

		protected TConfiguration GetConfiguration<TConfiguration>() where TConfiguration: Filter.Configurations {
			return (TConfiguration)this.config;
		}

		#endregion


		#region overrides

		protected override void ProcessOption(IEnumerator<string> args, string shortName, string longName, string value) {
			// argument checks
			Debug.Assert(args != null);
			Debug.Assert(value == null);    // using value-separated option 

			if (AreSameOptionNames(shortName, "i") || AreSameOptionNames(longName, "input")) {
				this.InputFile = GetSeparatedOptionValue(args, shortName, longName);
			} else {
				base.ProcessOption(args, shortName, longName, value);
			}
		}

		#endregion


		#region overridables

		protected virtual void Filter() {
			if (this.InputStream != null) {
				// filter using given i/o streams
				Debug.Assert(this.OutputStream != null);
				Filter(this.InputStream, this.OutputStream);
			} else {
				// filter using standard io
				Debug.Assert(this.OutputStream == null);
				using (Stream inputStream = OpenInput()) {
					using (Stream outputStream = Console.OpenStandardOutput()) {
						Filter(inputStream, outputStream);
					}
				}
			}
		}

		private Stream OpenInput() {
			string inputFile = this.InputFile;
			if (string.IsNullOrEmpty(inputFile)) {
				return Console.OpenStandardInput();
			} else {
				return File.OpenRead(inputFile);
			}
		}

		protected virtual void Filter(Stream inputStream, Stream outputStream) {
			// argument checks
			Debug.Assert(inputStream != null);
			Debug.Assert(outputStream != null);

			// create a filter
			(Filter filter, bool useGenerate) = CreateFilter();

			// read input AST
			Dictionary<string, object> inputAST = JsonSerializer.Deserialize<Dictionary<string, object>>(inputStream);

			// filter the AST
			Dictionary<string, object> outputAST;
			if (useGenerate) {
				outputAST = filter.Generate(inputAST);
			} else {
				filter.Modify(inputAST);
				outputAST = inputAST;
			}

			// write output AST
			JsonSerializer.Serialize(outputStream, outputAST);
		}

		protected abstract (Filter filter, bool useGenerate) CreateFilter();

		#endregion
	}
}
