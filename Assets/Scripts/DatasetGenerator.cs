using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 
using System.IO; 
using Moments.Encoder; 

public enum FileType {
	GIF,
	PNG
}

public class DatasetGenerator : MonoBehaviour
{
	
	[Header("Demo")]
	public bool demo = false;
	public float rotationSpeed = 0.2f; 
	
	[Header("Generation settings")]
	public FileType fileType; 

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

	[Header("Random colors")]
	public bool useRandomColors = false;
	public int seed = 321789; 
	public int amountOfRandomColors = 100; 
	
	[Header("Masks")]
	public bool onlyGenerateMasks = false; 

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
	private Camera floorCamera; 
	private Camera wallCamera;
	private Camera cubeCamera; 
	private float currentCameraAngleRadians = 0.0f; 
	
	private GifEncoder gifEncoder;

	private void Start() {
		currentCamera = GetComponent<Camera>();
		floorCamera = GameObject.Find("FloorCamera").GetComponent<Camera>();
		wallCamera = GameObject.Find("WallCamera").GetComponent<Camera>();
		cubeCamera = GameObject.Find("CubeCamera").GetComponent<Camera>();
	}
	
	private Vector3 calculateCameraPosition(Vector3 cubePosition, float inputAngle) {
		Vector3 newPosition = new Vector3((float)Math.Cos(inputAngle) * keepDistance, keepHeight, (float)Math.Sin(inputAngle) * keepDistance); 
		return cubePosition + newPosition; 
	}
		
	public void generateAngleFile() {
		

		string content = ""; 
		
		currentCameraAngleRadians = fromAngleRadians; 
		
		do {
			string lineEnd = currentCameraAngleRadians + step > toAngleRadians ? "" : "\n"; 

			content += $"{currentCameraAngleRadians}{lineEnd}"; 
			currentCameraAngleRadians += step;
		}
		while(currentCameraAngleRadians < toAngleRadians);

		File.WriteAllText("dataset/angles.txt", content); 
	}

	public void generateAllMasksPNG(uint cubeColor, uint floorColor, uint wallColor, HexHelpers.BitType bitType, RenderTexture renderTexture, Texture2D targetTexture) {

		cube.setColor(cubeColor, bitType); 
		floor.setColor(floorColor, bitType); 

		// The same value is set for all the walls
		northWall.setColor(wallColor, bitType); 
		southWall.setColor(wallColor, bitType); 
		westWall.setColor(wallColor, bitType); 
		eastWall.setColor(wallColor, bitType); 

		string folderPath = $"dataset/{cubeColor}_{floorColor}_{wallColor}/";
		System.IO.Directory.CreateDirectory(folderPath); 

		currentCameraAngleRadians = 0.0f;
		int index = 0; 
		// Then we loop over every position to place the camera in
		while(currentCameraAngleRadians < toAngleRadians) {
			
			// Updating the position and orientation of the camera
			transform.position = calculateCameraPosition(cube.transform.position, currentCameraAngleRadians); 
			transform.LookAt(new Vector3(cube.transform.position.x, keepHeight, cube.transform.position.z));
			floorCamera.transform.position = transform.position; 
			floorCamera.transform.rotation = transform.rotation;
			cubeCamera.transform.position = transform.position; 
			cubeCamera.transform.rotation = transform.rotation;
			wallCamera.transform.position = transform.position; 
			wallCamera.transform.rotation = transform.rotation;
			
			// Rendering the image with the camera
			currentCamera.targetTexture = renderTexture; 
			currentCamera.Render();

			// Copying the rendered image from the GPU
			RenderTexture.active = renderTexture;
			targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 

			// Saving the current frame to disk
			byte[] imageBytesCurrent = targetTexture.EncodeToPNG();
			File.WriteAllBytes($"{folderPath}/{index}.png", imageBytesCurrent);

			floorCamera.targetTexture = renderTexture; 
			floorCamera.Render();
			
			// Copying the rendered image from the GPU
			RenderTexture.active = renderTexture;
			targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 
			targetTexture = makeTextureBinary(targetTexture); 

			// Saving the current frame to disk
			byte[] imageBytesFloor = targetTexture.EncodeToPNG();
			File.WriteAllBytes($"{folderPath}/{index}_floor_mask.png", imageBytesFloor);

			cubeCamera.targetTexture = renderTexture; 
			cubeCamera.Render();
			
			// Copying the rendered image from the GPU
			RenderTexture.active = renderTexture;
			targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 
			targetTexture = makeTextureBinary(targetTexture); 


			// Saving the current frame to disk
			byte[] imageBytesCube = targetTexture.EncodeToPNG();
			File.WriteAllBytes($"{folderPath}/{index}_cube_mask.png", imageBytesCube);

			wallCamera.targetTexture = renderTexture; 
			wallCamera.Render();
			
				// Copying the rendered image from the GPU
			RenderTexture.active = renderTexture;
			targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 
			targetTexture = makeTextureBinary(targetTexture); 

			// Saving the current frame to disk
			byte[] imageBytesWall = targetTexture.EncodeToPNG();
			File.WriteAllBytes($"{folderPath}/{index}_wall_mask.png", imageBytesWall);

			currentCameraAngleRadians += step;
			index += 1; 
		}
	}
	Texture2D makeTextureBinary(Texture2D inTexture) {
		for(int y = 0; y < inTexture.height; y++) {
			for(int x = 0; x < inTexture.width; x++) {
				
				Color color = (inTexture.GetPixel(x, y) != Color.black ? Color.white : Color.black); 

				inTexture.SetPixel(x, y, color); 
			}
		}

		return inTexture; 
	}
	public void generateAllPNG(uint cubeColor, uint floorColor, uint wallColor, HexHelpers.BitType bitType, RenderTexture renderTexture, Texture2D targetTexture) {
		
		cube.setColor(cubeColor, bitType); 
		floor.setColor(floorColor, bitType); 

		// The same value is set for all the walls
		northWall.setColor(wallColor, bitType); 
		southWall.setColor(wallColor, bitType); 
		westWall.setColor(wallColor, bitType); 
		eastWall.setColor(wallColor, bitType); 

		string folderPath = $"dataset/{cubeColor}_{floorColor}_{wallColor}/";
		System.IO.Directory.CreateDirectory(folderPath); 

		currentCameraAngleRadians = 0.0f;
		int index = 0; 
		// Then we loop over every position to place the camera in
		while(currentCameraAngleRadians < toAngleRadians) {
			
			// Updating the position and orientation of the camera
			transform.position = calculateCameraPosition(cube.transform.position, currentCameraAngleRadians); 
			transform.LookAt(new Vector3(cube.transform.position.x, keepHeight, cube.transform.position.z));
			
			// Rendering the image with the camera
			currentCamera.Render();

			// Copying the rendered image from the GPU
			RenderTexture.active = renderTexture;
			targetTexture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0); 

			// Saving the current frame to disk
			byte[] imageBytes = targetTexture.EncodeToPNG();
			File.WriteAllBytes($"{folderPath}/{index}.png", imageBytes); 

			currentCameraAngleRadians += step; 
			index += 1; 
		}
	}
	public void generateAllGIF(uint cubeColor, uint floorColor, uint wallColor, HexHelpers.BitType bitType, RenderTexture renderTexture, Texture2D targetTexture) { 

		cube.setColor(cubeColor, bitType); 
		floor.setColor(floorColor, bitType); 

		// The same value is set for all the walls
		northWall.setColor(wallColor, bitType); 
		southWall.setColor(wallColor, bitType); 
		westWall.setColor(wallColor, bitType); 
		eastWall.setColor(wallColor, bitType); 
		
		string filePath = $"dataset/{cubeColor}_{floorColor}_{wallColor}.gif"; 

		// Creating a new GIF
		gifEncoder = new GifEncoder();
		gifEncoder.SetFrameRate(60);
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
	
	public void generateDataset() {
	
	// Setting the seed
	UnityEngine.Random.InitState(seed); 
	
	Texture2D targetTexture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false); 
	RenderTexture renderTexture = new RenderTexture(imageWidth, imageHeight, 0); 

	currentCamera = GetComponent<Camera>();
	floorCamera = GameObject.Find("FloorCamera").GetComponent<Camera>();
	wallCamera = GameObject.Find("WallCamera").GetComponent<Camera>();
	cubeCamera = GameObject.Find("CubeCamera").GetComponent<Camera>();

	// Setting the camera width and height in pixels
	currentCamera.pixelRect = new Rect(0, 0, imageWidth, imageHeight); 

	currentCamera.targetTexture = renderTexture; 
	
	if(onlyGenerateMasks) {
		
		uint randomCubeColor = (uint)UnityEngine.Random.Range(0, 4294967295); 
		uint randomFloorColor = (uint)UnityEngine.Random.Range(0, 4294967295); 
		uint randomWallColor = (uint)UnityEngine.Random.Range(0, 4294967295); 

		generateAllMasksPNG(randomCubeColor, randomFloorColor, randomWallColor, HexHelpers.BitType.BIT_32, renderTexture, targetTexture); 
		return; 
	}
	if(useRandomColors) {
		for(int i = 0; i < amountOfRandomColors; i++) {
			uint randomCubeColor = (uint)UnityEngine.Random.Range(0, 4294967295); 
			uint randomFloorColor = (uint)UnityEngine.Random.Range(0, 4294967295); 
			uint randomWallColor = (uint)UnityEngine.Random.Range(0, 4294967295); 
			
			if(fileType == FileType.GIF) {
				generateAllGIF(randomCubeColor, randomFloorColor, randomWallColor, HexHelpers.BitType.BIT_32, renderTexture, targetTexture); 
			} else {

				generateAllPNG(randomCubeColor, randomFloorColor, randomWallColor, HexHelpers.BitType.BIT_32, renderTexture, targetTexture); 
			}
		}
		return; 
	} 
	for(ushort cubeColor = fromColor; cubeColor < toColor; cubeColor++) 
		for(ushort floorColor = fromColor; floorColor < toColor; floorColor++) 
			for(ushort wallColor = fromColor; wallColor < toColor; wallColor++) {

				if(fileType == FileType.GIF) {
					generateAllGIF(cubeColor, floorColor, wallColor, HexHelpers.BitType.BIT_9, renderTexture, targetTexture); 
				} else {
					generateAllPNG(cubeColor, floorColor, wallColor, HexHelpers.BitType.BIT_9, renderTexture, targetTexture); 
				}
			}
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
