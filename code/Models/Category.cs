using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using BoardZ.API.TransferObjects;

namespace BoardZ.API.Models
{
    /// <summary>
    /// Category model
    /// </summary>
    public class Category
    {
        /// <summary>
        /// default ctor
        /// </summary>
        public Category()
        {
            GameCategories = new List<GameCategory>();
        }

        /// <summary>
        /// Category Id
        /// </summary>
        public Guid Id { get; set; }
        

        /// <summary>
        /// Name of the Category
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// The name of the user who craeated that category
        /// </summary>
        [JsonIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// All games in this category
        /// </summary>
        [JsonIgnore]
        public IEnumerable<GameCategory> GameCategories { get; set; }

        /// <summary>
        /// List with Games in that category
        /// </summary>
        public IList<String> GameNames
        {
            get
            {
                return GameCategories
                    .Where(gc => gc.Game.UserName.Equals(UserName, StringComparison.InvariantCultureIgnoreCase))
                    .Select(gc => gc.Game.Name)
                    .ToList();
            }
        }

        /// <summary>
        /// NumberOfGames
        /// </summary>
        public int NumberOfGames => GameNames.Count;

        /// <summary>
        /// Category Row Version
        /// </summary>
        [JsonIgnore]
        public byte[] RowVersion { get; set; }

        /// <summary>
        /// the version that goes to the client
        /// </summary>
        public ulong RowVersionAsInt => RowVersion != null ? BitConverter.ToUInt64(RowVersion.Reverse().ToArray(), 0) : 0;

        /// <summary>
        /// ModelState -> will be provided by the client when syncinc after connection was lost
        /// </summary>

     

        public static Category FromTransferObject(CategoryTransferObject transferObject)
        {
            switch (transferObject.ModelState)
            {
                case ModelState.New:
                    return new Category
                    {
                        Name = transferObject.Name
                    };
                default:
                    return new Category
                    {
                        Id = Guid.Parse(transferObject.Id),
                        Name = transferObject.Name
                    };
            }
        }
    }
}
