using System; 

namespace RG.Basic
{
    public class JsonReader
    {
        private Json dynamicData = null;
        private Json defaultData = null;

        #region Get and Set

        public object this[string key]
        {
            get
            {
                object ret = null;
                if(!dynamicData.Data.ContainsKey(key))
                {
                    ret = defaultData.GetByKey(key);
                }
                else ret = dynamicData.Data[key];
                if (ret is Json)
                    throw new Exception("JsonReader 禁止用[]取用Json Child");
                return ret;
            }
            set
            {
                if(dynamicData.Data.ContainsKey(key))
                {
                    dynamicData.Data[key] = value;
                }
                else
                {
                    dynamicData.Add(key, value);
                }
            }
        }

        public JsonReader Add(string name,object value)
        {
            if(defaultData.HasChild(name))
                throw new JsonException("[JsonReader Add] the Name (" + name + ") is already exist. ");
            dynamicData.Add(value);
            return this;
        }

        #endregion

        #region Create

        protected JsonReader()
        {
            dynamicData = Json.Create();
            defaultData = Json.Create();
        }

        protected JsonReader(Json _defaultData)
        {
            dynamicData = Json.Create();
            defaultData = _defaultData;
        }

        internal static JsonReader Create(Json _defaultData) { return new JsonReader(_defaultData); }

        internal static JsonReader Create(string code) { return Create(Json.Create(code)); }

        #endregion


        public bool HasKey(string key)
        {
            return defaultData.HasChild(key) || dynamicData.HasChild(key);
        }

        #region Bool Int Float

        public bool Bool(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Bool(key);
            return defaultData.Bool(key);
        }
        public int Int(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Int(key);
            return defaultData.Int(key);
        }
        public byte Byte(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Byte(key);
            return defaultData.Byte(key);
        }
        public long Long(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Long(key);
            return defaultData.Long(key);
        }
        public float Float(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Float(key);
            return defaultData.Float(key);
        }
        public double Double(string key)
        {
            if (dynamicData.HasChild(key)) return dynamicData.Double(key);
            return defaultData.Double(key);
        }
        public string String(string key, string format = null)
        {
            if (dynamicData.HasChild(key)) return dynamicData.String(key,format);
            return defaultData.String(key,format);
        }
        #endregion

        public JsonReader Child(string key)
        {
            return Create(defaultData.Child(key));
        }

    }

    public static class JsonReaderManager
    {
        public static Map<string, Json> jsonPool;

        public static JsonReader CreateReader(string path)
        {
            if (jsonPool == null) jsonPool = new Map<string, Json>();
            Json d = null;
            if (jsonPool.ContainsKey(path)) d = jsonPool[path];
            else
            {
                string data = Files.ReadStringIfFileExist(path);
                if(data.Exist())
                {
                    Json j = Json.Create(data);
                    jsonPool.Add(path, j);
                }
            }
            return JsonReader.Create(d);
        }
    }
}