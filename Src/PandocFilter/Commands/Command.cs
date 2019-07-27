using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PandocUtil.PandocFilter.Commands {
	public class Command {
		#region constants

		public const int SuccessExitCode = 0;
		public const int GeneralErrorExitCode = 1;

		#endregion


		#region constructors

		public Command() {
		}

		#endregion


		#region methods

		public int Run(string[] args) {
			Exception error = null;
			try {
				ProcessArguments(args);
				OnExecuting();
				Execute();
			} catch (Exception exception) {
				error = exception;
			}

			return OnExecuted(error);
		}

		#endregion


		#region overridables - argument processing

		protected virtual void ProcessArguments(string[] args) {
			// argument check
			if (args == null) {
				// nothing to do
				return;
			}

			// process each argument
			foreach (Argument arg in GetArgumentEnumerator(args)) {
				ProcessArgument(arg);
			}
		}

		protected virtual ArgumentEnumerator GetArgumentEnumerator(string[] args) {
			return new ArgumentEnumerator(args);
		}

		protected virtual void ProcessArgument(Argument arg) {
			if (arg.IsOption) {
				ProcessOption(arg);
			} else {
				ProcessNormalArgument(arg);
			}
		}

		protected virtual void ProcessNormalArgument(Argument arg) {
		}

		protected virtual void ProcessOption(Argument arg) {
		}

		protected virtual bool AreSameOptionNames(string name1, string name2) {
			// case-insensitive, by default
			return string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		#endregion


		#region overridables - command execution

		protected virtual void OnExecuting() {
		}

		protected virtual void Execute() {
		}

		protected virtual int OnExecuted(Exception error) {
			return (error == null)? SuccessExitCode: GeneralErrorExitCode;
		}

		#endregion
	}
}
