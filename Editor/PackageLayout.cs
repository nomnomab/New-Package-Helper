using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Nomnom.NewPackageHelper.Editor {
	internal class PackageLayout {
		public const string PREFS_BASE = "com.nomnom.new-package-helper.";
		public const string PREFS_README = PREFS_BASE + "README.md";
		public const string PREFS_CHANGELOG = PREFS_BASE + "CHANGELOG.md";
		public const string PREFS_LICENSE = PREFS_BASE + "LICENSE.md";
		public const string PREFS_THIRD_PARTY = PREFS_BASE + "Third Party Notices.md";
		public const string PREFS_EDITOR = PREFS_BASE + "Editor";
		public const string PREFS_EDITOR_ASMDEF = PREFS_BASE + "Editor.asmdef";
		public const string PREFS_RUNTIME = PREFS_BASE + "Runtime";
		public const string PREFS_RUNTIME_ASMDEF = PREFS_BASE + "Runtime.asmdef";
		public const string PREFS_TESTS = PREFS_BASE + "Tests";
		public const string PREFS_TESTS_ASMDEF = PREFS_BASE + "Tests.asmdef";
		public const string PREFS_DOCUMENTATION = PREFS_BASE + "Documentation~";
		public const string PREFS_DOCUMENTATION_MD = PREFS_BASE + "Documentation.md";
		
		public Item Root;

		public PackageLayout() {
			Root = new Item("Root", null, true, true);
		}

		public static PackageLayout Default() {
			PackageLayout layout = new PackageLayout();
			
			layout.Root.Items.Add(new Item("package.json", null, true, true));
			layout.Root.Items.Add(new Item("README.md", PREFS_README, EditorPrefs.GetBool(PREFS_README, true)));
			layout.Root.Items.Add(new Item("CHANGELOG.md", PREFS_CHANGELOG, EditorPrefs.GetBool(PREFS_CHANGELOG, false)));
			layout.Root.Items.Add(new Item("LICENSE.md", PREFS_LICENSE, EditorPrefs.GetBool(PREFS_LICENSE, true)));
			layout.Root.Items.Add(new Item("Third Party Notices.md", PREFS_THIRD_PARTY, EditorPrefs.GetBool(PREFS_THIRD_PARTY, false)));

			var editorItem = new Item("Editor", PREFS_EDITOR, EditorPrefs.GetBool(PREFS_EDITOR, false));
			editorItem.Items.Add(new Item(".asmdef", PREFS_EDITOR_ASMDEF, EditorPrefs.GetBool(PREFS_EDITOR_ASMDEF, true)));
			layout.Root.Items.Add(editorItem);
			
			var runtimeItem = new Item("Runtime", PREFS_RUNTIME, EditorPrefs.GetBool(PREFS_RUNTIME, true));
			runtimeItem.Items.Add(new Item(".asmdef", PREFS_RUNTIME_ASMDEF, EditorPrefs.GetBool(PREFS_RUNTIME_ASMDEF, true)));
			layout.Root.Items.Add(runtimeItem);
			
			var testsItem = new Item("Tests", PREFS_TESTS, EditorPrefs.GetBool(PREFS_TESTS, false));
			testsItem.Items.Add(new Item(".asmdef", PREFS_TESTS_ASMDEF, EditorPrefs.GetBool(PREFS_TESTS_ASMDEF, true)));
			layout.Root.Items.Add(testsItem);
			
			var documentationItem = new Item("Documentation~", PREFS_DOCUMENTATION, EditorPrefs.GetBool(PREFS_DOCUMENTATION, false));
			documentationItem.Items.Add(new Item(".md", PREFS_DOCUMENTATION_MD, EditorPrefs.GetBool(PREFS_DOCUMENTATION_MD, true)));
			layout.Root.Items.Add(documentationItem);
			
			return layout;
		}

		public class Item {
			public string Content;
			public string PrefsName;
			public bool Enabled;
			public bool Required;
			public List<Item> Items;

			public Item(string content, string prefsName, bool enabled, bool required = false) {
				Content = content;
				PrefsName = prefsName;
				Enabled = enabled;
				Required = required;
				Items = new List<Item>();
			}

			public void HandleNew(PackageJson package, string folder) {
				if (!string.IsNullOrEmpty(PrefsName)) {
					EditorPrefs.SetBool(PrefsName, Enabled);
				}
				
				if (!Enabled) {
					return;
				}

				bool isRoot = Content == "Root";
				if (!isRoot) {
					if (Path.HasExtension(Content)) {
						// file
						string extension = Path.GetExtension(Content);
						switch (extension) {
							case ".asmdef":
								string parent = Path.GetFileName(folder);
								string path = $"{folder}/{package.name}.{parent}.asmdef";
								using (StreamWriter writer = File.CreateText(path)) {
									JObject asmdefObj = new JObject {
										["name"] = $"{package.name}.{parent}",
										["rootNamespace"] = "",
										["references"] = new JArray(),
										["includePlatforms"] = folder.EndsWith("Editor") 
											? new JArray {
												"Editor"
											} 
											: new JArray(),
										["excludePlatforms"] = new JArray(),
										["allowUnsafeCode"] = false,
										["overrideReferences"] = false,
										["precompiledReferences"] = new JArray(),
										["autoReferenced"] = true,
										["defineConstraints"] = new JArray(),
										["versionDefines"] = new JArray(),
										["noEngineReferences"] = false
									};

									writer.Write(asmdefObj.ToString(Formatting.Indented));
								}
								break;
							case ".md":
								if (Path.GetFileNameWithoutExtension(Content) == "LICENSE" && string.IsNullOrEmpty(package.licensesUrl)) {
									// aquire license data
									try {
										using (WebClient wc = new WebClient()) {
											string url =
												$"https://choosealicense.com/licenses/{LicenseType.Cache[LicenseType.Keys[package.licenseType]]}/";
											string json = wc.DownloadString(url);
											string[] splitHeader = json.Split(new[] {"<pre id=\"license-text\">"},
												StringSplitOptions.RemoveEmptyEntries);
											string[] splitFinal =
												splitHeader[1].Split(new[] {"</pre>"}, StringSplitOptions.RemoveEmptyEntries);
											string licenseText = splitFinal[0];
											using (StreamWriter writer = File.CreateText($"{folder}/{Content}")) {
												writer.Write(licenseText);
											}
										}
									}
									catch (Exception e) {
										Debug.LogError("Could not auto-get license from https://choosealicense.com for some reason. Sorry.");
										Debug.LogException(e);
									}
								} else {
									File.CreateText($"{folder}/{Content}");	
								}
								break;
						}
					} else {
						// folder
						Directory.CreateDirectory($"{folder}/{Content}");
					}
				}

				foreach (Item item in Items) {
					item.HandleNew(package, $"{folder}/{(isRoot ? string.Empty : Content)}");
				}
			}
		}
	}
}