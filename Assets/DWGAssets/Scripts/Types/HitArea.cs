using UnityEngine;
using System;
using DartWheelGame.Types;

namespace DartWheelGame.Types
{
	// This defines the face of the victim along with a sound
	
	[Serializable]
	public class HitArea
	{
		// The object that detects the hit. This should contain a 2D Collider
		public Collider2D hitObject;
		
		// Various hurt faces and the sound associated with each
		public HurtFace[] hurtFaces;
	}
}