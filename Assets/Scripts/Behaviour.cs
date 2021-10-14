using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class EntityBehaviour
{
    // 靠近
    public static Vector3 Seek(this MovingEntity entity, Vector3 target)
    {
        var dis = target - entity.transform.position;
        var needVelocity = dis.normalized * entity.maxSpeed;
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
                return Vector3.zero;
            }

            return entity.Velocity.normalized * -1;
        }

        var needVelocity = (pos - target).normalized * entity.maxSpeed;
        var deltaVelocity = needVelocity - entity.Velocity;
        return deltaVelocity.normalized * entity.maxForce;
    }

    // 到达
    public static Vector3 Arrive(this MovingEntity entity, Vector3 target, float slowDownDis = 10)
    {
        var toTarget = target - entity.transform.position;
        var dis = toTarget.magnitude;
        if (dis > slowDownDis)
        {
            return entity.Seek(target);
        }

        if (dis > 0.1)
        {
            var needSpeed = (float)Math.Sqrt(2 * entity.maxForce / entity.mass * dis);
            var needVelocity = toTarget.normalized * needSpeed;
            return (needVelocity - entity.Velocity).normalized * entity.maxForce;
        }

        entity.Stop();
        return Vector3.zero;
    }

    // 逃避
    public static Vector3 Evade(this MovingEntity entity, MovingEntity target, float keepDistance)
    {
        var dir = entity.transform.position - target.transform.position;
        var tarSpeed = target.Velocity.magnitude;
        var lookAheadDis = dir.magnitude * tarSpeed / entity.maxSpeed;
        var tarPos = target.transform.position + target.Velocity.normalized * lookAheadDis;
        return entity.Flee(tarPos, keepDistance);
    }

    //追逐
    public static Vector3 Pursuit(this MovingEntity entity, MovingEntity target, float keepDistance = 0.1f)
    {
        var dir = entity.transform.position - target.transform.position;
        // 追到目标后停止
        if (dir.sqrMagnitude <= keepDistance * keepDistance)
        {
            entity.Stop();
            return Vector3.zero;
        }

        //当目标朝向自己时，直接向目标当前位置移动
        if (Vector3.Angle(dir, target.transform.forward) < 20)
        {
            return Seek(entity, target.transform.position);
        }

        //当目标在向其他方向移动时，需要预判目标一段时间后的位置。
        var tarSpeed = target.Velocity.magnitude;
        var lookAheadDis = dir.magnitude * tarSpeed / entity.maxSpeed;
        var tarPos = target.transform.position + target.Velocity.normalized * lookAheadDis;
        return entity.Seek(tarPos);
    }

    //带偏移的追逐
    public static Vector3 OffsetPursuit(this MovingEntity entity, MovingEntity target, Vector3 offset)
    {
        var dir = entity.transform.position - target.transform.position;
        var tarSpeed = target.Velocity.magnitude;
        var lookAheadDis = dir.magnitude * tarSpeed / entity.maxSpeed;
        var tarPos = target.transform.position + target.Velocity.normalized * lookAheadDis +
                     target.transform.TransformVector(offset);
        return entity.Arrive(tarPos, 10);
    }

    public static Vector3 Interpose(this MovingEntity entity, MovingEntity targetA, MovingEntity targetB)
    {
        var targetAPos = targetA.transform.position;
        var targetBPos = targetB.transform.position;
        var centerNow = (targetAPos + targetBPos) / 2;
        var time = (entity.transform.position - centerNow).magnitude / entity.maxSpeed;
        var targetAPosPre = targetAPos + targetA.Velocity * time;
        var targetBPosPre = targetBPos + targetB.Velocity * time;
        return entity.Arrive((targetAPosPre + targetBPosPre) / 2);
    }

    // 徘徊
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
        var force = Vector3.zero;
        Vector3 dir;
        float dis;
        var count = 0;
        foreach (var mv in teammate)
        {
            if (mv == entity) continue;
            dir = entity.transform.position - mv.transform.position;
            dis = dir.magnitude > 0.1f ? dir.magnitude : 0.1f;
            if (dis <= desiredSeparation)
            {
                force += dir.normalized / dis;
                count++;
            }
        }

        if (count == 0) return Vector3.zero;
        return force.normalized * entity.maxForce;
    }

    public static Vector3 Align(this MovingEntity entity, List<MovingEntity> teammate, float neighborDist = 10)
    {
        var needVelocity = Vector3.zero;
        var count = 0;
        foreach (var mv in teammate)
        {
            if (mv == entity) continue;
            if ((entity.transform.position - mv.transform.position).sqrMagnitude <= neighborDist * neighborDist)
            {
                needVelocity += mv.Velocity;
                count++;
            }
        }

        if (count <= 0) return Vector3.zero;
        needVelocity /= count;
        return (needVelocity - entity.Velocity).normalized * entity.maxForce;
    }

    public static Vector3 Cohesion(this MovingEntity entity, List<MovingEntity> teammate, float neighborDist = 10)
    {
        var center = Vector3.zero;
        var count = 0;
        foreach (var mv in teammate)
        {
            if (mv == entity) continue;
            if ((entity.transform.position - mv.transform.position).sqrMagnitude <= neighborDist * neighborDist)
            {
                center += mv.Velocity;
                count++;
            }
        }

        if (count <= 0) return Vector3.zero;
        center /= count;
        return entity.Seek(center);
    }
}