using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using BoardZ.API.TransferObjects;

namespace BoardZ.API.Models
{
    /// <summary>
    /// The Game Model
    /// </summary>
    public class Game
    {
        /// <summary>
        /// default ctor
        /// </summary>
        public Game()
        {
            GameCategories = new List<GameCategory>();
        }
        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of the board game
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Edition of the Game
        /// </summary>
        public string Edition { get; set; }

        /// <summary>
        /// List of categories applied to the game
        /// </summary>
        [JsonProperty("categories")]
        public List<GameCategory> GameCategories { get; set; }

        /// <summary>
        /// AgeRatingId
        /// </summary>
        public Guid? AgeRatingId { get; set; }

        /// <summary>
        /// Age Rating
        /// </summary>
        public virtual AgeRating AgeRating { get; set; }
        /// <summary>
        /// Additional description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the user who created the game
        /// </summary>
        [JsonIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// RowVersion -> required for Offline Support
        /// </summary>
        [JsonIgnore]
        public byte[] RowVersion { get; set; }
    
        /// <summary>
        /// the version that goes to the client
        /// </summary>
        public ulong RowVersionAsInt => RowVersion != null ? BitConverter.ToUInt64(RowVersion.Reverse().ToArray(), 0) : 0;

        internal static Game FromTransferObject(GameTransferObject transferObject)
        {
            Game game = null;
            switch (transferObject.ModelState)
            {
                case ModelState.New:
                    game = new Game()
                    {
                        Id = Guid.NewGuid(),
                        AgeRatingId = transferObject.AgeRatingId,
                        Description = transferObject.Description,
                        Name = transferObject.Name
                    };
                    break;
                default:
                    game = new Game
                    {
                        Id = String.IsNullOrEmpty(transferObject.Id) ? Guid.NewGuid() : Guid.Parse(transferObject.Id),
                        AgeRatingId = transferObject.AgeRatingId,
                        Description = transferObject.Description,
                        Name = transferObject.Name
                    };
                    break;
            } 

            foreach (var item in transferObject.Categories)
            {
                game.GameCategories.Add(new GameCategory
                {
                    CategoryId = Guid.Parse(item.Id),
                    Category = Category.FromTransferObject(item)
                });
            }
            return game;
        }
    }
}
