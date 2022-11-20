using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    public class Environment
    {
        public Dictionary<string, Object> values = new Dictionary<string, object>();
        
        public void assign(string identifier, object value)
        {            
            if (values.ContainsKey(identifier))
            {
                values[identifier] = value;
            }
            else
            {
                values.Add(identifier, value);  
            }
        }

        public Object? getValue(string identifier)
        {
            if (values.ContainsKey(identifier))
            {
                return values[identifier];
            }
            return null;
        }
    }
}
