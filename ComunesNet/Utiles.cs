using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Comunes
{
    public class Utiles
    {
        public enum ordenRelleno
        {
            a_la_derecha,
            a_la_izquierda
        }

        public Utiles()
        {
        }

        public string GetFilenameYYYMMDD(string sLogFile)
        {
            return Path.GetDirectoryName(sLogFile) + "\\" + Path.GetFileNameWithoutExtension(sLogFile) + "_" + System.DateTime.Now.ToString("yyyyMMdd") + Path.GetExtension(sLogFile);
        }

        #region strLen
        public String strLen(string input, int length, char relleno, ordenRelleno orden)
        {
            input = input ?? string.Empty;
            input = input.Length > length ? input.Substring(0, length) : input;
            //return string.Format("{0,-" + length + "}", input);
            if (orden == ordenRelleno.a_la_derecha)
                input = input.PadRight(length, relleno);
            else
                input = input.PadLeft(length, relleno);

            return input;
        }
        public String strLen(string input, int length)
        {
            return strLen(input, length, ' ', ordenRelleno.a_la_derecha);
        }
        public String strLen(int input, int length)
        {
            return strLen(input.ToString(), length, '0', ordenRelleno.a_la_izquierda);
        }
        #endregion

        public string Left(string s, int length)
        {
            if (string.IsNullOrEmpty(s) || length < 1)
                return string.Empty;
            else
                return s.Substring(0, Math.Min(length, s.Length));            
        }

        public bool revisaExistDir(string directorio)
        {
            bool retu = true;
            //si no existen se crean los directorios
            try
            {
                if (!Directory.Exists(directorio))
                {
                    Console.WriteLine("Creando directorio: " + directorio);
                    Directory.CreateDirectory(directorio);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear directorio: " + ex.ToString());
                retu = false;
            }
            return retu;
        }

    }
}
