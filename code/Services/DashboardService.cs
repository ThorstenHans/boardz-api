using BoardZ.API.Database;
using System;
using System.Linq;

namespace BoardZ.API.Services
{
    public class DashboardService
    {
        public DashboardService(BoardZContext context)
        {
            Context = context;
        }

        protected BoardZContext Context { get; }

        public object GetStats(string userName)
        {
            return new
            {
                categories = Context.Categories.Count(category =>
                    category.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)),
                games = Context.Games.Count(game =>
                    game.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)),
                players = Context.Players.Count(player => player.PlayingSince.Date.Equals(DateTime.Now.Date))
            };
        }
    }
}