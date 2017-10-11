using System.Linq;
using Behaviorable.Businesses;

namespace Behaviorable.Behaviors
{
    /// <summary>
    /// In order to define a behavior, you simply need to create a class that implements the IBehavior interface.
    /// 
    /// Adding a behavior to a Business gives it extra functionalities.Several business can use
    /// the same behavoir and so behavior allows code reuse and avoids code duplication. A Behavior offers three main
    /// way to extend the functionality of a business:
    /// 
    /// 1. It can add new custom finders to a business
    /// 
    /// 2. It can define callbacks which are called on the proper operation:
    ///      -BeforeFind is executed at the begining of a find (before the data has been found)
    ///      -AfterFind is executed after the data has been found
    ///      -BeforeSave is executed before a save operation
    ///      -AfterSave is executed after a save operation
    ///      -BeforeDelete is executed before a delete operation
    ///      -AfterDelete is executed after a delete operation
    ///      
    ///  Using these callback, a behavior can modify the way a business handles CRUD operation without actually modifying the Business
    ///  itself (except for the code adding the behavior).
    ///  
    /// 3. A behavior can have normal methods can then be called through the business like this: BusinessObject.BehaviorObject.MethodToCall
    /// 
    /// The IBehavior interface defines which callbacks a behavior must define
    /// </summary>
    /// <typeparam name="T">The type of data managed by the business to which the behavior is attached</typeparam>
    public interface IBehavior<T>
    {
        /// <summary>
        /// This method will be called back when the Business owning the behavior use the Find method to fetch data. The callback
        /// is called before the date is fetched.
        /// </summary>
        /// <param name="type">The type of the find</param>
        /// <param name="parameters">The business parameters passed to the find</param>
        /// <param name="results">In the case of before find, this is always null. This parameter is still include so
        /// both BeforeFinds and AfterFind delegates can be saved in the same dictionary in GenericBusiness.</param>
        /// <returns>In the case of before find, you should return null. The return value isn't used, but it's still there
        /// so both BeforeFinds and AfterFind delegates can be saved in the same dictionary in GenericBusiness.
        /// </returns>
        IQueryable<T> BeforeFind(string type, BusinessParameters parameters, IQueryable<T> results = null);

        /// <summary>
        /// This method will be called back when the Business owning the behavior use the Find method to fetch data. The callback
        /// is called after the data is fetched.
        /// </summary>
        /// <param name="type">The type of the find</param>
        /// <param name="parameters">The business parameters passed to the find</param>
        /// <param name="results">A collection containing the result from the find operation. The AfterFind method can perform
        /// treatment on these results and return them if necessary</param>
        /// <returns>The collection of data you wish the Find call to return. In the simplest case, this is simply results. It 
        /// can also be a new collection derived from results.
        /// </returns>
        IQueryable<T> AfterFind(string type, BusinessParameters parameters, IQueryable<T> results = null);

        /// <summary>
        /// This method will be called back when the Business owning the behavior use Save method to save data. The callback is called
        /// before the data is saved. 
        /// </summary>
        /// <param name="toSave">The data to be saved</param>
        /// <param name="parameters">Extra parameters that can be used to customize the BeforeSave functionality</param>
        /// <returns>True to indicate everything is alright, false to prevent the saving, null to stop executing the following BeforeSave,
        /// but still save the data</returns>
        bool? BeforeSave(T toSave, BusinessParameters parameters);


        /// <summary>
        /// This method will be called back when the Business owning the behavior use Save method to save data. The callback is called
        /// after the data is saved. 
        /// </summary>
        /// <param name="toSave">The data that was saved</param>
        /// <param name="parameters">Extra parameters that can be used to customize the AfterSave functionality</param>
        /// <returns>True to indicate everything is alright, false to stop executing the folowing AfterSave, 
        /// null to keep executing the following AfterSave but have the Save method return false.
        /// but still save the data</returns>
        bool? AfterSave(T toSave, BusinessParameters parameters);

        /// <summary>
        /// This method will be called back when the Business owning the behavior use Delete method to delete data. The callback is called
        /// before the data is deleted. 
        /// </summary>
        /// <param name="toDelete">The data that was deleted</param>
        /// <param name="parameters">Extra parameters that can be used to customize the BeforeDelete functionality</param>
        /// <returns>True to indicate everything is alright, false to prevent the deleting, null to stop executing the following BeforeDelete,
        /// but still delete the data</returns>
        bool? BeforeDelete(T toDelete, BusinessParameters parameters);

        /// <summary>
        /// This method will be called back when the Business owning the behavior use Delete method to delete data. The callback is called
        /// after the data is deleted. 
        /// </summary>
        /// <param name="toDelete">The data that was deleted</param>
        /// <param name="parameters">Extra parameters that can be used to customize the AfterSave functionality</param>
        /// <returns>True to indicate everything is alright, false to stop executing the folowing AfterDelete, 
        /// null to keep executing the following AfterDelete but have the Delete method return false.
        /// but still save the data</returns>
        bool? AfterDelete(T toDelete, BusinessParameters parameters);

    }

}
