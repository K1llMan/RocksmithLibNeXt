using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RocksmithLibNeXt.Formats.Sng.Common
{
    // Curiously Repeating Template Pattern
    /// <summary>
    /// Base collection
    /// </summary>
    /// <typeparam name="T">Collection type</typeparam>
    /// <typeparam name="IT">Collection item type</typeparam>
    public class SngCollection<T, IT> : IList<IT> where T: SngCollection<T, IT>,  new()
    {
        #region Fields

        private List<IT> items = new();

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Auxiliary functions

        private static MethodInfo GetObjectReader()
        {
            MethodInfo method = typeof(IT).GetMethod("Read");

            return method;
        }

        #endregion Auxiliary functions

        #region Main functions

        public static T Read(BinaryReader r)
        {
            if (r == null)
                return null;

            try {
                int count = r.ReadInt32();

                MethodInfo reader = GetObjectReader();
                if (reader == null)
                    throw new Exception($"Type \"{typeof(IT).FullName}\" does not contains method \"Read\".");

                List<IT> list = new();
                for (int i = 0; i < count; i++) 
                    list.Add((IT)reader.Invoke(null, new object[] { r }));

                return new T {
                    items = list
                };
            }
            catch (Exception ex) {
                throw new Exception($"Error reading collection \"{typeof(T).FullName}\": {ex}.");
            }
        }

        #endregion Main functions

        #region IList

        public void Add(IT item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(IT item)
        {
            return items.Contains(item);
        }

        public void CopyTo(IT[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(IT item)
        {
            return items.Remove(item);
        }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public int IndexOf(IT item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, IT item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public IT this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        #endregion IList

        #region IEnumerable

        public IEnumerator<IT> GetEnumerator()
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
