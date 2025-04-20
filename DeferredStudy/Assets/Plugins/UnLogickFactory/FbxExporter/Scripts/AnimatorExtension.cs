using UnityEngine;
using System;
using System.Reflection;

namespace UnLogickFactory
{
	public static class AnimatorExtension
	{
		static MethodInfo _writeDefaultPoseMethodInfo;
		public static void SetTPose(this Animator animator)
		{
			if (_writeDefaultPoseMethodInfo == null)
			{
				_writeDefaultPoseMethodInfo = typeof(Animator).GetMethod("WriteDefaultPose", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				if (_writeDefaultPoseMethodInfo == null)
				{
					Debug.LogErrorFormat("FbxExporter - Failed to set TPose, it seems unity {0} doesn't have the Animator.WriteDefaultPose method. You need to update the reflection code to match the new api.", Application.unityVersion);
				}
			}
			_writeDefaultPoseMethodInfo.Invoke(animator, null);
		}
	}
}



