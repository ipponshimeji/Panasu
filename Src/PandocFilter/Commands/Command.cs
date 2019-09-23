using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace PandocUtil.PandocFilter.Commands {
	public class Command {
		#region constants

		// standard exit codes
		public const int SuccessExitCode = 0;
		public const int GeneralErrorExitCode = 1;

		// special chars in command line
		public const char OptionMarker = '-';
		public const char DefaultOptionSeparator = ':';
		public const char SeparatedOptionSeparator = '\0';

		// reserved task kind
		public const string UsageTaskKind = "_Usage";
		public const string VersionTaskKind = "_Version";

		#endregion


		#region data

		public string CommandName { get; private set; }

		private readonly string invoker;

		private string taskKind = null;

		#endregion


		#region properties

		public string Invoker {
			get {
				return this.invoker ?? this.CommandName;
			}
		}

		#endregion


		#region constructors

		public Command(string commandName, string invoker) {
			// argument checks
			if (commandName == null) {
				throw new ArgumentNullException(nameof(commandName));
			}
			// invoker can be null

			// initialize members
			this.CommandName = commandName;
			this.invoker = invoker;
		}

		#endregion


		#region methods

		public int Run(string[] args) {
			Exception error = null;
			try {
				ProcessArguments(args);
				string taskKind = OnExecuting();
				Execute(taskKind);
			} catch (Exception exception) {
				error = exception;
			}

			return OnExecuted(error);
		}


		protected static ApplicationException CreateMissingIndispensableArgumentException(string argName) {
			// argument checks
			if (string.IsNullOrEmpty(argName)) {
				argName = "(unknown)";
			}

			return new ApplicationException($"The indispensable argument '{argName}' is missing.");
		}

		protected static ApplicationException CreateInvalidOptionException(string optionName, string reason) {
			// argument checks
			if (string.IsNullOrEmpty(optionName)) {
				throw new ArgumentNullException(nameof(optionName));
			}
			if (string.IsNullOrEmpty(reason)) {
				reason = "(unknown reason)";
			}

			return new ApplicationException($"The option '{optionName}' is invalid:\n{reason}");
		}

		protected static bool IsOptionMarker(char c) {
			switch (c) {
				case OptionMarker:
					return true;
				default:
					return false;
			}
		}

		protected static (bool option, string shortName, string longName, string value) IsOption(string arg, char separator) {
			// argument checks
			if (arg == null) {
				throw new ArgumentNullException(nameof(arg));
			}

			if (arg.Length == 0 || IsOptionMarker(arg[0]) == false) {
				// a normal argument
				return (false, null, null, arg);
			} else {
				// an option
				bool longName = false;
				int startIndex = 1;
				if (2 <= arg.Length && IsOptionMarker(arg[1])) {
					longName = true;
					startIndex = 2;
				}

				string name;
				string value;
				if (separator == SeparatedOptionSeparator) {
					// not a value-joined option
					name = arg.Substring(startIndex);
					value = null;	
				} else {
					// a value-joined option
					int separatorIndex = arg.IndexOf(separator, startIndex);
					if (0 <= separatorIndex) {
						name = arg.Substring(startIndex, separatorIndex - startIndex);
						value = arg.Substring(separatorIndex + 1);
					} else {
						name = arg.Substring(startIndex);
						value = string.Empty;
					}
				}

				return longName ? (true, string.Empty, name, value) : (true, name, string.Empty, value);
			}
		}

		protected static string GetOptionName(string shortName, string longName) {
			return string.IsNullOrEmpty(longName) ? shortName : longName;
		}

		protected static string GetSeparatedOptionValue(IEnumerator<string> enumerator, string shortName, string longName) {
			// argument checks
			if (enumerator == null) {
				throw new ArgumentNullException(nameof(enumerator));
			}

			// get next argument
			if (enumerator.MoveNext() == false) {
				throw CreateInvalidOptionException(GetOptionName(shortName, longName), "Its value is missing.");
			}

			return enumerator.Current;
		}

		protected static (string version, string copyright) GetVersionInfoFrom(Assembly assembly) {
			// argument checks
			if (assembly == null) {
				throw new ArgumentNullException(nameof(assembly));
			}

			// get version
			string version;
			AssemblyFileVersionAttribute assemblyFileVersionAttribute = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
			if (assemblyFileVersionAttribute != null) {
				version = assemblyFileVersionAttribute.Version;
			} else {
				version = assembly.GetName().Version.ToString();
			}

			// get copyright
			AssemblyCopyrightAttribute copyrightAttribute = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
			string copyright = copyrightAttribute?.Copyright;

			return (version, copyright);
		}

		protected static void WriteVersion(TextWriter writer, string commandName, string version, string copyright) {
			// argument checks
			if (writer == null) {
				throw new ArgumentNullException(nameof(writer));
			}
			if (string.IsNullOrEmpty(commandName)) {
				throw new ArgumentNullException(nameof(commandName));
			}
			if (string.IsNullOrEmpty(version)) {
				throw new ArgumentNullException(nameof(version));
			}
			// copyright can be null or empty

			// write version information
			writer.WriteLine($"{commandName} {version}");
			if (string.IsNullOrEmpty(copyright) == false) {
				writer.WriteLine(copyright);
			}
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
			using (IEnumerator<string> enumerator = ((IEnumerable<string>)args).GetEnumerator()) {
				// Note that ProcessArgument() may proceed enumeration,
				// for example, it process value-divided options.
				while (enumerator.MoveNext()) {
					ProcessArgument(enumerator);
				}
			}
		}

		protected virtual void ProcessArgument(IEnumerator<string> args) {
			(bool option, string shortName, string longName, string value) = IsOption(args.Current);
			if (option) {
				ProcessOption(args, shortName, longName, value);
			} else {
				ProcessNormalArgument(args);
			}
		}

		protected virtual (bool option, string shortName, string longName, string value) IsOption(string arg) {
			return IsOption(arg, SeparatedOptionSeparator);
		}

		protected virtual void ProcessNormalArgument(IEnumerator<string> args) {
			// argument checks
			Debug.Assert(args != null);

			throw new Exception($"An unrecognized argument is specified: {args.Current}");
		}

		protected virtual void ProcessOption(IEnumerator<string> args, string shortName, string longName, string value) {
			// argument checks
			Debug.Assert(args != null);
			Debug.Assert(shortName != null);
			Debug.Assert(longName != null);
			Debug.Assert(string.IsNullOrEmpty(shortName) == false || string.IsNullOrEmpty(longName) == false);
			// value can be null

			if (AreSameOptionNames(shortName, "h") || AreSameOptionNames(shortName, "?") || AreSameOptionNames(longName, "help")) {
				this.taskKind = UsageTaskKind;
			} else if (AreSameOptionNames(shortName, "v") || AreSameOptionNames(longName, "-version")) {
				this.taskKind = VersionTaskKind;
			} else {
				throw new Exception($"An unrecognized option is specified: {GetOptionName(shortName, longName)}");
			}
		}

		protected virtual bool AreSameOptionNames(string name1, string name2) {
			// case-insensitive, by default
			return string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		#endregion


		#region overridables - command execution

		protected virtual string OnExecuting() {
			return this.taskKind;
		}

		protected virtual void Execute(string taskKind) {
			switch (taskKind) {
				case UsageTaskKind:
					ShowUsage();
					break;
				case VersionTaskKind:
					ShowVersion();
					break;
				default:
					throw new ArgumentException("Unrecognized value.", nameof(taskKind));
			}
		}

		protected virtual int OnExecuted(Exception error) {
			int exitCode;

			// handle the execution result
			if (error == null) {
				// execution succeeded
				exitCode = SuccessExitCode;
			} else {
				// execution failed
				WriteError(error.Message);
				exitCode = GeneralErrorExitCode;
			}

			return exitCode;
		}

		protected virtual void WriteError(string message) {
			// argument checks
			if (message == null) {
				message = string.Empty;
			}

			Console.Error.WriteLine(message);
		}

		protected virtual void ShowUsage() {
			WriteUsage(Console.Out);
		}

		protected virtual void WriteUsage(TextWriter writer) {
			// argument checks
			if (writer == null) {
				throw new ArgumentNullException(nameof(writer));
			}

			writer.WriteLine(this.Invoker);
		}

		protected virtual void ShowVersion() {
			(string version, string copyright) = GetVersionInfo();
			WriteVersion(Console.Out, this.Invoker, version, copyright);
		}

		protected virtual (string version, string copyright) GetVersionInfo() {
			return GetVersionInfoFrom(typeof(Command).Assembly);
		}

		#endregion
	}
}
