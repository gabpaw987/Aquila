module CSVReader2
    open System
    let read (inputPath:string) = 
        printfn "Reading PriceData"
        let readLines:string[] = System.IO.File.ReadAllLines(inputPath)
        let prices = new System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal,int64>>()
        for i in 1 .. readLines.Length - 1 do
            
            let mutable date = new System.DateTime();
            date <- date.AddMonths(int ((readLines.[i]).Split([|';'|]).[0].Split([|'.'|]).[0]) - 1);
            date <- date.AddDays(float ((readLines.[i]).Split([|';'|]).[0].Split([|'.'|]).[1]) - 1.0);
            date <- date.AddYears(int ((readLines.[i]).Split([|';'|]).[0].Split([|'.'|]).[2]) - 1);
            date <- date.AddHours(float ((readLines.[i]).Split([|';'|]).[1].Split([|':'|]).[0]));
            date <- date.AddMinutes(float ((readLines.[i]).Split([|';'|]).[1].Split([|':'|]).[1]));
//            if ((readLines.[i]).Split([|','|]).[1].Split([|' '|]).[1]).Equals "AM" && date.Hour = 12 then
//                date <- date.AddHours(- 12.0);
//            if ((readLines.[i]).Split([|','|]).[1].Split([|' '|]).[1]).Equals "PM" && date.Hour <> 12 then
//                date <- date.AddHours(12.0);
            let a = (new System.Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>(date, (decimal ((readLines.[i]).Split([|';'|]).[2])), (decimal ((readLines.[i]).Split([|';'|]).[3])), 
                            (decimal ((readLines.[i]).Split([|';'|]).[4])), (decimal ((readLines.[i]).Split([|';'|]).[5])),int64 1)) 
            prices.Add a 
        printfn "Finished reading"            
        prices
