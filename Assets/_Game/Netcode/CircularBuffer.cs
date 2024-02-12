namespace _Game.Netcode
{
    public class CircularBuffer<T>
    {
        private T[] _buffer;
        private readonly int _bufferSize;
        
        public CircularBuffer(int size)
        {
            _buffer = new T[size];
            _bufferSize = size;
        }
        
        public void Add(T item, int index) => _buffer[index % _bufferSize] = item;
        public T Get(int index) => _buffer[index % _bufferSize];
        public void Clear() => _buffer = new T[_bufferSize];
    }
}