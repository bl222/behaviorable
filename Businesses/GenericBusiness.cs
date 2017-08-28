
using System;
using System.Collections.Generic;
using System.Linq;
using Behaviorable.Attributes;
using Behaviorable.Behaviors;

namespace Behaviorable.Businesses
{
    public delegate bool? SaveBehaviorDelegate<T>(T toSave, BusinessParameters parameters);
    public delegate bool? DeleteBehaviorDelegate<T>(T toSave, BusinessParameters parameters);
    public delegate IQueryable<T> FindBehaviorDelegate<T>(string type, BusinessParameters parameters, IQueryable<T> results = null);
    public delegate IQueryable<T> FindDelegate<T>(BusinessParameters parameters);


    public abstract class GenericBusiness<T> : IBusiness<T>
    {

        private List<FindBehaviorDelegate<T>> _beforeFinds = new List<FindBehaviorDelegate<T>>();
        private List<FindBehaviorDelegate<T>> _afterFinds = new List<FindBehaviorDelegate<T>>();
        private List<SaveBehaviorDelegate<T>> _beforeSaves = new List<SaveBehaviorDelegate<T>>();
        private List<SaveBehaviorDelegate<T>> _afterSaves = new List<SaveBehaviorDelegate<T>>();
        private List<DeleteBehaviorDelegate<T>> _beforeDeletes = new List<DeleteBehaviorDelegate<T>>();
        private List<DeleteBehaviorDelegate<T>> _afterDeletes = new List<DeleteBehaviorDelegate<T>>();

        private Dictionary<string, FindDelegate<T>> _finds = new Dictionary<string, FindDelegate<T>>();


        

        public GenericBusiness()
        {

            InitCustomFinders(this);
            
            this.InitBehaviors();
        }

  

        protected void InitCustomFinders(object target)
        {
            var methodsInfo = target.GetType().GetMethods().Where(m => m.GetCustomAttributes(true).OfType<BusinessFind>().Count() > 0);

            foreach (var methodInfo in methodsInfo)
            {
                var name = methodInfo.GetCustomAttributes(false).OfType<BusinessFind>().First().Name;
                var f = (FindDelegate<T>)methodInfo.CreateDelegate(typeof(FindDelegate<T>), target);

                if (!_finds.Keys.Contains(name))
                {
                    _finds.Add(name, f);
                }
                
            }
        }
        protected void InitBehaviors()
        {
            var propertiesInfo = this.GetType().GetProperties();//.Where(p => p.PropertyType == typeof(IBehavior<T>));

            foreach (var propertyInfo in propertiesInfo)
            {
                IBehavior<T> behavior = propertyInfo.GetValue(this) as IBehavior<T>;

                if(behavior != null)
                {
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

                    InitCustomFinders(behavior);
                }



            }
        }

        public virtual void Initialize()
        {

        } 

        public IQueryable<T> Find(string type = "all", BusinessParameters parameters = null)
        {
            IQueryable<T> results;
            foreach (FindBehaviorDelegate <T> bf in _beforeFinds)
            {
                bf(type, parameters);
            }

            if(_finds.Keys.Contains(type))
            {
                results = _finds[type](parameters);
            }
            else
            {
                throw new Exception("Attempted to use a non defined find type on a behaviorable business");
            }

            

            foreach (FindBehaviorDelegate<T> bf in _afterFinds)
            {
                results = bf(type, parameters, results);
            }

            return results as IQueryable<T>;
        }

        public virtual T FindFirst(string type, BusinessParameters parameters = null)
        {
            return (this.Find(type, parameters) as IQueryable<T>).FirstOrDefault();
        }


        public bool Save(T toSave, BusinessParameters parameters = null)
        {
            bool interruptSave = false;
            foreach(SaveBehaviorDelegate<T> bs in _beforeSaves)
            {
                bool? result = bs(toSave, parameters);
                if (result == false)
                {
                    return false;
                } else if(result == null)
                {
                    interruptSave = true;
                }
            }

            if(!interruptSave)
            {
                if (!SaveHelper(toSave, parameters))
                {
                    return false;
                }
            }

            interruptSave = false;
            foreach (SaveBehaviorDelegate<T> asd in _afterSaves)
            {
                bool? success = asd(toSave, parameters);

                if(success == false)
                {
                    return false;
                } else if(success == null)
                {
                    interruptSave = true;
                }
            }

            return interruptSave ? false : true;

        }


        public bool Delete(T toDelete, BusinessParameters parameters = null)
        {
            bool interruptDelete = false;
            foreach (DeleteBehaviorDelegate<T> bd in _beforeDeletes)
            {
                bool? result = bd(toDelete, parameters);
                if (result == false)
                {
                    return false;
                } else if(result == null)
                {
                    interruptDelete = true;
                }
            }

            if(!interruptDelete)
            {
                if (!DeleteHelper(toDelete, parameters))
                {
                    return false;
                }
            }

            interruptDelete = false;
            foreach (DeleteBehaviorDelegate<T> ad in _afterDeletes)
            {
                bool? result = ad(toDelete, parameters);

                if(result == false)
                {
                    return false;
                } else if(result == null)
                {
                    interruptDelete = true;
                }
            }

            return interruptDelete ? false : true;
        }

       

        protected abstract bool SaveHelper(T toSave, BusinessParameters parameters = null);

        protected abstract bool DeleteHelper(T toDelete, BusinessParameters parameters = null);
    }
}