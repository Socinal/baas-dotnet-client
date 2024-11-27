using System.Text.Json;
using System.Text;
using System.Security.Cryptography;
using System.Net.Http.Json;

public record class TokenResponse(string token);

internal class Program
{
    private static async Task Main(string[] args)
    {
        var tokenResponse = await SendRequest<TokenResponse>("sessao", new
        {
            email = "user@email.com",
            senha = "123123"
        });
        var requestBody = new
        {
            ChaveDoRecebedor = "999.999.999-99",
            ValorDaOperacao = 9.9,
            InformacaoAdicional = "A000..."
        };
        var response = await SendRequest<string>("contas_correntes/0000001-9/transferencias_pix", requestBody, tokenResponse.token);

        Console.WriteLine(response);
    }

    static async Task<T> SendRequest<T>(string path, object body, string token = null)
    {
        var rawBody = JsonSerializer.Serialize(body);
        var client = new HttpClient()
        {
            BaseAddress = new Uri("https://baas-staging.socinal.com.br/api/v2/")
        };

        if (token != null)
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("Signature", SignBody(rawBody));
        }

        var content = new StringContent(rawBody, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(path, content);

        Console.WriteLine(response.RequestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
        }

        return await response.EnsureSuccessStatusCode().Content.ReadFromJsonAsync<T>();
    }

    static string SignBody(string rawBody)
    {
        var privateKey = ECDsaOpenSsl.Create();
        var privateKeyContent = new StreamReader("./chave_privada.pem").ReadToEnd();

        privateKey.ImportFromPem(privateKeyContent);

        var signature = privateKey.SignData(
            Encoding.UTF8.GetBytes(rawBody),
            HashAlgorithmName.SHA256,
            DSASignatureFormat.Rfc3279DerSequence
        );

        return Convert.ToBase64String(signature);
    }
}
