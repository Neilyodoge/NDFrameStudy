using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnLogickFactory
{
	public class FbxExportSettings
	{
		public FbxLODScheme LODScheme = new FbxLODScheme();
		public int logLevel = 0;
		public FbxTextureExportScheme textureScheme = null;
		public int formatId = FbxExporter.GetDefaultFormat();
		public bool embedTextures = false;
		public bool embedShaderProperty = false;
		public int terrainQuality = 0;

		public SkinnedMeshOptions skinnedMeshOptions = SkinnedMeshOptions.ExportCurrentPose;
		public BlendShapeOptions blendShapeOptions = BlendShapeOptions.Reset;
		public ObjectExportMask objectExportMask = ObjectExportMask.ExportAll;

		public FbxNodeCallback OnFbxNodeCreated;
		public FbxMeshCallback OnFbxMeshCreated;
		public FbxSkinnedMeshCallback OnFbxSkinnedMeshCreated;
		public FbxTerrainCallback OnFbxTerrainCreated;
		public FbxMaterialCallback OnFbxMaterialCreated;

		public bool ExportCloth { get { return (objectExportMask & ObjectExportMask.ExportCloth) == ObjectExportMask.ExportCloth; } }
		public bool ExportMeshes { get { return (objectExportMask & ObjectExportMask.ExportMeshes) == ObjectExportMask.ExportMeshes; } }
		public bool ExportSkinnedMeshes { get { return (objectExportMask & ObjectExportMask.ExportSkinnedMeshes) == ObjectExportMask.ExportSkinnedMeshes; } }
		public bool ExportTerrains { get { return (objectExportMask & ObjectExportMask.ExportTerrains) == ObjectExportMask.ExportTerrains; } }
		public bool ExportLights { get { return (objectExportMask & ObjectExportMask.ExportLights) == ObjectExportMask.ExportLights; } }
		public bool ExportCameras { get { return (objectExportMask & ObjectExportMask.ExportCameras) == ObjectExportMask.ExportCameras; } }
	}

	public class FbxExportCollection
	{
		public Dictionary<Transform, IntPtr> FbxNodes = new Dictionary<Transform, IntPtr>();
		public List<MeshRenderer> Meshes = new List<MeshRenderer>();
		public List<SkinnedMeshRenderer> SkinnedMeshes = new List<SkinnedMeshRenderer>();
		public List<Cloth> Cloths = new List<Cloth>();
		public List<Terrain> Terrains = new List<Terrain>();
		public List<Camera> Cameras = new List<Camera>();
		public List<Light> Lights = new List<Light>();
	}

	public enum ObjectExportMask
	{
		ExportMeshes = 1,
		ExportSkinnedMeshes = 2,
		ExportTerrains = 4,
		ExportCloth = 8,
		ExportCameras = 16,
		ExportLights = 32,
		ExportAll = 63
	}
	public enum SkinnedMeshOptions
	{
		ExportCurrentPose,
		ExportTPose
	}

	public enum BlendShapeOptions
	{
		Reset,
		WriteDeformations
	}

	public delegate void FbxNodeCallback(Transform transform, IntPtr fbxNode);
	public delegate void FbxMeshCallback(Transform transform, IntPtr fbxNode, MeshRenderer meshRenderer, IntPtr fbxMesh);
	public delegate void FbxTerrainCallback(Transform transform, IntPtr fbxNode, Terrain terrain, IntPtr fbxMesh);
	public delegate void FbxSkinnedMeshCallback(Transform transform, IntPtr fbxNode, SkinnedMeshRenderer skinnedMeshRenderer, IntPtr fbxMesh);
	public delegate void FbxMaterialCallback(Transform transform, IntPtr fbxNode, IntPtr fbxMaterial, IntPtr[] fbxTextures);
}