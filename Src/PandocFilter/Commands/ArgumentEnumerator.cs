using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Commands {
	public class ArgumentEnumerator: IEnumerable<Argument> {
		#region constants

		public const char OptionMarker = '-';
		public const char DefaultOptionSeparator = ':';

		#endregion


		#region data

		protected readonly bool DividedOptionStyle;

		protected readonly IReadOnlyList<string> CommandLineArgs;

		protected int NextIndex;

		protected int NextNormalArgumentIndex;

		#endregion


		#region constructors

		public ArgumentEnumerator(string[] args, bool dividedOptionStyle = false) {
			// argument checks
			if (args == null) {
				throw new ArgumentNullException(nameof(args));
			}

			// initialize members
			this.DividedOptionStyle = dividedOptionStyle;
			this.CommandLineArgs = args;
			this.NextIndex = 0;
			this.NextNormalArgumentIndex = 0;
		}

		#endregion


		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator() {
			Argument argument;
			if (TryMoveNextArgument(out argument)) {
				yield return argument;
			}
		}

		#endregion


		#region IEnumerable<Argument>

		public IEnumerator<Argument> GetEnumerator() {
			Argument argument;
			while (TryMoveNextArgument(out argument)) {
				yield return argument;
			}
		}

		#endregion


		#region methods

		protected bool TryFetchNextCommandLineArg(out string arg) {
			int nextIndex = this.NextIndex;
			if (this.CommandLineArgs.Count <= nextIndex) {
				arg = string.Empty;
				return false;
			} else {
				arg = this.CommandLineArgs[nextIndex];
				return true;
			}
		}

		protected bool TryMoveNextCommandLineArg(out string arg) {
			bool result = TryFetchNextCommandLineArg(out arg);
			if (result) {
				Debug.Assert(this.NextIndex < this.CommandLineArgs.Count);
				++this.NextIndex;
			}
			return result;
		}

		protected Argument ParseJoinedOption(string arg, int startIndex, char optionSeparator) {
			// argument checks
			if (string.IsNullOrEmpty(arg)) {
				throw new ArgumentNullException(nameof(arg));
			}
			if (startIndex < 0 || arg.Length < startIndex) {
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			string name;
			string value;
			int separatorIndex = arg.IndexOf(optionSeparator, startIndex);
			if (0 <= separatorIndex) {
				name = arg.Substring(startIndex, separatorIndex - startIndex);
				value = arg.Substring(separatorIndex + 1);
			} else {
				name = arg.Substring(startIndex);
				value = string.Empty;
			}

			return new Argument(name, value);
		}

		#endregion


		#region overridables

		protected virtual bool TryMoveNextArgument(out Argument argument) {
			string arg;
			if (!TryMoveNextCommandLineArg(out arg)) {
				// no more command line arg
				argument = new Argument();
				return false;
			} else {
				if (string.IsNullOrEmpty(arg)) {
					// an empty normal argument
					argument = new Argument(this.NextNormalArgumentIndex++, string.Empty);
				} else {
					switch (arg[0]) {
						case OptionMarker:
							// an option
							argument = this.DividedOptionStyle ? ParseDividedOption(arg, 1) : ParseJoinedOption(arg, 1);
							break;
						default:
							// a normal argument
							argument = new Argument(this.NextNormalArgumentIndex++, arg);
							break;
					}
				}
				return true;
			}
		}

		protected virtual Argument ParseJoinedOption(string arg, int startIndex) {
			return ParseJoinedOption(arg, startIndex, DefaultOptionSeparator);
		}

		protected virtual Argument ParseDividedOption(string arg, int startIndex) {
			// argument checks
			if (!string.IsNullOrEmpty(arg)) {
				throw new ArgumentNullException(nameof(arg));
			}
			if (startIndex < 0 || arg.Length < startIndex) {
				throw new ArgumentOutOfRangeException(nameof(startIndex));
			}

			string value;
			bool result = TryMoveNextCommandLineArg(out value);
			Debug.Assert(result || value == string.Empty);
			return new Argument(arg, value);
		}

		#endregion
	}
}
