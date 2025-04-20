using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace UnLogickFactory
{
	public class FbxCustomPropertyExporterExample : MonoBehaviour
	{
        [FormerlySerializedAs("filename")]
        public string editorFilename = "Assets/Plugins/UnLogickFactory/FbxExporter/Example/ExportedCustomProperty.fbx";
        public string runtimeFilename = "ExportedCustomProperty.fbx";
        public int logLevel = 0;
		public bool manuallyExport;

        void Start()
		{
			if (!manuallyExport)
			{
				Export();
			}
		}

		public void Export()
		{ 
#if UNITY_EDITOR
            var filename = editorFilename;
#else
            var filename = runtimeFilename;
#endif
            var transforms = new Transform[1] { transform };
			if (!string.IsNullOrEmpty(filename))
			{
				Debug.Log("Fbx Custom Property Exporter Example - Exporting own GameObject with custom properties");
				var settings = new FbxExportSettings();
				settings.textureScheme = FbxTextureExportScheme.GetDefaultScheme();
				settings.OnFbxNodeCreated = OnFbxNodeCallback;
				settings.OnFbxMeshCreated = OnFbxMeshCallback;
				settings.OnFbxTerrainCreated = OnFbxTerrainCallback;
				settings.OnFbxSkinnedMeshCreated = OnFbxSkinnedMeshCallback;
				settings.OnFbxMaterialCreated = OnFbxMaterialCallback;
                settings.logLevel = logLevel;
                FbxExporter.Export(filename, settings, transforms);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
                UnityEditor.EditorGUIUtility.PingObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath(filename));
#endif
                Debug.Log("Fbx Custom Property Exporter Example - Done");
            }
        }

		void OnFbxNodeCallback(Transform transform, IntPtr fbxNode)
		{
			UnityFbxExporterBinding.SetBoolProperty(fbxNode, "CustomBool", true);
		}

		void OnFbxMeshCallback(Transform transform, IntPtr fbxNode, MeshRenderer meshRenderer, IntPtr fbxMesh)
		{
			UnityFbxExporterBinding.SetColorProperty(fbxNode, "CustomColor", 1, 0, 0, 1);
			UnityFbxExporterBinding.SetDoubleProperty(fbxMesh, "CustomValue", 0.33);
		}

		void OnFbxTerrainCallback(Transform transform, IntPtr fbxNode, Terrain terrain, IntPtr fbxMesh)
		{
			UnityFbxExporterBinding.SetDouble2Property(fbxNode, "CustomDouble2", 0.33, 0.66);
			UnityFbxExporterBinding.SetDouble3Property(fbxMesh, "CustomDouble3", 0.33, 0.66, 0.99);
		}

		void OnFbxSkinnedMeshCallback(Transform transform, IntPtr fbxNode, SkinnedMeshRenderer skinnedMeshRenderer, IntPtr fbxMesh)
		{
			UnityFbxExporterBinding.SetStringProperty(fbxNode, "CustomString", "SkinnedMesh");
		}

		void OnFbxMaterialCallback(Transform transform, IntPtr fbxNode, IntPtr fbxMaterial, IntPtr[] fbxTextures)
		{
			UnityFbxExporterBinding.SetStringProperty(fbxNode, "CustomString", "SkinnedMesh");
		}
	}
}