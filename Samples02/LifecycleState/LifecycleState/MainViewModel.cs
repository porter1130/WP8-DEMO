using System;

namespace LifecycleState
{
    public class MainViewModel
    {
        public string Timestamp { get; set; }

        public void LoadData()
        {
            Timestamp = DateTime.Now.ToLongTimeString();
        }
    }
}
