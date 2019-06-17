#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DartWheelGame.Types;

namespace DartWheelGame
{
	/// <summary>
	/// This script controls the game, starting it, following game progress, and finishing it with game over.
	/// </summary>
	public class DWGGameController:MonoBehaviour 
	{
		// The wheel object that spins
		public Transform wheel;
		internal Transform victimHead;

		// How fast the wheel spins
		public float wheelSpeed = 10;
		internal float wheelCurrentSpeed = 0;

		// The spin direction of the wheel
		public int wheelDirection = 1;

		//public float switchChance = 0.001f;

		// How much the speed of the wheel increases when leveling up
		public float wheelSpeedIncrease = 5;

		//public float switchChanceIncrease = 0.001f;

		// The point at which you aim before throwing a dart. Used for mobile and gamepad/keyboard controls
		public Transform crosshair;

		// How fast the crosshair moves
		public float crosshairSpeed = 5;

		// How far off the tap position the crosshair should be placed
		public Vector2 crosshairOffset = new Vector2( 0, 0.5f);

		// A list of darts that are randomly chosen to be thrown
		public Transform[] darts;

		// How many seconds before we can throw another dart
		public float dartCooldown = 1;
		internal float cooldownCount = 0;

		// The dart icon which shows the cooldown between dart shots
		public Transform dartIcon;

		// How many targets we have to hit in order to win a level
		public int targetsToWin = 8;
		
		// The number of targets currently on screen
		internal int targetCount = 0;

		// The number of lives the player has. When the player dies, it loses one life. When lives reach 0, it's game over.
		public int lives = 3;

		// The text that displays the number of lives we have left
		public Text livesText;

		// The score and score text of the player
		public int score = 0;
		public Transform scoreText;
		internal int highScore = 0;
		internal int scoreMultiplier = 1;

		// The shoot button, hold it or tap it to shoot
		public string shootButton = "Fire1";

		// The button used for mobile controls
		public Transform mobileControls;

		// Are we using the mouse now?
		internal bool usingMouse = false;

		// The current level we are on
		internal int currentLevel = 1;

		// The text that shows the current level/round/stage/etc
		public Text LevelText;

		public string LevelNamePrefix = "LEVEL";

		// The overall game speed
		internal float gameSpeed = 1;

		// Various canvases for the UI
		public Transform gameCanvas;
		public Transform pauseCanvas;
		public Transform gameOverCanvas;
		
		// Is the game over?
		internal bool  isGameOver = false;
		
		// The level of the main menu that can be loaded after the game ends
		public string mainMenuLevelName = "MainMenu";
		
		// Various sounds and their source
		public AudioClip soundLevelUp;
		public AudioClip soundGameOver;
		public string soundSourceTag = "GameController";
		internal GameObject soundSource;

		// The button that pauses the game. Clicking on the pause button in the UI also pauses the game
		public string pauseButton = "Cancel";
		internal bool  isPaused = false;

		// A general use index
		internal int index = 0;

		void Awake()
		{
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
		}

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		void Start()
		{
			// If we are not using a mobile ( iOS, Android, etc) platform, reset the crosshair offset to 0
			if ( Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.PS4 || Application.platform == RuntimePlatform.XboxOne )
			{
				crosshairOffset = Vector2.zero;

				if ( crosshair )    crosshair.gameObject.SetActive(false);

				if ( mobileControls )    mobileControls.gameObject.SetActive(false);
			}

			// Hold the victim head, so we can tilt it when rotating the wheel
			if ( GameObject.FindObjectOfType<DWGVictim>().transform.Find("Body/Head") )    victimHead = GameObject.FindObjectOfType<DWGVictim>().transform.Find("Body/Head");


			//Update the score and lives at the start of the game
			UpdateScore();
			ChangeLives(0);
			
			//Hide the game over and pause screens
			if ( gameOverCanvas )    gameOverCanvas.gameObject.SetActive(false);
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);

			//Get the highscore for the player
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			highScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HighScore", 0);
			#else
			highScore = PlayerPrefs.GetInt(Application.loadedLevelName + "HighScore", 0);
			#endif

			//Assign the sound source for easier access
			if ( GameObject.FindGameObjectWithTag(soundSourceTag) )    soundSource = GameObject.FindGameObjectWithTag(soundSourceTag);

			// Set the number of targets we need to hit to win
			targetCount = targetsToWin;
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		void  Update()
		{
			// If there is a wheel object, spin it based on direction and speed
			if ( wheel )    
			{
				// Accelerate towards the correct speed
				wheelCurrentSpeed = Mathf.Lerp(wheelCurrentSpeed, wheelSpeed * wheelDirection, Time.deltaTime * 4);

				// Rotate the wheel based on speed
				wheel.eulerAngles += Vector3.forward * wheelCurrentSpeed * Time.deltaTime;

				// Tilt the victim's head opposite the direction of the wheel. Just a nice effect.
				if ( victimHead )    victimHead.localEulerAngles = new Vector3(0,0,Mathf.LerpAngle(victimHead.localEulerAngles.z, Mathf.Clamp(wheelSpeed * -wheelDirection,-40,40), Time.deltaTime));
			}

			if ( isGameOver == false )
			{
				if ( isPaused == false )
				{
					// Count the cooldown to the next dart throw
					if ( cooldownCount < dartCooldown )    
					{
						cooldownCount += Time.deltaTime;
					
						// Display the cooldown percentage on the dart icon
						if ( dartIcon )    dartIcon.Find("Full").GetComponent<Image>().fillAmount = cooldownCount/dartCooldown;
					}

					// If we move the mouse in any direction, then mouse controls take effect
					if ( Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0 )    usingMouse = true;

					// We are using the mouse
					if ( usingMouse == true )
					{
						if ( crosshair )
						{
							// Hide the crosshair
							if ( crosshair.gameObject.activeSelf == true )    crosshair.gameObject.SetActive(false);

							// Calculate the mouse/tap position
							Vector3 aimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

							// Make sure it's 2D
							aimPosition.z = 0;

							// Place the crosshair at the position of the mouse/tap, with an added offset
							crosshair.transform.position = aimPosition + new Vector3( crosshairOffset.x, crosshairOffset.y, 0);
						}
					}

					// Keyboard and Gamepad controls
					if ( crosshair )
					{
						// If we press gamepad or keyboard arrows, then mouse controls are turned off
						if ( Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 )    
						{
							usingMouse = false;

							// Show the crosshair
							if ( crosshair.gameObject.activeSelf == false )    crosshair.gameObject.SetActive(true);
						}

						// Move the crosshair based on gamepad/keyboard directions
						crosshair.position += new Vector3( Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), crosshair.position.z) * crosshairSpeed * Time.deltaTime;

						// Limit the position of the crosshair to the edges of the screen
						// Limit to the left screen edge
						if ( crosshair.position.x < Camera.main.ScreenToWorldPoint(Vector3.zero).x )    crosshair.position = new Vector3( Camera.main.ScreenToWorldPoint(Vector3.zero).x, crosshair.position.y, crosshair.position.z);

						// Limit to the right screen edge
						if ( crosshair.position.x > Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x )    crosshair.position = new Vector3( Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x, crosshair.position.y, crosshair.position.z);

						// Limit to the left screen edge
						if ( crosshair.position.y < Camera.main.ScreenToWorldPoint(Vector3.zero).y )    crosshair.position = new Vector3( crosshair.position.x, Camera.main.ScreenToWorldPoint(Vector3.zero).y, crosshair.position.z);

						// Limit to the top screen edge
						if ( crosshair.position.y > Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y )    crosshair.position = new Vector3( crosshair.position.x, Camera.main.ScreenToWorldPoint(Vector3.up * Screen.height).y, crosshair.position.z);

						// Limit to the bottom screen edge
						if ( Input.GetButtonUp(shootButton) )    Throw();
					}
				}

				//Toggle pause/unpause in the game
				if ( Input.GetButtonDown(pauseButton) )
				{
					if ( isPaused == true )    Unpause();
					else    Pause();
				}
			}
		}

		//This function changes the score of the player
		void  ChangeScore( int changeValue )
		{
			score += changeValue;

			//Update the score
			UpdateScore();
		}
		
		//This function updates the player's score, ( and in a later update checks if we reached the required score to level up )
		void  UpdateScore()
		{
			//Update the score text
			if ( scoreText )    scoreText.GetComponent<Text>().text = score.ToString();
		}

		/// <summary>
		/// Set the score multiplier ( When the player picks up coins he gets 1X,2X,etc score )
		/// </summary>
		void SetScoreMultiplier( int setValue )
		{
			// Set the score multiplier
			scoreMultiplier = setValue;
		}
		
		/// <summary>
		/// Pause the game
		/// </summary>
		public void  Pause()
		{
			isPaused = true;
			
			//Set timescale to 0, preventing anything from moving
			Time.timeScale = 0;
			
			//Show the pause screen and hide the game screen
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(true);
			if ( gameCanvas )    gameCanvas.gameObject.SetActive(false);
		}

        /// <summary>
        /// Resume the game
        /// </summary>
        public void Unpause()
		{
			//Set timescale back to the current game speed
			Time.timeScale = gameSpeed;

			isPaused = false;
			
			//Hide the pause screen and show the game screen
			if ( pauseCanvas )    pauseCanvas.gameObject.SetActive(false);
			if ( gameCanvas )    gameCanvas.gameObject.SetActive(true);
		}
		
		/// <summary>
		/// Runs the game over event and shows the game over screen
		/// </summary>
		IEnumerator GameOver(float delay)
		{
			isGameOver = true;

			wheelSpeed = 0;

			yield return new WaitForSeconds(delay);
			
			//If there is a source and a sound, play it from the source
			if ( soundSource && soundGameOver )    soundSource.GetComponent<AudioSource>().PlayOneShot(soundGameOver);

			//Remove the pause and game screens
			if ( pauseCanvas )    Destroy(pauseCanvas.gameObject);
			if ( gameCanvas )    Destroy(gameCanvas.gameObject);
			
			//Show the game over screen
			if ( gameOverCanvas )    
			{
				//Show the game over screen
				gameOverCanvas.gameObject.SetActive(true);
				
				//Write the score text
				gameOverCanvas.Find("TextScore").GetComponent<Text>().text = "SCORE " + score.ToString();
				
				//Check if we got a high score
				if ( score > highScore )    
				{
					highScore = score;
					
					//Register the new high score
					#if UNITY_5_3 || UNITY_5_3_OR_NEWER
					PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "HighScore", score);
					#else
					PlayerPrefs.SetInt(Application.loadedLevelName + "HighScore", score);
					#endif
				}
				
				//Write the high sscore text
				gameOverCanvas.Find("TextHighScore").GetComponent<Text>().text = "HIGH SCORE " + highScore.ToString();
			}
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  Restart()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			#else
			Application.LoadLevel(Application.loadedLevelName);
			#endif
		}
		
		/// <summary>
		/// Restart the current level
		/// </summary>
		void  MainMenu()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			SceneManager.LoadScene(mainMenuLevelName);
			#else
			Application.LoadLevel(mainMenuLevelName);
			#endif
		}

		/// <summary>
		/// Aim this with the crosshair
		/// </summary>
		public void Aim()
		{
			if ( crosshair )    crosshair.gameObject.SetActive(true);
		}


		public void Throw()
		{
			// Hide the crosshair
			if ( crosshair )    crosshair.gameObject.SetActive(false);

			// Only throw if the cooldown time is over
			if ( cooldownCount >= dartCooldown && Time.deltaTime > 0 )
			{
				// Create a new dart at the position of the crosshair
				Transform newDart = Instantiate( darts[Mathf.FloorToInt(Random.Range(0, darts.Length))], crosshair.position, Quaternion.identity ) as Transform;
			
				newDart.eulerAngles = Vector3.forward * Random.Range( -40, 40);

				// Reset the cooldown counter
				cooldownCount = 0;
			}
		}

		/// <summary>
		/// Reduces the target count. If we reach 0, we go to the next level
		/// </summary>
		IEnumerator ReduceTarget()
		{
			// Reduce the target count
			if ( targetCount > 1 )    targetCount--;
			else
			{
				// Wait half a second
				yield return new WaitForSeconds(0.5f);

				// Remove all darts from the board
				ClearTargets();

				// Wait half a second
				yield return new WaitForSeconds(0.5f);

				// Go to the next level
				LevelUp();
			}
		}

		/// <summary>
		/// Clears the targets from the board, dropping and removing them
		/// </summary>
		void ClearTargets()
		{
            // Get a list of all darts
            //GameObject[] allDarts = GameObject.FindGameObjectsWithTag("Dart");
            DWGRemoveAfterTime[] allDarts = GameObject.FindObjectsOfType<DWGRemoveAfterTime>();

            // Drop all the targets off the wheel
            foreach (DWGRemoveAfterTime dart in allDarts )
			{
				dart.SendMessage("Remove");
			}
		}

		/// <summary>
		/// Levels up, resetting the targets and starting the next level
		/// </summary>
		void LevelUp()
		{
			// Increase the level
			currentLevel++;

			// Display the level text
			if ( LevelText ) 
			{
				// Display the level text
				LevelText.text = LevelNamePrefix + " " + currentLevel.ToString();

				// Animate the level text
				if ( LevelText.GetComponent<Animation>() )    LevelText.GetComponent<Animation>().Play();
			}

			// Set the target count for the next level
			targetCount = targetsToWin;

            // Get a list of all targets
            //GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");
            DWGTarget[] allTargets = DWGTarget.FindObjectsOfType<DWGTarget>();

            // Open all the targets so they can be hit again
            foreach ( DWGTarget target in allTargets )
			{
				target.SendMessage("OpenTarget", transform);
			}
			
			// Increase the wheel speed
			wheelSpeed += wheelSpeedIncrease;

			// Switch the wheel direction
			wheelDirection *= -1;

			//switchChance += switchChanceIncrease;
			
			//If there is a source and a sound, play it from the source
			if ( soundSourceTag != string.Empty && soundLevelUp )    GameObject.FindGameObjectWithTag(soundSourceTag).GetComponent<AudioSource>().PlayOneShot(soundLevelUp);
		}

		/// <summary>
		/// Changes the lives of the player. If we reach 0, it's game over
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		void ChangeLives( int changeValue )
		{
			if ( isGameOver == false )
			{
				// Change lives number
				lives += changeValue;

				// Display lives number
				livesText.text = lives.ToString();

				// If we reach 0, it's game over
				if ( lives <= 0 ) 
				{
					StartCoroutine(GameOver(1));
				}
			}
		}
	}
}