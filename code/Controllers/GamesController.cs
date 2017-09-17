using System;
using BoardZ.API.Extensions;
using BoardZ.API.Models;
using BoardZ.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using BoardZ.API.TransferObjects;

namespace BoardZ.API.Controllers
{
    /// <inheritdoc />
    /// <summary>
    /// Provides a CRUD api for board games
    /// </summary>
    [Route("/api/[controller]")]
    public class GamesController : BaseApiController
    {
        protected GamesService Service { get; }
        protected MailService MailService { get; }

        /// <summary>
        /// default CTOR
        /// </summary>
        public GamesController(GamesService service, MailService mailService)
        {
            Service = service;
            MailService = mailService;
        }

        /// <summary>
        /// Method for loading games since a given row version
        /// </summary>
        /// <param name="rowVersion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("since")]
        public IActionResult GetAllSince(ulong? rowVersion)
        {
            var username = User.GetSubjectOrThrow();
            return Json(Service.GetAll(username, rowVersion).Select(GameTransferObject.FromGame));
        }

        /// <summary>
        /// Lists all games
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IActionResult GetAll()
        {
            var username = User.GetSubjectOrThrow();
            return Json(Service.GetAll(username).Select(GameTransferObject.FromGame));
        }

        /// <summary>
        /// Returns the games count.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("count")]
        public IActionResult GetCount()
        {
            return Ok(Service.GetCount(User.GetSubjectOrThrow()));
        }

        /// <summary>
        /// Adds a new board game
        /// </summary>
        /// <param name="transferObject"></param>
        /// <returns></returns>
        [HttpPost] 
        [Route("")]
        public async Task<IActionResult> Add([FromBody]GameTransferObject transferObject)
        {
            var game = Game.FromTransferObject(transferObject);
            try
            {
                var newGame = Service.AddGame(game, User.GetSubjectOrThrow());
                await MailService.SendOnGameAdded(newGame);
                return Ok(newGame);
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        /// <summary>
        /// Returns a single board game
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetById(Guid id)
        {
            var game = Service.GetById(id, User.GetSubjectOrThrow());
            var transferObject = GameTransferObject.FromGame(game);
            return Ok(transferObject);
        }

        /// <summary>
        /// Removes a board game
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        public IActionResult Remove(Guid id)
        {
            Service.DeleteGame(id);
            return Ok();
        }

        /// <summary>
        /// Updates a board game
        /// </summary>
        /// <param name="transferObject"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        public IActionResult Update([FromBody]GameTransferObject transferObject)
        {
            var userName = User.GetSubjectOrThrow();
            var game = Game.FromTransferObject(transferObject);
            game.UserName = userName;
            Service.UpdateGame(game, userName);
            return Ok();
        }

        /// <summary>
        /// IDisposable
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Service?.Dispose();
        }
    }
}
