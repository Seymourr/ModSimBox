using UnityEngine;
using System.Collections;

/*
 * Checks at which position the mouse is and what it is doing. 
 */
public class MouseMovementControl : MonoBehaviour {

	[SerializeField]
	private Node DragPrefab = null;

	[SerializeField]
	private Booxie sprT = null; //Modify

	private const int m_mouseButton = 0;
	private const int m_targetGizmoRadius = 1;
	private const float m_dragSpeed = 0.015f;

	private Transform m_dragHook;
	private bool m_dragging;
	private Vector2 m_mouseStart;
	private Vector3 m_dragOrigin;
	private Vector3	m_dragClickHit;

	GameObject latestTarget;
	Node n1;
	Node n2;
	bool springDrag = false;
	private float m_integratorTimeStep = 1.0f / 60.0f; //Ev modify
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
			
				if(springDrag)
				{
					n2.transform.position = new Vector3(m_dragOrigin.x + move.x, m_dragOrigin.y, m_dragOrigin.z + move.y);
					n1.State.Velocity = (n2.transform.position - n1.State.Position) / m_integratorTimeStep;
				
				}
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
				springDrag = false;
				n1 = null;
				sprT.dragSpring = null;
				Destroy (n2.gameObject);
				sprT.nodes.Remove (n2);
				n2 = null;

				m_dragging = false;
				m_dragHook = null;
				latestTarget.GetComponent<Draggable>().dragged = false;
				latestTarget = null;
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
						target.GetComponent<Draggable>().dragged = true;
						latestTarget = target;
						if(target.GetComponent<Node>() != null)
						{
							springDrag = true;
							n1 = target.GetComponent<Node>();

							Vector2 move = (Vector2)Input.mousePosition - m_mouseStart;
							move *= m_dragSpeed;
							Vector3 startPos = new Vector3(m_dragOrigin.x + move.x, m_dragOrigin.y, m_dragOrigin.z + move.y);	
							n2 = Instantiate(DragPrefab, startPos, Quaternion.identity) as Node;

							sprT.nodes.Add (n2); //Requirement: Nodes has to be implemented and >active<
							sprT.dragSpring = new Spring(n1, n2); //Requirement, must have dragSpring
						}
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
