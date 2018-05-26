using System.Collections.Generic;

namespace Basics.Data
{
  public class Cache<K, V>
  {
    private class Pair
    {
      public Pair(K key, V value)
      {
        Key = key;
        Value = value;
      }
      public K Key { get; set; }
      public V Value { get; set; }

      public override string ToString()
      {
        return string.Format("{0}:{1}", Key, Value);
      }
    }
    private readonly Dictionary<K, LinkedListNode<Pair>> _dict;
    private readonly LinkedList<Pair> _list;

    public Cache()
      : this(1024)
    {
    }

    public Cache(int capacity)
    {
      Capacity = capacity;
      _dict = new Dictionary<K, LinkedListNode<Pair>>(capacity);
      _list = new LinkedList<Pair>();
    }

    public int Capacity { get; set; }

    public bool TryGetValue(K key, out V value)
    {
      if (_dict.TryGetValue(key, out LinkedListNode<Pair> node))
      {
        value = node.Value.Value;

        if (node.Previous != null)
        {
          _list.Remove(node);
          _list.AddFirst(node);
        }
        return true;
      }

      value = default(V);
      return false;
    }

    public void Add(K key, V value)
    {
      Resize(Capacity);

      Pair pair = new Pair(key, value);
      LinkedListNode<Pair> node = _list.AddFirst(pair);
      _dict.Add(key, node);
    }

    private void Resize(int capacity)
    {
      while (_list.Count >= capacity)
      {
        K removeKey = _list.Last.Value.Key;
        _dict.Remove(removeKey);
        _list.RemoveLast();
      }
    }
  }
}
