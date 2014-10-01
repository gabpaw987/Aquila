using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Krs.Ats.IBNet.Contracts;
using System.IO;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Net.Mail;
using System.Net;
using System.Text;

namespace TradingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer myTimer;

        public MainWindow()
        {
            InitializeComponent();

            this.mainViewModel.Workers = new List<Worker>();
            this.mainViewModel.WorkerViewModels = new List<WorkerViewModel>();
            this.mainViewModel.SignalBoxes = new List<ScrollViewer>();
            this.mainViewModel.CreationAlgorithmFilePath = "Algorithms.dll";

            XMLHandler.CreateSettingsFileIfNecessary();
            XMLHandler.LoadWorkersFromXML(this);
            SetTimerValue(IBID.TimeToReconnectToIB);
        }

        private void SetTimerValue(DateTime timerValue)
        {
            // trigger the event at 7 AM. For 7 PM use 19 i.e. 24 hour format
            DateTime requiredTime = timerValue;
            // initialize timer only, do not specify the start time or the interval
            myTimer = new System.Threading.Timer(new TimerCallback(TimerAction));
            // first parameter is the start time and the second parameter is the interval
            // Timeout.Infinite means do not repeat the interval, only start the timer
            myTimer.Change((int)(requiredTime - DateTime.Now).TotalMilliseconds, Timeout.Infinite);
        }

        private void TimerAction(object e)
        {
            //send trade report
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("aquila.ceon@gmail.com", "pEg200aQ"),
                EnableSsl = true
            };

            string csvFileName = "Reports/report.csv";
            int indexOfDate = 0;
            string[] tradeReportContent = File.ReadAllLines(csvFileName);
            for (int i = 0; i < tradeReportContent[0].Split(',').Length; i++)
            {
                if (tradeReportContent[0].Split(',')[i].Equals("Date"))
                {
                    indexOfDate = i;
                }
            }
            string currentDateAsString = tradeReportContent[1].Split(',')[indexOfDate];
            string newCsvFileName = "trades." + currentDateAsString + ".csv";

            //Move to send attachment without paths in the file name and copy to save it in the archive
            File.Copy(csvFileName, "Reports/Archive/" + newCsvFileName);
            File.Move(csvFileName, newCsvFileName);

            //using (MailMessage mailMessage = new MailMessage("aquila.ceon@gmail.com", "josef.nikolaus@sochovsky.at"))
            using (MailMessage mailMessage = new MailMessage("aquila.ceon@gmail.com", "futures@ing.lu"))
            {
                mailMessage.Bcc.Add("josef.nikolaus@sochovsky.at");
                mailMessage.Bcc.Add("peer@gmx.at");
                mailMessage.Bcc.Add("hans@brabenetz.com");
                mailMessage.Bcc.Add("gabriel.pawlowsky@yahoo.de");
                mailMessage.Subject = "Trade Confirmation CEON Trading Fund " + currentDateAsString;
                mailMessage.Body = File.ReadAllText("Reports/Trade Confirmation CEON Trading Fund 20140904.htm");
                mailMessage.IsBodyHtml = true;
                mailMessage.BodyEncoding = Encoding.UTF32;
                Attachment attachment = new Attachment(newCsvFileName);
                mailMessage.Attachments.Add(attachment);
                client.Send(mailMessage);
            }
            //Delete the temporary attachment version of the trade report
            File.Delete(newCsvFileName);

            //reconnect all running workers
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                if (worker.IsRunning())
                {
                    worker.Stop(false);
                    worker.Start(false);

                    worker.workerViewModel.ConsoleText += worker.workerViewModel.EquityAsString + ": Trade-Report sent and Worker reconnected.\n";
                    worker.workerViewModel.SignalText += worker.workerViewModel.EquityAsString + ": Trade-Report sent and Worker reconnected.\n";
                }
            }

            // now, call the set timer method to reset its next call time
            SetTimerValue(IBID.TimeToReconnectToIB);
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9]");
            e.Handled = reg.IsMatch(e.Text);
        }

        private void NumericOnly_WithDecimalPlace(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex reg = new Regex("[^0-9,]");
            e.Handled = reg.IsMatch(e.Text);
        }

        private void ScrollToEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox) sender;

            //Get symbol traded by of the sending Worker
            string symbol = "";
            for(int i = 0; i < textBox.Name.Length; i++)
            {
                if(!textBox.Name.ElementAt(i).Equals('_'))
                {
                    symbol += textBox.Name.ElementAt(i);
                }
                else
                {
                    i = textBox.Name.Length;
                }
            }

            foreach(ScrollViewer scrollViewer in this.mainViewModel.SignalBoxes)
            {                
                if(scrollViewer.Name.Substring(0,symbol.Length).Equals(symbol))
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }

        private void ScrollToEndCreationAlgorithmPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.Focus(); // just for sure
            textBox.CaretIndex = textBox.Text.Length;
            Rect rect = textBox.GetRectFromCharacterIndex(textBox.CaretIndex);
            textBox.ScrollToHorizontalOffset(Math.Max((textBox.HorizontalOffset + rect.Left - (textBox.ActualWidth - 40)), 0.0));
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerTab workerTab = new WorkerTab(this);

            XMLHandler.CreateWorker(this.mainViewModel.CreationSymbol,
                                    this.mainViewModel.CreationExchange,
                                    this.mainViewModel.CreationIsTrading,
                                    this.mainViewModel.CreationBarSize,
                                    this.mainViewModel.CreationDataType,
                                    this.mainViewModel.CreationAlgorithmFilePath,
                                    this.mainViewModel.CreationPricePremiumPercentage,
                                    this.mainViewModel.CreationIsFuture,
                                    this.mainViewModel.CreationCurrentPosition,
                                    this.mainViewModel.CreationShallIgnoreFirstSignal,
                                    this.mainViewModel.CreationHasAlgorithmParameters,
                                    (this.mainViewModel.CreationIsFuture ? 1 : this.mainViewModel.CreationRoundLotSize),
                                    this.mainViewModel.CreationAlgorithmParameters);

            // TODO: block creation if something is not filled out
            // TODO: GUI for algorithmFilePath
            Worker worker = new Worker(this.mainViewModel,
                                       workerTab.workerViewModel,
                                       this.mainViewModel.CreationSymbol,
                                       this.mainViewModel.CreationExchange,
                                       this.mainViewModel.CreationIsTrading,
                                       this.mainViewModel.CreationBarSize,
                                       this.mainViewModel.CreationDataType,
                                       this.mainViewModel.CreationPricePremiumPercentage,
                                       (this.mainViewModel.CreationIsFuture ? 1 : this.mainViewModel.CreationRoundLotSize),
                                       this.mainViewModel.CreationIsFuture,
                                       this.mainViewModel.CreationCurrentPosition,
                                       this.mainViewModel.CreationShallIgnoreFirstSignal,
                                       this.mainViewModel.CreationHasAlgorithmParameters,
                                       this.mainViewModel.CreationAlgorithmFilePath,
                                       this.mainViewModel.CreationAlgorithmParameters);
            this.mainViewModel.Workers.Add(worker);

            workerTab.setUpTabWorkerConnection(worker);
            this.mainViewModel.WorkerViewModels.Add(workerTab.workerViewModel);

            this.workersGrid.Items.Refresh();

            this.MainTabControl.Items.Insert(this.MainTabControl.Items.Count - 1, workerTab);
            this.AddSignalBoxToSummary(workerTab.workerViewModel);

            //reset creation-variables
            this.mainViewModel.CreationSymbol = "";
            this.mainViewModel.CreationIsTrading = false;
            this.mainViewModel.CreationExchange = "GLOBEX";
            this.mainViewModel.CreationAlgorithmFilePath = "Algorithms.dll";
        }

        public void AddSignalBoxToSummary(WorkerViewModel workerViewModel)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Name = workerViewModel.EquityAsString + "_scrollViewer";
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Margin = new Thickness(2);

            TextBox textBox = new TextBox();
            textBox.Name = workerViewModel.EquityAsString + "_textBox";
            textBox.DataContext = workerViewModel;
            textBox.SetBinding(TextBox.TextProperty, new Binding("SignalText"));
            textBox.IsEnabled = false;
            textBox.TextChanged += this.ScrollToEnd_TextChanged;

            scrollViewer.Content = textBox;
            this.mainViewModel.SignalBoxes.Add(scrollViewer);

            this.UpdateSignalBoxSummary();            
        }

        public void UpdateSignalBoxSummary()
        {
            this.SignalSummaryGrid.Children.Clear();
            this.SignalSummaryGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < this.mainViewModel.SignalBoxes.Count; i++)
            {
                if (((i / 2) + 1) > this.SignalSummaryGrid.ColumnDefinitions.Count)
                {
                    this.SignalSummaryGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                ScrollViewer signalbox = this.mainViewModel.SignalBoxes[i];
                Grid.SetRow(signalbox, i % 2);
                Grid.SetColumn(signalbox, i / 2);
                this.SignalSummaryGrid.Children.Add(signalbox);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.Stop(true);
            }
        }

        private void StopOneWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.FindWorker(sender).Stop(true);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.workerViewModel.IsTrading = true;
                if (!worker.workerViewModel.IsThreadRunning)
                {
                    worker.Start(true);
                }
            }
        }

        private void StartOneWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.FindWorker(sender).Start(true);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.Stop(true);
            }
        }

        private void StopTradingButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.StopTrading();
            }
        }

        private void StopTradingOneWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.FindWorker(sender).StopTrading();
        }

        private void StopTradingAfterSignalButton_Click(object sender, RoutedEventArgs e)
        {
            this.FindWorker(sender).StopTradingAfterSignal();
        }

        private void AlgorithmFilePathButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.FileName = "Algorithm.dll"; // Default file name
            dlg.DefaultExt = ".dll"; // Default file extension
            dlg.Filter = "Algorithm File (.dll)|*.dll"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                this.mainViewModel.CreationAlgorithmFilePath = dlg.FileName;
            }
        }

        private void ChangeWorkerSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;
            this.ShowSettingsWindow(workerViewModel);
        }

        public void ShowSettingsWindow(WorkerViewModel workerViewModel)
        {
            ChangeWorkerSettingsWindow settingsWindow = new ChangeWorkerSettingsWindow(workerViewModel);
            settingsWindow.Show();
        }

        private void RemoveWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.RemoveWorker(this.FindWorker(sender));
        }

        public void RemoveWorker(Worker worker)
        {
            if (worker != null)
            {
                worker.StopTrading();
                if (worker.didFirst)
                {
                    while (worker.workerViewModel.IsTrading)
                    {
                        Thread.Sleep(50);
                    }
                }
                worker.Stop(true);

                this.mainViewModel.Workers.Remove(worker);
                this.mainViewModel.WorkerViewModels.Remove(worker.workerViewModel);
                XMLHandler.RemoveWorker(worker.workerViewModel.EquityAsString);

                foreach (ScrollViewer signalBox in this.mainViewModel.SignalBoxes)
                {
                    string workerName = worker.workerViewModel.EquityAsString;
                    if (workerName.Equals(signalBox.Name.Substring(0, workerName.Length)))
                    {
                        this.mainViewModel.SignalBoxes.Remove(signalBox);
                        break;
                    }
                }
                this.UpdateSignalBoxSummary();

                WorkerTab workerTab = this.MainTabControl.Items.Cast<TabItem>()
                                                            .Where(item => item.Name.Equals(worker.workerViewModel.EquityAsString))
                                                            .FirstOrDefault() as WorkerTab;
                this.MainTabControl.Items.Remove(workerTab);

                this.workersGrid.Items.Refresh();
            }
        }

        public Worker FindWorker(object sender)
        {
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;

            foreach (Worker tmpWorker in this.mainViewModel.Workers)
            {
                if (tmpWorker.workerViewModel.Equals(workerViewModel))
                {
                    return tmpWorker;
                }
            }

            return null;
        }
    }
}