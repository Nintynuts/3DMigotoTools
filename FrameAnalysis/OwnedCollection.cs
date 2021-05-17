﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Migoto.Log.Parser
{
    internal class OwnedCollection<TOwner, TItem> : ICollection<TItem>
        where TOwner : class
        where TItem : IOwned<TOwner>
    {
        private readonly List<TItem> items = new List<TItem>();

        public OwnedCollection(TOwner owner)
        {
            Owner = owner;
        }

        protected TOwner Owner { get; }

        public virtual void Add(TItem item)
        {
            item.SetOwner(Owner);
            items.Add(item);
        }

        public void Clear()
        {
            items.ForEach(i => i.SetOwner(null));
            items.Clear();
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            array.ForEach(i => i.SetOwner(Owner));
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TItem item)
        {
            item.SetOwner(null);
            return items.Remove(item);
        }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public bool Contains(TItem item) => items.Contains(item);

        public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
