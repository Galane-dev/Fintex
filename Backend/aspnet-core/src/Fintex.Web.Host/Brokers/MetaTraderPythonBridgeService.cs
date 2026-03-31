using Abp.Dependency;
using Fintex.Investments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fintex.Web.Host.Brokers
{
    /// <summary>
    /// Host-side bridge that validates MetaTrader logins through a local Python helper.
    /// </summary>
    public class MetaTraderPythonBridgeService : IMetaTraderBridgeService, ITransientDependency
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public MetaTraderPythonBridgeService(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public async Task<MetaTraderConnectionProbeResult> ProbeConnectionAsync(MetaTraderConnectionProbeRequest request)
        {
            var pythonExecutable = _configuration["MetaTraderBridge:PythonExecutable"];
            var scriptPath = _configuration["MetaTraderBridge:ScriptPath"];

            if (string.IsNullOrWhiteSpace(pythonExecutable))
            {
                pythonExecutable = "python";
            }

            if (string.IsNullOrWhiteSpace(scriptPath))
            {
                scriptPath = Path.Combine(_hostingEnvironment.ContentRootPath, "MetaTraderBridge", "mt5_bridge.py");
            }

            if (!File.Exists(scriptPath))
            {
                return new MetaTraderConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "The MetaTrader bridge script could not be found on the server."
                };
            }

            try
            {
                var payload = JsonSerializer.Serialize(new
                {
                    login = request.AccountLogin,
                    password = request.Password,
                    server = request.Server,
                    terminalPath = request.TerminalPath
                });

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExecutable,
                    Arguments = Quote(scriptPath),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    await process.StandardInput.WriteAsync(payload);
                    process.StandardInput.Close();

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    var exited = process.WaitForExit(30000);

                    if (!exited)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignored
                        }

                        return new MetaTraderConnectionProbeResult
                        {
                            IsSuccess = false,
                            Error = "The MetaTrader bridge timed out while validating the login."
                        };
                    }

                    var output = await outputTask;
                    var error = await errorTask;

                    if (!string.IsNullOrWhiteSpace(error) && string.IsNullOrWhiteSpace(output))
                    {
                        return new MetaTraderConnectionProbeResult
                        {
                            IsSuccess = false,
                            Error = error.Trim()
                        };
                    }

                    return ParseBridgeResult(output, error);
                }
            }
            catch (Exception exception)
            {
                return new MetaTraderConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "MetaTrader bridge error: " + exception.Message
                };
            }
        }

        private static MetaTraderConnectionProbeResult ParseBridgeResult(string output, string error)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return new MetaTraderConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = string.IsNullOrWhiteSpace(error)
                        ? "The MetaTrader bridge returned no response."
                        : error.Trim()
                };
            }

            try
            {
                using (var document = JsonDocument.Parse(output))
                {
                    var root = document.RootElement;
                    var isSuccess = root.TryGetProperty("success", out var successElement) &&
                                    successElement.ValueKind == JsonValueKind.True;

                    if (!isSuccess)
                    {
                        return new MetaTraderConnectionProbeResult
                        {
                            IsSuccess = false,
                            Error = GetString(root, "error") ?? error ?? "The MetaTrader login failed."
                        };
                    }

                    var account = root.TryGetProperty("account", out var accountElement)
                        ? accountElement
                        : default(JsonElement);

                    return new MetaTraderConnectionProbeResult
                    {
                        IsSuccess = true,
                        AccountLogin = GetString(account, "login"),
                        AccountName = GetString(account, "name"),
                        Server = GetString(account, "server"),
                        Company = GetString(account, "company"),
                        Currency = GetString(account, "currency"),
                        Leverage = GetNullableInt(account, "leverage"),
                        Balance = GetNullableDecimal(account, "balance"),
                        Equity = GetNullableDecimal(account, "equity")
                    };
                }
            }
            catch (Exception exception)
            {
                return new MetaTraderConnectionProbeResult
                {
                    IsSuccess = false,
                    Error = "The MetaTrader bridge returned an unreadable response: " + exception.Message
                };
            }
        }

        private static string GetString(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty(propertyName, out var property) ||
                property.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            return property.ToString();
        }

        private static int? GetNullableInt(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            {
                return value;
            }

            return null;
        }

        private static decimal? GetNullableDecimal(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object ||
                !element.TryGetProperty(propertyName, out var property))
            {
                return null;
            }

            if (property.ValueKind == JsonValueKind.Number)
            {
                if (property.TryGetDecimal(out var decimalValue))
                {
                    return decimalValue;
                }

                if (property.TryGetDouble(out var doubleValue))
                {
                    return Convert.ToDecimal(doubleValue);
                }
            }

            return null;
        }

        private static string Quote(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}
