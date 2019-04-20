﻿using System;
using SSBHLib;
using SSBHLib.Formats.Materials;
using System.Xml.Serialization;
using System.IO;

namespace MatLab
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
                return;

            string matlPath = args[1];

            string code = args[0];

            ISSBH_File file;
            SSBH.TryParseSSBHFile(matlPath, out file);

            MATL matlFile = (MATL)file;
            SSBH.TrySaveSSBHFile("original.numatb", matlFile);

            material_library library = MATLtoLibrary(matlFile);

            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(library.GetType());
            using (TextWriter writer = new StringWriter())
            {
                x.Serialize(writer, library);
                string serial = writer.ToString();

                using (TextReader reader = new StringReader(serial))
                {
                    var result = (material_library)x.Deserialize(reader);

                    MATL newmatl = LibraryToMATL(result);

                    SSBH.TrySaveSSBHFile("test.numatb", newmatl);

                    Console.WriteLine(result.material.Length);
                }
            }
            Console.WriteLine("Saved");
            Console.ReadLine();
        }

        public static MATL LibraryToMATL(material_library library)
        {
            MATL matl = new MATL();

            matl.Entries = new MatlEntry[library.material.Length];

            for(int i = 0; i < library.material.Length; i++)
            {
                MatlEntry entry = new MatlEntry();

                entry.MaterialLabel = library.material[i].label;
                entry.MaterialName = library.material[i].name;
                entry.Attributes = new MatlAttribute[library.material[i].param.Length];

                for (int j = 0; j < library.material[i].param.Length; j++)
                {
                    entry.Attributes[j] = new MatlAttribute();

                    entry.Attributes[j].ParamID = library.material[i].param[j].name;

                    entry.Attributes[j].DataObject = library.material[i].param[j].Value;
                }

                matl.Entries[i] = entry;
            }

            return matl;
        }

        public static material_library MATLtoLibrary(MATL matlFile)
        {
            material_library library = new material_library();
            library.material = new material[matlFile.Entries.Length];

            int entryIndex = 0;
            foreach (var entry in matlFile.Entries)
            {
                material mat = new material();
                mat.name = entry.MaterialName;
                mat.label = entry.MaterialLabel;

                mat.param = new attribute[entry.Attributes.Length];

                int attribIndex = 0;
                foreach (var attr in entry.Attributes)
                {
                    attribute attrib = new attribute();
                    attrib.name = attr.ParamID;
                    attrib.Value = attr.DataObject;
                    mat.param[attribIndex++] = attrib;
                }

                library.material[entryIndex] = mat;
                entryIndex++;
            }

            return library;
        }

        public class material_library
        {
            [XmlElement]
            public material[] material;
        }

        public class material
        {
            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string label;

            [XmlElement]
            public attribute[] param;
        }

        [XmlInclude(typeof(MatlAttribute.MatlBlendState)), XmlInclude(typeof(MatlAttribute.MatlRasterizerState))
            , XmlInclude(typeof(MatlAttribute.MatlVector4)), XmlInclude(typeof(MatlAttribute.MatlSampler))
            , XmlInclude(typeof(MatlAttribute.MatlString)), XmlInclude(typeof(MatlAttribute.MatlUVTransform))]
        public class attribute
        {
            [XmlAttribute]
            public MatlEnums.ParamId name;

            [XmlElement("blend_state", typeof(MatlAttribute.MatlBlendState))]
            [XmlElement("rasterizer_state", typeof(MatlAttribute.MatlRasterizerState))]
            [XmlElement("vector4", typeof(MatlAttribute.MatlVector4))]
            [XmlElement("sampler", typeof(MatlAttribute.MatlSampler))]
            [XmlElement("string", typeof(MatlAttribute.MatlString))]
            [XmlElement("UVtransform", typeof(MatlAttribute.MatlUVTransform))]
            [XmlElement("float", typeof(float))]
            [XmlElement("bool", typeof(bool))]
            public object Value;
        }
    }
}
