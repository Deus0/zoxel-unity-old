using Unity.Entities;
using System.Collections.Generic;
using Zoxel.UI;
using Unity.Collections;

namespace Zoxel
{
    /// <summary>
    /// used to cycle through any entities and destroy them and their descendants!
    /// </summary>
    public struct Childrens : IComponentData
    {
        public BlitableArray<Entity> children;

        public Entity[] GetButtons(EntityManager EntityManager)
        {
            //List<Entity> entities = new List<Entity>();
            return GetButtonsLoop(EntityManager, this).ToArray();
        }

        private List<Entity> GetButtonsLoop(EntityManager EntityManager, Childrens moreChildren)
        {
            List<Entity> entities = new List<Entity>();
            for (int i = 0; i < moreChildren.children.Length; i++)
            {
                if (EntityManager.Exists(moreChildren.children[i]))
                {
                    if (EntityManager.HasComponent<Button>(moreChildren.children[i]))
                    {
                        entities.Add(moreChildren.children[i]);
                    }
                    if (EntityManager.HasComponent<Childrens>(moreChildren.children[i]))
                    {
                        Childrens childrensChildren = EntityManager.GetComponentData<Childrens>(moreChildren.children[i]);
                        entities.AddRange(GetButtonsLoop(EntityManager, childrensChildren));
                    }
                }
            }
            return entities;
        }

        public void DestroyEntities(EntityManager EntityManager)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (EntityManager.Exists(children[i]))
                {
                    if (EntityManager.HasComponent<Childrens>(children[i]))
                    {
                        Childrens childrensChildren = EntityManager.GetComponentData<Childrens>(children[i]);
                        childrensChildren.DestroyEntities(EntityManager);
                    }
                    if (EntityManager.HasComponent<RenderText>(children[i]))
                    {
                        RenderText text = EntityManager.GetComponentData<RenderText>(children[i]);
                        text.DestroyLetters(EntityManager);
                    }
                    EntityManager.DestroyEntity(children[i]);
                }
            }
            if (children.Length > 0)
            {
                children.Dispose();
            }
        }

        private void DestroChildAtIndex(EntityManager EntityManager, int index)
        {
            if (EntityManager.Exists(children[index]))
            {
                if (EntityManager.HasComponent<Childrens>(children[index]))
                {
                    Childrens childrensChildren = EntityManager.GetComponentData<Childrens>(children[index]);
                    childrensChildren.DestroyEntities(EntityManager);
                }
                if (EntityManager.HasComponent<RenderText>(children[index]))
                {
                    RenderText text = EntityManager.GetComponentData<RenderText>(children[index]);
                    text.DestroyLetters(EntityManager);
                }
                EntityManager.DestroyEntity(children[index]);
            }
        }

        public void DestroyEntity(EntityManager EntityManager, int index)
        {
            DestroChildAtIndex(EntityManager,index);
            var children_ = children.ToArray();
            children = new BlitableArray<Entity>(children.Length - 1, Allocator.Persistent);
            for (int i = 0; i < children.Length; i++) {
                if (i < index) {
                    children[i] = children_[i];
                }
                else {
                    children[i] = children_[i + 1];
                }
            }
        }
    }
}