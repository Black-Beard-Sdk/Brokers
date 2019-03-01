using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bb.Brokers
{

    public class SubscriptionInstances : IDisposable
    {

        public SubscriptionInstances(RabbitFactoryBrokers borkers)
        {
            _brokers = borkers;
            _items = new List<SubscriptionInstance>();
        }

        public SubscriptionInstance AddSubscription(SubscriptionInstance subscription)
        {
            _items.Add(subscription);
            return subscription;
        }

        public SubscriptionInstance AddSubscription(string key, string subscriberName, Func<IBrokerContext, Task> callback)
        {
            IBrokerSubscription subscription = _brokers.CreateSubscription(subscriberName, callback);
            var sub = new SubscriptionInstance(key, subscription);
            _items.Add(sub);
            return sub;
        }

        #region IDisposable Support
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var item in _items)
                        item.Subscription.Dispose();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        // ~Subscriptions() {
        //   // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
        //   Dispose(false);
        // }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            // GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        private readonly RabbitFactoryBrokers _brokers;
        private readonly List<SubscriptionInstance> _items;
        private bool disposedValue = false; // Pour détecter les appels redondants

    }

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

    }

}
