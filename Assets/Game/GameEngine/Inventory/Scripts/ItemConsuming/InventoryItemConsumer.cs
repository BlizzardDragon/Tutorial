using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Game.GameEngine.InventorySystem
{
    public class InventoryItemConsumer
    {
        public event Action<InventoryItem> OnItemConsumed;

        private StackableInventory inventory;
        private readonly List<IInventoryItemConsumeHandler> handlers = new();

        public InventoryItemConsumer(StackableInventory inventory)
        {
            this.inventory = inventory;
        }

        public InventoryItemConsumer()
        {
        }

        public void SetInventory(StackableInventory inventory)
        {
            this.inventory = inventory;
        }

        public void AddHandler(IInventoryItemConsumeHandler handler)
        {
            this.handlers.Add(handler);
        }

        public void RemoveHandler(IInventoryItemConsumeHandler handler)
        {
            this.handlers.Remove(handler);
        }

        [Button]
        [GUIColor(0, 1, 0)]
        public bool CanConsumeItem(InventoryItem item)
        {
            return item.FlagsExists(InventoryItemFlags.CONSUMABLE) &&
                   this.inventory.IsItemExists(item);
        }

        [Button]
        [GUIColor(0, 1, 0)]
        public void ConsumeItem(InventoryItem item)
        {
            if (!this.CanConsumeItem(item))
            {
                throw new Exception($"Can not consume item {item.Name}!");
            }

            this.inventory.RemoveItem(item);

            for (int i = 0, count = this.handlers.Count; i < count; i++)
            {
                var handler = this.handlers[i];
                handler.OnConsume(item);
            }

            this.OnItemConsumed?.Invoke(item);
        }
    }
}