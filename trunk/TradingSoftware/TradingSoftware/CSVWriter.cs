﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TradingSoftware
{
    class CSVWriter
    {
        public static string dirName = "ExportedData";

        public string fileName { get; set; }
        public DateTime lastWrittenDateTime { get; set; }

        public CSVWriter(WorkerViewModel workerViewModel)
        {
            this.fileName = dirName + "/" + workerViewModel.EquityAsString + ".csv";
        }

        public bool WriteBar(Tuple<DateTime, decimal, decimal, decimal, decimal, long> bar)
        {
            lock (IBID.DataExportLock)
            {
                try
                {
                    if (bar.Item1 > this.lastWrittenDateTime)
                    {
                        string barLineToWrite = "";

<<<<<<< HEAD
                        barLineToWrite += bar.Item1.ToString("MM/dd/yyyy, hh:mm:ss tt", CultureInfo.InvariantCulture) + ",";
                        barLineToWrite += bar.Item2.ToString(CultureInfo.InvariantCulture) + "," +
                                          bar.Item3.ToString(CultureInfo.InvariantCulture) + "," +
                                          bar.Item4.ToString(CultureInfo.InvariantCulture) + "," +
                                          bar.Item5.ToString(CultureInfo.InvariantCulture) + "," +
                                          bar.Item6.ToString(CultureInfo.InvariantCulture);
=======
                        barLineToWrite += bar.Item1.ToString("MM/dd/yyyy, hh:mm:ss tt") + ",";
                        barLineToWrite += bar.Item2 + "," + bar.Item3 + "," + bar.Item4 + "," + bar.Item5 + "," + bar.Item6;
>>>>>>> 9808b6b... Updated TS to use volume

                        using (StreamWriter writer = new StreamWriter(this.fileName, true))
                        {
                            writer.WriteLine(barLineToWrite);
                        }

                        this.lastWrittenDateTime = bar.Item1;
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> CreateOrReadCSV()
        {
            List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>> barsOfExistingFile = new List<Tuple<DateTime, decimal, decimal, decimal, decimal, long>>();

            try{                
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }

                if (!File.Exists(this.fileName))
                {
                    File.Create(this.fileName).Close();
                    using (StreamWriter writer = new StreamWriter(this.fileName))
                    {
                        writer.WriteLine("Date,Time,Open,High,Low,Close,Volume");
                    }

                    this.lastWrittenDateTime = new DateTime();
                }
                else
                {
                    if (File.ReadLines(this.fileName).Count() > 1)
                    {
                        barsOfExistingFile = CSVReader.EnumerateExcelFile(this.fileName, DateTime.MinValue, DateTime.MaxValue, false, 1).ToList();

                        //set lastWrittenDateTime
                        string lastLine = File.ReadLines(this.fileName).Last();
                        string[] splittedLastLine = lastLine.Split(',');
                        this.lastWrittenDateTime = ParseBarDateTime(splittedLastLine[0], splittedLastLine[1]);
                    }
                    else
                    {
                        this.lastWrittenDateTime = new DateTime();
                    }
                }
                return barsOfExistingFile;
            }
            catch(Exception)
            {
                return barsOfExistingFile;
            }
        }

        public static DateTime ParseBarDateTime(string date, string time)
        {
            string[] dateValues = date.Split('/');
            string[] timeValues = time.Split(':');

            DateTime timeOfBar = DateTime.MinValue;

            timeOfBar = timeOfBar.AddYears(int.Parse(dateValues[2]) - 1);
            timeOfBar = timeOfBar.AddMonths(int.Parse(dateValues[0]) - 1);
            timeOfBar = timeOfBar.AddDays(int.Parse(dateValues[1]) - 1);

            //Convert the eSignal 11's AM-PM 12 hour clock to our 24 hour clock
            if (timeValues[2].ToCharArray()[3].Equals('P') && !timeValues[0].Equals("12"))
            {
                timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]) + 12);
            }
            else if (timeValues[2].ToCharArray()[3].Equals('A') && timeValues[0].Equals("12"))
            {
                timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]) - 12);
            }
            else
            {
                timeOfBar = timeOfBar.AddHours(int.Parse(timeValues[0]));
            }

            timeOfBar = timeOfBar.AddMinutes(int.Parse(timeValues[1]));

            return timeOfBar;
        }
    }
}
