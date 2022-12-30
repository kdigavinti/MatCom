namespace MatCom.ZeroCrossing

open MatCom.Interpreter.Scanner
open System

module ZeroCrossing =
    let rec bisection (left : double, right : double, expression : string, iterations: int) =
        let tolerance = 0.001
        if (Evaluator.ParseFunction(expression, left) * Evaluator.ParseFunction(expression, right) > 0) then
            ""
        else
            let mid = (left + right)/2.0
            let yMid = Evaluator.ParseFunction(expression, mid)            
            if(Math.Abs(yMid) >= tolerance && iterations < 500) then
                if(Evaluator.ParseFunction(expression, mid) = 0.0) then
                    mid.ToString()
                else
                    if ((Evaluator.ParseFunction(expression, mid) * Evaluator.ParseFunction(expression, left)) > 0.0) then
                        bisection (mid, right, expression, iterations+1)
                    else
                        bisection (left, mid, expression, iterations+1)                
            else
                mid.ToString()
