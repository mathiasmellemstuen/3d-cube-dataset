using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentObject : MonoBehaviour {

	public MeshRenderer meshRenderer; 

	public void Awake() {
		meshRenderer = GetComponent<MeshRenderer>(); 
	}

	public void setColor(uint hexValue, HexHelpers.BitType bitType) { 
		
		switch(bitType) {
			case HexHelpers.BitType.BIT_6: 
					 meshRenderer.material.color = HexHelpers.ToColor6BitColor((byte)hexValue); 
				break; 
			case HexHelpers.BitType.BIT_9:
					 meshRenderer.material.color = HexHelpers.ToColor9BitColor((ushort)hexValue); 
				break; 
			case HexHelpers.BitType.BIT_32:
					 meshRenderer.material.color = HexHelpers.ToColor32BitColor(hexValue); 
				break; 
		}
	}
	public void setColor(Color color) {
		meshRenderer.material.color = color; 
	}
}
