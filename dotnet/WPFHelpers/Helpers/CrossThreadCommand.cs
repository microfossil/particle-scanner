namespace WPFHelpers.Helpers
{
    public class CrossThreadCommand<T,V>
    {
        private volatile object lobj = new object();

        public volatile bool IsMessage = false;
        public T Message { get; private set; }
        public V Response { get; set; }

        public AsyncAutoResetEvent WaitHandle = new AsyncAutoResetEvent(false);

        public void Send(T message)
        {
            lock (lobj)
            {
                Message = message;
                Response = default(V);
                IsMessage = true;
            }
        }

        public void Reply(V response)
        {
            lock (lobj)
            {
                IsMessage = false;
                Response = response;
                WaitHandle.Set();
            }
        }
    }
}
