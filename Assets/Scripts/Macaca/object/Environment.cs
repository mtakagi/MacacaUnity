using System.Collections.Generic;

namespace Macaca
{
    public class Environment
    {
        private Dictionary<string, Object> store = new Dictionary<string, Object>();
        private Environment outer;

        public Environment() { }

        public Environment(Environment outer)
        {
            this.outer = outer;
        }

        public Object this[string name]
        {
            get
            {
                return this.store[name];
            }
            set
            {
                this.store[name] = value;
            }
        }

        public bool TryGetValue(string name, out Object obj)
        {
            var result = this.store.TryGetValue(name, out obj);

            if (!result && this.outer != null)
            {
                return this.outer.TryGetValue(name, out obj);
            }

            return result;
        }
    }
}