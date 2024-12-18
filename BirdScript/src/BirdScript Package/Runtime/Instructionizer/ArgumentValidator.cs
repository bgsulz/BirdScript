#pragma warning disable CS8601

using System;
using System.Collections.Generic;
using BirdScript.Errors;
using BirdScript.Tokenizing;

namespace BirdScript.Instructionizing
{
    public static class ArgumentValidator
    {
        public static bool Match(this List<IToken> self, bool strictCount = true)
        {
            if (strictCount && self.Count != 0) return false;
            return true;
        }

        public static bool Match<T0>(this List<IToken> self, out T0 res0, bool strictCount = true)
        {
            res0 = default;
            if (strictCount && self.Count != 1) return false;
            if (!self[0].TryConvert(out res0)) return false;
            return true;
        }

        public static bool Match<T0, T1>(this List<IToken> self, out T0 res0, out T1 res1, bool strictCount = true)
        {
            res0 = default; res1 = default;
            if (strictCount && self.Count != 2) return false;
            if (!self[0].TryConvert(out res0)) return false;
            if (!self[1].TryConvert(out res1)) return false;
            return true;
        }

        public static bool Match<T0, T1, T2>(this List<IToken> self, out T0 res0, out T1 res1, out T2 res2, bool strictCount = true)
        {
            res0 = default; res1 = default; res2 = default;
            if (strictCount && self.Count != 3) return false;
            if (!self[0].TryConvert(out res0)) return false;
            if (!self[1].TryConvert(out res1)) return false;
            if (!self[2].TryConvert(out res2)) return false;
            return true;
        }

        public static bool MatchAll<T>(this List<IToken> self, out T[] res)
        {
            res = new T[self.Count];
            for (int i = 0; i < self.Count; i++)
            {
                if (!self[i].TryConvert(out res[i])) return false;
            }
            return true;
        }

        public static bool TryConvert<T>(this IToken token, out T res)
        {
            if (token is InfoToken<T> t)
            {
                res = t.Value;
                return true;
            }

            if (typeof(T) == typeof(int) && token is InfoToken<float> ti)
            {
                res = (T)Convert.ChangeType(Math.Round(ti.Value), typeof(T));
                return true;
            }

            res = default;
            return false;
        }

        public static bool ValidateBounds(InfoToken<Command> head, params int[] coordinates) => ValidateBounds(head, 0, 8, coordinates);

        public static bool ValidateBounds(InfoToken<Command> head, int min, int max, params int[] coordinates)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                var item = coordinates[i];
                if (item < min || item > max)
                    throw new ParameterOutOfBoundsException(i, head.Value, item, min, max, head.Line);

            }
            return true;
        }
    }
}