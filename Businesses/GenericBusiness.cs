/**
 *  Programmed by Benoit Lanteigne
 *  (c) all rights reserved
 *  Licensed under MIT
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Behaviorable.Attributes;
using Behaviorable.Behaviors;

namespace Behaviorable.Businesses
{
    // Delegates used to support behaviors.
    public delegate bool? SaveBehaviorDelegate<T>(T toSave, BusinessParameters parameters);
    public delegate bool? DeleteBehaviorDelegate<T>(T toSave, BusinessParameters parameters);
    public delegate IQueryable<T> FindBehaviorDelegate<T>(string type, BusinessParameters parameters, IQueryable<T> results = null);
    public delegate IQueryable<T> FindDelegate<T>(BusinessParameters parameters);

    /// <summary>
    /// In theory, it would be possible to create business on top of various data persistance technologie.
    /// For exemple, it would be possible to create behavior that uses Entity Framework to access a database,
    /// or SQL, or accesses or NoSQL solution, or XML files or anything really.
    /// 
    /// To create such a business, one could implement the IBusiness interface directly. However, most businesses, 
    /// regardless of the underlying technologie, share common functionalities, mostly behaviors support. GenericBusiness
    /// implement this common functionnalities and thus in most case it is faster and easier for a new business type
    /// to inherit from GenericBusiness instead of implementing the IBusiness interface.
    /// </summary>
    /// <typeparam name="T">The type of object managed by the business</typeparam>
    public abstract class GenericBusiness<T> : IBusiness<T>
    {

        // These list are used to contain behaviors callbacks. They are a key part of  
        // GenericBusiness' support of behaviors
        private List<FindBehaviorDelegate<T>> _beforeFinds = new List<FindBehaviorDelegate<T>>();
        private List<FindBehaviorDelegate<T>> _afterFinds = new List<FindBehaviorDelegate<T>>();
        private List<SaveBehaviorDelegate<T>> _beforeSaves = new List<SaveBehaviorDelegate<T>>();
        private List<SaveBehaviorDelegate<T>> _afterSaves = new List<SaveBehaviorDelegate<T>>();
        private List<DeleteBehaviorDelegate<T>> _beforeDeletes = new List<DeleteBehaviorDelegate<T>>();
        private List<DeleteBehaviorDelegate<T>> _afterDeletes = new List<DeleteBehaviorDelegate<T>>();

        // This list allows you to create a custom finders
        private Dictionary<string, FindDelegate<T>> _finds = new Dictionary<string, FindDelegate<T>>();


        public GenericBusiness()
        {
            InitCustomFinders(this);
            InitBehaviors();
        }

        /// <summary>
        /// Implements the Find method defined by the IBehavior interface. Finds the data
        /// represented by a given find type and extra parameters in a way that allows
        /// behaviors support
        /// </summary>
        /// <param name="type">The type. Changing the type modifies the way the find is performed.
        /// Each type correspond to a custom finder method. For more info on custom finders methods,
        /// see the comments for the InitCustomFinders method of this class. For a concrete example of
        /// a custom finder being defined, see the EFBusiness class</param>
        /// <param name="parameters">Custom parameters. Not used by default, but can be used
        /// to pass special parameters to custom finders to further customize their functionality</param>
        /// <returns>An IQueryable that contains the results of the Find</returns>
        public IQueryable<T> Find(string type = "all", BusinessParameters parameters = null)
        {
            IQueryable<T> results;

            // Call the BeforeFind method of any attached behaviors (if there are any)
            foreach (FindBehaviorDelegate <T> bf in _beforeFinds)
            {
                bf(type, parameters);
            }

            // Execute the custom finder method corresponding the type passed as a parameter.
            // This call perfomrs the actual find functionality
            if(_finds.Keys.Contains(type))
            {
                results = _finds[type](parameters);
            }
            else
            {
                throw new Exception("Attempted to use a non defined find type on a behaviorable business");
            }

            
            // Call the AfterFind method of any attached behaviors. The AfterFind
            // can then modify the results of the Find if necessary
            foreach (FindBehaviorDelegate<T> bf in _afterFinds)
            {
                results = bf(type, parameters, results);
            }

            return results as IQueryable<T>;
        }

        /// <summary>
        /// Finds the first element obtained by a given find type. This is useful in cases where a find operation can only possibly 
        /// return one value
        /// </summary>
        /// <param name="type">The type. Indicates which custom finder will be used by the find method</param>
        /// <param name="parameters"></param>
        /// <returns>Custom parameters. Not used by default, but can be used
        /// to pass special parameters to custom finders to further customize their functionality</returns>
        public virtual T FindFirst(string type, BusinessParameters parameters = null)
        {
            return (this.Find(type, parameters) as IQueryable<T>).FirstOrDefault();
        }

        /// <summary>
        /// Implements the Save method defined by the IBusiness interface. Create or update data in the storage solution in
        /// a way that supports behavior
        /// </summary>
        /// <param name="toSave">The data that will be saved</param>
        /// <param name="parameters">Custom parameters. Not used by default, but can be used
        /// to pass special parameters to extended Save methods to further customize their functionality</param>
        /// <returns>True if the save operation succeeded, else false</returns>
        public bool Save(T toSave, BusinessParameters parameters = null)
        {
            // Call the BeforeSave method of any attached behaviors (if there are any)
            bool interruptSave = false;
            foreach(SaveBehaviorDelegate<T> bs in _beforeSaves)
            {
                bool? result = bs(toSave, parameters);
                if (result == false)
                {
                    // if the BeforeSave method of the current behavior returned false, the
                    //current save operation is cancelled. Every BeforeSave calls on any behaviors
                    // attached after the current one will not execute. None of the AfterSaves defined
                    // on attached behaviors will execute.
                    return false;
                } else if(result == null)
                {
                    //If the BeforeSave Method returned null, the save wil be "soft" cancelled. This means the data
                    //isn't saved in the normal way, but the BeforeSave of behaviors attached to the business
                    //after the current one will still be executed. The AfterSave of attached behaviors will also
                    // be executed (unless another BeforeSave further down the line returns false)
                    interruptSave = true;
                }
            }

            //If the save hasn't been interrupted be a BeforeSave returning null, perform the save
            if(!interruptSave)
            {
                if (!SaveHelper(toSave, parameters))
                {
                    // Save failed, return false
                    return false;
                }
            }

            // Call the AfterSave method of any attached behaviors (if there are any)
            interruptSave = false;
            foreach (SaveBehaviorDelegate<T> asd in _afterSaves)
            {
                bool? success = asd(toSave, parameters);

                if(success == false)
                {
                    // if the AfterSave method of the current behavior returned false, the Save method ends
                    // here. In other words, the AfterSave method for the behaviors attached after the current one
                    // will not be executed.
                    return false;
                } else if(success == null)
                {
                    // if the AfterSave method of the current behavior returned null, the other AfterSave for behaviors
                    // attached after the current one are still executed, however the Save method will return false instead
                    // of true as if the Save failed.
                    interruptSave = true;
                }
            }

            return interruptSave ? false : true;

        }

        /// <summary>
        /// Implements the Delete method defined by the IBusiness interface. Deletes data in the storage solution in
        /// a way that supports behavior
        /// </summary>
        /// <param name="toDelete">The data that will be deleted</param>
        /// <param name="parameters">Custom parameters. Not used by default, but can be used
        /// to pass special parameters to extended Delete methods to further customize their functionality</param>
        /// <returns>True if the save operation succeeded, else false</returns>
        public bool Delete(T toDelete, BusinessParameters parameters = null)
        {
            // Call the BeforeDelete method of any attached behaviors (if there are any)
            bool interruptDelete = false;
            foreach (DeleteBehaviorDelegate<T> bd in _beforeDeletes)
            {
                bool? result = bd(toDelete, parameters);
                if (result == false)
                {
                    // if the BeforeDelete method of the current behavior returned false, the
                    //current delete operation is cancelled. Every BeforeDelete calls on any behaviors
                    // attached after the current one will not execute. None of the AfterDelete defined
                    // on attached behaviors will execute.
                    return false;
                } else if(result == null)
                {
                    //If the BeforeDelete Method returned null, the delete wil be "soft" cancelled. This means the data
                    //isn't deleted in the normal way, but the BeforeDelete of behaviors attached to the business
                    //after the current one will still be executed. The AfterDelete of attached behaviors will also
                    // be executed (unless another BeforeSave further down the line returns false)
                    interruptDelete = true;
                }
            }

            //If the delete hasn't been interrupted be a BeforeDelete returning null, perform the deletion
            if (!interruptDelete)
            {
                if (!DeleteHelper(toDelete, parameters))
                {
                    return false;
                }
            }

            // Call the AfterDelete method of any attached behaviors (if there are any)
            interruptDelete = false;
            foreach (DeleteBehaviorDelegate<T> ad in _afterDeletes)
            {

                bool? result = ad(toDelete, parameters);

                if(result == false)
                {
                    // if the AfterDelete method of the current behavior returned false, the Save method ends
                    // here. In other words, the AfterDelete method for the behaviors attached after the current one
                    // will not be executed.
                    return false;
                } else if(result == null)
                {
                    // if the AfterDelete method of the current behavior returned null, the other AfterDelete for behaviors
                    // attached after the current one are still executed, however the Delete method will return false instead
                    // of true as if the deletion failed.
                    interruptDelete = true;
                }
            }

            return interruptDelete ? false : true;
        }

        /// <summary>
        /// The method InitCustomFinders finds all the custom finders defined on a busines or behavior. A custom finder method can be
        /// called by the Find of FindFirst method of the GenericBusiness through the type parameter. The advangtage of this is you can create new way to
        /// find data without having to worry about the events that need to be fired when finding said data. The Find method handles the boilerplate
        /// code that ensure the proper events are fired for you.
        /// 
        /// To define a custom finder, you simply write a method taking a BusinessParameter object as a parameter and use the BusinessFind
        /// attribute on it in order to specify the string used to identify the custom finder when calling Find through the type parameter of
        /// Find.
        /// </summary>
        /// <param name="target">The object for which the custom finders are initialized. This is usually a business or a behavior. </param>
        protected void InitCustomFinders(object target)
        {
            // Get all methods marked by the BusinessFind Attribute
            var methodsInfo = target.GetType().GetMethods().Where(m => m.GetCustomAttributes(true).OfType<BusinessFind>().Count() > 0);

            foreach (var methodInfo in methodsInfo)
            {
                //Get the type of the custom find via the BusinessFind attribute
                var type = methodInfo.GetCustomAttributes(false).OfType<BusinessFind>().First().Type;

                // Create a new delegate for the custom find and save this delegate in the proper dictionary
                // using type as a key. This way, the Find and FindFirst methods can easily call the Custom Method
                // when given the type (as a string) as the first parameter.
                var f = (FindDelegate<T>)methodInfo.CreateDelegate(typeof(FindDelegate<T>), target);
                if (!_finds.Keys.Contains(type))
                {
                    _finds.Add(type, f);
                }

            }
        }


        /// <summary>
        /// InitBehaviors initialize the behavior attached to a Business to make sure those 
        /// behavior work properly. GenericBusiness doesn't have any behavior, but 
        /// its descendant can have them. Adding a behavior to a Business gives it extra functionalities. Several business can use
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
        /// </summary>
        protected void InitBehaviors()
        {
            // Iterate through the business' properties
            var propertiesInfo = this.GetType().GetProperties();
            foreach (var propertyInfo in propertiesInfo)
            {
                // Check if the property is a behavior
                IBehavior<T> behavior = propertyInfo.GetValue(this) as IBehavior<T>;
                if (behavior != null)
                {
                    // Create delegates pointing to the behaviors call back and adds those delegate to the proper
                    // collection so those callback are called when the corresponding CRUD operation is performed
                    FindBehaviorDelegate<T> beforeFind = new FindBehaviorDelegate<T>(behavior.BeforeFind);
                    FindBehaviorDelegate<T> afterFind = new FindBehaviorDelegate<T>(behavior.AfterFind);
                    SaveBehaviorDelegate<T> beforeSave = new SaveBehaviorDelegate<T>(behavior.BeforeSave);
                    SaveBehaviorDelegate<T> afterSave = new SaveBehaviorDelegate<T>(behavior.AfterSave);
                    DeleteBehaviorDelegate<T> beforeDelete = new DeleteBehaviorDelegate<T>(behavior.BeforeDelete);
                    DeleteBehaviorDelegate<T> afterDelete = new DeleteBehaviorDelegate<T>(behavior.AfterDelete);

                    _beforeFinds.Add(beforeFind);
                    _afterFinds.Add(afterFind);
                    _beforeSaves.Add(beforeSave);
                    _afterSaves.Add(afterSave);
                    _beforeDeletes.Add(beforeDelete);
                    _afterDeletes.Add(afterDelete);

                    // A behavior can have custom finders, initialize these custom finders so they are available 
                    // to the business.
                    InitCustomFinders(behavior);
                }

            }
        }

        /// <summary>
        /// Inheriting businesses should implement the SaveHelper method in order to provide the actual saving functionality.
        /// It is better to implement the save functionality in this method than by overriding the Save method because it means
        /// behaviors are treated correctly whithout having to write any extra code. On the other hand, overriding Save can be
        /// useful in cases where behavior support must be unconditionaly removed, or need a special treatment not offered by
        /// GenericBusiness' implementation.
        /// </summary>
        /// <param name="toSave">The data to be saved</param>
        /// <param name="parameters">Custom parameters. Not used by default, but can be used
        /// to pass special parameters to extended Save methods to further customize their functionality</param>
        /// <returns>True if the save operation succeeded, else false</returns>
        protected abstract bool SaveHelper(T toSave, BusinessParameters parameters = null);

        /// <summary>
        /// Inheriting businesses should implement the DeleteHelper method in order to provide the actual deleting functionality.
        /// It is better to implement the delete functionality in this method than by overriding the Delete method because it means
        /// behaviors are treated correctly whithout having to write any extra code. On the other hand, overriding Save can be
        /// useful in cases where behavior support must be unconditionaly removed, or need a special treatment not offered by
        /// GenericBusiness' implementation.
        /// </summary>
        /// <param name="toDelete">The data to be deleted</param>
        /// <param name="parameters">Custom parameters. Not used by default, but can be used
        /// to pass special parameters to extended Delete methods to further customize their functionality</param>
        /// <returns>True if the delete operation succeeded, else false</returns>
        protected abstract bool DeleteHelper(T toDelete, BusinessParameters parameters = null);
    }
}