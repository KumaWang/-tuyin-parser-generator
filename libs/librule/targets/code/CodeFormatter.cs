using System.Text;

namespace librule.targets.code
{
    /// <summary>
    /// 简单格式化工具，只能对行首(String.StartsWith)进行匹配
    /// 需要自己对文法结构进行标注,具体可以参考<see cref="CCodeFormatter"/>注释
    /// </summary>
    abstract class CodeFormatter
    {
        private StringBuilder _outputBuffer;

        private CodeFormatterBlock[] _blocks;
        private Stack<CodeFormatterLayer> _stacks;

        struct CodeFormatterLayer 
        {
            public CodeFormatterLayer(CodeFormatterBlock block, int indent)
            {
                Block = block;
                Indent = indent;
            }

            public CodeFormatterBlock Block { get; }

            public int Indent { get; }
        }

        protected CodeFormatter()
        {
            _stacks = new Stack<CodeFormatterLayer>();
            _outputBuffer = new StringBuilder();
        }

        public CodeFormatter(params CodeFormatterBlock[] blocks)
            : this()
        {
            InitBlocks(blocks);
        }

        protected void InitBlocks(params CodeFormatterBlock[] blocks)
        {
            _blocks = blocks;
        }

        public string Print(string code)
        {
            _outputBuffer.Clear();

            var line = 0;
            var lines = code.Replace("\r", string.Empty).Split('\n').SelectMany(SplitLines).ToArray();
            for (var i = 0; i < lines.Length; i++)
            {
                if (Apply(lines[i].Trim(), line))
                {
                    if (i != lines.Length - 1)
                        _outputBuffer.Append("\n");

                    line++;
                }
            }
            return _outputBuffer.ToString();
        }

        private IEnumerable<string> SplitLines(string code) 
        {
            var lastIndex = 0;
            (string, int) Check(int index)
            {
                var end = Math.Min(code.Length, index + 1);
                if (end - lastIndex <= 0)
                    return (string.Empty, index + 1);

                var add = 0;
                for (var i = index + 1; i < code.Length; i++)
                    if (code[i] == ';')
                        add++;
                    else
                        break;

                return (code.Substring(lastIndex, end - lastIndex + add), index + add + 1);
            }
  
            var num = 0;
            bool inConditional = false;
            bool inString = false;
            char attach = default;
            for(var i = 0; i < code.Length; i++) 
            {
                var c = code[i];
                switch (c) 
                {
                    case '\"':
                        if (attach == c)
                            attach = default;
                        else if (attach == default)
                            attach = c;

                        inString = !inString;
                        break;
                    case '{':
                        if (!inString)
                        {
                            if (attach == default)
                            {
                                attach = c;
                                num = 1;
                            }
                            else if (attach == c)
                            {
                                num++;
                            }
                        }
                        break;
                    case '}':
                        if (!inString)
                        {
                            if (attach == '{')
                                if (--num == 0)
                                {
                                    attach = default;
                                    var line3 = Check(i);
                                    if (!string.IsNullOrWhiteSpace(line3.Item1))
                                        yield return line3.Item1;
                                    lastIndex = line3.Item2;
                                }
                        }
                        break;
                    case '?':
                        inConditional = true;
                        break;
                    case ':':
                        if (!inConditional)
                            goto case ';';
                        else
                        {
                            inConditional = false;
                            break;
                        }
                    case ';':
                        if (attach == default)
                        {
                            var line2 = Check(i);
                            if (!string.IsNullOrWhiteSpace(line2.Item1))
                                yield return line2.Item1;
                            lastIndex = line2.Item2;
                        }
                        break;
                }
            }

            var line = Check(code.Length);
            if (!string.IsNullOrWhiteSpace(line.Item1))
                yield return line.Item1;
        }

        private bool Apply(string code, int line)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var applyBlock = _blocks.FirstOrDefault(x => code.StartsWith(x.Start));
            var isSame = _stacks.Count > 0 ? applyBlock?.Token != -1 && _stacks.Peek().Block.Token == applyBlock?.Token : false;
            var ends = _stacks.Count > 0 ? _stacks.Peek().Block.Ends.Where(x => code.StartsWith(x.Context)).ToArray() : Array.Empty<CodeFormatterBlockEnd>();
            if (ends.Length > 1)
                throw new NotImplementedException();

            var isBack = ends.Length > 0;
            if (isBack && ends[0].Immediately)
            {
                _stacks.Pop();
                if (ends[0].Pop && _stacks.Count > 0) _stacks.Pop();
                isBack = false;
            }

            var indent = _stacks.Count > 0 ? _stacks.Peek().Indent : 0;
            var len = Math.Max(0, indent - (isSame ? applyBlock.Spcae : 0));
            _outputBuffer.Append(len == 0 ? code : new string(' ', len) + code);

            if (!isSame)
            {
                if (isBack)
                {
                    _stacks.Pop();
                    if (ends[0].Pop && _stacks.Count > 0) _stacks.Pop();
                }

                // 需要检查行尾是否直接结束
                if (applyBlock != null && !applyBlock.Ends.Any(x => code.EndsWith(x.Context)))
                {
                    // 进入层级
                    if (applyBlock.Spcae != 0) _stacks.Push(new CodeFormatterLayer(applyBlock, indent + applyBlock.Spcae));
                }
            }

            return true;
        }

        public override string ToString()
        {
            return _outputBuffer.ToString();
        }


    }
}
