using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnLogickFactory
{
	/// <summary>
	/// This class allows you to set up an Fbx Exporter LOD handling scheme
	/// Currently this class allows you to set the Scheme public field.
	/// 
	/// It is kept as a separate class to allow you to derive from it and replace it with your own logic.
	/// </summary>
	[Serializable]
	public class FbxLODScheme
	{
		public LODScheme Scheme;
		public enum LODScheme
		{
			IgnoreLOD = 0,
			MainCamera = 1,
			OtherCameras = 2,
			AllCameras = 3,
			PerformCulling = 4,
			CullMainCamera = 5,
			CullOtherCameras = 6,
			CullAllCameras = 8,
		}

		public LODSchemeEnumMaskFlags SchemeMask
		{
			get { return (LODSchemeEnumMaskFlags)Scheme; }
			set { Scheme = (LODScheme)value; }
		}
		public enum LODSchemeEnumMaskFlags
		{
			MainCamera = 1,
			OtherCameras = 2,
			PerformCulling = 4
		}

		public virtual void Prepare(int logLevel)
		{
			_Prepare();
		}

		public virtual void GetRenderers(LODGroup lodGroup, FbxExportCollection result, FbxExportSettings settings)
		{
			_GetRenderers(lodGroup, result, settings);
		}

		#region internal workings of the Fbx LOD implementation
		Camera[] _lodCameras;


		private void _Prepare()
		{
			var cameraScheme = (LODScheme)((int)Scheme & 3);
			switch ((int)cameraScheme)
			{
				case (int)LODScheme.MainCamera:
					_lodCameras = new Camera[] { Camera.main };
					break;
				case 3:
					_lodCameras = Camera.allCameras;
					break;
				case (int)LODScheme.OtherCameras:
					var cameras = Camera.allCameras;
					_lodCameras = new Camera[cameras.Length - 1];
					int idx = 0;
					for (int i = 0; i < cameras.Length; i++)
					{
						var camera = cameras[i];
						if (camera == Camera.main)
							continue;
						_lodCameras[idx++] = camera;
					}
					break;
				default:
					_lodCameras = null;
					break;
			}
		}

		private void _GetRenderers(LODGroup lodGroup, FbxExportCollection result, FbxExportSettings settings)
		{
			if (!lodGroup.enabled)
				return;

			var exportLOD = DetermineLODGroup(lodGroup);
			if (exportLOD >= 0)
			{
				var lod = lodGroup.GetLODs()[exportLOD];
				bool cull = ((int)Scheme & (int)LODScheme.PerformCulling) == (int)LODScheme.PerformCulling;
				for (int i = 0; i < lod.renderers.Length; i++)
				{
					var renderer = lod.renderers[i];
					if (renderer == null) continue;
					if (cull && IsCulled(renderer))
					{
						FbxExporter.LogFormat(settings.logLevel >= 2, renderer.gameObject, "Fbx Exporter - Culled: {0}", renderer.name);
						continue;
					}
					if (renderer is MeshRenderer)
					{
						if (settings.ExportMeshes)
							result.Meshes.Add(renderer as MeshRenderer);
					}
					else if (renderer is SkinnedMeshRenderer)
					{
						var cloth = renderer.GetComponent<Cloth>();
						if (cloth != null)
						{
							if (settings.ExportCloth)
								result.Cloths.Add(cloth);
						}
						else
						{
							if (settings.ExportSkinnedMeshes)
								result.SkinnedMeshes.Add(renderer as SkinnedMeshRenderer);
						}
					}
					else
					{
						FbxExporter.LogFormat(settings.logLevel >= 2, renderer.gameObject, "Fbx Exporter - Ignoring unsupported renderer {0} (Type: {0})", renderer, renderer.GetType());
					}
				}
			}
			else
			{
				FbxExporter.LogFormat(settings.logLevel >= 2, lodGroup.gameObject, "Fbx Exporter - Lod Group discarded {0}", lodGroup);
			}
		}

		private bool IsCulled(Renderer renderer)
		{
			var bounds = renderer.bounds;
			var min = bounds.min;
			var max = bounds.max;
			renderer.name = renderer.sharedMaterial.name;

			var points = new Vector3[8]
			{
				new Vector3(min.x, min.y, min.z),
				new Vector3(max.x, min.y, min.z),
				new Vector3(min.x, max.y, min.z),
				new Vector3(max.x, max.y, min.z),
				new Vector3(min.x, min.y, max.z),
				new Vector3(max.x, min.y, max.z),
				new Vector3(min.x, max.y, max.z),
				new Vector3(max.x, max.y, max.z),
			};

			for (int i = 0; i < _lodCameras.Length; i++)
			{
				var camera = _lodCameras[i];
				if (((1 << renderer.gameObject.layer) & camera.cullingMask) == 0)
					continue;

				var WorldToCamera = camera.worldToCameraMatrix;
				var projection = camera.projectionMatrix;

				for (int j = 0; j < 8; j++)
				{
					var local = WorldToCamera.MultiplyPoint(points[j]);
					if (-local.z >= camera.nearClipPlane && -local.z <= camera.farClipPlane)
					{
						var view = projection.MultiplyPoint(local);
						if (view.x >= -1f && view.x <= 1 && view.y >= -1f && view.y <= 1)
							return false;
					}
				}
			}
			return true;
		}

		private int DetermineLODGroup(LODGroup lodGroup)
		{
			if ((int)Scheme == 0)
				return 0;

			var exportLOD = lodGroup.lodCount;
			for (int i = 0; i < _lodCameras.Length; i++)
			{
				var camera = _lodCameras[i];
				var cameraLOD = DetermineLOD(lodGroup, camera);
				if (cameraLOD >= 0 && cameraLOD < exportLOD)
					exportLOD = cameraLOD;
			}

			if (exportLOD == lodGroup.lodCount)
				exportLOD = -1;

			return exportLOD;
		}

		private static int DetermineLOD(LODGroup lodGroup, Camera camera)
		{
			var lodPosition = lodGroup.transform.localToWorldMatrix.MultiplyPoint(lodGroup.localReferencePoint);
			var upVector = lodGroup.size * camera.transform.up.normalized * lodGroup.transform.lossyScale.y * 0.5f;

			var lodPosition1 = camera.transform.position + camera.transform.forward * (camera.transform.position - lodPosition).magnitude;
			var lodPosition2 = lodPosition1 + upVector;

			var camMatrix = camera.worldToCameraMatrix;
			var projectionMatrix = camera.projectionMatrix;
			var camPos1 = projectionMatrix.MultiplyPoint(camMatrix.MultiplyPoint(lodPosition1));
			var camPos2 = projectionMatrix.MultiplyPoint(camMatrix.MultiplyPoint(lodPosition2));

			var ratio = (camPos2.y - camPos1.y) * QualitySettings.lodBias;

			var lods = lodGroup.GetLODs();
			int result;
			for (result = lods.Length - 1; result >= 0; result--)
			{
				if (ratio < lods[result].screenRelativeTransitionHeight)
				{
					break;
				}
			}

			result++;
			if (result == lods.Length)
				result = -1;

			return result;
		}
		#endregion
	}
}