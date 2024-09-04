using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MailKit.Net.Smtp;
using MimeKit;

namespace Airdropbot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Iniciando monitoramento de airdrops...");

            string airdropsIoUrl = "https://airdrops.io/";
            string coinMarketCapAirdropUrl = "https://coinmarketcap.com/airdrop/";

            while (true)
            {
                await CheckAirdropsAsync(airdropsIoUrl, "//div[@class='airdrops-list']", "Airdrops.io");
                await CheckAirdropsAsync(coinMarketCapAirdropUrl, "//div[contains(@class, 'content__row')]", "CoinMarketCap Airdrop");

                //await Task.Delay(3600000); // Verifica a cada 1 hora
            }
        }

        private static async Task CheckAirdropsAsync(string url, string xPath, string source)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Configurar cabeçalhos para simular um navegador real
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                    var response = await client.GetStringAsync(url);
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    var airdrops = htmlDoc.DocumentNode.SelectNodes(xPath);

                    if (airdrops != null)
                    {
                        foreach (var airdrop in airdrops)
                        {
                            var nameNode = airdrop.SelectSingleNode(".//h3") ?? airdrop.SelectSingleNode(".//span[contains(@class, 'title')]"); // Ajuste para CoinMarketCap
                            var detailsNode = airdrop.SelectSingleNode(".//p") ?? airdrop.SelectSingleNode(".//div[contains(@class, 'sc-1eb5slv-0')]"); // Ajuste para CoinMarketCap

                            if (nameNode != null && detailsNode != null)
                            {
                                string name = nameNode.InnerText.Trim();
                                string details = detailsNode.InnerText.Trim();
                                Console.WriteLine($"Novo Airdrop em {source}: {name}\nDetalhes: {details}\n");

                                // Envia notificação pelo Telegram
                                await SendTelegramNotificationAsync(name, details, source);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Nenhum airdrop encontrado em {source}.");
                        await CheckCoinMarketCapAirdropsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao acessar o site {source}: {ex.Message}");
                }
            }
        }

        private static async Task CheckCoinMarketCapAirdropsAsync()
        {
            string apiKey = "145bcacc-f453-435b-81f4-e4a4f0cf1e8c"; // Substitua pela sua chave API do CoinMarketCap
            string url = "https://pro-api.coinmarketcap.com/v1/cryptocurrency/airdrops";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", apiKey);
                    client.DefaultRequestHeaders.Add("Accepts", "application/json");

                    var response = await client.GetStringAsync(url);

                    // Processar a resposta JSON aqui
                    Console.WriteLine(response);

                    // Exemplo de processamento de JSON pode ser adicionado aqui

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao acessar o CoinMarketCap API: {ex.Message}");
                }
            }
        }

        private static async Task SendTelegramNotificationAsync(string name, string details, string source)
        {
            string botToken = "7522282948:AAEeauXeEu2jMEx3SvFiPJEjemQX5jc_gqk"; // Substitua pelo token do seu bot
            string chatId = "1029061795"; // Substitua pelo seu chat ID
            string message = $"Novo Airdrop em {source}!\nNome: {name}\nDetalhes: {details}";

            string url = $"https://api.telegram.org/bot{botToken}/sendMessage?chat_id={chatId}&text={Uri.EscapeDataString(message)}";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    await client.GetAsync(url);
                    Console.WriteLine("Notificação enviada via Telegram com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao enviar notificação pelo Telegram: {ex.Message}");
                }
            }
        }
    }
}