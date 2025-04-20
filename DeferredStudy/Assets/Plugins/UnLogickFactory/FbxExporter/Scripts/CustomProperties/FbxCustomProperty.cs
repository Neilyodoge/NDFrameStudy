using System;
using UnityEngine;

namespace UnLogickFactory
{
	[Serializable]
	public class FbxCustomProperty
	{
		public enum FbxCustomPropertyType
		{
			Color,
			Double4,
			Double3,
			Double2,
			Matrix,
			Double,
			Bool,
			Long,
			String,
		}
		[SerializeField]
		FbxCustomPropertyType type;
		[SerializeField]
		double m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44;
		[SerializeField]
		bool boolValue;
		[SerializeField]
		string stringValue;
		[SerializeField]
		long longValue;
		[SerializeField]
		string propertyName = "";

		public void SetValue(float value)
		{
			type = FbxCustomPropertyType.Double;
			m11 = (double)value;
		}

		public void SetValue(Vector2 value)
		{
			type = FbxCustomPropertyType.Double2;
			m11 = (double)value.x;
			m12 = (double)value.y;
		}

		public void SetValue(Vector3 value)
		{
			type = FbxCustomPropertyType.Double3;
			m11 = (double)value.x;
			m12 = (double)value.y;
			m13 = (double)value.z;
		}


		public void SetValue(Vector4 value)
		{
			type = FbxCustomPropertyType.Double4;
			m11 = (double)value.x;
			m12 = (double)value.y;
			m13 = (double)value.z;
			m13 = (double)value.w;
		}

		public void SetValue(Color value)
		{
			type = FbxCustomPropertyType.Color;
			m11 = (double)value.r;
			m12 = (double)value.g;
			m13 = (double)value.b;
			m13 = (double)value.a;
		}

		public void SetValue(Matrix4x4 value)
		{
			type = FbxCustomPropertyType.Matrix;
			m11 = (double)value.m00;
			m12 = (double)value.m01;
			m13 = (double)value.m02;
			m14 = (double)value.m03;
			m21 = (double)value.m10;
			m22 = (double)value.m11;
			m23 = (double)value.m12;
			m24 = (double)value.m13;
			m31 = (double)value.m20;
			m32 = (double)value.m21;
			m33 = (double)value.m22;
			m34 = (double)value.m23;
			m41 = (double)value.m30;
			m42 = (double)value.m31;
			m43 = (double)value.m32;
			m44 = (double)value.m33;
		}

		public void SetValue(bool value)
		{
			type = FbxCustomPropertyType.Bool;
			boolValue = value;
		}

		public void SetValue(string value)
		{
			type = FbxCustomPropertyType.String;
			stringValue = value;
		}

		public void SetValue(long value)
		{
			type = FbxCustomPropertyType.Long;
			longValue = value;
		}

		public void Apply(IntPtr target)
		{
			switch (type)
			{
				case FbxCustomPropertyType.Color:
					UnityFbxExporterBinding.SetColorProperty(target, propertyName, m11, m12, m13, m14);
					break;
				case FbxCustomPropertyType.Double4:
					UnityFbxExporterBinding.SetDouble4Property(target, propertyName, m11, m12, m13, m14);
					break;
				case FbxCustomPropertyType.Double3:
					UnityFbxExporterBinding.SetDouble3Property(target, propertyName, m11, m12, m13);
					break;
				case FbxCustomPropertyType.Double2:
					UnityFbxExporterBinding.SetDouble2Property(target, propertyName, m11, m12);
					break;
				case FbxCustomPropertyType.Matrix:
					UnityFbxExporterBinding.SetMatrixProperty(target, propertyName, m11, m12, m13, m14, m21, m22, m23, m24, m31, m32, m33, m34, m41, m42, m43, m44);
					break;
				case FbxCustomPropertyType.Double:
					UnityFbxExporterBinding.SetDoubleProperty(target, propertyName, m11);
					break;
				case FbxCustomPropertyType.Bool:
					UnityFbxExporterBinding.SetBoolProperty(target, propertyName, boolValue);
					break;
				case FbxCustomPropertyType.Long:
					UnityFbxExporterBinding.SetLongProperty(target, propertyName, longValue);
					break;
				case FbxCustomPropertyType.String:
					UnityFbxExporterBinding.SetStringProperty(target, propertyName, stringValue);
					break;
				default:
					throw new NotImplementedException(string.Format("FbxCustomProperty type ({0}) not supported.", (int)type));
			}
		}
	}
}