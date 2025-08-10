using Microsoft.OData;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.OData.Buffers;
using Microsoft.OData.Edm;
using Microsoft.OData.Json;
using Microsoft.VisualBasic;
using System.Xml;
using Microsoft.AspNetCore.Mvc;

namespace PostgreCore
{
    public sealed class UnprocessableEntityObject
    {
        public DataError Error { set; get;  }
        public UnprocessableEntityObject(DataError err) 
        { 
            Error = err; 
        }    
    }


    //
    // Summary:
    //     Class representing an error payload.
    [DebuggerDisplay("{ErrorCode}: {Message}")]
    public sealed class DataError
    {
        //
        // Summary:
        //     Gets or sets the error code to be used in payloads.
        //
        // Returns:
        //     The error code to be used in payloads.
        public string Code { get; set; }

        //
        // Summary:
        //     Gets or sets the error message.
        //
        // Returns:
        //     The error message.
        public string Message { get; set; }

        //
        // Summary:
        //     Gets or sets the target of the particular error.
        //
        // Returns:
        //     For example, the name of the property in error
        public string Target { get; set; }
        /// <summary>
        /// A collection of JSON objects that MUST contain name/value pairs for code and message, and MAY contain
        /// a name/value pair for target, as described above.
        /// </summary>
        /// <returns>The error details.</returns>
        public ICollection<DataErrorDetail> Details { get; set; }

        /// <summary>Gets or sets the implementation specific debugging information to help determine the cause of the error.</summary>
        /// <returns>The implementation specific debugging information.</returns>



        /// <summary>
        /// Serialization to Json format string representing the error object.
        /// </summary>
        /// <returns>The string in Json format.</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            // `code` and `message` must be included
            string code = this.Code == null ? string.Empty : JsonValueUtils.GetEscapedJsonString(this.Code);
            string message = this.Message == null ? string.Empty : JsonValueUtils.GetEscapedJsonString(this.Message);

            builder.Append("{\"error\":{");
            builder.Append('"').Append(JsonConstants.ODataErrorCodeName).Append("\":");
            builder.Append('"').Append(code).Append('"');
            builder.Append(",\"").Append(JsonConstants.ODataErrorMessageName).Append("\":");
            builder.Append('"').Append(message).Append('"');

            if (this.Target != null)
            {
                builder.Append(",\"").Append(JsonConstants.ODataErrorTargetName).Append("\":");
                builder.Append('"').Append(JsonValueUtils.GetEscapedJsonString(this.Target)).Append('"');
            }

            if (this.Details != null)
            {
                builder.Append(",\"").Append(JsonConstants.ODataErrorDetailsName).Append("\":");
                builder.Append(GetJsonStringForDetails());
            }

            builder.Append("}}");

            return builder.ToString();
        }

        /// <summary>
        /// Convert the Details property to Json format string.
        /// </summary>
        /// <returns>Json format string representing collection.</returns>
        private string GetJsonStringForDetails()
        {
            Debug.Assert(this.Details != null, "this.Details != null");

            StringBuilder builder = new StringBuilder();

            builder.Append('[');
            builder.AppendJoin(',', this.Details.Where(d => d != null).Select(d => d.ToJsonString()));
            builder.Append(']');

            return builder.ToString();
        }

    }

    public sealed class DataErrorDetail
    {
        /// <summary>Gets or sets the error code to be used in payloads.</summary>
        /// <returns>The error code to be used in payloads.</returns>
        public string Code { get; set; }

        /// <summary>Gets or sets the error message.</summary>
        /// <returns>The error message.</returns>
        public string Message { get; set; }

        /// <summary>Gets or sets the target of the particular error.</summary>
        /// <returns>For example, the name of the property in error</returns>
        public string Target { get; set; }

        /// <summary>
        /// Serialization to Json format string.
        /// </summary>
        /// <returns>The string in Json format</returns>
        internal string ToJsonString()
        {
            StringBuilder builder = new StringBuilder();

            // `code` and `message` must be included
            string code = this.Code == null ? string.Empty : JsonValueUtils.GetEscapedJsonString(this.Code);
            string message = this.Message == null ? string.Empty : JsonValueUtils.GetEscapedJsonString(this.Message);

            builder.Append('{');
            builder.Append('"').Append(JsonConstants.ODataErrorCodeName).Append("\":");
            builder.Append('"').Append(code).Append('"');
            builder.Append(",\"").Append(JsonConstants.ODataErrorMessageName).Append("\":");
            builder.Append('"').Append(message).Append('"');

            if (this.Target != null)
            {
                builder.Append(",\"").Append(JsonConstants.ODataErrorTargetName).Append("\":");
                builder.Append("\"").Append(JsonValueUtils.GetEscapedJsonString(this.Target)).Append('"');
            }

            builder.Append('}');

            return builder.ToString();
        }
    }

    internal static class JsonConstants
    {
        /// <summary>"actions" header for resource metadata.</summary>
        internal const string ODataActionsMetadataName = "actions";

        /// <summary>"functions" header for resource metadata.</summary>
        internal const string ODataFunctionsMetadataName = "functions";

        /// <summary>"title" header for "actions" and "functions" metadata.</summary>
        internal const string ODataOperationTitleName = "title";

        /// <summary>"metadata" header for "actions" and "functions" metadata.</summary>
        internal const string ODataOperationMetadataName = "metadata";

        /// <summary>"target" header for "actions" and "functions" metadata.</summary>
        internal const string ODataOperationTargetName = "target";

        /// <summary>
        /// "error" header for the error payload
        /// </summary>
        internal const string ODataErrorName = "error";

        /// <summary>
        /// "code" header for the error code property
        /// </summary>
        internal const string ODataErrorCodeName = "code";

        /// <summary>
        /// "message" header for the error message property
        /// </summary>
        internal const string ODataErrorMessageName = "message";

        /// <summary>
        /// "target" header for the error message property
        /// </summary>
        internal const string ODataErrorTargetName = "target";

        /// <summary>
        /// "details" header for the inner error property
        /// </summary>
        internal const string ODataErrorDetailsName = "details";

        /// <summary>
        /// "innererror" header for the inner error property
        /// </summary>
        internal const string ODataErrorInnerErrorName = "innererror";

        /// <summary>
        /// "message" header for an inner error (for Astoria compatibility)
        /// </summary>
        internal const string ODataErrorInnerErrorMessageName = "message";

        /// <summary>
        /// "typename" header for an inner error (for Astoria compatibility)
        /// </summary>
        internal const string ODataErrorInnerErrorTypeNameName = "type";

        /// <summary>
        /// "stacktrace" header for an inner error (for Astoria compatibility)
        /// </summary>
        internal const string ODataErrorInnerErrorStackTraceName = "stacktrace";

        /// <summary>
        /// "internalexception" header for an inner, inner error property (for Astoria compatibility)
        /// </summary>
        internal const string ODataErrorInnerErrorInnerErrorName = "internalexception";

        /// <summary>
        /// JSON datetime format.
        /// </summary>
        internal const string ODataDateTimeFormat = @"\/Date({0})\/";

        /// <summary>
        /// JSON datetime offset format.
        /// </summary>
        internal const string ODataDateTimeOffsetFormat = @"\/Date({0}{1}{2:D4})\/";

        /// <summary>
        /// A plus sign for the date time offset format.
        /// </summary>
        internal const string ODataDateTimeOffsetPlusSign = "+";

        /// <summary>
        /// The fixed property name for the entity sets array in a service document payload.
        /// </summary>
        internal const string ODataServiceDocumentEntitySetsName = "EntitySets";

        /// <summary>
        /// The true value literal.
        /// </summary>
        internal const string JsonTrueLiteral = "true";

        /// <summary>
        /// The false value literal.
        /// </summary>
        internal const string JsonFalseLiteral = "false";

        /// <summary>
        /// The null value literal.
        /// </summary>
        internal const string JsonNullLiteral = "null";

        /// <summary>
        /// Character which starts the object scope.
        /// </summary>
        internal const string StartObjectScope = "{";

        /// <summary>
        /// Character which ends the object scope.
        /// </summary>
        internal const string EndObjectScope = "}";

        /// <summary>
        /// Character which starts the array scope.
        /// </summary>
        internal const string StartArrayScope = "[";

        /// <summary>
        /// Character which ends the array scope.
        /// </summary>
        internal const string EndArrayScope = "]";

        /// <summary>
        /// "(" Json Padding Function scope open parens.
        /// </summary>
        internal const string StartPaddingFunctionScope = "(";

        /// <summary>
        /// ")" Json Padding Function scope close parens.
        /// </summary>
        internal const string EndPaddingFunctionScope = ")";

        /// <summary>
        /// The separator between object members.
        /// </summary>
        internal const string ObjectMemberSeparator = ",";

        /// <summary>
        /// The separator between array elements.
        /// </summary>
        internal const string ArrayElementSeparator = ",";

        /// <summary>
        /// The separator between the name and the value.
        /// </summary>
        internal const string NameValueSeparator = ":";

        /// <summary>
        /// The quote character.
        /// </summary>
        internal const char QuoteCharacter = '"';
    }
    internal static partial class JsonValueUtils
    {
        /// <summary>
        /// PositiveInfinitySymbol used in OData Json format
        /// </summary>
        internal const string ODataJsonPositiveInfinitySymbol = "INF";

        /// <summary>
        /// NegativeInfinitySymbol used in OData Json format
        /// </summary>
        internal const string ODataJsonNegativeInfinitySymbol = "-INF";

        /// <summary>
        /// The NumberFormatInfo used in OData Json format.
        /// </summary>
        internal static readonly NumberFormatInfo ODataNumberFormatInfo = JsonValueUtils.InitializeODataNumberFormatInfo();

        /// <summary>
        /// Const tick value for calculating tick values.
        /// </summary>
        private static readonly long JsonDateTimeMinTimeTicks = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;

        /// <summary>
        /// Characters which, if found inside a number, indicate that the number is a double when no other type information is available.
        /// </summary>
        internal static readonly char[] DoubleIndicatingCharacters = new char[] { '.', 'e', 'E' };

        /// <summary>
        /// Map of special characters to strings.
        /// </summary>
        private static readonly string[] SpecialCharToEscapedStringMap = JsonValueUtils.CreateSpecialCharToEscapedStringMap();

        /// <summary>
        /// Write a char value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">The char value to write.</param>
        /// <param name="stringEscapeOption">The ODataStringEscapeOption to use in escaping the string.</param>
        internal static void WriteValue(TextWriter writer, char value, ODataStringEscapeOption stringEscapeOption)
        {
            Debug.Assert(writer != null, "writer != null");

            if (stringEscapeOption == ODataStringEscapeOption.EscapeNonAscii || value <= 0x7F)
            {
                string escapedString = JsonValueUtils.SpecialCharToEscapedStringMap[value];
                if (escapedString != null)
                {
                    writer.Write(escapedString);
                    return;
                }
            }

            writer.Write(value);
        }

        /// <summary>
        /// Write a boolean value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">The boolean value to write.</param>
        internal static void WriteValue(TextWriter writer, bool value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(FormatAsBooleanLiteral(value));
        }

        /// <summary>
        /// Write an integer value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Integer value to be written.</param>
        internal static void WriteValue(TextWriter writer, int value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

 
        /// <summary>
        /// Write a short value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Short value to be written.</param>
        internal static void WriteValue(TextWriter writer, short value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Write a long value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Long value to be written.</param>
        internal static void WriteValue(TextWriter writer, long value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

  
        /// <summary>
        /// Write a Guid value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Guid value to be written.</param>
        internal static void WriteValue(TextWriter writer, Guid value)
        {
            Debug.Assert(writer != null, "writer != null");

            JsonValueUtils.WriteQuoted(writer, value.ToString());
        }

        /// <summary>
        /// Write a decimal value
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Decimal value to be written.</param>
        internal static void WriteValue(TextWriter writer, decimal value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }


 
        /// <summary>
        /// Write a TimeOfDay value
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">TimeOfDay value to be written.</param>
        internal static void WriteValue(TextWriter writer, TimeOfDay value)
        {
            Debug.Assert(writer != null, "writer != null");

            JsonValueUtils.WriteQuoted(writer, value.ToString());
        }

        /// <summary>
        /// Write a Date value
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Date value to be written.</param>
        internal static void WriteValue(TextWriter writer, Date value)
        {
            Debug.Assert(writer != null, "writer != null");

            JsonValueUtils.WriteQuoted(writer, value.ToString());
        }

        /// <summary>
        /// Write a byte value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">Byte value to be written.</param>
        internal static void WriteValue(TextWriter writer, byte value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Write an sbyte value.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="value">SByte value to be written.</param>
        internal static void WriteValue(TextWriter writer, sbyte value)
        {
            Debug.Assert(writer != null, "writer != null");

            writer.Write(value.ToString(CultureInfo.InvariantCulture));
        }




 
         /// <summary>
        /// Convert string to Json-formatted string with proper escaped special characters.
        /// Note that the return value is not enclosed by the top level double-quotes.
        /// </summary>
        /// <param name="inputString">string that might contain special characters.</param>
        /// <returns>A string with special characters escaped properly.</returns>
        internal static string GetEscapedJsonString(string inputString)
        {
            Debug.Assert(inputString != null, "The string value must not be null.");

            StringBuilder builder = new StringBuilder();
            int startIndex = 0;
            int inputStringLength = inputString.Length;
            int subStrLength;
            for (int currentIndex = 0; currentIndex < inputStringLength; currentIndex++)
            {
                char c = inputString[currentIndex];

                // Append the un-handled characters (that do not require special treatment)
                // to the string builder when special characters are detected.
                if (JsonValueUtils.SpecialCharToEscapedStringMap[c] == null)
                {
                    continue;
                }

                // Flush out the un-escaped characters we've built so far.
                subStrLength = currentIndex - startIndex;
                if (subStrLength > 0)
                {
                    builder.Append(inputString.Substring(startIndex, subStrLength));
                }

                builder.Append(JsonValueUtils.SpecialCharToEscapedStringMap[c]);
                startIndex = currentIndex + 1;
            }

            subStrLength = inputStringLength - startIndex;
            if (subStrLength > 0)
            {
                builder.Append(inputString.Substring(startIndex, subStrLength));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Escapes and writes a character buffer, flushing to the writer as the buffer fills.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="character">The character to write to the buffer.</param>
        /// <param name="buffer">Char buffer to use for streaming data.</param>
        /// <param name="bufferIndex">The index into the buffer in which to write the character.</param>
        /// <param name="stringEscapeOption">The string escape option.</param>
        /// <returns>Current position in the buffer after the character has been written.</returns>
        /// <remarks>
        /// IMPORTANT: After all characters have been written,
        /// caller is responsible for writing the final buffer contents to the writer.
        /// </remarks>
        private static int EscapeAndWriteCharToBuffer(TextWriter writer, char character, char[] buffer, int bufferIndex, ODataStringEscapeOption stringEscapeOption)
        {
            int bufferLength = buffer.Length;
            string escapedString = null;

            if (stringEscapeOption == ODataStringEscapeOption.EscapeNonAscii || character <= 0x7F)
            {
                escapedString = JsonValueUtils.SpecialCharToEscapedStringMap[character];
            }

            // Append the unhandled characters (that do not require special treatment)
            // to the buffer.
            if (escapedString == null)
            {
                buffer[bufferIndex] = character;
                bufferIndex++;
            }
            else
            {
                // Okay, an unhandled character was detected.
                // First lets check if we can fit it in the existing buffer, if not,
                // flush the current buffer and reset. Add the escaped string to the buffer
                // and continue.
                int escapedStringLength = escapedString.Length;
                Debug.Assert(escapedStringLength <= bufferLength, "Buffer should be larger than the escaped string");

                if ((bufferIndex + escapedStringLength) > bufferLength)
                {
                    writer.Write(buffer, 0, bufferIndex);
                    bufferIndex = 0;
                }

                escapedString.CopyTo(0, buffer, bufferIndex, escapedStringLength);
                bufferIndex += escapedStringLength;
            }

            if (bufferIndex >= bufferLength)
            {
                Debug.Assert(bufferIndex == bufferLength,
                    "We should never encounter a situation where the buffer index is greater than the buffer length");
                writer.Write(buffer, 0, bufferIndex);
                bufferIndex = 0;
            }

            return bufferIndex;
        }

        /// <summary>
        /// Checks if the string contains special char and returns the first index
        /// of special char if present.
        /// </summary>
        /// <param name="inputString">string that might contain special characters.</param>
        /// <param name="stringEscapeOption">The string escape option.</param>
        /// <param name="firstIndex">first index of the special char</param>
        /// <returns>A value indicating whether the string contains special character</returns>
        private static bool CheckIfStringHasSpecialChars(string inputString, ODataStringEscapeOption stringEscapeOption, out int firstIndex)
        {
            Debug.Assert(inputString != null, "The string value must not be null.");

            firstIndex = -1;
            int inputStringLength = inputString.Length;
            for (int currentIndex = 0; currentIndex < inputStringLength; currentIndex++)
            {
                char c = inputString[currentIndex];

                if (stringEscapeOption == ODataStringEscapeOption.EscapeOnlyControls && c >= 0x7F)
                {
                    continue;
                }

                // Append the un-handled characters (that do not require special treatment)
                // to the string builder when special characters are detected.
                if (JsonValueUtils.SpecialCharToEscapedStringMap[c] != null)
                {
                    firstIndex = currentIndex;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Initialize static property ODataNumberFormatInfo.
        /// </summary>
        /// <returns>The <see cref=" NumberFormatInfo"/> object.</returns>
        private static NumberFormatInfo InitializeODataNumberFormatInfo()
        {
            NumberFormatInfo odataNumberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            odataNumberFormatInfo.PositiveInfinitySymbol = JsonValueUtils.ODataJsonPositiveInfinitySymbol;
            odataNumberFormatInfo.NegativeInfinitySymbol = JsonValueUtils.ODataJsonNegativeInfinitySymbol;
            return odataNumberFormatInfo;
        }

        /// <summary>
        /// Write the string value with quotes.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="text">String value to be written.</param>
        private static void WriteQuoted(TextWriter writer, string text)
        {
            writer.Write(JsonConstants.QuoteCharacter);
            writer.Write(text);
            writer.Write(JsonConstants.QuoteCharacter);
        }

        /// <summary>
        /// Converts the number of ticks from the .NET DateTime or DateTimeOffset structure to the ticks use in the JSON date time format.
        /// </summary>
        /// <param name="ticks">The ticks from the .NET DateTime of DateTimeOffset structure.</param>
        /// <returns>The ticks to use in the JSON date time format.</returns>
        private static long DateTimeTicksToJsonTicks(long ticks)
        {
            // Ticks in .NET are in 100-nanoseconds and start at 1.1.0001.
            // Ticks in the JSON date time format are in milliseconds and start at 1.1.1970.
            return (ticks - JsonValueUtils.JsonDateTimeMinTimeTicks) / 10000;
        }

        /// <summary>
        /// Creates the special character to escaped string map.
        /// </summary>
        /// <returns>The map of special characters to the corresponding escaped strings.</returns>
        private static string[] CreateSpecialCharToEscapedStringMap()
        {
            string[] specialCharToEscapedStringMap = new string[char.MaxValue + 1];
            for (int c = char.MinValue; c <= char.MaxValue; ++c)
            {
                if ((c < ' ') || (c > 0x7F))
                {
                    // We only need to populate for characters < ' ' and > 0x7F.
                    specialCharToEscapedStringMap[c] = string.Format(CultureInfo.InvariantCulture, "\\u{0:x4}", c);
                }
                else
                {
                    specialCharToEscapedStringMap[c] = null;
                }
            }

            specialCharToEscapedStringMap['\r'] = "\\r";
            specialCharToEscapedStringMap['\t'] = "\\t";
            specialCharToEscapedStringMap['\"'] = "\\\"";
            specialCharToEscapedStringMap['\\'] = "\\\\";
            specialCharToEscapedStringMap['\n'] = "\\n";
            specialCharToEscapedStringMap['\b'] = "\\b";
            specialCharToEscapedStringMap['\f'] = "\\f";

            return specialCharToEscapedStringMap;
        }

        /// <summary>
        /// Formats a DateTimeOffset value into JSON "\/Date(number of ticks)\/" format.
        /// </summary>
        /// <param name="value">DateTimeOffset value to be formatted.</param>
        /// <returns>The string representation of the DateTimeOffset value in JSON "\/Date(number of ticks)\/" format.</returns>
        private static string FormatDateTimeAsJsonTicksString(DateTimeOffset value)
        {
            Int32 offsetMinutes = (Int32)value.Offset.TotalMinutes;

            return String.Format(
                CultureInfo.InvariantCulture,
                JsonConstants.ODataDateTimeOffsetFormat,
                DateTimeTicksToJsonTicks(value.Ticks),
                offsetMinutes >= 0 ? JsonConstants.ODataDateTimeOffsetPlusSign : string.Empty,
                offsetMinutes);
        }

        /// <summary>
        /// Formats a boolean value into its equivalent string representation in JSON.
        /// </summary>
        /// <param name="value">Boolean value to be formatted.</param>
        /// <returns>The string representation of the boolean value.</returns>
        private static string FormatAsBooleanLiteral(bool value)
        {
            return value ? JsonConstants.JsonTrueLiteral : JsonConstants.JsonFalseLiteral;
        }

        /// <summary>
        /// Writes the byte array to the buffer.
        /// </summary>
        /// <param name="value">Byte array to be written.</param>
        /// <param name="offsetIn">A position in the byte array.</param>
        /// <param name="buffer">Char buffer to use for streaming data.</param>
        /// <param name="bufferByteSize">Desired buffer byte size.</param>
        /// <returns>A count of the bytes written to the buffer.</returns>
        private static int WriteByteArrayToBuffer(byte[] value, int offsetIn, char[] buffer, int bufferByteSize)
        {
            Debug.Assert(value != null, "value != null");
            Debug.Assert(buffer != null, "buffer != null");

            if (offsetIn + bufferByteSize > value.Length)
            {
                bufferByteSize = value.Length - offsetIn;
            }

            return Convert.ToBase64CharArray(value, offsetIn, bufferByteSize, buffer, 0);
        }

        /// <summary>
        /// Writes a substring starting at a specified position on the string to the buffer.
        /// </summary>
        /// <param name="inputString">Input string value.</param>
        /// <param name="currentIndex">The index in the string at which the substring begins.</param>
        /// <param name="buffer">Char buffer to use for streaming data.</param>
        /// <param name="bufferIndex">Current position in the buffer after the substring has been written.</param>
        /// <param name="substrLength">The length of the substring to be copied.</param>
        private static void WriteSubstringToBuffer(string inputString, ref int currentIndex, char[] buffer, ref int bufferIndex, int substrLength)
        {
            Debug.Assert(inputString != null, "inputString != null");
            Debug.Assert(buffer != null, "buffer != null");

            inputString.CopyTo(currentIndex, buffer, 0, substrLength);
            bufferIndex = substrLength;
            currentIndex += substrLength;
        }

   
        /// <summary>
        /// Writes an escaped string to the buffer.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="inputString">Input string value.</param>
        /// <param name="currentIndex">The index in the string at which copying should begin.</param>
        /// <param name="buffer">Char buffer to use for streaming data.</param>
        /// <param name="bufferIndex">Current position in the buffer after the string has been written.</param>
        /// <param name="stringEscapeOption">The string escape option.</param>
        /// <remarks>
        /// IMPORTANT: After all characters have been written,
        /// caller is responsible for writing the final buffer contents to the writer.
        /// </remarks>
        private static void WriteEscapedStringToBuffer(
            TextWriter writer,
            string inputString,
            ref int currentIndex,
            char[] buffer,
            ref int bufferIndex,
            ODataStringEscapeOption stringEscapeOption)
        {
            Debug.Assert(inputString != null, "inputString != null");
            Debug.Assert(buffer != null, "buffer != null");

            for (; currentIndex < inputString.Length; currentIndex++)
            {
                bufferIndex = EscapeAndWriteCharToBuffer(writer, inputString[currentIndex], buffer, bufferIndex, stringEscapeOption);
            }
        }

        /// <summary>
        /// Writes an escaped char array to the buffer.
        /// </summary>
        /// <param name="writer">The text writer to write the output to.</param>
        /// <param name="inputArray">Character array to write.</param>
        /// <param name="inputArrayOffset">How many characters to skip in the input array.</param>
        /// <param name="inputArrayCount">How many characters to write from the input array.</param>
        /// <param name="buffer">Char buffer to use for streaming data.</param>
        /// <param name="bufferIndex">Current position in the buffer after the string has been written.</param>
        /// <param name="stringEscapeOption">The string escape option.</param>
        /// <remarks>
        /// IMPORTANT: After all characters have been written,
        /// caller is responsible for writing the final buffer contents to the writer.
        /// </remarks>
        private static void WriteEscapedCharArrayToBuffer(
            TextWriter writer,
            char[] inputArray,
            ref int inputArrayOffset,
            int inputArrayCount,
            char[] buffer,
            ref int bufferIndex,
            ODataStringEscapeOption stringEscapeOption)
        {
            Debug.Assert(inputArray != null, "inputArray != null");
            Debug.Assert(buffer != null, "buffer != null");

            for (; inputArrayOffset < inputArrayCount; inputArrayOffset++)
            {
                bufferIndex = EscapeAndWriteCharToBuffer(writer, inputArray[inputArrayOffset], buffer, bufferIndex, stringEscapeOption);
            }
        }
    }

}
