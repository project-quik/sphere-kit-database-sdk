#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NativeWebSocket;
using Newtonsoft.Json;
using SphereKit.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

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
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {CoreServices.AccessToken}");
            else
                _httpClient.DefaultRequestHeaders.Add("X-Sphere-Project-Name", CoreServices.ProjectId);
        }

        private static Dictionary<string, string> GetWebsocketHeaders()
        {
            var headers = new Dictionary<string, string>();

            if (CoreServices.AccessToken != null)
                headers.Add("Authorization", $"Bearer {CoreServices.AccessToken}");

            return headers;
        }

        private void CheckDatabaseAvailable()
        {
            if (Id == null)
                throw new Exception("The database is not set up. Please create it in the Sphere Kit dashboard.");
        }

        internal async Task<Collection> GetCollection(CollectionReference reference)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            var collectionResponse = await _httpClient.SendAsync(requestMessage);
            if (collectionResponse.IsSuccessStatusCode)
            {
                var collectionData =
                    JsonConvert.DeserializeObject<GetDocumentsResponse>(
                        await collectionResponse.Content.ReadAsStringAsync());
                return new Collection(reference, collectionData.Documents);
            }
            else
            {
                await CoreServices.HandleErrorResponse(collectionResponse);
                return new Collection();
            }
        }

        internal async Task<Collection> QueryCollection(CollectionReference reference,
            DocumentQueryOperation[]? query = null, string[]? includeFields = null, string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, Dictionary<string, object>? startAfter = null,
            int? limit = null)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            var requestData = new Dictionary<string, object>();

            if (query != null)
            {
                var queryRequestData = DocumentQueryOperation.ConvertQueryToRequestData(query);
                requestData["query"] = queryRequestData;
            }

            if (includeFields != null && excludeFields != null)
                throw new ArgumentException("Cannot include and exclude fields in the same query. Please choose one.");

            if (includeFields != null) requestData["projection"] = includeFields.ToDictionary(field => field, _ => 1);

            if (excludeFields != null) requestData["projection"] = excludeFields.ToDictionary(field => field, _ => 0);

            if (sort != null)
            {
                if (startAfter != null)
                {
                    if (!startAfter.Keys.SequenceEqual(sort.Keys))
                        throw new ArgumentException(
                            "The keys of the startAfter dictionary must match the keys of the sort dictionary.");

                    requestData["startAfter"] = startAfter;
                }

                requestData["sort"] = sort;
            }
            else
            {
                if (startAfter != null)
                    throw new ArgumentException("startAfter can only be used with sort.");
            }

            if (limit != null) requestData["limit"] = limit;

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{CoreServices.ServerUrl}/databases:query/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestData));
            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var collectionResponse = await _httpClient.SendAsync(requestMessage);
            if (collectionResponse.IsSuccessStatusCode)
            {
                var collectionData =
                    JsonConvert.DeserializeObject<GetDocumentsResponse>(
                        await collectionResponse.Content.ReadAsStringAsync());
                return new Collection(reference, collectionData.Documents);
            }
            else
            {
                await CoreServices.HandleErrorResponse(collectionResponse);
                return new Collection();
            }
        }

        internal async Task ListenDocuments(CollectionReference reference, Action<MultiDocumentChange> onData,
            Action<Exception> onError,
            Action onClosed, DocumentQueryOperation[]? query = null, string[]? includeFields = null,
            string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, bool autoReconnect = true,
            bool sendInitialData = false)
        {
            var url = UrlBuilder.New((CoreServices.ServerUrl.StartsWith("http://") ? "ws" : "wss") + ":" +
                                     string.Join(":", CoreServices.ServerUrl.Split(":").Skip(1)) +
                                     $"/databases:listen/{Id}/{reference.Path}");
            var urlQuery = new Dictionary<string, string>
            {
                { "initialFullResult", sendInitialData ? "true" : "false" }
            };
            if (CoreServices.AccessToken == null) urlQuery["projectName"] = CoreServices.ProjectId;
            if (query != null) urlQuery["willQuery"] = "true";
            url.SetQueryParameters(urlQuery);
            var websocket = new WebSocket(url.ToString(), GetWebsocketHeaders());

            var connectionAttempts = 0;
            var firstOpened = false;
            var timer = new System.Timers.Timer(16);

            websocket.OnOpen += () =>
            {
                Debug.Log("Websocket connection opened.");
                firstOpened = true;

                var requestData = new Dictionary<string, object>();

                if (query != null)
                {
                    var queryRequestData = DocumentQueryOperation.ConvertQueryToRequestData(query);
                    requestData["query"] = queryRequestData;
                }

                if (includeFields != null && excludeFields != null)
                    throw new ArgumentException(
                        "Cannot include and exclude fields in the same query. Please choose one.");

                if (includeFields != null)
                    requestData["projection"] = includeFields.ToDictionary(field => field, _ => 1);

                if (excludeFields != null)
                    requestData["projection"] = excludeFields.ToDictionary(field => field, _ => 0);

                if (sort != null) requestData["sort"] = sort;

                if (requestData.Count > 0)
                {
                    Debug.Log("Sending query data... " + JsonConvert.SerializeObject(requestData));
                    websocket.SendText(JsonConvert.SerializeObject(requestData));
                }
            };

            websocket.OnMessage += data =>
            {
                // Convert the message bytes to string
                var message = System.Text.Encoding.UTF8.GetString(data);

                // Throw error if needed
                var changeDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;
                if (changeDict.ContainsKey("error"))
                {
                    try
                    {
                        CoreServices.HandleErrorString(message);
                    }
                    catch (Exception ex)
                    {
                        onError(ex);
                    }

                    return;
                }

                // Parse the change data
                var change = JsonConvert.DeserializeObject<MultiDocumentChange>(message)!;
                change.GenerateDocuments(reference);
                onData(change);
            };

            websocket.OnError += e =>
            {
                try
                {
                    if (e == "Unable to connect to the remote server")
                    {
                        if (firstOpened) return;

                        throw new Exception("Unable to connect to the document listener.");
                    }

                    CoreServices.HandleErrorString(e);
                }
                catch (Exception ex)
                {
                    onError(ex);
                }
            };

            websocket.OnClose += e =>
            {
                if (e == WebSocketCloseCode.Abnormal && autoReconnect && firstOpened)
                {
                    Debug.Log("Websocket connection closed abnormally. Reconnecting...");
                    Task.Delay(TimeSpan.FromSeconds(Math.Min(20, Math.Pow(2, connectionAttempts)))).ContinueWith(
                        async _ =>
                        {
                            connectionAttempts++;
                            await websocket.Connect();
                        });
                    return;
                }

                timer.Dispose();
                onClosed();
            };

            // Check for new messages
            timer.Elapsed += (_, _) =>
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
#endif
            };
            timer.Enabled = true;

            await websocket.Connect();
        }

        internal async Task SetDocuments(CollectionReference reference,
            Dictionary<string, Dictionary<string, object>> documents)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(documents));
            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var setDocumentsResponse = await _httpClient.SendAsync(requestMessage);
            if (!setDocumentsResponse.IsSuccessStatusCode) await CoreServices.HandleErrorResponse(setDocumentsResponse);
        }

        internal async Task UpdateDocuments(CollectionReference reference,
            Dictionary<string, DocumentDataOperation> update, DocumentQueryOperation[]? filter = null)
        {
            filter ??= Array.Empty<DocumentQueryOperation>();

            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            var requestData = new Dictionary<string, object>();

            var filterRequestData = DocumentQueryOperation.ConvertQueryToRequestData(filter);
            requestData["filter"] = filterRequestData;

            var updateRequestData = DocumentDataOperation.ConvertUpdateToRequestData(update);
            if (updateRequestData.Count == 0) return;
            requestData["update"] = updateRequestData;

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Headers.Add("X-Http-Method-Override",
                "PATCH"); // PATCH method is not supported by UnityWebRequest (as of 6000)
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestData),
                System.Text.Encoding.UTF8, "application/json");
            var updateDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!updateDocumentResponse.IsSuccessStatusCode)
                await CoreServices.HandleErrorResponse(updateDocumentResponse);
        }

        internal async Task DeleteDocuments(CollectionReference reference, DocumentQueryOperation[]? filter = null)
        {
            filter ??= Array.Empty<DocumentQueryOperation>();

            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            var requestData = new Dictionary<string, object>();

            var filterRequestData = DocumentQueryOperation.ConvertQueryToRequestData(filter);
            requestData["filter"] = filterRequestData;

            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(requestData),
                System.Text.Encoding.UTF8, "application/json");
            var deleteDocumentsResponse = await _httpClient.SendAsync(requestMessage);
            if (!deleteDocumentsResponse.IsSuccessStatusCode)
                await CoreServices.HandleErrorResponse(deleteDocumentsResponse);
        }

        internal async Task<Document> GetDocument(DocumentReference reference)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            var documentResponse = await _httpClient.SendAsync(requestMessage);
            if (documentResponse.IsSuccessStatusCode)
            {
                var documentData =
                    JsonConvert.DeserializeObject<Dictionary<string, object>>(await documentResponse.Content
                        .ReadAsStringAsync())!;
                return new Document(reference, documentData);
            }
            else
            {
                await CoreServices.HandleErrorResponse(documentResponse);
                return new Document();
            }
        }

        internal async Task ListenDocument(DocumentReference reference, Action<SingleDocumentChange> onData,
            Action<Exception> onError,
            Action onClosed,
            bool autoReconnect = true,
            bool sendInitialData = false)
        {
            var url = UrlBuilder.New((CoreServices.ServerUrl.StartsWith("http://") ? "ws" : "wss") + ":" +
                                     string.Join(":", CoreServices.ServerUrl.Split(":").Skip(1)) +
                                     $"/databases:listen/{Id}/{reference.Path}");
            var urlQuery = new Dictionary<string, string>
            {
                { "initialFullResult", sendInitialData ? "true" : "false" }
            };
            if (CoreServices.AccessToken == null) urlQuery["projectName"] = CoreServices.ProjectId;
            url.SetQueryParameters(urlQuery);
            var websocket = new WebSocket(url.ToString(), GetWebsocketHeaders());

            var connectionAttempts = 0;
            var firstOpened = false;
            var timer = new System.Timers.Timer(16);

            websocket.OnOpen += () =>
            {
                Debug.Log("Websocket connection opened.");
                firstOpened = true;
            };

            websocket.OnMessage += data =>
            {
                // Convert the message bytes to string
                var message = System.Text.Encoding.UTF8.GetString(data);

                // Throw error if needed
                var changeDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(message)!;
                if (changeDict.ContainsKey("error"))
                {
                    try
                    {
                        CoreServices.HandleErrorString(message);
                    }
                    catch (Exception ex)
                    {
                        onError(ex);
                    }

                    return;
                }

                // Parse the change data
                var change = JsonConvert.DeserializeObject<SingleDocumentChange>(message)!;
                change.GenerateDocument(reference);
                onData(change);
            };

            websocket.OnError += e =>
            {
                try
                {
                    if (e == "Unable to connect to the remote server")
                    {
                        if (firstOpened) return;

                        throw new Exception("Unable to connect to the document listener.");
                    }

                    CoreServices.HandleErrorString(e);
                }
                catch (Exception ex)
                {
                    onError(ex);
                }
            };

            websocket.OnClose += e =>
            {
                if (e == WebSocketCloseCode.Abnormal && autoReconnect && firstOpened)
                {
                    Debug.Log("Websocket connection closed abnormally. Reconnecting...");
                    Task.Delay(TimeSpan.FromSeconds(Math.Min(20, Math.Pow(2, connectionAttempts)))).ContinueWith(
                        async _ =>
                        {
                            connectionAttempts++;
                            await websocket.Connect();
                        });
                    return;
                }

                timer.Dispose();
                onClosed();
            };

            // Check for new messages
            timer.Elapsed += (_, _) =>
            {
#if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
#endif
            };
            timer.Enabled = true;

            await websocket.Connect();
        }

        internal async Task SetDocument(DocumentReference reference, Dictionary<string, object> data)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(data));
            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var setDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!setDocumentResponse.IsSuccessStatusCode) await CoreServices.HandleErrorResponse(setDocumentResponse);
        }

        internal async Task UpdateDocument(DocumentReference reference,
            Dictionary<string, DocumentDataOperation> update)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            var updateRequestData = DocumentDataOperation.ConvertUpdateToRequestData(update);
            if (updateRequestData.Count == 0) return;

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Headers.Add("X-Http-Method-Override",
                "PATCH"); // PATCH method is not supported by UnityWebRequest (as of 6000)
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(updateRequestData),
                System.Text.Encoding.UTF8, "application/json");
            var updateDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!updateDocumentResponse.IsSuccessStatusCode)
                await CoreServices.HandleErrorResponse(updateDocumentResponse);
        }

        internal async Task DeleteDocument(DocumentReference reference)
        {
            CoreServices.CheckInitialized();
            CheckDatabaseAvailable();

            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete,
                $"{CoreServices.ServerUrl}/databases/{Id}/{reference.Path}");
            ConfigureHeaders();
            requestMessage.Content = new StringContent(JsonConvert.SerializeObject(new Dictionary<string, object>()));
            requestMessage.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            var deleteDocumentResponse = await _httpClient.SendAsync(requestMessage);
            if (!deleteDocumentResponse.IsSuccessStatusCode)
                await CoreServices.HandleErrorResponse(deleteDocumentResponse);
        }
    }
}