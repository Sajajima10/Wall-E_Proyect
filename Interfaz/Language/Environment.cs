using System.Collections.Generic;

namespace Interfaz.Language
{
    public class Environment
    {
        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();
        private readonly Environment _parent; // Para anidar entornos (útil en el futuro para funciones/scopes)

        public Environment(Environment parent = null)
        {
            _parent = parent;
        }

        // Define o actualiza una variable en el entorno actual
        public void Set(string name, object value)
        {
            _variables[name] = value;
        }

        // Obtiene el valor de una variable, buscándola en los entornos padre si no está en el actual
        public object Get(string name)
        {
            if (_variables.TryGetValue(name, out object value))
            {
                return value;
            }

            if (_parent != null)
            {
                return _parent.Get(name); // Buscar en el entorno padre
            }

            throw new KeyNotFoundException($"La variable '{name}' no ha sido declarada.");
        }

        // Verifica si una variable existe en el entorno actual o en cualquier padre
        public bool Has(string name)
        {
            if (_variables.ContainsKey(name))
            {
                return true;
            }
            if (_parent != null)
            {
                return _parent.Has(name);
            }
            return false;
        }
    }
}