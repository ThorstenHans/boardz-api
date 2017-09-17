using System;
using System.Collections.Generic;
using System.Linq;
using BoardZ.API.Database;
using BoardZ.API.Extensions;
using BoardZ.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BoardZ.API.Exceptions;
using System.Net;

namespace BoardZ.API.Services
{
    public class CategoriesService : IDisposable
    {
        protected BoardZContext Context { get; }

        public CategoriesService(BoardZContext context)
        {
            Context = context;
        }

        public IList<Category> GetAll(string userName, byte[] rowVersion)
        {
            IQueryable<Category> query = Context.Categories
                .Include(category => category.GameCategories)
                .ThenInclude(gamesCategory => gamesCategory.Game)
                .Where(category => category.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(category => category.Name);
            if (rowVersion != null)
            {
                query = query
                    .Where(category => category.RowVersion.Compare(rowVersion) > 0);
            }
            return query.ToList();

        }

        public IList<Category> GetAll(string username)
        {
            return Context.Categories
                .Include(category => category.GameCategories)
                .ThenInclude(gamesCategory => gamesCategory.Game)
                    .Where(c => c.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(category => category.Name)
                    .ToList();
        }

        public int GetCount(string username)
        {
            return Context.Categories.Count(category =>
                category.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase));
        }

        public Category AddCategory(Category category, string username)
        {
            try
            {
                category.Id = Guid.NewGuid();
                category.UserName = username;
                Context.Categories.Add(category);
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
            return category;
        }

        public Category GetById(Guid id, string username)
        {
            var result = Context.Categories
                .Include(category => category.GameCategories)
                .ThenInclude(gamesCategory => gamesCategory.Game)
                .Where(category => category.UserName.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault(category => category.Id.Equals(id));

            if (result == null)
            {
                throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Category with id {id} not found");
            }
            return result;
        }

        public void DeleteCategory(Guid id)
        {
            var found = Context.Categories
                .Include(category => category.GameCategories)
                .FirstOrDefault(category => category.Id.Equals(id));
            if (found == null)
            {
                throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Category with id {id} not found");
            }
            try
            {
                if (found.GameCategories.Any())
                {
                    throw new HttpStatusCodeException(HttpStatusCode.BadRequest, $"Category with id {id} is already in use.");
                }
                Context.Entry(found).State = EntityState.Deleted;
                Context.SaveChanges();
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        public Category UpdateCategory(Category category, string username)
        {
            try
            {
                var found = Context.Categories.FirstOrDefault(c => c.Id.Equals(category.Id) && c.UserName.Equals(username));
                if (found == null)
                    throw new HttpStatusCodeException(HttpStatusCode.NotFound, $"Category with id {category.Id} not found");
                Context.Entry(found).CurrentValues.SetValues(category);
                Context.SaveChanges();
                return found;
            }
            catch (Exception e)
            {
                throw new HttpStatusCodeException(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public IDbContextTransaction NewTransaction()
        {
            return Context.Database.BeginTransaction();
        }
    }
}
