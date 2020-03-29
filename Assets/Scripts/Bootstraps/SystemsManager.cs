using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Entities;
using Unity.Transforms;
using Zoxel.Animations;
using Zoxel.Voxels;
using Zoxel.WorldGeneration;
using Unity.Rendering;
using Zoxel.UI;

namespace Zoxel
{

    public class SystemsManager
    {
        private GameDatam data;
        public Unity.Entities.World space;
        private AISystemGroup aiSystemGroup;
        private AnimationSystemGroup animationSystemGroup;
        public BulletSystemGroup bulletSystemGroup;
        public CameraSystemGroup cameraSystemGroup;
        public CharacterSystemGroup characterSystemGroup;
        public GameSystemGroup gameSystemGroup;
        public ItemSystemGroup itemSystemGroup;
        public MovementSystemGroup movementSystemGroup;
        public PlayerSystemGroup playerSystemGroup;
        public SkillSystemGroup skillSystemGroup;
        private StatSystemGroup statSystemGroup;
        public UISystemGroup uiSystemGroup;
        public VoxelSystemGroup voxelSystemGroup;
        public WorldSystemGroup worldSystemGroup;

        public SystemsManager(GameDatam data_, string worldName = "Zoxel")
        {
            space = CreateWorld(GetTypes(), worldName);
            data = data_;
            // Core
            CreateSystemGroups();
            //SetSystems(false);
            SetData(data);
            SetSystems(true);
        }

        public static Unity.Entities.World CreateWorld(List<Type> types, string worldName)
        {
            //var space = new Unity.Entities.World(worldName);
            var space = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            //DefaultWorldInitialization.Initialize(worldName, false); // new Unity.Entities.World(worldName);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(space, GetTypes());
            Unity.Entities.World.DefaultGameObjectInjectionWorld = space;
            if (Application.isPlaying)
            {
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(space);
            }
            return space;
        }

        public static Unity.Entities.World CreateEditorWorld(List<Type> types, string worldName)
        {
            var space = new Unity.Entities.World(worldName);
            //DefaultWorldInitialization.Initialize(worldName, false); // new Unity.Entities.World(worldName);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(space, GetTypes());
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(space, GetUnityTypes());
            //Unity.Entities.World.DefaultGameObjectInjectionWorld = space;
            return space;
        }
        

        public void Dispose()
        {
            if (space != null)
            {
                try
                {
                    space.Dispose();
                }
                catch (ArgumentException e)
                {
                    Debug.LogWarning(e.ToString());
                }
                space = null;
            }
        }

        public void ClearGame()
        {
            gameSystemGroup.Clear();
            cameraSystemGroup.Clear();
            characterSystemGroup.Clear();
            bulletSystemGroup.Clear();
            voxelSystemGroup.Clear();
            itemSystemGroup.Clear();
            uiSystemGroup.Clear();
        }

        public void SetData(GameDatam newData)
        {
            data = newData;
            if (data == null)
            {
                Debug.LogError("Data in systems is null.");
                return;
            }
            InjectEditorData();
            InduceMoreThenNecessaryCouplingOfSystems();
        }

        public void SetSystems(bool state)
        {
            movementSystemGroup.Enabled = state;
            cameraSystemGroup.Enabled = state;
            voxelSystemGroup.Enabled = state;
            animationSystemGroup.Enabled = state;
            aiSystemGroup.Enabled = state;
            gameSystemGroup.Enabled = state;
            uiSystemGroup.Enabled = state;

            worldSystemGroup.Enabled = state;
            // gameish
            playerSystemGroup.Enabled = state;
            characterSystemGroup.Enabled = state;
            // game non core
            skillSystemGroup.Enabled = state;
            statSystemGroup.Enabled = state;
            bulletSystemGroup.Enabled = state;
            itemSystemGroup.Enabled = state;
        }

        public static List<Type> GetTypes()
        {
            List<Type> types = new List<Type>();
            /*
            types.AddRange(GetUnityTypes());*/

            // core
            types.Add(typeof(VoxelSystemGroup));
            types.Add(typeof(WorldSystemGroup));

            types.Add(typeof(MovementSystemGroup));
            types.Add(typeof(CameraSystemGroup));
            types.Add(typeof(UISystemGroup));
            types.Add(typeof(AnimationSystemGroup));
            // gameish
            types.Add(typeof(GameSystemGroup));
            types.Add(typeof(AISystemGroup));
            types.Add(typeof(PlayerSystemGroup));
            types.Add(typeof(CharacterSystemGroup));
            // game non core
            types.Add(typeof(SkillSystemGroup));
            types.Add(typeof(StatSystemGroup));
            types.Add(typeof(BulletSystemGroup));
            types.Add(typeof(ItemSystemGroup));
            return types;
        }

        #region TransformSystems
        /*EndFrameParentSystem endFrameParentSystem;
        EndFrameParentScaleInverseSystem endFrameParentScaleInverseSystem;
        EndFrameTRSToLocalToParentSystem endFrameTRSToLocalToParentSystem;
        EndFrameTRSToLocalToWorldSystem endFrameTRSToLocalToWorldSystem;
        EndFrameLocalToParentSystem endFrameLocalToParentSystem;
        CopyTransformToGameObjectSystem copyTransformToGameObjectSystem;
        LocalToParentSystem localToParentSystem;
        ParentSystem parentSystem;
        TRSToLocalToParentSystem trsToLocalToParentSystem;
        TRSToLocalToWorldSystem trsToLocalToWorldSystem;
        ParentScaleInverseSystem parentScaleInverseSystem;
        //TransformSystemGroup transformSystemGroup;*/

        public static List<Type> GetUnityTypes()
        {
            List<Type> types = new List<Type>();
            //types.Add(typeof(InitializationSystemGroup));
            //types.Add(typeof(SimulationSystemGroup));
            types.Add(typeof(TransformSystemGroup));

            types.Add(typeof(EndFrameParentSystem));
            types.Add(typeof(EndFrameLocalToParentSystem));
            types.Add(typeof(EndFrameTRSToLocalToParentSystem));
            types.Add(typeof(EndFrameTRSToLocalToWorldSystem));

            types.Add(typeof(PresentationSystemGroup));
            types.Add(typeof(RenderMeshSystemV2));
            //types.Add(typeof(Unity.Rendering.));
            types.Add(typeof(Zoxel.RenderBoundsUpdateSystem));
            //types.Add(typeof(CreateMissingRenderBoundsSystem));
            return types;
        }
        #endregion


        private void CreateSystemGroups()
        {
            worldSystemGroup = space.GetOrCreateSystem<WorldSystemGroup>();
            worldSystemGroup.Initialize(space);
            voxelSystemGroup = space.GetOrCreateSystem<VoxelSystemGroup>();
            voxelSystemGroup.Initialize(space);
            movementSystemGroup = space.GetOrCreateSystem<MovementSystemGroup>();
            movementSystemGroup.Initialize(space);
            uiSystemGroup = space.GetOrCreateSystem<UISystemGroup>();
            uiSystemGroup.Initialize(space);
            playerSystemGroup = space.GetOrCreateSystem<PlayerSystemGroup>();
            playerSystemGroup.Initialize(space);
            gameSystemGroup = space.GetOrCreateSystem<GameSystemGroup>();
            gameSystemGroup.Initialize(space);
            cameraSystemGroup = space.GetOrCreateSystem<CameraSystemGroup>();
            cameraSystemGroup.Initialize(space);
            bulletSystemGroup = space.GetOrCreateSystem<BulletSystemGroup>();
            bulletSystemGroup.Initialize(space);
            characterSystemGroup = space.GetOrCreateSystem<CharacterSystemGroup>();
            characterSystemGroup.Initialize(space);
            skillSystemGroup = space.GetOrCreateSystem<SkillSystemGroup>();
            skillSystemGroup.Initialize(space);
            statSystemGroup = space.GetOrCreateSystem<StatSystemGroup>();
            statSystemGroup.Initialize(space);
            itemSystemGroup = space.GetOrCreateSystem<ItemSystemGroup>();
            itemSystemGroup.Initialize(space);
            animationSystemGroup = space.GetOrCreateSystem<AnimationSystemGroup>();
            animationSystemGroup.Initialize(space);
            aiSystemGroup = space.GetOrCreateSystem<AISystemGroup>();
            aiSystemGroup.Initialize(space);
            
            //var presentationGroup = space.GetOrCreateSystem<PresentationSystemGroup>();
            //var renderBoundsUpdateSystem = space.GetOrCreateSystem<Zoxel.RenderBoundsUpdateSystem>();
            //presentationGroup.AddSystemToUpdateList(renderBoundsUpdateSystem);
            //var renderBoundsUpdateSystem = space.GetOrCreateSystem<CreateMissingRenderBoundsFromMeshRenderer>();
            //presentationGroup.AddSystemToUpdateList(createMissingRenderBoundsFromMeshRenderer);
        }
        private void InjectEditorData()
        {
            // static olds
            TurretSpawnerSystem.turretData = data.turrets;
            WaveModeSystem.waveData = data.waves;
            // new 
            bulletSystemGroup.SetMeta(data);
            itemSystemGroup.SetMeta(data);
            skillSystemGroup.SetMeta(data);
            characterSystemGroup.SetMeta(data);
            cameraSystemGroup.SetMeta(data);
            gameSystemGroup.SetMeta(data);
            voxelSystemGroup.SetMeta(data);
            uiSystemGroup.SetMeta(data);
        }

        /// <summary>
        /// Remove this eventually and have no coupling
        /// </summary>
        private void InduceMoreThenNecessaryCouplingOfSystems()
        {
            movementSystemGroup.CombineWithVoxels(voxelSystemGroup);
            // Voxels
            voxelSystemGroup.characterRaycastSystem.cameraSystem = cameraSystemGroup.cameraSystem;
            voxelSystemGroup.chunkSpawnSystem.characterDeathSystem = characterSystemGroup.characterDeathSystem;
            voxelSystemGroup.CombineWithCameras(cameraSystemGroup);
            // World
            worldSystemGroup.CombineWithCharacters(characterSystemGroup);
            worldSystemGroup.CombineWithVoxels(voxelSystemGroup);
            worldSystemGroup.CombineWithCameras(cameraSystemGroup);
            // cameras
            cameraSystemGroup.CombineWithCharacters(characterSystemGroup);
            cameraSystemGroup.CombineWithPlayers(playerSystemGroup);
            // bullets
            bulletSystemGroup.CombineWithCharacters(characterSystemGroup);
            bulletSystemGroup.CombineWithSkills(skillSystemGroup);
            // items
            itemSystemGroup.CombineWithAnimation(animationSystemGroup);
            itemSystemGroup.CombineWithUI(uiSystemGroup);
            // skills
            skillSystemGroup.CombineWithCharacters(characterSystemGroup);
            skillSystemGroup.CombineWithUI(uiSystemGroup);
            skillSystemGroup.CombineWithBullets(bulletSystemGroup);
            // Stats
            statSystemGroup.CombineWithUI(uiSystemGroup);
            // Players
            playerSystemGroup.CombineWithUI(uiSystemGroup);
            playerSystemGroup.CombineWithGame(gameSystemGroup);
            // UI
            uiSystemGroup.CombineWithCharacters(characterSystemGroup);
            uiSystemGroup.CombineWithVoxels(voxelSystemGroup);
            uiSystemGroup.CombineWithCameras(cameraSystemGroup);
            uiSystemGroup.CombineWithPlayers(playerSystemGroup);
            uiSystemGroup.CombineWithGame(gameSystemGroup);
            // AI
            aiSystemGroup.CombineWithCharacters(characterSystemGroup);
            // characters
            characterSystemGroup.CombineWithSkills(skillSystemGroup);
            characterSystemGroup.CombineWithUI(uiSystemGroup);
            characterSystemGroup.CombineWithVoxels(voxelSystemGroup);
            characterSystemGroup.CombineWithItems(itemSystemGroup);
            characterSystemGroup.CombineWithPlayers(playerSystemGroup);
            characterSystemGroup.CombineWithCameras(cameraSystemGroup);
            characterSystemGroup.CombineWithGame(gameSystemGroup);
            // Game
            gameSystemGroup.CombineWithPlayers(playerSystemGroup);
            gameSystemGroup.CombineWithVoxels(voxelSystemGroup);
            gameSystemGroup.CombineWithCharacters(characterSystemGroup);
            gameSystemGroup.CombineWithCameras(cameraSystemGroup);
            gameSystemGroup.CombineWithUI(uiSystemGroup.menuSpawnSystem, playerSystemGroup.gameUISystem);
            gameSystemGroup.CombinWithMovement(movementSystemGroup);
            gameSystemGroup.gameStartSystem.boss = this;    // for clearing gmames
        }

    }
}