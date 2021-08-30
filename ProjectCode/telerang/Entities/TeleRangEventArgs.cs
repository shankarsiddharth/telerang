using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace telerang.Entities
{
    public class TeleRangEventArgs : EventArgs
    {
        public Vector2 position { get; set; }
    }
}
