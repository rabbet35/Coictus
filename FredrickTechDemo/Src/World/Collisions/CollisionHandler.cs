﻿using FredrickTechDemo.FredsMath;
using System.Collections.Generic;
using System.Linq;

namespace FredrickTechDemo
{
    public enum ColliderType//used for detecting which type a collider is
    {
        none,
        aabb,
        plane,
        sphere,
        point
    }

    /*An abstraction class which manipulates multiple entity velocities based on their colliders and detects collisions.
      All collisions will be FLAT using this system. Meaning all collisions will be axis aligned. Any collision on any axis should
      cancel the respective axis on the velocity of the child entities.*/
    public static class CollisionHandler
    {
        /*Takes in a list of worldColliders and entity colliders and tests them against eachother, correcting the entities.
          This should be done before Entity vs entity collisions.*/
        public static void doWorldCollisions(List<ICollider> worldColliders, Dictionary<int, ICollider> entityColliders)
        {
            ICollider currentWorldCollider;
            ICollider currentEntityCollider;
            Vector3D currentCollisionDirection;
            double currentCollisionOverlapResult = 0;
            bool currentCollisionTouching = false;
            for (int i = 0; i < worldColliders.Count; i++)
            {
                currentWorldCollider = worldColliders.ElementAt(i);
                if (currentWorldCollider == null)
                {
                    Application.warn("CollisionHander.doWorldCollisions() has detcted a null world collider object at index " + i + ", removing.");
                    worldColliders.RemoveAt(i);
                    continue;
                }

                for(int j = 0; j < entityColliders.Count; j++)
                {
                    currentEntityCollider = entityColliders.Values.ElementAt(j);
                    if (currentEntityCollider == null)
                    {
                        Application.warn("CollisionHander.doWorldCollisions() has detcted a null entity collider object at index " + i + ", removing.");
                        entityColliders.Remove(entityColliders.Keys.ElementAt(j));
                        continue;
                    }

                    if(currentCollisionTouching = getOverlapAndDirectionForColliderTypes(currentWorldCollider, currentEntityCollider, out currentCollisionDirection, out currentCollisionOverlapResult))
                    {
                        currentEntityCollider.getParent().applyCollision(currentCollisionDirection, currentCollisionOverlapResult);
                    }
                }
            }
        }


        /*Takes in a dictionary of all the entity colliders in the world and tests them against eachother.*/
        public static void doEntityCollisions(Dictionary<int, ICollider> entityColliders)
        {
            double currentCollisionOverlapResult = 0;
            Vector3D currentCollisionDirection;
            ICollider currentColliderA;//collider A is tested with all other colliders after it, which are assigned to collider B. Collision results are applied to collider B's parent respectively.
            ICollider currentColliderB;

            for(int i = 0; i < entityColliders.Values.Count; i++)//Loop through each collider and chose the next one to be colliderA
            {
                currentColliderA = entityColliders.Values.ElementAt(i);
                if (currentColliderA == null)
                {
                    Application.warn("CollisionHander.doEntityCollisions() has detcted a null collider object at index " + i + ", removing.");
                    entityColliders.Remove(entityColliders.Keys.ElementAt(i));
                    continue;
                }

                for (int j = 0; j < entityColliders.Values.Count; j++)//Loop through each collider and chose the next one to be colliderB
                {
                    if (i == j)/*if this is the same entity collider, skip. We dont want an entity collding with itself.*/continue;

                    currentColliderB = entityColliders.Values.ElementAt(j);
                    if(currentColliderB == null)//check and weed out any null colliders
                    {
                        Application.warn("CollisionHander.doEntityCollisions() has detcted a null collider object at index " + j + ", removing.");
                        entityColliders.Remove(entityColliders.Keys.ElementAt(j));
                        continue;
                    }
                    if (getOverlapAndDirectionForColliderTypes(currentColliderA, currentColliderB, out currentCollisionDirection, out currentCollisionOverlapResult))
                    {
                        if (currentColliderA.getCollisionWeight() >= currentColliderB.getCollisionWeight())
                        {
                            currentColliderB.getParent().applyCollision(currentCollisionDirection, currentCollisionOverlapResult);
                        }

                        /*Send info about eachother and parents can decide what to do with it*/
                        if (currentColliderA.getHasParent() && currentColliderB.getHasParent())
                        {
                            currentColliderB.getParent().onCollidedBy(currentColliderA.getParent());
                        }
                    }

                }
            }
        }

        private static bool getOverlapAndDirectionForColliderTypes(ICollider colliderA, ICollider colliderB, out Vector3D direction, out double overlap)
        {
            if(colliderA.getType() == ColliderType.plane)
            {
                if (colliderB.getType() == ColliderType.aabb)
                {
                    return getOverlapAndDirection((PlaneCollider)colliderA, (AABBCollider)colliderB, out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.sphere)
                {
                    return getOverlapAndDirection((PlaneCollider)colliderA, (SphereCollider)colliderB, out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.point)
                {
                    return getOverlapAndDirection((PlaneCollider)colliderA, (PointCollider)colliderB, out direction, out overlap);
                }
            }
            else if(colliderA.getType() == ColliderType.aabb)
            {
                if (colliderB.getType() == ColliderType.aabb)
                {
                    return getOverlapAndDirection((AABBCollider)colliderA, (AABBCollider)colliderB, out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.sphere)
                {
                    return getOverlapAndDirection((AABBCollider)colliderA, (SphereCollider)colliderB, out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.point)
                {
                    overlap = 0;
                    direction = Vector3D.zero;
                    return !AABBCollider.isPositionNotInsideBox((AABBCollider)colliderA, ((PointCollider)colliderB).pos);
                }
            }
            else if(colliderA.getType() == ColliderType.sphere)
            {
                if (colliderB.getType() == ColliderType.aabb)
                {
                    return getOverlapAndDirection((SphereCollider)colliderA, (AABBCollider)colliderB, out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.sphere)
                {
                    return getOverlapAndDirection((SphereCollider)colliderA, (SphereCollider)colliderB,out direction, out overlap);
                }
                else if (colliderB.getType() == ColliderType.point)
                {
                    return getOverlapAndDirection((SphereCollider)colliderA, (PointCollider)colliderB, out direction, out overlap);
                }
            }
            else if(colliderA.getType() == ColliderType.point)
            {
                if (colliderB.getType() == ColliderType.aabb)
                {
                    overlap = 0;
                    direction = Vector3D.zero;
                    return !AABBCollider.isPositionNotInsideBox((AABBCollider)colliderB, ((PointCollider)colliderA).pos) ;
                }
                else if (colliderB.getType() == ColliderType.sphere)
                {
                    return getOverlapAndDirection((SphereCollider)colliderB, (PointCollider)colliderA, out direction, out overlap);
                }
            }
            direction = Vector3D.zero;
            overlap = 0;
            return false;
        }

        #region sphereCollisions
        public static bool getOverlapAndDirection(SphereCollider sphereA, SphereCollider sphereB, out Vector3D direction, out double overlap)
        {
            double radiusCombined = sphereA.radius + sphereB.radius;
            double distanceBetweenCenters = Vector3D.distance(sphereA.pos, sphereB.pos);

            if (distanceBetweenCenters < radiusCombined)//if they are touching
            {
                overlap = radiusCombined - distanceBetweenCenters;
                direction = Vector3D.zero;//For now, we do not need to do collision resolutions with spheres and spheres.
                return true;
            }
            direction = Vector3D.zero;
            overlap = 0;
            return false;
        }
        public static bool getOverlapAndDirection(SphereCollider sphere, PointCollider point, out Vector3D direction, out double overlap)
        {
            double distanceBetweenCenters = Vector3D.distance(sphere.pos, point.pos);

            if (distanceBetweenCenters < sphere.radius)//if they are touching
            {
                overlap = sphere.radius - distanceBetweenCenters;
                direction = Vector3D.zero;//For now, we do not need to do collision resolusions with spheres and points.
                return true;
            }
            direction = Vector3D.zero;
            overlap = 0;
            return false;
        }

        /*In a case where a sphere is colliding a aabb*/
        public static bool getOverlapAndDirection(SphereCollider sphere, AABBCollider aabb, out Vector3D direction, out double overlap)
        {
            Vector3D closestPointOnBoxToSphere = Vector3D.clamp(sphere.pos, aabb.minBounds, aabb.maxBounds);
            double distanceFromSphereCenterToBox = Vector3D.distance(sphere.pos, closestPointOnBoxToSphere);

            if (distanceFromSphereCenterToBox < sphere.radius)//if they are touching
            {
                overlap = sphere.radius - distanceFromSphereCenterToBox;
                direction = Vector3D.zero;//For now, we dont need spheres to move AABBs
                return true;
            }
            direction = Vector3D.zero;
            overlap = 0;
            return false;
        }

        /*in a case where an aabb is colliding a sphere*/
        public static bool getOverlapAndDirection(AABBCollider aabb, SphereCollider sphere, out Vector3D direction, out double overlap)
        {
            Vector3D closestPointOnBoxToSphere = Vector3D.clamp(sphere.pos, aabb.minBounds, aabb.maxBounds);
            double distanceFromSphereCenterToBox = Vector3D.distance(sphere.pos, closestPointOnBoxToSphere);

            if (distanceFromSphereCenterToBox < sphere.radius)//if they are touching
            {
                //Here we will be using a variation of the "shallowest axis" method to get overlap and normal, sort of treats sphere like a box, but thats ok for our purposes.
                double overlapX;
                double overlapY;
                double overlapZ;

                Vector3D boxCenter = aabb.centerVec;

                bool isSphereOnRight = boxCenter.x < sphere.pos.x;
                bool isSphereOnTop = boxCenter.y < sphere.pos.y;
                bool isSphereBehind = boxCenter.z < sphere.pos.z;

                overlapX = aabb.extentX + sphere.radius - (isSphereOnRight ? sphere.pos.x - boxCenter.x : boxCenter.x - sphere.pos.x);
                overlapY = aabb.extentY + sphere.radius - (isSphereOnTop ? sphere.pos.y - boxCenter.y : boxCenter.y - sphere.pos.y);
                overlapZ = aabb.extentZ + sphere.radius - (isSphereBehind ? sphere.pos.z - boxCenter.z : boxCenter.z - sphere.pos.z);

                double smallestOverlap = MathUtil.min3(overlapX, overlapY, overlapZ);


                if (overlapX == smallestOverlap)
                {
                    overlap = overlapX;
                    direction = new Vector3D(isSphereOnRight ? 1D : -1D, 0, 0);
                }
                else if (overlapY == smallestOverlap)
                {
                    overlap = overlapY;
                    direction = new Vector3D(0, isSphereOnTop ? 1D : -1D, 0);
                }
                else if (overlapZ == smallestOverlap)
                {
                    overlap = overlapZ;
                    direction = new Vector3D(0, 0, isSphereBehind ? 1D : -1D);
                }
                else
                {
                    overlap = 0;
                    direction = Vector3D.zero;
                }
                return true;
            }
            direction = Vector3D.zero;
            overlap = 0;
            return false;
        }
        #endregion sphereCollisions

        #region aabbCollisions

        public static bool getOverlapAndDirection(AABBCollider boxA, AABBCollider boxB, out Vector3D direction, out double overlap)
        {
            if(!AABBCollider.areBoxesNotTouching(boxA, boxB))
            {
                //Here we will be using the "shallowest axis" method to determine the resulting hit normal.
                double overlapX;
                double overlapY;
                double overlapZ;
                Vector3D boxACenter = boxA.centerVec;
                Vector3D boxBCenter = boxB.centerVec;

                bool isBoxBOnRight = boxACenter.x < boxBCenter.x;
                bool isBoxBOnTop = boxACenter.y < boxBCenter.y;
                bool isBoxBBehind = boxACenter.z < boxBCenter.z;

                overlapX = boxA.extentX + boxB.extentX - (isBoxBOnRight ? boxBCenter.x - boxACenter.x : boxACenter.x - boxBCenter.x);
                overlapY = boxA.extentY + boxB.extentY - (isBoxBOnTop ? boxBCenter.y - boxACenter.y : boxACenter.y - boxBCenter.y);
                overlapZ = boxA.extentZ + boxB.extentZ - (isBoxBBehind ? boxBCenter.z - boxACenter.z : boxACenter.z - boxBCenter.z);

                double smallestOverlap = MathUtil.min3(overlapX, overlapY, overlapZ);

                if(overlapX == smallestOverlap)
                {
                    overlap = overlapX;
                    direction = new Vector3D(isBoxBOnRight ? 1D : -1D, 0, 0);
                }
                else if (overlapY == smallestOverlap)
                {
                    overlap = overlapY;
                    direction = new Vector3D(0, isBoxBOnTop ? 1D : -1D, 0);
                }
                else if (overlapZ == smallestOverlap)
                {
                    overlap = overlapZ;
                    direction = new Vector3D(0, 0, isBoxBBehind ? 1D : -1D);
                }
                else
                {
                    overlap = 0;
                    direction = Vector3D.zero;
                }
                return true;
            }
            overlap = 0;
            direction = Vector3D.zero;
            return false;
        }

        #endregion aabbCollisions

        #region planeCollisions
        public static bool getOverlapAndDirection(PlaneCollider plane, SphereCollider sphere, out Vector3D direction, out double overlap)
        {
            overlap = -(PlaneCollider.vectorDistanceFromPlane(plane, sphere.pos) - sphere.radius);
            direction = plane.normal;
            return overlap > 0;
        }

        public static bool getOverlapAndDirection(PlaneCollider plane, AABBCollider aabb, out Vector3D direction, out double overlap)
        {
            double radiusOfTesterSphere = 0;
            //Check if the plane aligns with two of any axis
            if (plane.normal.y >= 1D || plane.normal.y <= -1D)
            {
                radiusOfTesterSphere = aabb.extentY;//if the plane aligns with a given axis, then the sphere will have the radius of the aabb extent of that axis,so the spheres edge will be aligning with the "Face" of the aabb
            }
            else if (plane.normal.x >= 1D || plane.normal.x <= -1D)
            {
                radiusOfTesterSphere = aabb.extentX;
            }
            else if (plane.normal.z >= 1D || plane.normal.z <= -1D)
            {
                radiusOfTesterSphere = aabb.extentZ;
            }
            else
            {
                radiusOfTesterSphere = MathUtil.max6(
                    Vector3D.dot(aabb.vecToBackRight, plane.normal), Vector3D.dot(aabb.vecToBackLeft, plane.normal), Vector3D.dot(aabb.vecToFrontRight, plane.normal),
                    Vector3D.dot(-aabb.vecToBackRight, plane.normal), Vector3D.dot(-aabb.vecToBackLeft, plane.normal), Vector3D.dot(-aabb.vecToFrontRight, plane.normal));
            }
            return getOverlapAndDirection(plane, new SphereCollider(aabb.centerVec, radiusOfTesterSphere), out direction, out overlap);
        }

        public static bool getOverlapAndDirection(PlaneCollider plane, PointCollider point, out Vector3D direction, out double overlap)//this func can return a negative overlap, no issue. Just means there is no collision.
        {
            overlap = -PlaneCollider.vectorDistanceFromPlane(plane, point.pos);
            direction = plane.normal;
            return overlap > 0;
        }


        #endregion planeCollisions
    }
}
