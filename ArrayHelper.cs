using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NPCUnlimiter
{
    public static class ArrayHelper
    {
        public static HashSet<T> SetUnion<T>(params HashSet<T>[] sets) {
            var result = new HashSet<T>();
            foreach (var set in sets) {
                result.UnionWith(set);
            }
            return result;
        }

        public class ArrayDim: IEquatable<ArrayDim>
        {
            public int[] Dim;
            public ArrayDim(Array arr)
            {
                Dim = new int[arr.Rank];
                for (var i = 0; i < Dim.Length; ++i)
                {
                    Dim[i] = arr.GetLength(i);
                }
            }
            public ArrayDim(params int[] dim)
            {
                this.Dim = dim;
            }
            public object this[int index]
            {
                get => Dim[index];
            }
            public int Rank => Dim.Length;

            public bool Equals(ArrayDim other)
            {
                return Enumerable.SequenceEqual(Dim, other.Dim);
            }

            public int IndexCount()
            {
                return Dim.Aggregate(1, (a, b) => a * b);
            }
            public int[] IndexToIndices(int idx)
            {
                var result = new int[Dim.Length];
                for (var i = 0; i < result.Length; ++i)
                {
                    result[i] = idx % Dim[i];
                    idx /= Dim[i];
                }
                return result;
            }

            public static ArrayDim MinOverlap(params ArrayDim[] dims)
            {
                if (dims.Length == 0)
                    return new ArrayDim(new int[0]);

                var rank = dims[0].Rank;

                if (dims.Any(d => d.Rank != rank))
                    return new ArrayDim(new int[0]);

                var result = new int[rank];
                for (var i = 0; i < rank; ++i)
                {
                    result[i] = dims.Select(d => d.Dim[i]).Min();
                }

                return new ArrayDim(result);
            }
        }

        public static void ResizeFieldArrayMulti(FieldInfo field, object obj, ArrayDim newSizes)
        {
            // Suppose it's all fine no checks

            var arrElType = field.FieldType.GetElementType();
            var oldArray = (Array)field.GetValue(obj);
            var oldSizes = new ArrayDim(oldArray);

            if (oldSizes == newSizes)
                return;

            var newArray = Array.CreateInstance(arrElType, newSizes.Dim);

            var minSizes = ArrayDim.MinOverlap(oldSizes, newSizes);
            var limit = minSizes.IndexCount();

            for (var idx = 0; idx < limit; ++idx)
            {
                var currPos = minSizes.IndexToIndices(idx);
                newArray.SetValue(oldArray.GetValue(currPos), currPos);
            }

            field.SetValue(obj, newArray);
        }

        public static void ResizeFieldArray(FieldInfo field, object obj, int newSize, bool resizeDown = true)
        {
            // Suppose it's all fine no checks

            var arrElType = field.FieldType.GetElementType();
            var oldArray = (Array)field.GetValue(obj);

            if (oldArray.Length == newSize)
                return;
            if (oldArray.Length > newSize && !resizeDown)
                return;

            // Since it's ref it will update the array
            var args = new object[] { oldArray, newSize };
            typeof(Array).GetMethod(nameof(Array.Resize)).MakeGenericMethod(new[] { arrElType }).Invoke(null, args);

            field.SetValue(obj, args[0]);
        }
    }
}
