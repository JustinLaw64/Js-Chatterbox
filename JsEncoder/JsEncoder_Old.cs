// Js Encoder by Justin Law

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsEncoder_Old
{
    public enum ValueType { Boolean = 1, Int = 2, Float = 3, String = 4, Table = 5 }

    public abstract class ValueBase
    {
        public ValueType GetValueType() { return _ValueType; }
        public abstract String EncodeIntoValue();
        public abstract bool Compare(ValueBase OtherValue);

        public override bool Equals(object obj) // Microsoft Specific
        {
            ValueBase value = (obj as ValueBase);
            if (value != null)
                return Compare(value);
            else
                return false;
        }
        public abstract override int GetHashCode(); // Microsoft Specific

        protected ValueType _ValueType;
        protected ValueBase(ValueType NewValueType) : base() { _ValueType = NewValueType; }

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
        public readonly bool Value;

        public override String EncodeIntoValue() { return EncoderStream.EncodeBoolean(Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
                return (Value == ((BooleanValue)OtherValue).Value);
            else
                return false;
        }
        public BooleanValue(bool NewValue) : base(ValueType.Boolean) { Value = NewValue; }

        public override int GetHashCode() { return Value.GetHashCode(); } // Microsoft Specific
    }
    public sealed class IntValue : ValueBase
    {
        public readonly int Value;

        public override String EncodeIntoValue() { return Value.ToString(); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
                return (Value == ((IntValue)OtherValue).Value);
            else
                return false;
        }
        public IntValue(int NewValue) : base(ValueType.Int) { Value = NewValue; }

        public override int GetHashCode() { return Value.GetHashCode(); } // Microsoft Specific
    }
    public sealed class FloatValue : ValueBase
    {
        public readonly double Value;

        public override String EncodeIntoValue() { return Value.ToString(); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
                return (Value == ((FloatValue)OtherValue).Value);
            else
                return false;
        }
        public FloatValue(double NewValue) : base(ValueType.Float) { Value = NewValue; }

        public override int GetHashCode() { return Value.GetHashCode(); } // Microsoft Specific
    }
    public sealed class StringValue : ValueBase
    {
        public readonly String Value;

        public override string EncodeIntoValue() { return EncoderStream.EncodeString(Value); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
                return (Value == ((StringValue)OtherValue).Value);
            else
                return false;
        }
        public StringValue(String NewValue) : base(ValueType.String)
        {
            if (NewValue != null)
                Value = NewValue;
            else
                throw new ArgumentNullException("NewValue"); // To keep from null weirdnesses.
        }

        public override int GetHashCode() { return Value.GetHashCode(); } // Microsoft Specific
    }
    public sealed class TableValue : ValueBase
    {
        public readonly Dictionary<ValueBase, ValueBase> Dictionary = new Dictionary<ValueBase, ValueBase>();

        public override string EncodeIntoValue() { return EncoderStream.EncodeTable(this); }
        public override bool Compare(ValueBase OtherValue)
        {
            if (OtherValue != null && OtherValue.GetValueType() == this._ValueType)
                return (((TableValue)OtherValue).Compare(this));
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
            Dictionary<ValueBase, ValueBase> ThisDictionary = this.Dictionary;
            Dictionary<ValueBase, ValueBase> ThatDictionary = that.Dictionary;

            // Compare the Tables!
            foreach (var item in ThisDictionary)
            {
                ValueBase v = null;
                if (!(ThatDictionary.TryGetValue(item.Key, out v) && v == item.Value))
                    return false;
            }
            return true;
        }
        public TableDigested DigestTable()
        {
            TableDigested Result = new TableDigested();

            int HighestIndex = 1;
            foreach (var item in Dictionary)
            {
                if (item.Key.GetValueType() == ValueType.Int)
                {
                    int index = ((IntValue)item.Key).Value;
                    if (index > HighestIndex) HighestIndex = index;

                    Result.ManualIntDictionary.Add(index, item.Value);
                }
                else
                    Result.MiscKeyDictionary.Add(item.Key, item.Value);
            }

            List<KeyValuePair<int, ValueBase>> SortedList = new List<KeyValuePair<int, ValueBase>>(Result.ManualIntDictionary);
            SortedList.Sort( (KeyValuePair<int, ValueBase> a, KeyValuePair<int, ValueBase> b) => { return a.Key - b.Key; } );

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

        public ValueBase Get(int ID) { return Get(new IntValue(ID)); }
        public ValueBase Get(ValueBase ID)
        {
            ValueBase r = null;
            Dictionary.TryGetValue(ID, out r);
            return r;
        }
        public void Set(int ID, ValueBase Value) { Set(new IntValue(ID), Value); }
        public void Set(ValueBase ID, ValueBase Value)
        {
            Dictionary.Remove(ID);
            Dictionary.Add(ID, Value);
        }

        public override int GetHashCode() { return Dictionary.GetHashCode(); } // Microsoft Specific

        public TableValue(IDictionary<ValueBase, ValueBase> Dictionary) : this(new Dictionary<ValueBase, ValueBase>(Dictionary)) { }
        public TableValue() : this(new Dictionary<ValueBase, ValueBase>()) { }

        private TableValue(Dictionary<ValueBase, ValueBase> Dictionary) : base(ValueType.Table) { this.Dictionary = Dictionary; }

        public static TableValue ArrayToTable(IEnumerable<ValueBase> array)
        {
            TableValue Result = new TableValue();
            int i = 0;
            Dictionary<ValueBase, ValueBase> ResultD = Result.Dictionary;
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
        public Dictionary<ValueBase, ValueBase> MiscKeyDictionary = new Dictionary<ValueBase, ValueBase>();
        public Dictionary<int, ValueBase> ManualIntDictionary = new Dictionary<int, ValueBase>();
        public List<ValueBase> AutoIntArray = new List<ValueBase>();

        public TableValue ToTable()
        {
            TableValue Result = new TableValue();

            foreach (var item in ManualIntDictionary)
            {
                Result.Dictionary.Add(new IntValue(item.Key), item.Value);
            }
            // We only added manual integer entries yet, so we only need look at that table
            int CurrentInt = 0;
            foreach (var item in AutoIntArray)
            {
                CurrentInt++;

                ValueBase TryOutput = null;
                while (ManualIntDictionary.TryGetValue(CurrentInt, out TryOutput))
                    CurrentInt++;

                Result.Dictionary.Add(new IntValue(CurrentInt), item);
            }
            // We don't care about what these keys are, so we add them.
            foreach (var item in MiscKeyDictionary)
                Result.Dictionary.Add(item.Key, item.Value);

            return Result;
        }

        public TableDigested() { }
    }

    public class EncoderStream
    {
        private StringBuilder _output = new StringBuilder();
        private bool _HasOutput = false;

        public String PopOutput()
        {
            String Result = _output.ToString();
            _output.Clear();
            _HasOutput = false;
            return Result;
        }
        public String ReadOutput() { return _output.ToString(); }
        public void InputValue(ValueBase Value)
        {
            _output.Append(EncodeValue(Value));
            _output.Append("||");
            _HasOutput = true;
        }
        public bool HasOutput() { return _HasOutput; }

        public EncoderStream() { }

        public static String EncodeValue(ValueBase Value) // This is the only function that depends on another class for help.
        {
            if (Value != null)
                return Value.EncodeIntoValue();
            else
                return EncodeNull();
        }

        public static String EncodeNull() { return "N"; } // Fallback for when a value to encode is null.
        public static String EncodeBoolean(bool Value) { return (Value ? "T" : "F"); }
        public static String EncodeInt(int Value) { return Value.ToString(); }
        public static String EncodeFloat(double Value) { return Value.ToString(); }
        public static String EncodeString(String Value)
        {
            String Result = Value;
            Result = Result.Replace("\\", "\\\\");
            Result = Result.Replace("|", "\\|");
            Result = Result.Replace("\"", "\\\"");
            Result = String.Concat("\"", Result, "\"");
            return Result;
        }
        public static String EncodeTable(TableValue Value)
        {
            // Figure out how to turn the table into text
            TableDigested DigestedTable = Value.DigestTable();

            // Begin result bracket thing.
            StringBuilder Result = new StringBuilder("[");

            // Turn everything into text.
            foreach (var item in DigestedTable.AutoIntArray)
            {
                Result.Append(EncoderStream.EncodeValue(item));
                Result.Append(';');
            }
            foreach (var item in DigestedTable.ManualIntDictionary)
            {
                ValueBase v = item.Value;
                Result.Append(item.Key.ToString());
                Result.Append(':');
                Result.Append(EncoderStream.EncodeValue(v));
                Result.Append(';');
            }
            foreach (var item in DigestedTable.MiscKeyDictionary)
            {
                Result.Append(item.Key.EncodeIntoValue());
                Result.Append(':');
                ValueBase v = item.Value;
                Result.Append(EncoderStream.EncodeValue(v));
                Result.Append(';');
            }

            // Close the plain-text table and return it.
            Result.Append(']');
            return Result.ToString();
        }
    }
    public class DecoderStream
    {
        private List<ValueBase> _output = new List<ValueBase>();
        private String _input = "";
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
        public void InputValue(String Value) { _input = String.Concat(_input, Value); }
        public String ReadInput() { return _input; }

        #region Parser

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
            if (P_TableStack.Count > 0 | false)
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
        private void P_ThrowError(int ErrorCode) {
            P_ErrorCode = ErrorCode;
        }
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
                char c = _input[P_Position];
                int TableStackUBound = P_TableStack.Count - 1;
                P_TableLevel CurrentTable = null;
                bool EndingBlock = false;

                if (P_TableStack.Count > 0)
                    CurrentTable = P_TableStack[TableStackUBound];

                // To know when to end a stream block
                #region BlockEnder
                if (c == '|' && !P_EscapedInString)
                {
                    P_FailSafeEndCharCount++;
                    if (!P_EscapedInString)
                        P_EndCharCount++;
                    if (P_EndCharCount == 2 | P_FailSafeEndCharCount == 2)
                        EndingBlock = true;
                }
                else
                {
                    if (P_EndCharCount == 1)
                        P_ThrowError(1);
                    P_FailSafeEndCharCount = 0;
                    P_EndCharCount = 0;
                }
                #endregion

                if (P_ErrorCode == 0) // Don't permit other processes if there is an error.
                {
                    bool CIsUnhandled = true;
                    bool String_IsFirstBracket = false;

                    #region ValueTypeDeterminer
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
                                    P_TableStack.Add(new P_TableLevel() { Table = new TableDigested() });
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
                    #endregion

                    // Parse the value.
                    #region ValueExtractor
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
                                            P_ValueParser_OutputValue(new IntValue(int.Parse(P_ValueParser_ValueRaw.ToString())));
                                            break;
                                        case 3:
                                            P_ValueParser_OutputValue(new FloatValue(double.Parse(P_ValueParser_ValueRaw.ToString())));
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
                    #endregion

                    #region TableChars
                    // Table-Specific
                    if (!P_ValueParser_StringMode) // Only process these when not in a string.
                    {
                        if (CurrentTable != null)
                        {
                            bool handled1 = true;

                            int EndType = 0; // 1 = Finish value and ready for next. 2 = Finish value and table.
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
                                                CurrentTable.Table.ManualIntDictionary.Add(((IntValue)V1).Value, V2);
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
                    #endregion

                    // If nothing handled this character, it must be invalid.
                    if (CIsUnhandled) 
                    {
                        if (c == ' ' | c == '|') // Ignore spaces in this case.
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

        private static bool P_IsCharNumber(char c)
        {
            foreach (char item in (new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }))
                if (c == item) return true;
            return false;
        }
        private class P_TableLevel
        {
            public TableDigested Table;
            public bool PairHasCustomKey = false;
            public ValueBase V1 = null;
            public ValueBase V2 = null;

            public P_TableLevel() { }
        }

        public int RunParser()
        {
            // Keep running the stepper until it says it can't go further,
            // and while doing so, count how many times it stepped.
            int result = 0;
            while (P_Step()) result++;
            return result;
        }

        #endregion

        public DecoderStream() { }

        public static ValueBase DecodeValue(String Value)
        {
            DecoderStream d = new DecoderStream();
            d.InputValue(Value.Replace("||","  ")); // Input the value, except remove the double lines to avoid problems.
            d.InputValue("||");
            d.RunParser();
            return d.PopOutput()[0];
        }
    }
}
