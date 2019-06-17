using UnityEngine;
using System.Collections;

namespace DartWheelGame
{
	/// <summary>
	/// This script removes the object after some time
	/// </summary>
	public class DWGRemoveAfterTime:MonoBehaviour 
	{
		// How many seconds to wait before removing this object
		public float removeAfterTime = 5;

		// An effect created when this object is removed
		public Transform deathEffect;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start() 
		{
			// Remove this object after a while
			Invoke("Remove", removeAfterTime);
		}

		/// <summary>
		/// Removes this object, and create a death effect
		/// </summary>
		void Remove()
		{
			// Create a death effect
			if ( deathEffect )    Instantiate( deathEffect, transform.position, Quaternion.identity );

			// Remove this object
			Destroy(gameObject);
		}
	}
}
