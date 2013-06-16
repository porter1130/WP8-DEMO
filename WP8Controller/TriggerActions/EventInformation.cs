namespace WP8Controller.TriggerActions
{
    public class EventInformation<TEventArgsType>
    {
        public object Sender { get; set; }
        public TEventArgsType EventArgs { get; set; }
        public object CommandArgument { get; set; }
    }
}
