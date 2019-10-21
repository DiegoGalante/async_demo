using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace async_sample
{
    public static class DemoMethods
    {
        /// <summary>
        /// Lista simpes de string.
        /// </summary>
        /// <returns></returns>
        private static List<string> PrepData()
        {
            //List<string> retorno = new List<string>();

            //retorno.Add("https://www.yahoo.com");
            //retorno.Add("https://www.google.com");
            //retorno.Add("https://www.microsoft.com");
            //retorno.Add("https://www.cnn.com");
            //retorno.Add("https://www.codeproject.com");
            //retorno.Add("https://www.stackoverflow.com");

            //return retorno;

            return new List<string> {
                                        "https://www.yahoo.com",
                                        "https://www.google.com",
                                        "https://www.microsoft.com",
                                        "https://www.cnn.com",
                                        "https://www.codeproject.com",
                                        "https://www.stackoverflow.com"
                                    };
        }

        /// <summary>
        /// Executa o metodo tradicional de forma síncrona.
        /// </summary>
        /// <returns></returns>
        public static List<WebsiteDataModel> RunDownloadSync()
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();

            foreach (string site in websites)
            {
                WebsiteDataModel results = DownloadWebSite(site);
                output.Add(results);
            }

            return output;
        }

        /// <summary>
        /// Execute o método tradicional de forma síncrona utilizando o Parallel.ForEach para otimizar/acelerar a execução já que este foreach executa de forma paralela em threads diferentes.
        /// </summary>
        /// <returns></returns>
        public static List<WebsiteDataModel> RunDownloadParallelSync()
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();

            //Obs: pode ser que as vezes seja necessário limitar um pouco o número de threads que vai ser utilizado.
            //var options = new ParallelOptions() { MaxDegreeOfParallelism = 6 };

            //Este Parallel.ForEach significa que ele faz o download dos sites de moto paralelo, ou seja, utiliza outras threads 
            Parallel.ForEach<string>(websites/*, options*/, (site) =>
             {
                 WebsiteDataModel results = DownloadWebSite(site);
                 output.Add(results);
             });

            return output;
        }

        /// <summary>
        /// Executa o método de forma assíncrona e usando também de forma assíncrona o Parallel.ForEach
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>
        /// Retorna uma lista List<WebsiteDataModel>.
        /// </returns>
        public async static Task<List<WebsiteDataModel>> RunDownloadParallelAsyncV2(IProgress<ProgressReportModel> progress)
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();
            ProgressReportModel report = new ProgressReportModel();

            //Obs: pode ser que as vezes seja necessário limitar um pouco o número de threads que vai ser utilizado.
            //var options = new ParallelOptions() { MaxDegreeOfParallelism = 6 };

            await Task.Run(() =>
            Parallel.ForEach<string>(websites/*, options*/, (site) =>
              {
                  WebsiteDataModel results = DownloadWebSite(site);
                  output.Add(results);

                  report.SitesDownloaded = output;
                  report.PercentageComplete = (output.Count * 100) / websites.Count;
                  progress.Report(report);
              })

            );
            return output;
        }


        /// <summary>
        /// Executa o método assíncrono, porém utilizando foreach normal.
        /// Obs: Quando o método retorna void. Por convenção, utiliza-se o Task no lugar do void.
        /// </summary>
        /// <returns>
        /// Retorna uma lista List<WebsiteDataModel>.
        /// </returns>
        public static async Task<List<WebsiteDataModel>> RunDownloadAsync(IProgress<ProgressReportModel> progress, CancellationToken cancellationToken)
        {
            List<string> websites = PrepData();
            List<WebsiteDataModel> output = new List<WebsiteDataModel>();
            ProgressReportModel report = new ProgressReportModel();

            foreach (string site in websites)
            {
                //Quando não se tem controle sobre o metodo síncrono, utiliza-se await Task.Run(() => nome_do_metodo).
                //WebsiteDataModel results = await Task.Run(() => DownloadWebSite(site));
                WebsiteDataModel results = await DownloadWebSiteAsync(site);
                output.Add(results);

                cancellationToken.ThrowIfCancellationRequested();

                report.SitesDownloaded = output;
                report.PercentageComplete = (output.Count * 100) / websites.Count;
                progress.Report(report);
            }

            return output;
        }

        /// <summary>
        /// Executa o método assíncrono, utilizando foreach normal e esperando a resposta do Task.WhenAll antes de retornar.
        /// Obs: Quando o método retorna void. Por convenção, utiliza-se o Task no lugar do void.
        /// </summary>
        /// <returns>
        /// Retorna uma lista List<WebsiteDataModel>.
        /// </returns>
        public static async Task<List<WebsiteDataModel>> RunDownloadParallelAsync()
        {
            List<string> websites = PrepData();
            List<Task<WebsiteDataModel>> tasks = new List<Task<WebsiteDataModel>>();

            foreach (string site in websites)
                tasks.Add(DownloadWebSiteAsync(site));

            //WhenAll executa/espera todo o procedimento das tasks antes de jogar na variavel results.
            var results = await Task.WhenAll(tasks);

            return new List<WebsiteDataModel>(results);
        }

        private static async Task<WebsiteDataModel> DownloadWebSiteAsync(string siteUrl)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();

            output.WebsiteUrl = siteUrl;
            output.WebsiteData = await client.DownloadStringTaskAsync(siteUrl);

            return output;
        }
        private static WebsiteDataModel DownloadWebSite(string siteUrl)
        {
            WebsiteDataModel output = new WebsiteDataModel();
            WebClient client = new WebClient();

            output.WebsiteUrl = siteUrl;
            output.WebsiteData = client.DownloadString(siteUrl);

            return output;
        }


    }
}
