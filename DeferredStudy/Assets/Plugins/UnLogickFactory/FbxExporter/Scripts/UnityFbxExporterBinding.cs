using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace UnLogickFactory
{
	public static class UnityFbxExporterBinding
	{
		static Vector3 rotAdjust = Vector3.one;
		static Vector3 posAdjust = Vector3.one;
		static Vector3 scaleAdjust = Vector3.one;
		const string dllFilenameBase = "UnityFbxExporter";
		const string dllVersion = "16";
		const string dllExtension = "dll";

		public static string GetDLLPath(int logLevel = 0)
		{
			var platform = IntPtr.Size == 8 ? "x86_64" : "x86";
			var dllFilename = string.Format("{0}_{1}_v{2}.{3}", dllFilenameBase, platform, dllVersion, dllExtension);

#if UNITY_EDITOR_WIN
			var dlls = UnityEditor.AssetDatabase.FindAssets(System.IO.Path.GetFileNameWithoutExtension(dllFilename));
			foreach (var dll in dlls)
			{
				var importer = UnityEditor.AssetImporter.GetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(dll)) as UnityEditor.PluginImporter;
				if (!importer.GetCompatibleWithEditor())
				{
					FbxExporter.LogWarningFormat(logLevel >= 0, "Fbx Exporter - There was a matching dll at {0} but it was flagged not compatible with editor.", importer.assetPath);
					continue;
				}

				return importer.assetPath;
			}

			FbxExporter.LogErrorFormat(true, "Fbx Exporter - No dll marked compatible with the current editor by the name {0}", System.IO.Path.GetFileNameWithoutExtension(dllFilename));

			return null;
#else
            var pluginFilename = System.IO.Path.Combine(Application.streamingAssetsPath, @"Plugins/" + dllFilename);
            if (System.IO.File.Exists(pluginFilename))
            {
                return pluginFilename;
            }

            pluginFilename = System.IO.Path.Combine(System.IO.Path.GetFileName(Application.dataPath), @"Plugins/"+ dllFilename);
            if ( System.IO.File.Exists(pluginFilename))
            {
                return pluginFilename;
            }

            pluginFilename = System.IO.Path.Combine(System.IO.Path.GetFileName(Application.dataPath), @"Plugins/"+ platform + "/" + dllFilename);
            if ( System.IO.File.Exists(pluginFilename))
            {
                return pluginFilename;
            }

            if (System.IO.File.Exists(dllFilename))
            {
                return dllFilename;
            }

            FbxExporter.LogErrorFormat(true, "Fbx Exporter - No dll found\nApplication.dataPath="+Application.dataPath+"\nApplication.streamingAssetsPath="+Application.streamingAssetsPath);
            return null;
#endif
		}

		public static bool Init(int logLevel = 0)
		{
			if (exportSceneDll != IntPtr.Zero)
				return true;

			if (!FbxExporter.FbxSupported)
			{
				FbxExporter.LogErrorFormat(true,
					"Fbx Exporter - Fbx Not Supported on this system\n" +
					"The Unity Fbx Exporter currently only supports the Windows 32 bit and 64 bit platforms");
				return false;
			}

			rotAdjust = new Vector3(1, -1, -1);
			posAdjust = new Vector3(1, 1, 1);
			scaleAdjust = new Vector3(1, 1, 1);

			var dllPath = GetDLLPath();
			if (dllPath == null)
			{
#if UNITY_EDITOR
				FbxExporter.LogErrorFormat(true,
					"Fbx Exporter - DLL not found!\n" +
					"DLL either not present or set to not be loaded for the current editor platform.");
				return false;
#else
                FbxExporter.LogErrorFormat(true,
                    "Fbx Exporter - DLL not found!\n" +
                    "To fix this either place the DLL in a streaming assets folder or place it next to the executable.");
                return false;
#endif
			}
			exportSceneDll = LoadLibrary(dllPath);
			if (exportSceneDll == IntPtr.Zero)
			{
#if UNITY_EDITOR
				FbxExporter.LogErrorFormat(true,
					"Fbx Exporter - DLL unable to load!\n" +
					"The Fbx Exporter DLL was located at {0} but failed to load.",
					dllPath);
				return false;
#else
                throw new Exception(string.Format(
                    "Fbx Exporter - DLL unable to load!\n" +
                    "The Fbx Exporter DLL was located at {0} but failed to load.",
                    dllPath));
#endif
			}
			try
			{
				SetupMethod("UnityFbxExporter_Create", ref Create);
				SetupMethod("UnityFbxExporter_Destroy", ref Destroy);
				SetupMethod("UnityFbxExporter_Save", ref Save);
				SetupMethod("UnityFbxExporter_MeshCreate", ref MeshCreate);
				SetupMethod("UnityFbxExporter_MeshGetNode", ref MeshGetNode);
				SetupMethod("UnityFbxExporter_MeshSetVertices", ref MeshSetVertices);
				SetupMethod("UnityFbxExporter_MeshSetNormals", ref MeshSetNormals);
				SetupMethod("UnityFbxExporter_MeshSetColor", ref MeshSetColors);
				SetupMethod("UnityFbxExporter_MeshAddTriangles", ref MeshAddTriangles);
				SetupMethod("UnityFbxExporter_CreateTexture", ref CreateTexture);
				SetupMethod("UnityFbxExporter_CreatePhongMaterial", ref CreatePhongMaterial);
				SetupMethod("UnityFbxExporter_SetTextureScale", ref SetTextureScale);
				SetupMethod("UnityFbxExporter_MaterialSetTexture", ref MaterialSetTexture);
				SetupMethod("UnityFbxExporter_MeshAddMaterial", ref MeshAddMaterial);
				SetupMethod("UnityFbxExporter_MeshSetUV", ref MeshSetUV);
				SetupMethod("UnityFbxExporter_SkeletonCreateRoot", ref SkeletonCreateRoot);
				SetupMethod("UnityFbxExporter_CreateNode", ref CreateNode);
				SetupMethod("UnityFbxExporter_MakeNodeSkeleton", ref MakeNodeSkeleton);
				SetupMethod("UnityFbxExporter_SkeletonCreateLimb", ref SkeletonCreateLimb);
				SetupMethod("UnityFbxExporter_NodeSetLocal", ref NodeSetLocal);
				SetupMethod("UnityFbxExporter_MeshCreateSkin", ref MeshCreateSkin);
				SetupMethod("UnityFbxExporter_ClusterCreate", ref ClusterCreate);
				SetupMethod("UnityFbxExporter_SkinAddCluster", ref SkinAddCluster);
				SetupMethod("UnityFbxExporter_ClusterAddWeight", ref ClusterAddWeight);
				SetupMethod("UnityFbxExporter_MeshStoreBindPose", ref MeshStoreBindPose);
				SetupMethod("UnityFbxExporter_MeshSetBindPose", ref MeshSetBindPose);
				SetupMethod("UnityFbxExporter_ClusterSetLinkMatrix", ref ClusterSetLinkMatrix);
				SetupMethod("UnityFbxExporter_NodeSetPreRotation", ref NodeSetPreRotation);
				SetupMethod("UnityFbxExporter_UpdateTransformation", ref UpdateTransformation);
				SetupMethod("UnityFbxExporter_CreateBindPose", ref CreateBindPose);
				SetupMethod("UnityFbxExporter_AddBoneToPose", ref AddBoneToPose);
				SetupMethod("UnityFbxExporter_IsValidBindPoseVerbose", ref IsValidBindPoseVerbose);
				SetupMethod("UnityFbxExporter_FbxStatusCreate", ref FbxStatusCreate);
				SetupMethod("UnityFbxExporter_NodeListCreate", ref NodeListCreate);
				SetupMethod("UnityFbxExporter_NodeListSize", ref NodeListSize);
				SetupMethod("UnityFbxExporter_NodeListGetAt", ref NodeListGetAt);
				SetupMethod("UnityFbxExporter_NodeGetName", ref NodeGetName);
				SetupMethod("UnityFbxExporter_PoseGetClassId", ref PoseGetClassId);
				SetupMethod("UnityFbxExporter_NodeGetClassId", ref NodeGetClassId);
				SetupMethod("UnityFbxExporter_LogFormats", ref LogFbxFormats);
				SetupMethod("UnityFbxExporter_FormatsCount", ref FormatsCount);
				SetupMethod("UnityFbxExporter_GetFormat", ref GetFormat);
				SetupMethod("UnityFbxExporter_IsFormatFbx", ref IsFormatFbx);
				SetupMethod("UnityFbxExporter_SetColorProperty", ref SetColorProperty);
				SetupMethod("UnityFbxExporter_SetBoolProperty", ref SetBoolProperty);
				SetupMethod("UnityFbxExporter_SetLongProperty", ref SetLongProperty);
				SetupMethod("UnityFbxExporter_SetDoubleProperty", ref SetDoubleProperty);
				SetupMethod("UnityFbxExporter_SetDouble2Property", ref SetDouble2Property);
				SetupMethod("UnityFbxExporter_SetDouble3Property", ref SetDouble3Property);
				SetupMethod("UnityFbxExporter_SetDouble4Property", ref SetDouble4Property);
				SetupMethod("UnityFbxExporter_SetDouble4x4Property", ref SetMatrixProperty);
				SetupMethod("UnityFbxExporter_SetStringProperty", ref SetStringProperty);
				SetupMethod("UnityFbxExporter_CameraCreate", ref CameraCreate);
				SetupMethod("UnityFbxExporter_LightCreate", ref LightCreate);
				SetupMethod("UnityFbxExporter_AddBlendShape", ref AddBlendShape);
				SetupMethod("UnityFbxExporter_AddBlendChannel", ref AddBlendChannel);
				SetupMethod("UnityFbxExporter_AddBlendFrame", ref AddBlendFrame);
				SetupMethod("UnityFbxExporter_SetDeformPercent", ref SetBlendShapeDeformPercent);
				return true;
			}
			catch (Exception e)
			{
				Close();
				Debug.LogException(e);
				return false;
			}
		}

		private static IntPtr LoadLibrary(string path)
		{
#if UNITY_EDITOR
			var directory = System.IO.Path.GetDirectoryName(path);
			var filename = System.IO.Path.GetFileNameWithoutExtension(path);
			var extension = System.IO.Path.GetExtension(path);

			int i = 0;
			while (System.IO.File.Exists(System.IO.Path.Combine(directory, filename + "_" + i + extension)))
			{
				path = System.IO.Path.Combine(directory, filename + "_" + i + extension);
				i++;
			}
			if (i != 0)
			{
				FbxExporter.LogFormat(true, "Loading Debug DLL: {0}", path);
			}
#endif
			if (!System.IO.File.Exists(path))
				return IntPtr.Zero;

			return NativeMethods.LoadLibrary(path);
		}

		static class NativeMethods
		{
			[DllImport("kernel32.dll")]
			public static extern IntPtr LoadLibrary(string dllToLoad);

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);


			[DllImport("kernel32.dll")]
			public static extern bool FreeLibrary(IntPtr hModule);
		}

		public delegate void LogCallback(string msg);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int IntVoid();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate string StringInt(int value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BoolInt(int value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidLogCallback(LogCallback msg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntLogCallback(int index, LogCallback msg);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidVoid();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BoolString(String value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BoolStringInt(String arg0, int arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BoolStringIntBool(String arg0, int arg1, bool arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrString(String value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringDouble3(String value, double r, double g, double b);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringIntPtrDoubleDoubleDouble(String value, IntPtr node, double nearPlane, double farPlane, double fieldOfView);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringIntPtrETypeDoubleDoubleDoubleDoubleBoolBool(String value, IntPtr node, EType lightType, double r, double g, double b, double intensity, bool enabled, bool shadows);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringIntPtr(String arg0, IntPtr arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringIntPtrDouble(String arg0, IntPtr arg1, double arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrVoid(LogCallback logCallback);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtr(IntPtr value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtrDoubleArrayInt(IntPtr arg0, double[] arg1, int arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDoubleArrayInt(IntPtr arg0, double[] arg1, int arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrIntIntArrayInt(IntPtr arg0, int arg1, int[] arg2, int arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrStringStringETextureUseEAlphaSource(String arg0, string arg1, ETextureUse arg2, EAlphaSource arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrIntPtrTextureChannels(IntPtr arg0, IntPtr arg1, TextureChannels arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrIntPtr(IntPtr arg0, IntPtr arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDoubleArrayIntString(IntPtr arg0, double[] arg1, int arg2, String arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtrString(IntPtr arg0, string arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDouble3x3(IntPtr node, double rot_x, double rot_y, double rot_z, double pos_x, double pos_y, double pos_z, double scale_x, double scale_y, double scale_z);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtr3(IntPtr arg0, IntPtr arg1, IntPtr arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtr3Double3x3(IntPtr arg0, IntPtr arg1, IntPtr arg2, double rot_x, double rot_y, double rot_z, double pos_x, double pos_y, double pos_z, double scale_x, double scale_y, double scale_z);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrIntDouble(IntPtr arg0, int arg1, double arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtr(IntPtr arg0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidStringIntPtrArrayDoubleArrayInt(string arg0, IntPtr[] arg1, double[] arg2, int arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDoubleArray(IntPtr cluster, double[] matrix);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDouble3(IntPtr arg0, double arg1, double arg2, double arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDouble4(IntPtr arg0, double arg1, double arg2, double arg3, double arg4);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrIntPtrDouble16(IntPtr arg0, IntPtr arg1, double p00, double p10, double p20, double p30, double p01, double p11, double p21, double p31, double p02, double p12, double p22, double p32, double p03, double p13, double p23, double p33);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BoolIntPtr6DoubleIntPtr(IntPtr arg0, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, double arg6, IntPtr arg7, LogCallback logCallback);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int IntIntPtr(IntPtr arg0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate IntPtr IntPtrIntPtrInt(IntPtr arg0, int arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate String StringIntPtr(IntPtr arg0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringDouble4(IntPtr arg0, string arg1, double arg2, double arg3, double arg4, double arg5);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringDouble3(IntPtr arg0, string arg1, double arg2, double arg3, double arg4);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringDouble2(IntPtr arg0, string arg1, double arg2, double arg3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringDouble(IntPtr arg0, string arg1, double arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrDouble(IntPtr arg0, double arg1);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringBool(IntPtr arg0, string arg1, bool arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringLong(IntPtr arg0, string arg1, long arg2);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringMatrix(IntPtr arg0, string arg1, double p00, double p10, double p20, double p30, double p01, double p11, double p21, double p31, double p02, double p12, double p22, double p32, double p03, double p13, double p23, double p33);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void VoidIntPtrStringString(IntPtr arg0, string arg1, string arg2);

		static IntPtr exportSceneDll;

		private static void SetupMethod<T>(string methodName, ref T methodDelegate)
			where T : class
		{
			var methodPtr = NativeMethods.GetProcAddress(exportSceneDll, methodName);
			if (methodPtr == IntPtr.Zero)
				throw new EntryPointNotFoundException(methodName);
			methodDelegate = Marshal.GetDelegateForFunctionPointer(methodPtr, typeof(T)) as T;
		}

		public static void Close()
		{
			// Unfortunately Unity doesn't really support Unloading of dlls... but this would be the code to unload the dll if they fix that.
			//if (exportSceneDll != IntPtr.Zero)
			//{
			//	NativeMethods.FreeLibrary(exportSceneDll);
			//	exportSceneDll = IntPtr.Zero;
			//}
		}

		public static VoidVoid Create;
		public static VoidVoid Destroy;
		public static BoolStringIntBool Save;
		public static IntPtrStringIntPtr MeshCreate;
		public static VoidLogCallback LogFbxFormats;
		public static IntVoid FormatsCount;
		public static VoidIntLogCallback GetFormat;
		public static BoolInt IsFormatFbx;

		public static IntPtrIntPtr MeshGetNode; //UnityFbxExporter_MeshGetNode
		public static IntPtrIntPtrDoubleArrayInt MeshSetVertices; // UnityFbxExporter_MeshSetVertices
		public static VoidIntPtrDoubleArrayInt MeshSetNormals; // UnityFbxExporter_MeshSetNormals
		public static VoidIntPtrDoubleArrayInt MeshSetColors;
		public static VoidIntPtrIntIntArrayInt MeshAddTriangles; // UnityFbxExporter_MeshAddTriangles
		public static IntPtrStringStringETextureUseEAlphaSource CreateTexture; //UnityFbxExporter_CreateTexture
		public static IntPtrStringDouble3 CreatePhongMaterial; // UnityFbxExporter_CreatePhongMaterial
		public static VoidIntPtrDouble4 SetTextureScale; // UnityFbxExporter_SetTextureScale
		public static VoidIntPtrIntPtrTextureChannels MaterialSetTexture; //UnityFbxExporter_MaterialSetTexture
		public static VoidIntPtrIntPtr MeshAddMaterial; //UnityFbxExporter_MeshAddMaterial
		public static VoidIntPtrDoubleArrayIntString MeshSetUV; // UnityFbxExporter_MeshSetUV

		public static IntPtrIntPtrString CreateNode; //UnityFbxExporter_SkeletonCreateRoot
		public static VoidIntPtr MakeNodeSkeleton; //UnityFbxExporter_SkeletonCreateRoot

		public static IntPtrString SkeletonCreateRoot; //UnityFbxExporter_SkeletonCreateRoot
		public static IntPtrIntPtrString SkeletonCreateLimb; //UnityFbxExporter_SkeletonCreateLimb
		public static VoidIntPtrDouble3x3 NodeSetLocal; //UnityFbxExporter_NodeSetLocal

		public static IntPtrIntPtr MeshCreateSkin;//UnityFbxExporter_MeshCreateSkin
		public static IntPtrIntPtr ClusterCreate;//UnityFbxExporter_ClusterCreate
		public static IntPtrIntPtr3Double3x3 SkinAddCluster;//UnityFbxExporter_SkinAddCluster
		public static VoidIntPtrIntDouble ClusterAddWeight;//UnityFbxExporter_ClusterAddWeight
		public static VoidIntPtr MeshStoreBindPose;//UnityFbxExporter_MeshStoreBindPose
		public static VoidStringIntPtrArrayDoubleArrayInt MeshSetBindPose;//UnityFbxExporter_MeshSetBindPose
		public static VoidIntPtrDoubleArray ClusterSetLinkMatrix;//UnityFbxExporter_ClusterSetLinkMatrix
		public static VoidIntPtrDouble3 NodeSetPreRotation;
		public static VoidIntPtr UpdateTransformation;
		public static IntPtrVoid CreateBindPose;
		public static VoidIntPtrIntPtrDouble16 AddBoneToPose;

		public static BoolIntPtr6DoubleIntPtr IsValidBindPoseVerbose;
		public static IntPtrVoid FbxStatusCreate;
		public static IntPtrVoid NodeListCreate;
		public static IntIntPtr NodeListSize;
		public static IntPtrIntPtrInt NodeListGetAt;
		public static StringIntPtr NodeGetName;

		public static StringIntPtr PoseGetClassId;
		public static StringIntPtr NodeGetClassId;

		public static VoidIntPtrStringDouble4 SetColorProperty;
		public static VoidIntPtrStringDouble4 SetDouble4Property;
		public static VoidIntPtrStringDouble3 SetDouble3Property;
		public static VoidIntPtrStringDouble2 SetDouble2Property;
		public static VoidIntPtrStringMatrix SetMatrixProperty;
		public static VoidIntPtrStringDouble SetDoubleProperty;
		public static VoidIntPtrStringBool SetBoolProperty;
		public static VoidIntPtrStringLong SetLongProperty;
		public static VoidIntPtrStringString SetStringProperty;
		public static IntPtrStringIntPtrDoubleDoubleDouble CameraCreate;
		public static IntPtrStringIntPtrETypeDoubleDoubleDoubleDoubleBoolBool LightCreate;

		public static IntPtrIntPtr AddBlendShape;
		public static IntPtrStringIntPtr AddBlendChannel;
		public static IntPtrStringIntPtrDouble AddBlendFrame;
		public static VoidIntPtrDouble SetBlendShapeDeformPercent;



		public enum EType
		{
			ePoint = 0,
			eDirectional = 1,
			eSpot = 2,
			eArea = 3,
			eVolume = 4
		};


		// This matches the fbx format, DO NOT MODIFY
		public enum ETextureUse
		{
			eStandard = 0,                  //! Standard texture use (ex. image)
			eShadowMap = 1,                 //! Shadow map
			eLightMap = 2,                  //! Light map
			eSphericalReflectionMap = 3,    //! Spherical reflection map: Object reflects the contents of the scene
			eSphereReflectionMap = 4,       //! Sphere reflection map: Object reflects the contents of the scene from only one point of view
			eBumpNormalMap = 5              //! Bump map: Texture contains two direction vectors, that are used to convey relief in a texture.
		};

		// This matches the fbx format, DO NOT MODIFY
		public enum EAlphaSource
		{
			eNone = 0,          //! No Alpha.
			eRGBIntensity = 1,  //! RGB Intensity (computed).
			eBlack = 2          //! Alpha channel. Black is 100% transparency, white is opaque.
		};

		// This matches the fbx exporter dll, DO NOT MODIFY
		public enum TextureChannels
		{
			Diffuse = 0,
			Emissive = 1,
			Ambient = 2,
			NormalMap = 3,
			BumpMap = 4,
			TransparentColor = 5,
			Shininess = 6,
			Specular = 7,
			Reflection = 8
		}

		public static IntPtr ProcessMesh(Mesh sourceMesh, Material[] materials, IntPtr node)
		{
			IntPtr[] fbxMaterials;
			IntPtr[][] fbxTextures;
			return ProcessMesh(sourceMesh, 0, materials, null, null, node, null, out fbxMaterials, out fbxTextures);
		}
		public static IntPtr ProcessMesh(Mesh sourceMesh, Material[] materials)
		{
			IntPtr[] fbxMaterials;
			IntPtr[][] fbxTextures;
			return ProcessMesh(sourceMesh, 0, materials, null, null, IntPtr.Zero, null, out fbxMaterials, out fbxTextures);
		}
		public static IntPtr ProcessMesh(Mesh sourceMesh, int logLevel, Material[] materials, Vector3[] overrideVertices, Vector3[] overrideNormals, IntPtr node, Dictionary<Material, IntPtr> KnownMaterials)
		{
			IntPtr[] fbxMaterials;
			IntPtr[][] fbxTextures;
			return ProcessMesh(sourceMesh, logLevel, materials, overrideVertices, overrideNormals, node, KnownMaterials, out fbxMaterials, out fbxTextures);
		}
		public static IntPtr ProcessMesh(Mesh sourceMesh, int logLevel, Material[] materials, Vector3[] overrideVertices, Vector3[] overrideNormals, IntPtr node, Dictionary<Material, IntPtr> KnownMaterials, out IntPtr[] fbxMaterials, out IntPtr[][] fbxTextures)
		{
			return ProcessMesh(sourceMesh, logLevel, materials, overrideVertices, overrideNormals, node, KnownMaterials, out fbxMaterials, out fbxTextures, BlendShapeOptions.Reset);
		}


		public static SkinnedMeshRenderer blendSmr;
		public static Mesh blendMesh;
		public static float[] blendWeights;

		public static IntPtr ProcessMesh(Mesh sourceMesh, int logLevel, Material[] materials, Vector3[] overrideVertices, Vector3[] overrideNormals, IntPtr node, Dictionary<Material, IntPtr> KnownMaterials, out IntPtr[] fbxMaterials, out IntPtr[][] fbxTextures, BlendShapeOptions blendShapeOptions)
		{
			if (!sourceMesh.isReadable)
			{
				fbxMaterials = null;
				fbxTextures = null;
				return IntPtr.Zero;
			}

			IntPtr mesh = MeshCreate(sourceMesh.name, node);
			var vertices = sourceMesh.vertices;
			var normals = sourceMesh.normals;
			if (overrideVertices != null) vertices = overrideVertices;
			if (overrideNormals != null) normals = overrideNormals;

			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i].Scale(posAdjust);
			}

			var uvs = sourceMesh.uv;
			double[] doubleData = new double[vertices.Length * 4];

			CopyPointsToDoubles(doubleData, vertices);
			MeshSetVertices(mesh, doubleData, vertices.Length);

			CopyPointsToDoubles(doubleData, normals);
			MeshSetNormals(mesh, doubleData, normals.Length);

			CopyVectorsToDoubles(doubleData, uvs);
			MeshSetUV(mesh, doubleData, uvs.Length, "uv1");

			uvs = sourceMesh.uv2;
			if (uvs != null && uvs.Length == vertices.Length)
			{
				FbxExporter.LogFormat(logLevel >= 1, "Fbx Exporter - exporting UV2");
				CopyVectorsToDoubles(doubleData, uvs);
				MeshSetUV(mesh, doubleData, uvs.Length, "uv2");
			}

			uvs = sourceMesh.uv3;
			if (uvs != null && uvs.Length == vertices.Length)
			{
				FbxExporter.LogFormat(logLevel >= 1, "Fbx Exporter - exporting UV3");
				CopyVectorsToDoubles(doubleData, uvs);
				MeshSetUV(mesh, doubleData, uvs.Length, "uv3");
			}

			uvs = sourceMesh.uv4;
			if (uvs != null && uvs.Length == vertices.Length)
			{
				FbxExporter.LogFormat(logLevel >= 1, "Fbx Exporter - exporting UV4");
				CopyVectorsToDoubles(doubleData, uvs);
				MeshSetUV(mesh, doubleData, uvs.Length, "uv4");
			}

			var colors = sourceMesh.colors;
			if (colors != null && colors.Length == vertices.Length)
			{
				CopyColorsToDoubles(doubleData, colors);
				MeshSetColors(mesh, doubleData, colors.Length);
			}

			for (int i = 0; i < sourceMesh.subMeshCount; i++)
			{
				var triangles = sourceMesh.GetTriangles(i);
				FlipTriangles(triangles);
				MeshAddTriangles(mesh, i, triangles, triangles.Length / 3);
			}

			var blendShapeCount = sourceMesh.blendShapeCount;
			if (blendShapeCount > 0)
			{
				Vector3[] deltaVertices = new Vector3[sourceMesh.vertexCount];
				Vector3[] deltaNormals = new Vector3[sourceMesh.vertexCount];
				//Vector4[] deltaTangents = new Vector4[sourceMesh.vertexCount];
				var blendShape = AddBlendShape(mesh);
				for (int i = 0; i < blendShapeCount; i++)
				{
					var blendShapeNameChannel = sourceMesh.GetBlendShapeName(i);
					var blendShapeName = blendShapeNameChannel;
					if (blendShapeName.Contains("."))
						blendShapeName = blendShapeName.Substring(blendShapeName.LastIndexOf('.') + 1);
					var channel = AddBlendChannel(blendShapeNameChannel, blendShape);
					var frames = sourceMesh.GetBlendShapeFrameCount(i);

					for (int j = 0; j < frames; j++)
					{
						var weight = sourceMesh.GetBlendShapeFrameWeight(i, j);
						FbxExporter.LogFormat(logLevel >= 1, "Defining BlendShape: {0} Weight: {1}", blendShapeName, weight);

						var frame = AddBlendFrame(string.Format(j == 0 ? "{0}" : "{0}_{1}", blendShapeName, j), channel, (double)weight);

						blendSmr.SetBlendShapeWeight(i, sourceMesh.GetBlendShapeFrameWeight(i, j));
						blendSmr.BakeMesh(blendMesh);
						blendSmr.SetBlendShapeWeight(i, 0);

						deltaVertices = blendMesh.vertices;
						deltaNormals = blendMesh.normals;
						//sourceMesh.GetBlendShapeFrameVertices(i, j, deltaVertices, deltaNormals, deltaTangents);

						//for(int k = 0; k < sourceMesh.vertexCount; k++)
						//{
						//	deltaVertices[k].Scale(posAdjust);

						//	deltaVertices[k].x += vertices[k].x;
						//	deltaVertices[k].y += vertices[k].y;
						//	deltaVertices[k].z += vertices[k].z;
						//	//deltaVertices[k].x = -deltaVertices[k].x;
						//}
						CopyPointsToDoubles(doubleData, deltaVertices);
						MeshSetVertices(frame, doubleData, sourceMesh.vertexCount);

						//for (int k = 0; k < sourceMesh.vertexCount; k++)
						//{
						//	deltaNormals[k] += normals[k];
						//}
						CopyPointsToDoubles(doubleData, deltaNormals);
						MeshSetNormals(frame, doubleData, sourceMesh.vertexCount);
					}

					if (blendShapeOptions == BlendShapeOptions.WriteDeformations)
					{
						SetBlendShapeDeformPercent(channel, blendWeights[i]);
						FbxExporter.LogFormat(logLevel >= 1, "Writing BlendShape: {0} Weight: {1}", blendShapeName, blendWeights[i]);
					}
				}
			}

			fbxMaterials = new IntPtr[materials.Length];
			fbxTextures = new IntPtr[materials.Length][];
			var fbxTextureList = new List<IntPtr>();
			for (int i = 0; i < materials.Length; i++)
			{
				var mat = materials[i];
				IntPtr material;
				if (KnownMaterials == null || !KnownMaterials.TryGetValue(mat, out material))
				{
					var matName = mat.name;
					if (matName.EndsWith(" (Instance)"))
						matName = matName.Substring(0, matName.Length - 11);

					Color color;
					if (mat.HasProperty("_Color"))
					{
						color = mat.color;
					}
					else
					{
						color = Color.white;
					}

					material = CreatePhongMaterial(matName, (double)color.r, (double)color.g, (double)color.b);
					fbxMaterials[i] = material;
					IntPtr texture;
					texture = AddTextureToMaterial(material, mat, "_MainTex", TextureChannels.Diffuse, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_BumpMap", TextureChannels.BumpMap, ETextureUse.eBumpNormalMap, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_BumpTex", TextureChannels.BumpMap, ETextureUse.eBumpNormalMap, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_SpecularMap", TextureChannels.Specular, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_Specular", TextureChannels.Specular, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_SpecularTex", TextureChannels.Specular, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_SpecGlossMap", TextureChannels.Specular, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_Spc", TextureChannels.Specular, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_Ambient", TextureChannels.Ambient, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_AO", TextureChannels.Ambient, ETextureUse.eStandard, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_NormalMap", TextureChannels.NormalMap, ETextureUse.eBumpNormalMap, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);
					texture = AddTextureToMaterial(material, mat, "_NormalTex", TextureChannels.NormalMap, ETextureUse.eBumpNormalMap, logLevel);
					if (texture != IntPtr.Zero) fbxTextureList.Add(texture);

					fbxTextures[i] = fbxTextureList.ToArray();

					if (KnownMaterials != null)
						KnownMaterials.Add(mat, material);
				}
				MeshAddMaterial(mesh, material);
			}
			return mesh;
		}

		public static IntPtr ProcessTerrain(Terrain terrain, TerrainData terrainData, int terrainQuality, IntPtr node, out IntPtr material, out IntPtr[] textures)
		{
			IntPtr mesh = MeshCreate(terrain.name, node);
#if UNITY_2018_3_OR_NEWER
			var heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

			var width = terrainData.heightmapResolution;
			var height = terrainData.heightmapResolution;
#else
			var heights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);
			var width = terrainData.heightmapWidth;
			var height = terrainData.heightmapHeight;
#endif
			var terrainSampling = 1;

			if (terrainQuality == 0)
			{
				while (width * height > 32000)
				{
					terrainSampling *= 2;
					width = (width - 1) / 2 + 1;
					height = (height - 1) / 2 + 1;
				}
			}
			else
			{
				for (int i = 1; i < terrainQuality; i++)
				{
					width = (width - 1) / 2 + 1;
					height = (height - 1) / 2 + 1;
					terrainSampling *= 2;
				}
			}
			if (width < 2 || height < 2)
			{
				material = IntPtr.Zero;
				textures = new IntPtr[0];
				return IntPtr.Zero;
			}

			var verticesLength = width * height;
			var doubleData = new double[verticesLength * 3];

			Vector3 meshScale = terrainData.size;
			int index = 0;

			double xStep = -((double)meshScale.x) / ((double)(width - 1));
			double yStep = ((double)meshScale.z) / ((double)(height - 1));

			double xPos = 0;
			double yPos = 0;

			for (int x = 0; x < width; x++)
			{
				yPos = 0;
				for (int y = 0; y < height; y++)
				{
					doubleData[index++] = xPos;
					doubleData[index++] = (double)(heights[y * terrainSampling, x * terrainSampling] * meshScale.y);
					doubleData[index++] = yPos;
					yPos += yStep;
				}
				xPos += xStep;
			}

			MeshSetVertices(mesh, doubleData, verticesLength);

			xStep = 1.0 / ((double)width);
			yStep = 1.0 / ((double)height);

			xPos = 0;
			yPos = 0;
			index = 0;

			for (int x = 0; x < width; x++)
			{
				yPos = 0;
				for (int y = 0; y < height; y++)
				{
					doubleData[index++] = xPos;
					doubleData[index++] = yPos;
					yPos += yStep;
				}
				xPos += xStep;
			}
			MeshSetUV(mesh, doubleData, verticesLength, "diffuse");

			var triangleCount = (width - 1) * (height - 1) * 2;
			int[] triangles = new int[triangleCount * 3];
			index = 0;

			for (int x = 0; x < width - 1; x++)
			{
				for (int y = 0; y < height - 1; y++)
				{
					int origin = x * height + y;
					triangles[index++] = origin;
					triangles[index++] = origin + 1 + height;
					triangles[index++] = origin + 1;

					triangles[index++] = origin;
					triangles[index++] = origin + height;
					triangles[index++] = origin + 1 + height;
				}
			}

			MeshAddTriangles(mesh, 0, triangles, triangles.Length / 3);

			material = CreatePhongMaterial(terrain.name + "_Mat", 1, 1, 1);
			textures = new IntPtr[3]
			{
				CreateTexture(string.Format("Textures/{0}_Diffuse.png", terrain.name), string.Format("{0}_Diffuse", terrain.name), ETextureUse.eStandard, EAlphaSource.eBlack),
				CreateTexture(string.Format("Textures/{0}_Normal.png", terrain.name), string.Format("{0}_Normal", terrain.name), ETextureUse.eBumpNormalMap, EAlphaSource.eNone),
				CreateTexture(string.Format("Textures/{0}_Specular.png", terrain.name), string.Format("{0}_Specular", terrain.name), ETextureUse.eStandard, EAlphaSource.eBlack)
			};
			MaterialSetTexture(material, textures[0], TextureChannels.Diffuse);
			MaterialSetTexture(material, textures[1], TextureChannels.BumpMap);
			MaterialSetTexture(material, textures[2], TextureChannels.Specular);
			MeshAddMaterial(mesh, material);
			return mesh;
		}

		private static void FlipTriangles(int[] triangles)
		{
			for (int i = 0; i < triangles.Length; i += 3)
			{
				int temp = triangles[i + 2];
				triangles[i + 2] = triangles[i + 1];
				triangles[i + 1] = temp;
			}
		}

		private static IntPtr AddTextureToMaterial(IntPtr material, Material mat, string shaderName, TextureChannels channel, ETextureUse textureUse, int logLevel)
		{
			if (mat.HasProperty(shaderName))
			{
				var tex = mat.GetTexture(shaderName);
				if (tex != null)
				{
					var alphaSource = textureUse == ETextureUse.eBumpNormalMap ? EAlphaSource.eNone : EAlphaSource.eBlack;
					var textureName = tex.name + ".png";
#if UNITY_EDITOR
					var assetPath = System.IO.Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(tex));
					if (!string.IsNullOrEmpty(assetPath))
						textureName = assetPath;
#endif
					FbxExporter.LogFormat(logLevel >= 2, "Fbx Exporter - exporting Texture {0} as channel {1}", textureName, channel);
					var texture = CreateTexture("Textures/" + textureName, tex.name, textureUse, alphaSource);
					MaterialSetTexture(material, texture, channel);
					return texture;
				}
			}
			return IntPtr.Zero;
		}

		private static void CopyVectorsToDoubles(double[] dest, Vector3[] source)
		{
			int index = 0;
			for (int i = 0; i < source.Length; i++)
			{
				AddVector3ToArray(dest, ref source[i], ref index);
			}
		}

		private static void CopyPointsToDoubles(double[] dest, Vector3[] source)
		{
			int index = 0;
			for (int i = 0; i < source.Length; i++)
			{
				AddPoint3ToArray(dest, ref source[i], ref index);
			}
		}

		private static void CopyColorsToDoubles(double[] dest, Color[] source)
		{
			int index = 0;
			for (int i = 0; i < source.Length; i++)
			{
				AddColorToArray(dest, ref source[i], ref index);
			}
		}

		private static void CopyVectorsToDoubles(double[] dest, Vector2[] source)
		{
			int index = 0;
			for (int i = 0; i < source.Length; i++)
			{
				AddVector2ToArray(dest, ref source[i], ref index);
			}
		}

		private static void AddVector3ToArray(double[] doubleData, ref Vector3 vector, ref int index)
		{
			doubleData[index++] = (double)vector.x;
			doubleData[index++] = (double)vector.y;
			doubleData[index++] = (double)vector.z;
		}

		private static void AddPoint3ToArray(double[] doubleData, ref Vector3 vector, ref int index)
		{
			doubleData[index++] = (double)-vector.x;
			doubleData[index++] = (double)vector.y;
			doubleData[index++] = (double)vector.z;
		}

		private static void AddColorToArray(double[] doubleData, ref Color color, ref int index)
		{
			doubleData[index++] = (double)color.r;
			doubleData[index++] = (double)color.g;
			doubleData[index++] = (double)color.b;
			doubleData[index++] = (double)color.a;
		}

		private static void AddVector2ToArray(double[] doubleData, ref Vector2 vector, ref int index)
		{
			doubleData[index++] = (double)vector.x;
			doubleData[index++] = (double)vector.y;
		}

		public static FbxExportCollection ScanHierarchy(Transform rootBone, bool createRoot, FbxExportSettings settings)
		{
			var result = new FbxExportCollection();
			var root = createRoot ? CreateNode(IntPtr.Zero, rootBone.name) : IntPtr.Zero;
			result.FbxNodes.Add(rootBone, root);
			if (createRoot)
			{
				NodeSetTransform(root, rootBone, true);
				if (settings.OnFbxNodeCreated != null)
					settings.OnFbxNodeCreated(rootBone, root);

				var nodeCreatedComponent = rootBone.GetComponent<FbxNodeCustomProperties>();
				if (nodeCreatedComponent != null)
					nodeCreatedComponent.Apply(root);
			}
			RecursiveAddTransforms(rootBone, root, result, !createRoot, settings, false);
			if ((settings.objectExportMask & ObjectExportMask.ExportLights) != ObjectExportMask.ExportLights)
				result.Lights = new List<Light>();
			if ((settings.objectExportMask & ObjectExportMask.ExportCameras) != ObjectExportMask.ExportCameras)
				result.Cameras = new List<Camera>();
			if ((settings.objectExportMask & ObjectExportMask.ExportMeshes) != ObjectExportMask.ExportMeshes)
				result.Meshes = new List<MeshRenderer>();
			if ((settings.objectExportMask & ObjectExportMask.ExportSkinnedMeshes) != ObjectExportMask.ExportSkinnedMeshes)
				result.SkinnedMeshes = new List<SkinnedMeshRenderer>();
			if ((settings.objectExportMask & ObjectExportMask.ExportTerrains) != ObjectExportMask.ExportTerrains)
				result.Terrains = new List<Terrain>();
			return result;
		}

		private static void RecursiveAddTransforms(Transform bone, IntPtr fbxBone, FbxExportCollection result, bool rootLevel, FbxExportSettings settings, bool belowLODGroup)
		{
			var lodGroup = bone.GetComponent<LODGroup>();
			if (lodGroup != null)
			{
				settings.LODScheme.GetRenderers(lodGroup, result, settings);
				belowLODGroup = true;
			}
			if (!belowLODGroup)
			{
				if (settings.ExportMeshes)
				{
					var mesh = bone.GetComponent<MeshRenderer>();
					if (mesh != null)
						result.Meshes.Add(mesh);
				}
				Cloth cloth = null;
				if (settings.ExportCloth)
				{
					cloth = bone.GetComponent<Cloth>();
					if (cloth != null)
					{
						result.Cloths.Add(cloth);
					}
				}
				if (cloth == null && settings.ExportSkinnedMeshes)
				{
					var skinnedMesh = bone.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMesh != null)
					{
						result.SkinnedMeshes.Add(skinnedMesh);
					}
				}
				if (settings.ExportTerrains)
				{
					var terrain = bone.GetComponent<Terrain>();
					if (terrain != null)
					{
						result.Terrains.Add(terrain);
					}
				}
				if (settings.ExportLights)
				{
					var light = bone.GetComponent<Light>();
					if (light != null)
					{
						result.Lights.Add(light);
					}
				}
				if (settings.ExportCameras)
				{
					var camera = bone.GetComponent<Camera>();
					if (camera != null)
					{
						result.Cameras.Add(camera);
					}
				}
			}

			for (int i = 0; i < bone.childCount; i++)
			{
				var child = bone.GetChild(i);
				if (!child.gameObject.activeSelf) continue;
				var fbxChild = CreateNode(fbxBone, child.name);
				result.FbxNodes.Add(child, fbxChild);
				NodeSetTransform(fbxChild, child, rootLevel);
				if (settings.OnFbxNodeCreated != null)
					settings.OnFbxNodeCreated(child, fbxChild);
				var nodeCreatedComponent = child.GetComponent<FbxNodeCustomProperties>();
				if (nodeCreatedComponent != null)
					nodeCreatedComponent.Apply(fbxChild);
				RecursiveAddTransforms(child, fbxChild, result, false, settings, belowLODGroup);
			}
		}

		private static void AddBoneWeight(ref BoneWeight boneWeight, IntPtr[] fbxClusters, int boneWeightIndex)
		{
			if (boneWeight.weight0 > 0) ClusterAddWeight(fbxClusters[boneWeight.boneIndex0], boneWeightIndex, (double)boneWeight.weight0);
			if (boneWeight.weight1 > 0) ClusterAddWeight(fbxClusters[boneWeight.boneIndex1], boneWeightIndex, (double)boneWeight.weight1);
			if (boneWeight.weight2 > 0) ClusterAddWeight(fbxClusters[boneWeight.boneIndex2], boneWeightIndex, (double)boneWeight.weight2);
			if (boneWeight.weight3 > 0) ClusterAddWeight(fbxClusters[boneWeight.boneIndex3], boneWeightIndex, (double)boneWeight.weight3);
		}

		public static void AddBoneWeights(BoneWeight[] boneWeights, IntPtr[] fbxClusters)
		{
			for (int i = 0; i < boneWeights.Length; i++)
			{
				AddBoneWeight(ref boneWeights[i], fbxClusters, i);
			}
		}

		public static void NodeSetTransform(IntPtr node, Transform transform, bool rootLevel)
		{
			var rot = rootLevel ? transform.eulerAngles : transform.localEulerAngles;
			rot.Scale(rotAdjust);
			var pos = rootLevel ? transform.position : transform.localPosition;
			pos.Scale(posAdjust);
			var scale = rootLevel ? transform.lossyScale : transform.localScale;
			scale.Scale(scaleAdjust);
			NodeSetLocal(node, (double)rot.x, (double)rot.y, (double)rot.z, (double)-pos.x, (double)pos.y, (double)pos.z, (double)scale.x, (double)scale.y, (double)scale.z);
		}

		public static void CameraSetTransform(IntPtr node, Transform transform, bool rootLevel)
		{
			var rot = rootLevel ? transform.eulerAngles : transform.localEulerAngles;
			rot.Scale(rotAdjust);
			var pos = rootLevel ? transform.position : transform.localPosition;
			pos.Scale(posAdjust);
			var scale = rootLevel ? transform.lossyScale : transform.localScale;
			scale.Scale(scaleAdjust);
			NodeSetLocal(node, (double)rot.x, (double)rot.y, (double)rot.z, (double)-pos.x, (double)pos.y, (double)pos.z, (double)scale.x, (double)scale.y, (double)scale.z);
		}
	}
}
