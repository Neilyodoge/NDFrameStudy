#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace UnLogickFactory
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(FbxCustomPropertyBehaviour), true)]
	public class FbxCustomPropertyBehaviourInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			var custom = serializedObject.FindProperty("customProperties");
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Custom Properties");
			custom.arraySize = EditorGUILayout.IntField(custom.arraySize);
			if (GUILayout.Button("+"))
			{
				int index = custom.arraySize;
				custom.InsertArrayElementAtIndex(index);
				var nameProperty = custom.GetArrayElementAtIndex(index).FindPropertyRelative("propertyName");

				var propertyName = nameProperty.stringValue;
				string coreName;
				int iteratorNumber;
				AnalyzeName(ref propertyName, out coreName, out iteratorNumber);

				while (IsNameDuplicate(custom, propertyName))
				{
					iteratorNumber++;
					propertyName = coreName + iteratorNumber;
				}

				nameProperty.stringValue = propertyName;
			}
			if (GUILayout.Button("-"))
			{
				custom.DeleteArrayElementAtIndex(custom.arraySize-1);
			}
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel++;

			for (int i = 0; i < custom.arraySize; i++)
			{
				var customProperty = custom.GetArrayElementAtIndex(i);
				EditorGUILayout.PropertyField(customProperty);
			}
			EditorGUI.indentLevel--;
			serializedObject.ApplyModifiedProperties();
		}

		private bool IsNameDuplicate(SerializedProperty custom, string propertyName)
		{
			for (int i = 0; i < custom.arraySize; i++)
			{
				if (custom.GetArrayElementAtIndex(i).FindPropertyRelative("propertyName").stringValue == propertyName)
				{
					return true;
				}
			}
			return false;
		}

		readonly char[] numbers = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		private void AnalyzeName(ref string propertyName, out string coreName, out int iteratorNumber)
		{
			if (string.IsNullOrEmpty(propertyName))
			{
				propertyName = "Custom Property";
				coreName = "Custom Property ";
				iteratorNumber = 0;
			}
			else
			{
				for(int i=propertyName.Length-1;  i>=0; i--)
				{
					bool isNumber = false;
					for (int j = 0; j < numbers.Length; j++)
					{
						if (propertyName[i] == numbers[j])
						{
							isNumber = true;
							break;
						}
					}
					if (!isNumber)
					{
						coreName = propertyName.Substring(0, i + 1);
						if (i == propertyName.Length - 1)
						{
							coreName = coreName + " ";
							iteratorNumber = 0;
						}
						else
						{
							iteratorNumber = int.Parse(propertyName.Substring(i + 1));
						}
						return;
					}
				}
				coreName = "";
				iteratorNumber = int.Parse(propertyName);
			}
		}
	}
}
#endif