using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Behaviour
{
    public static Vector3 Seek(this MovingEntity entity, Vector3 target)
    {
        var needVelocity = (target - entity.transform.position).normalized * entity.maxSpeed;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    public static Vector3 Flee(this MovingEntity entity, Vector3 target, float keepDistance)
    {
        var pos = entity.transform.position;
        if ((pos - target).sqrMagnitude >= keepDistance * keepDistance) return default;
        var needVelocity = (pos - target).normalized * entity.maxSpeed;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    public static Vector3 Arrive(this MovingEntity entity, Vector3 target)
    {
        return default;
    }

    public static Vector3 Pursuit(this MovingEntity entity, MovingEntity target)
    {
        return default;
    }

    public static Vector3 Evade(this MovingEntity entity, MovingEntity target)
    {
        return default;
    }

    public static Vector3 Separate(this MovingEntity entity, List<MovingEntity> teammate,float desiredSeparation=2)
    {
        var force = new Vector3();
        var count = 0;
        foreach (var mv in teammate.Where(mv => mv != entity))
        {
            var dis = entity.transform.position - mv.transform.position;
            if (dis.sqrMagnitude <= desiredSeparation * desiredSeparation)
            {
                force += dis.normalized/dis.magnitude;
                count++;
            }
        }

        if (count <= 0) return Vector3.zero;
        return force.normalized * entity.maxForce;
    }

    public static Vector3 Aline(this MovingEntity entity, List<MovingEntity> teammate, float neighborDist = 10)
    {
        var needVelocity = new Vector3();
        var count = 0;
        foreach (var mv in teammate.Where(mv => mv != entity))
        {
            if ((entity.transform.position - mv.transform.position).sqrMagnitude <= neighborDist * neighborDist)
            {
                needVelocity += mv.Velocity;
                count++;
            }
        }

        if (count <= 0) return Vector3.zero;
        needVelocity /= count;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    public static Vector3 Cohesion(this MovingEntity entity, List<MovingEntity> teammate, float neighborDist = 10)
    {
        var center = new Vector3();
        var count = 0;
        foreach (var mv in teammate.Where(mv => mv != entity))
        {
            if ((entity.transform.position - mv.transform.position).sqrMagnitude <= neighborDist * neighborDist)
            {
                center += mv.Velocity;
                count++;
            }
        }

        if (count <= 0) return Vector3.zero;
        center /= count;
        return (center - entity.transform.position).normalized * entity.maxForce;
    }
}