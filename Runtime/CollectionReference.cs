using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

#nullable enable
namespace SphereKit
{
    public class CollectionReference : DatabaseReference
    {
        public CollectionReference(string path, Database database) : base(path, database)
        {
            if (path.Length == 0) throw new ArgumentException("CollectionReference path must not be empty.");
            if (path.Split("/").Length % 2 == 0)
                throw new ArgumentException("CollectionReference path must have an odd number of segments.");
        }

        public DocumentReference Document(string id)
        {
            return new DocumentReference($"{Path}/{id}", Database);
        }

        public async Task<Collection> GetDocuments()
        {
            return await Database.GetCollection(this);
        }

        public async Task<Collection> QueryDocuments(DocumentQueryOperation[]? query = null,
            string[]? includeFields = null,
            string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, Dictionary<string, object>? startAfter = null,
            int? limit = null)
        {
            return await Database.QueryCollection(this, query, includeFields, excludeFields, sort, startAfter, limit);
        }

        public void ListenDocuments(Action<MultiDocumentChange> onData, Action<Exception> onError,
            Action onClosed, DocumentQueryOperation[]? query = null, string[]? includeFields = null,
            string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, bool autoReconnect = true,
            bool sendInitialData = false)
        {
            _ = Database.ListenDocuments(this, onData, onError, onClosed, query, includeFields, excludeFields, sort,
                autoReconnect, sendInitialData);
        }

        public async Task SetDocuments(Dictionary<string, Dictionary<string, object>> documents)
        {
            await Database.SetDocuments(this, documents);
        }

        public async Task UpdateDocuments(Dictionary<string, DocumentDataOperation> update,
            DocumentQueryOperation[]? filter = null)
        {
            await Database.UpdateDocuments(this, update, filter);
        }

        public async Task DeleteDocuments(DocumentQueryOperation[]? filter = null)
        {
            await Database.DeleteDocuments(this, filter);
        }
    }
}