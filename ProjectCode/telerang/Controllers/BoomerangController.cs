using System;
using System.Collections.Generic;
using System.Text;

using MonoGame.Extended;
using MonoGame.Extended.Input.InputListeners;

namespace telerang
{
    class BoomerangController
    {
        private readonly TouchListener _touchListener = new TouchListener();
        private readonly GamePadListener _gamePadListener = new GamePadListener();
        private readonly KeyboardListener _keyboardListener = new KeyboardListener();
        private readonly MouseListener _mouseListener = new MouseListener();

        public void Initialize()
        {
            //Components.Add(new InputListenerComponent(this, _keyboardListener, _gamePadListener, _mouseListener, _touchListener));
        }
    }
}
