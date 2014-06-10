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

namespace TradingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.mainViewModel.Workers = new List<Worker>();
            this.mainViewModel.WorkerViewModels = new List<WorkerViewModel>();
            this.mainViewModel.SignalBoxes = new List<ScrollViewer>();
            this.mainViewModel.CreationAlgorithmFilePath = "Algorithms.dll";

            XMLHandler.CreateSettingsFileIfNecessary();
            XMLHandler.LoadWorkersFromXML(this);
        }

        /*
         *To autogenerate columns for all properties again use AutoGeneratingColumn="OnAutoGeneratingColumn" in datagrid plus this method
        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
        }
        */

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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerTab workerTab = new WorkerTab(this);

            XMLHandler.CreateWorker(this.mainViewModel.CreationSymbol,
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
            this.mainViewModel.CreationAlgorithmFilePath = "Algorithms.dll";
        }

        public void AddSignalBoxToSummary(WorkerViewModel workerViewModel)
        {
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Name = workerViewModel.EquityAsString + "_scrollViewer";
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

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
                worker.Stop();
            }
        }

        private void ReenterButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                if (worker.workerViewModel.Equals(workerViewModel))
                {
                    worker.shallReenter = true;
                }
            }
        }

        private void StopOneWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                if (worker.workerViewModel.Equals(workerViewModel))
                {
                    worker.StopTrading();
                }
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.workerViewModel.IsTrading = true;
                if (!worker.IsRunning())
                {
                    worker.Start();
                }
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.Stop();
            }
        }

        private void StopTradingButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                worker.StopTrading();
            }
        }

        private void StopAfterSignalButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;
            foreach (Worker worker in this.mainViewModel.Workers)
            {
                if (worker.workerViewModel.Equals(workerViewModel))
                {
                    worker.StopTradingAfterSignal();
                }
            }
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
            WorkerViewModel workerViewModel = ((FrameworkElement)sender).DataContext as WorkerViewModel;

            Worker worker = null;

            foreach (Worker tmpWorker in this.mainViewModel.Workers)
            {
                if (tmpWorker.workerViewModel.Equals(workerViewModel))
                {
                    worker = tmpWorker;
                }
            }

            this.RemoveWorker(worker);
        }

        public void RemoveWorker(Worker worker)
        {
            if (worker != null)
            {
                worker.StopTrading();
                while (worker.workerViewModel.IsTrading)
                {
                    Thread.Sleep(50);
                }
                worker.Stop();

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
    }
}