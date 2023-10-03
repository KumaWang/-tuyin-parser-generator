using librule.targets.code;

namespace librule.generater
{
    class GrapBoxCallback
    {
        public GrapBoxCallback(Func<object, CodeTargetVisitor, string> callback, object sender)
        {
            Callback = callback;
            Sender = sender;
        }

        public Func<object, CodeTargetVisitor, string> Callback { get; }

        public object Sender { get; }

        public string Invoke(CodeTargetVisitor visitor) => Callback?.Invoke(Sender, visitor) ?? string.Empty;
    }
}
