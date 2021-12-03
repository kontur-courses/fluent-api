using System;

namespace Homework
{
    public class OneTimeSetValue<TValue> : ICloneable
    {
        private TValue? value;
        
        public TValue Value 
        {
            get => value ?? throw new ArgumentNullException();
            set
            {
                if (Setted) throw new InvalidOperationException("value already setted");
                this.value = value;
                Setted = true;
            }
        }
        public bool Setted { get; private set; }

        public OneTimeSetValue(TValue defaultValue)
        {
            value = defaultValue;
        }
        
        private OneTimeSetValue()
        {
        }

        public object Clone()
        {
            return new OneTimeSetValue<TValue>
            {
                value = value,
                Setted = Setted
            };
        }
    }
}