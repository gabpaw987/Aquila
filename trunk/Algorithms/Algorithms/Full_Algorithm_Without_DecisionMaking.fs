﻿(*
// 24.06.14
NQ, trad. thresh.
timeZone,1,1,1
quantity,1,1,1
rsiN,10,10,1
rsiEmaN,15,15,1
rsiLong,20,20,10
rsiExitLong,0,0,1
rsiShort,90,90,10
rsiExitShort,0,0,1
barExtrN,100,100,10
extrN,400,400,50
extrPIn,18,18,1
extrPOut,2,2,1
cutlossMax,2.2,2.2,0.2
cutlossMin,0.05,0.05,0.05
cutlossDecrN,300,300,50
takeEarningsMax,0.8,0.8,1
takeEarningsMin,0,0,1
takeEarningsD,38,38,1

// 03.06.14
NQ
timeZone,-5
quantity,2
rsiN,16
rsiEmaN,6
rsiLong,80
rsiExitLong,0
rsiShort,20
rsiExitShort,0
barExtrN,0
extrN,0
extrPIn,0
extrPOut,0
cutlossMax,2.4
cutlossMin,0.05
cutlossDecrN,800
takeEarningsMax,0
takeEarningsMin,0.05
takeEarningsD,40

ES
timeZone,-5
quantity,1
rsiN,20
rsiEmaN,11
rsiLong,80
rsiExitLong,0
rsiShort,20
rsiExitShort,0
barExtrN,100
extrN,1000
extrPIn,0
extrPOut,13
cutlossMax,2.5
cutlossMin,0.25
cutlossDecrN,1000
takeEarningsMax,0
takeEarningsMin,0
takeEarningsD,10

// 26.05.14
rsiN,18,18,1
rsiEmaN,5,5,1
rsiLong,80,80,10
rsiExitLong,0,0,1
rsiShort,20,20,10
rsiExitShort,0,0,1
barExtrN,0,0,100
extrN,0,0,1000
extrPIn,0,0,5
extrPOut,0,0,1
cutlossMax,0,0,0.5
cutlossMin,0,0,0.05
cutlossDecrN,0,0,50
takeEarningsMax,0,0,2.1
takeEarningsMin,0.05,0.05,1
takeEarningsD,45,45,1
*)

(*
// 25.03.14
rsiN,18,18,1
rsiEmaN,5,5,1
rsiLong,80,80,20
rsiExitLong,
rsiShort,20,20,20
rsiExitShort,
barExtrN,100,100,5
extrN,1000,1000,100
extrPIn,5,5,5
extrPOut,15,15,1
takeEarningsMax,2.1,2.1,2.1
takeEarningsMin,0.05,0.05,0.01
takeEarningsD,45,45,1
cutlossMax,5,5,1
cutlossMin,0.01,0.01,0.01
cutlossDecrN,148,148,20
*)

namespace Algorithm
    module DecisionCalculator0181=(*0181*)

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   GENERIC FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        (*
         * rounds the given number to the nearest even number
         *)
        let even (x:decimal)=
            let s = sign x
            let x = abs x
            let a = decimal(x) % 2m
            if a < 1m then
                s*(int (decimal(x) - a))
            else
                s*(int (decimal(x) + (2m-a)))

        let alphaToN (a) : int=
            int (round ((2.0m/a)-1.0m))

        let alphaToNDec (a) : decimal=
            (2.0m/a)-1.0m
            
        let nToAlpha (n:int) : decimal=
            (2.0m / (decimal n + 1.0m))

        let nToAlphaDec (n:decimal) : decimal=
            (2.0m / (n + 1.0m))

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   TREND FOLLOWER (e.g. averages)
        //////////////////////////////////////////////////////////////////////////////////////////////////////

//        let mutable dimension = new System.Collections.Generic.List<decimal>()
//        let mutable alphas = new System.Collections.Generic.List<decimal>()
//        let mutable ns = new System.Collections.Generic.List<decimal>()

        let sma(n:int, prices:decimal[])=
            prices
            |> Seq.windowed n
            |> Seq.map Seq.average
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        let ema (n:int, prices:List<decimal>)=
            let alpha = (2.0m / (decimal n + 1.0m))
            // return original prices if n = 1
            if (n = 1) then
                List.toArray prices
            else
                // t-1: calculate average of first n-1 elements as initial value for the ema
                let tm1 =
                    prices
                    |> Seq.take (n-1)
                    |> Seq.average
                // create output array
                let ema : decimal array = Array.zeroCreate (List.length prices)
                // put initial ema value into output as first t-1 value
                ema.[n-2] <- tm1
                // calculate ema for each price in the list
                prices
                |> List.iteri (fun i p -> 
                    match i with
                    | _ when i > n-2 -> ema.[i] <- alpha * p + (1m - alpha) * ema.[i-1]
                    | _              -> ignore i)
                // set initial ema value (sma) to 0
                ema.[n-2] <- 0m
                ema

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   OSCILLATORS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let rsi (n:int, prices:decimal[]) = 
            let intervals = 
                prices
                |> Array.toSeq
                |> Seq.windowed n
                |> Seq.toArray
            let sumup = [for i in 0 .. intervals.Length - 1 do yield Array.sum [|for j in 1 .. intervals.[i].Length - 1 do yield Array.max [|intervals.[i].[j] - intervals.[i].[j - 1]; 0m|]|] ]
            let sumdown = [for i in 0 .. intervals.Length - 1 do yield - Array.sum [|for j in 1 .. intervals.[i].Length - 1 do yield Array.min [|intervals.[i].[j] - intervals.[i].[j - 1]; 0m|]|] ]
            [| for i in 0 .. sumup.Length - 1 do yield if (sumup.[i] = sumdown.[i]) then 50m else 100m * (sumup.[i]/(decimal n))/((sumup.[i]/(decimal n)) + (sumdown.[i]/(decimal n)))|]
            |> Array.append (Array.create (n - 1) 0m)

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SPECIAL FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        (*
         * Tries to identify minima and maxima in the given array
         * @n: number of bars an extremum has to be more extreme on both sides
         *)
        let findExtremes(n:int, data:decimal[])=
            let extremes = Array.zeroCreate data.Length
            let mutable isMin = false
            let mutable isMax = false
            for i in n..data.Length-1-n do
                isMin <- true
                isMax <- true
                for j in -n..n do
                    if (data.[i+j] < data.[i]) then
                        isMin <- false
                    if (data.[i+j] > data.[i]) then
                        isMax <- false
                if (isMin) then extremes.[i] <- decimal (-1*n)
                if (isMax) then extremes.[i] <- decimal n
            extremes

        let getExtremeValues(n:int, data:decimal[], extremes:decimal[])=
            let mutable mins = new ResizeArray<decimal>()
            let mutable maxs = new ResizeArray<decimal>()
            let countDownTo = if (extremes.Length-n > 0) then extremes.Length-n else 0
            for i in extremes.Length-1 .. -1 .. countDownTo do
                if (extremes.[i] > 0m) then
                    maxs.Add(data.[i])
                else if (extremes.[i] < 0m) then
                    mins.Add(data.[i])
            mins, maxs

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   SIGNAL GENERATOR
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameters:System.Collections.Generic.Dictionary<string, decimal>
                              )=

            // time zone of the server country


            //Settings omitted


            // minimise take earnings based cutloss after this positive price change (absolute!)
            let takeEarningsD = 45m

            (*
             * Read Parameters
             *)
            // currently the only supported time zones are -7 to +2 (trading MO - FR)
            // other settings will produce 0 signals
            let timeZone = int parameters.["timeZone"]
            // Future count
            let quantity = int (abs parameters.["quantity"])
            // RSI
            let rsiN = int parameters.["rsiN"]
            let rsiEmaN = int parameters.["rsiEmaN"]
            let rsiLong = parameters.["rsiLong"]
            let rsiExitLong = parameters.["rsiExitLong"]
            let rsiShort = parameters.["rsiShort"]
            let rsiExitShort = parameters.["rsiExitShort"]
            // Price Extremes
            let barExtrN = int parameters.["barExtrN"]
            let extrN = int parameters.["extrN"]
            let extrPIn = parameters.["extrPIn"]
            let extrPOut = parameters.["extrPOut"]
            // Cutloss
            let cutlossMax = abs parameters.["cutlossMax"]
            let mutable cutloss = cutlossMax
            let cutlossMin = abs parameters.["cutlossMin"]
            let cutlossDecrN = abs (int parameters.["cutlossDecrN"])
            // Take Earnings
            let takeEarningsMax = parameters.["takeEarningsMax"]
            let takeEarningsMin = parameters.["takeEarningsMin"]
            let takeEarningsD = parameters.["takeEarningsD"]
            //*)
            
            // Chart Lines
            chart2.Add("LocalExtremes;#00FFFF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_long;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_short;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("cl;#00FF00", new System.Collections.Generic.List<decimal>())
            chart2.Add("loss;#4181F0", new System.Collections.Generic.List<decimal>())
            
            // list of closing prices
            let cPrices = 
                [| for i in prices -> i.Item5 |]
            // list of typical prices
            let tPrices =
                [| for i in prices -> (i.Item3 + i.Item4 + i.Item5)/3m |]

            let useRsi = if (rsiN <> 0 && rsiEmaN <> 0) then true else false
            // calculate RSI
            let rsi = if (useRsi) then rsi (rsiN, tPrices) else Array.empty
            // smooth RSI
            let rsiEma = if (useRsi) then ema (rsiEmaN, Array.toList rsi) else Array.empty
            for i in 0..rsiEma.Length-1 do chart2.["RSI;#FF0000"].Add(rsiEma.[i])
            for i in 0..rsiEma.Length-1 do chart2.["RSI_long;#0000FF"].Add(rsiLong)
            for i in 0..rsiEma.Length-1 do chart2.["RSI_short;#0000FF"].Add(rsiShort)

            // try to find n bar price extrema
            let localExtrema = findExtremes (barExtrN, cPrices)
            // add to chart2
            for i in 0..localExtrema.Length-1 do chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[i])

            // price at trade entry (long or short)
            let mutable entryPrice = 0m
            // price extreme in trade for cut loss
            let mutable priceExtreme = cPrices.[0]

            // first index with all data
            let firstI = ([ rsiN; rsiEmaN ] |> List.max) - 1
            let mutable missingData = firstI+1

            // clear list of all signals before calculation (necessary for real time testing/trading!)
            signals.Clear();
            for i in 0 .. prices.Count-1 do
                // one bar more available
                missingData <- missingData - 1

                // Not all neccessary data available yet
                // (.. or new day)
                if i < firstI || missingData > 0 then
                    signals.Add(0)
                    chart2.["cl;#00FF00"].Add(0m)
                    chart2.["loss;#4181F0"].Add(0m)
                else
                    (*
                     * // standard behaviour is to keep the last signal
                     *)
                    signals.Add(if (i=0) then 0 else signals.[i-1])

                    /////////////////////////////////////
                    //////   MISSING: ENTRY SIGNAL
                    /////////////////////////////////////

                    
                    /////////////////////////////////////
                    //////   EXIT SIGNAL
                    /////////////////////////////////////
                    let mutable exit = 4

                    (*
                     * // RSI EXIT
                     *)
                    // exit long position
                    if (rsiExitLong <> 0m && signals.[i] > 0 && rsiEma.[i] < rsiExitLong && rsiEma.[i-1] > rsiExitLong) then
                        exit <- 0
                    // exit short position
                    else if (rsiExitShort <> 0m && signals.[i] < 0 && rsiEma.[i] > rsiExitShort && rsiEma.[i-1] < rsiExitShort) then
                        exit <- 0

                    (*
                     * // MISSING: PRICE EXTREMES
                     *)

                

                    (*
                     * // MISSING: CUTLOSS: neutralise if loss is too big
                     *)

                   
                    chart2.["loss;#4181F0"].Add( abs(priceExtreme - cPrices.[i]) * -10m )

                    (*
                     * // MISSING: TAKE EARNINGS: proportionately decrease a separate earnings based cutloss with increasing profit
                     *)



                    (*
                     * // TRADING TIMES
                     *)

                    // Trading Times ignoring short pauses
                    // Monday to Friday: 0:00 - 22:10
                    if (match prices.[i].Item1.DayOfWeek with 
                        | System.DayOfWeek.Monday | System.DayOfWeek.Tuesday | System.DayOfWeek.Wednesday | System.DayOfWeek.Thursday | System.DayOfWeek.Friday
                            -> true
                        | _ -> false) then
                           if (prices.[i].Item1.Hour > 22 - 1 + timeZone || prices.[i].Item1.Hour < 8 - 1 + timeZone || (prices.[i].Item1.Hour = 22 - 1 + timeZone && prices.[i].Item1.Minute > 10)) then
                                signals.[i] <- 0
                    // Saturday, Sunday (no trading)
                    else
                        signals.[i] <- 0
                    // currently the only supported time zones are -7 to +2 (trading MO - FR)
                    // other settings will produce 0 signals
                    if (timeZone > 2 || timeZone < -7) then
                        signals.[i] <- 0
                    
            signals