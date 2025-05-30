/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.Linq;

namespace Oculus.Interaction.DebugTree
{
    /// <summary>
    /// Interface indicating the inheriting type can be represented as a node in a tree-like structure for debugging
    /// and other development purposes.
    /// </summary>
    /// <remarks>
    /// This type is explicitly intended for use alongside <see cref="DebugTree{TLeaf}"/>; for a canonical usage example,
    /// see <see cref="PoseDetection.Debug.ActiveStateDebugTree"/>.
    /// </remarks>
    /// <typeparam name="TLeaf"></typeparam>
    public interface ITreeNode<TLeaf>
        where TLeaf : class
    {
        /// <summary>
        /// The value "contained" in this node, which the containing <see cref="DebugTree{TLeaf}"/> makes available for
        /// debugging and development purposes.
        /// </summary>
        /// <remarks>
        /// For example, for <see cref="PoseDetection.Debug.ActiveStateDebugTree"/>, this value is an individual
        /// <see cref="IActiveState"/>.
        /// </remarks>
        TLeaf Value { get; }

        /// <summary>
        /// The children of this node in the <see cref="DebugTree{TLeaf}"/>.
        /// </summary>
        IEnumerable<ITreeNode<TLeaf>> Children { get; }
    }

    public abstract class DebugTree<TLeaf>
        where TLeaf : class
    {
        private class Node : ITreeNode<TLeaf>
        {
            TLeaf ITreeNode<TLeaf>.Value => Value;
            IEnumerable<ITreeNode<TLeaf>> ITreeNode<TLeaf>.Children => Children;

            public TLeaf Value { get; set; }
            public List<Node> Children { get; set; }
        }

        private Dictionary<TLeaf, Node> _existingNodes =
            new Dictionary<TLeaf, Node>();

        private readonly TLeaf Root;
        private Node _rootNode;

        public DebugTree(TLeaf root)
        {
            Root = root;
        }

        public ITreeNode<TLeaf> GetRootNode()
        {
            if (_rootNode == null)
            {
                _rootNode = BuildTree(Root);
            }
            return _rootNode;
        }

        public void Rebuild()
        {
            _rootNode = BuildTree(Root);
        }

        private Node BuildTree(TLeaf root)
        {
            _existingNodes.Clear();
            return BuildTreeRecursive(root);
        }

        private Node BuildTreeRecursive(TLeaf value)
        {
            if (value == null)
            {
                return null;
            }

            if (_existingNodes.ContainsKey(value))
            {
                return _existingNodes[value];
            }

            List<Node> children = new List<Node>();

            if (TryGetChildren(value, out IEnumerable<TLeaf> c))
            {
                children.AddRange(c
                    .Select((child) => BuildTreeRecursive(child))
                    .Where((child) => child != null));
            }

            Node self = new Node()
            {
                Value = value,
                Children = children,
            };

            _existingNodes.Add(value, self);
            return self;
        }

        protected abstract bool TryGetChildren(TLeaf node, out IEnumerable<TLeaf> children);
    }
}
