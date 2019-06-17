using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DartWheelGame.Types;

namespace DartWheelGame
{
	/// <summary>
	/// This script defines a victim which can be hit by a dart, reducing a life and changing the expression on the victim's face
	/// </summary>
	public class DWGVictim:MonoBehaviour 
	{	
		// The head object that can display different faces
		public Transform victimHead;

		// The normal face, not hurt
		public Sprite normalFace;
		
		// Various hit areas, each with its own hurt faces and the sound associated with them
		public HitArea[] hitAreas;
		
		// How many seconds to wait before returning to the normal face
		public float hurtTime = 1;
		internal float hurtTimeCount;
		
		// The effect when hitting the victim
		public Transform ouchEffect;
		
		// The source from which sound for this object play
		public string soundSourceTag = "GameController";
		
		// The audiosource from which sounds play
		internal GameObject soundSource;
		
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);
		}
		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update()
		{
			// If we have a normal face assigned
			if ( normalFace )
			{
				// Count down the hurt time, and then return to the normal face
				if ( hurtTimeCount > 0 )    hurtTimeCount -= Time.deltaTime;
				else if ( victimHead.GetComponent<SpriteRenderer>().sprite != normalFace )
				{
					victimHead.GetComponent<SpriteRenderer>().sprite = normalFace;
				}
			}
		}
		
		/// <summary>
		/// Hits the victim, changing the facial expression and playing a hurt sound
		/// </summary>
		/// <param name="hitSource">Hit source, the dart</param>
		void HitVictim( Transform hitSource )
		{
			// Reset the hurt counter
			hurtTimeCount = hurtTime;
			
			// Create an "OUCH!" effect
			if ( ouchEffect )
			{
				Transform newEffect = Instantiate( ouchEffect, hitSource.position, Quaternion.identity ) as Transform;
				
				// Rotate the effect slightly
				newEffect.eulerAngles = Vector3.forward * Random.Range(-10,10);
			}
		}
	}
}
