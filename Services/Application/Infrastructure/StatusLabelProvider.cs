using IPNoticeHub.Services.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace IPNoticeHub.Services.Application.Extensions
{
    public sealed class StatusLabelProvider : IStatusLabelProvider
    {
        private readonly Dictionary<int, string> usptoStatusClassLabels = new();

        public StatusLabelProvider(IConfiguration config, IHostEnvironment environment)
        {
            var relativePath = config["StatusCodeCatalogs:USPTO"] ?? "Configurations/uspto-status-codes.json";
            var fullPath = Path.Combine(environment.ContentRootPath, relativePath);

            using var stream = File.OpenRead(fullPath);
            using var doc = JsonDocument.Parse(stream);

            foreach (var item in doc.RootElement.GetProperty("codes").EnumerateArray())
            {
                int statusCode = item.GetProperty("code").GetInt32();
                string? statusLabel = item.GetProperty("label").GetString() ?? $"Status {statusCode}";
                usptoStatusClassLabels[statusCode] = statusLabel;
            }
        }

        public string GetStatusLabel(string source, int statusCode)
        {
            if (string.Equals(source, "USPTO", StringComparison.OrdinalIgnoreCase) &&
                usptoStatusClassLabels.TryGetValue(statusCode, out string? statusLabel))
            {
                return statusLabel;
            }

            return $"Status {statusCode}";
        }
    }
}
