using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Krs.Ats.IBNet.Contracts;

namespace TradingSoftware
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //TextWriter _consoleTextWriter;

        public MainWindow()
        {
            InitializeComponent();

            //_consoleTextWriter = new TextBoxStreamWriter(this.ConsoleBox);
            // Redirect the out Console stream
            //Console.SetOut(_consoleTextWriter);

            // Undo it again:
            // StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            // standardOutput.AutoFlush = true;
            // Console.SetOut(standardOutput);

            this.mainViewModel.Workers = new List<Worker>();

            Worker worker = new Worker(this.mainViewModel,
                                        new Equity("NQM4"),
                                        true,
                                        250000,
                                        "mBar",
                                        "Trades",
                                        100,
                                        100,
                                        true,
                                        0,
                                        true);
            worker.Start();

            this.mainViewModel.Workers.Add(worker);

            this.workersGrid.DataContext = this.mainViewModel.Workers;
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
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
            ConsoleBoxScrollViewer.ScrollToEnd();
            SignalBoxScrollViewer.ScrollToEnd();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: block creation if something is not filled out
            Worker worker = new Worker(this.mainViewModel,
                                       new Equity(this.mainViewModel.CreationSymbol),
                                       this.mainViewModel.CreationIsTrading,
                                       this.mainViewModel.CreationAmount,
                                       this.mainViewModel.CreationBarSize,
                                       this.mainViewModel.CreationDataType,
                                       this.mainViewModel.CreationPricePremiumPercentage,
                                       this.mainViewModel.CreationRoundLotSize,
                                       this.mainViewModel.CreationIsFuture,
                                       this.mainViewModel.CreationCurrentPosition,
                                       this.mainViewModel.CreationShallIgnoreFirstSignal);
            worker.Start();
            this.mainViewModel.Workers.Add(worker);

            this.workersGrid.Items.Refresh();
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