//--------------------------------------------------------------
// <copyright file="Extensions.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the Extensions class.</summary>
//--------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the <see cref="Extensions"/> class.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the absolute directory path to its relative directory path using a given reference directory.
        /// </summary>
        /// <param name="directory">The absolute directory path of the desired directory.</param>
        /// <param name="referenceDirectory">The absolute reference directory.</param>
        /// <returns>The relative path of the first directory to the reference directory.</returns>
        public static string GetRelativeDirectory(this string directory, string referenceDirectory)
        {
            string[] splittedDirectory = directory.Split('\\');
            string[] splittedReferenceDirectory = referenceDirectory.Split('\\');

            return "\\" + string.Join("\\", splittedDirectory.Except(splittedReferenceDirectory).ToArray());
        }

        /// <summary>
        /// Encodes specified data using run-length-encoding.
        /// </summary>
        /// <param name="data">The data to be encoded.</param>
        /// <returns>The run-length-encoded data.</returns>
        public static byte[] EncodeRLE(this byte[] data)
        {
            if (!data.Any())
            {
                return data;
            }

            List<byte> encodedData = new List<byte>();
    
            for (int i = 0; i < data.Length;)
            {
                int currentAmount = 1;
                byte currentByte = data[i];
                int nextIndex = i + 1;

                while (
                    nextIndex < data.Length
                    && currentAmount < byte.MaxValue
                    && currentByte == data[nextIndex])
                {
                    currentAmount++;
                    nextIndex++;
                }

                encodedData.Add(Convert.ToByte(currentAmount));
                encodedData.Add(currentByte);
                i += currentAmount;
            }

            return encodedData.ToArray();
        }

        /// <summary>
        /// Decodes specified run-length-encoded data.
        /// </summary>
        /// <param name="encodedData">The run-length-encoded data.</param>
        /// <returns>The decoded data.</returns>
        public static byte[] DecodeRLE(this byte[] encodedData)
        {
            List<byte> decodedData = new List<byte>();

            if (!encodedData.Any() || (encodedData.Length % 2) != 0)
            {
                // Invalid RLE data.
                return decodedData.ToArray();
            }

            for (int i = 0; i < encodedData.Length; i += 2)
            {
                for (int j = 0; j < encodedData[i]; j++)
                {
                    decodedData.Add(encodedData[i + 1]);
                }
            }

            return decodedData.ToArray();
        }
    }
}
