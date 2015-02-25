using UnityEngine;

public class Spring {

	private Node node1 = null;
	private Node node2 = null;
   
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
		node1 = n1;
		node2 = n2;
 
    }


	public void ApplySpringForces(){
		Vector3 normLized = node1.State.Position - node2.State.Position;
		float temp = normLized.magnitude;
		normLized /= temp;
		Vector3 FSpring = stiffness*(length - temp) * normLized;
		Vector3 FDamp = -damping * (node1.State.Velocity - node2.State.Velocity);
		
		node1.ApplyForce (FSpring);
		node1.ApplyForce (FDamp);
		node2.ApplyForce (-FSpring); //Opposite force
		node2.ApplyForce (-FDamp); //Opposite force

	}
}
