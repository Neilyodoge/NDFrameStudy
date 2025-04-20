using UnityEngine;
using System;

namespace UnLogickFactory
{
	public abstract class FbxCustomPropertyBehaviour : MonoBehaviour
	{
		public FbxCustomProperty[] customProperties;

		public void Apply(IntPtr target)
		{
			foreach(var property in customProperties)
			{
				property.Apply(target);
			}
		}
	}
}