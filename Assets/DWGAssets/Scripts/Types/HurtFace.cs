using UnityEngine;
using System;

namespace DartWheelGame.Types
{
	// This defines the face of the victim along with a sound

	[Serializable]
	public class HurtFace
	{
		// The sprite to use as teh face graphic
		public Sprite faceSprite;
			
		// The sound that goes along with that face
		public AudioClip hurtSound;
	}
}