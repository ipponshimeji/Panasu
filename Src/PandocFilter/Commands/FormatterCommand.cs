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


		#region data

		protected string FromBaseDirPath { get; set; } = null;

		protected string FromFileRelPath { get; set; } = null;

		protected string ToBaseDirPath { get; set; } = null;

		protected string ToFileRelPath { get; set; } = null;

		protected Dictionary<string, string> ExtensionMap = null;

		protected bool? RebaseOtherRelativeLinks { get; private set; } = false;

		#endregion


		#region constructors

		public FormatterCommand(string commandName, string invocation = null) : base(commandName, invocation) {
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
				if (this.ExtensionMap == null) {
					this.ExtensionMap = new Dictionary<string, string>();
				}
				this.ExtensionMap.Add(from, to);
			} else if (OptionNameStartsWith("RebaseOtherRelativeLinks", name)) {
				this.RebaseOtherRelativeLinks = true;
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
			FormattingFilter filter = new FormattingFilter();
			if (this.FromBaseDirPath != null) {
				filter.FromBaseDirPath = this.FromBaseDirPath;
			}
			if (this.FromFileRelPath != null) {
				filter.FromFileRelPath = this.FromFileRelPath;
			}
			if (this.ToBaseDirPath != null) {
				filter.ToBaseDirPath = this.ToBaseDirPath;
			}
			if (this.ToFileRelPath != null) {
				filter.ToFileRelPath = this.ToFileRelPath;
			}
			if (this.RebaseOtherRelativeLinks.HasValue) {
				filter.RebaseOtherRelativeLinks = this.RebaseOtherRelativeLinks;
			}
			if (this.ExtensionMap != null) {
				filter.ExtensionMap = this.ExtensionMap;
			}

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
