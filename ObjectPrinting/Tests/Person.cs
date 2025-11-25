using System;

namespace ObjectPrinting.Tests
{
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public double Height { get; set; }
        public Phone Phone { get; set; }
        public int Age;
        private string SecretCode;
        private string _passportName;

        public string PassportName
        {
            get => _passportName;
            set => _passportName = value;
        }
        
        public void SetSecretCode(string code) => this.SecretCode = code;
    }

    public class Phone
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Person Owner { get; set; }
    }
}