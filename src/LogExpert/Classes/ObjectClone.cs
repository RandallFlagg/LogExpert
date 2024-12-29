using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace LogExpert.Classes
{
    /*
     * By - Rahul Dantkale
     * Company - Indigo Architects
     * 
     */
    public static class ObjectClone
    {
        #region Public methods

        public static T Clone<T>(T RealObject)
        {
            using (Stream objectStream = new MemoryStream())
            {
                //FIXME: OBSOLETE
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonString = JsonSerializer.Serialize(RealObject, options);
                var deserializedObject = JsonSerializer.Deserialize<T>(jsonString);
                return deserializedObject;
                //IFormatter formatter = new BinaryFormatter();
                //formatter.Serialize(objectStream, RealObject);
                //objectStream.Seek(0, SeekOrigin.Begin);
                //return (T) formatter.Deserialize(objectStream);
            }
        }

        #endregion
    }
}