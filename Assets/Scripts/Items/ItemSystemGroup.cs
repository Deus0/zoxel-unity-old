using Unity.Entities;
using Zoxel.UI;

namespace Zoxel
{
    public class ItemSystemGroup : ComponentSystemGroup
    {
        public ItemSpawnerSystem itemSpawnSystem;
        private ItemHitSystem itemHitSystem;
        private ItemHitCompleterSystem itemHitCompleterSystem;
        private ItemBobSystem itemBobSystem;
        private EquipmentSystem equipmentSystem;
        private ItemUISystem itemUISystem;

        public void Initialize(Unity.Entities.World space)
        {

            itemSpawnSystem = space.GetOrCreateSystem<ItemSpawnerSystem>();
            itemHitSystem = space.GetOrCreateSystem<ItemHitSystem>();
            itemHitCompleterSystem = space.GetOrCreateSystem<ItemHitCompleterSystem>();
            itemBobSystem = space.GetOrCreateSystem<ItemBobSystem>();
            AddSystemToUpdateList(itemSpawnSystem);
            AddSystemToUpdateList(itemHitSystem);
            AddSystemToUpdateList(itemHitCompleterSystem);
            AddSystemToUpdateList(itemBobSystem);
            equipmentSystem = space.GetOrCreateSystem<EquipmentSystem>();
            AddSystemToUpdateList(equipmentSystem);

            itemUISystem = space.GetOrCreateSystem<ItemUISystem>();
            AddSystemToUpdateList(itemUISystem);

            SetLinks();
        }

        void SetLinks()
        {
            itemHitSystem.itemSpawnSystem = itemSpawnSystem;
            itemHitCompleterSystem.itemSpawnSystem = itemSpawnSystem;
        }

        public void Clear()
        {
            itemSpawnSystem.Clear();
        }
        public void SetMeta(GameDatam data)
        {
            itemSpawnSystem.meta = data.GetItems();
            equipmentSystem.meta = itemSpawnSystem.meta;
            itemUISystem.meta = itemSpawnSystem.meta;
            itemUISystem.defaultIconTexture = data.uiData.defaultItemIcon;
        }
        public void CombineWithUI(UISystemGroup uiSystemGroup)
        {
            itemHitCompleterSystem.inventoryUISpawnSystem = uiSystemGroup.inventoryUISpawnSystem;
        }
        public void CombineWithAnimation(AnimationSystemGroup animationSystemGroup)
        {
            itemHitCompleterSystem.doomedToDieSystem = animationSystemGroup.doomedToDieSystem;
        }

    }
}
