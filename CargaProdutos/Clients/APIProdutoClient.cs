using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using CargaProdutos.Models;

namespace CargaProdutos.Clients
{
    public class  APIProdutoClient
    {
        private HttpClient _client;
        private JsonSerializerOptions _jsonOptions;

        public APIProdutoClient(HttpClient client,
            JsonSerializerOptions jsonOptions)
        {
            _client = client;
            _jsonOptions = jsonOptions;
        }

        public void IncluirProduto(Produto produto)
        {
            HttpResponseMessage response = _client.PostAsync(
                "produtos", new StringContent(
                    JsonSerializer.Serialize(produto),
                    Encoding.UTF8, "application/json")).Result;

            Console.WriteLine(
                response.Content.ReadAsStringAsync().Result);
        }

        public List<Produto> ListarProdutos()
        {
            HttpResponseMessage response = _client.GetAsync(
                "produtos").Result;

            List<Produto> resultado = null;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string conteudo = response.Content.ReadAsStringAsync().Result;
                resultado = JsonSerializer
                    .Deserialize<List<Produto>>(conteudo, _jsonOptions);
            }
            else
                Console.WriteLine("Token provavelmente expirado!");

            return resultado;
        }        
    }
}