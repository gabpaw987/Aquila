namespace Algorithm
    module DecisionCalculator=(*4554*)

        //////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////   GENERIC FUNCTIONS
        //////////////////////////////////////////////////////////////////////////////////////////////////////

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

            // time zone of the server country
            let timeZone = -5
            // how many futures are traded
            let quantity = 1

            // entry signal based on RSI-EMA system on weighted average of market data
            let rsiN = 18
            let rsiEmaN = 5
            let rsiLong = 80m
            let rsiShort = 20m

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
            let rsiShort = parameters.["rsiShort"]
            
            // list of closing prices
            let cPrices = 
                [| for p in prices -> p.Item5 |]
            
            // Additional market data
            // weights
            let markets = new System.Collections.Generic.Dictionary<string, System.Tuple<decimal, System.Tuple<System.DateTime, decimal, decimal, decimal, decimal, int64>[]> >()
            
            // test!
            //markets.Add("ym", (50, readSymbol("YM")))

            if (parameters.ContainsKey("nqW")) then
                markets.Add("nq", (parameters.["nqW"], readSymbol("NQ")))
            if (parameters.ContainsKey("esW")) then
                markets.Add("es", (parameters.["esW"], readSymbol("ES")))
            if (parameters.ContainsKey("ymW")) then
                markets.Add("ym", (parameters.["ymW"], readSymbol("YM")))
            
            for market in markets do
                if (market.Value.Item2.Length <> cPrices.Length || market.Value.Item1 = 0m) then
                    ignore(markets.Remove(market.Key))

            let avgPrices : decimal array = Array.zeroCreate cPrices.Length
            // initially all weight is on current symbol
            let mutable thisW = 1m
            for market in markets do
                // subtract weight of all other symbols from current one
                thisW <- thisW - market.Value.Item1
            for i in 0 .. cPrices.Length-1 do
                // weighted average: begin with this instrument
                let mutable wAvg = thisW * cPrices.[i]
                for market in markets do
                    // weighted average = ... + weight * close
                    wAvg <- wAvg + market.Value.Item1 * market.Value.Item2.[i].Item5
                avgPrices.[i] <- wAvg

            // Chart Lines
            chart2.Add("AVG;#0000FF", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI;#FF0000", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_long;#00FF00", new System.Collections.Generic.List<decimal>())
            chart2.Add("RSI_short;#00FF00", new System.Collections.Generic.List<decimal>())

            // calculate RSI
            let rsi = rsi (rsiN, avgPrices)
            // smooth RSI
            let rsiEma = ema (rsiEmaN, Array.toList rsi)
            for i in 0..rsiEma.Length-1 do chart2.["RSI;#FF0000"].Add(rsiEma.[i])

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

                    if (rsiEma.[i] >= rsiLong && rsiEma.[i-1] < rsiLong) then
                        rsiSig <- quantity
                    else if (rsiEma.[i] <= rsiShort && rsiEma.[i-1] > rsiShort) then
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
                     * TODO: exit strategy
                     *)

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

        let readSymbol (sym:string)=
            let prices = //CSVReader.EnumerateExcelFile("D:/noctua/trunk/Input_Data/" + sym + ".csv", new DateTime(), DateTime.Now).ToList();
            prices