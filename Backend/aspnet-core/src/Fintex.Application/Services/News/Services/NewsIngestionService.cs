using Abp.Dependency;
using Abp.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;

namespace Fintex.Investments.News
{
    public partial class NewsIngestionService : FintexAppServiceBase, INewsIngestionService, ITransientDependency
    {
        private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);

        private static readonly string[] BitcoinKeywords =
        {
            "bitcoin", "btc", "crypto", "cryptocurrency", "spot etf", "etf", "mining", "stablecoin"
        };

        private static readonly string[] UsdKeywords =
        {
            "federal reserve", "fed", "dollar", "usd", "inflation", "cpi", "ppi", "payroll", "nfp", "rate", "rates", "yield", "treasury"
        };

        private readonly IRepository<NewsSource, long> _newsSourceRepository;
        private readonly IRepository<NewsArticle, long> _newsArticleRepository;
        private readonly IRepository<NewsRefreshRun, long> _newsRefreshRunRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NewsIngestionService(
            IRepository<NewsSource, long> newsSourceRepository,
            IRepository<NewsArticle, long> newsArticleRepository,
            IRepository<NewsRefreshRun, long> newsRefreshRunRepository,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _newsSourceRepository = newsSourceRepository;
            _newsArticleRepository = newsArticleRepository;
            _newsRefreshRunRepository = newsRefreshRunRepository;
            _httpClient = httpClient;
            _configuration = configuration;
        }
    }
}
