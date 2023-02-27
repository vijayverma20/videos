using Newtonsoft.Json;
using RSP.Dashboard.Services.Interfaces;
using RSP.Dashboard.Services.Models;
using RSP.Dashboard.Services.Services.Shared;

namespace RSP.Dashboard.Services.Services
{
    public class ProductLayerService : BaseHttpService, IProductLayerService
    {
        public ProductLayerService(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<List<AggregateData>> GetAggregatedData(CancellationToken cancellationToken)
        {
            return await Get<List<AggregateData>>("/api/v1/products/AggregateData/search?countOnly=false&exportToExcel=false", cancellationToken);
        }

        public async Task<List<RspPoliciesDto>> GetAllPolicies(CancellationToken cancellationToken)
        {
            var result = await Get<RspSearchResultDto>("/api/v1/products/bondlife/search?countOnly=false&getFullPolicyDetail=true", cancellationToken);
            return result.Policies;
        }

        public async Task<BillableInfo> GetBillableByReferenceNumber(string refNumber, CancellationToken cancellationToken)
        {
            return await Get<BillableInfo>($"/api/v1/products/bondlife/reference/{refNumber}/billable-info", cancellationToken);
        }

        public async Task<RspPoliciesDto> GetPolicyByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
        {
            return await Get<RspPoliciesDto>($"/api/v1/products/bondlife/{policyNumber}", cancellationToken);
        }

        public async Task<List<ProductLayerSliceDetails>> GetPolicySlicesByPolicyNumber(string policyNumber, CancellationToken cancellationToken)
        {
            return await Get<List<ProductLayerSliceDetails>>($"/api/v1/products/bondlife/policies/{policyNumber}/slices", cancellationToken);
        }

        public async Task<DeviatingPayer> UpdateDeviatingPayer(DeviatingPayer dp, CancellationToken cancellationToken)
        {
            return await Put<DeviatingPayer>($"/api/v1/products/bondlife/deviating-payer", dp, cancellationToken);
        }

        public async Task<List<PremiumUpdateHistory>> GetPolicyPremiumUpdateHistory(string policyNumber, CancellationToken cancellationToken)
        {
            return await Get<List<PremiumUpdateHistory>>($"/api/v1/products/bondlife/premium-update/history?policyNumber={policyNumber}", cancellationToken);
        }
    }
}
