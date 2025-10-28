using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RFID_CONTROLLER.Collections {
	public class Data<TValue> : IDictionary<String, TValue> {
		private Dictionary<String, TValue> dict = null;

		public Data() {
			dict = new Dictionary<String, TValue>();
		}

		public Data(IDictionary<String, TValue> d) {
			dict = new Dictionary<String, TValue>();
			foreach (var i in d) {
				this[i.Key] = i.Value;
			}
		}

		public Data(string jsonString) {
			var setting = new JsonSerializerSettings();
			setting.NullValueHandling = NullValueHandling.Include;
			dict = JsonConvert.DeserializeObject<Dictionary<String, TValue>>(jsonString, setting);
		}

		public TValue this[string key] {
			get {
				if (dict.ContainsKey(key))
					return dict[key];
				else
					return default(TValue);
			}
			set {
				dict[key] = value;
			}
		}

		public TValue this[string key, TValue defaultValue] {
			get {
				if (dict.ContainsKey(key)) {
					if (dict[key] != null) return dict[key];
					else return defaultValue;
				} else {
					return defaultValue;
				}
			}
		}

		public ICollection<string> Keys => dict.Keys;

		public ICollection<TValue> Values => dict.Values;

		public int Count => dict.Count;

		public bool IsReadOnly => false;

		public void Add(string key, TValue value) {
			dict.Add(key, value);
		}

		public void Add(KeyValuePair<string, TValue> item) {
			dict.Add(item.Key, item.Value);
		}

		public void Clear() {
			dict.Clear();
		}

		public bool Contains(KeyValuePair<string, TValue> item) {
			return dict.Contains(item);
		}

		public bool ContainsKey(string key) {
			return dict.ContainsKey(key);
		}

		public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex) {
			var dictArray = dict.ToArray();
			var dictArrayCount = dictArray.Count();
			for (int i = arrayIndex; i < dictArrayCount; i++) {
				array[i] = dictArray[i];
			}
		}

		public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator() {
			return dict.GetEnumerator();
		}

		public bool Remove(string key) {
			return dict.Remove(key);
		}

		public bool Remove(KeyValuePair<string, TValue> item) {
			return dict.Remove(item.Key);
		}

		public bool TryGetValue(string key, out TValue value) {
			return dict.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return dict.GetEnumerator();
		}

		public string ToJson() {
			var setting = new JsonSerializerSettings();
			setting.NullValueHandling = NullValueHandling.Include;
			return JsonConvert.SerializeObject(dict, setting);
		}

		public void Merge(Data<TValue> data) {
			foreach (var d in data) {
				this[d.Key] = d.Value;
			}
		}

		public Data<TValue> MergeTo(Data<TValue> data) {
			var newData = new Data<TValue>();
			newData.Merge(this);
			newData.Merge(data);
			return newData;
		}

		public override string ToString() {
			return ToJson();
		}

		public bool IsEmpty(string key)
		{
            if (!this.ContainsKey(key))
			{
				return true;
			}

            if ("".Equals(this[key]))
			{
				return true;
			}

			return false;
		}

	}

	public static class Data {
		public static Data<T> New<T>(params Tuple<String, T>[] args) {
			var result = new Data<T>();
			foreach (var t in args) {
				result.Add(t.Item1, t.Item2);
			}
			return result;
		}
	}

}