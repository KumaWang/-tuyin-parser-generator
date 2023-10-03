using System;

namespace libfsm
{
    [Flags]
    public enum FATableFlags
    {
        None                = 0,
        /// <summary>
        /// 连接到子集
        /// </summary>
        ConnectSubset       = 1,
        /// <summary>
        /// 最小化合并
        /// </summary>
        Minimize            = 2,
        /// <summary>
        /// 启用连接冲突解决
        /// </summary>
        EdgeConflicts       = 4,
        /// <summary>
        /// 启用元数据冲突解决
        /// </summary>
        MetadataConflicts   = 8,
        /// <summary>
        /// 启用符号冲突解决
        /// </summary>
        SymbolConflicts     = 16,
        /// <summary>
        /// 启用谓词预测
        /// 若禁用可能造成循环无法退出
        /// </summary>
        Predicate           = 32,
        /// <summary>
        /// 子图细分,是否将所有的转跳点切割成子图
        /// </summary>
        Subdivision         = 64,
        /// <summary>
        /// 保持子图原始结构
        /// </summary>
        KeepSubset          = 128,
        /// <summary>
        /// 自动解决歧义
        /// </summary>
        AmbiguityResolution = 256,
        /// <summary>
        /// 启用所有冲突解决
        /// </summary>
        ConflictResolution  = EdgeConflicts | SymbolConflicts | MetadataConflicts,
        /// <summary>
        /// 默认策略
        /// </summary>
        Default = ConnectSubset | ConflictResolution | Minimize
    }
}
