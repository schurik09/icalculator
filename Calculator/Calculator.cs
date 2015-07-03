using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Calculator
{
    public static class Calculator
    {
        private interface IEvaluationState
        {
            void AppendValue( TokenValue value );
            void AppendOperator( TokenOperator op );
        }

        private interface IToken
        {
            void ConvertToPostfixForm( IEvaluationState state );
            void Evaluate( Stack<double> stackOfValues );
        }

        private sealed class TokenValue : IToken
        {
            public double Value { get; private set; }

            public TokenValue( double value )
            {
                this.Value = value;
            }

            public override string ToString() { return Value.ToString(); }

            // Implementation of IToken 
            public void ConvertToPostfixForm( IEvaluationState state )
            { 
                state.AppendValue( this );
            }

            public void Evaluate( Stack<double> stackOfValues )
            {
                stackOfValues.Push( this.Value );
            }
        }

        private sealed class TokenOperator : IToken
        {
            public enum Associativity
            {
                Associative,
                LeftAssociative,
                RightAssociative,
            }

            public char                         OpCode   { get; private set; }
            public Func<double, double, double> Operator { get; private set; }
            public int                          Priority { get; private set; }
            public Associativity                Associac { get; private set; }

            public TokenOperator( char opcode, Func<double, double, double> op, int priority = 0, Associativity associac = Associativity.Associative ) 
            {
                this.OpCode   = opcode;
                this.Operator = op;
                this.Priority = priority;
                this.Associac = associac;
            }

            public override string ToString() { return OpCode.ToString(); }

            // Implementation of IToken 
            public void ConvertToPostfixForm( IEvaluationState state )
            {
                state.AppendOperator( this );
            }

            public void Evaluate( Stack<double> stackOfValues )
            {
                var rhs = stackOfValues.Pop();
                var lhs = stackOfValues.Pop();
                var res = this.Operator( lhs, rhs );
                stackOfValues.Push( res );
            }
        }

        private sealed class EvaluationState : IEvaluationState
        {
            // Access to built postfix expression
            public List<IToken> PostfixExpression { 
                get {
                    while( m_stack_of_ops.Count > 0 )
                        m_postfix_expression.Add( m_stack_of_ops.Pop() );
                    return m_postfix_expression;
                }
            }

            // Implementation of IEvaluationState
            public void AppendValue( TokenValue value )
            {
                m_postfix_expression.Add( value );
            }

            public void AppendOperator( TokenOperator op )
            {
                switch(op.OpCode)
                {
                case '(':
                    m_stack_of_ops.Push( op );
                    break;

                case ')':
                    {
                        var sop = m_stack_of_ops.Pop();
                        while( sop.OpCode != '(' )
                        {
                            m_postfix_expression.Add( sop );
                            sop = m_stack_of_ops.Pop();
                        }
                    }
                    break;

                default: 
                    if( m_stack_of_ops.Count > 0 )
                    {
                        var sop = m_stack_of_ops.Peek();
                        while( op.Priority <  sop.Priority && op.Associac == TokenOperator.Associativity.RightAssociative ||
                               op.Priority <= sop.Priority )
                        {
                            m_postfix_expression.Add( m_stack_of_ops.Pop() );
                            if( m_stack_of_ops.Count > 0 )
                                sop = m_stack_of_ops.Peek();
                            else
                                break;
                        }
                    }
                    m_stack_of_ops.Push( op );
                    break;
                }
            }

            private List<IToken>         m_postfix_expression = new List<IToken>();
            private Stack<TokenOperator> m_stack_of_ops       = new Stack<TokenOperator>();
        }

        private static class Token
        {
            public static IToken Make( string tokenStr )
            {
                tokenStr = tokenStr.Trim();
                if( string.IsNullOrEmpty( tokenStr ) )
                    return null;

                // Try to make the Operation token
                TokenOperator tokenOp;
                if( m_ops.TryGetValue( tokenStr, out tokenOp ) )
                    return tokenOp;

                // Try to make the Value token
                double value;
                if( double.TryParse( tokenStr, out value ) )
                    return new TokenValue( value );

                return null;
            }

            private static readonly Dictionary<string, TokenOperator> m_ops = new Dictionary<string, TokenOperator>()
            {
                { "(", new TokenOperator( '(', null )                                                                                               },
                { ")", new TokenOperator( ')', null )                                                                                               },
                { "+", new TokenOperator( '+', ( double lhs, double rhs ) => lhs + rhs           , 1 )                                              },
                { "-", new TokenOperator( '-', ( double lhs, double rhs ) => lhs - rhs           , 1, TokenOperator.Associativity.LeftAssociative ) },
                { "*", new TokenOperator( '*', ( double lhs, double rhs ) => lhs * rhs           , 2 )                                              },
                { "/", new TokenOperator( '/', ( double lhs, double rhs ) => lhs / rhs           , 2, TokenOperator.Associativity.LeftAssociative ) },
                { "%", new TokenOperator( '%', ( double lhs, double rhs ) => lhs % rhs           , 2, TokenOperator.Associativity.LeftAssociative ) },
                { "^", new TokenOperator( '^', ( double lhs, double rhs ) => Math.Pow( lhs, rhs ), 3, TokenOperator.Associativity.LeftAssociative ) },
            };
        }

        private static List<IToken> TokenizeInfixExpression( string infixExpressionStr )
        {
            var tokenizedExpression = new List<IToken>();
            foreach( var tokenStr in Regex.Split( infixExpressionStr, @"([-+*/%^()])" ) )
            {
                if( string.IsNullOrWhiteSpace( tokenStr ) == false )
                    tokenizedExpression.Add( Token.Make( tokenStr ) );
            }

            return tokenizedExpression;
        }

        private static List<IToken> ConvertToPostfixForm( List<IToken> infixExpression )
        {
            var evaluationState = new EvaluationState();
            foreach( var token in infixExpression )
                token.ConvertToPostfixForm( evaluationState );

            return evaluationState.PostfixExpression;
        }

        private static double Evaluate( List<IToken> postfixExpression )
        {
            var stackOfValues = new Stack<double>();
            foreach( var token in postfixExpression )
                token.Evaluate( stackOfValues );

            return stackOfValues.Pop();
        }

        public static double? EvaluateExpression( string expression )
        {
            //// 2+2*3/(4^(2%1.5))=-1
            //var infixExpression = new List<IToken>();
            //infixExpression.Add( Token.Make( "2.0" ) );
            //infixExpression.Add( Token.Make( "-" ) );
            //infixExpression.Add( Token.Make( "2.0" ) );
            //infixExpression.Add( Token.Make( "*" ) );
            //infixExpression.Add( Token.Make( "3.0" ) );
            //infixExpression.Add( Token.Make( "/" ) );
            //infixExpression.Add( Token.Make( "(" ) );
            //infixExpression.Add( Token.Make( "4.0" ) );
            //infixExpression.Add( Token.Make( "^" ) );
            //infixExpression.Add( Token.Make( "(" ) );
            //infixExpression.Add( Token.Make( "2.0" ) );
            //infixExpression.Add( Token.Make( "%" ) );
            //infixExpression.Add( Token.Make( "1.5" ) );
            //infixExpression.Add( Token.Make( ")" ) );
            //infixExpression.Add( Token.Make( ")" ) );

            double? res = null;
            try
            {
                var infixExpression = TokenizeInfixExpression( expression );

                //foreach( var token in infixExpression )
                //    Console.Write( "{0} ", token );
                //Console.WriteLine();

                var postfixExpression = ConvertToPostfixForm( infixExpression );

                //foreach( var token in postfixExpression )
                //    Console.Write( "{0} ", token );
                //Console.WriteLine();

                res = Evaluate( postfixExpression );

                //Console.WriteLine( res );
            }
            catch( SystemException )
            { }

            return res;
        }
    }
}
