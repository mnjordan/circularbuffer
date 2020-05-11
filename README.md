# circularbuffer
A generic circular buffer implementation in C#

## interface

- `void Add(T obj);`
- `T Get(int index);`
- `IEnumerable<T> GetAll();`
- `void Resize(int newSize);`
