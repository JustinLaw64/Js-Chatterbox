/*

Js Encoder by Justin Law

Copyright 2018-2019 Justin Law. All rights reserved.

*/

using System;
using System.Collections.Generic;
using System.Text;

using JsEncoder_Type_Float = System.Double;

namespace JsEncoder
{
    /// <summary>
    /// Serves as the abstract interface for the basic data types.
    /// 
    /// Do not inherit in your own classes!
    /// </summary>
    public interface IAbstractValue
    {
        ValueType ValueType { get; }

        string EncodeIntoValue();
        bool Compare(IAbstractValue OtherValue);
    }
    public struct BooleanValue : IAbstractValue
    {
        public bool Value { get; private set; }
        public ValueType ValueType { get; private set; }
        public string EncodeIntoValue() { return EncoderStream.EncodeBoolean(Value); }
        public bool Compare(IAbstractValue OtherValue)
        {
            if (OtherValue != null && OtherValue.ValueType == this.ValueType)
                return Value == ((BooleanValue)OtherValue).Value;
            else
                return false;
        }

        public BooleanValue(bool NewValue)
        {
            ValueType = ValueType.Boolean;
            Value = NewValue;
        }
    }
    public struct IntValue : IAbstractValue
    {
        public int Value { get; private set; }
        public ValueType ValueType { get; private set; }
        public string EncodeIntoValue() { return EncoderStream.EncodeInt(Value); }
        public bool Compare(IAbstractValue OtherValue)
        {
            if (OtherValue != null && OtherValue.ValueType == this.ValueType)
                return Value == ((IntValue)OtherValue).Value;
            else
                return false;
        }

        public IntValue(int NewValue)
        {
            ValueType = ValueType.Int;
            Value = NewValue;
        }
    }
    public struct FloatValue : IAbstractValue
    {
        public JsEncoder_Type_Float Value { get; private set; }
        public ValueType ValueType { get; private set; }
        public string EncodeIntoValue() { return EncoderStream.EncodeFloat(Value); }
        public bool Compare(IAbstractValue OtherValue)
        {
            if (OtherValue != null && OtherValue.ValueType == this.ValueType)
                return Value == ((FloatValue)OtherValue).Value;
            else
                return false;
        }

        public FloatValue(JsEncoder_Type_Float NewValue)
        {
            ValueType = ValueType.Float;
            Value = NewValue;
        }
    }
    public struct StringValue : IAbstractValue
    {
        public string Value { get; private set; }
        public ValueType ValueType { get; private set; }
        public string EncodeIntoValue() { return EncoderStream.EncodeString(Value); }
        public bool Compare(IAbstractValue OtherValue)
        {
            if (OtherValue != null && OtherValue.ValueType == this.ValueType)
                return Value == ((StringValue)OtherValue).Value;
            else
                return false;
        }

        public StringValue(string NewValue)
        {
            ValueType = ValueType.String;
            Value = NewValue;
        }
    }
    /// <summary>
    /// This is a basic data type unique to this encoder. The main differences between regular arrays
    /// and this data type are that it can hold a set of basic data types rather than only one and
    /// numerical indexes start at 1 instead of 0. It therefore functions similarly to tables in the Lua
    /// programming language.
    /// </summary>
    public sealed class TableValue : IAbstractValue
    {
        public ValueType ValueType { get; private set; }
        public readonly Dictionary<IAbstractValue, IAbstractValue> Dictionary;

        public string EncodeIntoValue() { return EncoderStream.EncodeTable(this); }
        public bool Compare(IAbstractValue OtherValue)
        {
            if (OtherValue != null && OtherValue.ValueType == this.ValueType)
                return Compare((TableValue)OtherValue);
            else
                return false;
        }
        public bool Compare(TableValue OtherTable)
        {
            TableValue that = OtherTable; // It's weird to use this as a parameter name.

            // Figure out whether they match in size. If it's not, immediate false.
            if (this.Dictionary.Count != that.Dictionary.Count)
                return false;

            // Compare the Tables!
            foreach (KeyValuePair<IAbstractValue, IAbstractValue> item in this.Dictionary)
            {
                IAbstractValue v = null;
                if (!(that.Dictionary.TryGetValue(item.Key, out v) && v == item.Value)) // Out v?
                    return false;
            }
            return true;
        }

        public IAbstractValue this[IAbstractValue key]
        {
            get
            {
                IAbstractValue r = null;
                Dictionary.TryGetValue(key, out r);
                return r;
            }
            set { Dictionary[key] = value; }
        }
        public IAbstractValue this[int key]
        {
            get { return this[new IntValue(key)]; }
            set { this[new IntValue(key)] = value; }
        }
        public bool ContainsKey(IAbstractValue key) { return Dictionary.ContainsKey(key); }
        public bool ContainsKey(int key) { return ContainsKey(new IntValue(key)); }

        public TableValue(IDictionary<IAbstractValue, IAbstractValue> Dictionary) : this(new Dictionary<IAbstractValue, IAbstractValue>(Dictionary)) { }
        public TableValue() : this(new Dictionary<IAbstractValue, IAbstractValue>()) { }

        private TableValue(Dictionary<IAbstractValue, IAbstractValue> Dictionary)
        {
            this.ValueType = ValueType.Table;
            this.Dictionary = Dictionary;
        }

        /// <summary>
        /// Converts an indexed array into a TableValue with the starting index being offset by 1.
        /// </summary>
        public static TableValue ArrayToTable(IEnumerable<IAbstractValue> array)
        {
            Dictionary<IAbstractValue, IAbstractValue> dictionary = new Dictionary<IAbstractValue, IAbstractValue>();
            int i = 1;
            foreach (IAbstractValue item in array)
            {
                dictionary.Add(new IntValue(i), item);
                i++;
            }
            return new TableValue(dictionary);
        }
    }

    /// <summary>
    /// Provides facilities for encoding various basic data types. An instance of it can also create
    /// a stream of encoded text which can then be decoded by DecoderStream in another application.
    /// </summary>
    public sealed class EncoderStream
    {
        private StringBuilder _output = new StringBuilder();
        private bool _HasOutput = false;

        public string PopOutput()
        {
            string r = _output.ToString();
            _output.Clear();
            _HasOutput = false;
            return r;
        }
        public string ReadOutput() { return _output.ToString(); }
        public void InputValue(IAbstractValue Value)
        {
            _output.Append(EncodeValue(Value));
            _output.Append("||");
            _HasOutput = true;
        }
        public bool HasOutput() { return _HasOutput; }

        public EncoderStream() { }
        
        public static string EncodeValue(IAbstractValue Value)
        {
            if (Value != null)
                return Value.EncodeIntoValue();
            else
                return EncodeNull();
        }
        public static string EncodeNull() { return "N"; } // Fallback for when a value to encode is null.
        public static string EncodeBoolean(bool Value) { return Value ? "T" : "F"; }
        public static string EncodeInt(int Value) { return Value.ToString(); }
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
        public static string EncodeTable(TableValue Table)
        {
            StringBuilder r = new StringBuilder();

            // Turn everything into text.
            r.Append('[');
            List<IAbstractValue> processedKeys = new List<IAbstractValue>();
            for (int i = 1; Table.ContainsKey(i); i++)
            {
                IAbstractValue item = Table[i];
                r.Append(EncoderStream.EncodeValue(item));
                r.Append(';');
                processedKeys.Add(new IntValue(i));
                item = null;
            }
            foreach (KeyValuePair<IAbstractValue, IAbstractValue> item2 in Table.Dictionary)
            {
                if (!processedKeys.Contains(item2.Key))
                {
                    r.Append(item2.Key.EncodeIntoValue());
                    r.Append(':');
                    IAbstractValue v = item2.Value;
                    r.Append(EncoderStream.EncodeValue(v));
                    r.Append(';');
                }
            }
            r.Append(']');
            
            return r.ToString();
        }
    }
    /// <summary>
    /// Provides facilities for decoding previously-encoded data. An instance of it can also decode
    /// portions of a stream of text at different times.
    /// </summary>
    public sealed class DecoderStream
    {
        private List<IAbstractValue> _output = new List<IAbstractValue>();
        private string _input = "";
        private bool _HasOutput = false;

        public bool HasOutput() { return _HasOutput; }

        public IAbstractValue[] PopOutput()
        {
            IAbstractValue[] r = _output.ToArray();
            _output.Clear();
            _HasOutput = false;
            return r;
        }
        public IAbstractValue[] ReadOutput() { return _output.ToArray(); }
        public void InputValue(string Value) { _input = string.Concat(_input, Value); }
        public string ReadInput() { return _input; }
        public int RunParser()
        {
            // Keep running the stepper until it says it can't go further,
            // and while doing so, count how many times it stepped.
            int r = 0;
            while (P_Step())
                r++;
            return r;
        }

        #region Parser

        private class P_TableLevel
        {
            public TableValue Table;
            public bool PairHasCustomKey = false;
            public IAbstractValue V1 = null;
            public IAbstractValue V2 = null;
            public int IndexingUBound = 0;

            public P_TableLevel() { }
        }

        private int P_Position = 0;

        private int P_EndCharCount = 0;
        private int P_FailSafeEndCharCount = 0;
        private int P_ErrorCode = 0;

        private List<P_TableLevel> P_TableStack = new List<P_TableLevel>();

        private StringBuilder P_ValueParser_ValueRaw = new StringBuilder();

        /// <summary>
        /// -2 = Not ready for value, -1 = Ready for value, 0 = null,
        /// 1 = Boolean , 2 = Int , 3 = Float, 4 = String
        /// </summary>
        private int P_ValueParser_ValueType = -1;
        private bool P_ValueParser_StringMode = false;
        private IAbstractValue P_ValueParser_ValueAtReady = null;
        private bool P_ValueParser_ValueIsReady = false;
        private bool P_CharEscapedInString = false;

        private void P_EndBlock()
        {
            if (P_TableStack.Count > 0)
                P_ThrowError(1);

            // Output
            if (P_ErrorCode == 0)
                _output.Add(P_ValueParser_TakeValue());
            else
            {
                TableValue TableResult = new TableValue();
                TableResult[1] = new StringValue("Error");
                TableResult[2] = new IntValue(P_ErrorCode);
                _output.Add(TableResult);
            }

            // Clear up broken business
            P_TableStack.Clear();
            P_ValueParser_StringMode = false;
            P_CharEscapedInString = false;
            P_ValueParser_ValueAtReady = null;
            P_ValueParser_ValueRaw.Clear();
            P_ValueParser_ValueType = -1;

            P_ErrorCode = 0;

            // Purge the last block from the input stream to save memory.
            _input = _input.Substring(P_Position);
            P_Position = 0;
        }
        private void P_ThrowError(int ErrorCode) { P_ErrorCode = ErrorCode; }
        private void P_ValueParser_OutputValue(IAbstractValue Value)
        {
            P_ValueParser_ValueRaw.Clear();
            P_ValueParser_ValueAtReady = Value;
            P_ValueParser_ValueType = -2;
            P_ValueParser_ValueIsReady = true;
        }
        private IAbstractValue P_ValueParser_TakeValue()
        {
            if (P_ValueParser_ValueIsReady)
            {
                IAbstractValue r = P_ValueParser_ValueAtReady;
                P_ValueParser_ValueAtReady = null;
                return r;
            }
            else
                throw new Exception("Can't take a finished value if it's not there. Use P_ValueParser_ValueIsReady to check first.");
        }

        private bool P_Step()
        {
            bool r = P_Position < _input.Length;
            if (r)
            {
                char c = _input[P_Position];
                int TableStackUBound = P_TableStack.Count - 1;
                P_TableLevel CurrentTable = null;
                bool EndingBlock = false;

                if (P_TableStack.Count > 0)
                    CurrentTable = P_TableStack[TableStackUBound];

                // To know when to end a stream block
                #region BlockEnder
                if (c == '|')
                {
                    P_FailSafeEndCharCount++;
                    if (!P_CharEscapedInString)
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

                if (P_ErrorCode == 0) // If no error then...
                {
                    bool CharIsHandled = false;
                    bool String_IsFirstBracket = false;

                    #region Value Type Determiner
                    if (P_ValueParser_ValueType == -1)
                    {
                        switch (c)
                        {
                            case 'T':
                            case 'F':
                                P_ValueParser_ValueType = 1; // Boolean
                                CharIsHandled = true;
                                break;
                            case 'N':
                                P_ValueParser_ValueType = 0; // null
                                CharIsHandled = true;
                                break;
                            case '\"':
                                P_ValueParser_ValueType = 4; // String
                                String_IsFirstBracket = true;
                                P_ValueParser_StringMode = true;
                                CharIsHandled = true;
                                break;
                            case '[':
                                P_TableLevel NewTableLevel = new P_TableLevel();
                                NewTableLevel.Table = new TableValue();
                                P_TableStack.Add(NewTableLevel);
                                CharIsHandled = true;
                                break;
                            default:
                                if (P_IsCharNumber(c))
                                {
                                    P_ValueParser_ValueType = 2; // Int
                                    CharIsHandled = true;
                                }

                                break;
                        }
                    }
                    #endregion

                    #region Value Extractor
                    // Parse the value.
                    if (P_ValueParser_ValueType >= 0)
                    {
                        switch (P_ValueParser_ValueType)
                        {
                            case 0: // null
                            case 1: // Boolean
                                if (P_ValueParser_ValueRaw.Length == 0)
                                    P_ValueParser_ValueRaw.Append(c);
                                else
                                {
                                    IAbstractValue o = null;
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
                                CharIsHandled = true;
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
                                        P_ThrowError(1); // There should not be two decimals in the same number.
                                }
                                else if (P_ValueParser_ValueType == 2)
                                    P_ValueParser_OutputValue(new IntValue(int.Parse(P_ValueParser_ValueRaw.ToString())));
                                else if (P_ValueParser_ValueType == 3)
                                    P_ValueParser_OutputValue(new FloatValue(JsEncoder_Type_Float.Parse(P_ValueParser_ValueRaw.ToString())));
                                CharIsHandled = true;
                                break;
                            case 4: // String
                                if (P_ValueParser_StringMode)
                                {
                                    bool String_RecordChar = true; // False to avoid recording current character into a string.

                                    if (!P_CharEscapedInString)
                                    {
                                        if (c == '\\')
                                        {
                                            String_RecordChar = false;
                                            P_CharEscapedInString = true;
                                        }
                                        else if (c == '\"')
                                        {
                                            String_RecordChar = false;
                                            if (!String_IsFirstBracket)
                                            {
                                                P_ValueParser_StringMode = false;
                                                P_ValueParser_OutputValue(new StringValue(P_ValueParser_ValueRaw.ToString()));
                                            }
                                        }
                                    }
                                    else
                                        P_CharEscapedInString = false;

                                    if (String_RecordChar)
                                        P_ValueParser_ValueRaw.Append(c);
                                }
                                CharIsHandled = true;
                                break;
                        }
                    }
                    #endregion

                    #region Table Parser
                    if (!P_ValueParser_StringMode && CurrentTable != null) // Only process these when not in a string.
                    {
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
                                CharIsHandled = true;
                                break;
                            case ']':
                                EndType = 2;
                                CharIsHandled = true;
                                break;
                            case ';':
                                EndType = 1;
                                CharIsHandled = true;
                                break;
                        }
                        if (EndType > 0)
                        {
                            if (P_ValueParser_ValueType != -1)
                            {
                                CurrentTable.V2 = P_ValueParser_TakeValue();

                                if (CurrentTable.V2 != null)
                                {
                                    if (CurrentTable.V1 != null)
                                        CurrentTable.Table[CurrentTable.V1] = CurrentTable.V2;
                                    else
                                    {
                                        CurrentTable.IndexingUBound++;
                                        CurrentTable.Table[CurrentTable.IndexingUBound] = CurrentTable.V2;
                                    }
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
                                P_ValueParser_OutputValue(CurrentTable.Table);
                                P_ValueParser_ValueType = -2;
                            }
                            else //if (EndType == 1)
                                P_ValueParser_ValueType = -1;
                        }
                    }
                    #endregion

                    #region Character Ignore Handler
                    if (!CharIsHandled && (c == ' ' | c == '|' | c == '\n'))
                        CharIsHandled = true;
                    #endregion

                    // If nothing handled this character, it must be an error.
                    if (!CharIsHandled)
                        P_ThrowError(1);
                }

                P_Position++;

                if (EndingBlock)
                    P_EndBlock();
            }
            return r;
        }

        private static bool P_IsCharNumber(char c) { return c >= '0' & c <= '9'; }

        #endregion

        public DecoderStream() { }

        /// <summary>
        /// Directly decodes a string into the data that it was originally encoded from.
        /// </summary>
        public static IAbstractValue DecodeValue(string Value)
        {
            DecoderStream d = new DecoderStream();
            d.InputValue(Value.Replace("||", "  ")); // Input the value, except remove the double lines to avoid problems.
            d.InputValue("||");
            d.RunParser();
            return d.PopOutput()[0];
        }
    }

    public enum ValueType { Boolean, Int, Float, String, Table }
}
