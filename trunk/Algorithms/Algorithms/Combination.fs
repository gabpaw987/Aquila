(*

timeZone,1,1,1
quantity,1,1,1
rsiAvgN,18,18,2
rsiAvgEmaN,5,5,2
rsiThisN,0,0,2
rsiThisEmaN,0,0,2
rsiLong,80,80,10
rsiShort,20,20,10
rsiExitLong,0,0,1
rsiExitShort,0,0,1
ymW,0.4,0.4,0.1

timeZone,1,1,1
quantity,1,1,1
rsiAvgN,18,18,2
rsiAvgEmaN,5,5,2
rsiThisN,0,0,2
rsiThisEmaN,0,0,2
rsiLong,80,80,10
rsiShort,20,20,10
rsiExitLong,0,0,1
rsiExitShort,0,0,1
ymW,0.5,0.5,0.1

 *)

namespace Algorithm
    module DecisionCalculator=(*4554*)
        
        open System

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   GENERIC FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let debug str=
            System.Diagnostics.Debug.WriteLine str 

        let readSymbol (sym:string)=
            let prices = CSVReader.read("C:/Users/Gabriel/Dropbox/AquilaExchange/10MinBars/" + sym + ".csv");
            debug ("reading: " + sym)
            prices.ToArray()

        let readSymbolRel (sym:string, initO:decimal)=
            let prices = readSymbol(sym)
            let relPrices = Array.zeroCreate prices.Length
            debug (((prices.[0].Item3 - prices.[0].Item2)/prices.[0].Item2).ToString())
            Array.set relPrices 0 ( new Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>(
                // DateTime
                prices.[0].Item1,
                // First Open
                initO,
                // First High
                initO + initO*((prices.[0].Item3 - prices.[0].Item2)/prices.[0].Item2),
                // First Low
                initO + initO*((prices.[0].Item4 - prices.[0].Item2)/prices.[0].Item2),
                // First Close
                initO + initO*((prices.[0].Item5 - prices.[0].Item2)/prices.[0].Item2),
                // Volume
                prices.[0].Item6
            ) )
            for i = 1 to prices.Length-1 do
                Array.set relPrices i ( new Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>(
                    prices.[i].Item1,
                    relPrices.[i-1].Item2 + relPrices.[i-1].Item2*((prices.[i].Item2 - prices.[i-1].Item2)/prices.[i-1].Item2),
                    relPrices.[i-1].Item3 + relPrices.[i-1].Item3*((prices.[i].Item3 - prices.[i-1].Item3)/prices.[i-1].Item3),
                    relPrices.[i-1].Item4 + relPrices.[i-1].Item4*((prices.[i].Item4 - prices.[i-1].Item4)/prices.[i-1].Item4),
                    relPrices.[i-1].Item5 + relPrices.[i-1].Item5*((prices.[i].Item5 - prices.[i-1].Item5)/prices.[i-1].Item5),
                    prices.[i].Item6
                ) )
            relPrices

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   STATS FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let sqr x = x * x
        
        let stdDev nums =
            let mean = nums |> Array.average
            let variance = nums |> Array.averageBy (fun x -> sqr(x - mean))
            decimal (sqrt(double variance))
        
        let volatility (intervalsPA:int) (prices:decimal[])=
            let change : decimal array = Array.zeroCreate ((Array.length prices)-1)
            prices
            |> Array.iteri (fun i p -> 
                match i with
                | _ when i > 0 -> change.[i-1] <- decimal (log( double p / double prices.[i-1] ))
                | _            -> ignore i)
            // annualised: scales in proportion to sqrt of time (e.g. for volatility 1.1% * sqrt(252) for daily bars)
            (stdDev change) * decimal (sqrt (double intervalsPA))

        // moving volatility: takes windows of price data and computes volatility
        let mVolatility (n:int, intervalsPA:int, prices:decimal[])= 
            prices
            |> Seq.windowed n
            |> Seq.map (volatility intervalsPA)
            |> Seq.toArray
            |> Array.append (Array.zeroCreate (n-1))

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   TREND FOLLOWER (e.g. averages)
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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
        /////////   SIGNAL GENERATOR
        //////////////////////////////////////////////////////////////////////////////////////////////////////

        let startCalculation (prices:System.Collections.Generic.List<System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>, 
                              signals:System.Collections.Generic.List<int>,
                              chart1:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              chart2:System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<decimal>>,
                              parameters:System.Collections.Generic.Dictionary<string, decimal>
                              )=

//            // time zone of the server country
//            let timeZone = -5
//            // how many futures are traded
//            let quantity = 1
//
//            // entry signal based on RSI-EMA system on weighted average of market data
//            let rsiAvgN = 18
//            let rsiAvgEmaN = 5
//            let rsiLong = 80m
//            let rsiShort = 20m
//            let rsiExitLong = 20m
//            let rsiExitShort = 80m

            // Chart Lines
            chart1.Add("AVG;#0000FF", new System.Collections.Generic.List<decimal>())
            chart1.Add("Low;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_AVG;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_THIS;#FF00FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_long;#00FF00", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_short;#00FF00", new System.Collections.Generic.List<decimal>())

            (*
             * Read Parameters
             *)
            // currently the only supported time zones are -7 to +2 (trading MO - FR)
            // other settings will produce 0 signals
            let timeZone = int parameters.["timeZone"]
            // Future count
            let quantity = int (abs parameters.["quantity"])

            // RSI
            let rsiAvgN = int parameters.["rsiAvgN"]
            let rsiAvgEmaN = int parameters.["rsiAvgEmaN"]
            let rsiLong = parameters.["rsiLong"]
            let rsiShort = parameters.["rsiShort"]
            // if given 0 then the parameters from the avg rsi are used for the individual as well
            let rsiThisN = if (parameters.["rsiThisN"] <> 0m) then int parameters.["rsiThisN"] else rsiAvgN
            let rsiThisEmaN = if (parameters.["rsiThisEmaN"] <> 0m) then int parameters.["rsiThisEmaN"] else rsiAvgEmaN
            // rsiExit parameters can be deactivated (i.e. exitShort at long threshhold) by assigning 0
            let rsiExitLong = if (parameters.["rsiExitLong"] <> 0m) then parameters.["rsiExitLong"] else rsiShort
            let rsiExitShort = if (parameters.["rsiExitShort"] <> 0m) then parameters.["rsiExitShort"] else rsiLong
            
            // list of closing prices
            let cPrices = 
                [| for p in prices -> p.Item5 |]
            // list of typical prices (used for RSI)
            let tPrices =
                [| for i in prices -> (i.Item3 + i.Item4 + i.Item5)/3m |]
            
            // Additional market data
            // weights
            let markets = new System.Collections.Generic.Dictionary<string, System.Tuple<decimal, System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>[]> >()
            
            // test!
            //markets.Add("ym", (50, readSymbolRel("YM")))

            //markets.Add("test", new Tuple<decimal, Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>[]>(40m, [|new Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>(System.DateTime.Now, 1m,1m,1m,1m,100L)|]))
            if (parameters.ContainsKey("nqW")) then
                markets.Add("nq", new Tuple<decimal,Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>[]>(parameters.["nqW"], readSymbolRel("NQ", prices.[0].Item2)))
            if (parameters.ContainsKey("esW")) then
                markets.Add("es", new Tuple<decimal,Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>[]>(parameters.["esW"], readSymbolRel("ES", prices.[0].Item2)))
            if (parameters.ContainsKey("ymW")) then
                debug "adding ym"
                markets.Add("ym", new Tuple<decimal,Tuple<System.DateTime,decimal,decimal,decimal,decimal,int64>[]>(parameters.["ymW"], readSymbolRel("YM", prices.[0].Item2)))
                debug ("first market count: " + markets.Count.ToString())
            
            let mutable removableKeys = new Collections.Generic.List<string>()
            for key in markets.Keys do
                if (markets.[key].Item2.Length <> cPrices.Length || markets.[key].Item1 = 0m) then
                    debug ("market length: " + markets.[key].Item2.Length.ToString() + " prices length: " + cPrices.Length.ToString())
                    removableKeys.Add(key)

            for key in removableKeys do
                markets.Remove(key)

            debug ("markets: " + markets.Count.ToString())

            let avgPrices = new Collections.Generic.List<Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>>()
            // initially all weight is on current symbol
            let mutable thisW = 1m
            for market in markets do
//                for i=0 to prices.Count-1 do chart1.["AVG;#0000FF"].Add(market.Value.Item2.[i].Item3)
//                for i=0 to prices.Count-1 do chart1.["Low;#0000FF"].Add(market.Value.Item2.[i].Item4)
                // subtract weight of all other symbols from current one
                thisW <- thisW - market.Value.Item1
                debug ("weight " + market.Key.ToString() + ": " + thisW.ToString())
            for i in 0 .. prices.Count-1 do
                // weighted average: begin with this instrument
                let mutable wAvgO = thisW * prices.[i].Item2
                let mutable wAvgH = thisW * prices.[i].Item3
                let mutable wAvgL = thisW * prices.[i].Item4
                let mutable wAvgC = thisW * prices.[i].Item5
                let mutable wAvgVol = (int64)(thisW * (decimal)prices.[i].Item6)
                for market in markets do
                    // weighted average = ... + weight * close
                    wAvgO <- wAvgO + market.Value.Item1 * market.Value.Item2.[i].Item2
                    wAvgH <- wAvgH + market.Value.Item1 * market.Value.Item2.[i].Item3
                    wAvgL <- wAvgL + market.Value.Item1 * market.Value.Item2.[i].Item4
                    wAvgC <- wAvgC + market.Value.Item1 * market.Value.Item2.[i].Item5
                    wAvgVol <- wAvgVol + (int64)(market.Value.Item1 * (decimal)market.Value.Item2.[i].Item6)
                avgPrices.Add(new Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>(prices.[i].Item1, wAvgO, wAvgH, wAvgL, wAvgC, wAvgVol))
                // add average closing price to chart1
                chart1.["AVG;#0000FF"].Add(wAvgC)
//                chart1.["Low;#0000FF"].Add(markets.["ym"].Item2.[i].Item4)

            // list of typical average prices (used for RSI)
            let tAvgPrices =
                [| for i in avgPrices -> (i.Item3 + i.Item4 + i.Item5)/3m |]

            // calculate RSI on weighted average prices
            let rsiAvg = rsi (rsiAvgN, tAvgPrices)
            // smooth RSI
            let rsiAvgEma = ema (rsiAvgEmaN, Array.toList rsiAvg)
            // add to chart: rsi on average prices, threshholds
            for i in 0..rsiAvgEma.Length-1 do chart2.["RSI_AVG;#FF0000"].Add(rsiAvgEma.[i])
            for i in 0..rsiAvgEma.Length-1 do chart2.["RSI_long;#00FF00"].Add(rsiLong)
            for i in 0..rsiAvgEma.Length-1 do chart2.["RSI_short;#00FF00"].Add(rsiShort)

            // calculate individual RSI from this price data (the traded instrument)
            let rsiThis = rsi (rsiThisN, tPrices)
            let rsiThisEma = ema (rsiThisEmaN, Array.toList rsiThis)
            // add to chart: individual rsi
            for i in 0..rsiThisEma.Length-1 do chart2.["RSI_THIS;#FF00FF"].Add(rsiThisEma.[i])

            // first index with all data
            let firstI = ([ rsiAvgN; rsiAvgEmaN; rsiThisN; rsiThisEmaN ] |> List.max) - 1
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

                    // long
                    if (rsiAvgEma.[i] >= rsiLong && rsiAvgEma.[i-1] < rsiLong) then
                        rsiSig <- quantity
                    // short
                    else if (rsiAvgEma.[i] <= rsiShort && rsiAvgEma.[i-1] > rsiShort) then
                        rsiSig <- -1 * quantity

                    (*
                     * // entry decision
                     *)
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
                     * // exit decision based on individual rsiEma
                     *)
                    // exit long if individual signal would short (rsiEma below exitLong threshold)
                    if (signals.[i] > 0 && rsiThisEma.[i] <= rsiExitLong && rsiThisEma.[i-1] > rsiExitLong) then
                        exit <- 0
                    // exit short if individual signal would go long (rsiEma above exitShort threshold)
                    else if (signals.[i] < 0 && rsiThisEma.[i] >= rsiExitShort && rsiThisEma.[i-1] < rsiExitShort) then
                        rsiSig <- -1 * quantity

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
//                    if (match prices.[i].Item1.DayOfWeek with 
//                        | System.DayOfWeek.Monday | System.DayOfWeek.Tuesday | System.DayOfWeek.Wednesday | System.DayOfWeek.Thursday | System.DayOfWeek.Friday
//                            -> true
//                        | _ -> false) then
//                           if (prices.[i].Item1.Hour > 22 - 1 + timeZone || prices.[i].Item1.Hour < 8 - 1 + timeZone || (prices.[i].Item1.Hour = 22 - 1 + timeZone && prices.[i].Item1.Minute > 10)) then
//                                signals.[i] <- 0
//                    // Saturday, Sunday (no trading)
//                    else
//                        signals.[i] <- 0
//                    // currently the only supported time zones are -7 to +2 (trading MO - FR)
//                    // other settings will produce 0 signals
//                    if (timeZone > 2 || timeZone < -7) then
//                        signals.[i] <- 0
            
            signals