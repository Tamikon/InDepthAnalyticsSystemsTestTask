using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using Dapper;
using Npgsql;
using TestApi.Models;

namespace TestApi.Services;

public partial class PageProcessingService : IPageProcessingService
{
    private readonly string _connectionString;
    private readonly ILogger<PageProcessingService> _logger;

    [GeneratedRegex(@"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex EmailRegex();

    public PageProcessingService(Microsoft.Extensions.Configuration.IConfiguration configuration, ILogger<PageProcessingService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres";
        _logger = logger;
    }

    public async Task<ResponseModel> ProcessAsync(RequestModel request)
    {
        var response = new ResponseModel
        {
            IsError = 0,
            ErrorCode = null,
            ErrorMessage = null,
            ElementsCount = 0,
            EmailsCount = 0,
            Url = null,
            DecryptedPlainText = null,
            ElementsAttrList = new List<string>(),
            EmailsList = new List<string>()
        };

        try
        {
            string url;
            string pageHtml;
            try
            {
                url = Encoding.UTF8.GetString(Convert.FromBase64String(request.Url_b64!));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("BASE64_URL_DECODE_ERROR", $"Error decoding base64 URL: {ex.Message}");
            }

            try
            {
                pageHtml = Encoding.UTF8.GetString(Convert.FromBase64String(request.Page_b64!));
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("BASE64_PAGE_DECODE_ERROR", $"Error decoding base64 Page: {ex.Message}");
            }

            response.Url = url;

            string decryptedText;
            try
            {
                decryptedText = DecryptAes256Ecb(request.Encrypted_text_bytes_b64!, request.Key_bytes_b64!);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse("DECRYPTION_ERROR", $"Error decrypting text: {ex.Message}");
            }
            response.DecryptedPlainText = decryptedText;

            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(pageHtml));

            var elements = document.QuerySelectorAll(request.Selector!);
            response.ElementsCount = elements.Length;

            var attrList = new List<string>();
            var entitiesToSave = new List<ElementEntity>();

            foreach (var el in elements)
            {
                var attrValue = el.GetAttribute(request.Attribute!) ?? string.Empty;
                attrList.Add(attrValue);

                entitiesToSave.Add(new ElementEntity
                {
                    AttributeValue = attrValue,
                    HtmlCode = el.OuterHtml
                });
            }
            response.ElementsAttrList = attrList;

            try
            {
                await SaveElementsAsync(entitiesToSave);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save elements to DB, continuing without DB write");
            }

            var emailMatches = EmailRegex().Matches(pageHtml);
            var emails = emailMatches.Select(m => m.Value).ToList();
            response.EmailsCount = emails.Count;
            response.EmailsList = emails;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during processing");
            return CreateErrorResponse("INTERNAL_ERROR", ex.Message);
        }
    }

    private static ResponseModel CreateErrorResponse(string code, string message)
    {
        return new ResponseModel
        {
            IsError = 1,
            ErrorCode = code,
            ErrorMessage = message,
            ElementsCount = 0,
            EmailsCount = 0,
            Url = null,
            DecryptedPlainText = null,
            ElementsAttrList = new List<string>(),
            EmailsList = new List<string>()
        };
    }

    private static string DecryptAes256Ecb(string cipherTextB64, string keyB64)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherTextB64);
        byte[] keyBytes = Convert.FromBase64String(keyB64);

        if (keyBytes.Length != 32)
            throw new ArgumentException($"Key must be 32 bytes for AES-256, got {keyBytes.Length}");

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;

        using var decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private async Task SaveElementsAsync(List<ElementEntity> entities)
    {
        if (entities.Count == 0) return;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS elements (
                id BIGSERIAL PRIMARY KEY,
                attribute_value TEXT NOT NULL,
                html_code TEXT NOT NULL
            );";
        await connection.ExecuteAsync(createTableSql);

        const string insertSql = @"
            INSERT INTO elements (attribute_value, html_code)
            VALUES (@AttributeValue, @HtmlCode);";

        await connection.ExecuteAsync(insertSql, entities);
    }
}
