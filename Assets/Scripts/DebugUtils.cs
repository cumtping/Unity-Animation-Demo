using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugUtils {
	// 将文字更新到Text控件
	public static void textToUI(GameObject obj, string text){
		if (obj == null || text == null) {
			return;
		}
		obj.GetComponent<Text> ().text = text;
	}
}
