using HtmlAgilityPack;

namespace Airdropbot
{
    class Program
    {
        private static Dictionary<string, string> detectedAirdrops = new Dictionary<string, string>();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Iniciando monitoramento de airdrops...");

            string airdropsIoUrl = "https://airdrops.io/";
            string airdropsIoXPath = "//div[@class='airdrops-list']"; // Exemplo de XPath

            while (true)
            {
                await CheckAirdropsAsync(airdropsIoUrl, airdropsIoXPath, "Airdrops.io");
                await Task.Delay(3600); // Verifica a cada 1 hora
            }
        }

        private static async Task CheckAirdropsAsync(string url, string xPath, string source)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

                    var response = await client.GetStringAsync(url);
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);

                    var airdrops = htmlDoc.DocumentNode.SelectNodes(xPath);

                    if (airdrops != null)
                    {
                        foreach (var airdrop in airdrops)
                        {
                            // Ajuste o XPath conforme necessário
                            var nameNode = airdrop.SelectSingleNode(".//h3[contains(@class, 'airdrop-name')]");
                            var detailsNode = airdrop.SelectSingleNode(".//p[contains(@class, 'airdrop-details')]");

                            if (nameNode != null && detailsNode != null)
                            {
                                string name = nameNode.InnerText.Trim();
                                string details = detailsNode.InnerText.Trim();

                                // Verifica se o airdrop já foi detectado
                                if (!detectedAirdrops.ContainsKey(name))
                                {
                                    // Se não foi detectado, adiciona ao dicionário e envia notificação
                                    detectedAirdrops[name] = details;
                                    Console.WriteLine($"Novo Airdrop em {source}: {name}\nDetalhes: {details}\n");
                                    await SendTelegramNotificationAsync(name, details, source);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Nenhum airdrop encontrado em {source}.");
                    }
                }
                catch (HttpRequestException httpRequestEx)
                {
                    Console.WriteLine($"Erro ao acessar o site {source}: {httpRequestEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro inesperado ao acessar o site {source}: {ex.Message}");
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