using System.Threading.Tasks;

namespace Fintex.Investments.Academy
{
    public interface IAcademyProgressService
    {
        Task<AcademyProgressState> GetStatusAsync(long userId, int? tenantId);

        Task EnsureTradeAcademyAccessAsync(long userId, int? tenantId);

        Task EnsureExternalBrokerAccessAsync(long userId, int? tenantId);
    }
}
