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
            ConsoleBoxScrollViewer.ScrollToEnd();
            SignalBoxScrollViewer.ScrollToEnd();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            WorkerTab workerTab = new WorkerTab(this);

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
            worker.Start();
            this.mainViewModel.Workers.Add(worker);

            workerTab.setUpTabWorkerConnection(worker);
            this.mainViewModel.WorkerViewModels.Add(workerTab.workerViewModel);

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

            this.workersGrid.Items.Refresh();

            this.MainTabControl.Items.Insert(this.MainTabControl.Items.Count - 1, workerTab);

            //reset creation-variables
            this.mainViewModel.CreationSymbol = "";
            this.mainViewModel.CreationIsTrading = false;
            this.mainViewModel.CreationAlgorithmFilePath = "Algorithms.dll";
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

        private void StopButton_Click(object sender, RoutedEventArgs e)
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

                WorkerTab workerTab = this.MainTabControl.Items.Cast<TabItem>()
                                                            .Where(item => item.Name.Equals(worker.workerViewModel.EquityAsString))
                                                            .FirstOrDefault() as WorkerTab;
                this.MainTabControl.Items.Remove(workerTab);

                this.workersGrid.Items.Refresh();
            }
        }
    }
}