using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Runtime.Session;
using Abp.Runtime.Security;
using Abp.UI;
using Fintex.Investments.Academy;
using Fintex.Investments.Brokers.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fintex.Investments.Brokers
{
    [AbpAuthorize]
    public class ExternalBrokerAppService : FintexAppServiceBase, IExternalBrokerAppService
    {
        private readonly IExternalBrokerConnectionRepository _externalBrokerConnectionRepository;
        private readonly IAlpacaBrokerService _alpacaBrokerService;
        private readonly IAcademyProgressService _academyProgressService;

        public ExternalBrokerAppService(
            IExternalBrokerConnectionRepository externalBrokerConnectionRepository,
            IAlpacaBrokerService alpacaBrokerService,
            IAcademyProgressService academyProgressService)
        {
            _externalBrokerConnectionRepository = externalBrokerConnectionRepository;
            _alpacaBrokerService = alpacaBrokerService;
            _academyProgressService = academyProgressService;
        }

        public async Task<ListResultDto<ExternalBrokerConnectionDto>> GetMyConnectionsAsync()
        {
            var userId = AbpSession.GetUserId();
            var connections = await _externalBrokerConnectionRepository.GetForUserAsync(userId);
            return new ListResultDto<ExternalBrokerConnectionDto>(
                ObjectMapper.Map<List<ExternalBrokerConnectionDto>>(connections));
        }

        public async Task<ExternalBrokerConnectionDto> ConnectAlpacaAccountAsync(ConnectAlpacaBrokerAccountInput input)
        {
            await _academyProgressService.EnsureExternalBrokerAccessAsync(AbpSession.GetUserId(), AbpSession.TenantId);
            var probeResult = await _alpacaBrokerService.ProbeConnectionAsync(new AlpacaConnectionProbeRequest
            {
                ApiKey = input.ApiKey,
                ApiSecret = input.ApiSecret,
                IsPaperEnvironment = input.IsPaperEnvironment
            });

            if (!probeResult.IsSuccess)
            {
                throw new UserFriendlyException(probeResult.Error ?? "The Alpaca credentials could not be validated.");
            }

            var userId = AbpSession.GetUserId();
            var encryptedPassword = SimpleStringCipher.Instance.Encrypt(input.ApiSecret);
            var connection = await _externalBrokerConnectionRepository.GetByUserAndLoginAsync(
                userId,
                ExternalBrokerProvider.Alpaca,
                input.ApiKey,
                probeResult.Endpoint);

            if (connection == null)
            {
                connection = new ExternalBrokerConnection(
                    AbpSession.TenantId,
                    userId,
                    input.DisplayName,
                    ExternalBrokerProvider.Alpaca,
                    ExternalBrokerPlatform.DirectApi,
                    input.ApiKey,
                    probeResult.Endpoint,
                    encryptedPassword,
                    null);

                await _externalBrokerConnectionRepository.InsertAsync(connection);
            }
            else
            {
                connection.UpdateCredentials(
                    input.DisplayName,
                    probeResult.Endpoint,
                    encryptedPassword,
                    null);
            }

            connection.MarkConnected(
                probeResult.AccountNumber,
                probeResult.Currency,
                probeResult.Company,
                probeResult.Multiplier,
                probeResult.Cash,
                probeResult.Equity,
                DateTime.UtcNow);

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<ExternalBrokerConnectionDto>(connection);
        }

        public async Task DisconnectAsync(EntityDto<long> input)
        {
            var userId = AbpSession.GetUserId();
            var connection = await _externalBrokerConnectionRepository.GetByIdForUserAsync(input.Id, userId);
            if (connection == null)
            {
                throw new UserFriendlyException("The external broker connection could not be found.");
            }

            connection.Disconnect(DateTime.UtcNow);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
