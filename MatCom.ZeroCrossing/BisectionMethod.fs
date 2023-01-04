namespace MatCom.ZeroCrossing

open MatCom.Interpreter.Scanner
open System
open System.Threading.Tasks

module ZeroCrossing =   
    let rec bisection (left : double, right : double, expression : string, iterations: int) =
        let tolerance = 0.0001
        if (Evaluator.ParseFunction(expression, left) * Evaluator.ParseFunction(expression, right) > 0) then
            ""
        else
            let mid = Math.Round((left + right)/2.0,4)
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
        
    let getZeroCrossingPoints (points: List<ZeroCrossingRange>, expression : string) = 
        let tasks = Array.zeroCreate points.Length
        let result = Array.zeroCreate points.Length
        for i in 0 .. points.Length-1 do
             tasks.[i] <- Task.Factory.StartNew(fun () -> 
                try
                    result[i] <- bisection(points[i].X1, points[i].X2, expression,0)
                with
                    | :? System.DivideByZeroException -> result[i] <- Math.Round((points[i].X1+ points[i].X2)/2.0,4).ToString()
                )
                 
        Task.WaitAll(tasks)
        result