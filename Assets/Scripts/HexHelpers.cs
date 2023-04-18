using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexHelpers
{
	public enum BitType {
		BIT_9,
		BIT_6,
		BIT_32
	}

	public static Color32 ToColor9BitColor(ushort hexValue) {

		byte red = (byte)((hexValue >> 6) & 0x07);
		byte green = (byte)((hexValue >> 3) & 0x07);
		byte blue = (byte)(hexValue & 0x07);
		
		// Then scaling the values between 0-255 (32-bit color), since Unity does not support 9-bit colors
		red = (byte)((red * 255) / 7); 
		green = (byte)((green * 255) / 7); 
		blue = (byte)((blue * 255) / 7); 

		return new Color32(red, green, blue, 255); 
	}

	public static Color32 ToColor6BitColor(byte hexValue) {

		byte red = (byte)((hexValue >> 4) & 0x3);
		byte green = (byte)((hexValue >> 2) & 0x3);
		byte blue = (byte)(hexValue & 0x3);
		
		// Then scaling the values between 0-255 (32-bit color), since Unity does not support 9-bit colors
		red = (byte)((red * 255) / 0x3); 
		green = (byte)((green * 255) / 0x3); 
		blue = (byte)((blue * 255) / 0x3); 

		return new Color32(red, green, blue, 255); 
	}

	public static Color32 ToColor32BitColor(uint hexValue) {
		byte red = (byte)((hexValue >> 16) & 0xFF);
		byte blue = (byte)((hexValue) & 0xFF);
		byte green = (byte)((hexValue >> 8) & 0xFF);
		byte alpha = (byte)((hexValue >> 24) & 0xFF);

		return new Color32(red, blue, green, 255); 
	}
}
