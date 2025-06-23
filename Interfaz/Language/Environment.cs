using System.Collections.Generic;

namespace Interfaz.Language
{
    /// <summary>
    /// Gestiona el entorno de ejecución, almacenando variables y sus valores.
    /// Admite ámbitos anidados (lexical scoping) a través de una referencia a un entorno "padre".
    /// </summary>
    public class Environment
    {
        // Un diccionario para almacenar las variables (nombre, valor) del ámbito actual.
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();
        
        // Una referencia al entorno padre, que permite buscar variables en ámbitos superiores.
        private readonly Environment _parent; 

        /// <summary>
        /// Crea un nuevo entorno.
        /// </summary>
        /// <param name="parent">El entorno padre, para crear un ámbito anidado. Es nulo para el entorno global.</param>
        public Environment(Environment parent = null)
        {
            _parent = parent;
        }

        /// <summary>
        /// Define o reasigna una variable en el ámbito actual.
        /// </summary>
        /// <param name="name">El nombre de la variable.</param>
        /// <param name="value">El valor a asignar.</param>
        public void Set(string name, object value)
        {
            _variables[name] = value;
        }

        /// <summary>
        /// Obtiene el valor de una variable. Si no se encuentra en el ámbito actual,
        /// busca recursivamente en los entornos padre.
        /// </summary>
        /// <param name="name">El nombre de la variable a buscar.</param>
        /// <returns>El valor de la variable.</returns>
        /// <exception cref="KeyNotFoundException">Se lanza si la variable no se encuentra en ningún ámbito.</exception>
        public object Get(string name)
        {
            // Intenta obtener la variable en el ámbito actual.
            if (_variables.TryGetValue(name, out object value))
            {
                return value;
            }

            // Si no se encuentra y hay un entorno padre, busca en él.
            if (_parent != null)
            {
                return _parent.Get(name); 
            }

            // Si no se encuentra en ninguna parte, lanza una excepción.
            throw new KeyNotFoundException($"La variable '{name}' no ha sido declarada.");
        }

        /// <summary>
        /// Comprueba si una variable ha sido declarada en el ámbito actual o en alguno de sus padres.
        /// </summary>
        /// <param name="name">El nombre de la variable.</param>
        /// <returns>Verdadero si la variable existe, falso en caso contrario.</returns>
        public bool Has(string name)
        {
            // Comprueba el ámbito actual.
            if (_variables.ContainsKey(name))
            {
                return true;
            }
            // Si no, comprueba recursivamente en el padre.
            if (_parent != null)
            {
                return _parent.Has(name);
            }
            return false;
        }
    }
}