using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RocksmithLibNeXt.Formats.Sng.Common
{
    /// <summary>
    /// Base collection
    /// </summary>
    public class SngCollection<T> : IList<T> where T : new()
    {
        #region Fields

        private List<T> items = new();

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Auxiliary functions

        private static MethodInfo GetObjectReader()
        {
            MethodInfo method = typeof(T).GetMethod("Read");

            return method;
        }

        private static MethodInfo GetObjectWriter()
        {
            MethodInfo method = typeof(T).GetMethod("Write");

            return method;
        }

        #endregion Auxiliary functions

        #region Main functions

        public static SngCollection<T> Read(BinaryReader r)
        {
            if (r == null)
                return null;

            try {
                int count = r.ReadInt32();

                MethodInfo reader = GetObjectReader();
                if (reader == null)
                    throw new Exception($"Type \"{typeof(T).FullName}\" does not contains method \"Read\".");

                List<T> list = new();
                for (int i = 0; i < count; i++) 
                    list.Add((T)reader.Invoke(null, new object[] { r }));

                return new SngCollection<T> {
                    items = list
                };
            }
            catch (Exception ex) {
                throw new Exception($"Error reading collection \"{typeof(T).FullName}\": {ex}.");
            }
        }

        public void Write(BinaryWriter w)
        {
            if (w == null)
                return;

            try
            {
                w.Write(items.Count);

                MethodInfo writer = GetObjectWriter();
                if (writer == null)
                    throw new Exception($"Type \"{typeof(T).FullName}\" does not contains method \"Write\".");

                foreach (T item in items)
                    writer.Invoke(item, new object[] { w });
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing collection \"{typeof(T).FullName}\": {ex}.");
            }
        }

        #endregion Main functions

        #region IList

        public void Add(T item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public T this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        #endregion IList

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) items).GetEnumerator();
        }

        #endregion IEnumerable
    }
}
