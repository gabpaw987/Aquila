using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TradingSoftware
{
    /// <summary>
    /// Interaction logic for WorkerTab.xaml
    /// </summary>
    public partial class WorkerTab : TabItem
    {
        private Worker worker;

        public WorkerTab()
        {
            InitializeComponent();
        }

        public void setUpTabWorkerConnection(Worker worker)
        {
            this.worker = worker;
            this.workerGrid.DataContext = new List<WorkerViewModel> { this.workerViewModel };
            this.workerGrid.Items.Refresh();

            this.WorkerTabItem.Header = this.workerViewModel.EquityAsString;
        }

        private void ScrollToEnd_TextChanged(object sender, TextChangedEventArgs e)
        {
            ConsoleBoxScrollViewer.ScrollToEnd();
            SignalBoxScrollViewer.ScrollToEnd();
        }

        private void StopThisWorkerButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.StopTrading();
        }
        
        private void StopAfterSignalButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.StopTradingAfterSignal();
        }

        private void ReenterButton_Click(object sender, RoutedEventArgs e)
        {
            this.worker.shallReenter = true;
        }
    }
}
