using System;
using System.Collections.Generic;
using System.Linq;
using BoardZ.API.Database;
using BoardZ.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BoardZ.API.Exceptions;
using System.Net;

namespace BoardZ.API.Services
{
    public class GamesService : IDisposable
    {
        protected BoardZContext Context { get; }

        public GamesService(BoardZContext context)
        {
            Context = context;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public IList<Game> GetAll(string username, ulong? rowVersion = null)
        {
            IQueryable<Game> query = Context.Games
                .Include(game => game.AgeRating)
                .Include(game => game.GameCategories)
                .ThenInclude(gameCategory => gameCategory.Category)
                .Where(game => game.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(game => game.Name);

            if (rowVersion.HasValue)
            {
                query = query
                    .Where(game => game.RowVersionAsInt > rowVersion);
            }
            return query.ToList();
        }

        public int GetCount(string username)
        {
            return Context.Games
                .Count(game => game.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        }

        public Game AddGame(Game game, String username)
        {
            try
            {
                game.UserName = username;
                Context.Games.Add(game);
                foreach (var item in game.GameCategories)
                {
                    if (item.Category != null)
                    {
                        Context.Entry(item.Category).State = EntityState.Unchanged;
                    }
                }
                if (game.AgeRating != null)
                {
                    Context.Entry(game.AgeRating).State = EntityState.Unchanged;
                }
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
            return game;
        }

        public Game GetById(Guid id, string username)
        {
            var result = Context.Games
                .Include(game => game.AgeRating)
                .Include(game => game.GameCategories)
                .ThenInclude(gameCategory => gameCategory.Category)
                .FirstOrDefault(game =>
                    game.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase) && game.Id.Equals(id));
            if (result == null)
            {
                throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Game with id {id} not found");
            }
            return result;
        }

        public void DeleteGame(Guid id)
        {
            var found = Context.Games
                .FirstOrDefault(game => game.Id.Equals(id));

            if (found == null)
            {
                throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Game with id {id} not found");
            }
            try
            {
                Context.Entry(found).State = EntityState.Deleted;
                Context.SaveChanges();
            }
            catch (Exception exception)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, exception.Message, exception);
            }
        }

        public Game UpdateGame(Game game, string username)
        {
            try
            {
                var foundGame = GetById(game.Id, username);
                if (foundGame == null)
                {
                    throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Game with id {game.Id} not found");
                }
                Context.Entry(foundGame).CurrentValues.SetValues(game);
                foundGame.GameCategories.Clear();
                Context.SaveChanges();
                foreach (var category in game.GameCategories)
                {
                    var foundCategory = Context.Categories.Find(category.CategoryId);
                    foundGame.GameCategories.Add(new GameCategory
                    {
                        Category = foundCategory,
                        CategoryId = foundCategory.Id,
                        Game = foundGame,
                        GameId = foundGame.Id
                    });
                }
                Context.SaveChanges();
                return game;
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        public IDbContextTransaction NewTransaction()
        {
            return Context.Database.BeginTransaction();
        }
    }
}
