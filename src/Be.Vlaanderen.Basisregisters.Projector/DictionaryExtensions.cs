namespace Be.Vlaanderen.Basisregisters.Projector
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;

    public static class DictionaryExtensions
    {
        public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject();
            var expandoDictionary = (IDictionary<string, object>)expando;

            // go through the items in the dictionary and copy over the key value pairs)
            foreach (var kvp in dictionary)
            {
                switch (kvp.Value)
                {
                    // if the value can also be turned into an ExpandoObject, then do it!
                    case IDictionary<string, object> objectDictionary:
                    {
                        var expandoValue = objectDictionary.ToExpando();
                        expandoDictionary.Add(kvp.Key, expandoValue);
                        break;
                    }

                    case ICollection value:
                    {
                        // iterate through the collection and convert any string-object dictionaries
                        // along the way into expando objects
                        var itemList = new List<object>();
                        foreach (var item in value)
                        {
                            if (item is IDictionary<string, object> objectDictionary)
                            {
                                var expandoItem = objectDictionary.ToExpando();
                                itemList.Add(expandoItem);
                            }
                            else
                            {
                                itemList.Add(item);
                            }
                        }

                        expandoDictionary.Add(kvp.Key, itemList);
                        break;
                    }

                    default:
                        expandoDictionary.Add(kvp);
                        break;
                }
            }

            return expando;
        }
    }
}
