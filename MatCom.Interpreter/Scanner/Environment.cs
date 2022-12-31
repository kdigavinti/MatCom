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
        public Dictionary<string, string> valueByRef = new Dictionary<string, string>();  

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
                if(valueByRef.ContainsKey(identifier))
                    return getValueByRef(identifier);
            }
            return null;
        }

        public void assignValueByRef(string left, string right)
        {
            if(valueByRef.ContainsKey(left))
            {
                valueByRef[left] = right;
            }
            else
            {
                valueByRef.Add(left, right);
            }
        }

        public Object? getValueByRef(string identifier)
        {
            if (valueByRef.ContainsKey(identifier))
            {
                string _refValue = valueByRef[identifier].ToString();
                string _expression = string.Empty;
                foreach(var item in values)
                {
                    if(_refValue.Contains(item.Key))
                    {
                        _refValue = _refValue.Replace(item.Key, item.Value.ToString());
                    }
                }

                Parser parser = new Parser();
                return (object)parser.Parse(_refValue);
            }
            return null;
        }
    }
}
