using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        TextWriter _consoleTextWriter;

        public MainWindow()
        {
            InitializeComponent();

            _consoleTextWriter = new TextBoxStreamWriter(this.ConsoleBox);
            // Redirect the out Console stream
            Console.SetOut(_consoleTextWriter);

            // Undo it again:
            // StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            // standardOutput.AutoFlush = true;
            // Console.SetOut(standardOutput);

            this.mainViewModel.Workers = new List<Worker>();

            Worker worker = new Worker(new Equity("AAPL"),
                                       true,
                                       100000,
                                       "mBar",
                                       "Trades",
                                       100,
                                       1000);
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

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Worker worker = new Worker(new Equity(this.mainViewModel.CreationSymbol),
                                       this.mainViewModel.CreationIsActive,
                                       this.mainViewModel.CreationAmount,
                                       this.mainViewModel.CreationBarSize,
                                       this.mainViewModel.CreationDataType,
                                       this.mainViewModel.CreationPricePremiumPercentage,
                                       this.mainViewModel.CreationCutLoss);
            worker.Start();
            this.mainViewModel.Workers.Add(worker);

            this.workersGrid.Items.Refresh();
        }
    }
}