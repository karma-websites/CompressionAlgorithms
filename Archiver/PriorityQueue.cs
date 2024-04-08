namespace SuperArchiver;

internal class PriorityQueue<T>
{
    private int size;

    public int Size => size;

    readonly SortedDictionary<int, Queue<T>> storage;

    public PriorityQueue()
    {
        storage = [];
        size = 0;
    }

    public void Enqueue(int priority, T item)
    {
        if (!storage.TryGetValue(priority, out Queue<T>? value))
        {
            value = new Queue<T>();
            storage.Add(priority, value);
        }

        value.Enqueue(item);
        size++;
    }

    public T Dequeue()
    {
        if (size == 0) throw new Exception("Queue is empty");
        size--;
        foreach (Queue<T> q in storage.Values)
        {
            if (q.Count > 0) return q.Dequeue();
        }

        throw new Exception("Queue error");
    }
}