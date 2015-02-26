using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpringTest: MonoBehaviour
{
    
    [SerializeField]
    private Node nodePreFab = null;
 
	[SerializeField]
    private Vector3 m_gravity = new Vector3(0,-9.82f,0);
    
    [SerializeField]
    private Vector3 m_spawnPoint = new Vector3(0,10,0);
    
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
    
    
    
    private List<Node> nodes = null;
    private List<Spring> springs = null;
    private float m_accumulator = 0.0f;
    private Dictionary<IntegratorType, Integrator> m_integrators = new Dictionary<IntegratorType,Integrator>();
    
	void Start ()
    {
        m_integrators.Add(IntegratorType.RK4, new RK4Integrator());
        Node node = Instantiate(nodePreFab, m_spawnPoint, Quaternion.identity) as Node;
        node.transform.parent = transform;
        Node node2 = Instantiate(nodePreFab, m_spawnPoint+(new Vector3(0.05f,1,0)), Quaternion.identity) as Node;
        node2.transform.parent = transform;
        nodes = new List<Node>();
        nodes.Add(node);
        nodes.Add(node2);
        springs = new List<Spring>();
        springs.Add(new Spring(node,node2));
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
        ApplySprings();
    }

    void ClearAndApplyGravity()
    {
        foreach (var node in nodes)
        {
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
                    node.transform.localScale = new Vector3(1,1-penetrationDepth*2,1);

                    //Spring force outwards
                    node.ApplyForce(m_groundStiffness * penetrationDepth * groundNormal);
                    //Damping
                    node.ApplyForce(-m_groundDamping * node.State.Velocity);
                }
                else{
                    node.transform.localScale = new Vector3(1,1,1);
                }
            }
        }
    }
    
    void ApplySprings(){
        foreach (var spring in springs){
            spring.ApplySpringForces();
        }
    }

    void AdvanceSimulation(){
        m_integrators[m_integratorType].Advance(nodes, ApplyForces, m_integratorTimeStep);
    }
}