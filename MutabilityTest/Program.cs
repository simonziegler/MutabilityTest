using System;
using Newtonsoft.Json;
using MessagePack;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MutabilityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize Person
            var person1 = new Person()
            {
                Name = "Bill",
                Age = 32
            };
            Console.WriteLine("Initial object: " + person1);

            // Mutate and JSON serialize/deserialize Person while mutable
            Console.WriteLine();
            Console.WriteLine("Mutate and JSON serialize/deserialize Person while mutable");
            Console.WriteLine("----------------------------------------------------------");

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

            // Mutate and MessagePack serialize/deserialize Person while mutable
            Console.WriteLine();
            Console.WriteLine("Mutate and MessagePack serialize/deserialize Person while mutable");
            Console.WriteLine("-----------------------------------------------------------------");
            var msg = MessagePackSerializer.Serialize(person1);
            var person3 = MessagePackSerializer.Deserialize<Person>(msg);
            Console.WriteLine("Deserialized mutable: " + person3);

            try
            {
                person3.Name = "Ted";
                Console.WriteLine("Name change on deserialized mutable: " + person3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Freeze and JSON serialize/deserialize, then attempt (and fail) mutation
            Console.WriteLine();
            Console.WriteLine("Freeze and JSON serialize/deserialize, then attempt (and fail) mutation");
            Console.WriteLine("-----------------------------------------------------------------------");

            person1.Freeze();
            Console.WriteLine("Post freeze: " + person1);

            jsonString = JsonConvert.SerializeObject(person1);
            Console.WriteLine("Immutable JSON: " + jsonString);

            var person4 = JsonConvert.DeserializeObject<Person>(jsonString);
            Console.WriteLine("Deserialized immutable: " + person4);

            try
            {
                person4.Name = "Ted";
                Console.WriteLine("Name change on deserialized immutable: " + person4);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // Freeze and JSON serialize/deserialize, then attempt (and fail) mutation
            Console.WriteLine();
            Console.WriteLine("Freeze and MessagePack serialize/deserialize, then attempt (and fail) mutation");
            Console.WriteLine("------------------------------------------------------------------------------");

            Console.WriteLine("Pre serialization: " + person1);
            msg = MessagePackSerializer.Serialize(person1);
            var person5 = MessagePackSerializer.Deserialize<Person>(msg);
            Console.WriteLine("Deserialized immutable: " + person5);

            try
            {
                person5.Name = "Ted";
                Console.WriteLine("Name change on deserialized immutable: " + person5);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }


    [MessagePackObject(keyAsPropertyName: true)]
    public abstract class Popsicle
    {
        private readonly bool performFreezeTest = false;

        private bool frozen = false;
        [JsonProperty]
        public bool Frozen { get => frozen; set => frozen = frozen ? frozen : value; }

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

        public override string ToString()
        {
            return Frozen ? "Immutable" : "Mutable";
        }
    }


    [MessagePackObject(keyAsPropertyName:true)]
    public class Person : Popsicle
    {
        private string name;
        public string Name { get => name; set => Setter(() => value == name, () => name = value); }
        
        
        private double age;
        public double Age { get => age; set => Setter(() => value == age, () => age = value); }

        public override string ToString()
        {
            return $"{Name}, {Age}, {base.ToString()}";
        }
    }
}