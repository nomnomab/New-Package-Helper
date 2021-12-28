﻿using System.Collections.Generic;
using System.Linq;

namespace Nomnom.NewPackageHelper.Editor {
	internal static class LicenseType {
		public const string DEFAULT_LICENSE = "None";

		public static Dictionary<string, string> Cache = new Dictionary<string, string> {
      {"None", null},
      {"Proprietary", null},
      {"Other", null},
			{"Academic Free License v3.0", "afl-3.0"},
			{"Apache license 2.0", "apache-2.0"},
			{"Artistic license 2.0", "artistic-2.0"},
			{"Boost Software License 1.0", "bsl-1.0"},
			{"BSD 2-clause \"Simplified\" license", "bsd-2-clause"},
			{"BSD 3-clause \"New\" or \"Revised\" license", "bsd-3-clause"},
			{"BSD 3-clause Clear license", "bsd-3-clause-clear"},
			{"Creative Commons license family", "cc"},
			{"Creative Commons Zero v1.0 Universal", "cc0-1.0"},
			{"Creative Commons Attribution 4.0", "cc-by-4.0"},
			{"Creative Commons Attribution Share Alike 4.0", "cc-by-sa-4.0"},
			{"Do What The F*ck You Want To Public License", "wtfpl"},
			{"Educational Community License v2.0", "ecl-2.0"},
			{"Eclipse Public License 1.0", "epl-1.0"},
			{"Eclipse Public License 2.0", "epl-2.0"},
			{"European Union Public License 1.1", "eupl-1.1"},
			{"GNU Affero General Public License v3.0", "agpl-3.0"},
			{"GNU General Public License family", "gpl"},
			{"GNU General Public License v2.0", "gpl-2.0"},
			{"GNU General Public License v3.0", "gpl-3.0"},
			{"GNU Lesser General Public License family", "lgpl"},
			{"GNU Lesser General Public License v2.1", "lgpl-2.1"},
			{"GNU Lesser General Public License v3.0", "lgpl-3.0"},
			{"ISC", "isc"},
			{"LaTeX Project Public License v1.3c", "lppl-1.3c"},
			{"Microsoft Public License", "ms-pl"},
			{"MIT", "mit"},
			{"Mozilla Public License 2.0", "mpl-2.0"},
			{"Open Software License 3.0", "osl-3.0"},
			{"PostgreSQL License", "postgresql"},
			{"SIL Open Font License 1.1", "ofl-1.1"},
			{"University of Illinois - NCSA Open Source License", "ncsa"},
			{"The Unlicense", "unlicense"},
			{"zLib License", "zlib"},
		};

		public static readonly string[] Keys = Cache.Keys.ToArray();

		public static int IndexOf(string key) {
			for (int i = 0; i < Keys.Length; i++) {
				string s = Keys[i];
				if (s == key) {
					return i;
				}
			}

			return -1;
		}
	}
}