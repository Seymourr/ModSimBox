using UnityEngine;
using System.Collections;

public class Draggable : MonoBehaviour {

	 public bool dragged = false;
	private Transform m_dragHook; 

	public Transform getDragHook()
	{
		return m_dragHook ?? this.transform;
	}

	/*
	 * Call this to set which draghook that is being used. 
	 */
	public void setDragHook(Transform dragHook){
		m_dragHook = dragHook;
	}
}
