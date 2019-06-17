using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DartWheelGame.Types;

namespace DartWheelGame
{
	/// <summary>
	/// This script defines a target that can be hit by a dart. The target also gives bonus based on how close to the center we hit it.
	/// </summary>
	public class DWGTarget:MonoBehaviour 
	{	
		internal GameObject GameController;

		// Has this target been hit. If so, it can't be hit again
		internal bool isHit = false;

		// The effect that displays the bonus we got
		public Transform bonusEffect;

		// A list of hit distances that decide the bonus we get when hitting a target with a dart
		public HitDistance[] hitDistance;

		// Animations for opening and closing the target
		public string animationOpen = "TargetOpen";
		public string animationClose = "TargetClose";

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
			//Assign the game controller for easier access
			if ( GameController == null )    GameController = GameObject.FindGameObjectWithTag("GameController");

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Open the target so it can be hit with a dart
			OpenTarget();
		}

		/// <summary>
		/// Opens the target, allowing it to be hit by a dart
		/// </summary>
		void OpenTarget()
		{
			// Play the open animation
			if ( GetComponent<Animation>() && animationOpen != string.Empty )    GetComponent<Animation>().Play(animationOpen);

			// The target is not hit, so it can be hit again
			isHit = false;
		}

		/// <summary>
		/// Checks how close to the center this target was hit, and gives bonus accordingly
		/// </summary>
		/// <param name="hitSource">Hit source, which is the dart that hit this target</param>
		void CheckHit( Transform hitSource )
		{
			// Only check this target if it hasn't been hit yet
			if ( isHit == false )
			{
				// Go through all the possible distances and see how close to the center we hit
				for ( int index = 0 ; index < hitDistance.Length ; index++ )
				{
					// If the hit is closer that the required distance, give a bonus
					if ( Vector3.Distance( hitSource.position, transform.position) < hitDistance[index].hitDistance )
					{
						// Run the hit target function in the dart. It creates a hit effect
						hitSource.SendMessage("HitTarget", transform);

						// If we have a bonus effect
						if ( bonusEffect )
						{
							// Create a new bonus effect at the hitSource position
							Transform newBonusEffect = Instantiate(bonusEffect, transform.position, Quaternion.identity) as Transform;
							
							// Display the bonus value
							newBonusEffect.Find("Text").GetComponent<Text>().text = hitDistance[index].bonusText + "\n" + hitDistance[index].hitBonus.ToString();
							
							// Rotate the bonus text slightly
							newBonusEffect.eulerAngles = Vector3.forward * Random.Range(-10,10);
						}

						GameController.SendMessage("ChangeScore", hitDistance[index].hitBonus);

						if ( GetComponent<Animation>() && animationClose != string.Empty )    GetComponent<Animation>().Play(animationClose);

						isHit = true;

						// Reduce the count of targets on the wheel
						GameController.SendMessage("ReduceTarget", gameObject.tag);		

						// If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
						if ( soundSourceTag != string.Empty && hitDistance[index].bonusSound )    soundSource.GetComponent<AudioSource>().PlayOneShot(hitDistance[index].bonusSound);

						// End the function because we gave a bonus and hit the target
						return;
					}
				}
			}
		}

		/// <summary>
		/// Raises the draw gizmos event.
		/// </summary>
		void OnDrawGizmos()
		{
			// Draw circles indicating the various distances we can hit a target at
			for ( int index = 0 ; index < hitDistance.Length ; index++ )
			{
				// Color the circle from black red, from closest to farthest
				Gizmos.color = new Color( 1.0f * (index + 1)/hitDistance.Length,0,0,1);

				Gizmos.DrawWireSphere( transform.position, hitDistance[index].hitDistance);
			}
		}
	}
}
