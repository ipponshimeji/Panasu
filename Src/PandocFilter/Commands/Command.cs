using System;
using System.Diagnostics;
using System.Reflection;

namespace PandocUtil.PandocFilter.Commands {
	public class Command {
		#region constants

		// standard exit codes
		public const int SuccessExitCode = 0;
		public const int GeneralErrorExitCode = 1;

		// reserved task kind
		public const string UsageTaskKind = "_Usage";
		public const string VersionTaskKind = "_Version";

		#endregion


		#region data

		public string CommandName { get; private set; }

		private string invocation = null;

		private string taskKind = null;

		#endregion


		#region properties

		public string Invocation {
			get {
				return invocation ?? this.CommandName;
			}
		}

		#endregion


		#region constructors

		public Command(string commandName, string invocation = null) {
			// argument checks
			if (commandName == null) {
				throw new ArgumentNullException(nameof(commandName));
			}
			// invocationCommand can be null

			// initialize members
			this.CommandName = commandName;
			this.invocation = invocation;
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


		protected static InvalidOperationException CreateMissingIndispensableArgumentException(string argName) {
			// argument checks
			if (string.IsNullOrEmpty(argName)) {
				argName = "(unknown)";
			}

			throw new InvalidOperationException($"The indispensable argument '{argName}' is missing.");
		}

		protected static (string version, string copyright) GetVersionInfo(Assembly assembly) {
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
			throw new Exception($"An unrecognized argument is specified: {arg.Value}");
		}

		protected virtual void ProcessOption(Argument arg) {
			// argument checks
			Debug.Assert(arg.IsOption);

			string name = arg.Name;
			if (AreSameOptionNames(name, "help")) {
				this.taskKind = UsageTaskKind;
			} else if (AreSameOptionNames(name, "version")) {
				this.taskKind = VersionTaskKind;
			} else {
				throw new Exception($"An unrecognized option is specified: {arg.Name}");
			}
		}

		protected virtual bool AreSameOptionNames(string name1, string name2) {
			// case-insensitive, by default
			return string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		protected virtual bool OptionNameStartsWith(string name, string str) {
			// argument checks
			if (name == null) {
				throw new ArgumentNullException(nameof(name));
			}

			// case-insensitive, by default
			return name.StartsWith(str, StringComparison.OrdinalIgnoreCase);
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
			return (error == null)? SuccessExitCode: GeneralErrorExitCode;
		}

		protected virtual void ShowUsage() {
			Console.WriteLine($"{this.CommandName}");
		}

		protected virtual void ShowVersion() {
			// get version information
			(string version, string copyright) = GetVersionInfo();
			Debug.Assert(version != null);
			// copyright can be null

			// write version information
			Console.WriteLine($"{this.CommandName} {version}");
			if (string.IsNullOrEmpty(copyright) == false) {
				Console.WriteLine(copyright);
			}
		}

		protected virtual (string version, string copyright) GetVersionInfo() {
			return GetVersionInfo(typeof(Command).Assembly);
		}

		#endregion
	}
}
