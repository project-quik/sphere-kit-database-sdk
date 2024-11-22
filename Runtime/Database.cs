#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SphereKit
{
    public class Database
    {
        public string? Id => _id ?? (CoreServices.DatabaseSettings.DatabaseId ?? null);
        
        private readonly HttpClient _httpClient = new();
        private readonly string? _id;
        
        
        public Database(string? id = null)
        {
            _id = id;
        }
        
        public CollectionReference Collection(string path)
        {
            return new CollectionReference(path, this);
        }
        
        public DocumentReference Document(string path)
        {
            return new DocumentReference(path, this);
        }

        private void ConfigureHeaders()
        {
            if (CoreServices.AccessToken != null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CoreServices.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("X-Sphere-Project-Name", CoreServices.ProjectId);
            }
        }
        
        internal async Task<Document> GetDocument(DocumentReference reference)
        {
            CoreServices.CheckInitialized();
            
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            var documentResponse = await _httpClient.SendAsync(requestMessage);
            if (documentResponse.IsSuccessStatusCode)
            {
                var documentData = JsonConvert.DeserializeObject<Dictionary<string, object>>(await documentResponse.Content.ReadAsStringAsync())!;
                return new Document(reference, documentData);
            }
            else
            {
                await CoreServices.HandleErrorResponse(documentResponse);
                return new Document();
            }
        }
    }
}
