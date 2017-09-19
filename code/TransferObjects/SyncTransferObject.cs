using System.Collections.Generic;

namespace BoardZ.API.TransferObjects
{
    public class SyncTransferObject
    {
        public List<CategoryTransferObject> Categories { get; set; }
        public List<GameTransferObject> Games { get; set; }
    }
}
