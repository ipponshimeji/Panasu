using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PandocUtil.PandocFilter.Commands.Test {
	public class ExtensionChangerCommandTest {
		#region types

		// The adapter class to access protected members of ExtensionChangerCommand class.
		public class ExtensionChangerCommandAdapter: ExtensionChangerCommand {
			#region properties

			public new string InputFilePath {
				get {
					return base.InputFilePath;
				}
			}

			public new string OutputFilePath {
				get {
					return base.OutputFilePath;
				}
			}

			public new bool RebaseOtherRelativeLink {
				get {
					return base.RebaseOtherRelativeLink;
				}
			}

			#endregion


			#region constructors

			public ExtensionChangerCommandAdapter(): base() {
			}

			#endregion
		}

		#endregion


		public class Foo {
			[Fact]
			public void Test() {

			}
		}
	}
}
