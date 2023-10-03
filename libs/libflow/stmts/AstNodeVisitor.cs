using System;

namespace libflow.stmts
{
    public abstract class AstNodeVisitor<T>
    {
        public virtual T Visit(IAstNode stmt)
        {
            switch (stmt.AstNodeType)
            {
                case AstNodeType.Break:
                    return VisitBreak(stmt as Break);
                case AstNodeType.Concatenation:
                    return VisitConcatenation(stmt as Concatenation);
                case AstNodeType.Call:
                    return VisitCall(stmt as Call);
                case AstNodeType.Continue:
                    return VisitContinue(stmt as Continue);
                case AstNodeType.DoWhile:
                    return VisitDoWhile(stmt as DoWhile);
                case AstNodeType.External:
                    return VisitExternal(stmt);
                case AstNodeType.For:
                    return VisitFor(stmt as For);
                case AstNodeType.Function:
                    return VisitFunction(stmt as Function);
                case AstNodeType.Goto:
                    return VisitGoto(stmt as Goto);
                case AstNodeType.IfElse:
                    return VisitIfElse(stmt as IfElse);
                case AstNodeType.If:
                    return VisitIf(stmt as If);
                case AstNodeType.Label:
                    return VisitLabel(stmt as DefineLabel);
                case AstNodeType.Return:
                    return VisitReturn(stmt as Return);
                case AstNodeType.Switch:
                    return VisitSwitch(stmt as Switch);
                case AstNodeType.While:
                    return VisitWhile(stmt as While);
                case AstNodeType.Empty:
                    return VisitEmpty(stmt as Empty);
                case AstNodeType.Binary:
                    return VisitBinary(stmt as Logic);
                case AstNodeType.Postfix:
                    return VisitPostfix(stmt as Postfix);
                case AstNodeType.Value:
                    return VisitValue(stmt as Value);
                case AstNodeType.Member:
                    return VisitMember(stmt as Member);
                case AstNodeType.Index:
                    return VisitIndex(stmt as Index);
                case AstNodeType.Block:
                    return VisitBlock(stmt as Block);
                case AstNodeType.Obstructive:
                    return VisitObstructive(stmt as IObstructive);
                case AstNodeType.Conditional:
                    return VisitConditional(stmt as Conditional);
                case AstNodeType.Boolean:
                    return VisitBoolean(stmt as Boolean);
                case AstNodeType.Number:
                    return VisitNumber(stmt as Number);
                case AstNodeType.Arithmetic:
                    return VisitArithmetic(stmt as Arithmetic);
                case AstNodeType.Assign:
                    return VisitAssign(stmt as Assign);
                case AstNodeType.Parenthese:
                    return VisitParenthese(stmt as Parenthese);
            }

            throw new NotImplementedException("内部错误");
        }

        protected virtual T VisitParenthese(Parenthese stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitAssign(Assign stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitArithmetic(Arithmetic stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitPostfix(Postfix stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitNumber(Number stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitBoolean(Boolean stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitIndex(Index stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitConditional(Conditional stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitObstructive(IObstructive stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitBlock(Block stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitMember(Member stmt) 
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitValue(Value stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitBinary(Logic stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitEmpty(Empty stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitConcatenation(Concatenation stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitExternal(IAstNode stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitLabel(DefineLabel stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitGoto(Goto stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitContinue(Continue stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitBreak(Break stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitSwitch(Switch stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitIf(If stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitIfElse(IfElse stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitFor(For stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitFunction(Function stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitWhile(While stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitDoWhile(DoWhile stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitCall(Call stmt)
        {
            throw new NotImplementedException();
        }

        protected virtual T VisitReturn(Return stmt)
        {
            throw new NotImplementedException();
        }
    }
}
