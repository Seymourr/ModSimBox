using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
    public class PointState
    {
        public PointState()
        {
            Position = Vector3.zero;
            Velocity = Vector3.zero;
        }

        public PointState(Vector3 pos, Vector3 vel)
        {
            Position = pos;
            Velocity = vel;
        }

        public Vector3 Position;
        public Vector3 Velocity;

        public PointState Clone()
        {
            return new PointState(Position, Velocity);
        }
    }

    public PointState State { get; set; }
   
    public Vector3 Force{ get; private set; }
    public float Mass = 1.0f;

    public void ClearForce()
    {
        Force = Vector3.zero;
    }

    public void ApplyForce(Vector3 force)
    {
        Force += force;
    }

    public void SaveState()
    {
        m_savedState = State.Clone();
    }
    public void LoadState()
    {
        State = m_savedState.Clone();
    }

    private PointState m_savedState = new PointState();

    //A note about this way of setting the position in Unity:
    //  This in not how you really would use Unity in most
    //  cases. Normally you use their physics engine, which 
    //  would handle this in a better way with interpolation 
    //  and other fancy features. This is only to get clean 
    //  access to Pos/Vel/Force in a consistent way.

    void Awake()
    {
        State = new Node.PointState();
        //Get initial position
        State.Position = transform.position;
    }
    
    void Update()
    { 
        //Update graphical representation
        transform.position = State.Position;
    }
}
