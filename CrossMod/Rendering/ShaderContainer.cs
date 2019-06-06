﻿using OpenTK.Graphics.OpenGL;
using SFGraphics.GLObjects.Shaders;
using System.Collections.Generic;
using System.IO;

namespace CrossMod.Rendering
{
    /// <summary>
    /// Stores all <see cref="Shader"/> instances used for rendering.
    /// </summary>
    public static class ShaderContainer
    {
        private static SFShaderLoader.ShaderLoader shaderLoader = new SFShaderLoader.ShaderLoader();

        public static Shader GetShader(string name)
        {
            return shaderLoader.GetShader(name);
        }

        public static void SetUpShaders()
        {
            CreateBinaryDirectory();

            RemoveExistingBinaries();

            // TODO: These methods can share most of their code.
            CreateRModelShader();
            CreateRModelUvShader();
            CreateTextureShader();

            // TODO: This shader can be generated by SFGraphics.
            CreateRModelDebugShader();
        }

        private static void RemoveExistingBinaries()
        {
            foreach (var file in Directory.GetFiles("Shaders/Binary", "*.bin"))
            {
                File.Delete(file);
            }
        }

        private static void CreateBinaryDirectory()
        {
            if (!Directory.Exists("Shaders/Binary"))
                Directory.CreateDirectory("Shaders/Binary");
        }

        private static void CreateTextureShader()
        {
            // Compile the shaders at runtime if the binary fails.
            if (TryLoadFromBinary("RTexture"))
                return;

            shaderLoader.AddShader("RTexture",
                new List<string>() { File.ReadAllText("Shaders/Texture.vert") },
                new List<string>()
                {
                    File.ReadAllText("Shaders/Texture.frag"),
                    File.ReadAllText("Shaders/Gamma.frag")
                },
                new List<string>()
            );
            SaveProgramBinary("RTexture");
        }

        private static void CreateRModelDebugShader()
        {
            // Compile the shaders at runtime if the binary fails.
            if (TryLoadFromBinary("RModelDebug"))
                return;

            shaderLoader.AddShader("RModelDebug",
                new List<string>() { File.ReadAllText("Shaders/RModel.vert") },
                new List<string>()
                {
                    File.ReadAllText("Shaders/RModelDebug.frag"),
                    File.ReadAllText("Shaders/NormalMap.frag"),
                    File.ReadAllText("Shaders/Gamma.frag"),
                    File.ReadAllText("Shaders/WireFrame.frag"),
                    File.ReadAllText("Shaders/TextureLayers.frag")
                },
                new List<string>() { File.ReadAllText("Shaders/RModel.geom") }
            );
            SaveProgramBinary("RModelDebug");
        }

        private static void CreateRModelUvShader()
        {
            // Compile the shaders at runtime if the binary fails.
            if (TryLoadFromBinary("RModelUV"))
                return;

            shaderLoader.AddShader("RModelUV",
                new List<string>() { File.ReadAllText("Shaders/RModelUV.vert") },
                new List<string>()
                {
                    File.ReadAllText("Shaders/RModelUV.frag"),
                    File.ReadAllText("Shaders/NormalMap.frag"),
                    File.ReadAllText("Shaders/Gamma.frag"),
                    File.ReadAllText("Shaders/Wireframe.frag"),
                },
                new List<string>() { File.ReadAllText("Shaders/RModel.geom") }
            );
            SaveProgramBinary("RModelUV");
        }

        private static void CreateRModelShader()
        {
            // Compile the shaders at runtime if the binary fails.
            if (TryLoadFromBinary("RModel"))
                return;

            shaderLoader.AddShader("RModel", 
                new List<string>() { File.ReadAllText("Shaders/RModel.vert") }, 
                new List<string>()
                {
                    File.ReadAllText("Shaders/RModel.frag"),
                    File.ReadAllText("Shaders/NormalMap.frag"),
                    File.ReadAllText("Shaders/Gamma.frag"),
                    File.ReadAllText("Shaders/WireFrame.frag"),
                    File.ReadAllText("Shaders/TextureLayers.frag")

                }, 
                new List<string>() { File.ReadAllText("Shaders/RModel.geom") }
            );
            SaveProgramBinary("RModel");
        }

        private static bool TryLoadFromBinary(string shaderName)
        {
            var formatPath = $"Shaders/Binary/{shaderName}.format.bin";
            var binaryPath = $"Shaders/Binary/{shaderName}.program.bin";

            if (File.Exists(formatPath) && File.Exists(binaryPath))
            {
                var binary = File.ReadAllBytes(binaryPath);
                var format = ReadBinaryFormat(formatPath);

                if (shaderLoader.AddShader(shaderName, binary, format))
                    return true;
            }

            return false;
        }

        private static BinaryFormat ReadBinaryFormat(string formatPath)
        {
            using (var stream = new FileStream(formatPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return (BinaryFormat)reader.ReadInt32();
                }
            }
        }

        private static void SaveProgramBinary(string shaderName)
        {
            var formatPath = $"Shaders/Binary/{shaderName}.format.bin";
            var binaryPath = $"Shaders/Binary/{shaderName}.program.bin";

            if (shaderLoader.CreateProgramBinary(shaderName, out byte[] shaderBinary, out BinaryFormat binaryFormat))
            {
                File.WriteAllBytes(binaryPath, shaderBinary);
                WriteBinaryFormat(formatPath, binaryFormat);
            }
        }

        private static void WriteBinaryFormat(string formatPath, BinaryFormat binaryFormat)
        {
            using (var stream = File.Create(formatPath))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((int)binaryFormat);
                }
            }
        }
    }
}
