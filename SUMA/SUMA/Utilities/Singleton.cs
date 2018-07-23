using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUMA
{    
    public class Singleton<T> where T : class, new()
    {
        private static Lazy<T> instance = null;

        /// <summary>
        /// Retorna una instancia creada automaticament, de la classe de la cual deriva
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new Lazy<T>(() => new T());

                return instance.Value;
            }
        }
    }
}
