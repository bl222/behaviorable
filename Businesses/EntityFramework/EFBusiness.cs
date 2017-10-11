using Behaviorable.Attributes;
using Behaviorable.Behaviors.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Behaviorable.Businesses.EntityFramework
{
    public class EFBusinessQuery<T>
    {
        public System.Linq.Expressions.Expression<Func<T, bool>> Where { get; set; }
        public System.Linq.Expressions.Expression<Func<T, dynamic>> OrderBy { get; set; }

    }

    /// <summary>
    /// EFBusiness is Business built for Entity Framework. If you aren't using
    /// entity framework, then EFBusiness isn't useful to you can be studied as exemple to implement a business type for
    /// the technologie you are using. Basicaly, an EF business can manage one of the Poco class representing a database table. The
    /// business can then be used to perform CRUD operations for that table.
    /// 
    /// While EfBusiness can be instanciated and used directly, it is usually a better idea
    /// to create a new business class inheriting from EFBusiness and add revelant behaviors, custom finders and other there.
    /// </summary>
    /// <typeparam name="Poco">The type of the EF poco class managed by the business</typeparam>
    /// <typeparam name="Context">The type of the database context used to access to the database with entity framework</typeparam>
    public class EFBusiness<Poco, Context> : GenericBusiness<Poco>
        where Poco : class, IBasePoco
        where Context : DbContext
    {
        /// <summary>
        /// The DbContext for the current database
        /// </summary>
        public Context Db { get; set; }

        // The database table represented by the Poco objects managed by the EFBusiness.
        // This is here to simplify querying 
        public DbSet<Poco> Table { get; set; }

        /// <summary>
        /// Constructor. Initialize the Table and DB properties
        /// </summary>
        /// <param name="db">The DbContext for the current database</param>
        /// <param name="table">The database table represented by the Poco objects managed by the EFBusiness</param>
        public EFBusiness(Context db, DbSet<Poco> table)
        {
            Table = table;
            Db = db;
        }

        /// <summary>
        /// Implements a custom finder that can fetch all the rows in the table managed by the EFBusiness.
        /// Can be called through Find using Find("all")
        /// </summary>
        /// <param name="parameters">Used to customize the find behavior. In DefaultFind's case,
        /// you can specifiy a EFBusinessQuery under a "query" key in the BusinessParameters parameter 
        /// to either or both a Where clause or an order clause to the find</param>
        /// <returns>A collection of Poco object for all the fetched rows</returns>
        [BusinessFind("all")]
        public virtual IQueryable<Poco> DefaultFind(BusinessParameters parameters)
        {
            IQueryable<Poco> target = Table;

            // If parameters contains a "query" key
            if (parameters != null && parameters.Keys.Contains("query"))
            {
                // Apply the passed queries to the find operation
                EFBusinessQuery<Poco> query = parameters["query"];

                if (query.Where != null)
                {
                    target = target.Where(query.Where);
                }

                if (query.OrderBy != null)
                {
                    target = target.OrderBy(query.OrderBy);
                }
            }

            return target;
        }

        /// <summary>
        /// A custom find that can be used to find a row with a specific ID from the managed table.
        /// Can be called using either Find("id") or FindFirst("id"). Since ID are uniques, only one row can be fetched
        /// and so it is recomended to use FindFirst in this case.
        /// </summary>
        /// <param name="parameters">The id is specified under the "id" key of BusinessParameters parameters</param>
        /// <returns>A collection of POCO  containing a single POCO. The POCO reperesents the table row matching the given ID.
        /// When calling IdFind via FindFirst("id"), the collection is stripped out and the single POCO object
        /// is returned directly</returns>
        [BusinessFind("id")]
        public IQueryable<Poco> IdFind(BusinessParameters parameters)
        {
            int id = parameters["id"];
            return Table.Where(p => p.ID == id);
        }

        /// <summary>
        /// This is a convinence method that can be usined instead of FindFirst("id"). It takes the id directly as a int and so 
        /// there is no need to create a Businessparameters object and it streamlines the process of finding an object by
        /// ID
        /// </summary>
        /// <param name="id">The id representing the desired row</param>
        /// <param name="parameters">Can be used to further customize the find with a EFBusinessQuery, but
        /// in this case there usually no reason to use this parameter</param>
        /// <returns></returns>
        public virtual Poco Find(int id, BusinessParameters parameters = null)
        {
            // No BusinessParameters passed, create an empty one
            if (parameters == null)
            {
                parameters = new BusinessParameters();
            }

            //  Use the IdFind custom finder through FindFirst to fetch the row represented by the given IDs
            parameters["id"] = id;
            return this.FindFirst("id", parameters);
        }

        /// <summary>
        /// This is a convenience method to delete a row with only the ID. Useful if you have the id of the row to delete, but
        /// not the POCO object to delete it. GenericBusiness expect having the POCO object to delete normaly, this method allows
        /// you to delete by id without having to write the code to fettch the poco object yourself
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual bool Delete(int id, BusinessParameters parameters = null)
        {
            var toDelete = this.Find(id);

            // If the row exists, delete it through the POCO object as it normally works
            if (toDelete != null)
            {
                return this.Delete(toDelete, parameters);
            }
            return false;
        }

        /// <summary>
        /// This defines how the data is saved to the database. SaveHelper is overriden instead of Save because
        /// Save includes code to call the proper behavior callbacks and we do not want to lose this functionality
        /// </summary>
        /// <param name="toSave">The Poco object that will be saved</param>
        /// <param name="parameters">Currently unused, but could be used to customize the save
        /// functionality in some way</param>
        /// <returns>True if the save succeded, else false</returns>
        protected override bool SaveHelper(Poco toSave, BusinessParameters parameters = null)
        {
            dynamic toSaveTmp = toSave;

            // Since ID is now, this is a create operation
            if (toSaveTmp.ID == null)
            {
                Table.Add(toSave);
                Db.SaveChanges();
            }
            // Since ID isn't null this is an update operation
            else
            {
                Db.Entry(toSave).State = EntityState.Modified;
                Db.SaveChanges();
            }

            return true;
        }

        /// <summary>
        /// This defines how the data is deleted from the database. DeleteHelper is overriden instead of Delete because
        /// Delete includes code to call the proper behavior callbacks and we do not want to lose this functionality
        /// </summary>
        /// <param name="toDelete">The Poco object that will be deleted from the database</param>
        /// <param name="parameters">Currently unused, but could be used to customize the delete
        /// functionality in some way<</param>
        /// <returns>True if delete success, else false</returns>
        protected override bool DeleteHelper(Poco toDelete, BusinessParameters parameters = null)
        {
            Table.Remove(toDelete);
            Db.SaveChanges();
            return true;
        }
    }
}