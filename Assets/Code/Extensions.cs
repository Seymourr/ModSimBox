using UnityEngine;
using System.Collections;

using System.Linq;



public static class Extensions{
	
	public static bool HasComponent<T>(this GameObject obj) where T : Component{
		return obj.GetComponentsInChildren<T>().FirstOrDefault() != null;
	}
}