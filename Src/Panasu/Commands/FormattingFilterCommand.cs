using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Panasu.Filters;

namespace Panasu.Commands {
	public class FormattingFilterCommand: ConvertingFilterCommand {
		#region properties

		protected new FormattingFilter.Configurations Config {
			get {
				return GetConfiguration<FormattingFilter.Configurations>();
			}
		}

		protected new FormattingFilter.Parameters Parameters {
			get {
				return this.Config.Parameters;
			}
		}

		#endregion


		#region constructors

		protected FormattingFilterCommand(FormattingFilter.Configurations config, string commandName, string invoker): base(config, commandName, invoker) {
		}

		public FormattingFilterCommand(string commandName, string invoker = null): base(new FormattingFilter.Configurations(), commandName, invoker) {
		}

		#endregion


		#region overrides

		protected override void ProcessOption(IEnumerator<string> args, string shortName, string longName, string value) {
			// argument checks
			Debug.Assert(args != null);
			Debug.Assert(value == null);	// using value-separated option 

			if (AreSameOptionNames(shortName, "r") || AreSameOptionNames(longName, FormattingFilter.Parameters.Names.RebaseOtherRelativeLinks)) {
				this.Parameters.RebaseOtherRelativeLinks = true;	// TODO:
			} else if (AreSameOptionNames(shortName, "m") || AreSameOptionNames(longName, FormattingFilter.Parameters.Names.ExtensionMap)) {
				(string from, string to) SplitExtensions(string val) {
					int index = val.IndexOf(':');
					if (index < 0) {
						string name = string.IsNullOrEmpty(longName) ? shortName : longName;
						throw CreateInvalidOptionException(name, "Its form must be \"<from ext>:<to ext>\"\nex. \".md:.html\"");
					}
					string f = val.Substring(0, index);
					string t = val.Substring(index + 1);

					return (f, t);
				}

				(string from, string to) = SplitExtensions(GetSeparatedOptionValue(args, shortName, longName));
				this.Parameters.AddExtensionMap(from, to);
			} else {
				base.ProcessOption(args, shortName, longName, value);
			}
		}

		protected override string OnExecuting() {
			string command = base.OnExecuting();
			if (command != null) {
				// reserved command (such as usage or version)
				return command;
			}

			// state checks
			// nothing to do

			return FilterTaskKind;
		}

		protected override void WriteUsage(TextWriter writer) {
			// argument checks
			if (writer == null) {
				throw new ArgumentNullException(nameof(writer));
			}

			writer.WriteLine($"{this.Invoker}:");
			writer.WriteLine("  Transforms a pandoc AST.");
			writer.WriteLine("  It reads input from the standard input,");
			writer.WriteLine("  and writes output to the standard output.");
			writer.WriteLine();
			writer.WriteLine("Usage:");
			writer.WriteLine($"  {this.Invoker} [options]");
			writer.WriteLine();
			writer.WriteLine("Options:");
			writer.WriteLine("  -m, --ExtensionMap <fromExt>:<toExt>");
			writer.WriteLine("    Add additional extension map. This options is cumulative.");
			writer.WriteLine("    These extensions in relative links will be replaced.");
			writer.WriteLine("    ex. -m \".md:.html\"");
			writer.WriteLine("  -fd, --FromBaseDirPath <dir>");
			writer.WriteLine("    The path of base directory for original source files.");
			writer.WriteLine("  -ff, --FromFileRelPath <relativePath>");
			writer.WriteLine("    The relative path of original source file.");
			writer.WriteLine("    The path must be relative from FromBaseDirPath.");
			writer.WriteLine("  -h, -?, --Help");
			writer.WriteLine("    Shows the help of this command.");
			writer.WriteLine("  --RebaseOtherRelativeLinks");
			writer.WriteLine("    Rebases relative links which are not included in the ExtensionMap.");
			writer.WriteLine("  -td, --ToBaseDirPath <dir>");
			writer.WriteLine("    The path of base directory for final output files.");
			writer.WriteLine("  -tf, --ToFileRelPath <relativePath>");
			writer.WriteLine("    The relative path of final output file.");
			writer.WriteLine("    The path must be relative from ToBaseDirPath.");
			writer.WriteLine("  -v, --Version");
			writer.WriteLine("    Shows the version of this command.");
			writer.WriteLine();
		}

		protected override (Filter filter, bool useGenerate) CreateFilter() {
			// clone the configuration
			FormattingFilter.Configurations config = new FormattingFilter.Configurations(this.Config);
			config.CompleteContents();
			config.Freeze();

			// create a formatting filter, and specify to use Modify method instead of Generate method
			return (new FormattingFilter(config), false);
		}

		#endregion
	}
}
