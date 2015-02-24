using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public enum IntegratorType
{ 
    RK4
}

public interface Integrator
{
    void Advance(List<Node> points, Action<float> updateForcesFunc, float timeStep);
}
