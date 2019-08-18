using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PandocUtil.PandocFilter.Filters {
	public class AST: ReadOnlyAST {
		#region properties

		public new Dictionary<string, object> JsonValue {
			get {
				return this.jsonValue;
			}
		}

		#endregion


		#region creation

		public AST(Dictionary<string, object> jsonValue): base(jsonValue) {
		}

		#endregion


		#region methods

		public IDictionary<string, object> GetMetadata(bool createIfNotExist) {
			IDictionary<string, object> ast = this.jsonValue;
			Debug.Assert(ast != null);

			IDictionary<string, object> metadata = ast.GetOptionalValue<IDictionary<string, object>>(Schema.Names.Meta, null);
			if (metadata == null && createIfNotExist) {
				metadata = new Dictionary<string, object>();
				ast[Schema.Names.Meta] = metadata;
			}

			return metadata;
		}

		#endregion
	}
}
