using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using CargaProdutos.Models;
using CargaProdutos.Clients;

namespace CargaProdutos
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile($"appsettings.json");
            var config = builder.Build();

            var jsonOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(
                    config.GetSection("APIProdutos_Access:UrlBase").Value);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Envio da requisição a fim de autenticar
                // e obter o token de acesso
                HttpResponseMessage respToken = client.PostAsync(
                    "login", new StringContent(
                        JsonSerializer.Serialize(new
                        {
                            UserID = config.GetSection("APIProdutos_Access:UserID").Value,
                            Password = config.GetSection("APIProdutos_Access:Password").Value
                        }), Encoding.UTF8, "application/json")).Result;

                string conteudo =
                    respToken.Content.ReadAsStringAsync().Result;
                Console.WriteLine(conteudo);

                if (respToken.StatusCode == HttpStatusCode.OK)
                {
                    Token token = JsonSerializer.Deserialize<Token>(
                        conteudo, jsonOptions);
                    if (token.Authenticated)
                    {
                        // Associar o token aos headers do objeto
                        // do tipo HttpClient
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", token.AccessToken);

                        var apiProdutoClient =
                            new APIProdutoClient(client, jsonOptions);

                        apiProdutoClient.IncluirProduto(
                            new Produto()
                            {
                                CodigoBarras = "00003",
                                Nome = "Teste Produto 03",
                                Preco = 30.33
                            });

                        apiProdutoClient.IncluirProduto(
                            new Produto()
                            {
                                CodigoBarras = "00004",
                                Nome = "Teste Produto 04",
                                Preco = 44.04
                            });

                        Console.WriteLine("Produtos cadastrados: " +
                            JsonSerializer.Serialize(
                                apiProdutoClient.ListarProdutos()));
                    }
                }
            }

            Console.WriteLine("\nFinalizado!");
        }
    }
}