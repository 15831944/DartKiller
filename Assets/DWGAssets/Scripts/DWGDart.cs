using UnityEngine;
using System.Collections;
using DartWheelGame.Types;

namespace DartWheelGame
{
	/// <summary>
	/// This script controls a dart, calculating when it hits or misses a target and creating the proper effects
	/// </summary>
	public class DWGDart : MonoBehaviour 
	{
		// Used to hold the victim object, tagged "Victim". This is the body of the victim that darts can hit and hurt
		internal DWGVictim victimObject;

		// Used to hold the wheel object, tagged "Wheel". This is the wheel that darts stick to if they don't hit a target
		static GameObject wheelObject;

		// Did we hit the victim?
		internal bool hitVictim = false;

		// Did we hit a target?
		internal bool hitTarget = false;

		// The dart that shows when hitting a target or the wheel behind it
		public Transform hitDart;

		// The dart that shows when missing the target or hitting the victim
		public Transform missDart;

		// The effect when missing the target or hitting the victim
		public Transform missEffect;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start() 
		{
			// Hold the victim object, tagged "Victim". This is the body of the victim that darts can hit and hurt
			if ( victimObject == null )    victimObject = GameObject.FindObjectOfType<DWGVictim>();

            // Hold the wheel object, tagged "Wheel". This is the wheel that darts stick to if they don't hit a target
            if (wheelObject == null) wheelObject = DWGGameController.FindObjectOfType<DWGGameController>().wheel.Find("WheelFront").gameObject;
		}


		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void Update() 
		{
			// If the animation of the dart ended, then we can check if we hit anything.
			if ( GetComponent<Animation>().isPlaying == false )
			{
				// Get a list of all targets
				//GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");

                DWGTarget[] allTargets = DWGTarget.FindObjectsOfType<DWGTarget>();


                // Go through each target and check if we hit it
                foreach (DWGTarget target in allTargets )
				{
					target.SendMessage("CheckHit", transform);
				}

				// If we didn't hit any targets, check if we hit the wheel behind them or the victim on the wheel
				if ( hitTarget == false )
				{
					// Check if we hit the victim
					foreach ( HitArea hitArea in victimObject.hitAreas )
					{
						if (hitArea.hitObject.OverlapPoint(transform.position))
						{
							// Drop the dart
							DropDart();
							
							// Run the hit function on the victim, making him get hurt
							victimObject.SendMessage("HitVictim", transform);
							
							// Switch to one of the hurt faces
							if ( victimObject.victimHead && hitArea.hurtFaces.Length > 0)
							{
								// Choose a random face
								int randomFace = Mathf.FloorToInt(Random.Range(0, hitArea.hurtFaces.Length));
								
								// Change the face of the victim
								if (hitArea.hurtFaces[randomFace].faceSprite) victimObject.victimHead.GetComponent<SpriteRenderer>().sprite = hitArea.hurtFaces[randomFace].faceSprite;
								
								//If there is a source and a sound, play it from the source
								if (victimObject.soundSourceTag != string.Empty && hitArea.hurtFaces[randomFace].hurtSound) GameObject.FindGameObjectWithTag(victimObject.soundSourceTag).GetComponent<AudioSource>().PlayOneShot(hitArea.hurtFaces[randomFace].hurtSound);
							}
							
							// Reduce the number of lives the player has
							GameObject.FindGameObjectWithTag("GameController").SendMessage("ChangeLives", -1);
							
							// We hit the victim
							hitVictim = true;
							
							break;
						}
					}
					
					if ( hitVictim == false ) // If we didn't hit the victim, it means we hit the wheel behind him, or missed the wheel entirely
					{
						// If we hit the wheel, stick the dart to it
						if ( wheelObject.GetComponent<Collider2D>().OverlapPoint(transform.position) )
						{
							HitTarget(wheelObject.transform);
						}
						else  // Otherwise, we missed the wheel entirely
						{
							MissTarget();
							
							// Lose a life because we missed completely
							GameObject.FindGameObjectWithTag("GameController").SendMessage("ChangeLives", -1);
						}
					}
				}
				
				// Remove the dart
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// Hits the target, and sticks the dart to it
		/// </summary>
		/// <param name="hitSource">Hit source, a target</param>
		void HitTarget( Transform hitSource )
		{
			hitTarget = true;

			// Create a new dart
			Transform newDart = Instantiate( hitDart, transform.position, transform.rotation ) as Transform;

			// Attach it to the target
			newDart.parent = hitSource;
		}

		/// <summary>
		/// Misses the target, and creates the MISS effect
		/// </summary>
		void MissTarget()
		{
			if ( missEffect )
			{
				// Create new miss effect
				Transform newEffect = Instantiate( missEffect, transform.position, Quaternion.identity ) as Transform;
				
				// Rotate the effect slightly
				newEffect.eulerAngles = Vector3.forward * Random.Range(-10,10);
			}

			// Drop the dart
			DropDart();
		}

		/// <summary>
		/// Drops the dart.
		/// </summary>
		void DropDart()
		{
			// Create a miss dart, which falls off
			if ( missDart )
			{
				Transform newDart = Instantiate( missDart, transform.position, Quaternion.identity ) as Transform;
				
				newDart.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(-300,300);
			}
		}
	}
}
