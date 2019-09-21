using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Utf8Json;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Commands {
	public class FormatterCommand: FilterCommand {
		#region constants

		public const string FormatTaskKind = "Format";

		#endregion


		#region properties

		public new FormattingFilter.Configurations Config {
			get {
				return GetConfiguration<FormattingFilter.Configurations>();
			}
		}

		public FormattingFilter.Parameters Parameters {
			get {
				return this.Config.Parameters;
			}
		}

		#endregion


		#region constructors

		protected FormatterCommand(FormattingFilter.Configurations config, string commandName, string invocation = null) : base(config, commandName, invocation) {
		}

		public FormatterCommand(string commandName, string invocation = null) : base(new FormattingFilter.Configurations(), commandName, invocation) {
		}

		#endregion


		#region overrides

		protected override void ProcessNormalArgument(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption == false);

			switch (arg.Index) {
				case 0:
					this.Parameters.FromBaseDirPath = arg.Value;
					break;
				case 1:
					this.Parameters.FromFileRelPath = arg.Value;
					break;
				case 2:
					this.Parameters.ToBaseDirPath = arg.Value;
					break;
				case 3:
					this.Parameters.ToFileRelPath = arg.Value;
					break;
				default:
					base.ProcessNormalArgument(arg);
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
				this.Parameters.AddExtensionMap(from, to);
			} else if (OptionNameStartsWith("RebaseOtherRelativeLinks", name)) {
				this.Parameters.RebaseOtherRelativeLinks = true;
			} else {
				base.ProcessOption(arg);
			}
		}

		protected override string OnExecuting() {
			string command = base.OnExecuting();
			if (command != null) {
				// reserved command (such as usage or version)
				return command;
			}

			// state checks

			return FormatTaskKind;
		}

		protected override void Execute(string taskKind) {
			switch (taskKind) {
				case FormatTaskKind:
					Filter(taskKind);
					break;
				default:
					base.Execute(taskKind);
					break;
			}
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

		protected override void ShowUsage() {
			Console.WriteLine($"{this.Invocation} [OPTIONS] FromBaseDirPath FromFileRelPath ToBaseDirPath ToFileRelPath");
			Console.WriteLine("OPTIONS:");
			Console.WriteLine("Note that option names are case-insensitive.");
			Console.WriteLine("  -Help");
			Console.WriteLine("  -Map:<fromExt>:<toExt>");
			Console.WriteLine("  -R[ebaseOtherRelativeLinks]");
			Console.WriteLine("  -Version");
		}

		protected override void Filter(string taskKind, Stream inputStream, Stream outputStream) {
			FormattingFilter.Configurations clonedConfig = new FormattingFilter.Configurations(this.Config);
			FormattingFilter filter = new FormattingFilter(clonedConfig);

			// read input AST
			Dictionary<string, object> ast = JsonSerializer.Deserialize<Dictionary<string, object>>(inputStream);

			// modify the AST
			filter.Modify(ast);

			// write output AST
			JsonSerializer.Serialize(outputStream, ast);
		}

		#endregion
	}
}
