using System.Collections.Generic;
using System.Linq;

namespace Behaviorable.Businesses
{
    public class BusinessParameters : Dictionary<string, dynamic>
    {
        public BusinessParameters()
        {

        }

        public dynamic GetDeepValue(string key)
        {
            string[] keyParts = key.Split('.');
            BusinessParameters parameters = this;

            int max = keyParts.Count() - 1;
            for (int i = 0; i <= max; ++i)
            {
                var tmp = parameters.ContainsKey(keyParts[i]) ? parameters[keyParts[i]] : null;

                if (tmp == null)
                {
                    return null;
                }
                else if (i == max)
                {
                    return tmp;
                }
                else if (tmp is BusinessParameters)
                {
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

        static public BusinessParameters SetDeepValue(BusinessParameters parameters, string key, dynamic value)
        {
            if (parameters == null)
            {
                parameters = new BusinessParameters();
            }

            string[] keyParts = key.Split('.');
            BusinessParameters originalParams = parameters;
            int max = keyParts.Count() - 1;
            for (int i = 0; i <= max; ++i)
            {
                if (i == max)
                {
                    parameters[keyParts[i]] = value;
                }
                else
                {
                    if (!parameters.ContainsKey(keyParts[i]) || !(parameters[keyParts[i]] is BusinessParameters))
                    {
                        parameters[keyParts[i]] = new BusinessParameters();

                    }

                    parameters = parameters[keyParts[i]];
                }
            }

            return originalParams;
        }
    }

}