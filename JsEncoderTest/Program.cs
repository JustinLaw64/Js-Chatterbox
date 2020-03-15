using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JsEncoder;

namespace JsEncoderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Test1

            IAbstractValue[] Test1_InitialDataArray = new IAbstractValue[] { new IntValue(685000), new StringValue("Hello!") };
            TableValue Test1_InitialData = TableValue.ArrayToTable(Test1_InitialDataArray);

            Console.WriteLine("Begin Test 1");
            Console.WriteLine();

            EncoderStream Test1_EnStream = new EncoderStream();
            string Test1_EncodeResult;

            Console.Write("Encoding...");
            Test1_EnStream.InputValue(Test1_InitialData);
            Test1_EncodeResult = Test1_EnStream.PopOutput();
            Console.WriteLine(" Done!");
            Console.WriteLine(string.Concat("Encode Result: ", Test1_EncodeResult));

            Console.WriteLine();
            Console.WriteLine("End Test 1");

            #endregion

            Console.WriteLine();

            #region Test2

            // The string that was in the Specification.odt document.
            string Test2_InitialString = "[\"My name is PC!\";800;]||[\"Data1\":0.56;\"Data2\":\"true \\| false\";]||[[\"My home.\";T;1000;F;];]||";

            Console.WriteLine("Begin Test 2");
            Console.WriteLine();
            Console.WriteLine(string.Concat("Initial String: ", Test2_InitialString));
            Console.WriteLine();

            DecoderStream Test2_DeStream = new DecoderStream();
            EncoderStream Test2_EnStream = new EncoderStream();
            IAbstractValue[] Test2_DecodeResult;
            string Test2_EncodeResult;

            Console.Write("Decoding...");
            Test2_DeStream.InputValue(Test2_InitialString);
            Test2_DeStream.RunParser();
            Test2_DecodeResult = Test2_DeStream.PopOutput(); // ((TableValue)Test2_DeStream.PopOutput()[0]).Value;
            Console.WriteLine(" Done!");
            Console.Write("Encoding...");
            foreach (var item in Test2_DecodeResult)
                Test2_EnStream.InputValue(item);
            Test2_EncodeResult = Test2_EnStream.PopOutput();
            Console.WriteLine(" Done!");

            Console.WriteLine();
            Console.WriteLine(string.Concat("Encode Result: ", Test2_EncodeResult));
            Console.WriteLine(string.Concat("Strings Match: ", (Test2_InitialString == Test2_EncodeResult).ToString()));
            Console.WriteLine();
            Console.WriteLine("End Test 2");

            #endregion

            Console.WriteLine();
            Console.WriteLine("Exiting Program...");

            return;
        }
    }
}
