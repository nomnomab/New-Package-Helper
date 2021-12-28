using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nomnom.NewPackageHelper.Editor {
	internal class PackageLayout {
		public Item Root;

		public PackageLayout() {
			Root = new Item("Root", true, true);
		}

		public static PackageLayout Default() {
			PackageLayout layout = new PackageLayout();
			
			layout.Root.Items.Add(new Item("package.json", true, true));
			layout.Root.Items.Add(new Item("README.md", true));
			layout.Root.Items.Add(new Item("CHANGELOG.md", false));
			layout.Root.Items.Add(new Item("LICENSE.md", true));
			layout.Root.Items.Add(new Item("Third Party Notices.md", false));

			var editorItem = new Item("Editor", false);
			editorItem.Items.Add(new Item(".asmdef", true));
			layout.Root.Items.Add(editorItem);
			
			var runtimeItem = new Item("Runtime", true);
			runtimeItem.Items.Add(new Item(".asmdef", true));
			layout.Root.Items.Add(runtimeItem);
			
			var testsItem = new Item("Tests", false);
			testsItem.Items.Add(new Item(".asmdef", true));
			layout.Root.Items.Add(testsItem);
			
			var documentationItem = new Item("Documentation~", false);
			documentationItem.Items.Add(new Item(".md", true));
			layout.Root.Items.Add(documentationItem);
			
			return layout;
		}

		public class Item {
			public string Content;
			public bool Enabled;
			public bool Required;
			public List<Item> Items;

			public Item(string content, bool enabled, bool required = false) {
				Content = content;
				Enabled = enabled;
				Required = required;
				Items = new List<Item>();
			}

			public void HandleNew(PackageJson package, string folder) {
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