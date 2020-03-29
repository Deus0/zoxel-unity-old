using Unity.Entities;
using System.Collections.Generic;
using Zoxel.WorldGeneration;
using UnityEngine;
using Zoxel.UI;

namespace Zoxel
{
    /// <summary>
    /// Systems made for game modes and rules
    ///     - Includes startup rules etc
    /// </summary>
    public class GameSystemGroup : ComponentSystemGroup
    {
        private WaveModeSystem waveSystem;
        public GameStartSystem gameStartSystem;
        public GameEndSystem endGameSystem;
        public SaveSystem saveSystem;

        public void Clear()
        {
            waveSystem.Clear();
        }

        public void SetMeta(GameDatam data)
        {
            gameStartSystem.startCamera = data.startingCamera;
            gameStartSystem.startMap = data.startingMap;
            gameStartSystem.startCharacter = data.startingCharacter;
            gameStartSystem.mainMenuMap = data.mainMenuMap;
            // gameStartSystem.boss = this;
            gameStartSystem.meta = new Dictionary<int, GameDatam>();
            gameStartSystem.meta.Add(data.id, data);
        }

        public void Initialize(World space)
        {
            gameStartSystem = space.GetOrCreateSystem<GameStartSystem>();
            AddSystemToUpdateList(gameStartSystem);
            endGameSystem = space.GetOrCreateSystem<GameEndSystem>();
            AddSystemToUpdateList(endGameSystem);
            saveSystem = space.GetOrCreateSystem<SaveSystem>();
            AddSystemToUpdateList(saveSystem);
            waveSystem = space.GetOrCreateSystem<WaveModeSystem>();
            AddSystemToUpdateList(waveSystem);
            SetLinks();
        }

        void SetLinks()
        {
            endGameSystem.waveSystem = waveSystem;
            gameStartSystem.gameEndSystem = endGameSystem;
        }

        public void CombineWithPlayers(PlayerSystemGroup playerSystemGroup)
        {
            gameStartSystem.playerSpawnSystem = playerSystemGroup.playerSpawnSystem;
            gameStartSystem.playerSkillsSystem = playerSystemGroup.playerSkillsSystem;
            gameStartSystem.playerControllerSystem = playerSystemGroup.playerControllerSystem;
            saveSystem.playerSpawnSystem = playerSystemGroup.playerSpawnSystem;
        }
        public void CombineWithVoxels(Voxels.VoxelSystemGroup voxelSystemGroup)
        {
            gameStartSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
            waveSystem.worldSpawnSystem = voxelSystemGroup.worldSpawnSystem;
        }

        public void CombineWithCharacters(CharacterSystemGroup characterSystemGroup)
        {
            gameStartSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            waveSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
            endGameSystem.characterSpawnSystem = characterSystemGroup.characterSpawnSystem;
        }
        public void CombineWithCameras(CameraSystemGroup cameraSystemGroup)
        {
            gameStartSystem.cameraSystem = cameraSystemGroup.cameraSystem;
            gameStartSystem.cameraFirstPersonSystem = cameraSystemGroup.cameraFirstPersonSystem;
        }
        public void CombineWithUI(MenuSpawnSystem menuSpawnSystem, GameUISystem gameUISystem)
        {
            gameStartSystem.menuSpawnSystem = menuSpawnSystem;
            gameStartSystem.gameUISystem = gameUISystem;
        }

        public void CombinWithMovement(MovementSystemGroup movementSystemGroup)
        {
            gameStartSystem.movementSystemGroup = movementSystemGroup;
        }
    }
}
