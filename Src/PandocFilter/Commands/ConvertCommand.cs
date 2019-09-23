using System;
using System.Collections.Generic;
using System.Diagnostics;
using PandocUtil.PandocFilter.Filters;

namespace PandocUtil.PandocFilter.Commands {
	public abstract class ConvertCommand: FilterCommand {
		#region properties

		protected new ConvertingFilter.Configurations Config {
			get {
				return GetConfiguration<ConvertingFilter.Configurations>();
			}
		}

		protected ConvertingFilter.Parameters Parameters {
			get {
				return this.Config.Parameters;
			}
		}

		#endregion


		#region constructors

		protected ConvertCommand(ConvertingFilter.Configurations config, string commandName, string invoker): base(config, commandName, invoker) {
		}

		#endregion


		#region overrides

		protected override void ProcessOption(IEnumerator<string> args, string shortName, string longName, string value) {
			// argument checks
			Debug.Assert(args != null);
			Debug.Assert(value == null);    // using value-separated option 

			if (AreSameOptionNames(shortName, "fd") || AreSameOptionNames(longName, ConvertingFilter.Parameters.Names.FromBaseDirPath)) {
				this.Parameters.FromBaseDirPath = GetSeparatedOptionValue(args, shortName, longName);
			} else if (AreSameOptionNames(shortName, "ff") || AreSameOptionNames(longName, ConvertingFilter.Parameters.Names.FromFileRelPath)) {
				this.Parameters.FromFileRelPath = GetSeparatedOptionValue(args, shortName, longName);
			} else if (AreSameOptionNames(shortName, "td") || AreSameOptionNames(longName, ConvertingFilter.Parameters.Names.ToBaseDirPath)) {
				this.Parameters.ToBaseDirPath = GetSeparatedOptionValue(args, shortName, longName);
			} else if (AreSameOptionNames(shortName, "tf") || AreSameOptionNames(longName, ConvertingFilter.Parameters.Names.ToFileRelPath)) {
				this.Parameters.ToFileRelPath = GetSeparatedOptionValue(args, shortName, longName);
			} else {
				base.ProcessOption(args, shortName, longName, value);
			}
		}

		#endregion
	}
}
