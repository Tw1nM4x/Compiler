using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    internal class HighLevelOptimization
    {
        public static NodeExpression ConstantFolding(NodeBinOp binOp)
        {
            dynamic leftValue = 0;
            dynamic rightValue = 0;
            object res;
            NodeExpression left = binOp.left;
            NodeExpression right = binOp.right;

            if (binOp.left is NodeVar varLeft)
                if (varLeft.var_ is SymVarConst constLeft)
                    left = constLeft.value;

            if (left is NodeInt intLeft)
                leftValue = intLeft.value;
            if (left is NodeReal realLeft)
                leftValue = realLeft.value;
            if (left is NodeString strLeft)
                leftValue = strLeft.value;

            if (binOp.right is NodeVar varRight)
                if (varRight.var_ is SymVarConst constRight)
                    right = constRight.value;

            if (right is NodeInt intRight)
                rightValue = intRight.value;
            if (right is NodeReal realRight)
                rightValue = realRight.value;
            if (right is NodeString strRight) 
                rightValue = strRight.value;

            switch (binOp.opname)
            {
                case OperationSign.Plus:
                    res = leftValue + rightValue;
                    break;
                case OperationSign.Minus:
                    res = leftValue - rightValue;
                    break;
                case OperationSign.Divide:
                    res = leftValue / rightValue;
                    break;
                case OperationSign.Multiply:
                    res = leftValue * rightValue;
                    break;
                default:
                    return binOp;
            }

            if(res is int intRes)
                return new NodeInt(intRes);
            if (res is double floatRes)
                return new NodeReal(floatRes);
            if (res is string stringRes)
                return new NodeString(stringRes);

            return binOp;
        }
        public static BlockStmt RemoveUnreachebleCode(BlockStmt block)
        {
            int index = block.body.FindIndex(x =>
            {
                if (x is CallStmt callStmt) 
                    if (callStmt.proc.name == (KeyWord.EXIT.ToString()).ToLower()) 
                        return true;
                return false;
            });

            if (index > 0)
                block.body.RemoveRange(index + 1, block.body.Count - (index + 1));

            return block;
        }
    }
}
