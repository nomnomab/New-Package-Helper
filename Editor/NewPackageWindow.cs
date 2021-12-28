using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Nomnom.NewPackageHelper.Editor {
	internal sealed class NewPackageWindow: EditorWindow {
		private static readonly string[] PACKAGE_TYPES = {
			"tests",
			"sample",
			"template",
			"module",
			"library",
			"tool"
		};

		private static readonly string[] TOOLBAR_TITLES = {
			"Package Info",
			"Package Layout"
		};
		
		private PackageJson _package;
		private PackageLayout _layout;
		private Vector2 _scroll;
		private int _toolbarIndex;

		[MenuItem("Tools/Nomnom/New Local Package - Packages Folder")]
		private static void OpenLocal() {
			GetWindow<NewPackageWindow>().Close();
			
			NewPackageWindow window = CreateInstance<NewPackageWindow>();
			window.titleContent = new GUIContent("New Package");
			window.ShowUtility();
		}

		private void OnEnable() {
			_package = PackageJson.Default();
			_layout = PackageLayout.Default();
		}

		private void OnGUI() {
			_scroll = EditorGUILayout.BeginScrollView(_scroll);

			_toolbarIndex = GUILayout.Toolbar(_toolbarIndex, TOOLBAR_TITLES);
			
			switch (_toolbarIndex) {
				case 0: DrawPackageEditor();
					break;
				case 1: DrawStructureEditor();
					break;
			}

			EditorGUILayout.EndScrollView();

			EditorGUILayout.BeginHorizontal();
			{
				string path = Path.Combine(Application.dataPath, $"../Packages/{_package.name}");
				bool validName = !string.IsNullOrEmpty(_package.name) && !Directory.Exists(path);
				bool validVersion = !string.IsNullOrEmpty(_package.version);
				bool validDisplayName = !string.IsNullOrEmpty(_package.displayName);
				bool validUnityVersion = !string.IsNullOrEmpty(_package.unity);
				bool validUnityRelease = !string.IsNullOrEmpty(_package.unityRelease);
				bool noCreate = !validName || !validVersion || !validDisplayName || !validUnityVersion || !validUnityRelease;
				
				GUI.backgroundColor = noCreate ? Color.red : Color.green;
				GUI.enabled = !noCreate;
				if (GUILayout.Button("Create")) {
					GeneratePackage();
					Close();
				}
				GUI.enabled = true;

				GUI.backgroundColor = Color.white;
				if (GUILayout.Button("Cancel")) {
					Close();
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawPackageEditor() {
      EditorGUILayout.BeginHorizontal();
      {
        EditorGUILayout.BeginVertical();
        {
          string path = Path.Combine(Application.dataPath, $"../Packages/{_package.name}");
          bool nameAlreadyExists = Directory.Exists(path);
          bool validName = !string.IsNullOrEmpty(_package.name) && !nameAlreadyExists;
          bool validVersion = !string.IsNullOrEmpty(_package.version);
          bool validDisplayName = !string.IsNullOrEmpty(_package.displayName);
          bool validUnityVersion = !string.IsNullOrEmpty(_package.unity);
          bool validUnityRelease = !string.IsNullOrEmpty(_package.unityRelease);

          EditorGUILayout.HelpBox("Anything unfilled will be ignored", MessageType.Info);

          if (nameAlreadyExists) {
            EditorGUILayout.HelpBox("This package seems to already exist in the Packages folder", MessageType.Error);
          }
          GUI.color = !validName ? Color.red : Color.white;
          _package.name = EditorGUILayout.TextField("Name*", _package.name);
          GUI.color = !validVersion ? Color.red : Color.white;
          _package.version = EditorGUILayout.TextField("Version*", _package.version);
          GUI.color = !validDisplayName ? Color.red : Color.white;
          _package.displayName = EditorGUILayout.TextField("DisplayName*", _package.displayName);
          GUI.color = Color.white;
          EditorGUILayout.PrefixLabel("Description");
          _package.description = EditorGUILayout.TextArea(_package.description);
          EditorGUILayout.BeginHorizontal();
          {
            GUI.color = !validUnityVersion ? Color.red : Color.white;
            _package.unity = EditorGUILayout.TextField("Unity Version*", _package.unity);
            GUI.color = !validUnityRelease ? Color.red : Color.white;
            _package.unityRelease = EditorGUILayout.TextField(_package.unityRelease);
            GUI.color = Color.white;
          }
          EditorGUILayout.EndHorizontal();
          _package.documentationUrl = EditorGUILayout.TextField("Documentation Url", _package.documentationUrl);
          _package.changelogUrl = EditorGUILayout.TextField("Changelog Url", _package.changelogUrl);
          _package.licenseType = EditorGUILayout.Popup("License Type*", _package.licenseType, LicenseType.Keys);

          if (_package.licenseType == 1) {
	          EditorGUI.indentLevel++;
	          _package.licensesUrl = EditorGUILayout.TextField("Licenses Url", _package.licensesUrl);
	          EditorGUI.indentLevel--;
          }

          // dependencies
          EditorGUILayout.BeginHorizontal();
          {
            EditorGUILayout.LabelField("Dependencies", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("+", GUILayout.Width(20))) {
              _package.dependencies.Add(new PackageDependency {
                key = "com.company.name",
                value = "1.0.0"
              });
            }
          }
          EditorGUILayout.EndHorizontal();

          if (_package.dependencies.Count != 0) {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(18 * _package.dependencies.Count), GUILayout.ExpandHeight(false));
            GUILayout.Space(4);
            EditorGUILayout.BeginVertical("Box");
            {
              for (int i = 0; i < _package.dependencies.Count; i++) {
                var dependency = _package.dependencies[i];
                EditorGUILayout.BeginHorizontal();
                {
                  dependency.key = EditorGUILayout.TextField(dependency.key);
                  dependency.value = EditorGUILayout.TextField(dependency.value);

                  GUI.backgroundColor = Color.red;
                  if (GUILayout.Button("x", GUILayout.Width(20))) {
                    _package.dependencies.RemoveAt(i);
                    break;
                  }
                  GUI.backgroundColor = Color.white;
                }
                EditorGUILayout.EndHorizontal();

                _package.dependencies[i] = dependency;
              }
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
          }

          // keywords
          EditorGUILayout.BeginHorizontal();
          {
            EditorGUILayout.LabelField("Keywords", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("+", GUILayout.Width(20))) {
              _package.keywords.Add("");
            }
          }
          EditorGUILayout.EndHorizontal();

          if (_package.keywords.Count != 0) {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(18 * _package.dependencies.Count), GUILayout.ExpandHeight(false));
            {
              GUILayout.Space(4);
              EditorGUILayout.BeginVertical("Box");
              {
                for (int i = 0; i < _package.keywords.Count; i++) {
                  var keyword = _package.keywords[i];
                  EditorGUILayout.BeginHorizontal();
                  {
                    keyword = EditorGUILayout.TextField(keyword);

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("x", GUILayout.Width(20))) {
                      _package.keywords.RemoveAt(i);
                      break;
                    }

                    GUI.backgroundColor = Color.white;
                  }
                  EditorGUILayout.EndHorizontal();

                  _package.keywords[i] = keyword;
                }
              }
              EditorGUILayout.EndVertical();
              GUILayout.Space(4);
            }
            EditorGUILayout.EndHorizontal();
          }

          // author
          EditorGUILayout.PrefixLabel("Author");
          EditorGUILayout.BeginHorizontal();
          {
            GUILayout.Space(4);
            EditorGUILayout.BeginVertical("Wizard Box", GUILayout.Height(18 * 3));
            {
              _package.author.name = EditorGUILayout.TextField("Name", _package.author.name);
              _package.author.email = EditorGUILayout.TextField("Email", _package.author.email);
              _package.author.url = EditorGUILayout.TextField("Url", _package.author.url);
            }
            EditorGUILayout.EndVertical();
            GUILayout.Space(4);
          }
          EditorGUILayout.EndHorizontal();

          // type
          _package.type = EditorGUILayout.Popup("Package Type*", _package.type, PACKAGE_TYPES);
        }
        EditorGUILayout.EndVertical();

        GUI.enabled = false;
        EditorGUILayout.TextArea(GenerateJson().ToString(Formatting.Indented), GUILayout.Width(Screen.width * 0.35f), GUILayout.Height(Screen.height - 60));
        GUI.enabled = true;
      }
      EditorGUILayout.EndHorizontal();
    }

		private void DrawStructureEditor() {
			var root = _layout.Root;

			EditorGUILayout.PrefixLabel("File structure");
			drawRecursive(root, root.Enabled);
			
			void drawRecursive(PackageLayout.Item item, bool enabled) {
				if (item != root) {
					EditorGUILayout.BeginHorizontal();
					{
						GUI.enabled = !item.Required;
						item.Enabled = EditorGUILayout.Toggle(GUIContent.none, item.Enabled, GUILayout.Width(20));
						GUI.enabled = item.Enabled && enabled;
						EditorGUILayout.LabelField(item.Content);
						GUI.enabled = true;
					}
					EditorGUILayout.EndHorizontal();
				}

				EditorGUI.indentLevel++;
				foreach (PackageLayout.Item child in item.Items) {
					drawRecursive(child, item.Enabled);
				}
				EditorGUI.indentLevel--;
			}
		}

    private JObject GenerateJson() {
      // generate json
      JObject json = new JObject {
        ["name"] = _package.name,
        ["version"] = _package.version,
        ["displayName"] = _package.displayName,
        ["description"] = _package.description,
        ["unity"] = _package.unity,
        ["unityRelease"] = _package.unityRelease
      };

      if (!string.IsNullOrEmpty(_package.documentationUrl)) {
        json["documentationUrl"] = _package.documentationUrl;
      }

      if (!string.IsNullOrEmpty(_package.changelogUrl)) {
        json["changelogUrl"] = _package.changelogUrl;
      }

      if (_package.licenseType != 0) {
	      if (_package.licenseType == 0) {
		      // none
		      json["licensesUrl"] = string.Empty;
	      } else if (_package.licenseType == 1) {
		      // prop
		      json["licensesUrl"] = _package.licensesUrl;
	      } else {
		      json["licensesUrl"] = $"https://choosealicense.com/licenses/{LicenseType.Cache[LicenseType.Keys[_package.licenseType]]}/";
	      }
      }

      json["dependencies"] = new JObject();
      json["devDependencies"] = new JObject();
      json["samples"] = new JArray();

      if (_package.dependencies.Count > 0) {
        JObject dependencies = new JObject();
        foreach (PackageDependency dependency in _package.dependencies) {
          dependencies[dependency.key] = dependency.value;
        }
        json["dependencies"] = dependencies;
      }

      if (_package.keywords.Count > 0) {
        JArray keywords = new JArray();
        foreach (string keyword in _package.keywords) {
          keywords.Add(keyword);
        }
        json["keywords"] = keywords;
      }

      JObject authorObj = new JObject();
      authorObj["name"] = _package.author.name;
      authorObj["email"] = _package.author.email;
      authorObj["url"] = _package.author.url;

      json["author"] = authorObj;
      json["type"] = PACKAGE_TYPES[_package.type];

      return json;
    }

		private void GeneratePackage() {
      JObject json = GenerateJson();

      // Debug.Log(json.ToString(Formatting.Indented));

      // handle file/folder structure
      string path = Path.Combine(Application.dataPath, $"../Packages/{_package.name}");
			Directory.CreateDirectory(path);
			_layout.Root.HandleNew(_package, path);
			
			File.WriteAllText($"{path}/package.json", json.ToString(Formatting.Indented));
			EditorUtility.RevealInFinder(path);
		}
	}
}