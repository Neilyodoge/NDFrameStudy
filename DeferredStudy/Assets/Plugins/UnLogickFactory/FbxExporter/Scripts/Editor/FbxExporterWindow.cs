#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UnLogickFactory
{
	public class FbxExporterWindow : EditorWindow
	{
		[MenuItem("Window/Unity Fbx Exporter")]
		public static void ShowFbxExporterWindow()
		{
			var window = EditorWindow.GetWindow<FbxExporterWindow>();
			window.titleContent = new GUIContent("Fbx Exporter");
		}

		string[] formatNames;
		GUIContent[] formatContent;
		GUIContent[] terrainQualityOptions =
		{
			new GUIContent("Auto", "Best fit within the 64k of limit of Unity mesh."),
			new GUIContent("Full resolution", "Warning a 512 terrain turns into half a million triangles!"),
			new GUIContent("Half resolution", "Warning a 1k terrain turns into half a million triangles!"),
			new GUIContent("Quarter resolution", "Warning a 2k terrain turns into half a million triangles!"),
			new GUIContent("1:8 resolution", "Warning a 4k terrain turns into half a million triangles!"),
			new GUIContent("1:16 resolution", ""),
			new GUIContent("1:32 resolution", ""),
			new GUIContent("1:64 resolution", ""),
			new GUIContent("1:128 resolution", ""),
		};
		string[] qualityHints =
		{
			"",
			"<color=red><b>Warning a 512 terrain turns into half a million triangles!</b></color>",
			"<color=red><b>Warning a 1k terrain turns into half a million triangles!</b></color>",
			"<color=red><b>Warning a 2k terrain turns into half a million triangles!</b></color>",
			"<color=red><b>Warning a 4k terrain turns into half a million triangles!</b></color>",
			"",
			"",
			"",
			"",
		};

		GUIContent[] logLevelOptions =
		{
			new GUIContent("Minimal", "Logs when exports begins, completes as well as errors."),
			new GUIContent("Normal", "Outputs which meshes was exported and which was culled!"),
			new GUIContent("Verboose", "Outputs LOD, culling and bones.")
		};

		int[] formatIds;
		int selectedFormat = -1;
		string copyrightNotice;
		int terrainSampling;
		int logLevel;
		bool hasSelection;
		bool embedTextures;
		bool embedShaderProperty;

		GUIStyle guiStyle;
		GUIStyle guiStyleSmall;
		GUIStyle guiStyleAbout;
		Vector2 scrollPosition;
		FbxLODScheme.LODScheme lodScheme;
		FbxTextureExportScheme textureScheme;
		SkinnedMeshOptions skinnedMeshOptions;
		ObjectExportMask objectExportMask;
		BlendShapeOptions blendShapeOptions;

		void OnEnable()
		{
			copyrightNotice = "Version " + FbxExporter.Version + ", \u00A9 2016-2020 UnLogick Factory";

			terrainSampling = EditorPrefs.GetInt("UnLogickFactory_FbxExporter_TerrainSamplingRate", 0);
			lodScheme = (FbxLODScheme.LODScheme)EditorPrefs.GetInt("UnLogickFactory_FbxExporter_LODScheme", 0);
			textureScheme = FbxTextureExportScheme.GetDefaultScheme();
			logLevel = EditorPrefs.GetInt("UnLogickFactory_FbxExporter_LogLevel", 0);
			skinnedMeshOptions = (SkinnedMeshOptions)EditorPrefs.GetInt("UnLogickFactory_FbxExporter_SkinnedMeshOptions2", (int)SkinnedMeshOptions.ExportCurrentPose);
			if ((int)skinnedMeshOptions > 1) skinnedMeshOptions = SkinnedMeshOptions.ExportTPose;
			blendShapeOptions = (BlendShapeOptions)EditorPrefs.GetInt("UnLogickFactory_FbxExporter_BlendShapeOptions", (int)BlendShapeOptions.Reset);
			objectExportMask = (ObjectExportMask)EditorPrefs.GetInt("UnLogickFactory_FbxExporter_ObjectExportMask", (int)ObjectExportMask.ExportAll);
			embedTextures = EditorPrefs.GetBool("UnLogickFactory_FbxExporter_EmbedTextures", false);
			embedShaderProperty = EditorPrefs.GetBool("UnLogickFactory_FbxExporter_EmbedShaderProperty", false);

			GUISkin skin = EditorGUIUtility.GetBuiltinSkin(Application.HasProLicense() ? EditorSkin.Game : EditorSkin.Inspector);
			
			guiStyleAbout = new GUIStyle(skin.label);
			guiStyleAbout.normal.textColor = skin.label.normal.textColor;
			guiStyleAbout.fontSize = 11;
			guiStyleAbout.padding = new RectOffset(6, 0, 6, 6);
			guiStyleAbout.wordWrap = true;
			guiStyleAbout.richText = true;
			guiStyleAbout.alignment = TextAnchor.MiddleLeft;

			guiStyle = new GUIStyle(skin.label);
			guiStyle.normal.textColor = skin.label.normal.textColor;
			guiStyle.fontSize = 20;
			guiStyle.padding = new RectOffset(6, 6, 6, 0);
			guiStyle.richText = true;
			guiStyle.alignment = TextAnchor.MiddleLeft;

			guiStyleSmall = new GUIStyle(skin.label);
			guiStyleSmall.normal.textColor = skin.label.normal.textColor;
			guiStyleSmall.fontSize = 11;
			guiStyleSmall.padding = new RectOffset(6, 0, 6, 6);
			guiStyleSmall.richText = true;
			guiStyleSmall.alignment = TextAnchor.MiddleLeft;

			if (!FbxExporter.FbxSupported)
				return;

			FbxExporter.GetFormatNames(true, out formatNames, out formatIds);
			selectedFormat = -1;
			formatContent = new GUIContent[formatNames.Length];
			for (int i = 0; i < formatNames.Length; i++)
			{
				formatContent[i] = new GUIContent(formatNames[i]);
				if (formatNames[i] == EditorPrefs.GetString("UnLogickFactory_SelectedFbxFormat"))
					selectedFormat = i;
			}
			if (selectedFormat == -1)
			{
				selectedFormat = FbxExporter.GetDefaultFormat();
				EditorPrefs.SetString("UnLogickFactory_SelectedFbxFormat", formatNames[0]);
			}
		}

		void OnSelectionChange()
		{
			Repaint();
		}

		void OnGUI()
		{
			hasSelection = Selection.gameObjects.Length > 0;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			EditorGUILayout.Space();
			GUILayout.Label("Unity Fbx Exporter", guiStyle);
			GUILayout.Label(copyrightNotice, guiStyleSmall);
			EditorGUILayout.Space();

			if (FbxExporter.FbxSupported)
			{
				GUILayout.Label("<b>General Export Options</b>", guiStyleSmall);
				EditorGUI.indentLevel++;
				var newFbxFormat = EditorGUILayout.IntPopup(new GUIContent("Exported Fbx Format", "This list is populated by the fbx sdk and the available formats may vary from system to system."), selectedFormat, formatContent, formatIds);
				if (selectedFormat != newFbxFormat)
				{
					selectedFormat = newFbxFormat;
					for (int i = 0; i < formatIds.Length; i++)
					{
						if (formatIds[i] == newFbxFormat)
						{
							EditorPrefs.SetString("UnLogickFactory_SelectedFbxFormat", formatNames[i]);
							break;
						}
					}
				}
				var newSampling = EditorGUILayout.Popup(new GUIContent("Terrain Quality", "Specify the quality of terrain export, if you don't subsample a 1k terrain it turns into a 2 million triangle mesh!"), terrainSampling, terrainQualityOptions);
				if (newSampling != terrainSampling)
				{
					terrainSampling = newSampling;
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_TerrainSamplingRate", terrainSampling);
				}
				if (!string.IsNullOrEmpty(qualityHints[terrainSampling]))
					GUILayout.Label(qualityHints[terrainSampling], guiStyleSmall);

#if UNITY_2017_3_OR_NEWER
				var newLodScheme = (FbxLODScheme.LODScheme)EditorGUILayout.EnumFlagsField(new GUIContent("LOD Quality"), (FbxLODScheme.LODSchemeEnumMaskFlags)lodScheme);
#else
				var newLodScheme = (FbxLODScheme.LODScheme)EditorGUILayout.EnumMaskField(new GUIContent("LOD Quality"), (FbxLODScheme.LODSchemeEnumMaskFlags)lodScheme);
#endif
				if (newLodScheme != lodScheme)
				{
					lodScheme = newLodScheme;
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_LODScheme", (int)lodScheme);
				}

#if UNITY_2017_3_OR_NEWER
				var newObjectExportMask = (ObjectExportMask)EditorGUILayout.EnumFlagsField("Object Export Mask", objectExportMask);
#else
				var newObjectExportMask = (ObjectExportMask)EditorGUILayout.EnumMaskField("Object Export Mask", objectExportMask);
#endif
				if (newObjectExportMask != objectExportMask)
				{
					objectExportMask = newObjectExportMask;
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_ObjectExportMask", (int)objectExportMask);
				}

				var newSkinnedMeshOptions = (SkinnedMeshOptions)EditorGUILayout.EnumPopup("Skinned Mesh Options", skinnedMeshOptions);
				if (newSkinnedMeshOptions != skinnedMeshOptions)
				{
					skinnedMeshOptions = newSkinnedMeshOptions;
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_SkinnedMeshOptions2", (int)skinnedMeshOptions);
				}

				if (skinnedMeshOptions == SkinnedMeshOptions.ExportTPose && (objectExportMask & ObjectExportMask.ExportCloth) == ObjectExportMask.ExportCloth)
				{
					var newStyle = new GUIStyle(EditorStyles.label);
					newStyle.normal.textColor = Color.red;
					newStyle.wordWrap = true;
					newStyle.fontStyle = FontStyle.Bold;
					EditorGUILayout.LabelField("Warning: ExportTPose can move bones which causes Cloth to be exported incorrectly.", newStyle);
				}

				EditorGUI.BeginChangeCheck();
				blendShapeOptions = (BlendShapeOptions)EditorGUILayout.EnumPopup("BlendShape Options", blendShapeOptions);
				if (EditorGUI.EndChangeCheck())
				{
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_BlendShapeOptions", (int)blendShapeOptions);
				}

				var newLogLevel = EditorGUILayout.Popup(new GUIContent("Logging"), logLevel, logLevelOptions);
				if (newLogLevel != logLevel)
				{
					logLevel = newLogLevel;
					EditorPrefs.SetInt("UnLogickFactory_FbxExporter_LogLevel", (int)logLevel);
				}
				EditorGUI.indentLevel--;

				textureScheme.OnInspectorGUI(true);

				EditorGUI.indentLevel++;
				EditorGUI.BeginChangeCheck();
				embedTextures = EditorGUILayout.Toggle("Embed Textures", embedTextures);
				if (EditorGUI.EndChangeCheck())
				{
					EditorPrefs.SetBool("UnLogickFactory_FbxExporter_EmbedTextures", embedTextures);
				}
				EditorGUI.BeginChangeCheck();
				embedShaderProperty = EditorGUILayout.Toggle(new GUIContent("Embed Shader Proprety", "This allows Unity Fbx Exporter to set the correct shader on import into Unity"), embedShaderProperty);
				if (EditorGUI.EndChangeCheck())
				{
					EditorPrefs.SetBool("UnLogickFactory_FbxExporter_EmbedShaderProperty", embedTextures);
				}

				EditorGUI.indentLevel--;

				EditorGUILayout.Space();

				if (GUILayout.Button("Export Entire Scene as Fbx"))
				{
					var gos = GameObject.FindObjectsOfType<GameObject>();
					var transforms = new List<Transform>(gos.Length);
					foreach (var go in gos)
					{
						if (!go.activeInHierarchy) continue;
						if (go.transform.parent != null) continue;
						transforms.Add(go.transform);
					}
					var filename = EditorUtility.SaveFilePanel("Fbx file", "", UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name, "fbx");
					if (!string.IsNullOrEmpty(filename))
					{
						Debug.Log("Fbx Exporter - Exporting Scene");
						FbxExporter.Export(filename, GetFbxSettings(), transforms.ToArray());
						AssetDatabase.Refresh();
					}
				}

				EditorGUILayout.Space();
				GUI.enabled = hasSelection;
				if (GUILayout.Button("Export Selected Object(s) as a single Fbx"))
				{
					var transforms = new Transform[Selection.gameObjects.Length];
					int index = 0;
					foreach (var go in Selection.gameObjects)
					{
						transforms[index++] = go.transform;
					}
					var filename = EditorUtility.SaveFilePanel("Fbx file", "", transforms[0].name, "fbx");
					if (!string.IsNullOrEmpty(filename))
					{
						Debug.Log("Fbx Exporter - Exporting Selected");
						FbxExporter.Export(filename, GetFbxSettings(), transforms);
						AssetDatabase.Refresh();
					}
				}
				GUI.enabled = true;

				EditorGUILayout.Space();

				GUI.enabled = hasSelection;
				if (GUILayout.Button("Export Selected Objects as multiple Fbxs"))
				{
					var exportedSomething = false;
					foreach (var go in Selection.gameObjects)
					{
						var filename = EditorUtility.SaveFilePanel("Fbx file", "", go.name, "fbx");
						if (!string.IsNullOrEmpty(filename))
						{
							Debug.LogFormat(go.transform, "Fbx Exporter - Exporting: {0}", go.name);
							FbxExporter.Export(filename, selectedFormat, textureScheme, terrainSampling, new FbxLODScheme() { Scheme = lodScheme }, logLevel, go.transform);
							exportedSomething = true;
						}
						else
						{
							break;
						}
					}
					if (exportedSomething)
					{
						AssetDatabase.Refresh();
					}
				}
				GUI.enabled = true;

				EditorGUILayout.Space();
				EditorGUILayout.Space();

				GUI.enabled = hasSelection;
				if (GUILayout.Button("Fix Selected Fbx Asset Import Settings"))
				{
					foreach (var obj in Selection.objects)
					{
						var go = obj as GameObject;
						if (go != null)
							FbxExporter.FixImportSettings(go);
						var path = AssetDatabase.GetAssetPath(obj);
						if (System.IO.Directory.Exists(path))
						{
							RecursiveScanFoldersForGOs(path);
						}
					}
				}
				GUI.enabled = true;
			}
			else
			{
				GUILayout.Label("The Unity FBX Exporter only works on Windows!");
			}

			EditorGUILayout.Space();

			GUILayout.Label("<b>To export from your own code simply call:</b>\n\nUnLogickFactory.FbxExporter.Export(...);\n(See the intellisense comments for full arguments list)\n\n<b>Examples:</b>\nFbxExporter.Export(\"output.fbx\", -1, transform);\nFbxExporter.Export(\"output.fbx\", -1, transform1, transform2);\n\n<b>To specify which Fbx format you want to export to use:</b>\n\nUnLogickFactory.FbxExporter.GetFormatNames(...)\n(See the intellisense comments for full arguments list)\n\n<b>Examples:</b>\nFbxExporter.GetFormatNames(true, out formatNames, out formatIds);\n\n<b>Runtime Export</b>\nSelect the appropriate dll in the DLL folder and select the standalone platform. It's recommended to uncheck the other\nplatform (32 bit or 64 bit) to avoid bloating your build size.\n\n<b>Please remember the Fbx Exporter is Windows only!</b>\n\n", guiStyleSmall);

			GUILayout.FlexibleSpace();

			GUILayout.Label("<b>Legal stuff about the Unity Fbx Exporter</b>\nThis software contains Autodesk® FBX® code developed by Autodesk, Inc. Copyright 2014 Autodesk, Inc. All rights, reserved. Such code is provided “as is” and Autodesk, Inc. disclaims any and all warranties, whether express or implied, including without limitation the implied warranties of merchantability, fitness for a particular purpose or non - infringement of third party rights. In no event shall Autodesk, Inc. be liable for any direct, indirect, incidental, special, exemplary, or consequential damages(including, but not limited to, procurement of substitute goods or services; loss of use, data, or profits; or business interruption) however caused and on any theory of liability, whether in contract, strict liability, or tort(including negligence or otherwise) arising in any way out of such code.", guiStyleAbout);
			GUILayout.EndScrollView();
		}

		private void RecursiveScanFoldersForGOs(string folder)
		{
			var guids = AssetDatabase.FindAssets("t:Model", new string[] { folder });
			foreach (var guid in guids)
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				FbxExporter.FixImportSettings(AssetDatabase.LoadMainAssetAtPath(path) as GameObject);
			}
		}

		public static void FixSelectedFbxMaterials()
		{
			foreach (var go in Selection.gameObjects)
			{
				var assetPath = AssetDatabase.GetAssetPath(go);
				var mi = AssetImporter.GetAtPath(assetPath) as ModelImporter;
				if (!(mi.globalScale == 100f && mi.animationType == ModelImporterAnimationType.None))
				{
					mi.globalScale = 100f;
					mi.animationType = ModelImporterAnimationType.None;
					mi.SaveAndReimport();
				}
				FbxExporter.FixMaterialsRecursive(go.transform);
			}
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		}

		FbxExportSettings GetFbxSettings()
		{
			return new FbxExportSettings()
			{
				formatId = selectedFormat,
				textureScheme = textureScheme,
				terrainQuality = terrainSampling,
				embedTextures = embedTextures,
				embedShaderProperty = embedShaderProperty,
				LODScheme = new FbxLODScheme() { Scheme = lodScheme },
				logLevel = logLevel,
				skinnedMeshOptions = skinnedMeshOptions,
				blendShapeOptions = blendShapeOptions,
				objectExportMask = objectExportMask,
			};
		}
	}
}
#endif