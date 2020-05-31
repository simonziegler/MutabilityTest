using System;
using Newtonsoft.Json;

namespace MutabilityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var person1 = new Person()
            {
                Name = "Bill",
                Age = 32
            };
            Console.WriteLine("Initial object: " + person1);

            person1.Age = 42;
            Console.WriteLine("Age changed: " + person1);

            var jsonString = JsonConvert.SerializeObject(person1);
            Console.WriteLine("Mutable JSON: " + jsonString);

            var person2 = JsonConvert.DeserializeObject<Person>(jsonString);
            Console.WriteLine("Deserialized mutable: " + person2);

            try
            {
                person2.Name = "Ted";
                Console.WriteLine("Name change on deserialized mutable: " + person2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            person1.Freeze();
            Console.WriteLine("Post freeze: " + person1);

            jsonString = JsonConvert.SerializeObject(person1);
            Console.WriteLine("Immutable JSON: " + jsonString);

            var person3 = JsonConvert.DeserializeObject<Person>(jsonString);
            Console.WriteLine("Deserialized immutable: " + person3);

            try
            {
                person3.Name = "Ted";
                Console.WriteLine("Name change on deserialized immutable: " + person3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }


    public abstract class Popsicle
    {
        private readonly bool performFreezeTest = false;
        
        [JsonProperty]
        private bool Frozen { get; set; } = false;

        public void Freeze() => Frozen = true;

        protected void Setter<T>(Func<bool> equalityTest, Func<T> setter)
        {
            if (performFreezeTest && Frozen) throw new InvalidOperationException("Cannot set a property on a frozen instance");

            if (!equalityTest())
            {
                setter();
                // Do some more stuff to notify observers of a change.
            }
        }

        public Popsicle()
        {
            performFreezeTest = true;
        }
    }


    public class Person : Popsicle
    {
        private string name;
        public string Name { get => name; set => Setter(() => value == name, () => name = value); }
        
        
        private double age;
        public double Age { get => age; set => Setter(() => value == age, () => age = value); }

        public override string ToString()
        {
            return $"{Name}, {Age}";
        }
    }
}