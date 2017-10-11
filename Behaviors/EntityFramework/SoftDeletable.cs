using Behaviorable.Businesses;
using Behaviorable.Businesses.EntityFramework;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;
using System;
using System.Linq;

namespace Behaviorable.Behaviors.EntityFramework
{
    /// <summary>
    /// In order for the SoftDeletable behavior to be able to manipulate an entity framework Poco object,
    /// the Poco in question must implement the ISoftDeletable interface.
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// The table represented by the Poco must have a nullable DeletedAt. If DeletedAt is null, the row
        /// isn't deleted, if it's a date, the row is deleted and and the date is the date of deletion
        /// </summary>
        DateTime? DeletedAt { get; set; }
    }

    /// <summary>
    /// When attached to a business, the SoftDeletable behavior modifes the delete functionality so rows aren't really
    /// deleted, but rather simply marked as deleted. This way, it is easy to restore a deleted row if necessary.
    /// </summary>
    /// <typeparam name="Poco">The Poco type managed by the business the behavior is attached to</typeparam>
    /// <typeparam name="Db">The type of the database context used to access the database</typeparam>
    /// <typeparam name="Business">The type of the business the behavior is attadched to</typeparam>
    public class SoftDeletableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ISoftDeletable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="b">The business the SoftDeletable is attached to</param>
        public SoftDeletableBehavior(Business b)
            : base(b)
        {

        }

        /// <summary>
        /// Actualy delete the row associated to a given Poco. You can use this if you need to really delete a row
        /// instead of just marking it as deleted
        /// </summary>
        /// <param name="toDelete">The Poco object that will be deleted</param>
        /// <param name="parameters">Can customize the functionality of the ForceDelete, not really  useful
        /// for now</param>
        /// <returns>True if the row was deleted, else false</returns>
        public bool ForceDelete(Poco toDelete, BusinessParameters parameters = null)
        {

            parameters = BusinessParameters.SetDeepValue(parameters, "soft-deletable.disable", true);

            return business.Delete(toDelete, parameters);

        }

        /// <summary>
        /// Actualy delete the row associated to a given id. You can use this if you need to really delete a row
        /// instead of just marking it as deleted
        /// </summary>
        /// <param name="toDelete">The id of the row that will be deleted</param>
        /// <param name="parameters">Can customize the functionality of the ForceDelete, not really  useful
        /// for now</param>
        /// <returns>True if the row was deleted, else false</returns>
        public bool ForceDelete(int id, BusinessParameters parameters = null)
        {

            parameters = BusinessParameters.SetDeepValue(parameters, "soft-deletable.mode", "all");
            
            return ForceDelete(business.Find(id, parameters),  parameters);
        }

        /// <summary>
        /// The BeforeDelete callback marks the Poco as deleted and prevent the execution of the actual deletion
        /// </summary>
        /// <param name="toDelete">The Poco object that will be soft deleted</param>
        /// <param name="parameters">You can specify true under [soft-deletable][disable] to delete for real</param>
        /// <returns></returns>
        public override bool? BeforeDelete(Poco toDelete, BusinessParameters parameters)
        {
            if (parameters != null && (parameters.GetDeepValue("soft-deletable.disable") as bool?) == true)
            {
                return true;
            }

            //Marks the object as deleted
            toDelete.DeletedAt = DateTime.Now;
            business.Save(toDelete);

            return null;
        }

        /// <summary>
        /// The after find callback filters the Find results so only the rows that aren't marked as deleted are returned
        /// </summary>
        /// <param name="type">The type of find</param>
        /// <param name="parameters">
        /// Can be used to adapt the functionality of AfterFind depending on the value of ["soft-deletable]["mode"]:
        ///   -If null (the default) or "not-deleted", than AfterFind will only return the rows that aren't marked as deleted
        ///   -If "only-deleted, the AfterFind will only return the rows that are marked as deleted 
        ///   -If any other value (suggested is "all"), than all rows are returned regardless of their deletion status
        /// </param>
        /// <param name="results">The result obtained by the find</param>
        /// <returns>The filtered out results</returns>
        public override IQueryable<Poco> AfterFind(string type, BusinessParameters parameters, IQueryable<Poco> results = null)
        { 
            string mode = parameters != null ? parameters.GetDeepValue("soft-deletable.mode") as string : null;

            if (mode == null)
            {
                mode = "not-deleted";
            }

            if (mode == "not-deleted")
            {
                return results.Where(m => m.DeletedAt == null);
            }
            else if (mode == "only-deleted")
            {
                return results.Where(m => m.DeletedAt != null);
            }


            return results;

        }

    }
}