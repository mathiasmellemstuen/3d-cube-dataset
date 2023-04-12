using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentObject : MonoBehaviour {

	public MeshRenderer meshRenderer; 

	public void Awake() {
		meshRenderer = GetComponent<MeshRenderer>(); 
	}
	
	public void setColor(ushort hexValue) { 
		 meshRenderer.material.color = HexHelpers.ToColor9BitColor(hexValue); 
	}
}
