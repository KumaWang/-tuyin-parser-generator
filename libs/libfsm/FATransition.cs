using libgraph;
using System;
using System.Collections.Generic;

namespace libfsm
{
    public struct FATransition<T> : IEquatable<FATransition<T>>
    {
        /// <summary>
        /// 左侧id
        /// </summary>
        public ushort Left { get; }

        /// <summary>
        /// 右侧id
        /// </summary>
        public ushort Right { get; }

        /// <summary>
        /// 移进符(代数分量)
        /// </summary>
        public EdgeInput Input { get; }

        /// <summary>
        /// 原始图左侧id
        /// 用于引用层级关系，被使用在制表函数中
        /// </summary>
        public ushort SourceLeft { get; }

        /// <summary>
        /// 原始图右侧id
        /// 用于引用层级关系，被使用在制表函数中
        /// </summary>
        public ushort SourceRight { get; }

        /// <summary>
        /// 连线所带的用户元数据
        /// </summary>
        public T Metadata { get; }

        /// <summary>
        /// FA内部符号
        /// </summary>
        public FASymbol Symbol { get; }

        public FATransition(ushort left, ushort right, ushort sourceLeft, ushort sourceRight, EdgeInput input, FASymbol symbol, T metadata)
        {
            Left = left;
            Right = right;
            SourceLeft = sourceLeft;
            SourceRight = sourceRight;
            Input = input;
            Symbol = symbol;
            Metadata = metadata;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Right, Input, Metadata, Symbol);
        }

        public int GetHashCodeFromOrgin()
        {
            return HashCode.Combine(SourceLeft, SourceRight, Input, Metadata, Symbol);
        }

        public override bool Equals(object obj)
        {
            return obj is FATransition<T> transition &&
                   Left == transition.Left &&
                   Right == transition.Right &&
                   Input == transition.Input &&
                   EqualityComparer<T>.Default.Equals(Metadata, transition.Metadata) &&
                   Symbol.Equals(transition.Symbol);
        }

        public override string ToString()
        {
            return $"{(Input == '\0' ? "ε" : Input)} {Left}->{Right}";
        }

        public bool Equals(FATransition<T> transition)
        {
            return Left == transition.Left &&
                   Right == transition.Right &&
                   Input == transition.Input &&
                   Metadata.Equals(transition.Metadata) &&
                   Symbol.Equals(transition.Symbol);
        }

        public bool Equals2(FATransition<T> transition)
        {
            return Left == transition.Left &&
                   Right == transition.Right &&
                   Input == transition.Input;
        }

        public bool EqualsWithoutInput(FATransition<T> transition)
        {
            return Left == transition.Left &&
                   Right == transition.Right &&
                   Metadata.Equals(transition.Metadata) &&
                   Symbol.Equals(transition.Symbol);
        }

        public FATransition<T> ChangeMetadata(T metadata)
        {
            return new FATransition<T>(Left, Right, SourceLeft, SourceRight, Input, Symbol, metadata);
        }
    }
}
