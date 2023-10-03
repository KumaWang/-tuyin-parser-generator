namespace librule.targets.code
{
    /// <summary>
    /// 简单C系代码格式化
    /// </summary>
    class CCodeFormatter : CodeFormatter
    {
        public CCodeFormatter()
        {
            var block = new CodeFormatterBlock("{", 2, -1, new CodeFormatterBlockEnd("}", true));
            
            var switchEnds = new CodeFormatterBlockEnd[]
            {
                new CodeFormatterBlockEnd("break", false),
                new CodeFormatterBlockEnd("return", false),
                new CodeFormatterBlockEnd("goto", false),
                new CodeFormatterBlockEnd("throw", false),
                // 如果在(case|default)中遇到}时且不再block文法内则代表文法结束并退出上一层
                new CodeFormatterBlockEnd("}", true, true)
            };

            // 注意switchEnds.Concat到一个Immediately(立即还原)在格式化引擎中应该是根据文法得出的
            // 比如switch文法中，后续(Follow)只有case .. | default .. 那么它的Follow级之间将成为Immediately
            // 在这里简化定义并输出代码
            // var @switch = new CodeFormatterBlock("switch", 0, 1, switchEnds.Concat(new CodeFormatterBlockEnd[] { new CodeFormatterBlockEnd("}", true) }).ToArray());
            var @case = new CodeFormatterBlock("case", 2, 2, switchEnds.Concat(new CodeFormatterBlockEnd[] { new CodeFormatterBlockEnd("default", true) }).ToArray());
            var @default = new CodeFormatterBlock("default", 2, 2, switchEnds.Concat(new CodeFormatterBlockEnd[] { new CodeFormatterBlockEnd("case", true) }).ToArray());
            InitBlocks(block, @case, @default);
        }
    }
}
