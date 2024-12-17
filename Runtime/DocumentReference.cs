using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable
namespace SphereKit
{
    public class DocumentReference : DatabaseReference
    {
        public DocumentReference(string path, Database database) : base(path, database)
        {
            if (path.Length == 0) throw new ArgumentException("DocumentReference path must not be empty.");
            if (path.Split("/").Length % 2 != 0)
                throw new ArgumentException("DocumentReference path must have an even number of segments.");
        }

        public CollectionReference Collection(string id)
        {
            return new CollectionReference($"{Path}/{id}", Database);
        }

        /// <summary>
        /// Retrieves this document.
        /// </summary>
        /// <returns>The retrieved document.</returns>
        public async Task<Document> Get()
        {
            return await Database.GetDocument(this);
        }

        /// <summary>
        /// Listens to changes to this document.<br></br>
        /// Changes notified are only document update and inserts <b>(no delete)</b>.
        /// </summary>
        /// <param name="onData">The callback when an update is received</param>
        /// <param name="onError">The callback when an error is received.</param>
        /// <param name="onClosed">The callback when the connection is closed and will not be restored.</param>
        /// <param name="autoReconnect">Whether to automatically reconnect to the server when the internet connection drops.</param>
        /// <param name="sendInitialData">Whether to send the document in its current state (if it exists) when the listener is first set up.</param>
        public void Listen(Action<SingleDocumentChange> onData, Action<Exception> onError,
            Action onClosed, bool autoReconnect = true,
            bool sendInitialData = false)
        {
            _ = Database.ListenDocument(this, onData, onError, onClosed, autoReconnect, sendInitialData);
        }

        /// <summary>
        /// Adds or replaces a document at this path.
        /// </summary>
        /// <param name="data">The document data.</param>
        public async Task Set(Dictionary<string, object> data)
        {
            await Database.SetDocument(this, data);
        }

        /// <summary>
        /// Updates the data of this document.
        /// </summary>
        /// <param name="update">The update specification, with field name as key and operation as value.</param>
        public async Task Update(Dictionary<string, DocumentDataOperation> update)
        {
            await Database.UpdateDocument(this, update);
        }

        /// <summary>
        /// Deletes this document.
        /// </summary>
        public async Task Delete()
        {
            await Database.DeleteDocument(this);
        }
    }
}