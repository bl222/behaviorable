using Behaviorable.Behaviors;
using Behaviorable.Behaviors.EntityFramework;
using Behaviorable.Businesses.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcMovie.Models.Behaviorable.Behaviors.EntityFramework
{
    /// <summary>
    /// If you wish to create a behavior for an entity framework based business, you can inherit
    /// fromm EFBehavior to simplify the process
    /// </summary>
    /// <typeparam name="Poco">The Poco type managed by the business the behavior is attached to</typeparam>
    /// <typeparam name="Db">The type of the database context used to access the database</typeparam>
    /// <typeparam name="Business">The type of the business the behavior is attadched to</typeparam>
    public class EFBehavior<Poco, Db, Business> : BaseBehavior<Poco>
        where Poco : class, IBasePoco
        where Db : System.Data.Entity.DbContext
        where Business : EFBusiness<Poco, Db>

    {
        /// <summary>
        /// A property giving access to the business owning the behavior
        /// </summary>
        protected Business business { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="b">The business the behavior is attached to</param>
        public EFBehavior(Business b)
        {
            business = b;
        }
    }
}