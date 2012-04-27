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
#endregion

namespace ProjectEditor.Classes
{

    /// <summary>
    /// Helper class to create XML documents and append elements.
    /// </summary>
    class XmlHelper
    {

        #region Public Methods

        /// <summary>
        /// Creates XML document to file.
        /// </summary>
        /// <param name="SchemaName">Name of schema.</param>
        /// <param name="FileName">Path to output file.</param>
        /// <returns></returns>
        public XmlDocument CreateXmlDocument(string SchemaName, string FileName)
        {
            // Ensure ".xml" is appended to document
            if (!FileName.EndsWith(".xml"))
                FileName += ".xml";

            // Create XML document, set schema, and get declaration
            XmlDocument document = new XmlDocument();
            XmlDeclaration declaration = document.CreateXmlDeclaration("1.0", "UTF-8", string.Empty);

            // Create XML writer and start file
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            XmlWriter writer = XmlWriter.Create(FileName, settings);
            writer.WriteProcessingInstruction(declaration.Name, declaration.Value);
            writer.WriteStartElement(SchemaName);
            writer.Close();

            // Load new document
            document.Load(FileName);

            return document;
        }

        /// <summary>
        /// Appends an element to XML node.
        /// </summary>
        /// <param name="Document">Document object.</param>
        /// <param name="ParentNode">Parent node for append.</param>
        /// <param name="ElementName">Name of new element.</param>
        /// <param name="ElementInnerText">Inner text of new element.</param>
        /// <returns></returns>
        public XmlElement AppendElement(XmlDocument Document, XmlNode ParentNode, string ElementName, string ElementInnerText)
        {
            // If ParentNode isn't set then get root node
            if (null == ParentNode)
                ParentNode = Document.DocumentElement;

            // If ParentNode still isn't set then exit
            if (null == ParentNode)
                return null;

            // Create child element and return new node
            XmlElement element = Document.CreateElement(ElementName);
            element.InnerText = ElementInnerText;
            XmlNode node = ParentNode.AppendChild(element);
            return element;
        }

        #endregion

    }

}
