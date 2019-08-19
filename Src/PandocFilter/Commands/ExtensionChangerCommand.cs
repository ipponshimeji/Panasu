using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Utf8Json;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Commands {
	public class ExtensionChangerCommand: FilteringCommand {
		#region data

		protected string FromBaseDirPath { get; set; } = null;

		protected string FromFileRelPath { get; set; } = null;

		protected string ToBaseDirPath { get; set; } = null;

		protected string ToFileRelPath { get; set; } = null;

		protected readonly Dictionary<string, string> ExtensionMap = new Dictionary<string, string>();

		protected bool RebaseOtherRelativeLinks { get; private set; } = false;

		#endregion


		#region constructors

		public ExtensionChangerCommand() {
		}

		#endregion


		#region overrides

		protected override void ProcessNormalArgument(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption == false);

			switch (arg.Index) {
				case 0:
					this.FromBaseDirPath = arg.Value;
					break;
				case 1:
					this.FromFileRelPath = arg.Value;
					break;
				case 2:
					this.ToBaseDirPath = arg.Value;
					break;
				case 3:
					this.ToFileRelPath = arg.Value;
					break;
			}
		}

		protected override void ProcessOption(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption);

			string name = arg.Name;
			if (AreSameOptionNames(name, "Map")) {
				(string from, string to) SplitExtensions(string val) {
					int index = val.IndexOf(':');
					if (index < 0) {
						throw new ArgumentException("Its form must be \"<from ext>:<to ext>\"");    // TODO: message
					}
					string f = val.Substring(0, index);
					string t = val.Substring(index + 1);

					return (f, t);
				}

				(string from, string to) = SplitExtensions(arg.Value);
				this.ExtensionMap.Add(from, to);
			} else if (OptionNameStartsWith("RebaseOtherRelativeLinks", name)) {
				this.RebaseOtherRelativeLinks = true;
			}
		}

		protected override void OnExecuting() {
			// state checks
			if (string.IsNullOrEmpty(this.FromBaseDirPath)) {
				throw CreateMissingIndispensableArgumentException(nameof(FromBaseDirPath));
			}
			if (string.IsNullOrEmpty(this.FromFileRelPath)) {
				throw CreateMissingIndispensableArgumentException(nameof(FromFileRelPath));
			}
			if (string.IsNullOrEmpty(this.ToBaseDirPath)) {
				throw CreateMissingIndispensableArgumentException(nameof(ToBaseDirPath));
			}
			if (string.IsNullOrEmpty(this.ToFileRelPath)) {
				throw CreateMissingIndispensableArgumentException(nameof(ToFileRelPath));
			}

			string fromExtension = Path.GetExtension(this.FromFileRelPath);
			if (!this.ExtensionMap.ContainsKey(fromExtension)) {
				this.ExtensionMap.Add(fromExtension, Path.GetExtension(this.ToFileRelPath));
			}
		}

		protected override void Execute(Stream inputStream, Stream outputStream) {
			ExtensionChangingFilter filter = new ExtensionChangingFilter(this.FromBaseDirPath, this.FromFileRelPath, this.ToBaseDirPath, this.ToFileRelPath, this.RebaseOtherRelativeLinks, this.ExtensionMap);

			// read input AST
			Dictionary<string, object> ast = JsonSerializer.Deserialize<Dictionary<string, object>>(inputStream);

			// modify the AST
			filter.Modify(ast);

			// write output AST
			JsonSerializer.Serialize(outputStream, ast);
		}

		protected override int OnExecuted(Exception error) {
			int exitCode;

			// handle the execution result
			if (error == null) {
				// execution succeeded
				exitCode = SuccessExitCode;
			} else {
				// execution failed
				Console.Error.WriteLine(error.Message);
				exitCode = GeneralErrorExitCode;
			}

			return exitCode;
		}

		#endregion
	}
}
