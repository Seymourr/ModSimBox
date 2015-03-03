using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Booxie: MonoBehaviour
{
    
    [SerializeField]
    private Node nodePreFab = null;
 
	[SerializeField]
    private Vector3 m_gravity = new Vector3(0,-9.82f,0);
    
    [SerializeField]
    private Vector3 m_spawnPoint = new Vector3(-4,3,5);
    
    [SerializeField]
    private IntegratorType m_integratorType = IntegratorType.RK4;

    [SerializeField]
    private float m_integratorTimeStep = 1.0f / 60.0f;
    
    [SerializeField]
    private Transform[] m_groundPlanes = null;
    
    [SerializeField]
    private float m_groundStiffness = 800.0f;

    [SerializeField]
    private float m_groundDamping = 5.0f;
    
    [SerializeField]
    private float scale = 1.0f;
    
    [SerializeField]
    private float leg_height = 2.0f;
    
    [SerializeField]
    private float leg_thickness = 1.0f;
    
    [SerializeField]
    private float hip_width = 2.0f;

    [SerializeField]
    private float nodeSize = 0.5f;
    
    [SerializeField]
    private float skeleton_stiffness_scale = 1.0f;
    
    [SerializeField]
    private bool puppet_strings = false;
    
    [SerializeField]
    private bool joint_restrictions = true;
    
    [SerializeField]
    private bool opposite_force = true;

    [SerializeField]
    private bool lift_right_leg = false;
    
    [SerializeField]
    private bool lift_left_leg = false;
    
    
    public List<Node> nodes = null; //Required for drag
	public Spring dragSpring = null;
    private List<Spring> springs = null;
    private List<SkeletonJoint> skeleton = null;
    private float m_accumulator = 0.0f;
    private Dictionary<IntegratorType, Integrator> m_integrators = new Dictionary<IntegratorType,Integrator>();
    
	void Start (){
        m_integrators.Add(IntegratorType.RK4, new RK4Integrator());
        nodes = new List<Node>();
        springs = new List<Spring>();
        skeleton = new List<SkeletonJoint>();
        
        //legs
        Vector3 leg_dist = new Vector3(hip_width*scale,0,0);
        List<Node> l_leg = createLeg(m_spawnPoint);
        List<Node> r_leg = createLeg(m_spawnPoint+leg_dist);
        
        //hips
        Vector3 hip_offset = new Vector3(leg_thickness/2,leg_thickness*1.5f+leg_height*2,leg_thickness/2)*scale;
        float hip_radius = leg_thickness*scale*1.5f;
        
        Node lhip = Instantiate(nodePreFab, m_spawnPoint + hip_offset, Quaternion.identity) as Node;
        Node rhip = Instantiate(nodePreFab, m_spawnPoint + leg_dist + hip_offset, Quaternion.identity) as Node;
        lhip.transform.parent = transform;
        rhip.transform.parent = transform;
        lhip.transform.localScale = new Vector3(hip_radius,hip_radius,hip_radius);
        rhip.transform.localScale = new Vector3(hip_radius,hip_radius,hip_radius);
        nodes.Add(lhip);
        nodes.Add(rhip);
        addSpringTo(lhip,l_leg[16]);
        addSpringTo(lhip,l_leg[15]);
        addSpringTo(lhip,l_leg[14]);
        addSpringTo(lhip,l_leg[13]);
        addSpringTo(rhip,r_leg[16]);
        addSpringTo(rhip,r_leg[15]);
        addSpringTo(rhip,r_leg[14]);
        addSpringTo(rhip,r_leg[13]);
        
        //spine
        int spine_cords = 4;
        float spine_length = hip_radius; //don't forget scale if changed
        Vector3 spine_offset = lhip.State.Position + (rhip.State.Position - lhip.State.Position)/2;
        spine_offset += new Vector3(0,spine_length/2,0);
        Node spine = Instantiate(nodePreFab, spine_offset, Quaternion.identity) as Node;
        spine.transform.parent = transform;
        addSpringTo(lhip,spine);
        addSpringTo(rhip,spine);
        nodes.Add(spine);
        spine.transform.localScale = new Vector3(spine_length,spine_length,spine_length);
        
        List<Node> spines = new List<Node>();
        spines.Add(spine);
        Node lastSpine = spine;
        for(int i = 1 ; i<spine_cords ; i++){
            Node prev = lastSpine;
            lastSpine = addNode(lastSpine,0,spine_length,0);
            spines.Add(lastSpine);
            lastSpine.transform.localScale = new Vector3(spine_length,spine_length,spine_length);
            addSpringTo(lastSpine,prev);
        }
        
        //shoulders
        float shoulder_size = spine_length; //don't forget scale if changed
        Vector3 shoulder_offset = lastSpine.State.Position+new Vector3(0,shoulder_size*0.4f,0);
        Node l_shoulder = Instantiate(nodePreFab, shoulder_offset+new Vector3(-shoulder_size*0.6f,0,0), Quaternion.identity) as Node;
        Node r_shoulder = Instantiate(nodePreFab, shoulder_offset+new Vector3(shoulder_size*0.6f,0,0), Quaternion.identity) as Node;
        l_shoulder.transform.parent = transform;
        r_shoulder.transform.parent = transform;
        l_shoulder.transform.localScale = new Vector3(shoulder_size,shoulder_size,shoulder_size);
        r_shoulder.transform.localScale = new Vector3(shoulder_size,shoulder_size,shoulder_size);
        nodes.Add(l_shoulder);
        nodes.Add(r_shoulder);
        addSpringTo(l_shoulder,lastSpine);
        addSpringTo(r_shoulder,lastSpine);
        
        //arms
        List<Node> r_arm = createLeg(r_shoulder.State.Position+new Vector3(-nodeSize*scale+leg_thickness*scale+leg_height*scale,-leg_height*scale-leg_thickness*scale/2,-leg_thickness/2+nodeSize*scale/2));
        Vector3 middle = r_arm[0].State.Position+(r_arm[16].State.Position-r_arm[0].State.Position)/2;
        var rotation = Quaternion.Euler(0, 0, 90);
        foreach (Node node in r_arm){
            node.State.Position = RotateAroundPoint(node.State.Position,middle,rotation);
        }
        addSpringTo(r_arm[16],r_shoulder);
        addSpringTo(r_arm[15],r_shoulder);
        addSpringTo(r_arm[14],r_shoulder);
        addSpringTo(r_arm[13],r_shoulder);
        
        List<Node> l_arm = createLeg(l_shoulder.State.Position+new Vector3(-shoulder_size/2+nodeSize*scale/2-leg_thickness*scale-leg_height*scale,-leg_height*scale-leg_thickness*scale/2,-leg_thickness/2+nodeSize*scale/2));
        middle = l_arm[0].State.Position+(l_arm[16].State.Position-l_arm[0].State.Position)/2;
        rotation = Quaternion.Euler(0, 0, -90);
        foreach (Node node in l_arm){
            node.State.Position = RotateAroundPoint(node.State.Position,middle,rotation);
        }
        addSpringTo(l_arm[16],l_shoulder);
        addSpringTo(l_arm[15],l_shoulder);
        addSpringTo(l_arm[14],l_shoulder);
        addSpringTo(l_arm[13],l_shoulder);
        
        //Add puppetstrings
        if(1+1==2){
            float stringHeight = 5.0f;
            addSpringTo(l_shoulder,l_shoulder.State.Position+new Vector3(0,stringHeight,0));
            addSpringTo(r_shoulder,l_shoulder.State.Position+new Vector3(0,stringHeight,0));
            addSpringTo(l_arm[8],l_arm[8].State.Position+new Vector3(0,stringHeight,0));
            addSpringTo(r_arm[8],r_arm[8].State.Position+new Vector3(0,stringHeight,0));
        }
        
        
        //Add skeleton
        // foot - knee - hip
        addSkeletonTo(10,r_leg[17],r_leg[8],rhip);
        addSkeletonTo(10,l_leg[17],l_leg[8],lhip);
        
        
        // knee - hip - spine
        if(lift_right_leg)
            addSkeletonTo(10,r_leg[8],rhip,spine,new Vector3(0,0,-1));
        else{
            addSkeletonTo(10,r_leg[8],rhip,spine);
            
        }
        if(lift_left_leg)
            addSkeletonTo(10,l_leg[8],lhip,spine,new Vector3(0,0,-1));
        else{
            addSkeletonTo(10,l_leg[8],lhip,spine);
            
        }
        
        // hip - spine - spine
        addSkeletonTo(10,lhip,spine,spines[1]);
        addSkeletonTo(10,rhip,spine,spines[1]);
        
        //shoulder - arm - hand
        addSkeletonTo(10,l_arm[17],l_arm[8],l_shoulder);
        addSkeletonTo(10,r_arm[17],r_arm[8],r_shoulder);
        
	}
    
    private Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion angle){
        return angle * ( point - pivot) + pivot;
    }
    
    private List<Node> createLeg(Vector3 pos){
        List<Node> c1 = createBox(leg_thickness,leg_height,leg_thickness,pos); //left calve
        List<Node> t1= createBox(leg_thickness,leg_height,leg_thickness,pos+new Vector3(0,(leg_thickness+leg_height)*scale,0)); // left thigh
        Node lknee = addNode(c1[4],leg_thickness*scale/2,leg_thickness*scale/2,leg_thickness*scale/2);
        lknee.transform.localScale = new Vector3(leg_thickness*scale*1.3f,leg_thickness*scale*1.3f,leg_thickness*scale*1.3f);
        Node foot = addNode(c1[0],leg_thickness*scale/2,-leg_thickness*scale/2,leg_thickness*scale/2);
        foot.transform.localScale = new Vector3(leg_thickness*scale*1.3f,leg_thickness*scale*1.3f,leg_thickness*scale*1.3f);
        addSpringTo(lknee,c1[5]);
        addSpringTo(lknee,c1[6]);
        addSpringTo(lknee,c1[7]);
        addSpringTo(lknee,t1[0]);
        addSpringTo(lknee,t1[1]);
        addSpringTo(lknee,t1[2]);
        addSpringTo(lknee,t1[3]);
        c1.Add(lknee);
        c1.AddRange(t1);
        addSpringTo(foot,c1[1]);
        addSpringTo(foot,c1[2]);
        addSpringTo(foot,c1[3]);
        c1.Add(foot);
        return c1;
    }

    private List<Node> createBox(float width, float height, float depth, Vector3 pos){
        Node n000 = Instantiate(nodePreFab, pos, Quaternion.identity) as Node;
        n000.transform.parent = transform;
        n000.transform.localScale = new Vector3(nodeSize*scale,nodeSize*scale,nodeSize*scale);
        List<Node> box = new List<Node>();
        nodes.Add(n000);
        
        Node n100 = addNode(n000,width*scale,0,0);
        Node n010 = addNode(n000,0,height*scale,0);
        Node n001 = addNode(n000,0,0,depth*scale);
        Node n110 = addNode(n010,width*scale,0,0);
        Node n101 = addNode(n100,0,0,depth*scale);
        Node n011 = addNode(n001,0,height*scale,0);
        Node n111 = addNode(n110,0,0,depth*scale);
        addSpringTo(n111,n101);
        addSpringTo(n111,n011);
        addSpringTo(n010,n011);
        addSpringTo(n001,n101);
        addSpringTo(n100,n110);
        box.Add(n000);
        box.Add(n001);
        box.Add(n101);
        box.Add(n100);
        box.Add(n010);
        box.Add(n011);
        box.Add(n111);
        box.Add(n110);
        
        //The 3d diagonals
        addSpringTo(n111,n000);
        addSpringTo(n101,n010);
        addSpringTo(n001,n110);
        addSpringTo(n011,n100);
        
        //2D diagonals
        addSpringTo(n111,n010);
        addSpringTo(n110,n011);
        addSpringTo(n101,n000);
        addSpringTo(n001,n100);
        
        return box;
    }
    
    private void addSkeletonTo(float stiffness,Node target, Node pivot1, Node pivot2){
        var f_angle = (target.State.Position-pivot1.State.Position).normalized;
        addSkeletonTo(stiffness,target,pivot1,pivot2,f_angle);
    }
    private void addSkeletonTo(float stiffness,Node target, Node pivot1, Node pivot2, Vector3 angle){
        var f_angle = (target.State.Position-pivot1.State.Position).normalized;
        print(f_angle);
        SkeletonJoint skel = new SkeletonJoint(stiffness,target,pivot1,pivot2,angle);
        SkeletonJoint skel2 = new SkeletonJoint(stiffness,pivot2,pivot1,target,-angle);
        skeleton.Add(skel);
       // skeleton.Add(skel2);
    }
    
    private Node addNode(Node parent, float offsetx, float offsety, float offsetz){
        Node node2 = Instantiate(nodePreFab, parent.transform.position+(new Vector3(offsetx,offsety,offsetz)), Quaternion.identity) as Node;
        node2.transform.parent = transform;
        node2.GetComponent<Draggable>().setDragHook (/*Insert something draggable of type Transform..*/transform);
        nodes.Add(node2);
        node2.transform.localScale = new Vector3(nodeSize*scale,nodeSize*scale,nodeSize*scale);
        addSpringTo(parent,node2);
        return node2;
    }
    
    private void addSpringTo(Node node1, Node node2){
        Spring spring = new Spring(node2,node1);
        Vector3 dist = node1.State.Position - node2.State.Position;
        spring.SpringLength = dist.magnitude;
        springs.Add(spring);
    }
    
    //puppet string
    private void addSpringTo(Node node1, Vector3 pos){
        Spring spring = new Spring(node1,pos);
        Vector3 dist = node1.State.Position - pos;
        spring.SpringLength = dist.magnitude;
        springs.Add(spring);
    }

	void Update () 
    {
        m_accumulator += Mathf.Min(Time.deltaTime / m_integratorTimeStep, 3.0f);

        while (m_accumulator > 1.0f)
        {
            m_accumulator -= 1.0f;

            AdvanceSimulation();
        }

	}

    void ApplyForces(float timeStep)
    {
        ClearAndApplyGravity();
        ApplyGroundForces();
		ApplyNodeForces();
        ApplySprings();
		DragUpdate ();
        if(joint_restrictions){
            applyJointRestrictions();
        }
    }
    
    void applyJointRestrictions(){
        foreach (var skel in skeleton){
            skel.ApplySkeletonForces(opposite_force,skeleton_stiffness_scale);
        }
    }

    void ClearAndApplyGravity()
    {
        foreach (var node in nodes){
            node.ClearForce();
            node.ApplyForce(m_gravity * node.Mass);
        }
    }

   void ApplyGroundForces()
    {
        if(m_groundPlanes == null)
            return;

        foreach (var ground in m_groundPlanes)
        {
            Vector3 groundNormal = ground.rotation * Vector3.up;

            foreach (var node in nodes)
            {
                Vector3 groundToPoint = node.State.Position - ground.position;
                float distToGround = Vector3.Dot(groundNormal, groundToPoint);
                float radius = node.transform.localScale.x * 0.5f;

                if (distToGround < radius){
                    float penetrationDepth = radius - distToGround;

                    //Spring force outwards
                    node.ApplyForce(m_groundStiffness * penetrationDepth * groundNormal);
                    //Damping
                    node.ApplyForce(-m_groundDamping * node.State.Velocity);
                }
            }
        }
    }
    
	/*Force between nodes (body parts) to enable collision between them. */
	void ApplyNodeForces(){
		foreach (var node1 in nodes) //Collision is possible between all body parts
		{
			foreach (var node2 in nodes)
			{
				float node1Radius = node1.transform.localScale.x * 0.5f; //Radius of node 1
				float node2Radius = node2.transform.localScale.x * 0.5f; //Radius of node 2

				Vector3 node1toNode2 = node2.State.Position - node1.State.Position;
				float distance = node1toNode2.magnitude;
				if(distance != 0)
				{
					node1toNode2 /= distance; //Normalize
					distance -= node1Radius + node2Radius; //Adjust distance based on radiuses. 
				}
				if(distance < 0)
				{
					float penetrationDepth = distance;
					node1.ApplyForce(1000.0f * penetrationDepth * node1toNode2); //Force outwards.
					node1.ApplyForce(0.001f * penetrationDepth * node1.State.Velocity); //Damping
					node2.ApplyForce(-(penetrationDepth * 1000.0f * node1toNode2)); //Force
					node2.ApplyForce(0.001f * -(penetrationDepth * node1.State.Velocity));//damping
				}
			}
		}
	}

    void ApplySprings(){
        foreach (var spring in springs){
            if(spring.IsPuppetString&&!puppet_strings){
                continue;
            }
            spring.ApplySpringForces();
        }
    }

	void DragUpdate()
	{
		if (dragSpring != null) 
		{
			dragSpring.ApplySpringForces ();
			
		}
	}

    void AdvanceSimulation(){
        m_integrators[m_integratorType].Advance(nodes, ApplyForces, m_integratorTimeStep);
    }
}
