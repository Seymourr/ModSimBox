using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Sphere: MonoBehaviour
{
    
    [SerializeField]
    private Node spherePreFab = null;
 
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
    
	public bool hasSpringForce = false;

	public Vector3 springForce;
    
    private Node node = null;
    private List<Node> the_Node = null;
    private float m_accumulator = 0.0f;
    private Dictionary<IntegratorType, Integrator> m_integrators = new Dictionary<IntegratorType,Integrator>();
    
	void Start ()
    {
        m_integrators.Add(IntegratorType.RK4, new RK4Integrator());
        node = Instantiate(spherePreFab, m_spawnPoint, Quaternion.identity) as Node;
        node.transform.parent = transform;
        the_Node = new List<Node>();
        the_Node.Add(node);
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
		if (hasSpringForce) {
			node.ApplyForce (springForce);
			hasSpringForce = false;
		}
    }

    void ClearAndApplyGravity()
    {
        node.ClearForce();
        node.ApplyForce(m_gravity * node.Mass);

    }

    void ApplyGroundForces()
    {
        if(m_groundPlanes == null)
            return;

        foreach (var ground in m_groundPlanes)
        {
            Vector3 groundNormal = ground.rotation * Vector3.up;

            Vector3 groundToPoint = node.State.Position - ground.position;
            float distToGround = Vector3.Dot(groundNormal, groundToPoint);
            float radius = node.transform.localScale.x * 0.5f;

            if (distToGround < radius)
            {
                float penetrationDepth = radius - distToGround;

                //Spring force outwards
                node.ApplyForce(m_groundStiffness * penetrationDepth * groundNormal);
                //Dampingm_groundDamping
                node.ApplyForce(-m_groundDamping * node.State.Velocity);
            }
        }
    }

    void AdvanceSimulation()
    {
        m_integrators[m_integratorType].Advance(the_Node, ApplyForces, m_integratorTimeStep);
    }

	public Node getNode()
	{
		return node;
	}
}
