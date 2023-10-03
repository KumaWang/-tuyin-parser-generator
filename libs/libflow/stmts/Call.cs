using System;
using System.Collections.Generic;
using System.Linq;

namespace libflow.stmts
{
    public class Call : Expression
    {
        public Call(string funcName, bool isMutableFormals)
        {
            FunctionName = funcName;
            IsMutableFormals = isMutableFormals;
            Formals = new List<IAstNode>();
        }

        public Call(string funcName, bool isMutableFormals, IEnumerable<IAstNode> formals)
            : this(funcName, isMutableFormals)
        {
            Formals = formals.ToList();
        }

        public Call(string funcName, bool isMutableFormals, params IAstNode[] formals)
            : this(funcName, isMutableFormals)
        {
            Formals = formals.ToList();
        }

        public Call(string funcName, bool isMutableFormals, IAstNode[] endFomrals, params IAstNode[] formals)
            : this(funcName, isMutableFormals)
        {
            Formals = formals.Concat(endFomrals).ToList();
        }

        public string FunctionName { get; }

        public bool IsMutableFormals { get; }

        public IList<IAstNode> Formals { get; }

        public override AstNodeType AstNodeType => AstNodeType.Call;

        public override IEnumerable<IAstNode> GetChildrens()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetStarts()
        {
            return Array.Empty<IAstNode>();
        }

        public override IEnumerable<IAstNode> GetEnds()
        {
            return Array.Empty<IAstNode>();
        }
    }
}
