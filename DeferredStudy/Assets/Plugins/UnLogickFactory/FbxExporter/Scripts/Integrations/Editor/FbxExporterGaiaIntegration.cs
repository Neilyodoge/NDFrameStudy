#if GAIA_PRESENT && UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnLogickFactory;
using System.IO;

namespace Gaia.GX.UnLogickFactory
{
	/// <summary>
	/// Unity Fbx Exporter Integration for Gaia.
	/// </summary>
	public class FbxExporterGaiaIntegration : MonoBehaviour
	{
		#region Generic informational methods

		public static string GetPublisherName()
		{
			return "UnLogick Factory";
		}

		public static string GetPackageName()
		{
			return "Unity Fbx Exporter";
		}

		#endregion

		#region Methods exposed by Gaia as buttons must be prefixed with GX_

		public static void GX_ExportEntireSceneFbx()
		{
			var gos = GameObject.FindObjectsOfType<GameObject>();
			var transforms = new List<Transform>(gos.Length);
			foreach (var go in gos)
			{
				if (!go.activeInHierarchy) continue;
				if (go.transform.parent != null) continue;
				transforms.Add(go.transform);
			}

			var path = EditorPrefs.GetString("UnLogickFactory_FbxExporter_DefaultPath", "Assets/ExportedFbx");
			var exportPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "Export1"));

			var systemPath = Path.Combine(Directory.GetCurrentDirectory(), exportPath);
			RecursivelyCreateDirectory(systemPath);

			var filename = Path.Combine(systemPath, "Gaia.fbx");

			var format = FbxExporter.GetDefaultFormat();
			var formatString = EditorPrefs.GetString("UnLogickFactory_SelectedFbxFormat");
			if (!string.IsNullOrEmpty(formatString))
			{
				int[] formatIds;
				string[] formatNames;
				FbxExporter.GetFormatNames(true, out formatNames, out formatIds);
				for (int i = 0; i < formatNames.Length; i++)
				{
					if (formatNames[i] == formatString)
					{
						format = formatIds[i];
						break;
					}
				}
			}

			FbxExporter.Export(filename, format, FbxTextureExportScheme.GetDefaultScheme(), EditorPrefs.GetInt("UnLogickFactory_FbxExporter_TerrainSamplingRate", 0), new FbxLODScheme() { Scheme = (FbxLODScheme.LODScheme)EditorPrefs.GetInt("UnLogickFactory_FbxExporter_LODScheme", 0) }, EditorPrefs.GetInt("UnLogickFactory_FbxExporter_LogLevel", 0), transforms.ToArray());
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			var assetPath = Path.Combine(exportPath, "Gaia.fbx");
			var modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
			modelImporter.globalScale = 100f;
			modelImporter.animationType = ModelImporterAnimationType.None;
			modelImporter.isReadable = false;
			modelImporter.importBlendShapes = false;
			modelImporter.SaveAndReimport();
			var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
			EditorGUIUtility.PingObject(asset);
		}

		public static void GX_UnityFbxExporterSettings()
		{
			FbxExporterWindow.ShowFbxExporterWindow();
		}

		#endregion

		#region Helper methods

		private static void RecursivelyCreateDirectory(string exportPath)
		{
			var parent = Path.GetDirectoryName(exportPath);
			if (!Directory.Exists(parent))
				RecursivelyCreateDirectory(parent);
			Directory.CreateDirectory(exportPath);
		}

		#endregion
	}
}

#endif