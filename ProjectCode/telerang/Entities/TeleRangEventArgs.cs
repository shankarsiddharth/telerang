using Microsoft.Xna.Framework;
using System;

namespace telerang.Entities
{
    public class TeleRangEventArgs : EventArgs
    {
        public Vector2 position { get; set; }
    }
}