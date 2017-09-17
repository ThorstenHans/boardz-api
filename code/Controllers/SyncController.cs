using System;
using System.Collections.Generic;
using System.Linq;
using BoardZ.API.Extensions;
using BoardZ.API.Models;
using BoardZ.API.Services;
using Microsoft.AspNetCore.Mvc;
using BoardZ.API.TransferObjects;
using BoardZ.API.Exceptions;
using System.Net;

namespace BoardZ.API.Controllers
{
    /// <inheritdoc>
    ///     <cref></cref>
    /// </inheritdoc>
    /// <summary>
    /// SnycController
    /// </summary>
    [Route("api/sync")]
    public class SyncController : Controller, IDisposable
    {
        protected GamesService GamesService { get; }
        protected CategoriesService CategoriesService { get; }

        /// <summary>
        /// Sync Controller Default CTOR
        /// </summary>
        public SyncController(GamesService gamesService, CategoriesService categoriesService)
        {
            GamesService = gamesService;
            CategoriesService = categoriesService;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            GamesService?.Dispose();
            CategoriesService?.Dispose();
        }

        [HttpPost]
        [Route("")]
        public IActionResult Sync([FromBody] SyncTransferObject transferObject)
        {
            if (transferObject == null || (transferObject.Games == null || transferObject.Games.Count == 0) &&
                (transferObject.Categories == null || transferObject.Categories.Count == 0))
            {
                return BadRequest("Don't call sync API if you've nothing yo sync.");
            }
            List<ItemSyncResponse> syncedCategories;
            List<ItemSyncResponse> syncedGames;
            try
            {
                var userName = User.GetSubjectOrThrow();
                syncedCategories = SyncCategories(transferObject, userName);
                syncedGames = SyncGames(transferObject, syncedCategories, userName);
            }
            catch (Exception ex)
            {
                throw new HttpStatusCodeException(HttpStatusCode.BadRequest, "Error while syncing data", ex);
            } 
            return Ok(new SyncResponseTransferObject
            {
                Categories = syncedCategories,
                Games = syncedGames
            });
        }

        private List<ItemSyncResponse> SyncGames(SyncTransferObject transferObject,
            List<ItemSyncResponse> syncedCategories, string userName)
        {
            var result = new List<ItemSyncResponse>();
            using (var transaction = GamesService.NewTransaction())
            {
                try
                {
                    transferObject.Games.ForEach(game =>
                    {
                        game.Categories = game.Categories
                            .Select(c =>
                            {
                                if (long.TryParse(c.Id, out _))
                                {
                                    var syncedCategory = syncedCategories
                                        .FirstOrDefault(syncedCat => syncedCat.OfflineId.Equals(c.Id));
                                    if (syncedCategory != null)
                                    {
                                        c.Id = syncedCategory.Id.ToString();
                                    }
                                }
                                return c;
                            })
                            .ToList();

                        switch (game.ModelState)
                        {
                            case Models.ModelState.New:
                                var newGame = GamesService.AddGame(Game.FromTransferObject(game), userName);
                                result.Add(new ItemSyncResponse
                                {
                                    OfflineId = game.Id,
                                    Id = newGame.Id,
                                    State = ItemState.Created,
                                    Replacement = GameTransferObject.FromGame(newGame)
                                });
                                break;
                            case Models.ModelState.Modified:
                                var gameToUpdate = Game.FromTransferObject(game);
                                var updatedGame = GamesService.UpdateGame(gameToUpdate, userName);
                                result.Add(new ItemSyncResponse
                                {
                                    OfflineId = game.Id,
                                    Id = gameToUpdate.Id,
                                    State = ItemState.Updated,
                                    Replacement = GameTransferObject.FromGame(updatedGame)
                                });
                                break;
                            case Models.ModelState.Deleted:
                                var id = Guid.Parse(game.Id);
                                GamesService.DeleteGame(id);
                                result.Add(new ItemSyncResponse
                                {
                                    Id = id,
                                    OfflineId = id.ToString(),
                                    State = ItemState.Deleted
                                });
                                break;
                        }
                    });
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return result;
        }

        private List<ItemSyncResponse> SyncCategories(SyncTransferObject transferObject, string userName)
        {
            var result = new List<ItemSyncResponse>();
            using (var transaction = CategoriesService.NewTransaction())
            {
                try
                {
                    transferObject.Categories.ForEach(categoryTransferObject =>
                    {
                        var modelState = (Models.ModelState) categoryTransferObject.State;
                        switch (modelState)
                        {
                            case Models.ModelState.New:
                                var category =
                                    CategoriesService.AddCategory(Category.FromTransferObject(categoryTransferObject), userName);
                                result.Add(new ItemSyncResponse
                                {
                                    OfflineId = categoryTransferObject.Id,
                                    Id = category.Id,
                                    State = ItemState.Created,
                                    Replacement = CategoryTransferObject.FromCategory(category)
                                });
                                break;
                            case Models.ModelState.Modified:
                                var cat = Category.FromTransferObject(categoryTransferObject);
                                var updatedCategory = CategoriesService.UpdateCategory(cat, userName);
                                result.Add(new ItemSyncResponse
                                {
                                    Id = cat.Id,
                                    OfflineId = cat.Id.ToString(),
                                    State = ItemState.Updated,
                                    Replacement = CategoryTransferObject.FromCategory(updatedCategory)
                                });
                                break;
                            case Models.ModelState.Deleted:
                                var id = Guid.Parse(categoryTransferObject.Id);
                                CategoriesService.DeleteCategory(id);
                                result.Add(new ItemSyncResponse
                                {
                                    Id = id,
                                    OfflineId = categoryTransferObject.Id,
                                    State = ItemState.Deleted
                                });
                                break;
                        }
                    });
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            return result;
        }
    }
}