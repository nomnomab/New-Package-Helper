using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nomnom.NewPackageHelper.Editor {
	[Serializable]
	internal struct PackageJson {
		public const string PREFS_AUTHOR = "com.nomnom.new-package-helper-last_author";
		public const string PREFS_EMAIL = "com.nomnom.new-package-helper-last_email";
		public const string PREFS_URL = "com.nomnom.new-package-helper-last_url";
		public const string PREFS_LICENSE = "com.nomnom.new-package-helper-last_license";
		
		public string name;
		public string version;
		public string displayName;
		public string description;
		public string unity;
		public string unityRelease;
		// optional
		public string documentationUrl;
		// optional
		public string changelogUrl;
		// default: https://choosealicense.com/licenses/<INSERT>/
		public string licensesUrl;
		public List<PackageDependency> dependencies;
		public List<string> keywords;
		public PackageAuthor author;
		// default: library
		public int type;
		public int licenseType;

		public static PackageJson Default() {
			string version = Application.unityVersion;
			string mainVersion = version.Substring(0, version.LastIndexOf('.'));
			string release = version.Substring(mainVersion.Length + 1);

			return new PackageJson {
				name = "com.company.name",
				version = "1.0.0",
				displayName = "Name",
				description = "Description",
				unity = mainVersion,
				unityRelease = release,
				dependencies = new List<PackageDependency>(),
				keywords = new List<string>(),
				author = new PackageAuthor {
					name = EditorPrefs.GetString(PREFS_AUTHOR, "Author"),
					email = EditorPrefs.GetString(PREFS_EMAIL, string.Empty),
					url = EditorPrefs.GetString(PREFS_URL, string.Empty)
				},
				type = 4,
				licenseType = EditorPrefs.GetInt(PREFS_LICENSE, LicenseType.IndexOf(LicenseType.DEFAULT_LICENSE)),
        licensesUrl = "LICENSE.md"
			};
		}
	}

	[System.Serializable]
	internal struct PackageDependency {
		public string key;
		public string value;
	}
	
	[System.Serializable]
	internal struct PackageAuthor {
		public string name;
		public string email;
		public string url;
	}
}