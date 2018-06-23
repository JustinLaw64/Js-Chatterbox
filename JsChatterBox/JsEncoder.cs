using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GenericCollections = System.Collections.Generic;

using JsEncoder_Type_Int = System.Int32;
using JsEncoder_Type_Float = System.Double;
using JsEncoder_Type_Char = System.Char;

namespace JsEncoder
{
    // Begin defining what's in the classes.
    public abstract class ValueBase
    {
        protected ValueBase(ValueType NewValueType) { _ValueType = NewValueType; }

        protected ValueType _ValueType;

        public ValueType GetValueType() { return _ValueType; }

        public abstract string EncodeIntoValue();
        public abstract bool Compare(ValueBase OtherValue);

        public override sealed bool Equals(Object obj)
        {
            ValueBase value = obj as ValueBase;
            if (value != null)
                return Compare(value);
            else
                return false;
        }
        public override abstract int GetHashCode(); // Microsoft Specific

        public static bool operator ==(ValueBase v1, ValueBase v2)
        {
            Object v1o = v1; // Workaround to avoid loops.
            Object v2o = v2;
            return (v1o != null ? v1.Compare(v2) : v1o == v2o);
        }
        public static bool operator !=(ValueBase v1, ValueBase v2)
        {
            Object v1o = v1; // Workaround to avoid loops.
            Object v2o = v2;
            return (v1o != null ? !v1.Compare(v2) : v1o != v2o);
        }
    }
    public sealed class BooleanValue : ValueBase
    {
        private bool _Value;
        public bool GetValue() { return _Value; }
        public override string EncodeIntoValue() { return EncoderStream.EncodeBoolean(_Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
            {
                BooleanValue ConvertedValue = (BooleanValue)OtherValue;
                return (_Value == ConvertedValue._Value);
            }
            else
                return false;
        }

        public override int GetHashCode() { return _Value.GetHashCode(); } // Microsoft Specific

        public BooleanValue(bool NewValue) : base(ValueType.Boolean) { _Value = NewValue; }
    }
    public sealed class IntValue : ValueBase
    {
        private JsEncoder_Type_Int _Value;
        public JsEncoder_Type_Int GetValue() { return _Value; }
        public override string EncodeIntoValue() { return EncoderStream.EncodeInt(_Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
            {
                IntValue ConvertedValue = (IntValue)OtherValue;
                return (_Value == ConvertedValue._Value);
            }
            else
                return false;
        }

        public override int GetHashCode() { return _Value.GetHashCode(); } // Microsoft Specific

        public IntValue(JsEncoder_Type_Int NewValue) : base(ValueType.Int) { _Value = NewValue; }
    }
    public sealed class FloatValue : ValueBase
    {
        private JsEncoder_Type_Float _Value;
        public JsEncoder_Type_Float GetValue() { return _Value; }
        public override string EncodeIntoValue() { return EncoderStream.EncodeFloat(_Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
            {
                FloatValue ConvertedValue = (FloatValue)OtherValue;
                return (_Value == ConvertedValue._Value);
            }
            else
                return false;
        }

        public override int GetHashCode() { return _Value.GetHashCode(); } // Microsoft Specific

        public FloatValue(JsEncoder_Type_Float NewValue) : base(ValueType.Float) { _Value = NewValue; }
    }
    public sealed class StringValue : ValueBase
    {
        private string _Value;
        public string GetValue() { return _Value; }
        public override string EncodeIntoValue() { return EncoderStream.EncodeString(_Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
            {
                StringValue ConvertedValue = (StringValue)OtherValue;
                return (_Value == ConvertedValue._Value);
            }
            else
                return false;
        }

        public override int GetHashCode() { return _Value.GetHashCode(); } // Microsoft Specific

        public StringValue(string NewValue) : base(ValueType.String) { _Value = NewValue; }
    }
    public sealed class TableValue : ValueBase
    {
        public Dictionary<ValueBase, ValueBase> Dictionary;

        public override string EncodeIntoValue() { return EncoderStream.EncodeTable(this); }
        public TableDigested DigestTable()
        {
            TableDigested Result = new TableDigested();

            int HighestIndex = 1;
            foreach (KeyValuePair<ValueBase, ValueBase> item in Dictionary)
            {
                if (item.Key.GetValueType() == ValueType.Int)
                {
                    int index = (int)((IntValue)item.Key).GetValue();
                    if (index > HighestIndex) HighestIndex = index;

                    Result.ManualIntDictionary.Add(index, item.Value);
                }
                else
                    Result.MiscKeyDictionary.Add(item.Key, item.Value);
            }

            List<KeyValuePair<int, ValueBase>> SortedList = new List<KeyValuePair<int, ValueBase>>(Result.ManualIntDictionary);
            SortedList.Sort(new Comparison<KeyValuePair<int, ValueBase>>(ComparisonFunctionForDigestTable));

            // Move all of the values, stacked uninterrupted on the lowest index, to the auto int array.
            for (int i = 1; i <= HighestIndex; i++)
            {
                ValueBase CurrentValue = null;
                if (Result.ManualIntDictionary.TryGetValue(i, out CurrentValue))
                {
                    Result.ManualIntDictionary.Remove(i);
                    Result.AutoIntArray.Add(CurrentValue);
                }
                else
                    break;
            }

            return Result;
        }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
            {
                TableValue ConvertedValue = (TableValue)OtherValue;
                return (ConvertedValue.Compare(this));
            }
            else
                return false;
        }
        public bool Compare(TableValue OtherTable)
        {
            TableValue that = OtherTable; // It's weird to use this as a parameter name.

            // Figure if they match in size. If it's not, immediate false.
            if (this.Dictionary.Count != that.Dictionary.Count)
                return false;

            // Store refs to save performance.
            GenericCollections.Dictionary<ValueBase, ValueBase> ThisDictionary = this.Dictionary;
            GenericCollections.Dictionary<ValueBase, ValueBase> ThatDictionary = that.Dictionary;

            // Compare the Tables!
            foreach (KeyValuePair<ValueBase, ValueBase> item in ThisDictionary)
            {
                ValueBase v = null;
                if (!(ThatDictionary.TryGetValue(item.Key, out v) && v == item.Value)) // Out v?
                    return false;
            }
            return true;
        }

        public ValueBase Get(JsEncoder_Type_Int ID) { return Get(new IntValue(ID)); }
        public ValueBase Get(ValueBase ID)
        {
            ValueBase r = null;
            Dictionary.TryGetValue(ID, out r);
            return r;
        }
        public void Set(JsEncoder_Type_Int ID, ValueBase Value) { Set(new IntValue(ID), Value); }
        public void Set(ValueBase ID, ValueBase Value)
        {
            Dictionary.Remove(ID);
            Dictionary.Add(ID, Value);
        }

        public override int GetHashCode() { return Dictionary.GetHashCode(); } // Microsoft Specific

        public TableValue(GenericCollections.IDictionary<ValueBase, ValueBase> Dictionary) : this(new GenericCollections::Dictionary<ValueBase, ValueBase>(Dictionary)) { }
        public TableValue() : this(new GenericCollections.Dictionary<ValueBase, ValueBase>()) { }

        private TableValue(GenericCollections.Dictionary<ValueBase, ValueBase> Dictionary) : base(ValueType.Table) { this.Dictionary = Dictionary; }

        private int ComparisonFunctionForDigestTable(KeyValuePair<int, ValueBase> a, KeyValuePair<int, ValueBase> b) { return a.Key - b.Key; }

        public static TableValue ArrayToTable(IEnumerable<ValueBase> array)
        {
            TableValue Result = new TableValue();
            JsEncoder_Type_Int i = 0;
            GenericCollections.Dictionary<ValueBase, ValueBase> ResultD = Result.Dictionary;
            foreach (ValueBase item in array)
            {
                i++;
                ResultD.Add(new IntValue(i), item);
            }
            return Result;
        }
    }

    public sealed class TableDigested
    {
        public GenericCollections::Dictionary<ValueBase, ValueBase> MiscKeyDictionary = new GenericCollections.Dictionary<ValueBase, ValueBase>();
        public GenericCollections::Dictionary<int, ValueBase> ManualIntDictionary = new GenericCollections.Dictionary<int, ValueBase>();
        public List<ValueBase> AutoIntArray = new List<ValueBase>();

        public TableValue ToTable()
        {
            TableValue Result = new TableValue();

            foreach (KeyValuePair<int, ValueBase> item in ManualIntDictionary)
                Result.Dictionary.Add(new IntValue(item.Key), item.Value);

            // We only added manual integer entries yet, so we only need look at that table
            int CurrentInt = 0;
            foreach (ValueBase item in AutoIntArray)
            {
                CurrentInt++;

                ValueBase TryOutput = null;
                while (ManualIntDictionary.TryGetValue(CurrentInt, out TryOutput)) // Out TryOutput?
                    CurrentInt++;

                Result.Dictionary.Add(new IntValue(CurrentInt), item);
            }

            // We don't care about what these keys are, so we add them.
            foreach (KeyValuePair<ValueBase, ValueBase> item in MiscKeyDictionary)
                Result.Dictionary.Add(item.Key, item.Value);

            return Result;
        }

        public TableDigested() { }
    }

    public class EncoderStream
    {
        private StringBuilder _output = new StringBuilder();
        private bool _HasOutput = false;

        public string PopOutput()
        {
            string Result = _output.ToString();
            _output.Clear();
            _HasOutput = false;
            return Result;
        }
        public string ReadOutput() { return _output.ToString(); }
        public void InputValue(ValueBase Value)
        {
            _output.Append(EncodeValue(Value));
            _output.Append("||");
            _HasOutput = true;
        }
        public bool HasOutput() { return _HasOutput; }

        public EncoderStream() { }

        // This is the only function that depends on another class for help.
        public static string EncodeValue(ValueBase Value) // This is the only function that depends on another class for help.
        {
            if (Value != null)
                return Value.EncodeIntoValue();
            else
                return EncodeNull();
        }

        public static string EncodeNull() { return "N"; } // Fallback for when a value to encode is null.
        public static string EncodeBoolean(bool Value) { return (Value ? "T" : "F"); }
        public static string EncodeInt(JsEncoder_Type_Int Value) { return Value.ToString(); }
        public static string EncodeFloat(JsEncoder_Type_Float Value) { return Value.ToString(); }
        public static string EncodeString(string Value)
        {
            string r = Value;
            if (r != null)
            {
                r = r.Replace("\\", "\\\\");
                r = r.Replace("|", "\\|");
                r = r.Replace("\"", "\\\"");
                r = string.Concat("\"", r, "\"");
            }
            else
                r = EncodeNull();
            return r;
        }
        public static string EncodeTable(TableValue Value)
        {
            // Figure out how to turn the table into text
            TableDigested DigestedTable = Value.DigestTable();

            // Begin result bracket thing.
            StringBuilder Result = new StringBuilder("[");

            // Turn everything into text.
            foreach (ValueBase item in DigestedTable.AutoIntArray)
            {
                Result.Append(EncoderStream.EncodeValue(item));
                Result.Append(";");
            }
            foreach (KeyValuePair<int, ValueBase> item in DigestedTable.ManualIntDictionary)
            {
                ValueBase v = item.Value;

                Result.Append(item.Key.ToString());
                Result.Append(":");
                Result.Append(EncoderStream.EncodeValue(v));
                Result.Append(";");
            }
            foreach (KeyValuePair<ValueBase, ValueBase> item in DigestedTable.MiscKeyDictionary)
            {
                Result.Append(item.Key.EncodeIntoValue());
                Result.Append(":");
                ValueBase v = item.Value;
                Result.Append(EncoderStream.EncodeValue(v));
                Result.Append(";");
            }

            // Close the plain-text table and return it.
            Result.Append("]");
            return Result.ToString();
        }
    }
    public class DecoderStream
    {
        private List<ValueBase> _output = new List<ValueBase>();
        private string _input = "";
        private bool _HasOutput = false;

        public bool HasOutput() { return _HasOutput; }

        public ValueBase[] PopOutput()
        {
            ValueBase[] Result = _output.ToArray();
            _output.Clear();
            _HasOutput = false;
            return Result;
        }
        public ValueBase[] ReadOutput() { return _output.ToArray(); }
        public void InputValue(string Value) { _input = string.Concat(_input, Value); }
        public string ReadInput() { return _input; }

        // ################################################################
        // ## BEGIN PARSER
        // ################################################################

        private class P_TableLevel
        {
            public TableDigested Table;
            public bool PairHasCustomKey = false;
            public ValueBase V1 = null;
            public ValueBase V2 = null;

            public P_TableLevel() { }
        }

        private int P_Position = 0;

        private int P_EndCharCount = 0;
        private int P_FailSafeEndCharCount = 0;
        private int P_ErrorCode = 0;

        private List<P_TableLevel> P_TableStack = new List<P_TableLevel>();

        private StringBuilder P_ValueParser_ValueRaw = new StringBuilder();

        private int P_ValueParser_ValueType = -1; // -2 = Not ready for value. ,-1 = Ready for value!, 0 = nullptr
        private bool P_ValueParser_StringMode = false;
        private ValueBase P_ValueParser_ValueAtReady = null;
        private bool P_ValueParser_ValueIsReady = false;
        private bool P_EscapedInString = false;

        private void P_EndBlock()
        {
            if ((P_TableStack.Count > 0) | false)
                P_ThrowError(1);

            // Output
            if (P_ErrorCode == 0)
            {
                _output.Add(P_ValueParser_TakeValue());
            }
            else
            {
                TableValue TableResult = new TableValue();
                TableResult.Dictionary.Add(new IntValue(1), new StringValue("Error"));
                TableResult.Dictionary.Add(new IntValue(2), new IntValue(P_ErrorCode));
                _output.Add(TableResult);
            }

            // Clear up broken business
            P_TableStack.Clear();
            P_ValueParser_StringMode = false;
            P_EscapedInString = false;
            P_ValueParser_ValueAtReady = null;
            P_ValueParser_ValueRaw.Clear();
            P_ValueParser_ValueType = -1;

            P_ErrorCode = 0;

            // Purge the last block from the input stream to save memory.
            _input = _input.Substring(P_Position);
            P_Position = 0;
        }
        private void P_ThrowError(int ErrorCode) { P_ErrorCode = ErrorCode; }
        private void P_ValueParser_OutputValue(ValueBase Value)
        {
            P_ValueParser_ValueRaw.Clear();
            P_ValueParser_ValueAtReady = Value;
            P_ValueParser_ValueType = -2;
            P_ValueParser_ValueIsReady = true;
        }
        private ValueBase P_ValueParser_TakeValue()
        {
            if (P_ValueParser_ValueIsReady)
            {
                ValueBase r = P_ValueParser_ValueAtReady;
                P_ValueParser_ValueAtReady = null;
                return r;
            }
            else
                throw new Exception("Can't take a finished value if it's not there. Use P_ValueParser_ValueIsReady to check first.");
        }

        private bool P_Step()
        {
            bool result = (P_Position < _input.Length);
            if (result)
            {
                JsEncoder_Type_Char c = _input[P_Position];
                int TableStackUBound = P_TableStack.Count - 1;
                P_TableLevel CurrentTable = null;
                bool EndingBlock = false;

                if (P_TableStack.Count > 0)
                    CurrentTable = P_TableStack[TableStackUBound];

                // To know when to end a stream block

                // ## BEGIN BlockEnder
                if (c == '|' && !P_EscapedInString)
                {
                    P_FailSafeEndCharCount++;
                    if (!P_EscapedInString)
                        P_EndCharCount++;
                    if ((P_EndCharCount == 2) | (P_FailSafeEndCharCount == 2))
                        EndingBlock = true;
                }
                else
                {
                    if (P_EndCharCount == 1)
                        P_ThrowError(1);
                    P_FailSafeEndCharCount = 0;
                    P_EndCharCount = 0;
                }
                // ## END BlockEnder

                if (P_ErrorCode == 0) // Don't permit other processes if there is an error.
                {
                    bool CIsUnhandled = true;
                    bool String_IsFirstBracket = false;

                    // ## BEGIN ValueTypeDeterminer
                    if (!P_ValueParser_StringMode)
                    {
                        bool handled2 = true;
                        if (P_ValueParser_ValueType == -1)
                        {
                            // What type is this value?
                            switch (c)
                            {
                                case 'T':
                                case 'F':
                                    P_ValueParser_ValueType = 1; // Boolean
                                    break;
                                case 'N':
                                    P_ValueParser_ValueType = 0; // nullptr
                                    break;
                                case '\"':
                                    P_ValueParser_ValueType = 4; // String
                                    String_IsFirstBracket = true;
                                    P_ValueParser_StringMode = true;
                                    break;
                                case '[':
                                    P_TableLevel NewTableLevel = new P_TableLevel();
                                    NewTableLevel.Table = new TableDigested();
                                    P_TableStack.Add(NewTableLevel);
                                    break;
                                default:
                                    if (P_IsCharNumber(c))
                                        P_ValueParser_ValueType = 2; // Int
                                    else
                                        handled2 = false;

                                    break;
                            }
                        }
                        if (handled2)
                            CIsUnhandled = false;
                    }
                    // ## END ValueTypeDeterminer

                    // ## BEGIN ValueExtractor
                    // Parse the value.
                    if (P_ValueParser_ValueType >= 0)
                    {
                        bool handled = true;
                        switch (P_ValueParser_ValueType)
                        {
                            case 0: // nullptr or
                            case 1: // Boolean
                                if (P_ValueParser_ValueRaw.Length == 0)
                                    P_ValueParser_ValueRaw.Append(c);
                                else
                                {
                                    ValueBase o = null;
                                    switch (P_ValueParser_ValueRaw[0])
                                    {
                                        case 'N':
                                            o = null;
                                            break;
                                        default:
                                            o = new BooleanValue(P_ValueParser_ValueRaw[0] == 'T');
                                            break;
                                    }
                                    P_ValueParser_OutputValue(o);
                                }
                                break;
                            case 2: // Int
                            case 3: // Float
                                if (P_IsCharNumber(c))
                                    P_ValueParser_ValueRaw.Append(c);
                                else if (c == '.')
                                {
                                    if (P_ValueParser_ValueType == 2)
                                    {
                                        P_ValueParser_ValueType = 3;
                                        P_ValueParser_ValueRaw.Append(c);
                                    }
                                    else
                                        P_ThrowError(1);
                                }
                                else
                                {
                                    switch (P_ValueParser_ValueType)
                                    {
                                        case 2:
                                            P_ValueParser_OutputValue(new IntValue(System.Int32.Parse(P_ValueParser_ValueRaw.ToString())));
                                            break;
                                        case 3:
                                            P_ValueParser_OutputValue(new FloatValue(System.Double.Parse(P_ValueParser_ValueRaw.ToString())));
                                            break;
                                    }
                                }
                                break;
                            case 4: // String
                                if (P_ValueParser_StringMode)
                                {
                                    bool String_RecordChar = true; // False to avoid recording current character into a string.

                                    if (!P_EscapedInString)
                                    {
                                        if (c == '\\')
                                        {
                                            P_EscapedInString = true;
                                            String_RecordChar = false;
                                        }
                                        if (c == '\"')
                                        {
                                            if (!String_IsFirstBracket)
                                            {
                                                P_ValueParser_StringMode = false;
                                                String_RecordChar = false;
                                                P_ValueParser_OutputValue(new StringValue(P_ValueParser_ValueRaw.ToString()));
                                            }
                                            else
                                                String_RecordChar = false;
                                        }
                                    }
                                    else
                                        P_EscapedInString = false;

                                    if (String_RecordChar)
                                        P_ValueParser_ValueRaw.Append(c);
                                }
                                break;
                            default:
                                handled = false;
                                break;
                        }

                        if (handled)
                            CIsUnhandled = false;
                    }
                    // ## END ValueExtractor

                    // ## BEGIN TableChars
                    // Table-Specific
                    if (!P_ValueParser_StringMode)
                    {
                        if (CurrentTable != null) // Only process these when not in a string.
                        {
                            bool handled1 = true;

                            int EndType = 0;
                            switch (c)
                            {
                                case ':':
                                    if (!CurrentTable.PairHasCustomKey)
                                    {
                                        CurrentTable.PairHasCustomKey = true;
                                        CurrentTable.V1 = P_ValueParser_TakeValue();
                                        P_ValueParser_ValueType = -1;
                                    }
                                    else
                                        P_ThrowError(1);
                                    break;
                                case ']':
                                    EndType = 2;
                                    break;
                                case ';':
                                    EndType = 1;
                                    break;
                                default:
                                    handled1 = false;
                                    break;
                            }
                            if (EndType > 0)
                            {
                                if (P_ValueParser_ValueType != -1)
                                {
                                    CurrentTable.V2 = P_ValueParser_TakeValue();

                                    ValueBase V1 = CurrentTable.V1;
                                    ValueBase V2 = CurrentTable.V2;

                                    if (V2 != null)
                                    {
                                        if (V1 != null)
                                        {
                                            if (CurrentTable.V1.GetValueType() == ValueType.Int)
                                                CurrentTable.Table.ManualIntDictionary.Add(((IntValue)V1).GetValue(), V2);
										else
											CurrentTable.Table.MiscKeyDictionary.Add(V1, V2);
                                        }
                                        else
                                            CurrentTable.Table.AutoIntArray.Add(V2);
                                    }
                                }

                                // Clear these so we can use them later.
                                CurrentTable.V1 = null;
                                CurrentTable.V2 = null;
                                CurrentTable.PairHasCustomKey = false;

                                if (EndType == 2)
                                {
                                    // Close table and go back up.
                                    P_TableStack.RemoveAt(TableStackUBound);
                                    P_ValueParser_OutputValue(CurrentTable.Table.ToTable());
                                    P_ValueParser_ValueType = -2;
                                }
                                else //if (EndType == 1)
                                    P_ValueParser_ValueType = -1;
                            }

                            if (handled1)
                                CIsUnhandled = false;
                        }
                    }
                    // ## END TableChars

                    // If nothing handled this character, it must be invalid.
                    if (CIsUnhandled)
                    {
                        if ((c == ' ') | (c == '|')) // Ignore spaces in this case.
                            CIsUnhandled = false;
                        else
                            P_ThrowError(1);
                    }
                }

                P_Position++;

                if (EndingBlock)
                    P_EndBlock();
            }
            return result;
        }

        private static bool P_IsCharNumber(JsEncoder_Type_Char c)
        {
            char[] Chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            foreach (char item in Chars)
                if (c == item)
                    return true;
            return false;
        }

        public int RunParser()
        {
            // Keep running the stepper until it says it can't go further,
            // and while doing so, count how many times it stepped.
            int result = 0;
            while (P_Step()) result++;
            return result;
        }

        // ################################################################
        // ## END PARSER
        // ################################################################

        public DecoderStream() { }

        public static ValueBase DecodeValue(string Value)
        {
            DecoderStream d = new DecoderStream();
            d.InputValue(Value.Replace("||", "  ")); // Input the value, except remove the double lines to avoid problems.
            d.InputValue("||");
            d.RunParser();
            return d.PopOutput()[0];
        }
    }

    public enum ValueType { Boolean = 1, Int = 2, Float = 3, String = 4, Table = 5 }
}
