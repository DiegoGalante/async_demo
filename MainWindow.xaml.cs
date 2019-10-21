using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace async_sample
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void executeSync_Click(object sender, RoutedEventArgs e)
        {
            AppArrowLoading();
            HabilitaItens(false, false);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            //var results = DemoMethods.RunDownloadSync();
            var results = DemoMethods.RunDownloadParallelSync();
            PrintResults(results);

            watch.Stop();
            var elapseMS = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Tempo total da execução: {elapseMS}";

            AppArrowDefault();
            HabilitaItens();
        }

        //Não é necessário mudar este void para Task pq ele é um evento.
        private async void executeAsync_Click(object sender, RoutedEventArgs e)
        {
            AppArrowLoading();
            HabilitaItens();

            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += ReportProgress;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var results = await DemoMethods.RunDownloadAsync(progress, cancellationTokenSource.Token);
                PrintResults(results);
            }
            catch (OperationCanceledException)
            {
                resultsWindow.Text += $"O download assíncrono foi cancelado.{Environment.NewLine}";
            }

            watch.Stop();
            var elapseMS = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Tempo total da execução: {elapseMS}{Environment.NewLine}";
            AppArrowDefault();
        }

        
        private void ReportProgress(object sender, ProgressReportModel e)
        {
            dashboardProgress.Value = e.PercentageComplete;
            PrintResults(e.SitesDownloaded);
        }

        //Não é necessário mudar este void para Task pq ele é um evento.
        private async void executeAsyncParallel_Click(object sender, RoutedEventArgs e)
        {
            AppArrowLoading();
            HabilitaItens(false);

            Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
            progress.ProgressChanged += ReportProgress;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                //var results = await DemoMethods.RunDownloadParallelAsync();
                var results = await DemoMethods.RunDownloadParallelAsyncV2(progress);
                PrintResults(results);
            }
            catch (OperationCanceledException)
            {
                resultsWindow.Text += $"O download assíncrono em paralelo foi cancelado.{Environment.NewLine}";
            }

            watch.Stop();
            var elapseMS = watch.ElapsedMilliseconds;

            resultsWindow.Text += $"Tempo total da execução: {elapseMS}{Environment.NewLine}";

            AppArrowDefault();
            HabilitaItens(true);
        }

        private void cancelOperation_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private void PrintResults(List<WebsiteDataModel> results)
        {
            resultsWindow.Text = "";
            foreach (var data in results)
            {
                resultsWindow.Text += $"{data.WebsiteUrl} downloaded: {data.WebsiteData.Length} characteres long. {Environment.NewLine}";
            }
        }

        internal void AppArrowLoading()
        {
            this.Cursor = Cursors.Wait;
        }

        internal void AppArrowDefault()
        {
            this.Cursor = Cursors.Arrow;
        }

        internal void HabilitaItens(bool btnCancelar = true, bool progressBar = true, int progresso = 0)
        {
            cancelOperation.IsEnabled = btnCancelar;
            dashboardProgress.IsEnabled = progressBar;
            dashboardProgress.Value = progresso;
        }
    }
}
