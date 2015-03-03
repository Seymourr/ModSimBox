using UnityEngine;

public class SkeletonJoint {

	private Node node1 = null; // leaf
	private Node node2 = null; // connected to pivot
    private Node pivot = null; // connected to node2
	public bool print = false;

    private Vector3 goal_angle;
    private Vector3 pivot_vector;
    
	float stiffness = 50.0f;
    public float Stiffness {
        get { return stiffness; }
        set { stiffness = value; }
    }
    
	float damping = 2.0f;
    public float Damping {
        get { return damping; }
        set { damping = value; }
    }
    
    public SkeletonJoint(float stiffness,Node n1, Node n2, Node pivot_, Vector3 forced_angle){
        this.stiffness = stiffness;
		node1 = n1;
		node2 = n2;
        pivot = pivot_;
        goal_angle = forced_angle;
        pivot_vector = (pivot.State.Position-node2.State.Position).normalized;
    }

	public void ApplySkeletonForces(bool oppositeForce,float skeleton_stiffness_scale){
        Vector3 currentPivot = (pivot.State.Position - node2.State.Position).normalized;
        Quaternion offset = Quaternion.FromToRotation(pivot_vector,currentPivot);
        
        Vector3 currentAngle = (node1.State.Position-node2.State.Position);
        Vector3 relAngle = RotateAroundPoint(currentAngle,Vector3.zero,offset);
        
        Vector3 force_to = RotateAroundPoint(goal_angle, Vector3.zero, offset);
        Vector3 dir = Vector3.Slerp(relAngle.normalized,force_to.normalized,1-(relAngle.normalized-force_to.normalized).magnitude);
    //    Debug.Log(relAngle.normalized);
    //    Debug.Log(force_to.normalized);
    //    Debug.Log(dir);
    //    Debug.Log("--------------------------------------------------------");
        node1.ApplyForce(dir.normalized*stiffness*skeleton_stiffness_scale);
        if(oppositeForce){
            float ratio = 0.1f;
            node2.ApplyForce(-dir.normalized*stiffness*skeleton_stiffness_scale*(1-ratio));
            pivot.ApplyForce(-dir.normalized*stiffness*skeleton_stiffness_scale*ratio);
        }
	}
    
    private Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle){
        return angle * ( point - pivot) + pivot;
    }
}
