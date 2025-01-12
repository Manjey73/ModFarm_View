// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Scada.Server.Modules.ModFarm.Config
{
    /// <summary>
    /// Represents a module configuration.
    /// <para>Представляет конфигурацию модуля.</para>
    /// </summary>
    [Serializable]
    internal class ModuleConfig // : ModuleConfigBase
    {
        /// <summary>
        /// The default configuration file name.
        /// </summary>
        public const string DefaultFileName = "ModFarm.xml";




        /// <summary>
        /// Loads the configuration from the specified reader.
        /// </summary>
        //protected override void Load(TextReader reader)
        //{
        //    XmlDocument xmlDoc = new();
        //    xmlDoc.Load(reader);

        //}

        /// <summary>
        /// Saves the configuration to the specified writer.
        /// </summary>
        //protected override void Save(TextWriter writer)
        //{
        //    XmlDocument xmlDoc = new();
        //    XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
        //    xmlDoc.AppendChild(xmlDecl);

        //    XmlElement rootElem = xmlDoc.CreateElement("ModUjin");
        //    xmlDoc.AppendChild(rootElem);


        //    xmlDoc.Save(writer);
        //}
    }
}
