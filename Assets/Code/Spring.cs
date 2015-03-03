using UnityEngine;

public class Spring {

	private Node node1 = null;
	private Node node2 = null;
    private Vector3 p;
	public bool print = false;
    
    bool isPuppetString = false;
    public bool IsPuppetString {
        get { return isPuppetString; }
    }
   
    float length = 2.0f;
    public float SpringLength {
        get { return length; }
        set { length = value; }
    }
    
    
	float stiffness = 2000.0f;
    public float Stiffness {
        get { return stiffness; }
        set { stiffness = value; }
    }
    
	float damping = 10.0f;
    public float Damping {
        get { return damping; }
        set { damping = value; }
    }
    
    public Spring(Node n1, Node n2){
		node1 = n1;
		node2 = n2;
    }
    
    // Hack solution for puppet string spring
    public Spring(Node n1, Vector3 pp){
		node1 = n1;
		p = pp;
        isPuppetString=true;
    }


	public void ApplySpringForces(){
				Vector3 normLized;
				if (node2 != null) {
		
						normLized = node1.State.Position - node2.State.Position;
				} else {
						normLized = node1.State.Position - p;
				}
				float temp = normLized.magnitude;
				normLized /= temp;
				Vector3 FSpring = stiffness * (length - temp) * normLized;
				if (node2 != null) {
						Vector3 FDamp = -damping * (node1.State.Velocity - node2.State.Velocity);
						node1.ApplyForce (FDamp);
						node2.ApplyForce (-FDamp); //Opposite force
						node2.ApplyForce (-FSpring); //Opposite force
				}
            
				node1.ApplyForce (FSpring);
		
		
				
	

	}
}
