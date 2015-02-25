using UnityEngine;

public class Spring {

	private Sphere s1 = null;
	private Sphere s2 = null;
    
    float length = 2.0f;
    public float SpringLength {
        get { return length; }
        set { length = value; }
    }
    
    
	float stiffness = 200.0f;
    public float Stiffness {
        get { return stiffness; }
        set { stiffness = value; }
    }
    
	float damping = 5.0f;
    public float Damping {
        get { return damping; }
        set { damping = value; }
    }
    
    public Spring(Node n1, Node n2){
 
    }

	public void ApplySpringForces(){
		Node node1 = s1.getNode ();
		Node node2 = s2.getNode ();

		Vector3 normLized = node1.State.Position - node2.State.Position;
		float temp = normLized.magnitude;
		normLized /= temp;
		Vector3 FSpring = stiffness*(length - temp) * normLized;
		Vector3 FDamp = -damping * (node1.State.Velocity - node2.State.Velocity);

		s1.hasSpringForce = true;
		s2.hasSpringForce = true;

		s1.springForce = (FSpring + FDamp);
		s2.springForce = ((-FSpring) + (-FDamp));


	}
}
