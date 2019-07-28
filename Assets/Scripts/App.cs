using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class App
    {
        private static App _INST = null;

        private string path;

        App()
        {
            this.path = @"E:\wall\momoco haru";
        }

        public static App GetInstance()
        {
            if (_INST == null )
            {
                _INST = new App();
            }

            return _INST;
        }

        public void setPath( string path )
        {
            if (!String.IsNullOrEmpty(path))
            {
                this.path = path;
            }
        }

        public string getPath()
        {
            return this.path;
        }
    }
}
