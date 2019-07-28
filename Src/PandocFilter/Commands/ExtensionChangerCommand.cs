using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utf8Json;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Commands {
	public class ExtensionChangerCommand: Command {
		#region data

		protected string InputFilePath { get; set; } = null;

		protected string OutputFilePath { get; set; } = null;

		protected readonly Dictionary<string, string> ExtensionMap = new Dictionary<string, string>();

		protected bool RebaseOtherRelativeLink { get; private set; } = false;

		#endregion


		#region constructors

		public ExtensionChangerCommand() {
		}

		#endregion


		#region overridables

		protected override void ProcessNormalArgument(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption == false);

			switch (arg.Index) {
				case 0:
					this.InputFilePath = arg.Value;
					break;
				case 1:
					this.OutputFilePath = arg.Value;
					break;
			}
		}

		protected override void ProcessOption(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption);

			string name = arg.Name;
			if (AreSameOptionNames(name, "map")) {
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
			} else if (AreSameOptionNames(name, "RebaseOtherRelativeLink")) {
				this.RebaseOtherRelativeLink = true;
			}
		}

		protected override void OnExecuting() {
			// state checks
			if (string.IsNullOrEmpty(this.InputFilePath)) {
				throw new InvalidOperationException("The indispensable argument 'InputFilePath' is missing.");
			}
			if (string.IsNullOrEmpty(this.OutputFilePath)) {
				throw new InvalidOperationException("The indispensable argument 'OutputFilePath' is missing.");
			}
		}

		protected override void Execute() {
			ChangingExtensionFilter filter = new ChangingExtensionFilter(this.InputFilePath, this.OutputFilePath, false, this.ExtensionMap);

			// read input AST
			Dictionary<string, object> ast;
			using (Stream stream = Console.OpenStandardInput()) {
				ast = JsonSerializer.Deserialize<Dictionary<string, object>>(stream);
			}

			// modify the AST
			filter.Modify(ast);

			// write output AST
			using (Stream stream = Console.OpenStandardOutput()) {
				JsonSerializer.Serialize(stream, ast);
			}

			return;
		}

		protected override int OnExecuted(Exception error) {
			Console.Error.WriteLine(error.Message);
			return (error == null) ? SuccessExitCode : GeneralErrorExitCode;
		}

		#endregion
	}
}
