using System;
using System.Collections.Generic;
using System.Linq;
using BoardZ.API.Models;

namespace BoardZ.API.TransferObjects
{
    public class GameTransferObject
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? AgeRatingId { get; set; }
        public List<CategoryTransferObject> Categories { get; set; }
        public ulong RowVerionAsInt { get; set; }
        public string UserName { get; set; }
        public string Id { get; set; }

        public int State { get; set; }
        
        public ModelState ModelState => (ModelState)State;
        internal static GameTransferObject FromGame(Game game) => new GameTransferObject
        {
            Id = game.Id.ToString(),
            UserName = game.UserName,
            AgeRatingId = game.AgeRatingId,
            Categories = game.GameCategories.Select(gc => CategoryTransferObject.FromCategory(gc.Category)).ToList(),
            Name = game.Name,
            Description = game.Description,
            RowVerionAsInt = game.RowVersionAsInt
        };
    }
}
