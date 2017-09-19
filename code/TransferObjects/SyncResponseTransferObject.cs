using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BoardZ.API.TransferObjects
{
    public class SyncResponseTransferObject
    {
        public List<ItemSyncResponse> Categories { get; set; }
        public List<ItemSyncResponse> Games { get; set; }
    }

    public enum ItemState
    {
        Created,
        Updated,
        Deleted
    }
    public class ItemSyncResponse
    {
        public string OfflineId { get; set; }
        public Guid Id { get; set; }
        public ItemState State { get; set; }
        public object Replacement { get; set; }
    } 
}
