using Behaviorable.Businesses.EntityFramework;
using System;
using System.Linq;
using Behaviorable.Businesses;
using MvcMovie.Models.Behaviorable.Behaviors.EntityFramework;

namespace Behaviorable.Behaviors.EntityFramework
{
    /// <summary>
    /// In order for the TimeStampable behavior to be able to manipulate an entity framework Poco object,
    /// the Poco in question must implement the IStampable interface.
    /// </summary>
    public interface ITimeStampable
    {
        /// <summary>
        /// The table represented by the Poco must have a nullable CreatedAt and ModifiedAt collumns
        /// </summary>
        DateTime? CreatedAt { get; set; }
        DateTime? ModifiedAt { get; set; }
    }

    /// <summary>
    /// When attached to a business, the TimeStampable behavior manages a CreatedAt and ModifiedAt At collumns.
    /// </summary>
    /// <typeparam name="Poco">The Poco type managed by the business the behavior is attached to</typeparam>
    /// <typeparam name="Db">The type of the database context used to access the database</typeparam>
    /// <typeparam name="Business">The type of the business the behavior is attadched to</typeparam>
    public class TimeStampableBehavior<Poco, Db, Business> : EFBehavior<Poco, Db, Business>
        where Poco : class, IBasePoco, ITimeStampable
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="b"></param>
        public TimeStampableBehavior(Business b)
            : base(b)
        {
            
        }


        /// <summary>
        /// The BeforeSave callback will set the CreatedAt date on creation and the ModifiedAt date on updates.
        /// </summary>
        /// <param name="toSave">The Poco object that is being saved</param>
        /// <param name="parameters">Unused for now</param>
        /// <returns>returns true</returns>
       public override bool? BeforeSave(Poco toSave, BusinessParameters parameters)
        {

            if(toSave.ID == null)
            {
                //The object is being created so set the CreatedAt date.
                toSave.ModifiedAt = null;
                toSave.CreatedAt = DateTime.Now;
            } else
            {
                // The Poco is being updated

                // If the CreatedAt date in the Poco is null, fetch the correct
                //date from the Db so CreatedAt isn't set to null in the db.
                if(toSave.CreatedAt == null)
                {
                    toSave.CreatedAt = (from m in business.Table
                                       where m.ID == toSave.ID
                                       select m.CreatedAt).First();
                }
                toSave.ModifiedAt = DateTime.Now;
            }

            return true;
        }
    }
}