/**
 *  Programmed by Benoit Lanteigne
 *  (c) all rights reserved
 *  Licensed under MIT
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Behaviorable.Business
{
    /// <summary>
    /// Interface defining a Behaviorable business. A Behaviorable business is inspired by
    /// cakePHP's model. A business is associated to a data source. The data source could be
    /// a table from a database, an entity framework object, an xml file. The business can then
    /// perform CRUD operations on the datasource. The main advantage of the business is that 
    /// it is possible to attach behavior to it that modify those crud operation and add extra 
    /// functionality to the business. For example, lets say that a SoftDeletable behavoir is
    /// implemented. The behavior could be added to a business and then the delete and find
    /// operations would operate through the rules of the SoftDelatable behavior.
    /// 
    /// Any Behaviorable Business must implement this interface either directly or through
    /// inheriting from a class implementing it.
    /// </summary>
    /// <typeparam name="T">The type of object managed by the business</typeparam>
    interface IBusiness<T>
    {
        /// <summary>
        /// A find method. Used to query the data source associated with the business. 
        /// In term of CRUD it correspond to a 
        /// </summary>
        /// <param name="type">A string representing the type of find that is performed.
        /// Basically, the type affects what will be returned by find method as differents
        /// perform different querying.</param>
        /// <param name="parameters">Parameters used to customized the find type behavior if necessary</param>
        /// <returns></returns>
        IQueryable<T> Find(string type = "default", IDictionary<string, dynamic> parameters = null);

        /// <summary>
        /// Perform a save operation on the associated data source. In term of crud, the save
        /// method performs both the Create and Update.
        /// </summary>
        /// <param name="toSave"></param>
        /// <returns></returns>
        bool Save(T toSave);

        /// <summary>
        /// Performs a delete operation on the associated data source. In term of crud, the delete
        /// method performs the Delete.
        /// 
        /// </summary>
        /// <param name="toDelete">The object that will be deleted</param>
        /// <returns></returns>
        bool Delete(T toDelete);

    }
}
