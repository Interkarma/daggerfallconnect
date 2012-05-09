// Project:         Deep Engine
// Description:     3D game engine for Ruins of Hill Deep and Daggerfall Workshop projects.
// Copyright:       Copyright (C) 2012 Gavin Clayton
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Web Site:        http://www.dfworkshop.net
// Contact:         Gavin Clayton (interkarma@dfworkshop.net)
// Project Page:    http://code.google.com/p/daggerfallconnect/

#region Using Statements
using System;
using System.Xml;
using System.Text;
using System.IO;
#endregion

namespace DeepEngine.Utility
{

    /// <summary>
    /// Helper class to create XML documents and append elements.
    /// </summary>
    public class XmlHelper
    {

        #region Fields

        private const string extension = ".xml";
        private const string version = "1.0";
        private const string encoding = "UTF-8";
        private const string indent = "    ";

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets .xml extension.
        /// </summary>
        static public string Extension
        {
            get { return extension; }
        }

        /// <summary>
        /// Gets XML version.
        /// </summary>
        static public string Version
        {
            get { return version; }
        }

        /// <summary>
        /// Gets encoding.
        /// </summary>
        static public string Encoding
        {
            get { return encoding; }
        }

        /// <summary>
        /// Gets XMLWriterSettings.
        /// </summary>
        static public XmlWriterSettings Settings
        {
            get
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = indent;
                return settings;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates XML document.
        /// </summary>
        /// <param name="schemaName">Name of schema.</param>
        /// <returns>XML document.</returns>
        static public XmlDocument CreateXmlDocument(string schemaName)
        {
            // Create XML document, set schema, and get declaration
            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration(version, encoding, string.Empty);

            // Create writer and start XML file
            MemoryStream stream = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(stream, Settings);
            writer.WriteProcessingInstruction(declaration.Name, declaration.Value);
            writer.WriteStartElement(schemaName);
            writer.Close();

            // Load document from stream
            stream.Seek(0, SeekOrigin.Begin);
            document.Load(stream);

            return document;
        }

        /// <summary>
        /// Creates XML document to a file.
        /// </summary>
        /// <param name="schemaName">Name of schema.</param>
        /// <param name="filename">Path to output file.</param>
        /// <returns>XML document.</returns>
        static public XmlDocument CreateXmlDocument(string schemaName, string filename)
        {
            // Ensure ".xml" is appended to document
            if (!filename.EndsWith(extension))
                filename += extension;

            // Create XML document, set schema, and get declaration
            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration(version, encoding, string.Empty);

            // Create writer and start XML file
            XmlWriter writer = XmlWriter.Create(filename, Settings);
            writer.WriteProcessingInstruction(declaration.Name, declaration.Value);
            writer.WriteStartElement(schemaName);
            writer.Close();

            // Load created document
            document.Load(filename);

            return document;
        }

        /// <summary>
        /// Appends an element to XML node.
        /// </summary>
        /// <param name="document">Document object.</param>
        /// <param name="parentNode">Parent node for append.</param>
        /// <param name="elementName">Name of new element.</param>
        /// <param name="elementInnerText">Inner text of new element.</param>
        /// <returns></returns>
        static public XmlElement AppendElement(XmlDocument document, XmlNode parentNode, string elementName, string elementInnerText)
        {
            // If ParentNode isn't set then get root node
            if (null == parentNode)
                parentNode = document.DocumentElement;

            // If ParentNode still isn't set then exit
            if (null == parentNode)
                return null;

            // Create child element and return new node
            XmlElement element = document.CreateElement(elementName);
            element.InnerText = elementInnerText;
            XmlNode node = parentNode.AppendChild(element);
            return element;
        }

        /// <summary>
        /// Saves an XML document to a memory stream.
        /// </summary>
        /// <param name="document">Input document.</param>
        /// <returns>Memory stream.</returns>
        static public MemoryStream ToMemoryStream(XmlDocument document)
        {
            // Save to stream
            MemoryStream stream = new MemoryStream();
            document.Save(stream);

            // Seek back to start of stream
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        #endregion

    }

}
