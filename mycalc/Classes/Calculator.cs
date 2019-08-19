using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace mycalc.Classes
{
    class Calculator
    {
        #region Private data
        private String m_data;
        private String m_format = "#,##0.####################";
        private String m_history;
        private Double m_memory;
        private Operations m_last_operation;
        private Double m_left_value;
        private Double m_right_value;
        private Double m_running_total;
        #endregion

        #region operation and macros
        /// <summary>
        /// arithmatic operation dictionary of macros
        /// </summary>
        private Dictionary<Operations, Func<double, double, double>> m_operations = new Dictionary<Operations, Func<double, double, double>>
        {
            { Operations.Add, (x, y) => x + y },
            { Operations.Divide, (x, y) => x / y },
            { Operations.Multiply, (x, y) => x * y },
            { Operations.None, (x,y) => x },
            { Operations.Power, (x, y) => Math.Pow(x, y) },
            { Operations.Percent, (x, y) => x / 100 * y },
            { Operations.Subtract, (x, y) => x - y }
        };

        /// <summary>
        /// enumeration of available operations
        /// </summary>
        private enum Operations
        {
            None,
            Add,
            Backspace,
            Clear,
            ClearAll,
            Data,
            Divide,
            Equals,
            Multiply,
            Percent,
            Power,
            ReverseSign,
            Subtract,
            MemAdd,
            MemRecall,
            MemClear
        }

        /// <summary>
        /// convert a Operations enum to a string representation
        /// </summary>
        /// <param name="value">the operation to convert</param>
        /// <returns>string - converted value</returns>
        private String OperationsToString(Operations value)
        {
            switch (value)
            {
                case Operations.Add:
                    return "+";
                case Operations.Backspace:
                    return "\\";
                case Operations.Clear:
                    return "C";
                case Operations.ClearAll:
                    return "AC";
                case Operations.Divide:
                    return "/";
                case Operations.Equals:
                    return "=";
                case Operations.Multiply:
                    return "x";
                case Operations.Percent:
                    return "%";
                case Operations.Power:
                    return "^";
                case Operations.ReverseSign:
                    return "±";
                case Operations.Subtract:
                    return "-";
                case Operations.MemAdd:
                    return "M";
                case Operations.MemClear:
                    return "MC";
                case Operations.MemRecall:
                    return "MR";
                default:
                    return "";
            }
        }

        /// <summary>
        /// convert a string to an Operations enum
        /// </summary>
        /// <param name="value">string value to convert</param>
        /// <returns>Operations - converted value</returns>
        private Operations StringToOperations(String value)
        {
            if (value.Equals("="))
                return Operations.Equals;
            else if (value.Equals("+"))
                return Operations.Add;
            else if (value.Equals("-"))
                return Operations.Subtract;
            else if (value.ToUpper().Equals("X"))
                return Operations.Multiply;
            else if (value.Equals("*"))
                return Operations.Multiply;
            else if (value.Equals("/"))
                return Operations.Divide;
            else if (value.Equals("%"))
                return Operations.Percent;
            else if (value.Contains("^"))
                return Operations.Power;
            // either contains ± OR contains + AND -
            else if (value.Contains("±") || (value.Contains("+") & value.Contains("-")))
                return Operations.ReverseSign;
            else if (value.Equals("\\") || value.Equals("\b"))
                return Operations.Backspace;
            else if (value.ToUpper().Equals("C"))
                return Operations.Clear;
            else if (value.ToUpper().Equals("AC"))
                return Operations.ClearAll;
            else if (value.ToUpper().Equals("M"))
                return Operations.MemAdd;
            else if (value.ToUpper().Equals("MC"))
                return Operations.MemClear;
            else if (value.ToUpper().Equals("MR"))
                return Operations.MemRecall;
            return Operations.None;
        }
        #endregion

        #region private methods
        /// <summary>
        /// remove the last digit
        /// </summary>
        private void Backspace()
        {
            // there's nothing to do if the last thing we did was =
            if (m_last_operation == Operations.Equals)
                return;
            RemoveLastChar();
            if (m_last_operation == Operations.None)
                Double.TryParse(m_data, out m_left_value);
            else
                Double.TryParse(m_data, out m_right_value);
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// remove the last entry
        /// </summary>
        private void ClearLast()
        {
            if (m_last_operation == Operations.None)
                m_left_value = 0.0;
            else
                m_right_value = 0.0;
            if (!String.IsNullOrEmpty(m_data))
            {
                m_history = RemoveFromString(m_history, m_data);
                m_data = "";
            }
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// clear everything and start over
        /// </summary>
        private void ClearAll()
        {
            Reset();
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// data to be processed; typically a digit or a period
        /// </summary>
        /// <param name="value">data value to be processed</param>
        private void Data(string value)
        {
            // if the last thing that was done was pressing equal and then
            // the user started typing a number, they are starting
            // a new operation hence the ClearAll()
            if (m_last_operation == Operations.Equals)
                ClearAll();
            // allow only one decimal point
            if (value.Equals("."))
            {
                value = m_data.Contains(".") ? "" : ".";
                if (String.IsNullOrEmpty(m_data))
                    value = "0.";
            }
            m_data += value;
            m_history += value;

            if (double.TryParse(m_data, out double @data))
            {
                if (m_last_operation == Operations.None)
                    m_left_value = data;
                else
                    m_right_value = data;
            }
            else
                // remove the last character as it was not a number
                RemoveLastChar();
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// perform a calculation using the calculation macro dictionary
        /// </summary>
        /// <param name="operation">the calculation opertaion to perform</param>
        /// <returns>double - the result of the calculation</returns>
        private double DoCalculate(Operations operation)
        {
            //            if (operation == Operations.Equals)
            if (m_last_operation != Operations.Equals)
                m_running_total = m_operations[m_last_operation](m_left_value, m_right_value);
            return m_running_total;
        }

        /// <summary>
        /// publish various properties to interested parties
        /// </summary>
        /// <param name="memoryEvent">boolean flag to indicate if property changes are limited to just memory functions</param>
        private void FireEvent(bool memoryEvent)
        {
            if (OnCalculatorAction != null)
            {
                var e = new CalculatorEventArgs();
                if (!memoryEvent && m_last_operation == Operations.Equals)
                    m_history += String.Format(" {0}", m_running_total);
                object @value = GetValue();
                e.DisplayValue = value.GetType().Equals(typeof(double))? ((double)value).ToString(m_format):(string)@value;
                e.History = m_history;
                e.Memory = m_memory == 0.0 ? "" : m_memory.ToString(m_format);
                if (e.DisplayValue.Length < 1)
                    e.DisplayValue = "0";
                OnCalculatorAction(this, e);
            }
        }

        /// <summary>
        /// Get the current value
        /// </summary>
        /// <returns>dynamic - the type of object returned will depend on what the previous operation was 
        /// and the type of value associated with that operation. It is the responsibility of the caller to enforce type safety.</returns>
        private dynamic GetValue()
        {
            switch (m_last_operation)
            {
                case Operations.None:
                    if (m_data.StartsWith("0."))
                        return m_data;
                    else
                        return m_left_value;
                case Operations.Equals:
                    return m_running_total;
                default:
                    if (m_data.StartsWith("0."))
                        return m_data;
                    else
                        return m_right_value;
            }
        }

        /// <summary>
        /// Method factory based on the required operation
        /// </summary>
        /// <param name="operation">the requested operation</param>
        private void Operation(Operations operation)
        {
            m_data = "";
            m_history += String.Format(" {0} ", OperationsToString(operation));
            double _rt = DoCalculate(operation);
            m_last_operation = operation;

            if ((m_last_operation != Operations.Equals) && (m_right_value > 0))
            {
                m_left_value = _rt;
                m_right_value = 0.0;
            }
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// Remove an expression string from the end of a source string (if exists)
        /// </summary>
        /// <param name="source">the source string</param>
        /// <param name="expression">the expression to remove from the source string</param>
        /// <returns>string - source string with the expression removed</returns>
        private String RemoveFromString(String source, String expression)
        {
            // remove data from history
            if (source.Length == 0 || source.Length - expression.Length > source.Length)
                return "";
            return source.Remove(source.Length - expression.Length);
        }

        /// <summary>
        /// remove the last character from the working storage
        /// </summary>
        private void RemoveLastChar()
        {
            if (m_data.Length > 0)
            {
                m_data = m_data.Remove(m_data.Length - 1);
                m_history = m_history.Remove(m_history.Length - 1);
            }
        }

        /// <summary>
        /// Reset and re-initailse the Calculator object
        /// </summary>
        private void Reset()
        {
            m_data = "";
            m_history = "";
            m_left_value = 0.0;
            m_right_value = 0.0;
            m_running_total = 0.0;
            m_last_operation = Operations.None;
            FireEvent(memoryEvent: false);
        }

        /// <summary>
        /// reverse the sign of working data
        /// </summary>
        private void ReverseSign()
        {
            if (m_data.Length > 0)
            {
                // remove data from history: which should always be at the end
                m_history = RemoveFromString(m_history, m_data);
                if (m_data[0] == '-')
                    m_data = m_data.Remove(0, 1);
                else
                    m_data = m_data.Insert(0, "-");
                // update values
                if (double.TryParse(m_data, out double @data))
                {
                    if (m_last_operation == Operations.None)
                        m_left_value = data;
                    else
                        m_right_value = data;
                    m_history += m_data;
                }
                // tell the subscribers
                FireEvent(memoryEvent: false);
            }
        }
        

        /// <summary>
        /// Set the working data value
        /// </summary>
        /// <param name="value">the value to be set</param>
        private void SetValue(string value)
        {
            if (double.TryParse(value, out double @d))
                Data(value);
        }
        #endregion

        /// <summary>
        /// Delegate template
        /// </summary>
        /// <param name="sender">the Calculator object</param>
        /// <param name="e"><seealso cref="CalculatorEventArgs"/>broadcasted properties</param>
        public delegate void CalculatorAction(object sender, CalculatorEventArgs e);

        /// <summary>
        /// public subscriber event
        /// </summary>
        public event CalculatorAction OnCalculatorAction;

        /// <summary>
        /// write-only property
        /// </summary>
        public String Value { set => SetValue(value); }

        /// <summary>
        /// standard constructor calls <seealso cref="Reset"/>
        /// </summary>
        public Calculator() => Reset();

        /// <summary>
        /// This public method is called to pass a value into the Calculator object. Based on the contents of
        /// this value, the object determines the next step(s) and whether or not the value was an operation or a data entry value.
        /// </summary>
        /// <param name="value">the value to be passed to the calculator object</param>
        public void DataEntry(string value)
        {
            Operations @operation = StringToOperations(value);
            switch (@operation)
            {
                case Operations.Add:
                case Operations.Divide:
                case Operations.Equals:
                case Operations.Multiply:
                case Operations.Percent:
                case Operations.Power:
                case Operations.Subtract:
                    Operation(@operation);
                    break;
                case Operations.Backspace:
                    Backspace();
                    break;
                case Operations.Clear:
                    ClearLast();
                    break;
                case Operations.ClearAll:
                    ClearAll();
                    break;
                case Operations.ReverseSign:
                    ReverseSign();
                    break;
                case Operations.MemAdd:
                    m_memory = GetValue();
                    FireEvent(memoryEvent: true);
                    break;
                case Operations.MemClear:
                    m_memory = 0.0;
                    FireEvent(memoryEvent: true);
                    break;
                case Operations.MemRecall:
                    SetValue(m_memory.ToString());
                    break;
                default:
                    Data(value);
                    break;
            }
        }
    }
}
