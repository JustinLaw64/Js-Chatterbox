// Copyright (c) Justin Law 2016
// This file is reserved for private use.

#pragma once

// You can redefine these in your code so that they use another type.
#ifndef JsEncoder_Namespace
#define JsEncoder_Namespace JsEncoder
#endif
#ifndef JsEncoder_Type_Boolean
#define JsEncoder_Type_Boolean bool
#endif
#ifndef JsEncoder_Type_Int
#define JsEncoder_Type_Int int
#endif
#ifndef JsEncoder_Type_Float
#define JsEncoder_Type_Float double
#endif
#ifndef JsEncoder_Type_Char
#define JsEncoder_Type_Char wchar_t
#endif

// The code that makes this file work

#define GenericCollections System::Collections::Generic
namespace JsEncoder_Namespace {
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Text;
	using namespace System::Threading::Tasks;
	using namespace System::Collections::Generic;

	// Declare these so that they can be referenced
	// even before their members are declared.
	ref class ValueBase;
	ref class BooleanValue;
	ref class IntValue;
	ref class FloatValue;
	ref class StringValue;
	ref class TableValue;

	ref class TableDigested;

	ref class EncoderStream;
	ref class DecoderStream;

	public enum ValueType { Boolean = 1, Int = 2, Float = 3, String = 4, Table = 5 };

	// Begin defining what's in the classes.

	public ref class ValueBase abstract
	{
	protected:
		ValueBase(ValueType NewValueType);

		ValueType _ValueType;
	public:
		ValueType GetValueType();

		virtual System::String^ EncodeIntoValue() abstract;
		virtual bool Compare(ValueBase^ OtherValue) abstract;

		virtual bool Equals(Object^ obj) override sealed;
		virtual int GetHashCode() override abstract; // Microsoft Specific

		static bool operator ==(ValueBase^ v1, ValueBase^ v2);
		static bool operator !=(ValueBase^ v1, ValueBase^ v2);
	};

	public ref class BooleanValue sealed : public ValueBase
	{
	private:
		bool _Value;
	public:
		bool GetValue();
		System::String^ EncodeIntoValue() override;
		bool Compare(ValueBase^ OtherValue) override;

		int GetHashCode() override; // Microsoft Specific

		BooleanValue(bool NewValue);
	};
	public ref class IntValue sealed : public ValueBase
	{
	private:
		JsEncoder_Type_Int _Value;
	public:
		JsEncoder_Type_Int GetValue();
		System::String^ EncodeIntoValue() override;
		bool Compare(ValueBase^ OtherValue) override;

		int GetHashCode() override; // Microsoft Specific

		IntValue(JsEncoder_Type_Int NewValue);
	};
	public ref class FloatValue sealed : public ValueBase
	{
	private:
		JsEncoder_Type_Float _Value;
	public:
		JsEncoder_Type_Float GetValue();
		System::String^ EncodeIntoValue() override;
		bool Compare(ValueBase^ OtherValue) override;

		int GetHashCode() override; // Microsoft Specific

		FloatValue(JsEncoder_Type_Float NewValue);
	};
	public ref class StringValue sealed : public ValueBase
	{
	private:
		System::String^ _Value;
	public:
		System::String^ GetValue();
		System::String^ EncodeIntoValue() override;
		bool Compare(ValueBase^ OtherValue) override;

		int GetHashCode() override; // Microsoft Specific

		StringValue(System::String^ NewValue);
	};
	public ref class TableValue sealed : public ValueBase
	{
	public:
		Dictionary<ValueBase^, ValueBase^>^ Dictionary;

		System::String^ EncodeIntoValue() override;
		TableDigested^ DigestTable();
		bool Compare(ValueBase^ OtherValue) override;
		bool Compare(TableValue^ OtherTable);

		ValueBase^ Get(JsEncoder_Type_Int ID);
		ValueBase^ Get(ValueBase^ ID);
		void Set(JsEncoder_Type_Int ID, ValueBase^ Value);
		void Set(ValueBase^ ID, ValueBase^ Value);

		int GetHashCode() override; // Microsoft Specific

		TableValue(GenericCollections::IDictionary<ValueBase^, ValueBase^>^ Dictionary);
		TableValue();
	private:
		TableValue(GenericCollections::Dictionary<ValueBase^, ValueBase^>^ Dictionary);

		int ComparisonFunctionForDigestTable(KeyValuePair<int, ValueBase^> a, KeyValuePair<int, ValueBase^> b) { return a.Key - b.Key; }
	public:
		static TableValue^ ArrayToTable(IEnumerable<ValueBase^>^ array);
	};

	public ref class TableDigested sealed
	{
	public:
		GenericCollections::Dictionary<ValueBase^, ValueBase^>^ MiscKeyDictionary = gcnew GenericCollections::Dictionary<ValueBase^, ValueBase^>();
		GenericCollections::Dictionary<int, ValueBase^>^ ManualIntDictionary = gcnew GenericCollections::Dictionary<int, ValueBase^>();
		List<ValueBase^>^ AutoIntArray = gcnew List<ValueBase^>();

		TableValue^ ToTable();

		TableDigested();
	};

    public ref class EncoderStream
    {
	private:
		StringBuilder^ _output = gcnew StringBuilder();
        bool _HasOutput = false;

	public:
		System::String^ PopOutput();
		System::String^ ReadOutput();
		void InputValue(ValueBase^ Value);
		bool HasOutput();

		EncoderStream();

		// This is the only function that depends on another class for help.
		static System::String^ EncodeValue(ValueBase^ Value);

		static System::String^ EncodeNull(); // Fallback for when a value to encode is null.
		static System::String^ EncodeBoolean(bool Value);
		static System::String^ EncodeInt(JsEncoder_Type_Int Value);
		static System::String^ EncodeFloat(JsEncoder_Type_Float Value);
		static System::String^ EncodeString(System::String^ Value);
		static System::String^ EncodeTable(TableValue^ Value);
	};
    public ref class DecoderStream
    {
	private:
		List<ValueBase^>^ _output = gcnew List<ValueBase^>();
		System::String^ _input = "";
        bool _HasOutput = false;

	public:
		bool HasOutput();

		array<ValueBase^>^ PopOutput();
		array<ValueBase^>^ ReadOutput();
		void InputValue(System::String^ Value);
		System::String^ ReadInput();

		// ################################################################
		// ## BEGIN PARSER
		// ################################################################

	private:
		ref class P_TableLevel;

		int P_Position = 0;

        int P_EndCharCount = 0;
        int P_FailSafeEndCharCount = 0;
        int P_ErrorCode = 0;

        List<P_TableLevel^>^ P_TableStack = gcnew List<P_TableLevel^>();

        StringBuilder^ P_ValueParser_ValueRaw = gcnew StringBuilder();

        int P_ValueParser_ValueType = -1; // -2 = Not ready for value. ,-1 = Ready for value!, 0 = nullptr
        bool P_ValueParser_StringMode = false;
        ValueBase^ P_ValueParser_ValueAtReady = nullptr;
        bool P_ValueParser_ValueIsReady = false;
        bool P_EscapedInString = false;

		void P_EndBlock();
		void P_ThrowError(int ErrorCode);
		void P_ValueParser_OutputValue(ValueBase^ Value);
		ValueBase^ P_ValueParser_TakeValue();

        bool P_Step();

		static bool P_IsCharNumber(JsEncoder_Type_Char c);
        
		ref class P_TableLevel
        {
		public:
			TableDigested^ Table;
            bool PairHasCustomKey = false;
            ValueBase^ V1 = nullptr;
            ValueBase^ V2 = nullptr;

            P_TableLevel() { }
		};
	public:
		int RunParser();

		// ################################################################
		// ## END PARSER
		// ################################################################

	public:
		DecoderStream();

		static ValueBase^ DecodeValue(System::String^ Value);
	};


	ValueBase::ValueBase(ValueType NewValueType) : _ValueType{ NewValueType } { }
	ValueType ValueBase::GetValueType() { return _ValueType; }
	bool ValueBase::Equals(Object^ obj) {
		ValueBase^ value = dynamic_cast<ValueBase^>(obj);
		if (value != nullptr)
			return Compare(value);
		else
			return false;
	}
	bool ValueBase::operator==(ValueBase^ v1, ValueBase^ v2)
	{
		Object^ v1o = v1; // Workaround to avoid loops.
		Object^ v2o = v2;
		return (v1o != nullptr ? v1->Compare(v2) : v1o == v2o);
	}
	bool ValueBase::operator!=(ValueBase^ v1, ValueBase^ v2)
	{
		Object^ v1o = v1; // Workaround to avoid loops.
		Object^ v2o = v2;
		return (v1o != nullptr ? !v1->Compare(v2) : v1o != v2o);
	}

	bool BooleanValue::GetValue() { return _Value; }
	System::String^ BooleanValue::EncodeIntoValue() { return EncoderStream::EncodeBoolean(_Value); }
	bool BooleanValue::Compare(ValueBase^ OtherValue)
	{
		if (OtherValue != nullptr && OtherValue->GetValueType() == this->_ValueType)
		{
			BooleanValue^ ConvertedValue = (BooleanValue^)OtherValue;
			return (_Value == ConvertedValue->_Value);
		}
		else
			return false;
	}
	int BooleanValue::GetHashCode() { return (gcnew System::Boolean(_Value))->GetHashCode(); }
	BooleanValue::BooleanValue(bool NewValue) : ValueBase(ValueType::Boolean) { _Value = NewValue; }

	JsEncoder_Type_Int IntValue::GetValue() { return _Value; }
	System::String^ IntValue::EncodeIntoValue() { return EncoderStream::EncodeInt(_Value); }
	bool IntValue::Compare(ValueBase^ OtherValue)
	{
		if (OtherValue != nullptr && OtherValue->GetValueType() == this->_ValueType)
		{
			IntValue^ ConvertedValue = (IntValue^)OtherValue;
			return (_Value == ConvertedValue->_Value);
		}
		else
			return false;
	}
	int IntValue::GetHashCode() { return (gcnew System::Int32(_Value))->GetHashCode(); }
	IntValue::IntValue(JsEncoder_Type_Int NewValue) : ValueBase(ValueType::Int) { _Value = NewValue; }

	JsEncoder_Type_Float FloatValue::GetValue() { return _Value; }
	System::String^ FloatValue::EncodeIntoValue() { return EncoderStream::EncodeFloat(_Value); }
	bool FloatValue::Compare(ValueBase^ OtherValue)
	{
		if (OtherValue != nullptr && OtherValue->GetValueType() == this->_ValueType)
		{
			FloatValue^ ConvertedValue = (FloatValue^)OtherValue;
			return (_Value == ConvertedValue->_Value);
		}
		else
			return false;
	}
	int FloatValue::GetHashCode() { return (gcnew System::Double(_Value))->GetHashCode(); }
	FloatValue::FloatValue(JsEncoder_Type_Float NewValue) : ValueBase(ValueType::Float) { _Value = NewValue; }

	System::String^ StringValue::GetValue() { return _Value; }
	System::String^ StringValue::EncodeIntoValue() { return EncoderStream::EncodeString(_Value); }
	bool StringValue::Compare(ValueBase^ OtherValue)
	{
		if (OtherValue != nullptr && OtherValue->GetValueType() == this->_ValueType)
		{
			StringValue^ ConvertedValue = (StringValue^)OtherValue;
			return (_Value == ConvertedValue->_Value);
		}
		else
			return false;
	}
	int StringValue::GetHashCode() { return _Value->GetHashCode(); }
	StringValue::StringValue(System::String^ NewValue) : ValueBase(ValueType::String) { _Value = NewValue; }

	System::String^ TableValue::EncodeIntoValue() { return EncoderStream::EncodeTable(this); }
	TableDigested^ TableValue::DigestTable()
	{
		TableDigested^ Result = gcnew TableDigested();

		int HighestIndex = 1;
		for each(KeyValuePair<ValueBase^, ValueBase^>^ item in Dictionary)
		{
			if (item->Key->GetValueType() == ValueType::Int)
			{
				int index = (int)((IntValue^)item->Key)->GetValue();
				if (index > HighestIndex) HighestIndex = index;

				Result->ManualIntDictionary->Add(index, item->Value);
			}
			else
				Result->MiscKeyDictionary->Add(item->Key, item->Value);
		}

		List<KeyValuePair<int, ValueBase^>>^ SortedList = gcnew List<KeyValuePair<int, ValueBase^>>(Result->ManualIntDictionary);
		SortedList->Sort(gcnew Comparison<KeyValuePair<int, ValueBase^>>(this, &TableValue::ComparisonFunctionForDigestTable));

		// Move all of the values, stacked uninterrupted on the lowest index, to the auto int array.
		for (int i = 1; i <= HighestIndex; i++)
		{
			ValueBase^ CurrentValue = nullptr;
			if (Result->ManualIntDictionary->TryGetValue(i, CurrentValue))
			{
				Result->ManualIntDictionary->Remove(i);
				Result->AutoIntArray->Add(CurrentValue);
			}
			else
				break;
		}

		return Result;
	}
	bool TableValue::Compare(ValueBase^ OtherValue)
	{
		if (OtherValue != nullptr && OtherValue->GetValueType() == this->_ValueType)
		{
			TableValue^ ConvertedValue = (TableValue^)OtherValue;
			return (ConvertedValue->Compare(this));
		}
		else
			return false;
	}
	bool TableValue::Compare(TableValue^ OtherTable)
	{
		TableValue^ that = OtherTable; // It's weird to use this as a parameter name.

		// Figure if they match in size. If it's not, immediate false.
		if (this->Dictionary->Count != that->Dictionary->Count)
			return false;

		// Store refs to save performance.
		GenericCollections::Dictionary<ValueBase^, ValueBase^>^ ThisDictionary = this->Dictionary;
		GenericCollections::Dictionary<ValueBase^, ValueBase^>^ ThatDictionary = that->Dictionary;

		// Compare the Tables!
		for each(KeyValuePair<ValueBase^, ValueBase^> item in ThisDictionary)
		{
			ValueBase^ v = nullptr;
			if (!(ThatDictionary->TryGetValue(item.Key, v) && v == item.Value)) // Out v?
				return false;
		}
		return true;
	}
	ValueBase^ TableValue::Get(JsEncoder_Type_Int ID) { return Get(gcnew IntValue(ID)); }
	ValueBase^ TableValue::Get(ValueBase^ ID)
	{
		ValueBase^ r = nullptr;
		Dictionary->TryGetValue(ID, r); // Out r?
		return r;
	}
	void TableValue::Set(JsEncoder_Type_Int ID, ValueBase^ Value) { Set(gcnew IntValue(ID), Value); }
	void TableValue::Set(ValueBase^ ID, ValueBase^ Value)
	{
		Dictionary->Remove(ID);
		Dictionary->Add(ID, Value);
	}
	int TableValue::GetHashCode() { return Dictionary->GetHashCode(); }
	TableValue::TableValue(GenericCollections::IDictionary<ValueBase^, ValueBase^>^ Dictionary) : TableValue(gcnew GenericCollections::Dictionary<ValueBase^, ValueBase^>(Dictionary)) { }
	TableValue::TableValue() : TableValue(gcnew GenericCollections::Dictionary<ValueBase^, ValueBase^>()) { }
	TableValue::TableValue(GenericCollections::Dictionary<ValueBase^, ValueBase^>^ Dictionary) : ValueBase(ValueType::Table) { this->Dictionary = Dictionary; }
	TableValue^ TableValue::ArrayToTable(IEnumerable<ValueBase^>^ array)
	{
		TableValue^ Result = gcnew TableValue();
		JsEncoder_Type_Int i = 0;
		GenericCollections::Dictionary<ValueBase^, ValueBase^>^ ResultD = Result->Dictionary;
		for each(ValueBase^ item in array)
		{
			i++;
			ResultD->Add(gcnew IntValue(i), item);
		}
		return Result;
	}


	TableValue^ TableDigested::ToTable()
	{
		TableValue^ Result = gcnew TableValue();

		for each(KeyValuePair<int, ValueBase^>^ item in ManualIntDictionary)
			Result->Dictionary->Add(gcnew IntValue(item->Key), item->Value);
		// We only added manual integer entries yet, so we only need look at that table
		int CurrentInt = 0;
		for each(ValueBase^ item in AutoIntArray)
		{
			CurrentInt++;

			ValueBase^ TryOutput = nullptr;
			while (ManualIntDictionary->TryGetValue(CurrentInt, TryOutput)) // Out TryOutput?
				CurrentInt++;

			Result->Dictionary->Add(gcnew IntValue(CurrentInt), item);
		}
		// We don't care about what these keys are, so we add them.
		for each(KeyValuePair<ValueBase^, ValueBase^>^ item in MiscKeyDictionary)
			Result->Dictionary->Add(item->Key, item->Value);

		return Result;
	}
	TableDigested::TableDigested() { }


	System::String^ EncoderStream::PopOutput()
	{
		System::String^ Result = _output->ToString();
		_output->Clear();
		_HasOutput = false;
		return Result;
	}
	System::String^ EncoderStream::ReadOutput() { return _output->ToString(); }
	void EncoderStream::InputValue(ValueBase^ Value)
	{
		_output->Append(EncodeValue(Value));
		_output->Append("||");
		_HasOutput = true;
	}
	bool EncoderStream::HasOutput() { return _HasOutput; }
	EncoderStream::EncoderStream() { }
	System::String^ EncoderStream::EncodeValue(ValueBase^ Value) // This is the only function that depends on another class for help.
	{
		if (Value != nullptr)
			return Value->EncodeIntoValue();
		else
			return EncodeNull();
	}
	System::String^ EncoderStream::EncodeNull() { return "N"; } // Fallback for when a value to encode is null.
	System::String^ EncoderStream::EncodeBoolean(bool Value) { return (Value ? "T" : "F"); }
	System::String^ EncoderStream::EncodeInt(JsEncoder_Type_Int Value) { return Value.ToString(); }
	System::String^ EncoderStream::EncodeFloat(JsEncoder_Type_Float Value) { return Value.ToString(); }
	System::String^ EncoderStream::EncodeString(System::String^ Value)
	{
		System::String^ Result = Value;
		Result = Result->Replace("\\", "\\\\");
		Result = Result->Replace("|", "\\|");
		Result = Result->Replace("\"", "\\\"");
		Result = System::String::Concat("\"", Result, "\"");
		return Result;
	}
	System::String^ EncoderStream::EncodeTable(TableValue^ Value)
	{
		// Figure out how to turn the table into text
		TableDigested^ DigestedTable = Value->DigestTable();

		// Begin result bracket thing.
		StringBuilder^ Result = gcnew StringBuilder("[");

		// Turn everything into text.
		for each (ValueBase^ item in DigestedTable->AutoIntArray)
		{
			Result->Append(EncoderStream::EncodeValue(item));
			Result->Append(";");
		}
		for each (KeyValuePair<int, ValueBase^>^ item in DigestedTable->ManualIntDictionary)
		{
			ValueBase^ v = item->Value;

			Result->Append((gcnew Int32(item->Key))->ToString());
			Result->Append(":");
			Result->Append(EncoderStream::EncodeValue(v));
			Result->Append(";");
		}
		for each (KeyValuePair<ValueBase^, ValueBase^>^ item in DigestedTable->MiscKeyDictionary)
		{
			Result->Append(item->Key->EncodeIntoValue());
			Result->Append(":");
			ValueBase^ v = item->Value;
			Result->Append(EncoderStream::EncodeValue(v));
			Result->Append(";");
		}

		// Close the plain-text table and return it.
		Result->Append("]");
		return Result->ToString();
	}

	bool DecoderStream::HasOutput() { return _HasOutput; }
	array<ValueBase^>^ DecoderStream::PopOutput()
	{
		array<ValueBase^>^ Result = _output->ToArray();
		_output->Clear();
		_HasOutput = false;
		return Result;
	}
	array<ValueBase^>^ DecoderStream::ReadOutput() { return _output->ToArray(); }
	void DecoderStream::InputValue(System::String^ Value) { _input = System::String::Concat(_input, Value); }
	System::String^ DecoderStream::ReadInput() { return _input; }
	void DecoderStream::P_EndBlock()
	{
		if ((P_TableStack->Count > 0) | false)
			P_ThrowError(1);

		// Output
		if (P_ErrorCode == 0)
		{
			_output->Add(P_ValueParser_TakeValue());
		}
		else
		{
			TableValue^ TableResult = gcnew TableValue();
			TableResult->Dictionary->Add(gcnew IntValue(1), gcnew StringValue("Error"));
			TableResult->Dictionary->Add(gcnew IntValue(2), gcnew IntValue(P_ErrorCode));
			_output->Add(TableResult);
		}

		// Clear up broken business
		P_TableStack->Clear();
		P_ValueParser_StringMode = false;
		P_EscapedInString = false;
		P_ValueParser_ValueAtReady = nullptr;
		P_ValueParser_ValueRaw->Clear();
		P_ValueParser_ValueType = -1;

		P_ErrorCode = 0;

		// Purge the last block from the input stream to save memory.
		_input = _input->Substring(P_Position);
		P_Position = 0;
	}
	void DecoderStream::P_ThrowError(int ErrorCode) { P_ErrorCode = ErrorCode; }
	bool DecoderStream::P_Step()
	{
		bool result = (P_Position < _input->Length);
		if (result)
		{
			JsEncoder_Type_Char c = _input[P_Position];
			int TableStackUBound = P_TableStack->Count - 1;
			P_TableLevel^ CurrentTable = nullptr;
			bool EndingBlock = false;

			if (P_TableStack->Count > 0)
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
						{
							P_TableLevel^ NewTableLevel = gcnew P_TableLevel();
							NewTableLevel->Table = gcnew TableDigested();
							P_TableStack->Add(NewTableLevel);
						}
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
						if (P_ValueParser_ValueRaw->Length == 0)
							P_ValueParser_ValueRaw->Append(c);
						else
						{
							ValueBase^ o = nullptr;
							switch (P_ValueParser_ValueRaw[0])
							{
							case 'N':
								o = nullptr;
								break;
							default:
								o = gcnew BooleanValue(P_ValueParser_ValueRaw[0] == 'T');
								break;
							}
							P_ValueParser_OutputValue(o);
						}
						break;
					case 2: // Int
					case 3: // Float
						if (P_IsCharNumber(c))
							P_ValueParser_ValueRaw->Append(c);
						else if (c == '.')
						{
							if (P_ValueParser_ValueType == 2)
							{
								P_ValueParser_ValueType = 3;
								P_ValueParser_ValueRaw->Append(c);
							}
							else
								P_ThrowError(1);
						}
						else
						{
							switch (P_ValueParser_ValueType)
							{
							case 2:
								P_ValueParser_OutputValue(gcnew IntValue(System::Int32::Parse(P_ValueParser_ValueRaw->ToString())));
								break;
							case 3:
								P_ValueParser_OutputValue(gcnew FloatValue(System::Double::Parse(P_ValueParser_ValueRaw->ToString())));
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
										P_ValueParser_OutputValue(gcnew StringValue(P_ValueParser_ValueRaw->ToString()));
									}
									else
										String_RecordChar = false;
								}
							}
							else
								P_EscapedInString = false;

							if (String_RecordChar)
								P_ValueParser_ValueRaw->Append(c);
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
					if (CurrentTable != nullptr) // Only process these when not in a string.
					{
						bool handled1 = true;

						int EndType = 0;
						switch (c)
						{
						case ':':
							if (!CurrentTable->PairHasCustomKey)
							{
								CurrentTable->PairHasCustomKey = true;
								CurrentTable->V1 = P_ValueParser_TakeValue();
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
								CurrentTable->V2 = P_ValueParser_TakeValue();

								ValueBase^ V1 = CurrentTable->V1;
								ValueBase^ V2 = CurrentTable->V2;

								if (V2 != nullptr)
								{
									if (V1 != nullptr)
									{
										if (CurrentTable->V1->GetValueType() == ValueType::Int)
											CurrentTable->Table->ManualIntDictionary->Add(((IntValue^)V1)->GetValue(), V2);
										else
											CurrentTable->Table->MiscKeyDictionary->Add(V1, V2);
									}
									else
										CurrentTable->Table->AutoIntArray->Add(V2);
								}
							}

							// Clear these so we can use them later.
							CurrentTable->V1 = nullptr;
							CurrentTable->V2 = nullptr;
							CurrentTable->PairHasCustomKey = false;

							if (EndType == 2)
							{
								// Close table and go back up.
								P_TableStack->RemoveAt(TableStackUBound);
								P_ValueParser_OutputValue(CurrentTable->Table->ToTable());
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
	void DecoderStream::P_ValueParser_OutputValue(ValueBase^ Value)
	{
		P_ValueParser_ValueRaw->Clear();
		P_ValueParser_ValueAtReady = Value;
		P_ValueParser_ValueType = -2;
		P_ValueParser_ValueIsReady = true;
	}
	ValueBase^ DecoderStream::P_ValueParser_TakeValue()
	{
		if (P_ValueParser_ValueIsReady)
		{
			ValueBase^ r = P_ValueParser_ValueAtReady;
			P_ValueParser_ValueAtReady = nullptr;
			return r;
		}
		else
			throw gcnew Exception("Can't take a finished value if it's not there. Use P_ValueParser_ValueIsReady to check first.");
	}
	bool DecoderStream::P_IsCharNumber(JsEncoder_Type_Char c)
	{
		char Chars[10] = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		for each (char item in Chars)
			if (c == item) return true;
		return false;
	}
	int DecoderStream::RunParser()
	{
		// Keep running the stepper until it says it can't go further,
		// and while doing so, count how many times it stepped.
		int result = 0;
		while (P_Step()) result++;
		return result;
	}
	DecoderStream::DecoderStream() { }
	ValueBase^ DecoderStream::DecodeValue(System::String^ Value)
	{
		DecoderStream^ d = gcnew DecoderStream();
		d->InputValue(Value->Replace("||", "  ")); // Input the value, except remove the double lines to avoid problems.
		d->InputValue("||");
		d->RunParser();
		return d->PopOutput()[0];
	}
}
#undef GenericCollections
