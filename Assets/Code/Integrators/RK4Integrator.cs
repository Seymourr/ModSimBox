using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class RK4Integrator : Integrator
{    
    private struct Derivative
    {
        public void Reset()
        {
            deltaPosition = Vector3.zero;
            deltaVelocity = Vector3.zero;
        }

        public Vector3 deltaPosition;
        public Vector3 deltaVelocity;
    }

    private class CalcData
    {
        public Derivative evalResult = new Derivative();
        public Derivative a = new Derivative();
        public Derivative b = new Derivative();
        public Derivative c = new Derivative();
        public Derivative d = new Derivative();
    }

    private List<CalcData> m_calcData = new List<CalcData>();
    private Derivative zero = new Derivative();

    public void Advance(List<Node> points, Action<float> updateForcesFunc, float timeStep)
    {
        PrepareCalcData(points.Count);

        Evaluate(points, updateForcesFunc, 0.0f);
        for (int i = 0; i < points.Count; i++)
            m_calcData[i].a = m_calcData[i].evalResult;

        Evaluate(points, updateForcesFunc, timeStep * 0.5f);
        for (int i = 0; i < points.Count; i++)
            m_calcData[i].b = m_calcData[i].evalResult;

        Evaluate(points, updateForcesFunc, timeStep * 0.5f);
        for (int i = 0; i < points.Count; i++)
            m_calcData[i].c = m_calcData[i].evalResult;

        Evaluate(points, updateForcesFunc, timeStep);
        for (int i = 0; i < points.Count; i++)
            m_calcData[i].d = m_calcData[i].evalResult;

        for (int i = 0; i < points.Count; i++)
        {
            CalcData p = m_calcData[i];
            Vector3 deltaPos = (1.0f / 6.0f) * (p.a.deltaPosition + 2.0f * (p.b.deltaPosition + p.c.deltaPosition) + p.d.deltaPosition);
            Vector3 deltaVel = (1.0f / 6.0f) * (p.a.deltaVelocity + 2.0f * (p.b.deltaVelocity + p.c.deltaVelocity) + p.d.deltaVelocity);

            points[i].State.Position += deltaPos * timeStep;
            points[i].State.Velocity += deltaVel * timeStep;
        }
    }

    private void Evaluate(List<Node> points, Action<float> updateForcesFunc, float timeStep)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Node point = points[i];
            Derivative derivative = m_calcData[i].evalResult;

            point.SaveState();
            point.State.Position += derivative.deltaPosition * timeStep;
            point.State.Velocity += derivative.deltaVelocity * timeStep;
        }

        updateForcesFunc(timeStep);

        for (int i = 0; i < points.Count; i++)
        {
            Node point = points[i];

            m_calcData[i].evalResult.deltaPosition = point.State.Velocity;
            m_calcData[i].evalResult.deltaVelocity = point.Force / point.Mass;
            
            point.LoadState();
        }
    }

    private void PrepareCalcData(int numPoints)
    {
        int diff = numPoints - m_calcData.Count;

        //Do we need more? Create more! Never shrink
        if (diff > 0)
        {
            for (int i = 0; i < diff; i++)
            {
                m_calcData.Add(new CalcData());
            }
        }

        for (int i = 0; i < numPoints; i++)
        {
            m_calcData[i].evalResult.Reset();
        }
    }
}