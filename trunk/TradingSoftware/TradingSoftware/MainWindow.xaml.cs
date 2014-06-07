using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Krs.Ats.IBNet.Contracts;
using System.IO;

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

            XMLHandler.CreateSettingsFileIfNecessary();
            List<WorkerTab> newWorkerTabs = XMLHandler.LoadWorkersFromXML(this.mainViewModel);

            this.workersGrid.Items.Refresh();

            if (newWorkerTabs.Count > 0)
            {
                foreach (WorkerTab workerTab in newWorkerTabs)
                {
                    this.MainTabControl.Items.Insert(this.MainTabControl.Items.Count - 1, workerTab);
                }
            }

            /*Worker worker = new Worker(this.mainViewModel,
                                        "NQM4",
                                        true,
                                        250000,
                                        "mBar",
                                        "Trades",
                                        100,
                                        100,
                                        true,
                                        0,
                                        true,
                                        true);
            worker.Start();
                        
            this.mainViewModel.Workers.Add(worker);

            // Second worker
            Worker worker2 = new Worker(this.mainViewModel,
                                        "ESM4",
                                        false,
                                        250000,
                                        "mBar",
                                        "Trades",
                                        100,
                                        100,
                                        true,
                                        0,
                                        true,
                                        true);
            worker2.Start();

            this.mainViewModel.Workers.Add(worker2);
            */

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
            WorkerTab workerTab = new WorkerTab();

            // TODO: block creation if something is not filled out
            // TODO: GUI for algorithmFilePath
            Worker worker = new Worker(this.mainViewModel,
                                       workerTab.workerViewModel,
                                       this.mainViewModel.CreationSymbol,
                                       this.mainViewModel.CreationIsTrading,
                                       this.mainViewModel.CreationBarSize,
                                       this.mainViewModel.CreationDataType,
                                       this.mainViewModel.CreationPricePremiumPercentage,
                                       this.mainViewModel.CreationRoundLotSize,
                                       this.mainViewModel.CreationIsFuture,
                                       this.mainViewModel.CreationCurrentPosition,
                                       this.mainViewModel.CreationShallIgnoreFirstSignal,
                                       this.mainViewModel.CreationHasAlgorithmParameters,
                                       this.mainViewModel.CreationAlgorithmFilePath);
            worker.Start();
            this.mainViewModel.Workers.Add(worker);

            workerTab.setUpTabWorkerConnection(worker);
            this.mainViewModel.WorkerViewModels.Add(workerTab.workerViewModel);

            XMLHandler.CreateWorker(this.mainViewModel.CreationSymbol,
                                    this.mainViewModel.CreationIsTrading,
                                    this.mainViewModel.CreationBarSize,
                                    this.mainViewModel.CreationDataType,
                                    "tobechanged",//this.mainViewModel.CreationAlgorithmFilePath,
                                    this.mainViewModel.CreationPricePremiumPercentage,
                                    this.mainViewModel.CreationIsFuture,
                                    this.mainViewModel.CreationCurrentPosition,
                                    this.mainViewModel.CreationShallIgnoreFirstSignal,
                                    this.mainViewModel.CreationHasAlgorithmParameters,
                                    this.mainViewModel.CreationRoundLotSize,
                                    "tobechanged");

            this.workersGrid.Items.Refresh();

            this.MainTabControl.Items.Insert(this.MainTabControl.Items.Count - 1, workerTab);
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
            Worker worker = ((FrameworkElement)sender).DataContext as Worker;
            worker.shallReenter = true;
        }

        private void StopOneWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            Worker worker = ((FrameworkElement)sender).DataContext as Worker;
            worker.StopTrading();
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
            Worker worker = ((FrameworkElement)sender).DataContext as Worker;
            worker.StopTradingAfterSignal();
        }
    }
}