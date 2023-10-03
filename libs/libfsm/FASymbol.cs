using System;

namespace libfsm
{
    public struct FASymbol
    {
        /// <summary>
        /// 符号类型
        /// </summary>
        public FASymbolType Type { get; }

        /// <summary>
        /// 制表符
        /// </summary>
        public ushort Value { get; }

        /// <summary>
        /// 堆栈变化
        /// </summary>
        public ushort K { get; }
       
        public FASymbol(FASymbolType type, ushort symbol, ushort k)
        {
            K = k;
            Type = type;
            Value = symbol;
        }

        public override bool Equals(object obj)
        {
            return obj is FASymbol symbol &&
                    K == symbol.K &&
                    Type == symbol.Type &&
                    Value == symbol.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, K, Value);
        }
    }
}
