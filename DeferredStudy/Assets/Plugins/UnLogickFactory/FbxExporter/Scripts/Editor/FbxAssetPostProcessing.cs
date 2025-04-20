using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnLogickFactory
{
	public class FbxAssetPostProcessing : AssetPostprocessor
	{
		void OnPostprocessGameObjectWithUserProperties(
			GameObject go,
			string[] propNames,
			System.Object[] values)
		{
			for (int i = 0; i < propNames.Length; i++)
			{
				string propName = propNames[i];
				if (propName == "UnLogickFactory_FbxExporter_CustomProperty")
				{
					Debug.LogFormat("{0} - {1} = {2}", go, propName, values[i]);
					var shader = Shader.Find(values[i].ToString());
					if (shader != null)
					{
						Debug.LogFormat("Shader Found!");
					}
					else
					{
						var shaderProperty = JsonUtility.FromJson<FbxShaderProperty>(values[i].ToString());
						var materials = go.GetComponent<Renderer>().sharedMaterials;
						if (materials.Length == shaderProperty.shaders.Count)
						{
							EnsureShaders(materials, shaderProperty.shaders);
						}
					}
				}
			}
		}

		private void EnsureShaders(Material[] materials, List<string> shaders)
		{
			for(int i = 0; i < materials.Length; i++)
			{
				if(materials[i].shader.name != shaders[i])
				{
					var shader = Shader.Find(shaders[i]);
					if (shader != null)
					{
						Debug.LogFormat("Shader Applied!");
						materials[i].shader = shader;
					}
				}
			}
		}
	}
}