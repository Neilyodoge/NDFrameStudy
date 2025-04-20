// Copyright 2020, UnLogick Factory IVS
// This file is released to the public domain
#if !UNITY_WSA && !UNITY_WSA_10_0
using System;
using System.Reflection;
using UnityEngine;
#endif

namespace YOURNAMESPACEHERE.Integrations
{
	public static class UnLogickFbxExporterIntegration
	{
#if !UNITY_WSA && !UNITY_WSA_10_0
		private static Type fbxExporter;
		private static Type GetFbxExporterType()
		{
			if (fbxExporter == null)
			{
				fbxExporter = Type.GetType("UnLogickFactory.FbxExporter");
			}
			return fbxExporter;
		}

		public static bool HasFbxExporter()
		{
			return GetFbxExporterType() != null;
		}

		private static MethodInfo exportMethodInfo;
		public static void Export(string filename, Transform[] roots)
		{
			if (exportMethodInfo == null)
			{
				var exporterType = GetFbxExporterType();
				exportMethodInfo = exporterType.GetMethod("Export", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(Transform[]) }, null);
				if (exportMethodInfo == null)
				{
					Debug.LogErrorFormat("UnLogickFactory.FbxExporter.Export(string filename, Transform[] roots) not found. Exiting without exporting.");
					return;
				}
			}
			exportMethodInfo.Invoke(null, new object[] { filename, roots });
		}

		private static MethodInfo openEditorWindow;
		public static void OpenEditorWindow()
		{
			if (openEditorWindow == null)
			{
				var editorType = Type.GetType("UnLogickFactory.FbxExporterWindow");
				if (editorType == null)
					return;

				openEditorWindow = editorType.GetMethod("ShowFbxExporterWindow", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				if (openEditorWindow == null)
				{
					Debug.LogErrorFormat("UnLogickFactory.FbxExporterWindow.ShowFbxExporterWindow() not found.");
					return;
				}
			}
			openEditorWindow.Invoke(null, null);
		}
#else
		public static bool HasFbxExporter()
		{
			return false
		}

		public static void Export(string filename, Transform[] roots)
		{
		}

		public static void OpenEditorWindow()
		{
		}
#endif
	}
}
