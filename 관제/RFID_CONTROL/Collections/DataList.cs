using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;

namespace RFID_CONTROLLER.Collections
{
    public class DataList<TValue> : IList<TValue> {
		private List<TValue> list = new List<TValue>();

		public DataList() { }

		public DataList(IEnumerable<TValue> pList) {
			foreach (var item in pList) {
				list.Add(item);
			}
		}

		public DataList(params TValue[] args) {
			foreach (var item in args) {
				list.Add(item);
			}
		}

		public DataList(string jsonString) {
			var setting = new JsonSerializerSettings();
			setting.NullValueHandling = NullValueHandling.Include;
			var l = JsonConvert.DeserializeObject<List<TValue>>(jsonString, setting);
			foreach (var item in l) {
				list.Add(item);
			}
		}

		public TValue this[int index] { get => (TValue)list[index]; set => list[index] = value; }

		public int Count => list.Count;

		public bool IsReadOnly => false;

		public void Add(TValue item) {
			list.Add(item);
		}

		public void Clear() {
			list.Clear();
		}

		public bool Contains(TValue item) {
			return list.Contains(item);
		}

		public void CopyTo(TValue[] array, int arrayIndex) {
			list.CopyTo(array, arrayIndex);
		}

		public IEnumerator<TValue> GetEnumerator() {
			return list.GetEnumerator();
		}

		public int IndexOf(TValue item) {
			return list.IndexOf(item);
		}

		public void Insert(int index, TValue item) {
			list.Insert(index, item);
		}

		public bool Remove(TValue item) {
			list.Remove(item);
			return true;
		}

		public void RemoveAt(int index) {
			list.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return list.GetEnumerator();
		}

		public string ToJson() {
			var setting = new JsonSerializerSettings();
			setting.NullValueHandling = NullValueHandling.Include;
			return JsonConvert.SerializeObject(list, setting);
		}

		public override string ToString() {
			return ToJson();
		}

	}

	public static class DataList {
		public static DataList<T> ToDataList<T>(this IEnumerable<T> list) {
			return new DataList<T>(list);
		}

		public static Data<Data<string>> ToData(this DataList<Data<string>> dataList, string keyFieldName) {
			var r = new Data<Data<string>>();
			foreach (var d in dataList) {
				r[d[keyFieldName]] = d;
			}
			return r;
		}

		public static DataList<T> New<T>(params T[] args) {
			return new DataList<T>(args);
		}
	}
}
