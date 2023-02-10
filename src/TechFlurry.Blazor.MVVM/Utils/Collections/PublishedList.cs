using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using TechFlurry.Blazor.MVVM.ViewModels;

namespace TechFlurry.Blazor.MVVM.Utils.Collections;
public class PublishedList<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable where T : ViewModelBase
{
    private List<T> _items;
    private bool disposedValue;

    public PublishedList()
    {
        _items = new List<T>();
    }
    public PublishedList(List<T> items)
    {
        _items = items;
        BindEvents();
    }
    public PublishedList(int capacity)
    {
        _items = new List<T>(capacity);
    }
    public PublishedList(IEnumerable<T> collection)
    {
        _items = new List<T>(collection);
        BindEvents();
    }

    public T this[int index]
    {
        get => _items[index];
        set
        {
            value.PropertyChanged += PropertyHasChanged(index);
            _items[index] = value;
            RaisePublishEvents(NotifyCollectionChangedAction.Add, _items[index], index, "Index[]");
        }
    }
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = value as T;
    }

    public int Count => _items.Count;
    bool ICollection<T>.IsReadOnly => ((ICollection<T>)_items).IsReadOnly;

    bool IList.IsReadOnly => ((IList)_items).IsReadOnly;

    bool IList.IsFixedSize => ((IList)_items).IsFixedSize;
    bool ICollection.IsSynchronized => ((IList)_items).IsSynchronized;
    object ICollection.SyncRoot => ((IList)_items).SyncRoot;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Add(T item)
    {
        item.PropertyChanged += PropertyHasChanged(_items.Count - 1);
        _items.Add(item);
        RaisePublishEvents(NotifyCollectionChangedAction.Add, item, _items.IndexOf(item), nameof(Count), "Index[]");
    }
    public void Clear()
    {
        UnBindEvents();
        _items.Clear();
        RaiseResetPublishEvents(nameof(Count), "Index[]");
    }
    public bool Contains(T item) => _items.Contains(item);

    public void CopyTo(T[ ] array, int arrayIndex)
    {
        _items.CopyTo(array, arrayIndex);
        RaisePublishEvents(NotifyCollectionChangedAction.Add, array[0], 0, "Index[]");
    }
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    public int IndexOf(T item) => _items.IndexOf(item);
    public void Insert(int index, T item)
    {
        var previousItem = _items.ElementAtOrDefault(Index.FromStart(index));
        if (previousItem is not null)
        {
            previousItem.PropertyChanged -= PropertyHasChanged(index);
        }
        item.PropertyChanged += PropertyHasChanged(index);
        _items.Insert(index, item);
        RaisePublishEvents(NotifyCollectionChangedAction.Add, item, index, "Index[]");
    }
    public bool Remove(T item)
    {
        var index = _items.IndexOf(item);
        item.PropertyChanged += PropertyHasChanged(index);
        var result = _items.Remove(item);
        RaisePublishEvents(NotifyCollectionChangedAction.Remove, item, index, nameof(Count), "Index[]");
        return result;
    }

    public void RemoveAt(int index)
    {
        var currentItem = _items.ElementAtOrDefault(Index.FromStart(index));
        if (currentItem is not null)
        {
            currentItem.PropertyChanged -= PropertyHasChanged(index);
        }
        _items.RemoveAt(index);
        RaisePublishEvents(NotifyCollectionChangedAction.Remove, currentItem, index, nameof(Count), "Index[]");
    }
    int IList.Add(object? value)
    {
        Add(value as T);
        return _items.Count - 1;
    }

    bool IList.Contains(object? value) => Contains(value as T);
    void ICollection.CopyTo(Array array, int index)
    {
        var collection = _items as ICollection;
        collection.CopyTo(array, index);
        RaisePublishEvents(NotifyCollectionChangedAction.Add, array.GetValue(0), 0, "Index[]");
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        var enumerable = _items as IEnumerable;
        return enumerable.GetEnumerator();
    }

    int IList.IndexOf(object? value) => IndexOf(value as T);
    void IList.Insert(int index, object? value) => Insert(index, value as T);
    void IList.Remove(object? value) => Remove(value as T);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public static PublishedList<T> GeneratePublishedList<TCollection>(IEnumerable<TCollection> collection, Func<TCollection, T> map) => new(collection.Select(map));

    private void RaisePublishEvents(NotifyCollectionChangedAction action, object item, int index, params string[ ] propertyNames)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item, index));
        foreach (var propertyName in propertyNames)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RaiseResetPublishEvents(params string[ ] propertyNames)
    {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        foreach (var propertyName in propertyNames)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void RaisePropertyChangedEvent(object sender, string proppertyName, int index) => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs($"{nameof(T)}[{index}].{proppertyName}"));
    private PropertyChangedEventHandler PropertyHasChanged(int index) => (sender, e) => RaisePropertyChangedEvent(sender, e.PropertyName, index);

    private void BindEvents()
    {
        foreach (var item in _items.Select((x, i) => (x, i)))
            item.x.PropertyChanged += PropertyHasChanged(item.i);
    }

    private void UnBindEvents()
    {
        foreach (var item in _items.Select((x, i) => (x, i)))
            item.x.PropertyChanged -= PropertyHasChanged(item.i);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                UnBindEvents();
            }
            _items = null!;
            disposedValue = true;
        }
    }

    ~PublishedList()
    {
        Dispose(disposing: false);
    }
}
