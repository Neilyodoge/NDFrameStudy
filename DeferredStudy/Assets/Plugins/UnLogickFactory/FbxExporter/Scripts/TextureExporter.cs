using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

namespace UnLogickFactory
{
	[ExecuteInEditMode]
	public class TextureExporter : MonoBehaviour
	{
		Camera _exportCamera;
		public Texture2D _diffuseTexture;
		public Texture2D _normalMapTexture;
		public Texture2D _specularMapTexture;
		public RenderTexture rt;
		public Renderer _textureExporter;

		Material _normalMat;
		Material _specularMat;
		Material _diffuseMat;
		public string _normalMapFilename;
		public string _specularMapFilename;
		public string _diffuseTextureFilename;

		Dictionary<Material, Material> AlreadyProcessedMaterials = new Dictionary<Material, Material>();
		Material lastMaterial;
		int renderTextureSize;
		private FbxTextureExportScheme currentScheme;

		public static TextureExporter CreateTextureExporter()
		{
			var go = Resources.Load("UnLogickFactory/FbxExporter/TextureExporter") as GameObject;
#if UNITY_EDITOR
			if (go == null)
				go = UnityEditor.AssetDatabase.LoadMainAssetAtPath("Assets/Plugins/UnLogickFactory/FbxExporter/Prefabs/TextureExporter.prefab") as GameObject;
#endif
			if (go == null)
				throw new NullReferenceException("UnLogickFactory - FbxExporter, cannot find Texture Exporter prefab!\nAt runtime it must be in 'Resources/UnLogickFactory/FbxExporter/' and named 'TextureExporter.prefab'\nIn the editor you can also put it at 'Assets/Plugins/UnLogickFactory/FbxExporter/Prefabs/TextureExporter.prefab'");
			go = Instantiate<GameObject>(go);
			go.hideFlags = HideFlags.DontSave;

			return go.GetComponent<TextureExporter>();
		}

		void Awake()
		{
			_exportCamera = GetComponent<Camera>();
			_exportCamera.enabled = false;

			_normalMat = new Material(Shader.Find("Hidden/UnLogickFactory/FbxExporter/ExportNormal"));
			_normalMat.hideFlags = HideFlags.HideAndDontSave;

			_specularMat = new Material(Shader.Find("Hidden/UnLogickFactory/FbxExporter/ExportSpecular"));
			_specularMat.hideFlags = HideFlags.HideAndDontSave;

			_diffuseMat = new Material(Shader.Find("Hidden/UnLogickFactory/FbxExporter/ExportDiffuse"));
			_diffuseMat.hideFlags = HideFlags.HideAndDontSave;
		}

		void OnDestroy()
		{
			foreach (var mat in AlreadyProcessedMaterials.Values)
			{
				DestroyImmediate(mat.GetTexture("_SpecGlossMap"), false);
				DestroyImmediate(mat.GetTexture("_MainTex"), false);
				DestroyImmediate(mat.GetTexture("_BumpMap"), false);
				DestroyImmediate(mat, false);
			}
		}

		public void ProcessTerrain(Terrain terrain, int resolution, FbxTextureExportScheme textureScheme, string path)
		{
			currentScheme = textureScheme;

			if (resolution > SystemInfo.maxTextureSize)
				resolution = SystemInfo.maxTextureSize;

			CleanupLastMaterial();
			textureScheme.AllocateTexture(ref _diffuseTexture, resolution, resolution);
			textureScheme.AllocateTexture(ref _specularMapTexture, resolution, resolution);
			textureScheme.AllocateTexture(ref _normalMapTexture, resolution, resolution);

			var data = terrain.terrainData;
			var alphaMaps = data.alphamapTextures;
			var mats = new Material[alphaMaps.Length];

#if UNITY_2018_3_OR_NEWER
			var layers = data.terrainLayers;
			for (int i = 0; i < alphaMaps.Length; i++)
			{
				mats[i] = SetupTerrainMaterial(terrain, i, layers, alphaMaps[i]);
			}
#else
			var splats = data.splatPrototypes;
			for (int i = 0; i < alphaMaps.Length; i++)
			{
				mats[i] = SetupTerrainMaterial(terrain, i, splats, alphaMaps[i]);
			}
#endif
			_normalMapFilename = Path.Combine(path, string.Format("Textures/{0}_Normal.{1}", terrain.name, textureScheme.textureExtension));
			_diffuseTextureFilename = Path.Combine(path, string.Format("Textures/{0}_Diffuse.{1}", terrain.name, textureScheme.textureExtension));
			_specularMapFilename = Path.Combine(path, string.Format("Textures/{0}_Specular.{1}", terrain.name, textureScheme.textureExtension));
			_diffuseMat.SetFloat("_ColorSpace", textureScheme.CalcColorSpace());

			_textureExporter.sharedMaterial = mats[0];
			var terrainRenderGOs = new GameObject[alphaMaps.Length];
			for (int i = 1; i < alphaMaps.Length; i++)
			{
				if (mats[i] == null)
				{
					continue;
				}
				terrainRenderGOs[i] = UnityEngine.Object.Instantiate(_textureExporter.gameObject) as GameObject;
				terrainRenderGOs[i].transform.SetParent(_textureExporter.transform.parent);
				terrainRenderGOs[i].transform.localPosition = _textureExporter.transform.localPosition;
				terrainRenderGOs[i].transform.localRotation = _textureExporter.transform.localRotation;
				terrainRenderGOs[i].transform.localScale = _textureExporter.transform.localScale;

				var renderer = terrainRenderGOs[i].GetComponent<MeshRenderer>();
				renderer.sharedMaterial = mats[i];
				renderer.enabled = true;
			}
			PerformTextureExport(resolution, resolution);
			for (int i = 1; i < alphaMaps.Length; i++)
			{
				if (terrainRenderGOs[i] != null)
				{
					DestroyImmediate(terrainRenderGOs[i], false);
				}
			}
			currentScheme = null;
		}

		public Material ProcessMaterial(Material mat, Renderer renderer, FbxTextureExportScheme textureScheme, string path)
		{
			Material result;
			if (!AlreadyProcessedMaterials.TryGetValue(mat, out result))
			{
				currentScheme = textureScheme;
				int resolutionX;
				int resolutionY;
				CleanupLastMaterial();
				textureScheme.AllocateTextures(mat, renderer, ref _diffuseTexture, ref _specularMapTexture, ref _normalMapTexture, out resolutionX, out resolutionY);
				if (_specularMapTexture != null)
				{
					_specularMapTexture.name = string.Format("{0}_{1}_Specular", mat.name, mat.GetInstanceID());
					_specularMapFilename = Path.Combine(path, string.Format("Textures/{0}.{1}", _specularMapTexture.name, textureScheme.textureExtension));
				}
				if (_diffuseTexture != null)
				{
					_diffuseTexture.name = string.Format("{0}_{1}_Diffuse", mat.name, mat.GetInstanceID());
					_diffuseTextureFilename = Path.Combine(path, string.Format("Textures/{0}.{1}", _diffuseTexture.name, textureScheme.textureExtension));
				}
				if (_normalMapTexture != null)
				{
					_normalMapTexture.name = string.Format("{0}_{1}_Normal", mat.name, mat.GetInstanceID());
					_normalMapFilename = Path.Combine(path, string.Format("Textures/{0}.{1}", _normalMapTexture.name, textureScheme.textureExtension));
				}

				if (_diffuseTexture != null || _normalMapTexture != null || _specularMapTexture != null)
				{
					_diffuseMat.SetFloat("_ColorSpace", textureScheme.CalcColorSpace());

					_textureExporter.sharedMaterial = mat;
					while (resolutionX > SystemInfo.maxTextureSize || resolutionY > SystemInfo.maxTextureSize)
					{
						resolutionX /= 2;
						resolutionY /= 2;
					}

					PerformTextureExport(resolutionX, resolutionY);
				}

				result = new Material(Shader.Find("Hidden/UnLogickFactory/FbxExporter/TextureExporterPlaceHolder"));
				result.name = string.Format("{0}_{1}", mat.name, mat.GetInstanceID());
				result.SetTexture("_SpecGlossMap", _specularMapTexture);
				result.SetTexture("_MainTex", _diffuseTexture);
				result.SetTexture("_BumpMap", _normalMapTexture);
				if (_diffuseTexture == null && mat.HasProperty("_Color"))
				{
					result.color = mat.color;
				}
				AlreadyProcessedMaterials.Add(mat, result);
				lastMaterial = result;
				currentScheme = null;
			}
			return result;
		}

		protected void CleanupLastMaterial()
		{
			if (lastMaterial != null)
			{
				lastMaterial.SetTexture("_SpecGlossMap", MakeDummyTexture(_specularMapTexture));
				lastMaterial.SetTexture("_MainTex", MakeDummyTexture(_diffuseTexture));
				lastMaterial.SetTexture("_BumpMap", MakeDummyTexture(_normalMapTexture));
				lastMaterial = null;
			}
			_diffuseTexture = null;
			_specularMapTexture = null;
			_normalMapTexture = null;
		}

		protected Texture MakeDummyTexture(Texture texture)
		{
			if (texture == null)
			{
				return null;
			}
			var newTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, true);
			newTexture.name = texture.name;
			return newTexture;
		}

#if UNITY_2018_3_OR_NEWER
		protected Material SetupTerrainMaterial(Terrain terrain, int layer, TerrainLayer[] layers, Texture2D control)
		{
			Material result;
#if UNITY_2019_1_OR_NEWER
			Shader shader = layer > 0 ? terrain.materialTemplate.shader.GetDependency("AddPassShader") : terrain.materialTemplate.shader;
#else
			Shader shader;
			switch (terrain.materialType)
			{
				case Terrain.MaterialType.Custom:
					shader = terrain.materialTemplate.shader;
					break;
				case Terrain.MaterialType.BuiltInStandard:
					shader = layer == 0 ? Shader.Find("Nature/Terrain/Standard") : Shader.Find("Hidden/TerrainEngine/Splatmap/Standard-AddPass");
					break;
				case Terrain.MaterialType.BuiltInLegacyDiffuse:
					shader = layer == 0 ? Shader.Find("Nature/Terrain/Diffuse") : Shader.Find("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass");
					break;
				case Terrain.MaterialType.BuiltInLegacySpecular:
					shader = layer == 0 ? Shader.Find("Nature/Terrain/Specular") : Shader.Find("Hidden/TerrainEngine/Splatmap/Specular-AddPass");
					break;
				default:
					shader = layer == 0 ? Shader.Find("Nature/Terrain/Standard") : Shader.Find("Hidden/TerrainEngine/Splatmap/Standard-AddPass");
					break;
			}
#endif
			if (shader == null)
			{
				return null;
			}
			result = new Material(shader);
			result.SetTexture("_Control", control);
			for (int i = 0; i < 4; i++)
			{
				var idx = layer * 4 + i;
				if (idx < layers.Length)
				{
					var size = layers[idx].tileSize;
					size.x = terrain.terrainData.size.x / layers[idx].tileSize.x;
					size.y = terrain.terrainData.size.z / layers[idx].tileSize.y;

					result.SetTexture("_Splat" + i, layers[idx].diffuseTexture);
					result.SetTextureOffset("_Splat" + i, new Vector2(layers[idx].tileOffset.x / layers[idx].tileSize.x, layers[idx].tileOffset.y / layers[idx].tileSize.y));
					result.SetTextureScale("_Splat" + i, size);
					result.SetTexture("_Normal" + i, layers[idx].normalMapTexture);
					result.SetFloat("_Smoothness" + i, layers[idx].smoothness);
					result.SetFloat("_Metallic" + i, layers[idx].metallic);
					result.SetFloat("_NormalScale" + i, layers[idx].normalScale);
				}
			}
			result.EnableKeyword("_NORMALMAP");
			result.EnableKeyword("UNITY_HDR_ON");

			return result;
		}

#else

		protected Material SetupTerrainMaterial(Terrain terrain, int layer, SplatPrototype[] splats, Texture2D control)
		{
			Material result;
			switch (terrain.materialType)
			{
				case Terrain.MaterialType.Custom:
					result = new Material(terrain.materialTemplate);
					break;
				case Terrain.MaterialType.BuiltInStandard:
					result = new Material(layer == 0 ? Shader.Find("Nature/Terrain/Standard") : Shader.Find("Hidden/TerrainEngine/Splatmap/Standard-AddPass"));
					break;
				case Terrain.MaterialType.BuiltInLegacyDiffuse:
					result = new Material(layer == 0 ? Shader.Find("Nature/Terrain/Diffuse") : Shader.Find("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass"));
					break;
				case Terrain.MaterialType.BuiltInLegacySpecular:
					result = new Material(layer == 0 ? Shader.Find("Nature/Terrain/Specular") : Shader.Find("Hidden/TerrainEngine/Splatmap/Specular-AddPass"));
					break;
				default:
					result = new Material(layer == 0 ? Shader.Find("Nature/Terrain/Standard") : Shader.Find("Hidden/TerrainEngine/Splatmap/Standard-AddPass"));
					break;
			}
			result.SetTexture("_Control", control);
			for (int i = 0; i < 4; i++)
			{
				var idx = layer * 4 + i;
				if (idx < splats.Length)
				{
					var size = splats[idx].tileSize;
					size.x = terrain.terrainData.size.x / splats[idx].tileSize.x;
					size.y = terrain.terrainData.size.z / splats[idx].tileSize.y;

					result.SetTexture("_Splat" + i, splats[idx].texture);
					result.SetTextureOffset("_Splat" + i, new Vector2(splats[idx].tileOffset.x / splats[idx].tileSize.x, splats[idx].tileOffset.y / splats[idx].tileSize.y));
					result.SetTextureScale("_Splat" + i, size);
					result.SetTexture("_Normal" + i, splats[idx].normalMap);
					result.SetFloat("_Smoothness" + i, splats[idx].smoothness);
					result.SetFloat("_Metallic" + i, splats[idx].metallic);
				}
			}
			result.EnableKeyword("_TERRAIN_NORMAL_MAP");

			return result;
		}
#endif

		protected void PerformTextureExport(int resolutionX, int resolutionY)
		{
			bool hasMainTex = _textureExporter.sharedMaterial.HasProperty("_MainTex");
			Vector2 oldMainTextureScale = new Vector2();
			if (hasMainTex)
			{
				oldMainTextureScale = _textureExporter.sharedMaterial.mainTextureScale;
				if (resolutionX > resolutionY)
				{
					_textureExporter.sharedMaterial.mainTextureScale = new Vector2(1, resolutionX / resolutionY);
				}
				else
				{
					_textureExporter.sharedMaterial.mainTextureScale = new Vector2(resolutionY / resolutionX, 1);
				}
			}

			if (resolutionX > resolutionY)
			{
				renderTextureSize = resolutionX;
			}
			else
			{
				renderTextureSize = resolutionY;
			}

			var rt = RenderTexture.GetTemporary(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			_textureExporter.enabled = true;
			_exportCamera.targetTexture = rt;
			_exportCamera.Render();
			_exportCamera.targetTexture = null;
			_textureExporter.enabled = false;
			RenderTexture.ReleaseTemporary(rt);

			if (hasMainTex)
			{
				_textureExporter.sharedMaterial.mainTextureScale = oldMainTextureScale;
			}
		}

		public virtual void OnRenderImage(RenderTexture src, RenderTexture dest)
		{
			if (_specularMapTexture != null)
			{
				ExportSingleTexture(src, _specularMapTexture, _specularMat, _specularMapFilename, 2);
			}
			if (_diffuseTexture != null)
			{
				ExportSingleTexture(src, _diffuseTexture, _diffuseMat, _diffuseTextureFilename, 0);
			}
			if (_normalMapTexture != null)
			{
				ExportSingleTexture(src, _normalMapTexture, _normalMat, _normalMapFilename, 1);
			}
		}

		protected void ExportSingleTexture(RenderTexture src, Texture2D dest, Material mat, string filename, int textureType)
		{
			var rt = RenderTexture.GetTemporary(renderTextureSize, renderTextureSize, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
			Graphics.Blit(src, rt, mat);
			RenderTexture.active = rt;
			dest.ReadPixels(new Rect(0, 0, dest.width, dest.height), 0, 0, false);
			RenderTexture.active = null;

			bool valid = true;
			if (textureType == 1)
			{
				valid = VerifyNormals(dest);
			}
			else if (textureType == 2)
			{
				valid = VerifySpecular(dest);
			}

			if (valid)
			{
				switch (currentScheme.textureFormat)
				{
					case FbxTextureExportScheme.TextureExportFormat.Png:
						WriteAllBytes(filename, dest.EncodeToPNG());
						break;
					case FbxTextureExportScheme.TextureExportFormat.Jpeg:
						WriteAllBytes(filename, dest.EncodeToJPG());
						break;
					default:
						WriteAllBytes(filename, dest.EncodeToPNG());
						break;
				}
			}
			RenderTexture.ReleaseTemporary(rt);
		}

		private bool VerifySpecular(Texture2D dest)
		{
			return true;
		}

		private bool VerifyNormals(Texture2D dest)
		{
			return true;
		}

		protected void WriteAllBytes(string filename, byte[] data)
		{
			EnsureFolderExists(Path.GetDirectoryName(filename));
			using (var file = File.Open(filename, FileMode.OpenOrCreate))
			{
				file.Write(data, 0, data.Length);
				file.Flush();
			}
		}

		protected void EnsureFolderExists(string folder)
		{
			if (string.IsNullOrEmpty(folder))
				return;
			if (!Directory.Exists(folder))
			{
				EnsureFolderExists(Path.GetDirectoryName(folder));
				Directory.CreateDirectory(folder);
			}
		}
	}
}