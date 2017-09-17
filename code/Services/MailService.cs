using BoardZ.API.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BoardZ.API.Models;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BoardZ.API.Services
{

    
    public class MailService
    {
        public MailService(IOptions<BoardZConfiguration> configuration)
        {
            Configuration = configuration.Value;
        }

        protected BoardZConfiguration Configuration { get; }

        public Task<Response> SendOnGameAdded(Game game)
        {
            var token = Configuration.SendGridApiToken;
            var client = new SendGridClient(token);
            var from = new EmailAddress("boardz@thinktecture.com", "BoardZ Mailer");
            var to = new EmailAddress(Configuration.MailRecipient);
            var content =
                $"Hey,\r\nThis is BoardZ!. Thanks for adding {game.Name} to our App.\r\nWe hope you enjoy playing.";
            var htmlContent =
                $"Hey,<br/><br/>This is <b>BoardZ!</b>. Thanks for adding <i>{game.Name} to our App.<br/><br/>We hope you enjoy playing.";
            var message = MailHelper.CreateSingleEmail(from, to, $"Thanks for adding {game.Name} to BoardZ", content,
                htmlContent);
            return client.SendEmailAsync(message);
        }
    }
}
