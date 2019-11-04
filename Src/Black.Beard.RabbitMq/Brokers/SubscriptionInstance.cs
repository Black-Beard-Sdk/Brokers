namespace Bb.Brokers
{
    public class SubscriptionInstance
    {

         public SubscriptionInstance(string name, IBrokerSubscription subscription)
        {
            this.Name = name;
            this.Subscription = subscription;
        }

        protected SubscriptionInstance(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public IBrokerSubscription Subscription { get; protected set; }

        public void PrepareStop()
        {
            Subscription.Close();
        }

    }

}
