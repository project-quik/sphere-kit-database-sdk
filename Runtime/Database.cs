#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;

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
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Remove("X-Sphere-Project-Name");
            
            if (CoreServices.AccessToken != null)
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CoreServices.AccessToken}");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("X-Sphere-Project-Name", CoreServices.ProjectId);
            }
        }

        private void CheckDatabaseAvailable()
        {
            if (Id == null)
            {
                throw new System.Exception("The database is not set up. Please create it in the Sphere Kit dashboard.");
            }
        }
        
        internal async Task<Document> GetDocument(DocumentReference reference)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();
            
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

        internal async Task SetDocument(DocumentReference reference, Dictionary<string, object> data)
        {
            CoreServices.CheckInitialized();    
            CheckDatabaseAvailable();
            
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data));
            requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var setDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!setDocumentResponse.IsSuccessStatusCode)
            {
                await CoreServices.HandleErrorResponse(setDocumentResponse);
            }
        }

        internal async Task UpdateDocument(DocumentReference reference,
            Dictionary<string, DocumentDataOperation> update)
        {
            CoreServices.CheckInitialized();
            
            var updateRequestData = new Dictionary<string, object>();
            foreach (var (fieldKey, value) in update)
            {
                var operationKey = value.OperationType;
                var operationValue = value.Value;
                var operationKeyStr = operationKey switch
                {
                    DocumentDataOperationType.Set => "$set",
                    DocumentDataOperationType.SetOnInsert => "$setOnInsert",
                    DocumentDataOperationType.Inc => "$inc",
                    DocumentDataOperationType.Dec => "$dec",
                    DocumentDataOperationType.Min => "$min",
                    DocumentDataOperationType.Max => "$max",
                    DocumentDataOperationType.Mul => "$mul",
                    DocumentDataOperationType.Div => "$div",
                    DocumentDataOperationType.Rename => "$rename",
                    DocumentDataOperationType.Unset => "$unset",
                    DocumentDataOperationType.AddToSet => "$addToSet",
                    DocumentDataOperationType.Pop => "$pop",
                    DocumentDataOperationType.Push => "$push",
                    _ => ""
                };

                if (operationKey != DocumentDataOperationType.Unset)
                {
                    if (!updateRequestData.TryGetValue(operationKeyStr, out var operationData))
                    {
                        operationData = new Dictionary<string, object>();
                        updateRequestData[operationKeyStr] = operationData;
                    }

                    var operationDataDict = (Dictionary<string, object>)operationData;
                    operationDataDict[fieldKey] = operationValue!;
                }
                else
                {
                    if (!updateRequestData.TryGetValue(operationKeyStr, out var operationData))
                    {
                        operationData = new List<object>();
                        updateRequestData[operationKeyStr] = operationData;
                    }

                    var operationDataList = (List<object>)operationData;
                    operationDataList.Add(fieldKey);
                }
            }

            if (updateRequestData.Count == 0)
            {
                return;
            }

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Headers.Add("X-Http-Method-Override",
                "PATCH"); // PATCH method is not supported by UnityWebRequest (as of 6000)
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(updateRequestData),
                System.Text.Encoding.UTF8, "application/json");
            Debug.Log("Updating document with update json: " + await requestMessage.Content.ReadAsStringAsync());
            var updateDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!updateDocumentResponse.IsSuccessStatusCode)
            {
                await CoreServices.HandleErrorResponse(updateDocumentResponse);
            }
        }

        internal async Task DeleteDocument(DocumentReference reference)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();
            
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, object>()));
            requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var deleteDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!deleteDocumentResponse.IsSuccessStatusCode)
            {
                await CoreServices.HandleErrorResponse(deleteDocumentResponse);
            }
        }
    }
}
