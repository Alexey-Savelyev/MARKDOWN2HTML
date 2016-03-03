﻿using System;
using System.Collections.Generic;
using Textamina.Markdig.Syntax;
using Textamina.Markdig.Syntax.Inlines;

namespace Textamina.Markdig.Renderers
{
    public abstract class RendererBase : IMarkdownRenderer
    {
        private readonly Dictionary<Type, IMarkdownObjectRenderer> renderersPerType;
        private IMarkdownObjectRenderer previousRenderer;
        private Type previousObjectType;

        protected RendererBase()
        {
            ObjectRenderers = new ObjectRendererCollection();
            renderersPerType = new Dictionary<Type, IMarkdownObjectRenderer>();
        }

        public ObjectRendererCollection ObjectRenderers { get; }

        public abstract object Render(MarkdownObject markdownObject);

        public void WriteChildren(ContainerBlock containerBlock)
        {
            if (containerBlock == null)
            {
                return;
            }

            var children = containerBlock.Children;
            for (int i = 0; i < children.Count; i++)
            {
                Write(children[i]);
            }
        }

        public void WriteChildren(ContainerInline containerInline)
        {
            if (containerInline == null)
            {
                return;
            }

            var inline = containerInline.FirstChild;
            while (inline != null)
            {
                Write(inline);
                inline = inline.NextSibling;
            }
        }

        public void Write<T>(T obj) where T : MarkdownObject
        {
            if (obj == null)
            {
                return;
            }

            var objectType = obj.GetType();

            // Handle regular renderers
            IMarkdownObjectRenderer renderer = previousObjectType == objectType ? previousRenderer : null;
            if (renderer == null && !renderersPerType.TryGetValue(objectType, out renderer))
            {
                for (int i = 0; i < ObjectRenderers.Count; i++)
                {
                    var testRenderer = ObjectRenderers[i];
                    if (testRenderer.Accept(this, obj))
                    {
                        renderersPerType[objectType] = renderer = testRenderer;
                        break;
                    }
                }
            }
            if (renderer != null)
            {
                renderer.Write(this, obj);
            }
            else
            {
                var containerBlock = obj as ContainerBlock;
                if (containerBlock != null)
                {
                    WriteChildren(containerBlock);
                }
                else
                {
                    var containerInline = obj as ContainerInline;
                    if (containerInline != null)
                    {
                        WriteChildren(containerInline);
                    }
                }
            }

            previousObjectType = objectType;
            previousRenderer = renderer;
        }
    }
}