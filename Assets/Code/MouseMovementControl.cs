using UnityEngine;
using System.Collections;

/*
 * Checks at which position the mouse is and what it is doing. 
 */
public class MouseMovementControl : MonoBehaviour {

	private const int m_mouseButton = 0;
	private const int m_targetGizmoRadius = 1;
	private const float m_dragSpeed = 0.015f;

	private Transform m_dragHook;
	private bool m_dragging;
	private Vector2 m_mouseStart;
	private Vector3 m_dragOrigin;
	private Vector3	m_dragClickHit;

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(m_dragging){
			if(Input.GetMouseButton(m_mouseButton) && m_dragHook != null){
				//drag along xz-plane
				Vector2 move = (Vector2)Input.mousePosition - m_mouseStart;
				move *= m_dragSpeed;
				m_dragHook.position = new Vector3(m_dragOrigin.x + move.x, m_dragOrigin.y, m_dragOrigin.z + move.y);

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit [] hits;
				hits = Physics.RaycastAll(ray, 100.0f);
				foreach(RaycastHit hit in hits){
					if(hit.transform.gameObject.tag == "Plane"){
						Vector3 delta = hit.point - m_dragClickHit;
						Vector3 position = new Vector3(m_dragOrigin.x + delta.x, m_dragOrigin.y, m_dragOrigin.z + delta.z);
						m_dragHook.position = position;
						break;						
					}
				}
			}
			if(Input.GetMouseButtonUp(m_mouseButton)){
				m_dragging = false;
				m_dragHook = null;
				Screen.showCursor = true;
			}
		} else if(Input.GetMouseButtonDown(m_mouseButton)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			bool foundDraggable = false;
			RaycastHit[] hits;
			hits = Physics.RaycastAll(ray, 100.0f);
			foreach(RaycastHit hit in hits){
				if(hit.transform.gameObject.tag == "Plane"){
					m_dragClickHit = hit.point;
				} else {
					if (foundDraggable) continue;
					
					GameObject target = hit.transform.gameObject;
					if (target.HasComponent<Draggable>()) {	
						//Debug.Log("begin drag");
						m_dragging = true;
						m_dragHook = target.GetComponent<Draggable>().getDragHook();
						m_mouseStart = Input.mousePosition;
						m_dragOrigin = m_dragHook.position;
						Screen.showCursor = false;
						foundDraggable = true;
					}
				}
			}
		}
	}
}
