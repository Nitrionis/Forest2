
public class SuperQueue<T>
{
	public int capacity, first, last, count;
	public T[] arr;

	public SuperQueue(int capacity)
	{
		this.capacity = capacity;
		arr = new T[capacity];
		first = 0;
		last = 0;
		count = 0;
	}

	public void Enqueue(T value)
	{
		count++;
		arr[last] = value;
		last = (last + 1) % capacity;
	}

	public T Dequeue()
	{
		count--;
		T v = arr[first];
		first = (first + 1) % capacity;
		return v;
	}

	public T Peek()
	{
		return arr[first];
	}
}
