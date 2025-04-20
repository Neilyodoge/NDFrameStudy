using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnLogickFactory
{
	public class FbxExporter
	{
		private static int loggingLevel;

		/// <summary>
		/// This property can tell you if you're on a supported platform and if the required DLL is present. 
		/// Calling Export on a platform where FBX isn't supported will simply terminate early with a false return value
		/// </summary>
		public static bool FbxSupported
		{
			get
			{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Just a quick helper function that returns the first fbx format in the list.
		/// Historically this have always been latest binary version, but there is no guarantees other than it is fbx.
		/// </summary>
		/// <returns></returns>
		public static int GetDefaultFormat()
		{
			if (!UnityFbxExporterBinding.Init())
			{
				return 0;
			}
			try
			{
				UnityFbxExporterBinding.Create();
				try
				{
					var formatsCount = UnityFbxExporterBinding.FormatsCount();
					for (int i = 0; i < formatsCount; i++)
					{
						if (!UnityFbxExporterBinding.IsFormatFbx(i))
							continue;
						return i;
					}
					return 0;
				}
				finally
				{
					UnityFbxExporterBinding.Destroy();
				}
			}
			finally
			{
				UnityFbxExporterBinding.Close();
			}
		}

		/// <summary>
		/// This method will export one or more Transforms as a single fbx file. 
		/// Each transform will be treated as a root object in the fbx and only their local position/rotation/scale will be preserved. 
		/// The transforms will be recursively scanned and all active children will be added as well as their various MeshRenderers, SkinnedMeshRenderers and Terrains.
		/// </summary>
		/// <param name="filename">The name of the fbx file to create</param>
		/// <param name="roots">One or more transform to be added to the fbx file</param>
		/// <returns>True means the fbx was exported correctly.</returns>
		public static bool Export(string filename, params Transform[] roots)
		{
			return Export(filename, GetDefaultFormat(), null, 0, new FbxLODScheme(), 0, roots);
		}
		/// <summary>
		/// This method will export one or more Transforms as a single fbx file. 
		/// Each transform will be treated as a root object in the fbx and only their local position/rotation/scale will be preserved. 
		/// The transforms will be recursively scanned and all active children will be added as well as their various MeshRenderers, SkinnedMeshRenderers and Terrains.
		/// </summary>
		/// <param name="filename">The name of the fbx file to create</param>
		/// <param name="formatId">The formatId as provided by the FbxExporter.GetFormatNames.</param>
		/// <param name="roots">One or more transform to be added to the fbx file</param>
		/// <returns>True means the fbx was exported correctly.</returns>
		public static bool Export(string filename, int formatId, params Transform[] roots)
		{
			return Export(filename, formatId, null, 0, new FbxLODScheme(), 0, roots);
		}

		/// <summary>
		/// This method will export one or more Transforms as a single fbx file. 
		/// </summary>
		/// <param name="filename">The name of the fbx file to create</param>
		/// <param name="settings">Settings object for all fbx exporter settings.</param>
		/// <param name="roots">One or more transform to be added to the fbx file</param>
		/// <returns>True means the fbx was exported correctly.</returns>
		public static bool Export(string filename, FbxExportSettings settings, params Transform[] roots)
		{
			if (settings.textureScheme == null)
			{
				settings.textureScheme = FbxTextureExportScheme.GetDefaultScheme();
			}

			if ((settings.objectExportMask & ObjectExportMask.ExportSkinnedMeshes) == ObjectExportMask.ExportSkinnedMeshes && settings.skinnedMeshOptions == SkinnedMeshOptions.ExportTPose && (settings.objectExportMask & ObjectExportMask.ExportCloth) == ObjectExportMask.ExportCloth)
			{
				LogErrorFormat(settings.logLevel >= 0, "FbxExporter - Export Settings says export skinned meshes in TPose and export Cloth at the same time. Only static mesh cloth will be exported as cloth. SkinnedMeshCloth will be exported as skinned mesh in it's T-Pose.");
			}

			if (settings.LODScheme != null)
			{
				settings.LODScheme.Prepare(settings.logLevel);
			}
			TextureExporter textureExporter = null;
#if UNITY_EDITOR
			bool containsHumanoidRig = roots.Length == 1 && IsHumanoidRoot(roots[0]);
#endif
			if (!UnityFbxExporterBinding.Init(settings.logLevel))
			{
				return false;
			}
			try
			{
				bool success = true;
				UnityFbxExporterBinding.Create();
				try
				{
					var materialDictionary = new Dictionary<Material, IntPtr>();
					var previousDir = System.IO.Directory.GetCurrentDirectory();
					var shaderProperty = new FbxShaderProperty();
					if (!System.IO.Path.IsPathRooted(filename))
					{
						filename = System.IO.Path.Combine(previousDir, filename);
					}
					var outputPath = System.IO.Path.GetDirectoryName(filename);
					var outputFilename = System.IO.Path.GetFileName(filename);
					foreach (var root in roots)
					{
						var createRoot = root.GetComponent<MeshRenderer>() != null;
						var skeletonNodes = new Dictionary<IntPtr, IntPtr>();

						if ((settings.objectExportMask & ObjectExportMask.ExportSkinnedMeshes) == ObjectExportMask.ExportSkinnedMeshes && settings.skinnedMeshOptions == SkinnedMeshOptions.ExportTPose)
						{
							foreach (var animator in root.GetComponentsInChildren<Animator>())
							{
								animator.SetTPose();
							}
						}

						var fbxCollection = UnityFbxExporterBinding.ScanHierarchy(root, createRoot, settings);
						foreach (var smr in fbxCollection.SkinnedMeshes)
						{
							ExportSkinnedMeshOrCloth(smr, root, textureExporter, outputPath, skeletonNodes, materialDictionary, settings, fbxCollection);
						}

						foreach (var cloth in fbxCollection.Cloths)
						{
							var smr = cloth.GetComponent<SkinnedMeshRenderer>();
							ExportSkinnedMeshOrCloth(smr, root, textureExporter, outputPath, skeletonNodes, materialDictionary, settings, fbxCollection);
						}

						foreach (var mr in fbxCollection.Meshes)
						{
							Mesh mesh;
							Material[] materials;

							if (!checkIsValid(mr, out mesh, out materials))
								continue;

							LogFormat(settings.logLevel >= 1, mr.gameObject, "FbxExporter - Exporting {0}", mr.name);

							var node = fbxCollection.FbxNodes[mr.transform];
							if (node == IntPtr.Zero)
							{
								node = UnityFbxExporterBinding.SkeletonCreateRoot(root.name);
								fbxCollection.FbxNodes[mr.transform] = node;
								UnityFbxExporterBinding.NodeSetTransform(node, root, true);
							}

							if (settings.embedShaderProperty)
							{
								EmbedShaderProperty(node, materials, shaderProperty);
							}

							if (settings.textureScheme != null && settings.textureScheme.ExportMeshTextures)
							{
								if (textureExporter == null)
									textureExporter = TextureExporter.CreateTextureExporter();

								for (int i = 0; i < materials.Length; i++)
								{
									materials[i] = textureExporter.ProcessMaterial(materials[i], mr, settings.textureScheme, outputPath);
								}
							}
							IntPtr[] fbxMaterials;
							IntPtr[][] fbxTextures;
							var fbxMesh = UnityFbxExporterBinding.ProcessMesh(mesh, settings.logLevel, materials, null, null, node, materialDictionary, out fbxMaterials, out fbxTextures);
							if (fbxMesh == IntPtr.Zero)
							{
								Debug.LogErrorFormat(mr.gameObject, "Fbx Exporter - Unable to export mesh {0}, mesh is not readable.", mesh.name);
								success = false;
								continue;
							}

							for (int i = 0; i < fbxMaterials.Length; i++)
							{
								if (fbxMaterials[i] != IntPtr.Zero)
								{
									if (settings.OnFbxMaterialCreated != null)
										settings.OnFbxMaterialCreated(mr.transform, node, fbxMaterials[i], fbxTextures[i]);

									var materialCreatedComponent = mr.GetComponent<FbxMaterialCustomProperties>();
									if (materialCreatedComponent != null)
										materialCreatedComponent.Apply(fbxMesh);
								}
							}

							var meshCreatedComponent = mr.GetComponent<FbxMeshCustomProperties>();
							if (meshCreatedComponent != null)
								meshCreatedComponent.Apply(fbxMesh);

							if (settings.OnFbxMeshCreated != null)
								settings.OnFbxMeshCreated(mr.transform, node, mr, fbxMesh);
						}

						foreach (var terrain in fbxCollection.Terrains)
						{
							TerrainData terrainData;
							if (!checkIsValid(terrain, out terrainData))
							{
								Debug.LogErrorFormat(terrain.gameObject, "Fbx Exporter - Unable to export terrain {0}, mesh is not readable.", terrain.name);
								success = false;
								continue;
							}

							LogFormat(settings.logLevel >= 1, terrain.gameObject, "FbxExporter - Exporting {0}", terrain.name);
							var node = fbxCollection.FbxNodes[terrain.transform];
							if (node == IntPtr.Zero)
							{
								node = UnityFbxExporterBinding.SkeletonCreateRoot(root.name);
								fbxCollection.FbxNodes[terrain.transform] = node;
								UnityFbxExporterBinding.NodeSetTransform(node, root, true);
							}
							var oldPosition = terrain.transform.transform.localPosition;
							terrain.transform.transform.localPosition = terrain.transform.transform.position;
							terrain.transform.transform.localScale = Vector3.one;
							terrain.transform.transform.localRotation = Quaternion.identity;
							UnityFbxExporterBinding.NodeSetTransform(node, terrain.transform, false);
							terrain.transform.transform.localPosition = oldPosition;
							IntPtr fbxMaterial;
							IntPtr[] fbxTextures;
							var fbxMesh = UnityFbxExporterBinding.ProcessTerrain(terrain, terrainData, settings.terrainQuality, node, out fbxMaterial, out fbxTextures);

							if (settings.textureScheme != null && settings.textureScheme.ExportTerrainTextures)
							{
								if (textureExporter == null)
									textureExporter = TextureExporter.CreateTextureExporter();
								textureExporter.ProcessTerrain(terrain, 4096, settings.textureScheme, outputPath);
							}

							var materialCreatedComponent = terrain.GetComponent<FbxMaterialCustomProperties>();
							if (materialCreatedComponent != null)
								materialCreatedComponent.Apply(fbxMesh);

							if (settings.OnFbxMaterialCreated != null)
								settings.OnFbxMaterialCreated(terrain.transform, node, fbxMaterial, fbxTextures);

							var terrainCreatedComponent = terrain.GetComponent<FbxTerrainCustomProperties>();
							if (terrainCreatedComponent != null)
								terrainCreatedComponent.Apply(fbxMesh);

							if (settings.OnFbxTerrainCreated != null)
								settings.OnFbxTerrainCreated(terrain.transform, node, terrain, fbxMesh);
						}

						foreach (var camera in fbxCollection.Cameras)
						{
							var node = fbxCollection.FbxNodes[camera.transform];
							if (node == IntPtr.Zero)
							{
								node = UnityFbxExporterBinding.SkeletonCreateRoot(root.name);
								fbxCollection.FbxNodes[camera.transform] = node;
								UnityFbxExporterBinding.NodeSetTransform(node, root, true);
							}
							UnityFbxExporterBinding.CameraCreate(camera.name, node, camera.nearClipPlane, camera.farClipPlane, camera.fieldOfView);
						}

						foreach (var light in fbxCollection.Lights)
						{
							var node = fbxCollection.FbxNodes[light.transform];
							if (node == IntPtr.Zero)
							{
								node = UnityFbxExporterBinding.SkeletonCreateRoot(root.name);
								fbxCollection.FbxNodes[light.transform] = node;
								UnityFbxExporterBinding.NodeSetTransform(node, root, true);
							}

							UnityFbxExporterBinding.LightCreate(light.name, node, ConvertLightType(light.type), light.color.r, light.color.g, light.color.b, light.intensity, light.enabled, light.shadows != LightShadows.None);
						}
					}

					System.IO.Directory.SetCurrentDirectory(outputPath);
					bool fbxCreated = UnityFbxExporterBinding.Save(outputFilename, settings.formatId, settings.embedTextures);
					System.IO.Directory.SetCurrentDirectory(previousDir);
					if (fbxCreated)
					{
						LogFormat(settings.logLevel >= 0, "FbxExporter - Fbx file generated {0} {1}", outputFilename, success ? "without errors" : "with errors, check the log");
					}
					else
					{
						LogErrorFormat(settings.logLevel >= 0, "FbxExporter - Failed to write {0} {1}", outputFilename, success ? "sdk error" : "check the log");
						success = false;
					}
#if UNITY_EDITOR
					FixImportSettings(filename, containsHumanoidRig);
#endif
					return success;
				}
				finally
				{
					UnityFbxExporterBinding.Destroy();
				}
			}
			finally
			{
				UnityFbxExporterBinding.Close();
				if (textureExporter != null)
				{
#if UNITY_EDITOR
					UnityEngine.Object.DestroyImmediate(textureExporter.gameObject, false);
#else
					UnityEngine.Object.Destroy(textureExporter.gameObject);
#endif
				}
			}
		}

		private static void EmbedShaderProperty(IntPtr node, Material[] materials, FbxShaderProperty shaderProperty)
		{
			if (shaderProperty.shaders.Capacity < materials.Length)
			{
				var newCapacity = shaderProperty.shaders.Capacity;
				while (newCapacity < materials.Length)
				{
					newCapacity *= 2;
				}
				shaderProperty.shaders.Capacity = newCapacity;
			}

			foreach (var material in materials)
			{
				shaderProperty.shaders.Add(GetShader(material));
			}

			UnityFbxExporterBinding.SetStringProperty(node, "UnLogickFactory_FbxExporter_CustomProperty", JsonUtility.ToJson(shaderProperty));
			shaderProperty.shaders.Clear();
		}

		private static string GetShader(Material material)
		{
			return (material == null || material.shader == null) ? "" : material.shader.name;
		}

		private static bool IsHumanoidRoot(Transform root)
		{
			var animator = root.GetComponent<Animator>();
			return animator != null && animator.isHuman;
		}

		private static void ExportSkinnedMeshOrCloth(SkinnedMeshRenderer smr, Transform root, TextureExporter textureExporter, string outputPath, Dictionary<IntPtr, IntPtr> skeletonNodes, Dictionary<Material, IntPtr> materialDictionary, FbxExportSettings settings, FbxExportCollection fbxCollection)
		{
			Transform[] bones;
			Mesh mesh;
			Material[] materials;
			Cloth cloth;
			if (!checkIsValid(smr, settings.ExportCloth, out bones, out mesh, out cloth, out materials))
				return;

			LogFormat(settings.logLevel >= 1, smr.gameObject, "FbxExporter - Exporting {0}", smr.name);

			var node = fbxCollection.FbxNodes[smr.transform];
			if (node == IntPtr.Zero)
			{
				node = UnityFbxExporterBinding.SkeletonCreateRoot(root.name);
				fbxCollection.FbxNodes[smr.transform] = node;
				UnityFbxExporterBinding.NodeSetTransform(node, root, true);
			}
			Vector3[] vertices;
			Vector3[] normals;
			float[] blends = null;

			var rootBone = smr.rootBone;

			if (cloth != null && (rootBone == null || settings.skinnedMeshOptions == SkinnedMeshOptions.ExportCurrentPose))
			{
				var storedVertices = mesh.vertices;

				var clothSpaceToSkinnedSpaceMatrix = rootBone == null ? Matrix4x4.identity : smr.transform.worldToLocalMatrix * smr.rootBone.localToWorldMatrix;
				var clothVertices = cloth.vertices;
				var clothNormals = cloth.normals;

				vertices = new Vector3[storedVertices.Length];
				normals = new Vector3[storedVertices.Length];
				var lookupDict = new Dictionary<Vector3, int>(storedVertices.Length);
				for (int i = 0; i < storedVertices.Length; i++)
				{
					int idx;
					if (!lookupDict.TryGetValue(storedVertices[i], out idx))
					{
						idx = lookupDict.Count;
						lookupDict.Add(storedVertices[i], idx);
					}
					vertices[i] = clothSpaceToSkinnedSpaceMatrix.MultiplyPoint(clothVertices[idx]);
					normals[i] = clothSpaceToSkinnedSpaceMatrix.MultiplyVector(clothNormals[idx]);
				}
			}
			else
			{
				if (mesh.blendShapeCount > 0)
				{
					blends = new float[mesh.blendShapeCount];
					UnityFbxExporterBinding.blendWeights = blends;
					for (int i = 0; i < mesh.blendShapeCount; i++)
					{
						blends[i] = smr.GetBlendShapeWeight(i);
						smr.SetBlendShapeWeight(i, 0);
					}
				}
				var bakedMesh = new Mesh();
				UnityFbxExporterBinding.blendSmr = smr;
				UnityFbxExporterBinding.blendMesh = bakedMesh;

				//var smrPosition = smr.transform.localPosition;
				//var smrRotation = smr.transform.localRotation;
				//var smrScale = smr.transform.localScale;
				//var smrParent = smr.transform.parent;

				//smr.transform.parent = null;
				//smr.transform.localPosition = Vector3.zero;
				//smr.transform.localRotation = Quaternion.identity;
				//smr.transform.localScale = Vector3.one;

				smr.BakeMesh(bakedMesh);

				//smr.transform.parent = smrParent;
				//smr.transform.localPosition = smrPosition;
				//smr.transform.localRotation = smrRotation;
				//smr.transform.localScale = smrScale;

				vertices = bakedMesh.vertices;
				//for (int i = 0; i < vertices.Length; i++)
				//{
				//	vertices[i] = vertices[i] * 0.25f;
				//}

				normals = bakedMesh.normals;
			}

			if (settings.textureScheme != null && settings.textureScheme.ExportSkinnedMeshTextures)
			{
				if (textureExporter == null)
					textureExporter = TextureExporter.CreateTextureExporter();
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i] = textureExporter.ProcessMaterial(materials[i], smr, settings.textureScheme, outputPath);
				}
			}

			IntPtr[] fbxMaterials;
			IntPtr[][] fbxTextures;
			if (string.IsNullOrEmpty(mesh.name))
				mesh.name = smr.name;
			var fbxMesh = UnityFbxExporterBinding.ProcessMesh(mesh, settings.logLevel, materials, vertices, normals, node, materialDictionary, out fbxMaterials, out fbxTextures, settings.blendShapeOptions);

			if (blends != null)
			{
				for (int i = 0; i < mesh.blendShapeCount; i++)
				{
					smr.SetBlendShapeWeight(i, blends[i]);
				}
				UnityEngine.Object.DestroyImmediate(UnityFbxExporterBinding.blendMesh, false);
				UnityFbxExporterBinding.blendSmr = null;
				UnityFbxExporterBinding.blendMesh = null;
				UnityFbxExporterBinding.blendWeights = null;
			}

			if (bones.Length > 0)
			{
				var fbxSkin = UnityFbxExporterBinding.MeshCreateSkin(fbxMesh);
				var fbxClusters = new IntPtr[bones.Length];
				var fbxBones = new IntPtr[bones.Length];
				for (int i = 0; i < bones.Length; i++)
				{
					LogFormat(settings.logLevel >= 2, bones[i], "FbxExporter - Skinned Bone: {0}", bones[i]);
					fbxBones[i] = fbxCollection.FbxNodes[bones[i]];
					RecursivelyEnsureLimb(root, bones[i], fbxCollection.FbxNodes, skeletonNodes);
					fbxClusters[i] = UnityFbxExporterBinding.ClusterCreate(fbxBones[i]);
				}
				var boneWeights = mesh.boneWeights;
				UnityFbxExporterBinding.AddBoneWeights(boneWeights, fbxClusters);
				for (int i = 0; i < fbxClusters.Length; i++)
				{
					SkinAddCluster(fbxSkin, fbxClusters[i], node, bones[i]);
				}

				UnityFbxExporterBinding.MeshStoreBindPose(fbxMesh);
			}

			for (int i = 0; i < fbxMaterials.Length; i++)
			{
				if (fbxMaterials[i] != IntPtr.Zero)
				{
					if (settings.OnFbxMaterialCreated != null)
						settings.OnFbxMaterialCreated(smr.transform, node, fbxMaterials[i], fbxTextures[i]);

					var materialCreatedComponent = smr.GetComponent<FbxMaterialCustomProperties>();
					if (materialCreatedComponent != null)
						materialCreatedComponent.Apply(fbxMesh);
				}
			}

			if (settings.OnFbxSkinnedMeshCreated != null)
				settings.OnFbxSkinnedMeshCreated(smr.transform, node, smr, fbxMesh);

			var skinnedMeshCreatedComponent = smr.GetComponent<FbxSkinnedMeshCustomProperties>();
			if (skinnedMeshCreatedComponent != null)
				skinnedMeshCreatedComponent.Apply(fbxMesh);
		}

		private static UnityFbxExporterBinding.EType ConvertLightType(LightType type)
		{
			switch (type)
			{
				case LightType.Spot:
					return UnityFbxExporterBinding.EType.eSpot;
				case LightType.Directional:
					return UnityFbxExporterBinding.EType.eDirectional;
				case LightType.Point:
					return UnityFbxExporterBinding.EType.ePoint;
				case LightType.Area:
					return UnityFbxExporterBinding.EType.eArea;
				default:
					return UnityFbxExporterBinding.EType.ePoint;
			}
		}


		/// <summary>
		/// This method will export one or more Transforms as a single fbx file. 
		/// Each transform will be treated as a root object in the fbx and only their local position/rotation/scale will be preserved. 
		/// The transforms will be recursively scanned and all active children will be added as well as their various MeshRenderers, SkinnedMeshRenderers and Terrains.
		/// </summary>
		/// <param name="filename">The name of the fbx file to create</param>
		/// <param name="formatId">The formatId as provided by the FbxExporter.GetFormatNames.</param>
		/// <param name="textureScheme">Null means don't export texures, look at FbxTextureExportScheme for detailed information.</param>
		/// <param name="terrainQuality">Specify the quality of terrain export, if you don't subsample a 1k terrain it turns into a 2 million triangle mesh! 
		/// The default value of 0 aims for highest quality that fits a 64k triangle mesh. 
		/// Quality 1 keeps full resolution, each increment after that halves the resolution of the generated terrain.</param>
		/// <param name="LODScheme">Null means export everything at full quality, look at FbxLODScheme for detailed information.</param>
		/// <param name="logLevel">How much information do you want, 0 normal, 1 more, 2 most, (-1 only errors!)</param>
		/// <param name="roots">One or more transform to be added to the fbx file</param>
		/// <returns>True means the fbx was exported correctly.</returns>
		public static bool Export(string filename, int formatId, FbxTextureExportScheme textureScheme, int terrainQuality, FbxLODScheme LODScheme, int logLevel, params Transform[] roots)
		{
			var settings = new FbxExportSettings();
			settings.formatId = formatId;
			settings.textureScheme = textureScheme;
			settings.terrainQuality = terrainQuality;
			settings.LODScheme = LODScheme;
			settings.logLevel = logLevel;
			return Export(filename, settings, roots);
		}

		private static void RecursivelyEnsureLimb(Transform root, Transform bone, Dictionary<Transform, IntPtr> fbxNodes, Dictionary<IntPtr, IntPtr> skeletonNodes)
		{
			var node = fbxNodes[bone];
			if (node == IntPtr.Zero)
				return;

			if (skeletonNodes.ContainsKey(node))
				return;

			UnityFbxExporterBinding.MakeNodeSkeleton(node);
			skeletonNodes.Add(node, node);

			if (bone != root)
			{
				RecursivelyEnsureLimb(root, bone.parent, fbxNodes, skeletonNodes);
			}
		}

		public static void LogFormat(bool actuallyLogThis, string format, params object[] arguments)
		{
			if (actuallyLogThis)
				Debug.LogFormat(format, arguments);
		}

		public static void LogFormat(bool actuallyLogThis, UnityEngine.Object context, string format, params object[] arguments)
		{
			if (actuallyLogThis)
				Debug.LogFormat(context, format, arguments);
		}

		public static void LogWarningFormat(bool actuallyLogThis, string format, params object[] arguments)
		{
			if (actuallyLogThis)
				Debug.LogWarningFormat(format, arguments);
		}

		public static void LogErrorFormat(bool actuallyLogThis, string format, params object[] arguments)
		{
			if (actuallyLogThis)
				Debug.LogErrorFormat(format, arguments);
		}


#if UNITY_EDITOR
		public static void FixImportSettings(string path, bool containsHumanoid)
		{
			var cwd = System.IO.Directory.GetCurrentDirectory().Replace('\\', '/');
			if (path.StartsWith(cwd))
			{
				var assetPath = path.Substring(cwd.Length + 1);
				AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
				var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
				FixImportSettings(asset as GameObject, containsHumanoid);
			}
		}

		public static void FixImportSettings(GameObject fbx)
		{
			if (fbx == null)
				return;
			var assetPath = AssetDatabase.GetAssetPath(fbx);
			if (!string.IsNullOrEmpty(assetPath))
			{
				var mi = AssetImporter.GetAtPath(assetPath) as ModelImporter;
				if (mi == null)
					return;
				var smrs = fbx.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				var containsSkinnedMeshes = smrs != null && smrs.Length > 0;

				bool updateRequired = false;
				if (mi.globalScale != 100f)
				{
					mi.globalScale = 100f;
					updateRequired = true;
				}
				if (containsSkinnedMeshes)
				{
					if (mi.animationType == ModelImporterAnimationType.None)
					{
						mi.animationType = ModelImporterAnimationType.Human;
						updateRequired = true;
					}
				}
				else
				{
					if (mi.animationType != ModelImporterAnimationType.None)
					{
						mi.animationType = ModelImporterAnimationType.None;
						updateRequired = true;
					}
				}
				if (updateRequired)
				{
					mi.SaveAndReimport();
				}
			}
			FbxExporter.FixMaterialsRecursive(fbx.transform);
		}


		public static void FixImportSettings(GameObject fbx, bool containsHumanoid)
		{
			if (fbx == null)
				return;
			var assetPath = AssetDatabase.GetAssetPath(fbx);
			if (!string.IsNullOrEmpty(assetPath))
			{
				var mi = AssetImporter.GetAtPath(assetPath) as ModelImporter;
				if (mi == null)
					return;
				var smrs = fbx.GetComponentsInChildren<SkinnedMeshRenderer>(true);
				var containsSkinnedMeshes = smrs.Length > 0;

				bool updateRequired = false;
				if (mi.globalScale != 100f)
				{
					mi.globalScale = 100f;
					updateRequired = true;
				}
				if (containsSkinnedMeshes)
				{
					if (containsHumanoid && mi.animationType != ModelImporterAnimationType.Human)
					{
						mi.animationType = ModelImporterAnimationType.Human;
						updateRequired = true;
					}
					if (containsSkinnedMeshes && mi.animationType == ModelImporterAnimationType.None)
					{
						mi.animationType = ModelImporterAnimationType.Generic;
						updateRequired = true;
					}
				}
				else
				{
					if (mi.animationType != ModelImporterAnimationType.None)
					{
						mi.animationType = ModelImporterAnimationType.None;
						updateRequired = true;
					}
				}
				if (updateRequired)
				{
					mi.SaveAndReimport();
				}
			}
			FbxExporter.FixMaterialsRecursive(fbx.transform);
		}

		public static void FixMaterialsRecursive(Transform prefab)
		{
			FixMaterials(prefab.GetComponent<Renderer>());
			for (int i = 0; i < prefab.childCount; i++)
			{
				FixMaterialsRecursive(prefab.GetChild(i));
			}
		}

		private static void FixMaterials(Renderer renderer)
		{
			if (renderer == null) return;
			foreach (var mat in renderer.sharedMaterials)
			{
				if (mat.shader.name == "Standard")
				{
					mat.shader = Shader.Find("Standard (Specular setup)");
					EditorUtility.SetDirty(mat);
				}
				FixMaterial(mat);
			}
		}

		private static void FixMaterial(Material mat)
		{
			var texture = mat.GetTexture("_MainTex");
			var diffusePath = AssetDatabase.GetAssetPath(texture);
			var normalPath = diffusePath.Replace("_Diffuse.", "_Normal.");
			var specularPath = diffusePath.Replace("_Diffuse.", "_Specular.");
			if (normalPath == diffusePath)
			{
				// This material wasn't exported with textures by the fbx exporter.
				return;
			}

			var diffuseImporter = AssetImporter.GetAtPath(diffusePath) as TextureImporter;
#if UNITY_5_5_OR_NEWER
			if (diffuseImporter != null && (!(diffuseImporter.textureType == TextureImporterType.Default && diffuseImporter.alphaIsTransparency)))
#else
			if (diffuseImporter != null && (!(diffuseImporter.textureType == TextureImporterType.Image && diffuseImporter.alphaIsTransparency)))
#endif
			{
#if UNITY_5_5_OR_NEWER
				diffuseImporter.textureType = TextureImporterType.Default;
#else
				diffuseImporter.textureType = TextureImporterType.Image;
#endif
				diffuseImporter.alphaIsTransparency = true;
				diffuseImporter.SaveAndReimport();
			}

			var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
#if UNITY_5_5_OR_NEWER
			if (normalImporter != null && (!(normalImporter.textureType == TextureImporterType.NormalMap && !normalImporter.convertToNormalmap)))
#else
			if (normalImporter != null && (!(normalImporter.textureType == TextureImporterType.Bump && normalImporter.normalmap && !normalImporter.convertToNormalmap)))
#endif
			{
#if UNITY_5_5_OR_NEWER
				normalImporter.textureType = TextureImporterType.NormalMap;
#else
				normalImporter.textureType = TextureImporterType.Bump;
				normalImporter.normalmap = true;
#endif
				normalImporter.convertToNormalmap = false;
				normalImporter.SaveAndReimport();
			}

			var specularImporter = AssetImporter.GetAtPath(specularPath) as TextureImporter;
#if UNITY_5_5_OR_NEWER
			if (specularImporter != null && (!(specularImporter.textureType == TextureImporterType.Default && specularImporter.alphaIsTransparency)))
#else
			if (specularImporter != null && (!(specularImporter.textureType == TextureImporterType.Image && specularImporter.alphaIsTransparency)))
#endif
			{
#if UNITY_5_5_OR_NEWER
				specularImporter.textureType = TextureImporterType.Default;
#else
				specularImporter.textureType = TextureImporterType.Image;
#endif
				specularImporter.alphaIsTransparency = true;
				specularImporter.SaveAndReimport();
			}

			var normal = AssetDatabase.LoadAssetAtPath<Texture>(normalPath);
			var specular = AssetDatabase.LoadAssetAtPath<Texture>(specularPath);
			mat.SetTexture("_BumpMap", normal);
			mat.SetTexture("_SpecGlossMap", specular);
			mat.EnableKeyword("_NORMALMAP");
			mat.EnableKeyword("_SPECGLOSSMAP");
			EditorUtility.SetDirty(mat);
		}
#endif

		private static bool checkIsValid(Terrain terrain, out TerrainData terrainData)
		{
			terrainData = terrain.terrainData;

			if (!terrain.enabled)
				return false;

			if (terrainData == null)
				return false;

			return true;
		}

		private static bool checkIsValid(MeshRenderer mr, out Mesh mesh, out Material[] materials)
		{
			mesh = null;
			materials = mr.sharedMaterials;

			if (!mr.enabled)
				return false;

			var meshFilter = mr.GetComponent<MeshFilter>();
			if (meshFilter == null)
				return false;

			mesh = meshFilter.sharedMesh;
			if (mesh == null)
				return false;

			if (materials == null || materials.Length == 0)
				return false;

			for (int i = 0; i < materials.Length; i++)
				if (materials[i] == null)
					return false;

			return true;
		}

		private static bool checkIsValid(SkinnedMeshRenderer smr, bool exportCloth, out Transform[] bones, out Mesh mesh, out Cloth cloth, out Material[] materials)
		{
			bones = smr.bones;
			mesh = smr.sharedMesh;
			materials = smr.sharedMaterials;
			cloth = exportCloth ? smr.GetComponent<Cloth>() : null;

			if (!smr.enabled)
				return false;

			if (bones == null)
				bones = new Transform[0];

			if (mesh == null)
				return false;

			if (bones.Length == 0)
			{
				if (cloth == null && mesh.blendShapeCount == 0)
					return false;
			}

			for (int i = 0; i < bones.Length; i++)
				if (bones[i] == null)
					return false;

			for (int i = 0; i < materials.Length; i++)
				if (materials[i] == null)
					return false;

			return true;
		}


		/// <summary>
		/// This method will give you the list of formats supported. The dual array output is convenience to match the unity int popup formats.
		/// </summary>
		/// <param name="filterFbx">List only fbx formats or all supported formats like collada, etc.</param>
		/// <param name="names">The names of the supported formats</param>
		/// <param name="formatIds">The matching id of the format name at the same index</param>
		public static void GetFormatNames(bool filterFbx, out string[] names, out int[] formatIds)
		{
			if (!UnityFbxExporterBinding.Init())
			{
				names = new string[] { "Platform not supported or dll is missing." };
				formatIds = new int[] { 0 };
				return;
			}
			try
			{
				UnityFbxExporterBinding.Create();
				try
				{
					var formatsCount = UnityFbxExporterBinding.FormatsCount();
					var namesList = new List<string>(formatsCount);
					var formatIdsList = new List<int>(formatsCount);
					for (int i = 0; i < formatsCount; i++)
					{
						if (filterFbx)
						{
							if (!UnityFbxExporterBinding.IsFormatFbx(i))
							{
								continue;
							}
						}
						formatIdsList.Add(i);
						UnityFbxExporterBinding.GetFormat(i, (value) => namesList.Add(value));
					}
					names = namesList.ToArray();
					formatIds = formatIdsList.ToArray();
				}
				finally
				{
					UnityFbxExporterBinding.Destroy();
				}
			}
			finally
			{
				UnityFbxExporterBinding.Close();
			}
		}

		public static string Version { get { return "1.6.0"; } }

		private static void SkinAddCluster(IntPtr fbxSkin, IntPtr fbxCluster, IntPtr fbxRoot, Transform bone)
		{
			var pos = bone.position;
			var euler = bone.eulerAngles;
			var scale = bone.lossyScale;
			UnityFbxExporterBinding.SkinAddCluster(fbxSkin, fbxCluster, fbxRoot, -euler.x, euler.y, euler.z, -pos.x, pos.y, pos.z, scale.x, scale.y, scale.z);
		}
	}
}