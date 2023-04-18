using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

[CustomEditor(typeof(DatasetGenerator))]
public class DatasetGeneratorEditor : Editor
{
	public override void OnInspectorGUI() {
		
		DrawDefaultInspector(); 

		DatasetGenerator datasetGenerator = (DatasetGenerator)target; 

		if(GUILayout.Button("Generate dataset")) {
			datasetGenerator.generateDataset();
		}

		if(GUILayout.Button("Generate angles list file")) {
			datasetGenerator.generateAngleFile();
		}

	}
}
