using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace libgraph
{
    public struct EdgeInput : IComparable<EdgeInput>, IEquatable<EdgeInput>
    {
        public readonly static EdgeInput Any = new EdgeInput(true, new char[0]);
        public readonly static EdgeInput Empty = new EdgeInput(false, new char[0]);

        private int mHashCode;

        public bool Xor { get; }

        public char[] Chars
        {
            get;
            private set;
        }

        public EdgeInput(ushort c)
           : this((char)c)
        {
        }

        public EdgeInput(char c)
            : this(false, new char[] { c })
        {
        }

        public EdgeInput(bool xor, char c)
            : this(xor, new char[] { c })
        {
        }

        public EdgeInput(bool xor, IEnumerable<char> chars)
        {
            var count = chars.Count();
            chars = chars.OrderBy(x => x);
            var optimize = count > char.MaxValue / 2;
            Xor = optimize ? !xor : xor;
            if (optimize)
            {
                var offset = 0;
                Chars = new char[char.MaxValue - count];
                for (var i = 0; i < char.MaxValue; i++)
                {
                    var c = (char)i;
                    if (!chars.Contains(c))
                    {
                        Chars[offset++] = c;
                    }
                }
            }
            else
            {
                Chars = chars.ToArray();
            }

            mHashCode = 0;
            mHashCode = GetCompareValue();
        }

        public EdgeInput GetCommonDivisor(EdgeInput other)
        {
            var otherToken = other;
            var otherChars = otherToken.Chars;
            if (!Xor)
            {
                var chars = new List<char>();
                if (!otherToken.Xor)
                {
                    if (Chars != null)
                    {
                        for (var i = 0; i < Chars.Length; i++)
                        {
                            var c = Chars[i];
                            if (otherChars != null && otherChars.Contains(c))
                            {
                                if (!chars.Contains(c))
                                {
                                    chars.Add(c);
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (var i = 0; i < Chars.Length; i++)
                    {
                        var c = Chars[i];
                        if (!otherChars.Contains(c))
                        {
                            if (!chars.Contains(c))
                            {
                                chars.Add(c);
                            }
                        }
                    }
                }

                return new EdgeInput(false, chars);
            }
            else
            {
                if (!otherToken.Xor)
                {
                    var chars = new List<char>();
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (!Chars.Contains(c))
                        {
                            if (!chars.Contains(c))
                            {
                                chars.Add(c);
                            }
                        }
                    }

                    return new EdgeInput(false, chars);
                }
                else
                {
                    var chars = new List<char>(Chars);
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (!chars.Contains(c))
                        {
                            chars.Add(c);
                        }
                    }

                    return new EdgeInput(true, chars);
                }
            }
        }

        public EdgeInput Eliminate(EdgeInput other)
        {
            EdgeInput result = default;
            EliminateIntersection(other, out result);
            return result;
        }

        public bool EliminateIntersection(EdgeInput other, out EdgeInput vector)
        {
            var selfChars = new List<char>(Chars);
            var selfXor = Xor;
            var otherToken = other;
            var otherChars = otherToken.Chars;
            if (!Xor)
            {
                if (!otherToken.Xor)
                {
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        selfChars.Remove(c);
                    }
                }
                else
                {
                    for (var i = 0; i < selfChars.Count; i++)
                    {
                        var c = selfChars[i];
                        if (!otherChars.Contains(c))
                        {
                            selfChars.Remove(c);
                        }
                    }
                }
            }
            else
            {
                if (!otherToken.Xor)
                {
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (!selfChars.Contains(c))
                        {
                            selfChars.Add(c);
                        }
                    }
                }
                else
                {
                    var xorChars = new List<char>();
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (!selfChars.Contains(c))
                        {
                            xorChars.Add(c);
                        }
                    }

                    selfChars = xorChars;
                    selfXor = false;
                }
            }

            vector = new EdgeInput(selfXor, selfChars);
            return selfChars.Count == 0;
        }

        public static EdgeInput Combine(IList<EdgeInput> inputs) 
        {
            var input = inputs[0];
            for (var i = 1; i < inputs.Count; i++)
                input = input.Combine(inputs[i]);

            return input;
        }

        public EdgeInput Combine(EdgeInput other)
        {
            var otherToken = other;
            var otherChars = otherToken.Chars;
            if (!Xor)
            {
                if (!otherToken.Xor)
                {
                    var newChars = Chars == null? new List<char>() : new List<char>(Chars);
                    if (otherChars != null)
                    {
                        for (var i = 0; i < otherChars.Length; i++)
                        {
                            var c = otherChars[i];
                            if (Chars == null || !Chars.Contains(c))
                            {
                                newChars.Add(c);
                            }
                        }
                    }
                    return new EdgeInput(false, newChars);
                }
                else
                {
                    var newChars = new List<char>(otherChars);
                    for (var i = 0; i < Chars.Length; i++)
                    {
                        var c = Chars[i];
                        if (newChars.Contains(c))
                        {
                            newChars.Remove(c);
                        }
                    }

                    return new EdgeInput(true, newChars);
                }
            }
            else
            {
                if (!otherToken.Xor)
                {
                    var newChars = new List<char>(Chars);
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (newChars.Contains(c))
                        {
                            newChars.Remove(c);
                        }
                    }
                    return new EdgeInput(true, newChars);
                }
                else
                {
                    var newChars = new List<char>(Chars);
                    for (var i = 0; i < otherChars.Length; i++)
                    {
                        var c = otherChars[i];
                        if (!newChars.Contains(c))
                        {
                            newChars.Add(c);
                        }
                    }

                    return new EdgeInput(true, newChars);
                }
            }
        }

        public bool IsVaild()
        {
            if (Chars == null)
                return false;

            return Chars.Length > 0;
        }

        public override string ToString()
        {
            if (Chars == null)
                return string.Empty;

            if (Chars.Length == 0)
                return Xor ? "^*" : "*";

            if (Chars.Length == 1)
                return (Xor ? "^" : string.Empty) + Chars[0].ToString();

            if (Chars.Length == 2)
                return $"{(Xor ? "^" : string.Empty)}({Chars[0]}|{Chars[1]})";


            // 查找段落
            return (Xor ? "^" : string.Empty) + RangesToString(Chars); // $"[{Chars.First()}..{Chars.Last()}]" + (xorChars.Count > 0 ? "^" + string.Join('|', xorChars) : string.Empty);
        }

        static string RangesToString(char[] a)
        {
            return string.Join(' ', CombineRanges(a));
        }

        static List<string> CombineRanges(char[] a)
        {
            const int xorCount = 3;
            const int showCount = 4;
            var list = new List<string>();

            var ranges = ConsecutiveRanges(a);
            if (ranges.Count > 0)
            {
                var regions = new List<(List<(char, char)>, List<char>)>();
                regions.Add((new List<(char, char)>(), new List<char>()));
                regions[0].Item1.Add(ranges[0]);

                for (var i = 0; i < ranges.Count - 1; i++)
                {
                    var curr = ranges[i];
                    var next = ranges[i + 1];
                    var start = curr.Item2 + 1;
                    var count = next.Item1 - start;
                    if (count < xorCount && count >= 0)
                    {
                        // 合并区间
                        regions[regions.Count - 1].Item1.Add(next);

                        // 得到xor字符
                        regions[regions.Count - 1].Item2.AddRange(Enumerable.Range(start, count).Select(x => (Char)x));
                    }
                    else
                    {
                        // 新建区间
                        regions.Add((new List<(char, char)>(), new List<char>()));
                        regions[regions.Count - 1].Item1.Add(next);
                    }
                }

                foreach (var region in regions)
                {
                    var start = region.Item1.First().Item1;
                    var end = region.Item1.Last().Item2;
                    var diff = end - start;

                    var head = string.Empty;
                    if (diff == 0)
                    {
                        head = start.ToString();
                    }
                    else if (diff < showCount)
                    {
                        head = string.Join("|", Enumerable.Range(start, end - start).Select(x => (Char)x));
                    }
                    else
                    {
                        head = $"[{start}..{end}]";
                    }

                    list.Add(Regex.Escape(head + (region.Item2.Count > 0 ? "^" + RangesToString(region.Item2.ToArray()) : string.Empty)));
                }
            }

            return list;
        }

        static List<(char, char)> ConsecutiveRanges(char[] a)
        {
            int length = 1;
            var list = new List<(char, char)>();

            // If the array is empty,
            // return the list
            if (a.Length == 0)
            {
                return list;
            }

            // Traverse the array from first position
            for (int i = 1; i <= a.Length; i++)
            {

                // Check the difference between the
                // current and the previous elements
                // If the difference doesn't equal to 1
                // just increment the length variable.
                if (i == a.Length || a[i] - a[i - 1] != 1)
                {
                    // If the range contains
                    // only one element.
                    // add it into the list.
                    if (length == 1)
                    {
                        list.Add((a[i - length], a[i - length]));
                    }
                    else
                    {

                        // Build the range between the first
                        // element of the range and the
                        // current previous element as the
                        // last range.
                        list.Add((a[i - length], a[i - 1]));
                    }

                    // After finding the first range
                    // initialize the length by 1 to
                    // build the next range.
                    length = 1;
                }
                else
                {
                    length++;
                }
            }
            return list;
        }

        private int GetCompareValue()
        {
            int result = 0;
            foreach (var c in Chars.OrderBy(x => x))
            {
                result = 37 ^ result + c;
            }

            if (Xor)
            {
                result = int.MaxValue - result;
            }

            return result;
        }

        public override int GetHashCode()
        {
            return mHashCode;
        }

        public int CompareTo(EdgeInput other)
        {
            return mHashCode.CompareTo(other.mHashCode);
        }

        public IEnumerable<char> GetChars(int length)
        {
            if (!Xor)
            {
                if (Chars != null)
                {
                    foreach (var c in Chars)
                    {
                        yield return c;
                    }
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    var c = (char)i;
                    if (!Chars.Contains(c))
                    {
                        yield return c;
                    }
                }
            }
        }

        public bool Contains(char val)
        {
            if (Chars == null)
                return Xor;

            return Xor ? !Chars.Contains(val) : Chars.Contains(val);
        }

        public EdgeInput Clone()
        {
            return new EdgeInput(Xor, Chars.ToArray());
        }

        public int GetLength()
        {
            return Xor ? char.MaxValue - Chars.Length : Chars.Length;
        }

        public EdgeInput Reverse()
        {
            if (Chars.Length == 0)
                return Xor ? EdgeInput.Empty : EdgeInput.Any;

            return new EdgeInput(!Xor, Chars);
        }

        public bool Equals(EdgeInput other)
        {
            return mHashCode.Equals(other.mHashCode);
        }

        public override bool Equals(object obj)
        {
            if (obj is EdgeInput input)
                return Equals(input);

            return false;
        }

        public static bool operator ==(EdgeInput left, EdgeInput right) => left.Equals(right);

        public static bool operator !=(EdgeInput left, EdgeInput right) => !left.Equals(right);

        public static bool operator ==(EdgeInput left, char right) => left.Chars.Length == 1 && left.Chars[0] == right;

        public static bool operator !=(EdgeInput left, char right) => !(left == right);
    }
}
