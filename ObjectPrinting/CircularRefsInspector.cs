using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class CircularRefsInspector
    {
        private static readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };
        private readonly HashSet<object> alreadyHandledObjects;
        private readonly HashSet<object> circularlyReferencedObjects;
        private readonly HashSet<Type> typesToSkip;
        private readonly HashSet<PropertyInfo> propertiesToSkip;
        private readonly object objectToInspect;
        

        public CircularRefsInspector(object objectToInspect, HashSet<Type> typesToSkip, HashSet<PropertyInfo> propertiesToSkip)
        {
            alreadyHandledObjects = new HashSet<object>();
            circularlyReferencedObjects = new HashSet<object>();
            this.objectToInspect = objectToInspect;
            this.typesToSkip = typesToSkip;
            this.propertiesToSkip = propertiesToSkip;
        }

        public HashSet<object> GetCircularlyReferencedObjects()
        {
            Inspect(objectToInspect);
            return circularlyReferencedObjects;
        }

        private void Inspect(object obj)
        {
            if (obj == null || FinalTypes.Contains(obj.GetType()))
                return;

            var type = obj.GetType();

            if (typesToSkip.Contains(type))
                return;

            if (alreadyHandledObjects.Contains(obj))
            {
                circularlyReferencedObjects.Add(obj);
                return;
            }

            alreadyHandledObjects.Add(obj);

            if (Implements(type, typeof(IEnumerable)))
            {
                InspectIEnumerable(obj);
                return;
            }

            InspectComplexType(obj, type);
        }

        private bool Implements(Type type, Type interfaceName)
        {
            return type.GetInterfaces().Any(t => t == interfaceName);
        }

        private void InspectIEnumerable(object obj)
        {
            foreach (var element in obj as IEnumerable)
                Inspect(element);
        }

        private void InspectComplexType(object obj, Type type)
        {
            foreach (var propertyInfo in type.GetProperties())
                if (!propertiesToSkip.Contains(propertyInfo))
                    Inspect(propertyInfo.GetValue(obj));
        }
    }
}
