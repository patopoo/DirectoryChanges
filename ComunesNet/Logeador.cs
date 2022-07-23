using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comunes
{
    public class Logeador
    {
        LogApp oLog;

        public Logeador(String sLogPathXML)
        {
            oLog = new LogApp(sLogPathXML);
        }

        public void Escribe(String sMsg)
        {
            oLog.Escribe(sMsg);
        }

        public void ErrorLog(string cadena)
        {
            oLog.ErrorLog(cadena);
        }
    }
}
