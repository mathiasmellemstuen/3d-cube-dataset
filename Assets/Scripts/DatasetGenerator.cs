using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
using Moments.Encoder; 

public class DatasetGenerator : MonoBehaviour
{
	
	[Header("Demo")]
	public bool demo = false;
	public float rotationSpeed = 0.2f; 

	[Header("Environment")]
	public EnvironmentObject northWall;
	public EnvironmentObject southWall;
	public EnvironmentObject eastWall;
	public EnvironmentObject westWall;
	public EnvironmentObject floor; 
	public EnvironmentObject cube;
	
	[Range(0, 63)]
	public ushort fromColor = 0;

	[Range(0, 63)]
	public ushort toColor = 63; 
	
	[Header("Camera settings")]
	public float keepDistance;
	public float keepHeight; 
	public float fromAngleRadians = 0.0f; 
	public float toAngleRadians = 3.14f;
	public float step = 0.1f; 

	public int imageWidth = 256;
	public int imageHeight = 256;

	private RenderTexture renderTexture; 

	private Camera currentCamera; 
	private float currentCameraAngleRadians = 0.0f; 
	
	private GifEncoder gifEncoder;

	private void Start() {
		currentCamera = GetComponent<Camera>();
	}
	
	private Vector3 calculateCameraPosition(Vector3 cubePosition, float inputAngle) {
		Vector3 newPosition = new Vector3((float)Math.Cos(inputAngle) * keepDistance, keepHeight, (float)Math.Sin(inputAngle) * keepDistance); 
		return cubePosition + newPosition; 
	}

	public void generateDataset() {
	

	currentCamera = GetComponent<Camera>();

	// Setting the camera width and height in pixels
	currentCamera.pixelRect = new Rect(0, 0, imageWidth, imageHeight); 

	// Setting the target texture of the camera
	RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 0); 
	currentCamera.targetTexture = renderTexture; 

	Texture2D targetTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false); 

	for(ushort cubeColor = fromColor; cubeColor < toColor; cubeColor++) 
		for(ushort floorColor = fromColor; floorColor < toColor; floorColor++) 
			for(ushort wallColor = fromColor; wallColor < toColor; wallColor++) {
				cube.setColor(cubeColor); 
				floor.setColor(floorColor); 

				// The same value is set for all the walls
				northWall.setColor(wallColor); 
				southWall.setColor(wallColor); 
				westWall.setColor(wallColor); 
				eastWall.setColor(wallColor); 
				
				string filePath = $"dataset/{cubeColor}_{floorColor}_{wallColor}.gif"; 

				// Creating a new GIF
				gifEncoder = new GifEncoder();
				gifEncoder.Start(filePath); 
				
				currentCameraAngleRadians = 0.0f;

				// Then we loop over every position to place the camera in
				while(currentCameraAngleRadians < toAngleRadians) {
					
					// Creating and allocating memory for the frame
					GifFrame frame = new GifFrame(); 
					frame.Width = imageWidth; 
					frame.Height = imageHeight; 
					frame.Data = new Color32[imageWidth * imageHeight]; 
					
					// Updating the position and orientation of the camera
					transform.position = calculateCameraPosition(cube.transform.position, currentCameraAngleRadians); 
					transform.LookAt(new Vector3(cube.transform.position.x, keepHeight, cube.transform.position.z)); 
					
					// Rendering the image with the camera
					currentCamera.Render();
					
					// Copying the rendered image from the GPU
					RenderTexture.active = renderTexture;
					targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 
					
					// Copying rendered image to the allocated frame
					frame.Data = targetTexture.GetPixels32(); 

					// Adding the frame to the gif
					gifEncoder.AddFrame(frame); 

					currentCameraAngleRadians += step; 
				}

				// Finializing the GIF
				gifEncoder.Finish();
			}

	Debug.Log("Finished creating GIFS"); 
	}

	private void Update() {
		
		if(!demo) {
			return; 
		}

		currentCameraAngleRadians += rotationSpeed * Time.deltaTime;

		transform.position = calculateCameraPosition(cube.transform.position, currentCameraAngleRadians); 
		transform.LookAt(new Vector3(cube.transform.position.x, keepHeight, cube.transform.position.z)); 
	}
}
