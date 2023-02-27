using Newtonsoft.Json;
using System.Text;
using RSP.Dashboard.Services.Models;

namespace RSP.Dashboard.Services.Services.Shared
{
    public abstract class BaseHttpService
    {
        readonly HttpClient httpClient;

        public BaseHttpService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public event Action OnNoRegionError;

        public async virtual Task<T> Get<T>(string endpoint, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            ErrorModel errorDetail;

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            else
                errorDetail = await GetError(response);

            throw new ApplicationException(errorDetail.Title);
        }

        public async virtual Task<T> Post<T>(string endpoint, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            ErrorModel errorDetail;

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            else
                errorDetail = await GetError(response);

            throw new ApplicationException(errorDetail.Title);
        }

        public async virtual Task Post(string endpoint, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await GetError(response);
                throw new ApplicationException(errorDetail.Title);
            }
        }

        public async virtual Task<T> Put<T>(string endpoint, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);
            ErrorModel errorDetail;

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            else
                errorDetail = await GetError(response);

            throw new ApplicationException(errorDetail.Title);
        }

        public async virtual Task Put(string endpoint, object data, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            request.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await GetError(response);
                throw new ApplicationException(errorDetail.Title);
            }
        }

        private async Task<ErrorModel> GetError(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var error = JsonConvert.DeserializeObject<ErrorModel>(content);
                if (error.Title == "User hasn't been granted access to any region." && error.Type == "MultitenancyException")
                    OnNoRegionError?.Invoke();
                return error;
            }
            catch
            {
                return new ErrorModel()
                {
                    Title = response.ReasonPhrase + " " + content
                };
            }
        }
    }
}
