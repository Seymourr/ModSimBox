using UnityEngine;
using System.Collections;

public class Spring : MonoBehaviour {

	// Use this for initialization
	[SerializeField]
	private Sphere s1 = null;

	[SerializeField]
	private Sphere s2 = null;


	void Start () {
		s1.transform.parent = transform;
		s2.transform.parent = transform;
	}
	
	// Update is called once per frame
	void Update () {
		ApplySpringForces ();
	}

	void ApplySpringForces()
	{
		Node p1 = s1.getNode ();
		Node p2 = s2.getNode ();

		float m_totalLength = 2.0f;
		float m_ropeStiffness = 800.0f;
		float m_ropeDamping = 5.0f;

		float segmentLength = m_totalLength;
		Vector3 normLized = p1.State.Position - p2.State.Position;
		float temp = normLized.magnitude;
		normLized /= temp;
		Vector3 FSpring = m_ropeStiffness*(segmentLength - temp) * normLized;
		Vector3 FDamp = -1*m_ropeDamping * (p1.State.Velocity - p2.State.Velocity);
		print ("The force of p1 was " + p1.Force);
		print ("The force of p1 was2 " + s1.getNode().Force);
		p1.ApplyForce (FSpring);
		p1.ApplyForce (FDamp);
		p2.ApplyForce (-FSpring); //Opposite force
		p2.ApplyForce (-FDamp); //Opposite force
		
		print ("The force of p1 is " + p1.Force);
		print ("The force of p1 is2 " + s1.getNode().Force);
	}
}
