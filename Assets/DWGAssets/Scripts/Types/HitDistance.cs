using UnityEngine;
using System;

namespace DartWheelGame.Types
{
	//This class defines the hit distance from the center of a target, and the bonus it gives

	[Serializable]
	public class HitDistance 
	{
		// How far from the center of the target we hit
		public float hitDistance = 0.5f;

		// How many points we get when we hit this close to the center
		public int hitBonus = 100;

		// The text displayed alongside the bonus
		public string bonusText = "GREAT!";

		// The sound played when hitting this close to center
		public AudioClip bonusSound;
	}
}
