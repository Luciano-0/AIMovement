using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Behaviour
{
    // 靠近
    public static Vector3 Seek(this MovingEntity entity, Vector3 target)
    {
        var needVelocity = (target - entity.transform.position).normalized * entity.maxSpeed;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    // 远离
    public static Vector3 Flee(this MovingEntity entity, Vector3 target, float keepDistance)
    {
        var pos = entity.transform.position;
        if ((pos - target).sqrMagnitude >= keepDistance * keepDistance)
        {
            if (entity.Velocity.sqrMagnitude < 0.1)
            {
                entity.Stop();
                return default;
            }

            return entity.Velocity.normalized * -1;
        }

        var needVelocity = (pos - target).normalized * entity.maxSpeed;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    // 到达
    public static Vector3 Arrive(this MovingEntity entity, Vector3 target, float time = 4)
    {
        var toTarget = target - entity.transform.position;
        var dis = toTarget.magnitude;
        if (dis > 0)
        {
            var needSpeed = Math.Min(dis / time, entity.maxSpeed);
            var needVelocity = toTarget.normalized * needSpeed;
            return (needVelocity - entity.Velocity).normalized;
        }

        return Vector3.zero;
    }

    // 追逐
    public static Vector3 Pursuit(this MovingEntity entity, MovingEntity target, float keepDistance = 0.1f)
    {
        var dir = entity.transform.position - target.transform.position;
        // 追到目标后停止
        if (dir.sqrMagnitude <= keepDistance * keepDistance)
        {
            entity.Stop();
            return default;
        }

        //当目标朝向自己时，直接向目标当前位置移动
        if (Vector3.Angle(dir, target.transform.forward) < 20)
        {
            return Seek(entity, target.transform.position);
        }

        //当目标在向其他方向移动时，需要预判目标一段时间后的位置。
        var tarSpeed = target.Velocity.magnitude;
        var lookAheadDis = dir.magnitude * tarSpeed * tarSpeed / (entity.maxSpeed * entity.maxSpeed);
        var tarPos = target.transform.position + target.Velocity.normalized * lookAheadDis;
        return Seek(entity, tarPos);
    }

    // 逃避
    public static Vector3 Wander(this MovingEntity entity, float wanderRadius, float wanderDistance, float wanderJitter,
        bool limit = true)
    {
        entity.WanderTarget += new Vector3(Random.Range(-1 * wanderJitter, wanderJitter),
            Random.Range(-1 * wanderJitter, wanderJitter), 0);
        var limitParam = limit ? 0 : 1;
        var wanderCirclePoint = entity.Velocity.normalized * wanderDistance + entity.transform.position * limitParam;

        entity.WanderTarget = wanderCirclePoint + wanderRadius * (entity.WanderTarget - wanderCirclePoint).normalized;
        return entity.Seek(entity.WanderTarget);
    }

    public static Vector3 Hide()
    {
        return Vector3.down;
    }

    public static Vector3 Separate(this MovingEntity entity, List<MovingEntity> teammate, float desiredSeparation = 2)
    {
        var force = new Vector3();
        var count = 0;
        foreach (var mv in teammate.Where(mv => mv != entity))
        {
            var dis = entity.transform.position - mv.transform.position;
            if (dis.sqrMagnitude <= desiredSeparation * desiredSeparation)
            {
                force += dis.normalized / dis.magnitude;
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