﻿using System;
using System.Collections.Generic;
using System.Text;

using Calculator;

namespace cli_Calculator
{
    class Program
    {
        static void Main( string[] args )
        {
            var expression = string.Join( "", args );
            if( expression == string.Empty )
            {
                Console.Write( "Enter expression:\n  " );
                expression = Console.ReadLine();
            }
            var result = Calculator.Calculator.EvaluateExpression( expression );
            var resultStr = result == null ? "  Invalid expression" : "  = " + result;
            
            Console.WriteLine( resultStr );
            //Console.ReadLine();
        }
    }
}
