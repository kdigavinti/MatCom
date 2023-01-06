using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatCom.Interpreter.Scanner
{
    public class Environment
    {
        public Dictionary<string, Object> Values = new Dictionary<string, object>();
        public Dictionary<string, string> ValuesByRef = new Dictionary<string, string>();  

        public void assign(string identifier, object value)
        {            
            if (Values.ContainsKey(identifier))
            {
                Values[identifier] = value;
            }
            else
            {
                Values.Add(identifier, value);  
            }
        }

        public Object? getValue(string identifier)
        {
            if (Values.ContainsKey(identifier))
            {
                if(ValuesByRef.ContainsKey(identifier))
                    return getValueByRef(identifier);
            }
            return null;
        }

        public void assignValueByRef(string left, string right)
        {
            if(ValuesByRef.ContainsKey(left))
            {
                ValuesByRef[left] = right;
            }
            else
            {
                ValuesByRef.Add(left, right);
            }
        }

        public Object? getValueByRef(string identifier)
        {
            if (ValuesByRef.ContainsKey(identifier))
            {
                string _refValue = ValuesByRef[identifier].ToString();
                string _expression = string.Empty;
                foreach(var item in Values)
                {
                    if(_refValue.Contains(item.Key))
                    {
                        _refValue = _refValue.Replace(item.Key, item.Value.ToString());
                    }
                }
                //return _refValue;
                if(decimal.TryParse(_refValue, out decimal result))
                {
                    return result;
                }
                else
                {
                    Parser parser = new Parser();
                    return (object)parser.Parse(_refValue);
                }

            }
            return null;
        }
    }
}
