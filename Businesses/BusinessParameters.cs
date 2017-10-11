/**
 *  Programmed by Benoit Lanteigne
 *  (c) all rights reserved
 *  Licensed under MIT
 */

using System.Collections.Generic;
using System.Linq;

namespace Behaviorable.Businesses
{
    /// <summary>
    /// A Business CRUD related methods takes an extra parameter. This parameter is basically a dictionarly using string for keys
    /// and dynamic as a value. It is possible to save another such directory under a key for hiearchical data. The methods take
    /// such parameters in order to be easily customizable when creating a new business type. 
    /// 
    /// Instead of working directly with the Dictionary type, the business uses objects of the type BusinessParameters for this tasks.
    /// A business parameter object is the same as a Dictionary<string, dynamic> object except with extra methods to facilitate
    /// working with hiearchal data.
    /// </summary>
    public class BusinessParameters : Dictionary<string, dynamic>
    {
        public BusinessParameters()
        {

        }

        /// <summary>
        /// Obtains a value stored deeply into an hiarchy of BusinessParameters objects. Takes a serie of keys delimited by
        /// dots, such as this:
        /// 
        /// key1.key2.key3
        /// 
        /// This means it will find the value stored in key one at the top level. This value should be a second BusinessParameters.
        /// The method will then find the value under key2 in the second BusinessParameters. This should be a third BusinessParameters
        /// object. The method will then find the value under key3 in the third BusinessParameters and return it.
        /// 
        /// This simplify fetching deep hiarchical values because there is no need to check wheter all those keys exist manuallyl,
        /// the GetDeepValue methods handles it automatically.
        /// </summary>
        /// <param name="key">A serie of keys delimited by dots.</param>
        /// <returns>The value under the key, or null is such a value does not exists.</returns>
        public dynamic GetDeepValue(string key)
        {
            string[] keyParts = key.Split('.');
            BusinessParameters parameters = this;

            // Iterate through every keys separated by dots.
            int max = keyParts.Count() - 1;
            for (int i = 0; i <= max; ++i)
            {
                // Get the value under the current key or null if it doesn't exists
                var tmp = parameters.ContainsKey(keyParts[i]) ? parameters[keyParts[i]] : null;

                if (tmp == null)
                {
                    //The value specified by the dot separated keys does not exist so return null
                    return null;
                }
                else if (i == max)
                {
                    // This is the last key thus return the value
                    return tmp;
                }
                else if (tmp is BusinessParameters)
                {
                    // This is before the last key so go one level
                    // deeper in the hiearchie.
                    parameters = tmp;
                }
                else
                {
                    return null;
                }


            }

            return null;
        }

        public void SetDeepValue(string key, dynamic value)
        {
            BusinessParameters.SetDeepValue(this, key, value);
        }

        /// <summary>
        /// Save a value in the hiearchy of BusinessParameters objects using a key made of several dot separated keys.
        /// like as this one:
        /// 
        /// key1.key2.key3
        /// 
        /// The method starts at the top level. It checks if there is already a second BusinessParameters object under the key1 key.
        /// If not, a second BusinessParameters object is created and set under key one, if there was already one, then the already 
        /// existing BusinessParameters is used. Then, the method checks if there is already a third BusinessParameters object
        /// under the key2 keyof the second BusinessParameters object. If not, a third business parameter is created and set under
        /// the key2, if there was already one the existing BusinessParameters is used. Then, the method saves the new value under key3
        /// in the third BusinessParameters.
        /// 
        /// The advantage of the static method vs the non static method is that it can start from a null value.
        /// </summary>
        /// <param name="parameters">The top level of the BusinessParameters hiearchy. It can be null. If null, a new
        /// hiearchy is created from scratch</param>
        /// <param name="key">A key made of several dot separted keys</param>
        /// <param name="value">The value that will be set</param>
        /// <returns>The new hiearchy containing the new value</returns>
        static public BusinessParameters SetDeepValue(BusinessParameters parameters, string key, dynamic value)
        {
            // No top of hiearchy passed as a parameter, so start a new hiearchy.
            if (parameters == null)
            {
                parameters = new BusinessParameters();
            }

            string[] keyParts = key.Split('.');
            BusinessParameters originalParams = parameters;

            // For every part of the key
            int max = keyParts.Count() - 1;
            for (int i = 0; i <= max; ++i)
            {
                if (i == max)
                {
                    // Last part of the key, set value under the last dot delimited key
                    parameters[keyParts[i]] = value;
                }
                else
                {
                    //If there isn't already a BusinessParameters object under the current key, create a new
                    // one and set it under the current key
                    if (!parameters.ContainsKey(keyParts[i]) || !(parameters[keyParts[i]] is BusinessParameters))
                    {
                        parameters[keyParts[i]] = new BusinessParameters();
                    }

                    // go one level deeper into the hiearchy
                    parameters = parameters[keyParts[i]];
                }
            }

            return originalParams;
        }
    }

}