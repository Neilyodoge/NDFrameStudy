#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnLogickFactory
{
	[CustomPropertyDrawer(typeof(FbxCustomProperty))]
	public class FbxCustomPropertyPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0;
		}
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var typeproperty = property.FindPropertyRelative("type");
			EditorGUILayout.PropertyField((SerializedProperty)typeproperty, new GUIContent("Custom Property Type"));
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(property.FindPropertyRelative("propertyName"));
			switch ((FbxCustomProperty.FbxCustomPropertyType)typeproperty.enumValueIndex)
			{
				case FbxCustomProperty.FbxCustomPropertyType.Color:
					EditorGUI.BeginChangeCheck();
					var color = new Color(property.FindPropertyRelative("m11").floatValue, property.FindPropertyRelative("m12").floatValue, property.FindPropertyRelative("m13").floatValue, property.FindPropertyRelative("m14").floatValue);
					color = EditorGUILayout.ColorField("Color", color);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = color.r;
						property.FindPropertyRelative("m12").floatValue = color.g;
						property.FindPropertyRelative("m13").floatValue = color.b;
						property.FindPropertyRelative("m14").floatValue = color.a;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Double4:
					EditorGUI.BeginChangeCheck();
					var vector4 = new Vector4(property.FindPropertyRelative("m11").floatValue, property.FindPropertyRelative("m12").floatValue, property.FindPropertyRelative("m13").floatValue, property.FindPropertyRelative("m14").floatValue);
					GUILayout.BeginHorizontal();
					vector4.x = EditorGUILayout.FloatField(vector4.x);
					vector4.y = EditorGUILayout.FloatField(vector4.y);
					vector4.z = EditorGUILayout.FloatField(vector4.z);
					vector4.w = EditorGUILayout.FloatField(vector4.w);
					GUILayout.EndHorizontal();

					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = vector4.x;
						property.FindPropertyRelative("m12").floatValue = vector4.y;
						property.FindPropertyRelative("m13").floatValue = vector4.z;
						property.FindPropertyRelative("m14").floatValue = vector4.w;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Double3:
					EditorGUI.BeginChangeCheck();
					var vector3 = new Vector3(property.FindPropertyRelative("m11").floatValue, property.FindPropertyRelative("m12").floatValue, property.FindPropertyRelative("m13").floatValue);
					vector3 = EditorGUILayout.Vector3Field("Vector", vector3);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = vector3.x;
						property.FindPropertyRelative("m12").floatValue = vector3.y;
						property.FindPropertyRelative("m13").floatValue = vector3.z;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Double2:
					EditorGUI.BeginChangeCheck();
					var vector2 = new Vector2(property.FindPropertyRelative("m11").floatValue, property.FindPropertyRelative("m12").floatValue);
					vector2 = EditorGUILayout.Vector2Field("Vector", vector2);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = vector2.x;
						property.FindPropertyRelative("m12").floatValue = vector2.y;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Matrix:
					var m11 = property.FindPropertyRelative("m11").floatValue;
					var m12 = property.FindPropertyRelative("m12").floatValue;
					var m13 = property.FindPropertyRelative("m13").floatValue;
					var m14 = property.FindPropertyRelative("m14").floatValue;
					var m21 = property.FindPropertyRelative("m21").floatValue;
					var m22 = property.FindPropertyRelative("m22").floatValue;
					var m23 = property.FindPropertyRelative("m23").floatValue;
					var m24 = property.FindPropertyRelative("m24").floatValue;
					var m31 = property.FindPropertyRelative("m31").floatValue;
					var m32 = property.FindPropertyRelative("m32").floatValue;
					var m33 = property.FindPropertyRelative("m33").floatValue;
					var m34 = property.FindPropertyRelative("m34").floatValue;
					var m41 = property.FindPropertyRelative("m41").floatValue;
					var m42 = property.FindPropertyRelative("m42").floatValue;
					var m43 = property.FindPropertyRelative("m43").floatValue;
					var m44 = property.FindPropertyRelative("m44").floatValue;
					EditorGUI.BeginChangeCheck();
					GUILayout.BeginHorizontal();
					m11 = EditorGUILayout.FloatField(m11);
					m12 = EditorGUILayout.FloatField(m12);
					m13 = EditorGUILayout.FloatField(m13);
					m14 = EditorGUILayout.FloatField(m14);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					m21 = EditorGUILayout.FloatField(m21);
					m22 = EditorGUILayout.FloatField(m22);
					m23 = EditorGUILayout.FloatField(m23);
					m24 = EditorGUILayout.FloatField(m24);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					m31 = EditorGUILayout.FloatField(m31);
					m32 = EditorGUILayout.FloatField(m32);
					m33 = EditorGUILayout.FloatField(m33);
					m34 = EditorGUILayout.FloatField(m34);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
					m41 = EditorGUILayout.FloatField(m41);
					m42 = EditorGUILayout.FloatField(m42);
					m43 = EditorGUILayout.FloatField(m43);
					m44 = EditorGUILayout.FloatField(m44);
					GUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = m11;
						property.FindPropertyRelative("m12").floatValue = m12;
						property.FindPropertyRelative("m13").floatValue = m13;
						property.FindPropertyRelative("m14").floatValue = m14;
						property.FindPropertyRelative("m21").floatValue = m21;
						property.FindPropertyRelative("m22").floatValue = m22;
						property.FindPropertyRelative("m23").floatValue = m23;
						property.FindPropertyRelative("m24").floatValue = m24;
						property.FindPropertyRelative("m31").floatValue = m31;
						property.FindPropertyRelative("m32").floatValue = m32;
						property.FindPropertyRelative("m33").floatValue = m33;
						property.FindPropertyRelative("m34").floatValue = m34;
						property.FindPropertyRelative("m41").floatValue = m41;
						property.FindPropertyRelative("m42").floatValue = m42;
						property.FindPropertyRelative("m43").floatValue = m43;
						property.FindPropertyRelative("m44").floatValue = m44;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Double:
					EditorGUI.BeginChangeCheck();
					var floatValue = EditorGUILayout.FloatField("Value", property.FindPropertyRelative("m11").floatValue);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("m11").floatValue = floatValue;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Bool:
					EditorGUI.BeginChangeCheck();
					var boolValue = EditorGUILayout.Toggle("Boolean", property.FindPropertyRelative("boolValue").boolValue);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("boolValue").boolValue = boolValue;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.Long:
					EditorGUI.BeginChangeCheck();
					var longValue = EditorGUILayout.LongField("Value", property.FindPropertyRelative("longValue").longValue);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("longValue").longValue = longValue;
					}
					break;
				case FbxCustomProperty.FbxCustomPropertyType.String:
					EditorGUI.BeginChangeCheck();
					var stringValue = EditorGUILayout.TextField("Value", property.FindPropertyRelative("stringValue").stringValue);
					if (EditorGUI.EndChangeCheck())
					{
						property.FindPropertyRelative("stringValue").stringValue = stringValue;
					}
					break;
				default:
					break;
			}
			EditorGUI.indentLevel--;
		}
	}
}
#endif