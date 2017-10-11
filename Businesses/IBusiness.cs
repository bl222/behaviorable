/**
 *  Programmed by Benoit Lanteigne
 *  (c) all rights reserved
 *  Licensed under MIT
 */

using System.Linq;

namespace Behaviorable.Businesses
{
    
    /// <summary>
    /// Interface defining a Behaviorable business. A Behaviorable business is inspired by
    /// cakePHP's models. A business is associated to a data source. The data source could be
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
        /// In term of CRUD it correspond to a read.
        /// </summary>
        /// <param name="type">A string representing the type of find that is performed.
        /// Basically, the type affects what will be returned by find method as differents
        /// perform different querying.</param>
        /// <param name="parameters">Parameters used to customized the find type behavior if necessary</param>
        /// <returns>An IQueryable object that represents the finded data</returns>
        IQueryable<T> Find(string type = "default", BusinessParameters parameters = null);

        /// <summary>
        /// A find method. Used to query the data source associated with the business. 
        /// In term of CRUD it correspond to a read. FindFirst returns only the first element
        /// that is found instead of all of them. This is useful for cases when performin
        /// a search that can only return one element.
        /// </summary>
        /// <param name="type">A string representing the type of find that is performed.
        /// Basically, the type affects what will be returned by find method as differents
        /// perform different querying.</param>
        /// <param name="parameters">Parameters used to customized the find type behavior if necessary</param>
        /// <returns>An IQueryable object that represents the finded data</returns>
        T FindFirst(string type, BusinessParameters parameters = null);

        /// <summary>
        /// Perform a save operation on the associated data source. In term of crud, the save
        /// method performs both the Create and Update.
        /// </summary>
        /// <param name="toSave">The data that is saved</param>
        /// <param name="parameters">Parameters used to customized the save functinality if necessary</param>
        /// <returns>True if the save succeeded, else false</returns>
        bool Save(T toSave, BusinessParameters parameters = null);

        /// <summary>
        /// Performs a delete operation on the associated data source. In term of crud, the delete
        /// method performs the Delete.
        /// </summary>
        /// <param name="toDelete">The object that will be deleted</param>
        /// <param name="parameters">Parameters used to customized the delete functinality if necessary</param>
        /// <returns>True if the deletee succeeded, else false</returns>
        bool Delete(T toDelete, BusinessParameters parameters = null);

    }
}
