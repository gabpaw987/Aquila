﻿(*

// 29.07.14 (ES)
timeZone,1,1,1
quantity,1,1,1
minVolume,0,0,100
maxVolume,0,0,100
adxN,14,14,1
adxMax,0,0,5
adxMin,0,0,5
rsiN,11,11,1
rsiEmaN,15,15,1
rsiLong,60,60,10
rsiExitLong,0,0,1
rsiShort,20,20,10
rsiExitShort,0,0,1
barExtrN,100,100,10
extrN,400,400,100
extrPIn,20,20,2
extrPOut,18,18,18
cutlossMax,0.6,0.6,0.2
cutlossMin,0.15,0.15,0.05
cutlossDecrN,400,400,100
takeEarningsMax,0,0,1
takeEarningsMin,0,0,1
takeEarningsD,38,38,1

*)

namespace Algorithm
    module DecisionCalculator687=(*687*)

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

        (*
         * Divides the numerator by the denominator
         * Returns 0 if denominator is 0
         *)
        let divideZero denom num =
            match denom with
            | 0m -> 0m
            | _ -> num/denom

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

        (*
         * Calculates the Directional Movement 
         * dependent on which char is given(+/-) the positive or negative dm
         *)
        let calculateDm(decision:char, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>)=
            // calculate the directional movements (pos / neg)
            let dm = Array.zeroCreate prices.Count
            dm.[0] <- 0m
            for i = 1 to prices.Count - 1 do
                if(decision.Equals('+')) then
                    if(prices.[i].Item5 > prices.[i-1].Item5) then
                        dm.[i] <- prices.[i].Item3 - prices.[i-1].Item3
                    else 
                        dm.[i] <- 0m
                else if(decision.Equals('-')) then
                    if (prices.[i].Item5 < prices.[i-1].Item5) then 
                        dm.[i] <- prices.[i-1].Item4 - prices.[i].Item4
                    else 
                        dm.[i] <- 0m
                else 
                    dm.[i] <- 0m
                if(dm.[i] < 0m) then
                    dm.[i] <- 0m
            Array.toList dm

        (*
         * Calculates the True Range
         * the tr is the highest of:
         * Today's High - Today's Low,
         * Today's High - Yesterday's Close, and
         * Yesterday's Close - Today's Low
         *)
        let calculateTr(prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>)=
            let tr = Array.zeroCreate prices.Count
            tr.[0] <- 0m
            for i = 1 to prices.Count - 1 do
                let tr1 = prices.[i].Item3-prices.[i].Item4
                let tr2 = prices.[i].Item3-prices.[i-1].Item5
                let tr3 = prices.[i-1].Item5-prices.[i].Item4
                let max = [tr1;tr2;tr3] |> List.max
                tr.[i] <- max
            Array.toList tr

        let adx(n:int, prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>)=
            // calculate the directional movements (pos & neg)
            let posdm = ema (n, calculateDm ('+', prices))
            let negdm = ema (n, calculateDm ('-', prices))
            let tr    = ema (n, calculateTr (prices))

            let posdi = Array.zeroCreate (prices.Count)
            let negdi = Array.zeroCreate prices.Count
            for i in n .. posdm.Length-1 do
                posdi.[i] <- 100m*(posdm.[i] |> divideZero tr.[i])
                negdi.[i] <- 100m*(negdm.[i] |> divideZero tr.[i])


            // calculate the difference between the two indicators as a positive number
            let dx = [ for x in 0 .. prices.Count - 1 ->   100m*((abs(posdi.[x] - negdi.[x]))) |> divideZero (abs(posdi.[x] + negdi.[x])) ]
            // why n*2???
//            let adx = ema(n*2, dx)
            let adx = ema(n, dx)
            adx

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
            let timeZone = -5
            // how many futures are traded
            let quantity = 1

            let minVolume = 0L
            let maxVolume = 0L

            let adxN = 14
            let adxMin = 0m
            let adxMax = 100m

            let rsiN = 18
            let rsiEmaN = 5
            let rsiLong = 80m
            let rsiShort = 20m

            let barExtrN = 100
            let extrN = 1000
            let extrPIn = 5m
            let extrPOut = 15m

            let cutlossMax = 5m
            let mutable cutloss = cutlossMax
            let cutlossMin = 0.01m
            let cutlossDecrN = 148

            let takeEarningsMax = 5m
            let mutable takeEarnings = takeEarningsMax
            let takeEarningsMin = 0.01m
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
            // Volume
            let minVolume = int64 parameters.["minVolume"]
            let maxVolume = int64 parameters.["maxVolume"]
            // ADX
            let adxN = int parameters.["adxN"]
            let adxMin = parameters.["adxMin"]
            let adxMax = parameters.["adxMax"]
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
            chart2.Add("ADX;#BBBBFF", new System.Collections.Generic.List<decimal>())
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

            // calculate ADX
            let adx = if (adxN <> 0) then adx (adxN, prices) else [||]
            for i in 0..adx.Length-1 do chart2.["ADX;#BBBBFF"].Add(adx.[i])

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
                    //chart2.["cl;#00FF00"].Add(0m)
                    //chart2.["loss;#4181F0"].Add(0m)
                else
                    (*
                     * // standard behaviour is to keep the last signal
                     *)
                    signals.Add(if (i=0) then 0 else signals.[i-1])

                    /////////////////////////////////////
                    //////   ENTRY SIGNAL
                    /////////////////////////////////////
                    let mutable entry = 0

                    (*
                     * // RSI entry
                     *)
                    let mutable rsiSig = 0

                    if (useRsi) then
                        if (rsiEma.[i] >= rsiLong && rsiEma.[i-1] < rsiLong) then
                            rsiSig <- quantity
                        else if (rsiEma.[i] <= rsiShort && rsiEma.[i-1] > rsiShort) then
                            rsiSig <- -1 * quantity

                    (*
                     * // entry decision
                     *)
                    if (useRsi) then
                        if (sign rsiSig <> sign signals.[i]) then
                            entry <- rsiSig

                    // open position / add to position
                    if (entry <> 0) then
                        signals.[i] <- entry
                    
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
                     * // PRICE EXTREMES
                     *)

//                    // try to find n bar price extremes
//                    let firstExtrI = if (i-extrN+1 > 0) then (i-extrN+1) else 0
//                    printfn "%d before" i
//                    let localExtrema = findExtremes (barExtrN, cPrices.[firstExtrI..i])
//                    // add to chart2
//                    chart2.["LocalExtremes;#00FFFF"].Add(localExtrema.[localExtrema.Length-1])

                    // don't enter in extreme price situations
                    if (i >= extrN-1 && (extrN <> 0 && (extrPIn > 0m || extrPOut > 0m))) then
                        let firstExtrI = if (i-extrN+1 > 0) then (i-extrN+1) else 0

                        // maximum minus minimum price in range
                        let priceBreadth = ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item3] |> List.max) - ([for p in prices.GetRange(i-extrN+1, extrN) -> p.Item4] |> List.min)
                        let mins, maxs = getExtremeValues(extrN, cPrices.[firstExtrI..if(i-barExtrN/2 > 0) then i-barExtrN/2 else 0], localExtrema.[firstExtrI..if(i-barExtrN/2 > 0) then i-barExtrN/2 else 0])
                        if (entry > 0) then
                            for max in maxs do
                                let extrInVal = max-(extrPIn*0.01m*priceBreadth/2m)
                                let extrOutVal = max+(extrPOut*0.01m*priceBreadth/2m)
                                // don't go long in maxs
                                if (cPrices.[i] > extrInVal && cPrices.[i] < extrOutVal) then
                                    entry <- 0
                                    // neutral instead of new position
                                    if (signals.[i-1] < 0) then
                                        exit <- 0
                        else if (entry < 0) then
                            for min in mins do
                                let extrInVal = min-(extrPOut*0.01m*priceBreadth/2m)
                                let extrOutVal = min+(extrPIn*0.01m*priceBreadth/2m)
                                // don't short in mins
                                if (cPrices.[i] > extrInVal && cPrices.[i] < extrOutVal) then
                                    entry <- 0
                                    // neutral instead of new position
                                    if (signals.[i-1] > 0) then
                                        exit <- 0

                    (*
                     * // TREND IDENTIFICATION: only trade in trending markets (adx)
                     *)

                    if (adxN <> 0) then
                        if (adxMax > 0m && adx.[i] > adxMax) || (adxMin > 0m && adx.[i] < adxMin) then
                            exit <- 0

                    (*
                     * // VOLUME
                     *)
                    if (maxVolume > 0L && prices.[i].Item6 > maxVolume) || (minVolume > 0L && prices.[i].Item6 < minVolume) then
                        exit <- 0

                    (*
                     * // CUTLOSS: neutralise if loss is too big
                     *)

                    if (cutlossMax <> 0m) then
                        // same sign: signal now and last bar
                        if (sign signals.[i] = sign signals.[i-1]) then
                            // decrease cutloss over time until it reaches the given minimum
                            // e.g. <- 2 - (2-1)/100
                            cutloss <- cutloss - (cutlossMax-cutlossMin)/(decimal cutlossDecrN)
                            if (cutloss < cutlossMin) then
                                cutloss <- cutlossMin
                            // cut loss: price extreme
                            if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * priceExtreme) then
                                priceExtreme <- cPrices.[i]

                        // new buy or sell signal (different direction)
                        if (signals.[i] <> 0 && sign signals.[i] <> sign signals.[i-1]) then
                            // reset cutloss to maximum for new trade
                            cutloss <- cutlossMax
                            entryPrice <- cPrices.[i]
                            // reset priceExtreme for new trade
                            priceExtreme <- cPrices.[i]

                        // same trading direction (-/+)
                        else if (signals.[i] <> 0) then
                            // check cut loss:
                            if (abs (priceExtreme - cPrices.[i]) > cutloss*0.01m*entryPrice) then
                                // neutral -> liquidate
                                exit <- 0

                    //chart2.["cl;#00FF00"].Add(cutloss*entryPrice * -0.1m)
                    //chart2.["loss;#4181F0"].Add( abs(priceExtreme - cPrices.[i]) * -10m )

                    (*
                     * // TAKE EARNINGS: proportionately decrease a separate earnings based cutloss with increasing profit
                     *)

                    if (takeEarningsMax <> 0m) then
                        // same sign: signal now and last bar
                        if (sign signals.[i] = sign signals.[i-1]) then
                            // profit oriented take earnings: factor (50) is the positive price change (absolute!) for cutloss to be minimum
                            //         5          - (5         - 1        )*50/50 +1
                            takeEarnings <- takeEarningsMax - (takeEarningsMax-takeEarningsMin)*abs(priceExtreme - entryPrice)/(takeEarningsD)
                            if (takeEarnings < takeEarningsMin) then
                                takeEarnings <- takeEarningsMin
                            // take earnings: price extreme
                            if (decimal(sign signals.[i]) * cPrices.[i] > decimal(sign signals.[i-1]) * priceExtreme) then
                                priceExtreme <- cPrices.[i]

                        // new buy or sell signal (different direction)
                        if (signals.[i] <> 0 && sign signals.[i] <> sign signals.[i-1]) then
                            // reset cutloss to maximum for new trade
                            takeEarnings <- takeEarningsMax
                            entryPrice <- cPrices.[i]
                            // reset priceExtreme for new trade
                            priceExtreme <- cPrices.[i]

                        // same trading direction (-/+)
                        else if (signals.[i] <> 0) then
                            // check cut loss:
                            if (abs (priceExtreme - cPrices.[i]) > takeEarnings*0.01m*entryPrice) then
                                // neutral -> liquidate
                                exit <- 0

                    (*
                     * // close position / liquidate part
                     *)
                    if (exit <> 4) then
                        signals.[i] <- exit


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