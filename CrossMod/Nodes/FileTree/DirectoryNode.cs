﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CrossMod.Nodes
{
    /// <summary>
    /// Class for all Directory entries in the file system. Executing Open()
    /// populates sub-nodes, and executing OpenNodes() calls Open() on all sub-nodes.
    /// </summary>
    public class DirectoryNode : FileNode
    {
        private bool isOpened = false;
        private bool hasOpenedFiles = false;
        private bool isNestedOpened = false;

        // Convert to list to avoid evaluating the LINQ more than once.
        private static readonly List<Type> fileNodeTypes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                                                            from assemblyType in domainAssembly.GetTypes()
                                                            where typeof(FileNode).IsAssignableFrom(assemblyType)
                                                            select assemblyType).ToList();

        private static readonly Dictionary<string, Type> typeByExtension = new Dictionary<string, Type>();

        /// <summary>
        /// Creates a new DirectoryNode. The FilePath is set to the given value
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isRoot">Whether this is the topmost parent. Decides whether to display full or partial name.</param>
        public DirectoryNode(string path, bool isRoot = true) : base(path)
        {
            Text = (isRoot) ? Path.GetFullPath(path) : Path.GetFileName(path);
            SelectedImageKey = "folder";
            ImageKey = "folder";
        }

        /// <summary>
        /// Reads the directory, populating all subnodes.
        /// Subnodes are not opened, use <see cref="OpenChildNodes"/> afterwards to do that.
        /// Repeated executions are no-ops.
        /// </summary>
        public override void Open()
        {
            if (isOpened)
            {
                return;
            }

            foreach (var name in Directory.EnumerateFileSystemEntries(AbsolutePath))
            {
                if (Directory.Exists(name))
                {
                    var dirNode = new DirectoryNode(name, isRoot: false);
                    Nodes.Add(dirNode);
                    dirNode.Open();
                }
                else
                {
                    Nodes.Add(CreateFileNode(fileNodeTypes, name));
                }
            }

            isOpened = true;
        }

        /// <summary>
        /// Opens all files in this directory.
        /// Repeated calls are ignored.
        /// </summary>
        public void OpenFileNodes()
        {
            if (hasOpenedFiles)
                return;

            // Some nodes take a while to open, so use a threadpool to save time.
            var openNodes = new List<Task>();
            foreach (var node in Nodes)
            {
                if (node is FileNode file)
                    openNodes.Add(Task.Run(() => file.Open()));
            }

            Task.WaitAll(openNodes.ToArray());
            hasOpenedFiles = true;
        }

        /// <summary>
        /// Internal helper to open a file entry.
        /// </summary>
        /// <param name="Types"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private FileNode CreateFileNode(IEnumerable<Type> Types, string file)
        {
            // TODO: Possible separation of concerns improvement: IFileLoader injected into DirectoryNode.

            FileNode fileNode = null;

            string extension = Path.GetExtension(file);

            var type = GetType(extension);
            if (type != null)
                fileNode = (FileNode)Activator.CreateInstance(type, file);

            if (fileNode == null)
                fileNode = new FileNode(file);

            // Change style of unrenderable nodes
            if (!(fileNode is IRenderableNode) && !(fileNode is IExportableAnimationNode))
            {
                fileNode.ForeColor = Color.Gray;
            }

            fileNode.Text = Path.GetFileName(file);
            return fileNode;
        }

        private static Type GetType(string extension)
        {
            // Cache results to avoid doing lots of lookups.
            // TODO: Hard code the common types into the dictionary initialization.
            if (typeByExtension.ContainsKey(extension))
            {
                return typeByExtension[extension];
            }
            else
            {
                var type = FindType(extension);
                typeByExtension[extension] = type;
                return type;
            }
        }

        private static Type FindType(string extension)
        {
            foreach (var type in fileNodeTypes)
            {
                if (type.GetCustomAttributes(typeof(FileTypeAttribute), true).FirstOrDefault() is FileTypeAttribute attr)
                {
                    if (attr.Extension.Equals(extension))
                    {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}
