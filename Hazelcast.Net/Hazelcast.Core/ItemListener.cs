using System;

namespace Hazelcast.Core
{
    public class ItemListener<T> : IItemListener<T>
    {
        public Action<ItemEvent<T>> OnItemAdded { get; set; } 
        public Action<ItemEvent<T>> OnItemRemoved { get; set; } 

        public void ItemAdded(ItemEvent<T> item)
        {
            if (OnItemAdded != null)
            {
                OnItemAdded(item);
            }
        }

        public void ItemRemoved(ItemEvent<T> item)
        {
            if (OnItemRemoved != null)
            {
                OnItemRemoved(item);
            }
        }
    }
}
