using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TextRendererClass = AELDHKGBIFD;

namespace CheepsAmongUsApi.API
{
    public class TextRenderer
    {
        public TextRendererClass TextRendererObject { get; }

        public string Text
        {
            get
            {
                return TextRendererObject.Text;
            }

            set
            {
                TextRendererObject.Text = value;
            }
        }

        public Color Color
        {
            get
            {
                return TextRendererObject.Color;
            }

            set
            {
                TextRendererObject.Color = value;
            }
        }

        public Color OutlineColor
        {
            get
            {
                return TextRendererObject.OutlineColor;
            }

            set
            {
                TextRendererObject.OutlineColor = value;
            }
        }

        public Transform Transform
        {
            get
            {
                return TextRendererObject.transform;
            }
        }

        public Vector2 Position
        {
            get
            {
                return Transform.position;
            }

            set
            {
                Transform.position = value;
            }
        }

        public Vector2 Scale
        {
            get
            {
                return Transform.localScale;
            }

            set
            {
                Transform.localScale = value;
            }
        }

        /// <summary>
        /// Api Object for the among us text renderer object
        /// </summary>
        /// <param name="obj">Among Us Text Renderer Object</param>
        public TextRenderer(TextRendererClass obj)
        {
            TextRendererObject = obj;
        }

    }
}
