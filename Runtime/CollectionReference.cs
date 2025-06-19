using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

#nullable enable
namespace SphereKit
{
    public class CollectionReference : DatabaseReference
    {
        public new DocumentReference? Parent
        {
            get
            {
                var segments = Path.Split('/');
                if (segments.Length < 3)
                {
                    return null;
                }

                var parentPath = string.Join("/", segments.SkipLast(1));
                return new DocumentReference(parentPath, Database);
            }
        }
        
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

        /// <summary>
        /// Gets all documents in this collection.
        /// </summary>
        /// <returns>The collection with an array of documents.</returns>
        public async Task<Collection> GetDocuments()
        {
            return await Database.GetDocuments(this);
        }

        /// <summary>
        /// Finds/projects/sorts documents in this collection.<br></br>
        /// Only either <see cref="includeFields"/> or <see cref="excludeFields"/> can be specified at a time.<br></br>
        /// <see cref="sort"/> must be provided and matching the fields of <see cref="startAfter"/> if <see cref="startAfter"/> is provided.
        /// </summary>
        /// <param name="query">The queries to filter documents by.</param>
        /// <param name="includeFields">The fields to be retrieved in each document retrieved.</param>
        /// <param name="excludeFields">The fields to be excluded in each document retrieved.</param>
        /// <param name="sort">The sort specification for the documents.</param>
        /// <param name="startAfter">The field values to start after for sorted fields. Allows for pagination.</param>
        /// <param name="limit">The maximum number of documents to retrieve at a time.</param>
        /// <returns>The collection with the array of filtered documents.</returns>
        /// <exception cref="ArgumentException">Invalid parameter values.</exception>
        public async Task<Collection> QueryDocuments(DocumentQueryOperation[]? query = null,
            string[]? includeFields = null,
            string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, Dictionary<string, object>? startAfter = null,
            int? limit = null)
        {
            return await Database.QueryDocuments(this, query, includeFields, excludeFields, sort, startAfter, limit);
        }

        /// <summary>
        /// Listens to changes in documents of this collection.<br></br>
        /// Changes notified are document update, inserts and deletes.<br></br>
        /// Only either <see cref="includeFields"/> or <see cref="excludeFields"/> can be specified at a time.
        /// </summary>
        /// <param name="onData">The callback when an update is received.</param>
        /// <param name="onError">The callback when an error is received.</param>
        /// <param name="onClosed">The callback when the connection is closed and will not be restored.</param>
        /// <param name="query">The queries to filter documents by.</param>
        /// <param name="includeFields">The fields to be retrieved in each document received.</param>
        /// <param name="excludeFields">The fields to be excluded in each document received.</param>
        /// <param name="sort">The sort specification for the documents (for initial data).</param>
        /// <param name="autoReconnect">Whether to automatically reconnect to the server when the internet connection drops.</param>
        /// <param name="sendInitialData">Whether to send all matching documents when the listener is first set up.</param>
        public void ListenDocuments(Action<MultiDocumentChange> onData, Action<Exception> onError,
            Action onClosed, DocumentQueryOperation[]? query = null, string[]? includeFields = null,
            string[]? excludeFields = null,
            Dictionary<string, FieldSortDirection>? sort = null, bool autoReconnect = true,
            bool sendInitialData = false)
        {
            _ = Database.ListenDocuments(this, onData, onError, onClosed, query, includeFields, excludeFields, sort,
                autoReconnect, sendInitialData);
        }

        /// <summary>
        /// Adds or replaces multiple documents to this collection.<br></br>
        /// A maximum of 50 documents can be set at once.
        /// </summary>
        /// <param name="documents">The documents to be set, with ID as key and data as value.</param>
        public async Task SetDocuments(Dictionary<string, Dictionary<string, object>> documents)
        {
            await Database.SetDocuments(this, documents);
        }

        /// <summary>
        /// Updates matching documents in this collection.
        /// </summary>
        /// <param name="update">The update specification, with field name as key and operation as value.</param>
        /// <param name="filter">The filters to find matching documents to update.</param>
        public async Task UpdateDocuments(Dictionary<string, DocumentDataOperation> update,
            DocumentQueryOperation[]? filter = null)
        {
            await Database.UpdateDocuments(this, update, filter);
        }

        /// <summary>
        /// Deletes matching documents from this collection.
        /// </summary>
        /// <param name="filter">The filters to find matching documents to delete.</param>
        public async Task DeleteDocuments(DocumentQueryOperation[]? filter = null)
        {
            await Database.DeleteDocuments(this, filter);
        }
    }
}