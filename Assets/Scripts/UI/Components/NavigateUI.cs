using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Core;
using UnityEngine;
using Unity.Collections;

namespace Zoxel
{
    public enum NavigationUIDirection
    {
        Right,
        Left,
        Up,
        Down
    }
    // next move between skillbar and inventory

    [System.Serializable]
    public struct NavigateUIElement
    {
        public Entity entity;
        public float3 previousPosition;
        public int previousIndex;          // UISpawnSystem index in the UI
        public byte direction;          // direction that goes to next position
        public float3 targetPosition;
        public int targetIndex;    // BlitableArray<NavigateUIElement> navigationElements index 
        public Entity ui;
    }

    [System.Serializable]
    public struct NavigateUI : IComponentData
    {
        // selected
        public Entity selected;
        public int selectedIndex;
        public float3 position;

        // meta
        public Entity character;
        public BlitableArray<NavigateUIElement> navigationElements;

        // state / actions
        //public byte lastUIIndex;
        public float lastMoved;
        public byte clicked;
        public byte updated;

        public void Initialize(List<Entity> entities, List<float3> positions, List<Entity> newParents)
        {
            // should find closest in certain directions
            // if nothing in that direction dont add that direction
            // create navigation based no positions
            // top left is start of navigation
            List<NavigateUIElement> newNavigationElements = new List<NavigateUIElement>();
            if (positions.Count == 1)
            {
                NavigateUIElement element = new NavigateUIElement();
                //element.previousPosition = positions[0];
                element.targetPosition = positions[0];
                element.direction = 0;
                element.ui = newParents[0];
                //element.arrayIndex = 0;
                element.entity = entities[0];
                newNavigationElements.Add(element);
            }
            else
            {
                for (int i = 0; i < positions.Count; i++)
                {
                    // up
                    newNavigationElements = AddNavigationUI(entities, newParents, newNavigationElements, positions,
                        positions[i], i, 2, 10000);
                    // down
                    newNavigationElements = AddNavigationUI(entities, newParents, newNavigationElements, positions,
                        positions[i], i, 3, -10000);
                    // left
                    newNavigationElements = AddNavigationUI(entities, newParents, newNavigationElements, positions,
                        positions[i], i, 1, -10000);
                    // right
                    newNavigationElements = AddNavigationUI(entities, newParents, newNavigationElements, positions,
                        positions[i], i, 0, 10000);
                }

                /*for (int i = 0; i < newNavigationElements.Count; i++)
                {
                    float3 nextPosition = newNavigationElements[i].nextPosition;
                    bool hasNextNode = false;
                    for (int j = 0; j < newNavigationElements.Count; j++)
                    {
                        if (nextPosition.x == newNavigationElements[j].position.x
                        && nextPosition.y == newNavigationElements[j].position.y
                        && nextPosition.z == newNavigationElements[j].position.z)
                        {
                            hasNextNode = true;
                            break;
                        }
                    }
                    if (!hasNextNode)
                    {
                        Debug.LogError("Node: " + newNavigationElements[i].position + " has no next node.");
                        // add node with next position
                        NavigateUIElement element = new NavigateUIElement();
                        element.thisPositionIndex = newNavigationElements[i].nextPositionIndex;
                        element.position = newNavigationElements[i].nextPosition;
                        element.uiIndex = newNavigationElements[i].uiIndex;
                        //element.nextPosition = positions[closestIndex];
                        element.nextPositionIndex = -1;
                        newNavigationElements.Add(element);
                        //element.direction = newDirection;
                    }
                }*/
            }
            NavigateUIElement[] originalNavigationElements = navigationElements.ToArray();
            navigationElements = new BlitableArray<NavigateUIElement>(originalNavigationElements.Length + newNavigationElements.Count, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < originalNavigationElements.Length; i++)
            {
                navigationElements[i] = originalNavigationElements[i];
            }
            for (int i = 0; i < newNavigationElements.Count; i++)
            {
                navigationElements[i + originalNavigationElements.Length] = newNavigationElements[i];
            }
            //UnityEngine.Debug.LogError("Initialized " + navigationElements.Length + " with navigateUIElement.");
        }

        // Fix: if x is different then it doesnt work
       List<NavigateUIElement> AddNavigationUI(List<Entity> entities, List<Entity> parents, List<NavigateUIElement> newNavigationElements,
            List<float3> positions, float3 position, int i, byte newDirection, float closest)
        {
            int closestIndex = -1;
            float closestOther = 0.1f;// 10000;
            for (int j = 0; j < positions.Count; j++)
            {
                if (i != j)
                {
                    // 
                    if (newDirection == 3 && positions[j].y < position.y && positions[j].y >= closest)
                    {
                        if (positions[j].y != closest || (
                            positions[j].y == closest && closestOther > math.abs(positions[j].x - position.x)))
                        {
                            closestOther = math.abs(positions[j].x - position.x);
                            closest = positions[j].y;
                            closestIndex = j;
                        }
                    }
                    else if (newDirection == 2 && positions[j].y > position.y && positions[j].y <= closest)
                    {
                        if (positions[j].y != closest || (
                            positions[j].y == closest && closestOther > math.abs(positions[j].x - position.x)))
                        {
                            closestOther = math.abs(positions[j].x - position.x);
                            closest = positions[j].y;
                            closestIndex = j;
                        }
                    }
                    // left
                    else if (newDirection == 1 && positions[j].x < position.x && positions[j].x >= closest)
                    {
                        float differentY = math.abs(positions[j].y - position.y);
                        if ((positions[j].x != closest && differentY < 0.05f) ||
                            (positions[j].x == closest && closestOther > differentY))
                        {
                            closestOther = differentY;
                            closest = positions[j].x;
                            closestIndex = j;
                        }
                    }
                    // right
                    else if (newDirection == 0 && positions[j].x > position.x && positions[j].x <= closest)
                    {
                        float differentY = math.abs(positions[j].y - position.y);
                        if ((positions[j].x != closest && differentY < 0.05f) ||
                            (positions[j].x == closest && closestOther > differentY))
                        {
                            closestOther = differentY;
                            closest = positions[j].x;
                            closestIndex = j;
                        }
                    }
                }
            }
            if (closestIndex != -1)
            {
                //Debug.LogError("Adding Closest Index NavigationUIElement: " + closestIndex + " : " + positions[i]);
                NavigateUIElement element = new NavigateUIElement();
                element.previousIndex = i;
                element.previousPosition = positions[i];
                element.entity = entities[closestIndex];
                element.targetIndex = closestIndex;
                element.targetPosition = positions[closestIndex];
                element.direction = newDirection;
                element.ui = parents[closestIndex];
                newNavigationElements.Add(element);
            }
            return newNavigationElements;
        }

        public void SelectFirst(EntityManager entityManager, Entity firstEntity) //, Entity selectionIcon)
        {
            if (navigationElements.Length > 0)
            {
                for (int i = 0; i < navigationElements.Length; i++)
                {
                    var navi = navigationElements[i];
                    //if (navi.targetIndex == 0)
                    if (firstEntity == navi.entity)
                    {
                        /*Debug.LogError("Setting First Navigation UI to: " + i +"\n" +
                            "navi.direction: " + ((NavigationUIDirection)navi.direction) + "\n" +
                            "navi.previousIndex: " + navi.previousIndex + "\n" +
                            "navi.targetIndex: " + navi.targetIndex);*/
                        selectedIndex = i;
                        position = navi.targetPosition;
                        //lastUIIndex = navi.ui;
                        SetSelected(entityManager, navi.entity);
                        break;
                    }
                }
            }
        }
        public void SetSelected(EntityManager entityManager, Entity entity)
        {

            if (entityManager.Exists(selected) && entityManager.HasComponent<NavigationElementUI>(selected))
            {
                NavigationElementUI oldSelected = entityManager.GetComponentData<NavigationElementUI>(selected);
                oldSelected.Deselect(UnityEngine.Time.realtimeSinceStartup);
                entityManager.SetComponentData(selected, oldSelected);
            }
            selected = entity;
            if (entityManager.Exists(selected) && entityManager.HasComponent<NavigationElementUI>(selected))
            {
                NavigationElementUI newSelected = entityManager.GetComponentData<NavigationElementUI>(entity);
                newSelected.Select(UnityEngine.Time.realtimeSinceStartup);
                entityManager.SetComponentData(entity, newSelected);
            }
        }
    }

}
