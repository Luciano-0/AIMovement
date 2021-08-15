using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingEntity : MonoBehaviour
{
    public float mass = 1f; //质量
    public float maxSpeed = 5f; //最大速度
    public float maxForce = 1f; //最大作用力
    public float maxTurnRate = 100f; //最大转向速度
    public float rotationSpeed = 100f; //自转速度

    public Vector3 Force { get; private set; }
    public Vector3 Velocity { get; private set; }

    public void AddForce(Vector3 force)
    {
        Force += force;
        if (Force.sqrMagnitude > maxForce * maxForce)
        {
            Force = Force.normalized * maxForce;
        }
    }

    void Start()
    {
    }


    void Update()
    {
        if (mass == 0) mass = 1;
        Velocity = Force / mass * Time.deltaTime + Velocity;
        if (Velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            Velocity = Velocity.normalized * maxSpeed;
        }

        transform.position += Velocity * Time.deltaTime;

        var fn = Velocity.normalized;
        var vn = transform.forward;
        if (fn == default) return;
        var angle = Vector3.Angle(fn, vn);
        if (angle < 0.1) return;
        var axis = Vector3.Cross(fn, vn);
        transform.Rotate(axis, -maxTurnRate * Time.deltaTime, Space.World);
        transform.Rotate(transform.forward * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnDrawGizmosSelected()
    {
    }
}