//--------------------------------------------------------------------
// <copyright file="EnhancedFileInfo.cs" company="FH Wiener Neustadt">
//     Copyright (c) FH Wiener Neustadt. All rights reserved.
// </copyright>
// <author>Benjamin Bogner</author>
// <summary>Contains the EnhancedFileInfo class.</summary>
//--------------------------------------------------------------------
namespace CustomArchiver
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents the <see cref="EnhancedFileInfo"/> class.
    /// </summary>
    [Serializable]
    public class EnhancedFileInfo
    {
        /// <summary>
        /// The name of this <see cref="EnhancedFileInfo"/> including its extension.
        /// </summary>
        private string name;

        /// <summary>
        /// The relative path of this <see cref="EnhancedFileInfo"/>.
        /// </summary>
        private string relativePath;

        /// <summary>
        /// The absolute path of this <see cref="EnhancedFileInfo"/>.
        /// </summary>
        [NonSerialized]
        private string absolutePath;

        /// <summary>
        /// The uncompressed size of this <see cref="EnhancedFileInfo"/> in bytes.
        /// </summary>
        private long uncompressedSize;

        /// <summary>
        /// The compressed size of this <see cref="EnhancedFileInfo"/> in bytes.
        /// </summary>
        private long compressedSize;

        /// <summary>
        /// Initialises a new instance of the <see cref="EnhancedFileInfo"/> class.
        /// </summary>
        /// <param name="absolutePath">The absolute path of the file.</param>
        /// <param name="referencePath">The absolute reference path for creating the relative path of the file.</param>
        /// <exception cref="ArgumentException">
        /// Is raised when either one of the paths is invalid,
        /// or when the file path is no subdirectory of the reference directory.</exception>
        public EnhancedFileInfo(string absolutePath, string referencePath)
        {
            if (!File.Exists(absolutePath) || !Directory.Exists(referencePath))
            {
                throw new ArgumentException("The specifed file and directory paths must be valid.");
            }

            var fileInfo = new FileInfo(absolutePath);

            this.Name = fileInfo.Name;
            this.UncompressedSize = fileInfo.Length;
            this.AbsolutePath = fileInfo.FullName;
            this.RelativePath = absolutePath.GetRelativeDirectory(referencePath);
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="EnhancedFileInfo"/> class.
        /// </summary>
        public EnhancedFileInfo()
        {
        }

        /// <summary>
        /// Gets or sets name of this <see cref="EnhancedFileInfo"/> including its extension.
        /// </summary>
        /// <value>The name of this <see cref="EnhancedFileInfo"/> including its extension.</value>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (value == string.Empty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be empty.");
                }

                this.name = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the relative path of this <see cref="EnhancedFileInfo"/>.
        /// </summary>
        /// <value>The relative path of this <see cref="EnhancedFileInfo"/>..</value>
        public string RelativePath
        {
            get
            {
                return this.relativePath;
            }

            set
            {
                if (value == string.Empty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be empty.");
                }

                this.relativePath = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the absolute path of this <see cref="EnhancedFileInfo"/>
        /// </summary>
        /// <value>The absolute path of this <see cref="EnhancedFileInfo"/></value>
        public string AbsolutePath
        {
            get
            {
                return this.absolutePath;
            }

            set
            {
                if (value == string.Empty)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be empty.");
                }

                this.absolutePath = value ?? throw new ArgumentNullException(nameof(value), "The specified value cannot be null.");
            }
        }

        /// <summary>
        /// Gets or sets the uncompressed size of this <see cref="EnhancedFileInfo"/> in bytes.
        /// </summary>
        /// <value>The uncompressed size of this <see cref="EnhancedFileInfo"/> in bytes.</value>
        public long UncompressedSize
        {
            get
            {
                return this.uncompressedSize;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be less than zero.");
                }

                this.uncompressedSize = value;
            }
        }

        /// <summary>
        /// Gets or sets or sets the compressed size of this <see cref="EnhancedFileInfo"/> in bytes.
        /// </summary>
        /// <value>The compressed size of this <see cref="EnhancedFileInfo"/> in bytes.</value>
        public long CompressedSize
        {
            get
            {
                return this.compressedSize;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The specified value cannot be less than zero.");
                }

                this.compressedSize = value;
            }
        }
    }
}
