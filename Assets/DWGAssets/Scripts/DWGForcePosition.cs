using UnityEngine;
using System.Collections;

namespace DartWheelGame
{
	/// <summary>
	/// This script forces an object position based on the object parent
	/// This is used to make sure the wheel and target backs are always at the bottom, given an illusion of thickness
	/// </summary>
	public class DWGForcePosition:MonoBehaviour 
	{
		internal Transform thisTransform;

		// The correct position relative to the parent object
		public Vector3 relativePosition = new Vector3(0, -0.05f, 0);

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start() 
		{
			thisTransform = transform;
		}
		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update() 
		{
			// Keep the position relative to the parent
			thisTransform.position = thisTransform.parent.position + relativePosition;
		}
	}
}
