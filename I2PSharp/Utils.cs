using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace I2PSharp
{
    public class Utils
    {
        private static Random _Random = new Random();

        public static string GenID(int MinLength, int MaxLength)
        {

            int Length = _Random.Next(MinLength, MaxLength);
            char[] IDChar = new char[Length];
            //string AllowedChars = "!\"#$%&\'()*+,-./0123456789:;<>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            string AllowedChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // Unsure what characters are allowed in IDs, playing it safe.

            for (int i = 0; i < Length; i++)
            {
                IDChar[i] = AllowedChars[_Random.Next(AllowedChars.Length - 1)];
            }
            return new string(IDChar);
        }

        public static (SAMResponseResults result, Dictionary<string, string> response) TryParseResponse(string response)
        {
            string[] ResponseSplit = Regex.Split(response, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"); // To preserve double quoted text while splitting at space
            Dictionary<string, string> ResponseDict = new Dictionary<string, string>();

            foreach (string KeyValuePair in ResponseSplit.Where(x => x.Contains("=")))
            {
                string[] KeyValuePairSplit = KeyValuePair.Split(new char[] { '=' }, 2); 
                ResponseDict.Add(KeyValuePairSplit[0], KeyValuePairSplit[1].Replace("\"", ""));
            }

            ResponseDict["COMMAND"] = ResponseSplit[0];
            ResponseDict["SUBCOMMAND"] = ResponseSplit[1].Contains("=") ? "" : ResponseSplit[1];

            if (ResponseDict.ContainsKey("RESULT"))
            {
                string[] PossibleResponses = Enum.GetNames(typeof(SAMResponseResults));
                int ResponseIndex = Array.IndexOf(PossibleResponses, ResponseDict["RESULT"]);
                if (ResponseIndex > -1)
                {
                    return ((SAMResponseResults)ResponseIndex, ResponseDict);
                }
                else
                {
                    throw new SAMBadResultException($"Unidentified result: {ResponseDict["RESULT"]} \r\n Response: {response}");
                }
            }
            else
            {
                return (SAMResponseResults.NONE, ResponseDict);
            }
        }


    }
}
