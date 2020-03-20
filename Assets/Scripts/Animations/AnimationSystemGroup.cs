using Unity.Entities;
using Zoxel.Animations;

namespace Zoxel
{
    public class AnimationSystemGroup : ComponentSystemGroup
    {
        Zoxel.Animations.AnimatorSystem animatorSystem;
        AnimatorEndSystem animatorEndSystem;
        ShrinkSystem2 shrinkSystem2;
        ShrinkSystem3 shrinkSystem3;
        LerpPositionSystem lerpPositionSystem;
        LerpScaleSystem lerpScaleSystem;
        public DoomedToDieSystem doomedToDieSystem;
        LerpPositionEntitySystem lerpPositionEntitySystem;
        SinRotatorSystem sinRotatorSystem;

        public void Initialize(Unity.Entities.World space)
        {
            sinRotatorSystem = space.CreateSystem<SinRotatorSystem>();
            AddSystemToUpdateList(sinRotatorSystem);

            animatorSystem = space.GetOrCreateSystem<Zoxel.Animations.AnimatorSystem>();
            animatorEndSystem = space.GetOrCreateSystem<Zoxel.Animations.AnimatorEndSystem>();
            shrinkSystem2 = space.GetOrCreateSystem<ShrinkSystem2>();
            shrinkSystem3 = space.GetOrCreateSystem<ShrinkSystem3>();
            lerpPositionSystem = space.GetOrCreateSystem<LerpPositionSystem>();
            lerpScaleSystem = space.GetOrCreateSystem<LerpScaleSystem>();
            lerpPositionEntitySystem = space.GetOrCreateSystem<LerpPositionEntitySystem>();
            doomedToDieSystem = space.GetOrCreateSystem<DoomedToDieSystem>();
            AddSystemToUpdateList(animatorSystem);
            AddSystemToUpdateList(animatorEndSystem);
            AddSystemToUpdateList(shrinkSystem2);
            AddSystemToUpdateList(shrinkSystem3);
            AddSystemToUpdateList(lerpPositionSystem);
            AddSystemToUpdateList(lerpScaleSystem);
            AddSystemToUpdateList(lerpPositionEntitySystem);
            AddSystemToUpdateList(doomedToDieSystem);

            SetLinks();
        }
        void SetLinks()
        {

        }

        public void Clear()
        {

        }
    }
}
