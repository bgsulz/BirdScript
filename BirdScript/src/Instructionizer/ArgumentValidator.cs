#pragma warning disable CS8601

using BirdScript.Errors;
using BirdScript.Tokenizer;

namespace BirdScript.Instructionizer
{
    public static class ArgumentValidator
    {
        public static bool Match<T0>(this List<IToken> self, out T0 res0)
        {
            res0 = default;
            if (!self[0].TryConvert(out res0)) return false;
            return true;
        }

        public static bool Match<T0, T1>(this List<IToken> self, out T0 res0, out T1 res1)
        {
            res0 = default; res1 = default;
            if (!self[0].TryConvert(out res0)) return false;
            if (!self[1].TryConvert(out res1)) return false;
            return true;
        }

        public static bool Match<T0, T1, T2>(this List<IToken> self, out T0 res0, out T1 res1, out T2 res2)
        {
            res0 = default; res1 = default; res2 = default;
            if (!self[0].TryConvert(out res0)) return false;
            if (!self[1].TryConvert(out res1)) return false;
            if (!self[2].TryConvert(out res2)) return false;
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

        public static bool ValidateBounds(InfoToken<Command> head, params int[] coordinates)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                var item = coordinates[i];
                if (item < 0 || item > 8)
                    throw new ParameterOutOfBoundsException(i, head.Value, item, 0, 8, head.Line);

            }
            return true;
        }
    }
}